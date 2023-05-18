using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using TestDataGrid_01.DataModels;
using TestDataGrid_01.Helper;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;
using TableCell = DocumentFormat.OpenXml.Wordprocessing.TableCell;

namespace TestDataGrid_01.WordToXliff;

public class ConvertitoreWord_Xliff
{
    private static int imageCounter = 0;
    private static string SourceLanguage = "en-US";
    private static string TargetLanguage = "it-IT";

    public static void CreaXliff(string PercorsoWord, string PercorsoXliff)
    {
        if (!File.Exists(PercorsoWord))
        {
            throw new FileNotFoundException($"File not found: {PercorsoWord}");
        }
        
        var OggettiTesto = new List<DataModel>();
        
        using (var wordDoc = WordprocessingDocument.Open(PercorsoWord, false))
        {
            var mainDocumentPart = wordDoc.MainDocumentPart;
            // Process elements in the order they appear in the XML tree
            ProcessaElementi(mainDocumentPart.RootElement, wordDoc, OggettiTesto);
        }
        SalvaElementiComeXliff(OggettiTesto, PercorsoXliff);
    }

    private static void ProcessaElementi(OpenXmlElement elemento, WordprocessingDocument wordDoc, List<DataModel> oggettiTesto)
    {
        switch (elemento)
        {
            case Paragraph paragraph when
                !paragraph.Ancestors<TableCell>().Any() &&
                !paragraph.Ancestors<TextBoxContent>().Any():
                EstraiContenutoParagrafo(paragraph, oggettiTesto);
                break;
            case Table table:
                EstraiContenutoTabella(table, oggettiTesto);
                break;
            case HeaderReference headerReference:
                EstraiContenutoInstestazione(wordDoc, headerReference, oggettiTesto);
                break;
            case FooterReference footerReference:
                EstraiContenutoPiePagina(wordDoc, footerReference, oggettiTesto);
                break;
            case TextBoxContent textBoxContent:
                EstraiContenutoCasellaTesto(textBoxContent, oggettiTesto);
                break;
        }
        if (!(elemento is TextBoxContent) && !(elemento is DocumentFormat.OpenXml.Vml.Shape))
        {
            foreach (var childElement in elemento.Elements())
            {
                ProcessaElementi(childElement, wordDoc, oggettiTesto);
            }
        }
    }

    private static void EstraiContenutoParagrafo(Paragraph paragrafo, List<DataModel> oggettiTesto)
    {
        var segmentazione = RegoleSegmentazione.SplitParagraph(paragrafo);
        
        foreach (var customSegmentRuns in segmentazione)
        {
            var textsAndTags = new List<string>();
            var tagId = 1;

            foreach (var element in customSegmentRuns)
            {
                if (element is Run run)
                {
                    textsAndTags.AddRange(AggiungiTestoConTag(run, ref tagId));
                }
            }
            var result = string.Join("", textsAndTags);
            if (!string.IsNullOrWhiteSpace(result))
            {
                oggettiTesto.Add(new DataModel { Source = result });
            }
        }
    }

    private static List<string> AggiungiTestoConTag(Run run, ref int tagId)
    {
        var risultato = new List<string>();
        string NomeFileImmagine = null;
        foreach (var drawing in run.Elements<Drawing>())
        {
            imageCounter++;
            NomeFileImmagine = Helper_ProcessamentoWord.SalvaImmagini(drawing, Percorsi.PercorsoFileWord, imageCounter);
        }
        var runPropertiesTags = ManagerFormattazione.CreateRunPropertiesTags(run.RunProperties, ref tagId, NomeFileImmagine);
        risultato.AddRange(runPropertiesTags);
        
        var tElements = run.Elements<Text>().ToList();

        for (int i = 0; i < tElements.Count; i++)
        {
            if (i % 2 == 0)
            {
                risultato.Add(EncodeAngleBrackets(tElements[i].Text));

                string EncodeAngleBrackets(string text)
                {
                    return text.Replace("<", "&lt;").Replace(">", "&gt;");
                }
            }
            else
            {
                risultato.Add(tElements[i].Text);
            }
        }
        risultato.AddRange(runPropertiesTags.Reverse<string>()
            .Select(tag => ManagerFormattazione.CreateClosingTag(tag)));
        return risultato;
    }
    
    private static void EstraiContenutoTabella(Table table, List<DataModel> textItems)
    {
        foreach (var cell in table.Descendants<TableCell>())
        {
            foreach (var paragraph in cell.Elements<Paragraph>())
            {
                EstraiContenutoParagrafo(paragraph, textItems);
            }
        }
    }
    
    private static void EstraiContenutoInstestazione(WordprocessingDocument wordDoc, HeaderReference headerReference,
        List<DataModel> textItems)
    {
        var headerPart = (HeaderPart)wordDoc.MainDocumentPart?.GetPartById(headerReference.Id);
        foreach (var paragraph in headerPart?.RootElement?.Descendants<Paragraph>()!)
        {
            EstraiContenutoParagrafo(paragraph, textItems);
        }
    }
    
    private static void EstraiContenutoPiePagina(WordprocessingDocument wordDoc, FooterReference footerReference,
        List<DataModel> textItems)
    {
        var footerPart = (FooterPart)wordDoc.MainDocumentPart.GetPartById(footerReference.Id);
        foreach (var paragraph in footerPart.RootElement.Descendants<Paragraph>())
        {
            EstraiContenutoParagrafo(paragraph, textItems);
        }
    }
    
    private static void EstraiContenutoCasellaTesto(TextBoxContent textBoxContent, List<DataModel> textItems)
    {
        foreach (var paragraph in textBoxContent.Descendants<Paragraph>())
        {
            EstraiContenutoParagrafo(paragraph, textItems);
        }
    }

    private static void SalvaElementiComeXliff(List<DataModel> OggettiDatamodel, string percorsoXliff)
    {
        #region Struttura XLIFF

        XNamespace ns = "urn:oasis:names:tc:xliff:document:1.2";
        XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
        XNamespace schemaLocation = "urn:oasis:names:tc:xliff:document:1.2 xliff-core-1.2-transitional.xsd";
        var xliffDoc = new XElement(ns + "xliff",
            new XAttribute("version", "1.2"),
            new XAttribute(XNamespace.Xmlns + "xsi", xsi),
            new XAttribute(xsi + "schemaLocation", schemaLocation),
            new XElement(ns + "file",
                new XAttribute("original", Percorsi.PercorsoFileWord),
                new XAttribute("source-language", SourceLanguage),
                new XAttribute("target-language", TargetLanguage),
                new XAttribute("datatype", "plaintext"),
                new XElement(ns + "body",
                    OggettiDatamodel.Where(t => t.Source != null)
                        .Select((t, i) => new XElement(ns + "trans-unit",
                            new XAttribute("id", $"{i + 1}"),
                            new XElement(ns + "source", new XText(t.Source),
                                new XAttribute(XNamespace.Xml + "space", "preserve")
                            ),
                            new XElement(ns + "target", ""),
                                new XAttribute(XNamespace.Xml + "space", "preserve")
                        ))
                )
            )
        );

        #endregion

        var xliffString = xliffDoc.ToString();
        xliffString = xliffString.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;gt;", "&lt;")
            .Replace("&amp;lt;", "&gt;");
        using (var writer = new StreamWriter(percorsoXliff))
        {
            writer.Write(xliffString);
        }
    }
}
