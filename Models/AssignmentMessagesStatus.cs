using Dapper.Contrib.Extensions;

namespace SmithSmsStatusFetcher.Models
{
    public partial class AssignmentMessagesStatus
    {

        [Key]
        public string Message_Sid { get; set; }
        public long? Contact_Id { get; set; }
        public string Status { get; set; }
        public int? Error_Code { get; set; }
    }
}
