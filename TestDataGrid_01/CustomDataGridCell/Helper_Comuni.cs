using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml.Linq;
using TestDataGrid_01.CustomDataGridCell.SOURCE;
using TestDataGrid_01.CustomDataGridCell.TARGET;

namespace TestDataGrid_01.CustomDataGridCell;

public static class HelperComuni
{
    // METODO PER COPIARE LA SELEZIONE NELLA CLIPBOARD
    public static void CopiaInilinesSelezionatiInClipboard(List<Inline> inlines)
    {
        var copiedInlines = HelperComuni.DeepCopyInlineList(inlines);
        (var plainText, Dictionary<InlineUIContainer, string> inlineUIContainerTexts) =
            HelperComuni.ConvertInlinesToPlainText(copiedInlines);
        var rtfText = HelperComuni.ConvertInlinesToRtf(copiedInlines, inlineUIContainerTexts);
        var xamlText = HelperComuni.ConvertInlinesToXaml(copiedInlines);

        var dataObject = new DataObject();
        dataObject.SetData("CustomRichTextBoxFormat", xamlText);
        dataObject.SetText(plainText, TextDataFormat.UnicodeText);
        dataObject.SetText(rtfText, TextDataFormat.Rtf);


        Clipboard.SetDataObject(dataObject, true);
    }

    // METODO PER OTTENERE DATAGRIDCELL DA DATAGRID E CELLA
    public static DataGridCell OttieniDataGridCell(System.Windows.Controls.DataGrid dataGrid, DataGridCellInfo cellInfo)
    {
        var cellContent = cellInfo.Column.GetCellContent(cellInfo.Item);
        if (cellContent != null)
        {
            return (DataGridCell)cellContent.Parent;
        }
        return null;
    }
    
    // METODO PER ESTRARRE IL NUMERO DI RIGA IN BASE ALLA CELLA ATTIVA
    public static int? EstraiNumeroDiRiga(System.Windows.Controls.DataGrid dataGrid)
    {
        // Check if the current cell is not null
        if (dataGrid.CurrentCell != null)
        {
            // Get the item associated with the current cell
            var item = dataGrid.CurrentCell.Item;

            // Find the index of the item in the Items collection
            int index = dataGrid.Items.IndexOf(item);

            // Return the index
            return index;
        }

        // Return null if the current cell is null
        return null;
    }
    
    
    // METODO PER OTTENERE UN INDICE INTERO DI POSIZIONE DI UN INLINE
    public static int OttieneIndicePosizioneCursore(TextPointer caretPosition, Inline inline)
    {
        if (inline is Run run)
        {
            return run.ContentStart.GetOffsetToPosition(caretPosition);
        }

        throw new ArgumentException("Inline must be of type Run.");
    }
    
    
    // METODO PER ESTRARRE INDICI INIZIO E FINE SELEZIONE
    public static (int startIndex, int endIndex) GetSelectionIndices(Inline startInline, Inline endInline, TextPointer startPosition, TextPointer endPosition)
    {
        var startIndex = 0;
        if (startInline is Run startRun)
        {
            startIndex = startRun.ContentStart.GetOffsetToPosition(startPosition);
        }

        var endIndex = 0;
        if (endInline is Run endRun)
        {
            endIndex = endRun.ContentStart.GetOffsetToPosition(endPosition);
        }
        return (startIndex, endIndex);
    }
    
    // METODO PER AGGIUNGERE INLINE ALLA LISTA INLINESINSELEZIONE
    
    public static void AddToSelection(Inline inline, List<Inline> inlinesInSelezione, ref int conteggioInlines)
    {
        if (inline is Run || inline is InlineUIContainer && !inlinesInSelezione.Contains(inline))
        {
            inlinesInSelezione.Add(inline);
            conteggioInlines++;
        }
    }
    
