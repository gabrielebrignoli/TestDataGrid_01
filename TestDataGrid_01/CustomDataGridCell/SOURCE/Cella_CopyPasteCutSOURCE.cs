using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using TestDataGrid_01.Helper;

namespace TestDataGrid_01.CustomDataGridCell;

public static class CellaSOURCE_CopyPasteCut
{
    #region Classe Variabili

    public static class Variabili_CopyPaste_SOURCE
    {
        public static int  NumeroInlinesSelezionatiS  { get; set; }
        public static int PosizioneInizioSelezioneS { get; set; }
        public static int  PosizioneFineSelezioneS { get; set; }
        public static TextPointer PuntatoreInizioSelezioneS { get; set; }
        public static TextPointer PuntatoreFineSelezioneS { get; set; }
        public static List<Inline> InlinesSelezionatiS { get; set; }
    }

    #endregion
    
    
    #region Metodi conversione contenuto Clipboard
    
    private static (string plainText, Dictionary<InlineUIContainer, string> inlineUIContainerTexts) ConvertInlinesToPlainText(List<Inline> inlines)
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
        
        private static string ConvertInlinesToRtf(List<Inline> inlines, Dictionary<InlineUIContainer, string> inlineUIContainerTexts)
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
        
        private static string ConvertInlinesToXaml(List<Inline> inlines)
        {
            Span span = new Span();
            foreach (Inline inline in inlines)
            {
                span.Inlines.Add(inline);
            }
            return XamlWriter.Save(span);
        }
    #endregion
    
    #region Medoti Helper Copia/Incolla

    public static (int, List<Inline>, int, int, TextPointer, TextPointer, int, int) EstraiSelezione(CellaRichTextBox cellaRichTextBox)
    {
        if (!cellaRichTextBox.Selection.IsEmpty)
        {
            var numeroInlinesSelezionati = Variabili_CopyPaste.NumeroInlinesSelezionati = 0;
            var inlineSelezionati = Variabili_CopyPaste.InlinesSelezionati = new List<Inline>();
            var IndiceInizioSelezione = Variabili_CopyPaste.PosizioneInizioSelezione = OttieniInizioSelezione(cellaRichTextBox);
            var indiceFineSelezione = Variabili_CopyPaste.PosizioneFineSelezione = OttieniFineSelezione(cellaRichTextBox);
            var CursoreInizioSelezione = Variabili_CopyPaste.PuntatoreInizioSelezione = cellaRichTextBox.Selection.Start;
            var CursoreFineSelezione = Variabili_CopyPaste.PuntatoreFineSelezione = cellaRichTextBox.Selection.End;
            
            while (CursoreInizioSelezione != null && CursoreInizioSelezione.CompareTo(CursoreFineSelezione) <= 0)
            {
                var inline = CursoreInizioSelezione.Parent as Inline;
                if (inline != null)
                {
                    if (!inlineSelezionati.Contains(inline) && (inline is Run || inline is InlineUIContainer))
                    {
                        inlineSelezionati.Add(inline);
                        numeroInlinesSelezionati++;
                    }
                }
                var puntatoreSuccessivo = CursoreInizioSelezione.GetNextContextPosition(LogicalDirection.Forward);
                if (puntatoreSuccessivo == null || puntatoreSuccessivo.CompareTo(CursoreInizioSelezione) <= 0)
                {
                    break;
                }
                CursoreInizioSelezione = puntatoreSuccessivo;
            }
            
            var ParagrafoCorrente = inlineSelezionati.First().Parent as Paragraph;
            var indicePrimoRun = ((IList)ParagrafoCorrente.Inlines).IndexOf(inlineSelezionati.First());
            var indiceUltimoRun = ((IList)ParagrafoCorrente.Inlines).IndexOf(inlineSelezionati.Last());
            
            return (numeroInlinesSelezionati, inlineSelezionati, IndiceInizioSelezione, indiceFineSelezione, CursoreInizioSelezione, CursoreFineSelezione, indicePrimoRun, indiceUltimoRun);
        }
        return (0, null, -1, -1, null, null, -1, -1);
    }
    
