using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace TestDataGrid_01.CustomDataGridCell.SOURCE;

public abstract class SelectionChangeSource
{
    public static void SelezioneSourceCorrente(CellaRichTextBoxSource cellaRichTextBoxSource)
    {
        int? indicePrimo = -1;
        int? indiceUltimo = -1;
        
        if (!cellaRichTextBoxSource.Selection.IsEmpty)
        {
            
            var InlineSelezionati_S = new List<Inline>();

            (InlineSelezionati_S, indicePrimo, indiceUltimo) = HelperSource.MemorizzaInlinesSelezione(InlineSelezionati_S, cellaRichTextBoxSource);
            Variabili.InlineSelezioneCorrente_SOURCE = InlineSelezionati_S;
            HelperComuni.EstraiTestoDaListInlines(InlineSelezionati_S);
        }
        else if (cellaRichTextBoxSource.Selection.IsEmpty &&
                 (cellaRichTextBoxSource.Selection.Start != cellaRichTextBoxSource.Selection.End))
        {
            Console.WriteLine("TAGS???");
        }
    }
}