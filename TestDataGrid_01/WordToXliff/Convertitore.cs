using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using Paragraph = System.Windows.Documents.Paragraph;
using Run = System.Windows.Documents.Run;

namespace TestDataGrid_01.WordToXliff;

public static class Visualizzatore_XLIFF
{
    public static string ConvertToXamlString(string content)
    {
        string xamlString;
        //Check if string content begins with <FlowDocument>
        if (content.Contains("<FlowDocument"))
        {
            xamlString = content;
        }
        else
        {
            var paragraph = ConvertStringToParagraph(content);
            var  s_xamlString = ParagraphToFlowDocument(paragraph);
            xamlString = XamlWriter.Save(s_xamlString);
        }
        return xamlString;
    }
    
    public static Paragraph ConvertStringToParagraph(string content)
    {
        var openTags = new Dictionary<int, (string Tag, string Value)>();
        var paragraph = new Paragraph();
        if (string.IsNullOrEmpty(content))
        {
            return paragraph;
        }
        else
        {
            var matches = Regex.Matches(content,
                @"(<bpt id=""(?<bpt_id>\d+)"" ctype=""(?<tag_attribute>.*?)""(?:\s(?<attribute_value>.*?)=""(?<value>.*?)"")?>\{\}</bpt>)|(<ept id=""(?<ept_id>\d+)"">\{\}</ept>)|(?<text>(?:[^<>]|&lt;|&gt;)+)");
            foreach (Match match in matches)
            {
                if (match.Groups["bpt_id"].Success) // opening tag
                {
                    var tagId = int.Parse(match.Groups["bpt_id"].Value);
                    var formattingAttribute = match.Groups["tag_attribute"].Value;
                    var tagValue = match.Groups["value"].Success ? match.Groups["value"].Value : null;
                    openTags[tagId] = (formattingAttribute, tagValue);
                }
                else if (match.Groups["ept_id"].Success) // closing tag
                {
                    var tagId = int.Parse(match.Groups["ept_id"].Value);
                    if (openTags.ContainsKey(tagId))
                    {
                        openTags.Remove(tagId);
                    }
                    else
                    {
                        throw new InvalidOperationException("Mismatched closing tag encountered.");
                    }
                }
                else // plain text
                {
                    var run = new Run();

                    var text = match.Groups["text"].Value;

                    Span span = new Span(run);

                    run.Text = WebUtility.HtmlDecode(text);

                    foreach (var (_, (tag, value)) in openTags)
                    {
                        switch (tag)
                        {
                            case "bold":
                                run.FontWeight = FontWeights.Bold;
                                break;
                            case "italic":
                                run.FontStyle = FontStyles.Italic;
                                break;
                            case "underline":
                                run.TextDecorations = TextDecorations.Underline;
                                break;
                            case "underlined":
                                run.TextDecorations = TextDecorations.Underline;
                                break;
                            case "fontsize":
                                if (double.TryParse(value, out double fontSize))
                                {
                                    run.FontSize = fontSize / 2; // The font size in DOCX is measured in half-points
                                }

                                break;
                            case "fonttype":
                                run.FontFamily = new FontFamily(value);
                                break;
                            case "superscript":
                                run.BaselineAlignment = BaselineAlignment.TextTop;
                                run.FontSize *= 0.65;
                                break;
                            case "x-sup":
                                run.BaselineAlignment = BaselineAlignment.TextTop;
                                run.FontSize *= 0.65;
                                break;
                            case "subscript":
                                run.BaselineAlignment = BaselineAlignment.Subscript;
                                run.FontSize *= 0.65;
                                break;
                            case "x-sub":
                                run.BaselineAlignment = BaselineAlignment.Subscript;
                                run.FontSize *= 0.65;
                                break;
                            case "coloredtext":
                                run.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString($"#{value}");
                                break;
                            case "highlightedtext":
                                run.Background =
                                    (SolidColorBrush)new BrushConverter().ConvertFromString(
                                        WmlToXamlColorConverter.Convert(value));
                                break;
                            case "hiddentext":
                                run.Foreground = new SolidColorBrush(Colors.LightGray);
                                break;
                            case "image":
                                var imageNameRegex = new Regex(@"[^\s]*_image_\d+\.[^<]*");
                                var imageNameMatch = imageNameRegex.Match(text);

                                if (imageNameMatch.Success)
                                {
                                    int imageNumber = ExtractImageNumber(imageNameMatch.Value);
                                    run.Text = run.Text.Replace(imageNameMatch.Value, "");

                                    var imagePlaceholder = Tag_helper_methods.CreateImagePlaceholder(imageNumber);
                                    paragraph.Inlines.Add(new Run());
                                    paragraph.Inlines.Add(imagePlaceholder);
                                    paragraph.Inlines.Add(new Run());
                                }

                                break;
                            default:
                                throw new InvalidOperationException($"Unknown formatting attribute: {tag}");
                        }
                    }

                    paragraph.Inlines.Add(run);
                }
            }
        }

        return paragraph;
    }
    
    public static FlowDocument ParagraphToFlowDocument(Paragraph paragraph)
    {
        FlowDocument flowDocument = new FlowDocument();
        flowDocument.Blocks.Add(paragraph);
        return flowDocument;
    }

    private static class WmlToXamlColorConverter
    {
        private static readonly Dictionary<string, string> WmlToXamlColors = new Dictionary<string, string>
        {
            {"auto", "Transparent"},
            {"black", "Black"},
            {"blue", "Blue"},
            {"cyan", "Cyan"},
            {"green", "Green"},
            {"magenta", "Magenta"},
            {"red", "Red"},
            {"yellow", "Yellow"},
            {"white", "White"},
            {"darkBlue", "DarkBlue"},
            {"darkCyan", "DarkCyan"},
            {"darkGreen", "DarkGreen"},
            {"darkMagenta", "DarkMagenta"},
            {"darkRed", "DarkRed"},
            {"darkYellow", "DarkGoldenrod"},
            {"darkGray", "DarkGray"},
            {"lightGray", "LightGray"}
        };
        public static string Convert(string wmlColor)
        {
            return WmlToXamlColors.TryGetValue(wmlColor, out string xamlColor) ? xamlColor : wmlColor;
        }
    }

    private static int ExtractImageNumber(string imageName)
    {
        if (string.IsNullOrEmpty(imageName))
        {
            throw new ArgumentNullException(nameof(imageName));
        }

        var imageNumberMatch = Regex.Match(imageName, @"_image_(\d+)\.");
        if (imageNumberMatch.Success)
        {
            return int.Parse(imageNumberMatch.Groups[1].Value);
        }

        throw new InvalidOperationException("Image number could not be extracted.");
    }
}