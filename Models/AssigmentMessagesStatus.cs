using System;
using System.Collections.Generic;

namespace SmithSmsStatusFetcher.Models
{
    public partial class AssigmentMessagesStatus
    {
        public string MessageSid { get; set; }
        public long? ContactId { get; set; }
        public string Status { get; set; }
    }
}
