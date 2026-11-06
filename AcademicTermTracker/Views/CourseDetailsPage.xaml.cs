using AcademicTermTracker.Models;
using AcademicTermTracker.Services;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace AcademicTermTracker.Views
{
    public partial class CourseDetailsPage : ContentPage, IQueryAttributable
    {
        private readonly DatabaseService _databaseService;

        private int _courseId;
        private Course? _course;

        public CourseDetailsPage(DatabaseService databaseService)
        {
            InitializeComponent();

            _databaseService = databaseService;
        }

        public void ApplyQueryAttributes(
            IDictionary<string, object> query)
        {
            if (query.TryGetValue("courseId", out object? value) &&
                int.TryParse(value?.ToString(), out int courseId) &&
                courseId > 0)
            {
                _courseId = courseId;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_courseId <= 0)
            {
                await DisplayAlert(
                    "Error",
                    "No course was selected.",
                    "OK");

                return;
            }

            try
            {
                _course = await _databaseService.GetCourseAsync(_courseId);

                if (_course == null)
                {
                    await DisplayAlert(
                        "Error",
                        "The selected course could not be found.",
                        "OK");

                    return;
                }

                CourseTitleLabel.Text = _course.Title;
                StatusLabel.Text = _course.Status;

                StartDateLabel.Text =
                    _course.StartDate.ToString("MMMM dd, yyyy");

                EndDateLabel.Text =
                    _course.EndDate.ToString("MMMM dd, yyyy");

                InstructorNameLabel.Text = _course.InstructorName;
                InstructorPhoneLabel.Text = _course.InstructorPhone;
                InstructorEmailLabel.Text = _course.InstructorEmail;

                NotesLabel.Text = string.IsNullOrWhiteSpace(_course.Notes)
                    ? "No notes available."
                    : _course.Notes;

                ShareNotesButton.IsEnabled =
                    !string.IsNullOrWhiteSpace(_course.Notes);
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Error",
                    $"Unable to load course details: {ex.Message}",
                    "OK");
            }
        }

        private async void OnShareNotesClicked(
            object sender, EventArgs e)
        {
            if (_course == null ||
                string.IsNullOrWhiteSpace(_course.Notes))
            {
                await DisplayAlert(
                    "No Notes",
                    "There are no course notes to share.",
                    "OK");

                return;
            }

            try
            {
                await Share.Default.RequestAsync(
                    new ShareTextRequest
                    {
                        Title = "Share Course Notes",
                        Subject = _course.Title,
                        Text = _course.Notes
                    });
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Sharing Error",
                    $"Unable to share notes: {ex.Message}",
                    "OK");
            }
        }

        private async void OnEditCourseClicked(
            object sender, EventArgs e)
        {
            if (_course == null)
            {
                return;
            }

            try
            {
                await Shell.Current.GoToAsync(
                    $"{nameof(CourseEditPage)}?courseId={_course.Id}");
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Navigation Error",
                    $"Unable to open course editor: {ex.Message}",
                    "OK");
            }
        }
        
        private async void OnViewAssessmentsClicked(
            object sender, EventArgs e)
        {
            if (_course == null)
            {
                return;
            }

            try
            {
                await Shell.Current.GoToAsync(
                    $"{nameof(AssessmentsPage)}?courseId={_course.Id}");
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Navigation Error",
                    $"Unable to open assessments: {ex.Message}",
                    "OK");
            }
        }
    }
}