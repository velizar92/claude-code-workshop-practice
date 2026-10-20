# CLAUDE.md

Guidance for Claude Code (and other contributors) working in this repository.

## Project overview

StudentManagementSystem is a small ASP.NET Core Web API for managing students,
courses, and course enrollments. It exposes CRUD endpoints for students and
courses, plus endpoints to enroll a student in a course, list a student's
courses, list a course's students, and remove an enrollment.

**Stack:**
- C# / .NET 10
- ASP.NET Core Web API (controller-based, not minimal APIs)
- Entity Framework Core with SQLite (`studentmanagement.db`)
- Swagger / OpenAPI (Swashbuckle) for interactive API docs
- xUnit for the test project

**Structure:**
- `src/StudentManagementSystem.Api/Controllers/` — API controllers
  (`StudentsController`, `CoursesController`, `EnrollmentsController`)
- `src/StudentManagementSystem.Api/Models/` — EF Core entities (`Student`,
  `Course`, `Enrollment`)
- `src/StudentManagementSystem.Api/Data/` — `AppDbContext` and `SeedData`
- `tests/StudentManagementSystem.Tests/` — xUnit test project
- `StudentManagementSystem.sln` — solution file referencing both projects

## Build, run, and test

```bash
dotnet restore
dotnet build
dotnet run --project src/StudentManagementSystem.Api
dotnet test
```

The API uses `Database.EnsureCreated()` plus deterministic seed data on
startup — see `README.md` for details on resetting the local database.

## Before making changes

- Read the existing controller and model for the area you're touching before
  editing it — the three controllers currently follow the same shape
  (constructor-injected `AppDbContext`, synchronous EF Core calls), so new
  code should match that shape unless there's a deliberate reason to diverge.
- Check `tests/StudentManagementSystem.Tests/` for existing coverage of the
  code you're changing before assuming there is none.
- Prefer understanding why a piece of code is the way it is over guessing;
  when in doubt, trace the request path from controller to `AppDbContext`.

## Conventions

- Namespaces and folders follow the feature-area layout already in place:
  `StudentManagementSystem.Api.Controllers`, `.Models`, `.Data`.
- Controllers are named `<Resource>Controller` and routed at
  `api/<resource>` (e.g. `api/students`, `api/courses`, `api/enrollments`).
- Use file-scoped namespaces and standard C# naming (PascalCase for public
  members and types, camelCase for locals/parameters, `_camelCase` for
  private fields).
- Return `ActionResult<T>` from controller actions and use the standard
  ASP.NET Core result helpers (`Ok`, `NotFound`, `CreatedAtAction`,
  `NoContent`, etc.) rather than throwing exceptions for expected
  not-found/validation cases.
- Keep request/response DTOs close to the controller that uses them unless
  they're shared across multiple controllers.

## Where business logic belongs

Business rules (e.g. enrollment validation, capacity checks, duplicate
prevention) should live in a service layer under
`src/StudentManagementSystem.Api/Services/`, injected into controllers via
dependency injection — not written inline in controller actions. Controllers
should be responsible for HTTP concerns only: routing, request binding, and
translating service results into HTTP responses and status codes. If no
service exists yet for the area you're changing, introduce one rather than
growing the controller method further.

## Testing and build expectations

- Any change to business logic must be covered by a corresponding test in
  `tests/StudentManagementSystem.Tests/`.
- Before considering a task complete, run `dotnet build` and `dotnet test`
  and confirm both succeed.

## Workflow

- Work against a GitHub Issue where one exists.
- Create a dedicated branch per issue/change rather than committing directly
  to `main`.
- Open a Pull Request for review instead of pushing finished work straight
  to `main`.
