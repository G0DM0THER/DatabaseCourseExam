using System.Data;
using DatabaseCourseExam.Data;
using Npgsql;

namespace DatabaseCourseExam.AdoNet;

public static class DisconnectedTasks
{
    // задание 6 и 9
    public static async Task<DataTable> LoadStudentsAsync()
    {
        var dataSet = new DataSet("School");

        await using var connection = DatabaseConnection.Create();
        await connection.OpenAsync();

        // Students
        var studentsCmd = new NpgsqlCommand("SELECT id, first_name, last_name, age, group_id FROM students", connection);
        var studentsTable = new DataTable("Students");
        new NpgsqlDataAdapter(studentsCmd).Fill(studentsTable);
        studentsTable.PrimaryKey = [studentsTable.Columns["id"]!];

        // Groups
        var groupsCmd = new NpgsqlCommand("SELECT id, name FROM student_groups", connection);
        var groupsTable = new DataTable("Groups");
        new NpgsqlDataAdapter(groupsCmd).Fill(groupsTable);
        groupsTable.PrimaryKey = [groupsTable.Columns["id"]!];

        //задания 6
        var coursesCmd = new NpgsqlCommand("SELECT student_id, course_id FROM student_courses", connection);
        var studentCoursesTable = new DataTable("StudentCourses");
        new NpgsqlDataAdapter(coursesCmd).Fill(studentCoursesTable);

        dataSet.Tables.Add(groupsTable);
        dataSet.Tables.Add(studentsTable);
        dataSet.Tables.Add(studentCoursesTable);

        // задание 9
        dataSet.Relations.Add("GroupStudents", groupsTable.Columns["id"]!, studentsTable.Columns["group_id"]!);

        return studentsTable;
    }

    public static void PrintTable(DataTable table)
    {
        foreach (DataRow row in table.Rows)
        {
            Console.WriteLine(
                $"{row["id"]}: {row["first_name"]} " +
                $"{row["last_name"]}, возраст {row["age"]}");
        }
    }

    public static void ChangeAgeInMemory(DataTable table, int studentId, int newAge)
    {
        DataRow? row = table.Rows.Find(studentId);
        if (row is null) { Console.WriteLine("студент не найден."); return; }
        row["age"] = newAge;
        Console.WriteLine($"возраст изменён. состояние строки: {row.RowState}");
    }

    // задание 2 DataAdapter.Update
    public static int SaveChanges(DataTable table)
    {
        using var connection = DatabaseConnection.Create();
        connection.Open();

        const string sql = "UPDATE students SET age = @age WHERE id = @id;";
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add("@age", NpgsqlTypes.NpgsqlDbType.Integer, 4, "age");
        command.Parameters.Add("@id", NpgsqlTypes.NpgsqlDbType.Integer, 4, "id");

        using var adapter = new NpgsqlDataAdapter();
        adapter.UpdateCommand = command;

        int affectedRows = adapter.Update(table);
        Console.WriteLine($"сохранено изменений: {affectedRows}");
        //проверка
        foreach (DataRow row in table.Rows)
        {
            if (row.RowState == DataRowState.Modified)
                Console.WriteLine($"строка {row["id"]} всё ещё Modified!");
        }

        return affectedRows;
    }
}