using TestDataGrid_01.Data;

namespace TestDataGrid_01.Database;

public class Database_Helper
{
    public static bool DatabaseExists()
    {
        using (var context = new AppDbContext())
        {
            return context.Database.CanConnect();
        }
    }

    public static void InitializeDatabase()
    {
        using (var context = new AppDbContext())
        {
            context.Database.EnsureCreated();
        }
    }
}