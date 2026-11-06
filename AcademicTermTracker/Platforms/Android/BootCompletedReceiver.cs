using AcademicTermTracker.Services;
using Android.App;
using Android.Content;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicTermTracker.Platforms.Android
{
    [BroadcastReceiver(
        Enabled = true,
        Exported = true)]
    [IntentFilter(new[]
    {
        Intent.ActionBootCompleted
    })]
    public class BootCompletedReceiver : BroadcastReceiver
    {
        public override void OnReceive(
            Context? context,
            Intent? intent)
        {
            if (intent?.Action != Intent.ActionBootCompleted)
            {
                return;
            }

            PendingResult? pendingResult = GoAsync();

            _ = Task.Run(async () =>
            {
                try
                {
                    DatabaseService databaseService =
                        IPlatformApplication.Current!
                            .Services
                            .GetRequiredService<DatabaseService>();

                    INotificationService notificationService =
                        IPlatformApplication.Current!
                            .Services
                            .GetRequiredService<INotificationService>();

                    await databaseService
                        .SynchronizeNotificationsAsync(
                            notificationService);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Boot notification restoration failed: {ex.Message}");
                }
                finally
                {
                    pendingResult?.Finish();
                }
            });
        }
    }
}