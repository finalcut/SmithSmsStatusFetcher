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
        private static TwilioSecrets _twilioSecrets;
        private static DbSettings _dbSettings;

        public static void Main(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddUserSecrets<Program>()
               .Build();

            LoadDbSettings(configuration);
            LoadTwilioSecrets(configuration);

            ReadBatchoFMesssages(50);


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



        public static void ReadBatchoFMesssages(int batchSize)
        {

            // get all the ids where status is null.

            var s = "SELECT * FROM assignment_messages_status WHERE STATUS IS NULL ORDER BY message_sid";
            List<string> ids;
            using (DbConnection conn = GetMySqlConnection(true, false, false))
            {
                ids = conn.Query<string>(s) as List<string>;
            }


            while (ids.Count > 0)
            {


                List<string> batchIds = new List<string>();
                for (int x = 1; x <= batchSize; x++)
                {
                    if (ids.Count > 0)
                    {
                        batchIds.Add(ids[0]);
                        ids.RemoveAt(0);
                    }
                }

                var sql = $"SELECT * FROM assignment_messages_status WHERE message_sid IN @ids";

                List<AssignmentMessagesStatus> statuses;
                using (DbConnection conn2 = GetMySqlConnection(true, false, false))
                {
                    statuses = conn2.Query<AssignmentMessagesStatus>(sql, new { ids = batchIds }) as List<AssignmentMessagesStatus>;
                }

                _ = Parallel.ForEach(statuses,
                    new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                    async (status, newStatus) =>
                    {
                        TwilioClient.Init(_twilioSecrets.AccountSid, _twilioSecrets.AuthToken);

                        MessageResource message = MessageResource.Fetch(
                            pathSid: status.Message_Sid
                        );


                        status.Status = message.Status.ToString();
                        status.Error_Code = message.ErrorCode;

                        using var connection = GetMySqlConnection(true, false, false);
                        var sql = "UPDATE assignment_messages_status SET status = @status, error_code = @errorCode WHERE message_sid = @id";
                        await connection.ExecuteAsync(sql, new { status = status.Status, ErrorCode = status.Error_Code, id = status.Message_Sid });



                    });

                System.Threading.Thread.Sleep(1000);
            }
        }



        internal static void LoadTwilioSecrets(IConfiguration config)
        {
            _twilioSecrets = new TwilioSecrets
            {
                AuthToken = GetSetting<string>(config, "TwilioSecrets:AuthToken"),
                AccountSid = GetSetting<string>(config, "TwilioSecrets:AccountSid")
            };
        }
        internal static void LoadDbSettings(IConfiguration config)
        {
            _dbSettings = new DbSettings
            {
                Username = GetSetting<string>(config, "Database:Username"),
                Password = GetSetting<string>(config, "Database:Password"),
                Server = GetSetting<string>(config, "Database:Server"),
                Database = GetSetting<string>(config, "Database:Database")
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
