using DatabaseCourseExam.Data;
using Dapper;
using DatabaseCourseExam.DapperTasks.DTO;

namespace DatabaseCourseExam.DapperTasks;

public static class DapperStudentTasks
{
    public static async Task ShowStudentsAsync()
    {
        await using var connection = DatabaseConnection.Create();

        const string sql = """
            SELECT s.id AS Id, s.first_name AS FirstName, s.last_name AS LastName, 
                   s.age AS Age, g.name AS GroupName
            FROM students AS s
            JOIN student_groups AS g ON g.id = s.group_id
            ORDER BY s.id;
            """;

        var students = await connection.QueryAsync<StudentDto>(sql);

        foreach (StudentDto student in students)
        {
            Console.WriteLine($"{student.Id}. {student.FirstName} {student.LastName}, возраст: {student.Age}, группа: {student.GroupName}");
        }
    }

    // задание 3 ExecuteAsync
    public static async Task UpdateStudentAgeAsync(int studentId, int newAge)
    {
        //проверка возраста
        if (newAge is < 14 or > 100)
        {
            Console.WriteLine("возраст должен быть от 14 до 100.");
            return;
        }

        await using var connection = DatabaseConnection.Create();

        const string sql = "UPDATE students SET age = @NewAge WHERE id = @StudentId;";

        int affectedRows = await connection.ExecuteAsync(sql, new { NewAge = newAge, StudentId = studentId });
        //затронутые строки
        Console.WriteLine(affectedRows == 0
            ? "студент не найден."
            : $"изменено записей: {affectedRows}");
    }

    public static async Task ShowCoursesAsync()
    {
        await using var connection = DatabaseConnection.Create();

        const string sql = """
            SELECT id AS Id, name AS Name, price AS Price, available_places AS AvailablePlaces
            FROM courses
            ORDER BY name;
            """;

        var courses = await connection.QueryAsync<CourseDto>(sql);

        foreach (CourseDto course in courses)
        {
            Console.WriteLine($"{course.Name} | цена: {course.Price:N2} | свободных мест: {course.AvailablePlaces}");
        }
    }

    public static async Task EnrollStudentAsync(int studentId, int courseId)
    {
        await using var connection = DatabaseConnection.Create();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            const string updateSql = """
                UPDATE courses
                SET available_places = available_places - 1
                WHERE id = @CourseId AND available_places > 0
                RETURNING available_places;
                """;

            int? remainingPlaces = await connection.QuerySingleOrDefaultAsync<int?>(updateSql, new { CourseId = courseId }, transaction);
            if (remainingPlaces is null) throw new InvalidOperationException("курс не найден или свободных мест нет.");

            const string insertSql = "INSERT INTO student_courses (student_id, course_id) VALUES (@StudentId, @CourseId);";
            await connection.ExecuteAsync(insertSql, new { StudentId = studentId, CourseId = courseId }, transaction);

            await transaction.CommitAsync();
            Console.WriteLine($"студент записан. осталось мест: {remainingPlaces}");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"отменено: {ex.Message}");
        }
    }
}