    // NUOVO METODO PER DIVIDERE UN RUN IN DUE PARTI IN BASE A UN VALORE INTERO
    public static (Run FirstPart, Run SecondPart) SplitRunAtPosition(Inline inline, int position)
    {
        if (!(inline is Run runToSplit))
        {
            throw new ArgumentException("Inline must be of type Run.", nameof(inline));
        }
        
        var textlenght = runToSplit.Text.Length;

        if (position < 0 || position > textlenght)
        {
            return (null, null);
            throw new ArgumentOutOfRangeException(nameof(position),
                "Position must be within the range of the Run's text.");
        }

        // Split the Run's text at the specified position.
        string firstPartText = runToSplit.Text.Substring(0, position);
        string secondPartText = runToSplit.Text.Substring(position);
        Console.WriteLine($"PRIMO E SECONDO RUN TESTO = {firstPartText} - {secondPartText}");

        // Create two new Runs with the split text using the CloneEmptyRun method.
        Run firstPart = CloneEmptyRun(runToSplit);
        firstPart.Text = firstPartText;
        string xamlfirstPart = XamlWriter.Save(firstPart);
        Console.WriteLine($"SPLITTED TEXT PRE = {xamlfirstPart}");

        Run secondPart = CloneEmptyRun(runToSplit);
        secondPart.Text = secondPartText;
        string xamlsecondPart = XamlWriter.Save(secondPart);
        Console.WriteLine($"SPLITTED TEXT POST = {xamlsecondPart}");

        return (firstPart, secondPart);
    }
    private static Run CloneEmptyRun(Run source)
    {
        // Convert the source Run to XAML, ensuring that only the Run itself is included.
        string xaml = XamlWriter.Save(source);

        // Load the XAML into an XElement.
        XElement element = XElement.Parse(xaml);

        // Remove the text content of the Run.
        element.Value = "";

        // Convert the XAML back to a Run.
        Run clone = (Run)XamlReader.Parse(element.ToString());

        return clone;
    }
    
    //  METODO PER VERIFICARE L'EFFETTIVO CONTENUTO DI UNA SELEZIONE
    public static string EstraiTestoDaListInlines(List<Inline> inlines)
    {
        StringBuilder result = new StringBuilder();
    
        foreach (var inline in inlines)
        {
            if (inline is Run run)
            {
                result.Append(run.Text);
            }
            else if (inline is InlineUIContainer uiContainer)
            {
                if (uiContainer.Child is TextBlock textBlock)
                {
                    result.Append(textBlock.Text);
                }
            }
        }
        Console.WriteLine("------------------------------------------------------");
        Console.WriteLine($"Testo selezione = {result.ToString()}");
        Console.WriteLine("------------------------------------------------------");
        return result.ToString();
    }

    // METODO PER ESTRARRE L'INLINE AL CURSORE
    public static Inline OttieniInlineAlCursore(TextPointer cursor)
    {
        if (cursor == null)
        {
            throw new ArgumentNullException(nameof(cursor));
        }
    
        if (cursor.Parent is Inline inline)
        {
            return inline;
        }
        return null;
    }
    
    // METODI DI CONVERSIONE
    #region METODI DI CONVERSIONE
    public static (string plainText, Dictionary<InlineUIContainer, string> inlineUIContainerTexts) ConvertInlinesToPlainText(List<Inline> inlines)
    {
        var plainText = new StringBuilder();
        var inlineUIContainerTexts = new Dictionary<InlineUIContainer, string>();

        foreach (Inline inline in inlines)
        {
            if (inline is Run run)
            {
                plainText.Append(run.Text);
            }
            else if (inline is LineBreak)
            {
                plainText.AppendLine();
            }
            else if (inline is InlineUIContainer inlineUIContainer && inlineUIContainer.Child is Border border && border.Child is TextBlock textBlock)
            {
                string text = "|" + textBlock.Text + "|";
                inlineUIContainerTexts[inlineUIContainer] = text;
                plainText.Append(text);
            }
        }
        return (plainText.ToString(), inlineUIContainerTexts);
    }
    public static string ConvertInlinesToRtf(List<Inline> inlines, Dictionary<InlineUIContainer, string> inlineUIContainerTexts)
    {
        var flowDocument = new Span();
        foreach (Inline inline in inlines)
        {
            if (inline is InlineUIContainer inlineUIContainer && inlineUIContainerTexts.ContainsKey(inlineUIContainer))
            {
                string text = inlineUIContainerTexts[inlineUIContainer];
                flowDocument.Inlines.Add(new Run(text));
            }
            else
            {
                flowDocument.Inlines.Add(inline);
            }
        }
        var textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
        using var memoryStream = new MemoryStream();
        textRange.Save(memoryStream, DataFormats.Rtf);
        memoryStream.Seek(0, SeekOrigin.Begin);

        using var streamReader = new StreamReader(memoryStream);
        return streamReader.ReadToEnd();
    }
        