    private static int OttieniFineSelezione(CellaRichTextBox cellaRichTextBox)
    {
        if (cellaRichTextBox.Selection.IsEmpty)
        {
            return -1;
        }
        var fine = cellaRichTextBox.Selection.End;
        var ultimoInline = fine.Parent as Inline;

        if (ultimoInline is Run ultimoRun)
        {
            var runContentEnd = ultimoRun.ContentStart;
            return fine.GetOffsetToPosition(runContentEnd) * -1;
        }

        return -1;
    }
    private static int OttieniInizioSelezione(CellaRichTextBox cellaRichTextBox)
    {
        if (cellaRichTextBox.Selection.IsEmpty)
        {
            return -1;
        }
        var inizio = cellaRichTextBox.Selection.Start;
        var primoInline = inizio.Parent as Inline;

        if (primoInline is Run primoRun)
        {
            var contenutoRunAvvio = primoRun.ContentStart;
            return inizio.GetOffsetToPosition(contenutoRunAvvio) * -1;
        }
        return -1;
    }
    
    #endregion
    
    #region Metodi COPIA

    public static void SelezionaInlinesDaCopiare(CellaRichTextBox cellaRichTextBox)
    {
        if (!cellaRichTextBox.Selection.IsEmpty)
        {
            #region Variabili 
            var (numeroInlinesSelezionati,
                inlineSelezionati, 
                IndiceInizioSelezione, 
                indiceFineSelezione, 
                CursoreInizioSelezione, 
                CursoreFineSelezione, 
                indicePrimoRun, 
                indiceUltimoRun) = EstraiSelezione(cellaRichTextBox);
            var PrePrimoRun = new Run();
            var PostPrimoRun = new Run();
            var PreUltimoRun = new Run();
            var PostUltimoRun = new Run();
            #endregion

            // Adattamento degli Inlines selezionati
            if (numeroInlinesSelezionati >= 2)
            {
                if (inlineSelezionati[0] is Run && IndiceInizioSelezione >= 1)
                {
                    (PrePrimoRun, PostPrimoRun) = InlineExtensions.SplitRun(inlineSelezionati[0] as Run, IndiceInizioSelezione);
                    inlineSelezionati.RemoveAt(0);
                    inlineSelezionati.Insert(0, PostPrimoRun);
                }

                if (inlineSelezionati.Last() is Run && indiceFineSelezione >= 1)
                {
                    (PreUltimoRun, PostUltimoRun) = InlineExtensions.SplitRun(inlineSelezionati.Last() as Run, indiceFineSelezione);
                    inlineSelezionati.RemoveAt(inlineSelezionati.LastIndexOf(inlineSelezionati.Last()));
                    inlineSelezionati.Insert(inlineSelezionati.Count, PrePrimoRun);
                }
            }
            else if (numeroInlinesSelezionati == 1)
            {
                (PrePrimoRun, PostPrimoRun) = InlineExtensions.SplitRun(inlineSelezionati[0] as Run, IndiceInizioSelezione);
                (PreUltimoRun, PostUltimoRun) = InlineExtensions.SplitRun(PostPrimoRun, indiceFineSelezione-IndiceInizioSelezione);
                inlineSelezionati.RemoveAt(0);
                inlineSelezionati.Insert(0, PreUltimoRun);
            }
            CopiaInilinesSelezionatiInClipboard(inlineSelezionati);
        }
    }
    
    private static void CopiaInilinesSelezionatiInClipboard(List<Inline> inlines)
    {
        var copiedInlines = InlineExtensions.DeepCopyInlineList(inlines);
        (var plainText, Dictionary<InlineUIContainer, string> inlineUIContainerTexts) = ConvertInlinesToPlainText(copiedInlines);
        var rtfText = ConvertInlinesToRtf(copiedInlines, inlineUIContainerTexts);
        var xamlText = ConvertInlinesToXaml(copiedInlines);
            
        var dataObject = new DataObject();
        dataObject.SetText(plainText, TextDataFormat.UnicodeText);
        dataObject.SetText(rtfText, TextDataFormat.Rtf);
        dataObject.SetData("CustomRichTextBoxFormat", xamlText);

        Clipboard.SetDataObject(dataObject, true);
    }
    #endregion

