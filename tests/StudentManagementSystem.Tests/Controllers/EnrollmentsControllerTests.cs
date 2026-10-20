using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Api.Controllers;
using StudentManagementSystem.Api.Data;
using StudentManagementSystem.Api.Models;

namespace StudentManagementSystem.Tests.Controllers;

public class EnrollmentsControllerTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static (AppDbContext Context, Student Student, Course Course) SeedStudentAndCourse()
    {
        var context = CreateContext();

        var student = new Student { FirstName = "Ivan", LastName = "Ivanov", Email = "ivan@example.com" };
        var course = new Course { Name = "Math", MaxCapacity = 10 };

        context.Students.Add(student);
        context.Courses.Add(course);
        context.SaveChanges();

        return (context, student, course);
    }

    [Fact]
    public void Enroll_FirstTime_ReturnsCreatedEnrollment()
    {
        var (context, student, course) = SeedStudentAndCourse();
        var controller = new EnrollmentsController(context);

        var result = controller.Enroll(new EnrollmentRequest { StudentId = student.Id, CourseId = course.Id });

        var createdResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Single(context.Enrollments);
    }

    [Fact]
    public void Enroll_DuplicateEnrollment_ReturnsConflictAndDoesNotCreateSecondRecord()
    {
        var (context, student, course) = SeedStudentAndCourse();
        var controller = new EnrollmentsController(context);

        controller.Enroll(new EnrollmentRequest { StudentId = student.Id, CourseId = course.Id });
        var secondResult = controller.Enroll(new EnrollmentRequest { StudentId = student.Id, CourseId = course.Id });

        Assert.IsType<ConflictObjectResult>(secondResult.Result);
        Assert.Single(context.Enrollments);
    }

    [Fact]
    public void Enroll_SameStudentDifferentCourse_Succeeds()
    {
        var (context, student, course) = SeedStudentAndCourse();
        var otherCourse = new Course { Name = "Physics", MaxCapacity = 10 };
        context.Courses.Add(otherCourse);
        context.SaveChanges();

        var controller = new EnrollmentsController(context);

        controller.Enroll(new EnrollmentRequest { StudentId = student.Id, CourseId = course.Id });
        var secondResult = controller.Enroll(new EnrollmentRequest { StudentId = student.Id, CourseId = otherCourse.Id });

        var createdResult = Assert.IsType<ObjectResult>(secondResult.Result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(2, context.Enrollments.Count());
    }
}
