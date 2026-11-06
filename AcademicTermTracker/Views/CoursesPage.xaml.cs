using AcademicTermTracker.Models;
using AcademicTermTracker.Services;

namespace AcademicTermTracker.Views
{
    public partial class CoursesPage : ContentPage, IQueryAttributable
    {
        private readonly DatabaseService _databaseService;
        private int _termId;

        public CoursesPage(DatabaseService databaseService)
        {
            InitializeComponent();

            _databaseService = databaseService;
        }

        public void ApplyQueryAttributes(
            IDictionary<string, object> query)
        {
            if (query.TryGetValue("termId", out object? value) &&
                int.TryParse(value?.ToString(), out int termId) &&
                termId > 0)
            {
                _termId = termId;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_termId <= 0)
            {
                await DisplayAlert(
                    "Error",
                    "No academic term was selected.",
                    "OK");

                return;
            }

            try
            {
                AcademicTerm? term =
                    await _databaseService.GetTermAsync(_termId);

                if (term == null)
                {
                    await DisplayAlert(
                        "Error",
                        "The selected academic term no longer exists.",
                        "OK");

                    return;
                }

                TermTitleLabel.Text = term.Title;

                List<Course> courses =
                    await _databaseService.GetCoursesAsync(_termId);

                CoursesCollectionView.ItemsSource = courses;

                CourseCountLabel.Text =
                    $"{courses.Count} of 6 courses";

                AddCourseButton.IsEnabled = courses.Count < 6;
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Error",
                    $"Unable to load courses: {ex.Message}",
                    "OK");
            }
        }
      
        private async void OnAddCourseClicked(
            object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync(
                    $"{nameof(CourseEditPage)}?termId={_termId}");
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Navigation Error",
                    $"Unable to open course editor: {ex.Message}",
                    "OK");
            }
        }
        
        private async void OnEditCourseClicked(
            object sender, EventArgs e)
        {
            if (sender is not Button button ||
                button.CommandParameter is not Course course)
            {
                return;
            }

            try
            {
                await Shell.Current.GoToAsync(
                    $"{nameof(CourseEditPage)}?courseId={course.Id}");
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Navigation Error",
                    $"Unable to edit course: {ex.Message}",
                    "OK");
            }
        }
        
        private async void OnCourseSelected(
            object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault()
                is not Course course)
            {
                return;
            }

            CoursesCollectionView.SelectedItem = null;

            try
            {
                await Shell.Current.GoToAsync(
                    $"{nameof(CourseDetailsPage)}?courseId={course.Id}");
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Navigation Error",
                    $"Unable to open course details: {ex.Message}",
                    "OK");
            }
        }
    }
}