using System;

namespace EFHooks.Tests.Hooks
{
    public interface ITimeStamped
    {
        DateTime CreatedAt { get; set; }
        DateTime? ModifiedAt { get; set; }
    }

    public class TimestampedSoftDeletedEntity : ITimeStamped, ISoftDeleted
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}