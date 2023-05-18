using System;
using System.Collections;
using System.Windows.Documents;

namespace TestDataGrid_01.CustomDataGridCell;

public class Helper_TARGET
{
    // Metodo per dividere un Run in due parti in base a dove è posizionato il cursore
    public static (int, Run) SplitRunAlCursore_SOURCE(CellaRichTextBox_SOURCE richTextBox, TextPointer cursorPosition)
    {
            if (cursorPosition == null)
            {
                throw new ArgumentNullException(nameof(cursorPosition));
            }

            // Get the Run at the current cursor position.
            var currentRun = cursorPosition.Parent as Run;

            if (currentRun == null)
            {
                throw new InvalidOperationException("The cursor is not currently within a Run element.");
            }

            // Find the index of the cursor within the current Run.
            var cursorIndex = cursorPosition.GetTextRunLength(LogicalDirection.Backward);

            // Split the Run's text into two parts.
            var textBeforeCursor = currentRun.Text[..cursorIndex];
            var textAfterCursor = currentRun.Text.Substring(cursorIndex);

            // Create two new Run elements with the split text.
            var runBeforeCursor = new Run(textBeforeCursor);
            var runAfterCursor = new Run(textAfterCursor);
            
            // Copy the formatting properties from the original Run to the two new Runs.
            Helper_Comuni.CopyProperties(currentRun, runBeforeCursor);
            Helper_Comuni.CopyProperties(currentRun, runAfterCursor);
            
            // Replace the original Run with the two new Runs.
            var parentParagraph = currentRun.Parent as Paragraph;
            if (parentParagraph == null)
            {
                throw new InvalidOperationException("The current Run is not within a Paragraph element.");
            }
            richTextBox.BeginChangeWrapper();
            var runIndex = ((IList)parentParagraph.Inlines).IndexOf(currentRun);
            parentParagraph.Inlines.Remove(currentRun);
            ((IList)parentParagraph.Inlines).Insert(runIndex, runBeforeCursor);
            //((IList)parentParagraph.Inlines).Insert(runIndex + 1, runAfterCursor);
            richTextBox.EndChangeWrapper();
            
            return (runIndex, runBeforeCursor);
    }
}