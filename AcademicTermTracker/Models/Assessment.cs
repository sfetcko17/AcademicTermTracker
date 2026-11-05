
using SQLite;

namespace AcademicTermTracker.Models
{
    public class Assessment
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed("IX_Assessment_Course_Type", 1, Unique = true)]
        public int CourseId { get; set; }

        [NotNull]
        public string Title { get; set; } = string.Empty;

        [NotNull]
        [Indexed("IX_Assessment_Course_Type", 2, Unique = true)]
        public string Type { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool StartNotificationEnabled { get; set; }

        public bool EndNotificationEnabled { get; set; }
    }
}
