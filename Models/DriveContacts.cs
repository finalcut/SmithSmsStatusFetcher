using System;
using System.Collections.Generic;

namespace SmithSmsStatusFetcher.Models
{
    public partial class DriveContacts
    {
        public ulong Id { get; set; }
        public uint DriveId { get; set; }
        public string FirstName { get; set; }
        public string PhoneNumber { get; set; }
        public string ExternalId { get; set; }
        public string Friends { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
