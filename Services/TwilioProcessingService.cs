using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmithSmsStatusFetcher.Settings;
using System.Data.Common;

namespace SmithSmsStatusFetcher.Services
{

    public class TwilioProcessingService
    {
        private readonly TwilioSecrets _twilioSecrets;
        private readonly DbSettings _dbSettings;
        private readonly ILogger<TwilioProcessingService> _logger;
        public DbProviderFactory Factory => MySql.Data.MySqlClient.MySqlClientFactory.Instance;

        public TwilioProcessingService(IOptions<TwilioSecrets> twilioSecrets, IOptions<DbSettings> dbSettings,
            ILogger<TwilioProcessingService> logger)
        {
            _twilioSecrets = twilioSecrets.Value;
            _dbSettings = dbSettings.Value;
            _logger = logger;

        }

        private DbConnection GetMySqlConnection(bool open = true,
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




    }
}
