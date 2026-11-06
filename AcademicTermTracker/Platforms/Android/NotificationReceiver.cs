using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

namespace AcademicTermTracker.Platforms.Android
{
    [BroadcastReceiver(
        Enabled = true,
        Exported = false)]
    public class NotificationReceiver : BroadcastReceiver
    {
        public const string ChannelId = "academic_term_reminders";

        public override void OnReceive(
            Context? context,
            Intent? intent)
        {
            if (context == null || intent == null)
            {
                return;
            }

            int notificationId = intent.GetIntExtra(
                "notificationId", 0);

            string title = intent.GetStringExtra("title")
                ?? "Academic Reminder";

            string message = intent.GetStringExtra("message")
                ?? "You have an upcoming academic event.";

            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                var channel = new NotificationChannel(
                    ChannelId,
                    "Academic Reminders",
                    NotificationImportance.Default)
                {
                    Description =
                        "Course and assessment date reminders"
                };

                var manager = context.GetSystemService(
                    Context.NotificationService)
                    as NotificationManager;

                manager?.CreateNotificationChannel(channel);
            }

            var notification = new NotificationCompat.Builder(
                context,
                ChannelId)
                .SetSmallIcon(
                    global::Android.Resource.Drawable.IcDialogInfo)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetAutoCancel(true)
                .SetPriority(NotificationCompat.PriorityDefault)
                .Build();

            var notificationManager =
                NotificationManagerCompat.From(context);

            if (OperatingSystem.IsAndroidVersionAtLeast(33) &&
                context.CheckSelfPermission(
                    global::Android.Manifest.Permission.PostNotifications)
                    != global::Android.Content.PM.Permission.Granted)
            {
                return;
            }

            notificationManager.Notify(
                notificationId,
                notification);
        }
    }
}