using Azure.Data.Tables;
using Azure;

namespace HomeYatra.Dto
{
    public class Account : ITableEntity
    {

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; } = default!;
        public ETag ETag { get; set; } = default!;
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int GenderId { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }
        public string ChannelIds { get; set; }
        public int PreferenceChannelIds { get; set; }
        public int PlanId { get; set; }
        public DateTimeOffset? CreatedDT { get; set; }
        public DateTimeOffset? UpdatedDT { get; set; }
        public bool IsActive { get; set; }
        public bool IsPreferenceSet { get; set; }
        public int MessageCount { get; set; }
        public string OTP { get; set; }

    }
}