    #region Metodo INCOLLA
    public static void PasteInlineDaClipboard(CellaRichTextBox cellaRichTextBox)
    {
        if (Clipboard.ContainsData("CustomRichTextBoxFormat"))
        {
            var contenutoClipboardXaml = Clipboard.GetData("CustomRichTextBoxFormat") as string;
            
            if (string.IsNullOrEmpty(contenutoClipboardXaml))
            {
                return;
            }
            try
            {
                List<Inline> inlinesPaste = InlineExtensions.ParseInlineListFromXaml(contenutoClipboardXaml);
                
                if (inlinesPaste == null)
                {
                    return;
                }
                
                // Verifica che non ci sia una Selezione
                if (cellaRichTextBox.Selection.IsEmpty)
                {
                    // Ottieni la posizione del cursore
                    var PosizioneCursore = cellaRichTextBox.CellaEditing.CaretPosition;
                
                    //  Dividi l'elemento Run dove si trova il cursore in due parti
                    (var IndiceRun, var runPrimaDelCursore) = InlineExtensions.SplitRunAlCursore(cellaRichTextBox, PosizioneCursore);
                
                    // Inserisci gli Inlines alla posizione del cursore
                    var paragrafoParente = runPrimaDelCursore.Parent as Paragraph;
                
                    if (paragrafoParente == null)
                    {
                        throw new InvalidOperationException("The current Run is not within a Paragraph element.");
                    }
                
                    cellaRichTextBox.BeginChangeWrapper();
                    var indiceInserimento = IndiceRun+1;
                    foreach (var inline in inlinesPaste)
                    {
                        ((IList)paragrafoParente.Inlines).Insert(indiceInserimento, inline);
                        indiceInserimento++;
                    }
                
                    // Sposta il cursore alla fine del contenuto incollato
                    var runDopoCursore = paragrafoParente.Inlines.ElementAt(IndiceRun + inlinesPaste.Count) as Run;
                    if (runDopoCursore != null)
                    {
                        cellaRichTextBox.CellaEditing.CaretPosition = runDopoCursore.ContentEnd;
                    }
                    cellaRichTextBox.EndChangeWrapper();
                }
                
                // Se esiste una Selezione, cancellala e inserisci il contentuo copiato nel punto di inizio della selezione
                else
                {
                    var (numeroInlinesSelezionati,
                        inlineSelezionati, 
                        IndiceInizioSelezione, 
                        indiceFineSelezione, 
                        CursoreInizioSelezione, 
                        CursoreFineSelezione, 
                        indicePrimoRun, 
                        indiceUltimoRun) = EstraiSelezione(cellaRichTextBox);
                    var PrimoRunSelezione = inlineSelezionati[0] as Run;
                    var UltimoRunSelezione = inlineSelezionati.Last() as Run;
                    var (PrimoRunSplitA, PrimoRunSplitB) = InlineExtensions.SplitRun(PrimoRunSelezione, IndiceInizioSelezione);
                    var (UltimoRunSplitA, UltimoRunSplitB) = InlineExtensions.SplitRun(UltimoRunSelezione, indiceFineSelezione);
                    
                    // Sostituisci tutti gli Inlines selezionati con il Run PrimoRunSplit
                    var paragrafoParente = PrimoRunSelezione.Parent as Paragraph;
                    if (paragrafoParente == null)
                    {
                        throw new InvalidOperationException("The current Run is not within a Paragraph element.");
                    }
                    
                    cellaRichTextBox.BeginChangeWrapper();
                    int runIndex = ((IList)paragrafoParente.Inlines).IndexOf(PrimoRunSelezione);
                    // Rimuovi tutti gli Inlines selezionati
                    for (int i = 0; i < numeroInlinesSelezionati; i++)
                    {
                        ((IList)paragrafoParente.Inlines).RemoveAt(runIndex);
                    }
                    // Inserisci il Run PrimoRunSplit al posto degli Inlines selezionati
                    var indiceInserimento = runIndex+1;
                    ((IList)paragrafoParente.Inlines).Insert(runIndex, PrimoRunSplitA);
                    foreach (var inline in inlinesPaste)
                    {
                        ((IList)paragrafoParente.Inlines).Insert(indiceInserimento, inline);
                        indiceInserimento++;
                    }
                    ((IList)paragrafoParente.Inlines).Insert(indiceInserimento, UltimoRunSplitB);
                    cellaRichTextBox.EndChangeWrapper();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error pasting content: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else
        {
            // Paste normale se non è presente il formato CustomRichTextBoxFormat
            cellaRichTextBox.Paste();
        }
    }
    #endregion
}

