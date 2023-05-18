using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using TestDataGrid_01.DataModels;
using TestDataGrid_01.WordToXliff;

namespace TestDataGrid_01.Loader_Files;

public class Loader_Csv
{
    public static List<DataModel> LoadDataFromCsv(string filePath)
    {
        var _dataModelCollection = new List<DataModel>();
        using (var reader = new StreamReader(filePath))
        {
            // Ignore the first line (headers)
            reader.ReadLine();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split('\t');
                var dataModel = new DataModel()
                {
                    ID = int.Parse(values[0]),
                    Source = Visualizzatore_XLIFF.ConvertToXamlString(values[1]),
                    Target = values.Length > 2 ? Visualizzatore_XLIFF.ConvertToXamlString(values[2]) : Visualizzatore_XLIFF.ConvertToXamlString("")
                };
                _dataModelCollection.Add(dataModel);
                DataModelExtensions.Print(dataModel);
            }
        }
        return (_dataModelCollection);
    }
}