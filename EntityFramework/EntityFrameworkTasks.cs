using DatabaseCourseExam.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatabaseCourseExam.EntityFramework;

public static class EntityFrameworkTasks
{
    // задание 4 фильтрация и сортировка
    public static async Task ShowStudentsAsync()
    {
        await using var context = SchoolDbContextFactory.Create();

        List<Student> students = await context.Students
            .Include(student => student.Group)
            .Where(student => student.Age >= 18)
            .OrderBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .ToListAsync();

        foreach (Student student in students)
        {
            Console.WriteLine(
                $"{student.Id}. {student.FirstName} {student.LastName}, " +
                $"возраст: {student.Age}, группа: {student.Group.Name}");
        }
    }

    public static async Task UpdateStudentAgeAsync(int studentId, int newAge)
    {
        await using var context = SchoolDbContextFactory.Create();

        Student? student = await context.Students.FirstOrDefaultAsync(x => x.Id == studentId);

        if (student is null)
        {
            Console.WriteLine("студент не найден.");
            return;
        }

        student.Age = newAge;
        int affectedRows = await context.SaveChangesAsync();
        Console.WriteLine($"возраст изменён. сохранено: {affectedRows}");
    }

    // задание 7 проекция и  вычисляемый признак
    public static async Task ShowCoursesAsync()
    {
        await using var context = SchoolDbContextFactory.Create();

        var courses = await context.Courses
            .OrderBy(course => course.Name)
            .Select(course => new
            {
                course.Name,
                course.Price,
                course.AvailablePlaces,
                IsFull = course.AvailablePlaces == 0 // признак
            })
            .ToListAsync();

        foreach (var course in courses)
        {
            string status = course.IsFull ? "МЕСТ НЕТ" : $"свободно: {course.AvailablePlaces}";
            Console.WriteLine($"{course.Name} | цена: {course.Price:N2} | {status}");
        }
    }
}