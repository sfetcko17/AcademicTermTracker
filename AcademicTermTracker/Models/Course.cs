
using SQLite;

namespace AcademicTermTracker.Models
{
    public class Course
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int TermId { get; set; }

        [NotNull]
        public string Title { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [NotNull]
        public string Status { get; set; } = string.Empty;

        [NotNull]
        public string InstructorName { get; set; } = string.Empty;

        [NotNull]
        public string InstructorPhone { get; set; } = string.Empty;

        [NotNull]
        public string InstructorEmail { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public bool StartNotificationEnabled { get; set; }

        public bool EndNotificationEnabled { get; set; }
    }
}
