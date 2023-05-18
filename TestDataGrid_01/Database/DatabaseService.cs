using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using TestDataGrid_01.DataModels;
using TestDataGrid_01.ViewModels;
using TestDataGrid_01.WordToXliff;

namespace TestDataGrid_01
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly string _databasePath;

        public DatabaseService(string databasePath)
        {
            _connectionString = $"Data Source={databasePath}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var createTableCommand = connection.CreateCommand();
            createTableCommand.CommandText =
            @" 
                CREATE TABLE IF NOT EXISTS documents (
                    id INTEGER PRIMARY KEY,
                    source TEXT NOT NULL,
                    target TEXT  NOT NULL
                );
            ";
            createTableCommand.ExecuteNonQuery();
        }

        public List<DocumentViewModel> GetAllDocuments()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var selectAllCommand = connection.CreateCommand();
            selectAllCommand.CommandText = "SELECT id, source, target FROM documents";

            using var reader = selectAllCommand.ExecuteReader();
            var documents = new List<DocumentViewModel>();

            while (reader.Read())
            {
                documents.Add(new DocumentViewModel
                {
                    ID = reader.GetInt32(0),
                    Source =reader.GetString(1),
                    Target = reader.GetString(2)
                });
            }

            return documents;
        }

        private void AddDocument(DataModel document)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = "INSERT INTO documents (source, target) VALUES (@source, @target)";
            insertCommand.Parameters.AddWithValue("@source", document.Source);
            insertCommand.Parameters.AddWithValue("@target", document.Target);
            insertCommand.ExecuteNonQuery();
        }

        public void AddDataModels(IEnumerable<DataModel> dataModels)
        {
            foreach (var dataModel in dataModels)
            {
                AddDocument(dataModel);
            }
        }
        
        /*public bool IsDataDifferentFromDatabase(IEnumerable<DataModel> dataModels)
        {
            var documents = GetAllDocuments();
            var dataModelCount = dataModels.Count();
            var documentCount = documents.Count;

            if (dataModelCount != documentCount)
            {
                return true;
            }

            for (int i = 0; i < dataModelCount; i++)
            {
                if (dataModels.ElementAt(i).ID != documents[i].ID ||
                    dataModels.ElementAt(i).Source != documents[i].Source ||
                    dataModels.ElementAt(i).Target != documents[i].Target)
                {
                    return true;
                }
            }

            return false;
        }*/
    }
}
