using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace TestDataGrid_01.Helper;

public static class RegoleSegmentazione
{
    private static List<string> segmentationRules = new List<string>
    {
        @"\.", @"\!", @"\?", "(?<=wherein)\\s"
    };

    private static List<string> exclusionRules = new List<string>
    {
        "e\\.g\\.", "i\\.e\\.", "\\s[A-Z]\\.\\s", "[0-9].[0-9]", "^[A-Z][.]\\s"
    };
    private static bool ShouldSplit(string text)
    {
        return segmentationRules.Any(rule => Regex.IsMatch(text, rule));
    }

    private static bool ShouldExcludeSplit(string text)
    {
        return exclusionRules.Any(rule => Regex.IsMatch(text, rule));
    }
    
    public static List<List<OpenXmlElement>> SplitParagraph(Paragraph paragraph)
    {
        var splitRuns = new List<List<OpenXmlElement>>();
        var currentSegment = new List<OpenXmlElement>();
        foreach (var element in paragraph.ChildElements)
        {
            if (element is Run run)
            {
                var runText = run.InnerText;
                bool isSplit = false;

                foreach (var rule in segmentationRules)
                {
                    if (Regex.IsMatch(runText, rule) && !exclusionRules.Any(exclusionRule => Regex.IsMatch(runText, exclusionRule)))
                    {
                        isSplit = true;

                        // Implement splitting logic based on your custom rules
                        var splitPoints = Regex.Matches(runText, rule).OfType<Match>().Select(m => m.Index).Distinct().OrderBy(x => x).ToList();
                        int lastIndex = 0;

                        foreach (int splitPoint in splitPoints)
                        {
                            string splitText = runText.Substring(lastIndex, splitPoint - lastIndex + 1);
                            lastIndex = splitPoint + 1;

                            // Add the split text as a new Run to the current segment
                            var newRun = (Run)run.CloneNode(true);
                            newRun.RemoveAllChildren<Text>();
                            newRun.AppendChild(new Text(splitText));
                            currentSegment.Add(newRun);

                            // Add the current segment to splitRuns and start a new segment
                            splitRuns.Add(currentSegment);
                            currentSegment = new List<OpenXmlElement>();
                        }
                        // Add the remaining text as a new Run to the current segment
                        if (lastIndex < runText.Length)
                        {
                            string remainingText = runText.Substring(lastIndex);
                            var newRun = (Run)run.CloneNode(true);
                            newRun.RemoveAllChildren<Text>();
                            newRun.AppendChild(new Text(remainingText));
                            currentSegment.Add(newRun);
                        }
                        break;
                    }
                }
                if (!isSplit)
                {
                    currentSegment.Add(run.CloneNode(true));
                }
            }
            else
            {
                currentSegment.Add(element.CloneNode(true));
            }
        }
        if (currentSegment.Count > 0)
        {
            splitRuns.Add(currentSegment);
        }
        return splitRuns;
    }
}