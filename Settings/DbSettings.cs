namespace SmithSmsStatusFetcher.Settings
{
    public class DbSettings
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }

        public string ConnectionString
        {
            get
            {
                return $"Server={Server}; Database={Database};User={Username};Password={Password}";

            }
        }
    }
}
