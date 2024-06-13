using ApiCrud.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiCrud.Students;

public static class StudentsRoutes
{
    public static void AddStudentsRoutes(this WebApplication app)
    {
        var studentsRoutes = app.MapGroup("students");

        studentsRoutes.MapPost("", async (AddStudentRequest request, AppDbContext context, CancellationToken ct) =>
        {

            var studentExists = await context.Students.AnyAsync(student => student.Name == request.Name, ct);

            if (studentExists) return Results.Conflict("Já existe");

            var newStudent = new Student(request.Name);
            await context.Students.AddAsync(newStudent, ct);
            await context.SaveChangesAsync(ct);

            var student = new StudentDto(newStudent.Id, newStudent.Name);

            return Results.Ok(student);
        });

        studentsRoutes.MapGet("", async (AppDbContext context, CancellationToken ct) =>
        {   
            var students = await context
            .Students
            .Where(student => student.Active)
            .Select(student => new StudentDto(student.Id, student.Name))
            .ToListAsync(ct);
            return students;
        });

        studentsRoutes.MapPut("{id:guid}", async (Guid id, UpdateStudentRequest request, AppDbContext context, CancellationToken ct) =>
        {
            var student = await context.Students.SingleOrDefaultAsync(student => student.Id == id, ct); 


            if(student is null)
            {
                return Results.NotFound();
            }

            student.UpdateName(request.Name);

            await context.SaveChangesAsync(ct);

            return Results.Ok(new StudentDto(student.Id, student.Name));
        });


        studentsRoutes.MapDelete("{id}", async (Guid id, AppDbContext context, CancellationToken ct) =>
        {
            var student = await context.Students.SingleOrDefaultAsync(student => student.Id == id, ct);

            if (student is null)
            {
                return Results.NotFound();
            }

            student.Deactive();

            await context.SaveChangesAsync(ct);
            return Results.Ok();
        });
    }
}
