using AcademicTermTracker.Services;

namespace AcademicTermTracker;

public partial class App : Application
{
	private readonly INotificationService _notificationService;
	private readonly DatabaseService _databaseService;
	public App(INotificationService notificationService, DatabaseService databaseService)
	{
		InitializeComponent();

		_notificationService = notificationService;
		_databaseService = databaseService;

		MainPage = new AppShell();
	}

	protected override void OnStart()
    {
        base.OnStart();

        _ = RequestNotificationPermissionAsync();
    }


	private async Task RequestNotificationPermissionAsync()
	{
		try
		{
			bool permissionGranted =
				await _notificationService.RequestPermissionAsync();

			if (permissionGranted)
			{
				await _databaseService.SynchronizeNotificationsAsync(
					_notificationService);
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(
				$"Notification synchronization failed: {ex.Message}");
		}
	}
}