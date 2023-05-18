using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using TestDataGrid_01.DataModels;
using TestDataGrid_01.ViewModels;


namespace TestDataGrid_01.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<DataModel> DataModels { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={Percorsi.PercorsoDatabase}");
        }
    }

    public static class DataModelRepository
    {
        public static void UpdateDatabase(List<DataModel> dataModels)
        {
            using (var context = new AppDbContext())
            {
                foreach (var dataModel in dataModels)
                {
                    // Check if the record exists in the database
                    var existingRecord = context.DataModels.FirstOrDefault(x => x.ID == dataModel.ID);

                    if (existingRecord == null)
                    {
                        // Add a new record
                        context.DataModels.Add(dataModel);
                    }
                    else
                    {
                        // Update the existing record
                        existingRecord.Source = dataModel.Source;
                        existingRecord.Target = dataModel.Target;
                    }
                }
                // Save changes to the database
                context.SaveChanges();
            }
        }

        public static void UpdateDatabaseRealTime(object dataContext, int rowIndex, int columnIndex, string content)
        {
            if (dataContext is MainViewModel mainViewModel)
            {
                using (var context = new AppDbContext())
                {
                    // Get the corresponding DocumentViewModel from the DataItems collection
                    var documentViewModel = mainViewModel.DataItems[rowIndex];

                    // Update the property based on the columnIndex
                    if (columnIndex == 1)
                    {
                        documentViewModel.Source = content;
                    }
                    else if (columnIndex == 2)
                    {
                        documentViewModel.Target = content;
                    }

                    // Find the corresponding DataModel in the database
                    var dataModel = context.DataModels.FirstOrDefault(d => d.ID == documentViewModel.ID);

                    if (dataModel != null)
                    {
                        // Update the DataModel with the new content
                        dataModel.Source = documentViewModel.Source;
                        dataModel.Target = documentViewModel.Target;

                        // Save changes to the database
                        context.SaveChanges();
                    }
                }
            }
        }
    }
}