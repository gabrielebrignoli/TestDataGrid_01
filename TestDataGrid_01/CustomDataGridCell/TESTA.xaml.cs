using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using TestDataGrid_01.Helper;

namespace TestDataGrid_01.CustomDataGridCell;

public partial class CellaRichTextBoxA : UserControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register("Testo_Cella", typeof(FlowDocument), typeof(CellaRichTextBoxA),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTextChanged));

    public CellaRichTextBoxA()
    {
        InitializeComponent();
        CellaTest.LostFocus += CellaEditing_LostFocus;
        RequestFocus += CellaRichTextBox_RequestFocus;
        CellaTest.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(CellaEditing_MouseLeftButtonDown),
            true);
        CellaTest.MouseEnter += CellaEditing_MouseEnter;
        CellaTest.MouseLeave += CellaEditing_MouseLeave;
        UpdateTextRequested += CellaRichTextBox_UpdateTextRequested;
    }

    public FlowDocument Testo_Cella
    {
        get => (FlowDocument)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public event EventHandler UpdateDatabaseRequested;

    public event EventHandler UpdateTextRequested;

    public void OnUpdateTextRequested()
    {
        UpdateTextRequested?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler RequestFocus;

    public virtual void OnRequestFocus()
    {
        RequestFocus?.Invoke(this, EventArgs.Empty);
    }

    private void CellaRichTextBox_UpdateTextRequested(object sender, EventArgs e)
    {
        Testo_Cella = CellaTest.Document;
        Console.WriteLine(Testo_Cella);
    }

    private void CellaEditing_MouseLeave(object sender, MouseEventArgs e)
    {
        CellaTest.Cursor = Cursors.Arrow;
    }

    private void CellaEditing_MouseEnter(object sender, MouseEventArgs e)
    {
        if (!CellaTest.IsFocused) CellaTest.Cursor = Cursors.Arrow;
    }

    private void CellaEditing_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        if (e.ClickCount == 1)
        {
            CellaTest.Cursor = Cursors.IBeam;
            CellaTest.Focus();
            SetFocusAndPlaceCursorAtStart(CellaTest);
        }
        else if (e.ClickCount == 2)
        {
            var textPointer = CellaTest.GetPositionFromPoint(e.GetPosition(CellaTest), false);
            if (textPointer == null) textPointer = CellaTest.GetPositionFromPoint(e.GetPosition(CellaTest), true);

            if (textPointer != null)
            {
                var wordRange = textPointer.GetWordRange();
                if (wordRange != null)
                {
                    // Check if the last character is a space
                    if (wordRange.Text.Length > 0 && wordRange.Text[wordRange.Text.Length - 1] == ' ')
                    {
                        // If it is, adjust the selection endpoint to exclude the space
                        var adjustedEnd = wordRange.End.GetPositionAtOffset(-1);
                        CellaTest.Selection.Select(wordRange.Start, adjustedEnd);
                    }
                    else
                    {
                        CellaTest.Selection.Select(wordRange.Start, wordRange.End);
                    }
                }
            }
        }
    }

    private void CellaRichTextBox_RequestFocus(object sender, EventArgs e)
    {
        SetFocusAndPlaceCursorAtStart(CellaTest);
    }

    private void SetFocusAndPlaceCursorAtStart(RichTextBox richTextBox)
    {
        richTextBox.Focus();
        richTextBox.CaretPosition = richTextBox.Document.ContentStart;
    }

    private void CellaEditing_LostFocus(object sender, RoutedEventArgs e)
    {
        Testo_Cella =CellaTest.Document;
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (CellaRichTextBoxA)d;

        if (e.NewValue != null)
        {
            control.CellaTest.Document.Blocks.Clear();
            var originalDocument = (FlowDocument)e.NewValue;
            var newDocument = new FlowDocument();
            foreach (var block in originalDocument.Blocks)
            {
                newDocument.Blocks.Add(block.Clone());
            }
            control.CellaTest.Document = newDocument;
        }
    }

    [Serializable]
    public class NonEditableRun : Run
    {
        public NonEditableRun(string text) : base(text)
        {
        }
    }
}