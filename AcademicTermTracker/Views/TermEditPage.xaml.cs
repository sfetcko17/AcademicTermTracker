
using AcademicTermTracker.Models;
using AcademicTermTracker.Services;

namespace AcademicTermTracker.Views
{
    public partial class TermEditPage : ContentPage, IQueryAttributable
    {
        private readonly DatabaseService _databaseService;
        private int _termId;
        
        public TermEditPage(DatabaseService databaseService)
        {
            InitializeComponent();

            _databaseService = databaseService;

            _termId = 0;

            DeleteButton.IsVisible = false;
            PageHeading.Text = "Add Academic Term";
            Title = PageHeading.Text;

            StartDatePicker.Date = DateTime.Today;
            EndDatePicker.Date = DateTime.Today.AddMonths(6);
        }
        
        public void ApplyQueryAttributes(
            IDictionary<string, object> query)
        {
            if (query.TryGetValue("termId", out object? value) &&
                int.TryParse(value?.ToString(), out int termId) &&
                termId > 0)
            {
                _termId = termId;

                DeleteButton.IsVisible = true;
                PageHeading.Text = "Edit Academic Term";
                Title = PageHeading.Text;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_termId == 0)
            {
                return;
            }

            try
            {
                AcademicTerm? term =
                    await _databaseService.GetTermAsync(_termId);

                if (term == null)
                {
                    await DisplayAlert(
                        "Not Found",
                        "The academic term no longer exists.",
                        "OK");

                    await Shell.Current.GoToAsync("..");
                    return;
                }

                TitleEntry.Text = term.Title;
                StartDatePicker.Date = term.StartDate;
                EndDatePicker.Date = term.EndDate;
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Error",
                    $"Unable to load the term: {ex.Message}",
                    "OK");
            }
        }

        private async void OnSaveClicked(
            object sender, EventArgs e)
        {
            try
            {
                AcademicTerm term = new AcademicTerm
                {
                    Id = _termId,
                    Title = TitleEntry.Text ?? string.Empty,
                    StartDate = StartDatePicker.Date,
                    EndDate = EndDatePicker.Date
                };

                await _databaseService.SaveTermAsync(term);

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
                    $"Unable to save the term: {ex.Message}",
                    "OK");
            }
        }

        private async void OnDeleteClicked(
            object sender, EventArgs e)
        {
            if (_termId <= 0)
            {
                return;
            }

            bool confirmed = await DisplayAlert(
                "Delete Academic Term",
                "Deleting this term will also delete all " +
                "associated courses and assessments. Continue?",
                "Delete",
                "Cancel");

            if (!confirmed)
            {
                return;
            }

            try
            {
                AcademicTerm? term =
                    await _databaseService.GetTermAsync(_termId);

                if (term != null)
                {
                    await _databaseService.DeleteTermAsync(term);
                }

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Error",
                    $"Unable to delete the term: {ex.Message}",
                    "OK");
            }
        }

        private async void OnCancelClicked(
            object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}