namespace SmithSmsStatusFetcher.Settings
{
    public class TwilioSecrets : ISettings
    {
        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
    }
}
