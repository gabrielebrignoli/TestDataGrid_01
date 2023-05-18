using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace TestDataGrid_01.Helper;

public class Helper_ProcessamentoWord
{
    public static string SalvaImmagini(Drawing drawing, string docxPath, int imageCounter)
    {
        using (var wordDoc = WordprocessingDocument.Open(docxPath, false))
        {
            var imagePart = drawing.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().FirstOrDefault()?.Embed?.Value;
            if (imagePart != null)
            {
                var imagePartById = wordDoc.MainDocumentPart.GetPartById(imagePart) as ImagePart;

                if (imagePartById != null)
                {
                    var imageFileNameWithExtension = $"{Path.GetFileNameWithoutExtension(docxPath)}_image_{imageCounter}{Path.GetExtension(imagePartById.Uri.ToString())}";
                    var DirectoryImmagini = Path.Combine(Percorsi.WorkingDir, "Immagini");
                    if (!Directory.Exists(DirectoryImmagini))
                    {
                        Directory.CreateDirectory(DirectoryImmagini);
                    }
                    var outputImagePath = Path.Combine(DirectoryImmagini, imageFileNameWithExtension);

                    using (var fileStream = new FileStream(outputImagePath, FileMode.Create))
                    {
                        imagePartById.GetStream().CopyTo(fileStream);
                    }

                    return imageFileNameWithExtension;
                }
            }
        }
        return null;
    }
}
public abstract class ManagerFormattazione
{
    public static List<string> CreateRunPropertiesTags(RunProperties runProperties, ref int tagId, string imageFileName = null)
    {
        var tags = new List<string>();

            if (runProperties != null)
            {
                if (runProperties.Bold != null)
                {
                    tags.Add($"<bpt id=\"{tagId++}\" ctype=\"bold\">{{}}</bpt>");
                }
                if (runProperties.Italic != null)
                {
                    tags.Add($"<bpt id=\"{tagId++}\" ctype=\"italic\">{{}}</bpt>");
                }
                if (runProperties.Underline != null)
                {
                    tags.Add($"<bpt id=\"{tagId++}\" ctype=\"underline\">{{}}</bpt>");
                }
                if (runProperties.FontSize != null)
                {
                    tags.Add($"<bpt id=\"{tagId++}\" ctype=\"fontsize\" size=\"{runProperties.FontSize.Val}\">{{}}</bpt>");
                }
                if (runProperties.RunFonts != null && runProperties.RunFonts.Ascii != null)
                {
                    tags.Add($"<bpt id=\"{tagId++}\" ctype=\"fonttype\" type=\"{runProperties.RunFonts.Ascii.Value}\">{{}}</bpt>");
                }
                if (runProperties.VerticalTextAlignment != null && runProperties.VerticalTextAlignment.Val == VerticalPositionValues.Superscript)
                {
                    tags.Add($"<bpt id=\"{tagId++}\" ctype=\"superscript\">{{}}</bpt>");
                }
                if (runProperties.VerticalTextAlignment != null && runProperties.VerticalTextAlignment.Val == VerticalPositionValues.Subscript)
                {
                    tags.Add($"<bpt id=\"{tagId++}\" ctype=\"subscript\">{{}}</bpt>");
                }
                if (runProperties.Color != null)
                {
                    tags.Add($"<bpt id=\"{tagId++}\" ctype=\"coloredtext\" color=\"{runProperties.Color.Val}\">{{}}</bpt>");
                }
                if (runProperties.Highlight != null)
                {
                    tags.Add($"<bpt id=\"{tagId++}\" ctype=\"highlightedtext\" color=\"{runProperties.Highlight.Val}\">{{}}</bpt>");
                }
                if (runProperties.Vanish != null)
                {
                    tags.Add($"<bpt id=\"{tagId++}\" ctype=\"hiddentext\">{{}}</bpt>");
                }
                if (imageFileName != null)
                {
                    tags.Add($"<bpt id=\"{tagId++}\" ctype=\"image\">{{}}</bpt>{imageFileName}");
                }
            }
            return tags;
        }
        public static string CreateClosingTag(string openingTag)
        {
            var match = Regex.Match(openingTag, @"<bpt id=""(\d+)""");
            if (match.Success)
            {
                return $"<ept id=\"{match.Groups[1].Value}\">{{}}</ept>";
            }
            return openingTag;
        }
}