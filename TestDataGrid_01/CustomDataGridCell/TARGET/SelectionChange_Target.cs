using System;
using System.Collections.Generic;
using System.Windows.Documents;
using TestDataGrid_01.CustomDataGridCell.TARGET;

namespace TestDataGrid_01.CustomDataGridCell;

public class SelectionChangeTarget
{
    public static void SelezioneTargetCorrente(CellaRichTextBoxTarget cellaRichTextBoxTarget)
    {
        int? indicePrimo = -1;
        int? indiceUltimo = -1;
        
        if (!cellaRichTextBoxTarget.Selection.IsEmpty)
        {
            
            var InlineSelezionati_T = new List<Inline>();

            (InlineSelezionati_T, indicePrimo, indiceUltimo) = HelperTarget.MemorizzaInlinesSelezione(InlineSelezionati_T, cellaRichTextBoxTarget);
            Variabili.InlineSelezioneCorrente_SOURCE = InlineSelezionati_T;
            HelperComuni.EstraiTestoDaListInlines(InlineSelezionati_T);
        }
        else if (cellaRichTextBoxTarget.Selection.IsEmpty &&
                 (cellaRichTextBoxTarget.Selection.Start != cellaRichTextBoxTarget.Selection.End))
        {
            Console.WriteLine("TAGS???");
        }
    }
}