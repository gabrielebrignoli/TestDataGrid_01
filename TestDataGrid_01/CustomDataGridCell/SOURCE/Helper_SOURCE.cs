using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
namespace TestDataGrid_01.CustomDataGridCell.SOURCE;

public abstract class HelperSource
{
    // METODO GENERALE PER AGGIUNGERE INLINE ADATTATI IN UNA SELEZIONE A UN ELEMENT LIST  *************** METODO PRINCIPALE ***************
    public static (List<Inline>, int? firstRunIndex, int? lastRunIndex) MemorizzaInlinesSelezione(List<Inline> inlinesInSelezione, CellaRichTextBoxSource cellaRichTextBoxSource)
    {
        var start = cellaRichTextBoxSource.Selection.Start;
        var end = cellaRichTextBoxSource.Selection.End;
        var conteggioInlines = 0;
        int? firstRunIndex = null;
        int? lastRunIndex = null;
        inlinesInSelezione.Clear();

        if (start != end)
        {
            var primoinline = start.Parent as Inline;
            var ultimoInline = end.Parent as Inline;

            var (indiceInizioSelezione, indiceFineSelezione) = HelperComuni.GetSelectionIndices(primoinline, ultimoInline, start, end);
            Variabili.InizioSelezione_INT_SOURCE = indiceInizioSelezione;
            Variabili.FineSelezione_INT_SOURCE = indiceFineSelezione;
            Inline lastAddedInline = null;

            while (start != null && start.CompareTo(end) < 0)
            {
                var currentInline = start.Parent as Inline;

                if (currentInline != null && currentInline != lastAddedInline)
                {
                    HelperComuni.AddToSelection(currentInline, inlinesInSelezione, ref conteggioInlines);
                    lastAddedInline = currentInline;
                }

                start = start.GetNextContextPosition(LogicalDirection.Forward);
            }

            Run postPrimoRun;
            Run preUltimoRun;
            if(conteggioInlines>=2)
            {
                if(inlinesInSelezione[0] is Run && indiceInizioSelezione >=1)
                {
                    (_,postPrimoRun) = HelperComuni.SplitRunAtPosition(inlinesInSelezione[0] as Run, indiceInizioSelezione);
                    inlinesInSelezione.RemoveAt(0);
                    inlinesInSelezione.Insert(0,postPrimoRun);
                }

                if(inlinesInSelezione.Last() is Run&&indiceFineSelezione>=1)
                {
                    (preUltimoRun,_) = HelperComuni.SplitRunAtPosition(inlinesInSelezione.Last() as Run, indiceFineSelezione);
                    inlinesInSelezione.RemoveAt(inlinesInSelezione.LastIndexOf(inlinesInSelezione.Last()));
                    inlinesInSelezione.Insert(inlinesInSelezione.Count,preUltimoRun);
                }
            }
            else if(conteggioInlines==1)
            {
                var indiceselezionecorretto = indiceFineSelezione-indiceInizioSelezione;
                
                (_,postPrimoRun) = HelperComuni.SplitRunAtPosition(inlinesInSelezione[0] as Run,indiceInizioSelezione);
                (preUltimoRun,_) = HelperComuni.SplitRunAtPosition(postPrimoRun,indiceselezionecorretto);
                inlinesInSelezione.RemoveAt(0);
                inlinesInSelezione.Insert(0,preUltimoRun);
            }
        }
        return (inlinesInSelezione, firstRunIndex, lastRunIndex);
    }
    
    // METODO GENERALE PER CANCELLARE UNA SELEZIONE E ADATTARE INLINE ALL'INIZIO E ALLA FINE
    public static void CancellaSelezioneEmodificaParagrafo(List<Inline> inlinepaste, CellaRichTextBoxSource cellaRichTextBoxSource)
    {
        var start = cellaRichTextBoxSource.Selection.Start;
        var end = cellaRichTextBoxSource.Selection.End;
        var conteggioInlines = 0;
        var inlinesInSelezione = new List<Inline>();

        if (start != end)
        {
            var primoinline = start.Parent as Inline;
            var ultimoInline = end.Parent as Inline;
            var inlinecorrente = primoinline;

            (var indiceInizioSelezione, var indiceFineSelezione) = HelperComuni.GetSelectionIndices(primoinline, ultimoInline, start, end);

            while (start != null && start.CompareTo(end) < 0)
            {
                if (inlinecorrente != null)
                {
                    HelperComuni.AddToSelection(start.Parent as Inline, inlinesInSelezione, ref conteggioInlines);
                    start = start.GetNextContextPosition(LogicalDirection.Forward);
                }
            }
            var paragrafo = primoinline?.Parent as Paragraph;

            Run prePrimoRun;
            Run postPrimoRun;
            Run postUltimoRun;
            if(conteggioInlines>=2)
            {
                if(inlinesInSelezione[0] is Run && indiceInizioSelezione >=1)
                {
                    (prePrimoRun, postPrimoRun) = HelperComuni.SplitRunAtPosition(inlinesInSelezione[0] as Run, indiceInizioSelezione);
                    inlinepaste.Insert(0, prePrimoRun);
                }
                if(inlinesInSelezione.Last() is Run&&indiceFineSelezione >=1)
                {
                    (_, postUltimoRun) = HelperComuni.SplitRunAtPosition(inlinesInSelezione.Last() as Run, indiceFineSelezione);
                    inlinepaste.Insert(0, postUltimoRun);
                }
                    
                var tempInline = primoinline;
                cellaRichTextBoxSource.BeginChange();
                foreach (var inline in inlinepaste)
                {
                    paragrafo.Inlines.InsertAfter(tempInline, inline);
                    tempInline = inline;
                }
                foreach (var inlines in inlinesInSelezione)
                {
                    paragrafo.Inlines.Remove(inlines);
                }

                cellaRichTextBoxSource.EndChange();
            }
                
            else if(conteggioInlines==1)
            {
                var indiceselezionecorretto = indiceFineSelezione-indiceInizioSelezione;
                
                (prePrimoRun, postPrimoRun) = HelperComuni.SplitRunAtPosition(primoinline, indiceInizioSelezione);
                inlinepaste.Insert(0, prePrimoRun);
                (_, postUltimoRun) = HelperComuni.SplitRunAtPosition(postPrimoRun, indiceselezionecorretto);
                inlinepaste.Insert(inlinepaste.Count, postUltimoRun);
                    
                var tempInline = primoinline;
                cellaRichTextBoxSource.BeginChange();
                
                foreach (var inline in inlinepaste)
                {
                    paragrafo.Inlines.InsertAfter(tempInline, inline);
                    tempInline = inline; // Update inlineA to be the last inserted inline
                }
                
                paragrafo.Inlines.Remove(primoinline);
                cellaRichTextBoxSource.EndChange();
            }
        }
        else
        {
            throw new ArgumentException("The provided TextPointer instances must belong to a Paragraph.");
        }
    }
    
}
