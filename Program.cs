using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Configuration;
using SmithSmsStatusFetcher.Models;
using SmithSmsStatusFetcher.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
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
            var sql = $"SELECT * FROM assignment_message_status WHERE STATUS IS NULL ORDER BY message_sid LIMIT {batchSize}";

            List<AssignmentMessagesStatus> statuses;
            using (DbConnection conn = GetMySqlConnection(true, false, false))
            {
                statuses = conn.Query<AssignmentMessagesStatus>(sql).ToList();
            }

            _ = Parallel.ForEach(statuses,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                (status, newStatus) =>
                {
                    TwilioClient.Init(_twilioSecrets.AccountSid, _twilioSecrets.AuthToken);

                    MessageResource message = MessageResource.Fetch(
                        pathSid: status.MessageSid
                    );


                    status.Status = message.Status.ToString();
                    status.ErrorCode = message.ErrorCode;

                    using var connection = GetMySqlConnection(true, false, false);
                    connection.Update(status);



                });
        }



        internal static void LoadTwilioSecrets(IConfiguration config)
        {

            _twilioSecrets.AuthToken = GetSetting<string>(config, "TwilioSecrets:AuthToken");
            _twilioSecrets.AccountSid = GetSetting<string>(config, "TwilioSecrets:AccountSid");
        }
        internal static void LoadDbSettings(IConfiguration config)
        {
            _dbSettings.Username = GetSetting<string>(config, "Database:Username");
            _dbSettings.Password = GetSetting<string>(config, "Database:Password");
            _dbSettings.Server = GetSetting<string>(config, "Database:Server");
            _dbSettings.Database = GetSetting<string>(config, "Database:Database");
        }



        private static T GetSetting<T>(IConfiguration config, string fullKey)
        {
            var setting = config.GetSection(fullKey).Value;
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)(converter.ConvertFromInvariantString(setting));
        }
    }
}
