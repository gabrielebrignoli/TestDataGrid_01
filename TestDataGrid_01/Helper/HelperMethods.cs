using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace TestDataGrid_01.Helper;

public static class HelperMethods
{
   
}

public static class TextPointerExtensions
{
    public static TextRange GetWordRange(this TextPointer position)
    {
        TextRange wordRange = null;
        if (position != null)
        {
            var start = position.GetPositionAtOffset(0, LogicalDirection.Backward);
            var end = position.GetPositionAtOffset(0, LogicalDirection.Forward);

            while (start != null && !char.IsWhiteSpace((char)start.GetPointerContext(LogicalDirection.Backward)))
                start = start.GetNextInsertionPosition(LogicalDirection.Backward);

            while (end != null && !char.IsWhiteSpace((char)end.GetPointerContext(LogicalDirection.Forward)))
                end = end.GetNextInsertionPosition(LogicalDirection.Forward);

            if (start != null && end != null) wordRange = new TextRange(start, end);
        }
        return wordRange;
    }
}