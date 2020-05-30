using System;
using System.Collections.Generic;

namespace SmithSmsStatusFetcher.Models
{
    public partial class AssignmentMessagesStatus
    {
        public string MessageSid { get; set; }
        public long? ContactId { get; set; }
        public string Status { get; set; }
        public string ErrorStatus { get; set; }
    }
}
