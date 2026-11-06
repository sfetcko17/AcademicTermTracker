using AcademicTermTracker.Models;
using AcademicTermTracker.Services;

namespace AcademicTermTracker.Views
{
    public partial class AssessmentsPage : ContentPage, IQueryAttributable
    {
        private readonly DatabaseService _databaseService;
        private int _courseId;

        public AssessmentsPage(DatabaseService databaseService)
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
                Course? course =
                    await _databaseService.GetCourseAsync(_courseId);

                if (course == null)
                {
                    await DisplayAlert(
                        "Error",
                        "The selected course no longer exists.",
                        "OK");

                    return;
                }

                CourseTitleLabel.Text = course.Title;

                List<Assessment> assessments =
                    await _databaseService.GetAssessmentsAsync(_courseId);

                AssessmentsCollectionView.ItemsSource = assessments;

                AssessmentCountLabel.Text =
                    $"{assessments.Count} of 2 assessments";

                bool hasObjective = assessments.Any(
                    assessment => assessment.Type == "Objective");

                bool hasPerformance = assessments.Any(
                    assessment => assessment.Type == "Performance");

                AddAssessmentButton.IsEnabled =
                    !hasObjective || !hasPerformance;
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Error",
                    $"Unable to load assessments: {ex.Message}",
                    "OK");
            }
        }

        private async void OnAddAssessmentClicked(
            object sender, EventArgs e)
        {
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
                await Shell.Current.GoToAsync(
                    $"{nameof(AssessmentEditPage)}?courseId={_courseId}");
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Navigation Error",
                    $"Unable to open assessment editor: {ex.Message}",
                    "OK");
            }
        }

        private async void OnEditAssessmentClicked(
            object sender, EventArgs e)
        {
            if (sender is not Button button ||
                button.CommandParameter is not Assessment assessment)
            {
                return;
            }

            try
            {
                await Shell.Current.GoToAsync(
                    $"{nameof(AssessmentEditPage)}?assessmentId={assessment.Id}");
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Navigation Error",
                    $"Unable to edit assessment: {ex.Message}",
                    "OK");
            }
        }

        private async void OnAssessmentSelected(
            object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault()
                is not Assessment assessment)
            {
                return;
            }

            AssessmentsCollectionView.SelectedItem = null;

            try
            {
                await Shell.Current.GoToAsync(
                    $"{nameof(AssessmentEditPage)}?assessmentId={assessment.Id}");
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Navigation Error",
                    $"Unable to open assessment: {ex.Message}",
                    "OK");
            }
        }
    }
}