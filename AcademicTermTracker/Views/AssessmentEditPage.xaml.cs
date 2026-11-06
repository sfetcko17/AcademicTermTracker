using AcademicTermTracker.Models;
using AcademicTermTracker.Services;

namespace AcademicTermTracker.Views
{
    public partial class AssessmentEditPage : ContentPage, IQueryAttributable
    {
        private readonly DatabaseService _databaseService;

        private int _courseId;
        private int _assessmentId;
        private Assessment? _assessment;

        public AssessmentEditPage(DatabaseService databaseService)
        {
            InitializeComponent();

            _databaseService = databaseService;

            StartDatePicker.Date = DateTime.Today;
            EndDatePicker.Date = DateTime.Today.AddDays(14);

            TypePicker.SelectedIndex = 0;
            DeleteButton.IsVisible = false;
        }

        public void ApplyQueryAttributes(
            IDictionary<string, object> query)
        {
            if (query.TryGetValue("courseId", out object? courseValue) &&
                int.TryParse(courseValue?.ToString(), out int courseId) &&
                courseId > 0)
            {
                _courseId = courseId;
            }

            if (query.TryGetValue("assessmentId", out object? assessmentValue) &&
                int.TryParse(assessmentValue?.ToString(), out int assessmentId) &&
                assessmentId > 0)
            {
                _assessmentId = assessmentId;

                PageHeading.Text = "Edit Assessment";
                Title = "Edit Assessment";
                DeleteButton.IsVisible = true;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_assessmentId <= 0)
            {
                return;
            }

            try
            {
                _assessment = await _databaseService.GetAssessmentAsync(
                    _assessmentId);

                if (_assessment == null)
                {
                    await DisplayAlert(
                        "Error",
                        "The selected assessment could not be found.",
                        "OK");

                    return;
                }

                _courseId = _assessment.CourseId;

                TitleEntry.Text = _assessment.Title;
                TypePicker.SelectedItem = _assessment.Type;

                StartDatePicker.Date = _assessment.StartDate;
                EndDatePicker.Date = _assessment.EndDate;

                StartNotificationSwitch.IsToggled =
                    _assessment.StartNotificationEnabled;

                EndNotificationSwitch.IsToggled =
                    _assessment.EndNotificationEnabled;
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Error",
                    $"Unable to load assessment: {ex.Message}",
                    "OK");
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                if (_courseId <= 0)
                {
                    await DisplayAlert(
                        "Error",
                        "No course was selected.",
                        "OK");

                    return;
                }

                Assessment assessment = _assessment ?? new Assessment();

                assessment.CourseId = _courseId;
                assessment.Title = TitleEntry.Text ?? string.Empty;
                assessment.Type =
                    TypePicker.SelectedItem?.ToString() ?? string.Empty;

                assessment.StartDate = StartDatePicker.Date;
                assessment.EndDate = EndDatePicker.Date;

                assessment.StartNotificationEnabled =
                    StartNotificationSwitch.IsToggled;

                assessment.EndNotificationEnabled =
                    EndNotificationSwitch.IsToggled;

                await _databaseService.SaveAssessmentAsync(assessment);

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
                    $"Unable to save assessment: {ex.Message}",
                    "OK");
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (_assessment == null)
            {
                return;
            }

            bool confirmed = await DisplayAlert(
                "Delete Assessment",
                "Are you sure you want to delete this assessment?",
                "Delete",
                "Cancel");

            if (!confirmed)
            {
                return;
            }

            try
            {
                await _databaseService.DeleteAssessmentAsync(_assessment);

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Error",
                    $"Unable to delete assessment: {ex.Message}",
                    "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}