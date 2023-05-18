using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using TestDataGrid_01.Helper;

namespace TestDataGrid_01.CustomDataGridCell;

public partial class CellaRichTextBox_SOURCE : UserControl
{
    public TextSelection Selection
    {
        get => CellaEditing_Source.Selection;
    }
    public FlowDocument Document
    {
        get => CellaEditing_Source.Document;
    }
    public void BeginChangeWrapper()
    {
        CellaEditing_Source.BeginChange();
    }
    public void EndChangeWrapper()
    {
        CellaEditing_Source.EndChange();
    }
    
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register("Testo_Cella", typeof(string), typeof(CellaRichTextBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTextChanged));

    public CellaRichTextBox_SOURCE()
    {
        InitializeComponent();
        CellaEditing_Source.LostFocus += CellaEditingLostFocus;
        RequestFocus += CellaRichTextBox_RequestFocus;
        CellaEditing_Source.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(CellaEditing_MouseLeftButtonDown),
            true);
        CellaEditing_Source.MouseEnter += CellaEditingMouseEnter;
        CellaEditing_Source.MouseLeave += CellaEditingMouseLeave;
        UpdateTextRequested += CellaRichTextBox_UpdateTextRequested;
        
        CellaEditing_Source.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, OnCopyCommand));
        CellaEditing_Source.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, OnPasteCommand));
        //CellaEditing.CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, OnCutCommand));
    }

    private string teststring;

    public void OnCopyCommand(object sender, ExecutedRoutedEventArgs e)
    {
        var richTextBox = sender as CellaRichTextBox_SOURCE;
        if (richTextBox != null)
        {
            CellaSOURCE_CopyPasteCut.SelezionaInlinesDaCopiare(richTextBox);
        }
    }
    
    public void OnPasteCommand(object sender, ExecutedRoutedEventArgs e)
    {
        var richTextBox = sender as CellaRichTextBox_SOURCE;
        if (richTextBox != null)
        {
            CellaSOURCE_CopyPasteCut.PasteInlineDaClipboard(richTextBox);
        }
    }

public string Testo_Cella
{
    get => (string)GetValue(TextProperty);
    set => SetValue(TextProperty, value);
}

public TextPointer CaretPosition { get; set; }

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
        Testo_Cella = XamlWriter.Save(CellaEditing_Source.Document);
        Console.WriteLine(Testo_Cella);
    }
    private void CellaEditingMouseLeave(object sender, MouseEventArgs e)
    {
        CellaEditing_Source.Cursor = Cursors.Arrow;
    }
    private void CellaEditingMouseEnter(object sender, MouseEventArgs e)
    {
        if (!CellaEditing_Source.IsFocused) CellaEditing_Source.Cursor = Cursors.Arrow;
    }
    
    private void CellaEditing_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        if (e.ClickCount == 1)
        {
            CellaEditing_Source.Cursor = Cursors.IBeam;
            CellaEditing_Source.Focus();
            SetFocusAndPlaceCursorAtStart(CellaEditing_Source);
        }
        else if (e.ClickCount == 2)
        {
            var textPointer = CellaEditing_Source.GetPositionFromPoint(e.GetPosition(CellaEditing_Source), false);
            if (textPointer == null) 
                textPointer = CellaEditing_Source.GetPositionFromPoint(e.GetPosition(CellaEditing_Source), true); 
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
                            CellaEditing_Source.Selection.Select(wordRange.Start, adjustedEnd);
                        }
                        else
                        {
                            CellaEditing_Source.Selection.Select(wordRange.Start, wordRange.End);
                        }
                    }
                }
            }
        }
   
    private void CellaRichTextBox_RequestFocus(object sender, EventArgs e)
    {
        SetFocusAndPlaceCursorAtStart(CellaEditing_Source);
    }
    private void SetFocusAndPlaceCursorAtStart(RichTextBox richTextBox)
    {
        richTextBox.Focus();
        //richTextBox.CaretPosition = richTextBox.Document.ContentStart;
    }
    private void CellaEditingLostFocus(object sender, RoutedEventArgs e)
    {
        Testo_Cella = XamlWriter.Save(CellaEditing_Source.Document);
    }
    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (CellaRichTextBox)d;
        var xaml = (string)e.NewValue;
        if (!string.IsNullOrEmpty(xaml))
        {
            var newContent = (FlowDocument)XamlReader.Parse(xaml);
            control.CellaEditing.Document.Blocks.Clear();
            foreach (var block in newContent.Blocks.ToList())
            {
                newContent.Blocks.Remove(block);
                control.CellaEditing.Document.Blocks.Add(block);
            }
        }
    }
    public void Paste()
    {
        CellaEditing_Source.Paste();
    }
}