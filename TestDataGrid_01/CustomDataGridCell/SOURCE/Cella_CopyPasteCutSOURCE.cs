using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using TestDataGrid_01.CustomDataGridCell.SOURCE;

namespace TestDataGrid_01.CustomDataGridCell;

public static class CellaSOURCE_CopyPasteCut
{
    #region Metodo INCOLLA
    public static void PasteInlineDaClipboard(CellaRichTextBoxSource cellaRichTextBox)
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
                List<Inline> inlinesPaste = HelperComuni.ParseInlineListFromXaml(contenutoClipboardXaml);
                
                if (inlinesPaste == null)
                {
                    return;
                }

                // PASTE IN CASO DI MANCANZA DI SELEZIONE
                if (cellaRichTextBox.Selection.IsEmpty)
                {
                    var PosizioneCursore = cellaRichTextBox.CaretPosition;
                    var PosizioneCursoreInt = -1;
                    var RunPrimaPaste = new Run();
                    var RunDopoPaste = new Run();
                    
                    
                    // OTTIENI INLINE DOVE SI TROVA IL CURSORE = inlineAlCursore
                    if (HelperComuni.OttieniInlineAlCursore(PosizioneCursore) is Run inlineAlCursore)
                    {
                        if (inlineAlCursore == null)
                        {
                            throw new InvalidOperationException("Cursor is not within an Inline.");
                        }
                        
                        // OTTIENI IL PARAGRAFO DELL'INLINE
                        var paragraph = inlineAlCursore.Parent as Paragraph;
                        
                        // OTTIENI LA LISTA DI INLINES DEL PARAGRAFO
                        var inlines = paragraph.Inlines.ToList();
                        
                        // OTTIENI L'INDICE DELL'INLINE DOVE SI TROVA IL CURSORE
                        var IndiceInlineCursore = inlines.IndexOf(inlineAlCursore);
                        
                        // OTTIENI LA POSIZIONE DEL CURSORE NELL'INLINE
                        PosizioneCursoreInt = inlineAlCursore.ContentStart.GetOffsetToPosition(PosizioneCursore);
                        
                        // DIVIDI L'INLINE DOVE SI TROVA IL CURSORE
                        (RunPrimaPaste, RunDopoPaste) = HelperComuni.SplitRunAtPosition(inlineAlCursore, PosizioneCursoreInt);
                        
                        cellaRichTextBox.BeginChange();
                        // MODIFICA IL PARAGRAFO
                        var tempInline = RunPrimaPaste as Inline;
                        paragraph.Inlines.InsertAfter(inlineAlCursore, RunPrimaPaste);
                        foreach (var inline in inlinesPaste)
                        {
                            paragraph.Inlines.InsertAfter(tempInline, inline);
                            tempInline = inline; // Update inlineA to be the last inserted inline
                        }
                        paragraph.Inlines.InsertAfter(tempInline, RunDopoPaste);
                        paragraph.Inlines.Remove(inlineAlCursore);
                        
                        
                        // Sposta il cursore alla fine del contenuto incollato
                        var runDopoCursore = paragraph.Inlines.ElementAt(IndiceInlineCursore + inlinesPaste.Count) as Run;
                        if (runDopoCursore != null)
                        {
                            cellaRichTextBox.CaretPosition = runDopoCursore.ContentEnd;
                        }
                        cellaRichTextBox.EndChange();
                    }
                }
                
                // PASTE IN CASO DI SELEZIONE
                else if (!cellaRichTextBox.Selection.IsEmpty)
                {
                    HelperSource.CancellaSelezioneEmodificaParagrafo(inlinesPaste, cellaRichTextBox);
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
            //cellaRichTextBox.Paste();
            MessageBox.Show($"Error pasting content:  Error");
        }
    }
    #endregion
}

