using SQLite;
using AcademicTermTracker.Models;

namespace AcademicTermTracker.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;
        private readonly SemaphoreSlim _initializationLock = new(1, 1);
        private bool _isInitialized;
        private readonly SemaphoreSlim _courseSaveLock = new(1, 1);
        private readonly SemaphoreSlim _assessmentSaveLock = new(1, 1);


        // DATABASE INITIALIZATION
        private async Task InitializeAsync()
        {
            if (_isInitialized)
            {
                return;
            }

            await _initializationLock.WaitAsync();

            try
            {
                if (_isInitialized)
                {
                    return;
                }

                string databasePath = Path.Combine(
                    FileSystem.AppDataDirectory,
                    "AcademicTermTracker.db3");

                _database ??= new SQLiteAsyncConnection(databasePath);

                await _database.CreateTableAsync<AcademicTerm>();
                await _database.CreateTableAsync<Course>();
                await _database.CreateTableAsync<Assessment>();

                _isInitialized = true;
            }
            finally
            {
                _initializationLock.Release();
            }
        }


        // ACADEMIC TERM DATABASE OPERATIONS

        // Retrieve all academic terms
        public async Task<List<AcademicTerm>> GetTermsAsync()
        {
            await InitializeAsync();

            return await _database!.Table<AcademicTerm>()
                .OrderBy(t => t.StartDate)
                .ToListAsync();
        }

        // Retrieve a specific academic term
        public async Task<AcademicTerm?> GetTermAsync(int termId)
        {
            await InitializeAsync();

            return await _database!.Table<AcademicTerm>()
                .Where(t => t.Id == termId)
                .FirstOrDefaultAsync();
        }

        // Create or update an academic term
        public async Task<int> SaveTermAsync(AcademicTerm term)
        {
            await InitializeAsync();

            if (string.IsNullOrWhiteSpace(term.Title))
            {
                throw new ArgumentException(
                    "Academic term title is required.");
            }

            if (term.StartDate >= term.EndDate)
            {
                throw new ArgumentException(
                    "The term start date must be before the end date.");
            }

            term.Title = term.Title.Trim();

            if (term.Id == 0)
            {
                return await _database!.InsertAsync(term);
            }

            return await _database!.UpdateAsync(term);
        }

        // Delete an academic term        
        public async Task<int> DeleteTermAsync(AcademicTerm term)
        {
            await InitializeAsync();

            if (term.Id <= 0)
            {
                throw new ArgumentException(
                    "A valid academic term ID is required.");
            }

            int deletedTerms = 0;

            await _database!.RunInTransactionAsync(connection =>
            {
                // Delete assessments belonging to courses in this term
                connection.Execute(
                    @"DELETE FROM Assessment
                    WHERE CourseId IN (
                        SELECT Id FROM Course WHERE TermId = ?
                    )",
                    term.Id);

                // Delete all courses associated with this term
                connection.Execute(
                    "DELETE FROM Course WHERE TermId = ?",
                    term.Id);

                // Delete the academic term itself
                deletedTerms = connection.Execute(
                    "DELETE FROM AcademicTerm WHERE Id = ?",
                    term.Id);
            });

            return deletedTerms;
        }

    
        // COURSE DATABASE OPERATIONS

        // Retrieve all courses belonging to a term
        public async Task<List<Course>> GetCoursesAsync(int termId)
        {
            await InitializeAsync();

            return await _database!.Table<Course>()
                .Where(c => c.TermId == termId)
                .OrderBy(c => c.StartDate)
                .ToListAsync();
        }

        // Retrieve a specific course
        public async Task<Course?> GetCourseAsync(int courseId)
        {
            await InitializeAsync();

            return await _database!.Table<Course>()
                .Where(c => c.Id == courseId)
                .FirstOrDefaultAsync();
        }

        // Create or update a course        
        public async Task<int> SaveCourseAsync(Course course)
        {
            await InitializeAsync();

            await _courseSaveLock.WaitAsync();

            try
            {
                if (string.IsNullOrWhiteSpace(course.Title) ||
                    string.IsNullOrWhiteSpace(course.Status) ||
                    string.IsNullOrWhiteSpace(course.InstructorName) ||
                    string.IsNullOrWhiteSpace(course.InstructorPhone) ||
                    string.IsNullOrWhiteSpace(course.InstructorEmail))
                {
                    throw new ArgumentException(
                        "All required course fields must be completed.");
                }

                if (course.StartDate >= course.EndDate)
                {
                    throw new ArgumentException(
                        "The course start date must be before the end date.");
                }

                course.Title = course.Title.Trim();
                course.Status = course.Status.Trim();
                course.InstructorName = course.InstructorName.Trim();
                course.InstructorPhone = course.InstructorPhone.Trim();
                course.InstructorEmail = course.InstructorEmail.Trim();

                if (!System.Net.Mail.MailAddress.TryCreate(
                        course.InstructorEmail, out _))
                {
                    throw new ArgumentException(
                        "A valid instructor email address is required.");
                }

                string phoneDigits = new string(
                    course.InstructorPhone
                        .Where(char.IsDigit)
                        .ToArray());

                if(phoneDigits.Length < 10 || phoneDigits.Length > 15)
                {
                    throw new ArgumentException(
                        "A valid instructor phone number is required.");
                }

                string[] validStatuses =
                {
                    "In Progress",
                    "Completed",
                    "Dropped",
                    "Plan to Take"
                };

                if (!validStatuses.Contains(
                        course.Status,
                        StringComparer.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(
                        "Please select a valid course status.");
                }

                course.Status = validStatuses.First(s =>
                    s.Equals(course.Status, StringComparison.OrdinalIgnoreCase));

                if (course.TermId <= 0 ||
                    await GetTermAsync(course.TermId) == null)
                {
                    throw new ArgumentException(
                        "The course must belong to an existing academic term.");
                }

                if (course.Id == 0)
                {
                    int courseCount = await _database!.Table<Course>()
                        .Where(c => c.TermId == course.TermId)
                        .CountAsync();

                    if (courseCount >= 6)
                    {
                        throw new InvalidOperationException(
                            "An academic term cannot contain more than six courses.");
                    }

                    return await _database!.InsertAsync(course);
                }

                Course? existingCourse = await GetCourseAsync(course.Id);

                if (existingCourse == null)
                {
                    throw new InvalidOperationException(
                        "The course being edited no longer exists.");
                }

                if (existingCourse.TermId != course.TermId)
                {
                    int destinationCount = await _database!.Table<Course>()
                        .Where(c => c.TermId == course.TermId)
                        .CountAsync();

                    if (destinationCount >= 6)
                    {
                        throw new InvalidOperationException(
                            "The destination term already contains six courses.");
                    }
                }

                return await _database!.UpdateAsync(course);
            }
            finally
            {
                _courseSaveLock.Release();
            }
        }


        // Delete a course      
        public async Task<int> DeleteCourseAsync(Course course)
        {
            await InitializeAsync();

            if (course.Id <= 0)
            {
                throw new ArgumentException(
                    "A valid course ID is required.");
            }
            
            int deletedCourses = 0;

            await _database!.RunInTransactionAsync(connection =>
            {
                // Delete assessments associated with this course
                connection.Execute(
                    "DELETE FROM Assessment WHERE CourseId = ?",
                    course.Id);

                //Delete the course itself
                deletedCourses = connection.Execute(
                    "DELETE FROM Course WHERE Id = ?",
                    course.Id);
            });

            return deletedCourses;
        }
    
    
        // ASSESSMENT DATABASE OPERATIONS

        // Retrieve all assessments for a course
        public async Task<List<Assessment>> GetAssessmentsAsync(int courseId)
        {
            await InitializeAsync();

            return await _database!.Table<Assessment>()
                .Where(a => a.CourseId == courseId)
                .OrderBy(a => a.StartDate)
                .ToListAsync();
        }

        // Retrieve a specific assessment
        public async Task<Assessment?> GetAssessmentAsync(int assessmentId)
        {
            await InitializeAsync();

            return await _database!.Table<Assessment>()
                .Where(a => a.Id == assessmentId)
                .FirstOrDefaultAsync();
        }

        // Create or update an assessment
        public async Task<int> SaveAssessmentAsync(Assessment assessment)
        {
            await InitializeAsync();

            await _assessmentSaveLock.WaitAsync();

            try
            {
                if (string.IsNullOrWhiteSpace(assessment.Title))
                {
                    throw new ArgumentException(
                        "Assessment title is required.");
                }

                if (assessment.StartDate >= assessment.EndDate)
                {
                    throw new ArgumentException(
                        "The assessment start date must be before the end date.");
                }

                if (string.IsNullOrWhiteSpace(assessment.Type))
                {
                    throw new ArgumentException(
                        "Assessment type is required.");
                }

                string assessmentType = assessment.Type.Trim();

                if (!assessmentType.Equals(
                        "Objective", StringComparison.OrdinalIgnoreCase) &&
                    !assessmentType.Equals(
                        "Performance", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(
                        "Assessment type must be Objective or Performance.");
                }

                assessment.Type = assessmentType.Equals(
                    "Objective", StringComparison.OrdinalIgnoreCase)
                    ? "Objective"
                    : "Performance";

                if (assessment.CourseId <= 0 ||
                    await GetCourseAsync(assessment.CourseId) == null)
                {
                    throw new ArgumentException(
                        "Assessment must belong to an existing course.");
                }

                Assessment? existingAssessment = null;

                if (assessment.Id != 0)
                {
                    existingAssessment =
                        await GetAssessmentAsync(assessment.Id);

                    if (existingAssessment == null)
                    {
                        throw new InvalidOperationException(
                            "The assessment being edited no longer exists.");
                    }
                }

                List<Assessment> existingAssessments =
                    await GetAssessmentsAsync(assessment.CourseId);

                bool duplicateType = existingAssessments.Any(a =>
                    a.Id != assessment.Id &&
                    a.Type.Equals(
                        assessment.Type,
                        StringComparison.OrdinalIgnoreCase));

                if (duplicateType)
                {
                    throw new InvalidOperationException(
                        $"This course already has a {assessment.Type} assessment.");
                }

                assessment.Title = assessment.Title.Trim();

                if (assessment.Id == 0)
                {
                    return await _database!.InsertAsync(assessment);
                }

                return await _database!.UpdateAsync(assessment);
            }
            finally
            {
                _assessmentSaveLock.Release();
            }
        }

        // Delete an assessment
        public async Task<int> DeleteAssessmentAsync(Assessment assessment)
        {
            await InitializeAsync();

            return await _database!.DeleteAsync(assessment);
        }

        //Sample data seeding
        public async Task SeedSampleDataAsync()
        {
            await InitializeAsync();

            await _database!.RunInTransactionAsync(connection =>
            {
                // Only seed when the database contains no terms.
                int existingTerms = connection.Table<AcademicTerm>()
                    .Count();

                if (existingTerms > 0)
                {
                    return;
                }

                // Create sample academic term.
                AcademicTerm term = new AcademicTerm
                {
                    Title = "Sample Academic Term",
                    StartDate = new DateTime(2026, 11, 1),
                    EndDate = new DateTime(2027, 4, 30)
                };

                connection.Insert(term);

                // Create sample course.
                Course course = new Course
                {
                    TermId = term.Id,
                    Title = "Software Engineering",
                    StartDate = new DateTime(2026, 11, 1),
                    EndDate = new DateTime(2027, 1, 31),
                    Status = "In Progress",
                    InstructorName = "Anika Patel",
                    InstructorPhone = "555-123-4567",
                    InstructorEmail = "anika.patel@strimeuniversity.edu",
                    Notes = "Sample course for application evaluation."
                };

                connection.Insert(course);

                // Create objective assessment.
                Assessment objective = new Assessment
                {
                    CourseId = course.Id,
                    Title = "Software Engineering Objective Assessment",
                    Type = "Objective",
                    StartDate = new DateTime(2026, 12, 1),
                    EndDate = new DateTime(2026, 12, 15)
                };

                connection.Insert(objective);

                // Create performance assessment.
                Assessment performance = new Assessment
                {
                    CourseId = course.Id,
                    Title = "Software Engineering Performance Assessment",
                    Type = "Performance",
                    StartDate = new DateTime(2026, 12, 16),
                    EndDate = new DateTime(2027, 1, 15)
                };

                connection.Insert(performance);
            });
        }

        // NOTIFICATION SYNCHRONIZATION       
        public async Task SynchronizeNotificationsAsync(
            INotificationService notificationService)
        {
            List<AcademicTerm> terms = await GetTermsAsync();

            foreach (AcademicTerm term in terms)
            {
                List<Course> courses =
                    await GetCoursesAsync(term.Id);

                foreach (Course course in courses)
                {
                    await notificationService
                        .ScheduleCourseNotificationsAsync(course);

                    List<Assessment> assessments =
                        await GetAssessmentsAsync(course.Id);

                    foreach (Assessment assessment in assessments)
                    {
                        await notificationService
                            .ScheduleAssessmentNotificationsAsync(
                                assessment);
                    }
                }
            }
        }
    }
}