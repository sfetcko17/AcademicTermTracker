using AcademicTermTracker.Models;

namespace AcademicTermTracker.Services
{
    public interface INotificationService
    {
        Task<bool> RequestPermissionAsync();

        Task ScheduleCourseNotificationsAsync(Course course);

        Task ScheduleAssessmentNotificationsAsync(
            Assessment assessment);

        Task CancelCourseNotificationsAsync(int courseId);

        Task CancelAssessmentNotificationsAsync(
            int assessmentId);
    }
}