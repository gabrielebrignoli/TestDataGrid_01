using System;

namespace TestDataGrid_01.DataModels
{
    public class DataModel
    {
        public int ID { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
    }

    //A method to print the data in DataModel in console
    public static class DataModelExtensions
    {
        public static void Print(this DataModel dataModel)
        {
            Console.WriteLine($"ID: {dataModel.ID}, Source: {dataModel.Source}, Target: {dataModel.Target}");
        }
    }
}