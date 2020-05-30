namespace SmithSmsStatusFetcher.Models
{
    public partial class AssignmentMessagesStatus
    {
        public string MessageSid { get; set; }
        public long? ContactId { get; set; }
        public string Status { get; set; }
        public int? ErrorCode { get; set; }
    }
}
