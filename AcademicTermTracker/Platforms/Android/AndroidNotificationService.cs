using Android.OS;
using Java.Util;
using AcademicTermTracker.Services;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Microsoft.Maui.ApplicationModel;


namespace AcademicTermTracker.Platforms.Android
{
    public class AndroidNotificationService : INotificationService
    {
        // Request notification permission
        public async Task<bool> RequestPermissionAsync()
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                return true;
            }

            var status = await Permissions.CheckStatusAsync<
                Permissions.PostNotifications>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<
                    Permissions.PostNotifications>();
            }

            return status == PermissionStatus.Granted;
        }

        // Schedule an individual Android notification
        private void ScheduleNotification(
            int notificationId,
            string title,
            string message,
            DateTime notificationDate)
        {
            if (notificationDate <= DateTime.Now)
            {
                return;
            }

            var context = global::Android.App.Application.Context;

            var alarmManager = context.GetSystemService(
                Context.AlarmService) as AlarmManager;

            if (alarmManager == null)
            {
                return;
            }

            var intent = new Intent();
            intent.SetClass(
                context,
                Java.Lang.Class.FromType(typeof(NotificationReceiver)));


            intent.PutExtra("notificationId", notificationId);
            intent.PutExtra("title", title);
            intent.PutExtra("message", message);

            PendingIntentFlags flags = PendingIntentFlags.UpdateCurrent;

            if (OperatingSystem.IsAndroidVersionAtLeast(23))
            {
                flags |= PendingIntentFlags.Immutable;
            }

            var pendingIntent = PendingIntent.GetBroadcast(
                context,
                notificationId,
                intent,
                flags);

            if (pendingIntent == null)
            {
                return;
            }

            long triggerTime = new DateTimeOffset(
                notificationDate).ToUnixTimeMilliseconds();

            if (OperatingSystem.IsAndroidVersionAtLeast(23))
            {
                alarmManager.SetAndAllowWhileIdle(
                    AlarmType.RtcWakeup,
                    triggerTime,
                    pendingIntent);
            }
            else
            {
                alarmManager.Set(
                    AlarmType.RtcWakeup,
                    triggerTime,
                    pendingIntent);
            }
        }

        // Cancel an individual Android notification
        private void CancelNotification(int notificationId)
        {
            var context = global::Android.App.Application.Context;

            var alarmManager = context.GetSystemService(
                Context.AlarmService) as AlarmManager;

            if (alarmManager == null)
            {
                return;
            }

            var intent = new Intent();

            intent.SetClass(
                context,
                Java.Lang.Class.FromType(typeof(NotificationReceiver)));

            PendingIntentFlags flags = PendingIntentFlags.NoCreate;

            if (OperatingSystem.IsAndroidVersionAtLeast(23))
            {
                flags |= PendingIntentFlags.Immutable;
            }

            var pendingIntent = PendingIntent.GetBroadcast(
                context,
                notificationId,
                intent,
                flags);

            if (pendingIntent != null)
            {
                alarmManager.Cancel(pendingIntent);
                pendingIntent.Cancel();
                pendingIntent.Dispose();
            }
        }

        // Course notification scheduling
        public Task ScheduleCourseNotificationsAsync(
            Models.Course course)
        {
            if (course == null)
            {
                throw new ArgumentNullException(nameof(course));
            }

            if (course.Id <= 0)
            {
                throw new ArgumentException(
                    "The course must be saved before scheduling notifications.");
            }

            int startNotificationId = checked(
                course.Id * 4 + 100000);

            int endNotificationId = checked(
                course.Id * 4 + 100001);

            // Remove previously scheduled reminders before updating them.
            CancelNotification(startNotificationId);
            CancelNotification(endNotificationId);

            if (course.StartNotificationEnabled)
            {
                ScheduleNotification(
                    startNotificationId,
                    "Course Starting",
                    $"{course.Title} starts today.",
                    course.StartDate.Date.AddHours(9));
            }

            if (course.EndNotificationEnabled)
            {
                ScheduleNotification(
                    endNotificationId,
                    "Course Ending",
                    $"{course.Title} ends today.",
                    course.EndDate.Date.AddHours(9));
            }

            return Task.CompletedTask;
        }

        // Assessment notification scheduling
        public Task ScheduleAssessmentNotificationsAsync(
            Models.Assessment assessment)
        {
            if (assessment == null)
            {
                throw new ArgumentNullException(nameof(assessment));
            }

            if (assessment.Id <= 0)
            {
                throw new ArgumentException(
                    "The assessment must be saved before scheduling notifications.");
            }

            int startNotificationId = checked(
                -((assessment.Id * 2) + 1));

            int endNotificationId = checked(
                -((assessment.Id * 2) + 2));

            // Cancel previously scheduled assessment reminders.
            CancelNotification(startNotificationId);
            CancelNotification(endNotificationId);

            if (assessment.StartNotificationEnabled)
            {
                ScheduleNotification(
                    startNotificationId,
                    "Assessment Starting",
                    $"{assessment.Title} starts today.",
                    assessment.StartDate.Date.AddHours(9));
            }

            if (assessment.EndNotificationEnabled)
            {
                ScheduleNotification(
                    endNotificationId,
                    "Assessment Ending",
                    $"{assessment.Title} ends today.",
                    assessment.EndDate.Date.AddHours(9));
            }

            return Task.CompletedTask;
        }

        // Course notification cancellation 
        public Task CancelCourseNotificationsAsync(int courseId)
        {
            if (courseId <= 0)
            {
                return Task.CompletedTask;
            }

            int startNotificationId = checked(courseId * 4 + 100000);
            int endNotificationId = checked(courseId * 4 + 100001);

            CancelNotification(startNotificationId);
            CancelNotification(endNotificationId);

            return Task.CompletedTask;
        }

        // Assessment notification cancellation
        public Task CancelAssessmentNotificationsAsync(
            int assessmentId)
        {
            if (assessmentId <= 0)
            {
                return Task.CompletedTask;
            }

            int startNotificationId = checked(
                -((assessmentId * 2) + 1));

            int endNotificationId = checked(
                -((assessmentId * 2) + 2));

            CancelNotification(startNotificationId);
            CancelNotification(endNotificationId);

            return Task.CompletedTask;
        }
    }
}