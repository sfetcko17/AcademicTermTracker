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
        }
    }
}
