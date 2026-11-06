using AcademicTermTracker.Views;

namespace AcademicTermTracker
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(
                nameof(TermEditPage),
                typeof(TermEditPage));
            Routing.RegisterRoute(
                nameof(CoursesPage),
                typeof(CoursesPage));
            Routing.RegisterRoute(
                nameof(CourseEditPage),
                typeof(CourseEditPage));
            Routing.RegisterRoute(
                nameof(CourseDetailsPage),
                typeof(CourseDetailsPage));
            Routing.RegisterRoute(
                nameof(AssessmentsPage),
                typeof(AssessmentsPage));
            Routing.RegisterRoute(
                nameof(AssessmentEditPage),
                typeof(AssessmentEditPage));
        }
    }
}