    public static string ConvertInlinesToXaml(List<Inline> inlines)
    {
        Span span = new Span();
        foreach (Inline inline in inlines)
        {
            span.Inlines.Add(inline);
        }
        return XamlWriter.Save(span);
    }
    #endregion
    
    
    // METODO PER CLONARE UN INLINE
    public static T Clone<T>(this T source) where T : Inline
    {
        if (source == null)
            return null;

        var xaml = XamlWriter.Save(source);
        T result = (T)XamlReader.Parse(xaml);
        return result;
    }
    
    // METODO PER CLONARE UNA LISTA DI INLINE
    public static List<Inline> DeepCopyInlineList(List<Inline> inlines)
    {
        List<Inline> copiedInlines = new List<Inline>();
        foreach (Inline inline in inlines)
        {
            Inline copiedInline = inline.Clone();
            if (copiedInline != null)
            {
                copiedInlines.Add(copiedInline);
            }
        }
        return copiedInlines;
    }
    
    // Metodo per il parsing di un elenco di Inline da un testo XAML
    public static List<Inline> ParseInlineListFromXaml(string xamlText)
    {
        if (string.IsNullOrEmpty(xamlText))
        {
            return null;
        }
        try
        {
            // Wrap the XAML content in a Span element to ensure it can be parsed correctly
            string spanXaml = xamlText;

            // Parse the XAML content
            Span span = XamlReader.Parse(spanXaml) as Span;

            if (span == null)
            {
                return null;
            }
            // Extract the inlines from the parsed Span element
            List<Inline> inlines = span.Inlines.ToList();
            // Detach the inlines from the Span to avoid parent-child relationship issues when inserting them into the target RichTextBox
            span.Inlines.Clear();
            return inlines;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error parsing XAML content: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }
    
    // Metodo generale per impostare il focus su una cella della Datagrid
    public static void ImpostaFocusSuCella(System.Windows.Controls.DataGrid nomeDatagrid, int indiceColonna)
    {
        var contenuto =  nomeDatagrid.Columns[indiceColonna].
            GetCellContent(nomeDatagrid.SelectedItem);
        var NomeColonna = indiceColonna == 1 ? "CellaSource" : "CellaTarget";

        if (contenuto is ContentPresenter contentPresenter)
        {
            var cellaFocalizzata = 
                contentPresenter.ContentTemplate.FindName(NomeColonna, contentPresenter) as RichTextBox;
            
            if (cellaFocalizzata != null && cellaFocalizzata is CellaRichTextBoxSource cellaRichTextBoxSource)
            {
               cellaRichTextBoxSource.OnRequestFocus(); 
            }
            else if (cellaFocalizzata != null && cellaFocalizzata is CellaRichTextBoxTarget cellaRichTextBoxTarget)
            {
                cellaRichTextBoxTarget.OnRequestFocus();
            }
        }
    }
    public static void ClearSelectionSource(CellaRichTextBoxSource cellaSource)
    {
        cellaSource.Selection.Select(cellaSource.Document.ContentStart, cellaSource.Document.ContentStart);
    }
    public static void ClearSelectionTarget(CellaRichTextBoxTarget cellaSource)
    {
        cellaSource.Selection.Select(cellaSource.Document.ContentStart, cellaSource.Document.ContentStart);
    }
    
    // METODO PER STAMPARE CONTENUTO INLINES
    public static void StampaInline(List<Inline> _inlines)
    {
        foreach (var VARIABLE in _inlines)
        {
            using (var stringWriter = new StringWriter())
            {
                XamlWriter.Save(VARIABLE, stringWriter); 
                string xamlString = stringWriter.ToString(); Console.WriteLine(xamlString); 
            }
        }
    }
    
    // METODO PER STAMPARE CONTENUTO DI UN SINGOLO INLINE
    public static void StampaSingoloInline(Inline _inline)
    {
        using (var stringWriter = new StringWriter())
        {
            XamlWriter.Save(_inline, stringWriter); 
            string xamlString = stringWriter.ToString(); Console.WriteLine(xamlString); 
        }
    }
    // METODO PER STAMPARE CONTENUTO DI UN SINGOLO PARAGRAFO
    public static void StampaParagrafo(Paragraph _inline)
    {
        using (var stringWriter = new StringWriter())
        {
            XamlWriter.Save(_inline, stringWriter); 
            string xamlString = stringWriter.ToString(); Console.WriteLine(xamlString); 
        }
    }
    
    //METODO PER INSERIRE TESTO DA SOURCE A TARGET
    public static void InserisciTestoSourceToTarget(CellaRichTextBoxSource cellaRichTextBoxSource, CellaRichTextBoxTarget cellaRichTextBoxTarget, System.Windows.Controls.DataGridCell DocumentDataGridCell)
    {
        // TESTO SELEZIONATO NEL SOURCE
        var InlineSelezionatiSource = Variabili.InlineSelezioneCorrente_SOURCE;
        Console.WriteLine("------------------------------------XXX------------------------------------------");
        StampaInline(InlineSelezionatiSource);
        Console.WriteLine("------------------------------------XXX------------------------------------------");
        // POSIZIONE DEL CURSORE NELLA CASELLA TARGET
        TextPointer PosizioneCursoreCellaTarget;
            
        // SONO NELLA CORRISPONDENTE CASELLA TARGET (STESSA RIGA) E NON HO SELEZIONATO NULLA NELLA CASELLA TARGET
        if (DocumentDataGridCell.Column.DisplayIndex == 2)
        {
            if (cellaRichTextBoxTarget.Selection.IsEmpty)
            {
                if (cellaRichTextBoxTarget != null && cellaRichTextBoxTarget.Document.Blocks.FirstBlock is  Paragraph paragrafoAttuale && paragrafoAttuale.Inlines.Count > 0)
                {
                    PosizioneCursoreCellaTarget = cellaRichTextBoxTarget.CaretPosition;
                    var inlineAlCursore = OttieniInlineAlCursore(PosizioneCursoreCellaTarget);
                
                    var indicePosizioneCursore = OttieneIndicePosizioneCursore(PosizioneCursoreCellaTarget, inlineAlCursore);
                    var (primoRun, ultimoRun) = SplitRunAtPosition(inlineAlCursore, indicePosizioneCursore);
                
                    paragrafoAttuale.Inlines.InsertAfter(inlineAlCursore, primoRun);
                    paragrafoAttuale.Inlines.InsertAfter(primoRun, ultimoRun);
                    var tempInline = primoRun as Inline;
                    var listaClonata = DeepCopyInlineList(Variabili.InlineSelezioneCorrente_SOURCE);
                    foreach (var VARIABLE in listaClonata)
                    {
                        paragrafoAttuale.Inlines.InsertAfter(primoRun, VARIABLE);
                        tempInline = VARIABLE;
                    }
                }
                else
                {
                    paragrafoAttuale = cellaRichTextBoxTarget.Document.Blocks.FirstBlock as Paragraph;
                    var listaClonata = DeepCopyInlineList(Variabili.InlineSelezioneCorrente_SOURCE);
                    foreach (var inline in listaClonata)
                    {
                        paragrafoAttuale.Inlines.Add(inline);
                    }
                    PosizioneCursoreCellaTarget = Variabili.UltimaPosizioneCursoreTarget;
                }
            }
            else if (!cellaRichTextBoxTarget.Selection.IsEmpty)
            {
                HelperTarget.CancellaSelezioneEmodificaParagrafo(InlineSelezionatiSource, cellaRichTextBoxTarget);
            }
        }
    }
}