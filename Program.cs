using Dapper;
using Microsoft.Extensions.Configuration;
using SmithSmsStatusFetcher.Models;
using SmithSmsStatusFetcher.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace SmithSmsStatusFetcher
{
    public class Program
    {
        private static IConfigurationRoot _configuration;
        private static DbSettings _dbSettings;
        private static TwilioSecrets _twilioSecrets;
        private static BatchSettings _batchSettings;

        public static void Main(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddUserSecrets<Program>();

            if (args != null)
            {
                builder.AddCommandLine(args);
            }

            _configuration = builder.Build();

            _dbSettings = LoadDbSettings();
            _twilioSecrets = LoadTwilioSecrets();
            _batchSettings = LoadBatchSettings();

            ReadBatchoFMesssages();
        }

        public static DbProviderFactory Factory => MySql.Data.MySqlClient.MySqlClientFactory.Instance;


        private static DbConnection GetMySqlConnection(bool open = true,
        bool convertZeroDatetime = false, bool allowZeroDatetime = false)
        {
            string cs = _dbSettings.ConnectionString;
            var csb = Factory.CreateConnectionStringBuilder();
            csb.ConnectionString = cs;
            ((dynamic)csb).AllowZeroDateTime = allowZeroDatetime;
            ((dynamic)csb).ConvertZeroDateTime = convertZeroDatetime;
            var conn = Factory.CreateConnection();
            conn.ConnectionString = csb.ConnectionString;
            if (open) conn.Open();
            return conn;
        }



        public static void ReadBatchoFMesssages()
        {
            // get all the ids where status is null.

            var s = "SELECT * FROM assignment_messages_status WHERE STATUS IS NULL ORDER BY message_sid";
            List<string> ids;
            using (DbConnection conn = GetMySqlConnection())
            {
                ids = conn.Query<string>(s) as List<string>;
            }


            while (ids.Count > 0)
            {


                List<string> batchIds = new List<string>();
                for (int x = 1; x <= _batchSettings.BatchSize; x++)
                {
                    if (ids.Count > 0)
                    {
                        batchIds.Add(ids[0]);
                        ids.RemoveAt(0);
                    }
                }

                var sql = $"SELECT * FROM assignment_messages_status WHERE message_sid IN @ids";

                List<AssignmentMessagesStatus> statuses;
                using (DbConnection conn2 = GetMySqlConnection())
                {
                    statuses = conn2.Query<AssignmentMessagesStatus>(sql, new { ids = batchIds }) as List<AssignmentMessagesStatus>;
                }

                _ = Parallel.ForEach(statuses,
                    new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                    async (status) =>
                    {
                        await ProcessMessage(status);
                    });

                System.Threading.Thread.Sleep(_batchSettings.ApiCallPauseInMilliseconds);
            }
        }


        private static async Task ProcessMessage(AssignmentMessagesStatus status)
        {
            TwilioClient.Init(_twilioSecrets.AccountSid, _twilioSecrets.AuthToken);

            MessageResource message = MessageResource.Fetch(
                pathSid: status.Message_Sid
            );


            status.Status = message.Status.ToString();
            status.Error_Code = message.ErrorCode;

            using var connection = GetMySqlConnection();
            var sql = "UPDATE assignment_messages_status SET status = @status, error_code = @errorCode WHERE message_sid = @id";
            await connection.ExecuteAsync(sql, new { status = status.Status, ErrorCode = status.Error_Code, id = status.Message_Sid });


        }


        private static TwilioSecrets LoadTwilioSecrets()
        {
            return new TwilioSecrets
            {
                AuthToken = GetSetting<string>(_configuration, "TwilioSecrets:AuthToken"),
                AccountSid = GetSetting<string>(_configuration, "TwilioSecrets:AccountSid")
            };
        }
        private static DbSettings LoadDbSettings()
        {
            return new DbSettings
            {
                Username = GetSetting<string>(_configuration, "Database:Username"),
                Password = GetSetting<string>(_configuration, "Database:Password"),
                Server = GetSetting<string>(_configuration, "Database:Server"),
                Database = GetSetting<string>(_configuration, "Database:Database")
            };
        }

        private static BatchSettings LoadBatchSettings()
        {
            return new BatchSettings
            {
                BatchSize = GetSetting<int>(_configuration, "Batch:Size"),
                ApiCallPauseInMilliseconds = GetSetting<int>(_configuration, "Batch:ApiCallPauseInMilliseconds")
            };

        }



        private static T GetSetting<T>(IConfiguration config, string fullKey)
        {
            var setting = config.GetSection(fullKey).Value;
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)(converter.ConvertFromInvariantString(setting));
        }
    }
}
