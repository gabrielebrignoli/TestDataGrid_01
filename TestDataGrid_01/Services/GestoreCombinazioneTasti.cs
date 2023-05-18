using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace TestDataGrid_01.Services;

public class GestoreCombinazioneTasti
{
    public ModifierKeys PrimoModificatore { get; set; }
    public ModifierKeys? SecondoModificatore { get; set; }
    public Key Tasto { get; set; }

    
    public GestoreCombinazioneTasti(ModifierKeys primoModificatore, Key tasto, ModifierKeys? secondoModificatore = null)
    {
        PrimoModificatore = primoModificatore;
        SecondoModificatore = secondoModificatore;
        Tasto = tasto;
    }
    
    public static GestoreCombinazioneTasti ParserCombinazioneTasti(string StringaCombinazioneTasti)
    {
        var partistringatasti = StringaCombinazioneTasti.Split('+');
        if (partistringatasti.Length < 2 || partistringatasti.Length > 3)
        {
            throw new ArgumentException("Invalid key combination string format");
        }
        
        var MappaTastiModificatori = new Dictionary<string, ModifierKeys>(StringComparer.OrdinalIgnoreCase)
        {
            { "Ctrl", ModifierKeys.Control },
            { "Shift", ModifierKeys.Shift },
            { "Alt", ModifierKeys.Alt },
            { "Windows", ModifierKeys.Windows },
        };
        
        if (!MappaTastiModificatori.TryGetValue(partistringatasti[0].Trim(), out var primoModificatore))
        {
            throw new ArgumentException($"Invalid first modifier key: '{partistringatasti[0].Trim()}'");
        }

        if (!Enum.TryParse(typeof(Key), partistringatasti[partistringatasti.Length - 1].Trim(), true, out var tastoObj))
        {
            throw new ArgumentException($"Invalid key: '{partistringatasti[partistringatasti.Length - 1].Trim()}'");
        }
        
        var tasto = (Key)tastoObj;

        ModifierKeys? secondoModificatore = null;
        if (partistringatasti.Length == 3)
        {
            if (!MappaTastiModificatori.TryGetValue(partistringatasti[1].Trim(), out var parsedSecondModifier))
            {
                throw new ArgumentException($"Invalid second modifier key: '{partistringatasti[1].Trim()}'");
            }
            secondoModificatore = parsedSecondModifier;
        }

        return new GestoreCombinazioneTasti(primoModificatore, tasto, secondoModificatore);
    }
    
    public bool IsMatch(KeyEventArgs e)
    {
        Key keyToCheck = e.Key;

        bool isTasto = keyToCheck == Tasto;
        if (!isTasto)
        {
            return false;
        }

        bool matchFirstModifier = (Keyboard.Modifiers & PrimoModificatore) == PrimoModificatore;
        bool matchSecondModifier = SecondoModificatore.HasValue ? (Keyboard.Modifiers & SecondoModificatore.Value) == SecondoModificatore.Value : true;

        // Check if any other modifiers are pressed
        ModifierKeys allModifiers = PrimoModificatore | (SecondoModificatore.HasValue ? SecondoModificatore.Value : 0);
        bool noExtraModifiers = (Keyboard.Modifiers & ~allModifiers) == 0;

        if (matchFirstModifier && matchSecondModifier && noExtraModifiers)
        {
            e.Handled = true;
            return true;
        }

        return false;
    }

}