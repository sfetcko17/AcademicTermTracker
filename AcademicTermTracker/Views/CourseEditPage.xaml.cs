using AcademicTermTracker.Models;
using AcademicTermTracker.Services;

namespace AcademicTermTracker.Views
{
    public partial class CourseEditPage : ContentPage, IQueryAttributable
    {
        private readonly DatabaseService _databaseService;
        private readonly INotificationService _notificationService;

        private int _termId;
        private int _courseId;
        private Course? _course;

        public CourseEditPage(DatabaseService databaseService, INotificationService notificationService)
        {
            InitializeComponent();

            _databaseService = databaseService;
            _notificationService = notificationService;

            StartDatePicker.Date = DateTime.Today;
            EndDatePicker.Date = DateTime.Today.AddMonths(3);

            StatusPicker.SelectedIndex = 0;
            DeleteButton.IsVisible = false;
        }

        public void ApplyQueryAttributes(
            IDictionary<string, object> query)
        {
            if (query.TryGetValue("termId", out object? termValue) &&
                int.TryParse(termValue?.ToString(), out int termId))
            {
                _termId = termId;
            }

            if (query.TryGetValue("courseId", out object? courseValue) &&
                int.TryParse(courseValue?.ToString(), out int courseId))
            {
                _courseId = courseId;

                PageHeading.Text = "Edit Course";
                Title = "Edit Course";
                DeleteButton.IsVisible = true;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_courseId <= 0)
            {
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

                _termId = _course.TermId;

                TitleEntry.Text = _course.Title;
                StartDatePicker.Date = _course.StartDate;
                EndDatePicker.Date = _course.EndDate;

                StatusPicker.SelectedItem = _course.Status;

                InstructorNameEntry.Text = _course.InstructorName;
                InstructorPhoneEntry.Text = _course.InstructorPhone;
                InstructorEmailEntry.Text = _course.InstructorEmail;

                NotesEditor.Text = _course.Notes;

                StartNotificationSwitch.IsToggled =
                    _course.StartNotificationEnabled;

                EndNotificationSwitch.IsToggled =
                    _course.EndNotificationEnabled;
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Error",
                    $"Unable to load course: {ex.Message}",
                    "OK");
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                if (_termId <= 0)
                {
                    await DisplayAlert(
                        "Error",
                        "No academic term was selected.",
                        "OK");

                    return;
                }

                Course course = _course ?? new Course();

                course.TermId = _termId;
                course.Title = TitleEntry.Text ?? string.Empty;
                course.StartDate = StartDatePicker.Date;
                course.EndDate = EndDatePicker.Date;
                course.Status =
                    StatusPicker.SelectedItem?.ToString() ?? string.Empty;

                course.InstructorName =
                    InstructorNameEntry.Text ?? string.Empty;

                course.InstructorPhone =
                    InstructorPhoneEntry.Text ?? string.Empty;

                course.InstructorEmail =
                    InstructorEmailEntry.Text ?? string.Empty;

                course.Notes = NotesEditor.Text ?? string.Empty;

                course.StartNotificationEnabled =
                    StartNotificationSwitch.IsToggled;

                course.EndNotificationEnabled =
                    EndNotificationSwitch.IsToggled;

                await _databaseService.SaveCourseAsync(course);
                
                // Update Android reminders using the saved course.
                await _notificationService.ScheduleCourseNotificationsAsync(course);

                await Shell.Current.GoToAsync("..");
            }
            catch (ArgumentException ex)
            {
                await DisplayAlert(
                    "Validation Error",
                    ex.Message,
                    "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Error",
                    $"Unable to save course: {ex.Message}",
                    "OK");
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (_course == null)
            {
                return;
            }

            bool confirmed = await DisplayAlert(
                "Delete Course",
                "Are you sure you want to delete this course? " +
                "Its assessments will also be deleted.",
                "Delete",
                "Cancel");

            if (!confirmed)
            {
                return;
            }

            try
            {
                if (_course == null)
                {
                    return;
                }

                // Retrieve assessments before deleting the course.
                // Database deletion also removes associated assessments.
                List<Assessment> assessments =
                    await _databaseService.GetAssessmentsAsync(_course.Id);

                // Cancel notifications for each associated assessment.
                foreach (Assessment assessment in assessments)
                {
                    await _notificationService.CancelAssessmentNotificationsAsync(
                        assessment.Id);
                }

                // Cancel notifications for the course itself.
                await _notificationService.CancelCourseNotificationsAsync(
                    _course.Id);

                await _databaseService.DeleteCourseAsync(_course);

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Error",
                    $"Unable to delete course: {ex.Message}",
                    "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}