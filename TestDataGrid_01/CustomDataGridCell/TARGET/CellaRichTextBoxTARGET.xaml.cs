using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using TestDataGrid_01.Helper;

namespace TestDataGrid_01.CustomDataGridCell.TARGET;

public sealed partial class CellaRichTextBoxTarget : RichTextBox
{
    public static int UltimaInizioSelezione_INT { get; set; }
    public static int UltimaFineSelezione_INT  { get; set; }
    public static TextPointer UltimoInizioSelezione { get; set; }
    public static TextPointer UltimaFineSelezione { get; set; }
    public static int NumeroRigaPrecedenteTarget { get; set; } = -1;
    
    public void BeginChangeWrapper_S()
    {
        BeginChange();
    }
    public void EndChangeWrappe_S()
    {
        EndChange();
    }
    
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(TestoCella), typeof(string), typeof(CellaRichTextBoxTarget),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTextChanged));
    
    public CellaRichTextBoxTarget()
    {
        InitializeComponent();
        LostFocus += CellaEditingLostFocus;
        //GotFocus += CellaEditingGotFocus;
        RequestFocus += CellaRichTextBox_RequestFocus;
        AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(CellaEditing_MouseLeftButtonDown),
            true);
        MouseEnter += CellaEditingMouseEnter;
        MouseLeave += CellaEditingMouseLeave;
        UpdateTextRequested += CellaRichTextBox_UpdateTextRequested;
        SelectionChanged += CellaRichTextBox_SelectionChanged;
        //PreviewLostKeyboardFocus += CellaEditing_Target_PreviewLostKeyboardFocus;
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, OnCopyCommand));
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, OnPasteCommand));
        //CellaEditing.CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, OnCutCommand));
    }

    private static void CellaRichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
        if (sender is CellaRichTextBoxTarget richTextBoxTarget) SelectionChangeTarget.SelezioneTargetCorrente(richTextBoxTarget);
    }

    public static void OnCopyCommand(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is CellaRichTextBoxTarget richTextBoxTarget)
        {
            HelperComuni.CopiaInilinesSelezionatiInClipboard(Variabili.InlineSelezioneCorrente_TARGET);
        }
    }
    public static void OnPasteCommand(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is CellaRichTextBoxTarget richTextBoxTarget)
        {
            CellaTARGET_CopyPasteCut.PasteInlineDaClipboard(richTextBoxTarget);
        }
    }
public string TestoCella
{
    get => (string)GetValue(TextProperty);
    set => SetValue(TextProperty, value);
}

public event EventHandler? UpdateDatabaseRequested;

    public event EventHandler UpdateTextRequested;

    public void OnUpdateTextRequested()
    {
        UpdateTextRequested?.Invoke(this, EventArgs.Empty);
    }

    #region Regione GET AND LOST FOCUS

    public event EventHandler RequestFocus;

    // Metodo che viene chiamato quando la casella ottiene il FOCUS
    public void OnRequestFocus()
    {   
        RequestFocus?.Invoke(this, EventArgs.Empty);
        if (UltimaInizioSelezione_INT != 0 && UltimaFineSelezione_INT != 0 && NumeroRigaPrecedenteTarget != Percorsi.NumeroRigaCorrente)
        {
            //Variabili.InlineSelezioneCorrente_TARGET = null;
            UltimaInizioSelezione_INT = -1;
            UltimaFineSelezione_INT = -1;

        }
        if (UltimaInizioSelezione_INT != -1 && UltimaFineSelezione_INT != -1 && UltimaInizioSelezione_INT != UltimaFineSelezione_INT)
        {
            if (UltimoInizioSelezione != null && UltimaFineSelezione != null)
            {
                Selection.Select(UltimoInizioSelezione, UltimaFineSelezione);
            }
        }
    }

    // Metodo che viene chiamato quando la casella --PERDE-- il FOCUS
    private void CellaEditingLostFocus(object sender, RoutedEventArgs e)
    {
        UltimaInizioSelezione_INT = Variabili.InizioSelezione_INT_TARGET;
        UltimaFineSelezione_INT = Variabili.FineSelezione_INT_TARGET;
        UltimoInizioSelezione = Variabili.InizioSelezione_TARGET;
        UltimaFineSelezione = Variabili.FineSelezione_TARGET;
        
        
        //Print TexpPointer position as check as Integer
        Console.WriteLine("Inizio-Fine Selezione LOST FOCUS: " + UltimaInizioSelezione_INT + "- " +
                          UltimaFineSelezione_INT);
        if (UltimaInizioSelezione_INT != UltimaFineSelezione_INT)
        {
            var UltimaSelezione = Variabili.InlineSelezioneCorrente_TARGET;
        }
        TestoCella = XamlWriter.Save(Document);
        Console.WriteLine($"LOST FOCUS: {TestoCella}");
        NumeroRigaPrecedenteTarget = Percorsi.NumeroRiga;
        Variabili.LastSelectedInstanceTarget = this;
        
        // ULTIMA POSIZIONE DEL CURSORE QUANDO LASCIO LA CELLA
        Variabili.UltimaPosizioneCursoreTarget = CaretPosition;
        e.Handled = true;
    }
    #endregion
    
    private void CellaRichTextBox_UpdateTextRequested(object sender, EventArgs e)
    {
        TestoCella = XamlWriter.Save(Document);
        Console.WriteLine(TestoCella);
    }
    private void CellaEditingMouseLeave(object sender, MouseEventArgs e)
    {
        Cursor = Cursors.Arrow;
    }
    private void CellaEditingMouseEnter(object sender, MouseEventArgs e)
    {
        if (!IsFocused) Cursor = Cursors.Arrow;
    }
   
    private void CellaEditing_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        if (e.ClickCount == 1)
        {
            Cursor = Cursors.IBeam;
            Focus();
            SetFocusAndPlaceCursorAtStart();
        }
        else if (e.ClickCount == 2)
        {
            var textPointer = GetPositionFromPoint(e.GetPosition(CellaEditing_Target), false);
            if (textPointer == null) 
                textPointer = GetPositionFromPoint(e.GetPosition(CellaEditing_Target), true); 
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
                        Selection.Select(wordRange.Start, adjustedEnd);
                    }
                    else
                    {
                        Selection.Select(wordRange.Start, wordRange.End);
                    }
                }
            }
        }
    }
   
    private void CellaRichTextBox_RequestFocus(object sender, EventArgs e)
    {
        SetFocusAndPlaceCursorAtStart();
    }
    private void SetFocusAndPlaceCursorAtStart()
    {
        Focus();
    }
    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (CellaRichTextBoxTarget)d;
        var xaml = (string)e.NewValue;
        Console.WriteLine("XAML: " + xaml);
        if (!string.IsNullOrEmpty(xaml))
        {
            var newContent = (FlowDocument)XamlReader.Parse(xaml);
            control.Document.Blocks.Clear();
            foreach (var block in newContent.Blocks.ToList())
            {
                newContent.Blocks.Remove(block);
                control.Document.Blocks.Add(block);
            }
        }
    }
}

