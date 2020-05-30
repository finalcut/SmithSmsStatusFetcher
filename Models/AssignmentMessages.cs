using System;
using System.Collections.Generic;

namespace SmithSmsStatusFetcher.Models
{
    public partial class AssignmentMessages
    {
        public ulong Id { get; set; }
        public bool Incoming { get; set; }
        public string Body { get; set; }
        public string ParticipantPhone { get; set; }
        public string ServicePhone { get; set; }
        public string MessageSid { get; set; }
        public uint AssignmentId { get; set; }
        public uint? UserId { get; set; }
        public uint? ReplyId { get; set; }
        public uint ContactId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime Microtime { get; set; }
    }
}
