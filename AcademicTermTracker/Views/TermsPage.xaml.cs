
using AcademicTermTracker.Models;
using AcademicTermTracker.Services;

namespace AcademicTermTracker.Views
{
    public partial class TermsPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public TermsPage(DatabaseService databaseService)
        {
            InitializeComponent();

            _databaseService = databaseService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                TermsCollectionView.ItemsSource =
                    await _databaseService.GetTermsAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Error",
                    $"Unable to load academic terms: {ex.Message}",
                    "OK");
            }
        }

        private async void OnAddTermClicked(
            object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(TermEditPage));
        }

        private async void OnEditTermClicked(
            object sender, EventArgs e)
        {
            if (sender is not Button button
                || button.BindingContext is not AcademicTerm term)
            {
                return;
            }

            await Shell.Current.GoToAsync(
                $"{nameof(TermEditPage)}?termId={term.Id}");
        }

        
        private async void OnTermSelected(
            object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault()
                is not AcademicTerm term)
            {
                return;
            }

            TermsCollectionView.SelectedItem = null;

            try
            {
                await Shell.Current.GoToAsync(
                    $"{nameof(CoursesPage)}?termId={term.Id}");
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Navigation Error",
                    $"Unable to open courses: {ex.Message}",
                    "OK");
            }
        }
    }
}
