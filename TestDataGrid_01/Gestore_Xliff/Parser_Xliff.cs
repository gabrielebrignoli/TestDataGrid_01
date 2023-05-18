using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TestDataGrid_01.DataModels;
using TestDataGrid_01.WordToXliff;

namespace TestDataGrid_01.Gestore_Xliff;

public static class Parser_Xliff
{

    public static List<DataModel> Parse_Xliff(string fileXliff)
    {
        var ElencoOggettiXliff = new List<DataModel>();
        
        XDocument documentoXliff = XDocument.Load(fileXliff);
        XNamespace ns = "urn:oasis:names:tc:xliff:document:1.2";

        // Store the Xliff file content in the DataModel
        foreach (var TRANSUNIT in documentoXliff.Descendants(ns + "trans-unit"))
        {
            var idSegmento = int.Parse(TRANSUNIT.Attribute("id")?.Value);
            var source = Xliff_HelperMethods.GetInnerTextWithoutNamespace((TRANSUNIT.Element(ns + "source"))); 
            var target = Xliff_HelperMethods.GetInnerTextWithoutNamespace((TRANSUNIT.Element(ns + "target")));
            Console.WriteLine($"ID: {idSegmento}, Source: {source}, Target: {target}");

            var DatiXliff = new DataModel
            {
                ID = idSegmento,
                Source = Visualizzatore_XLIFF.ConvertToXamlString(source),
                Target = Visualizzatore_XLIFF.ConvertToXamlString(target)
            };
            ElencoOggettiXliff.Add(DatiXliff);
            DataModelExtensions.Print(DatiXliff);
        }
        return (ElencoOggettiXliff);
    }
}