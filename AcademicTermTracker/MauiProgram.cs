using Microsoft.Extensions.Logging;
using AcademicTermTracker.Services;
using AcademicTermTracker.Views;

namespace AcademicTermTracker;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif
		// Register database service and app pages
		builder.Services.AddSingleton<DatabaseService>();
		builder.Services.AddTransient<TermsPage>();
		builder.Services.AddTransient<TermEditPage>();
		builder.Services.AddTransient<CoursesPage>();
		builder.Services.AddTransient<CourseEditPage>();

		return builder.Build();
	}
}
