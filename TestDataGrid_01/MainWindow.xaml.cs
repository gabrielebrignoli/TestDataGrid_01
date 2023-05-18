using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Win32;
using TestDataGrid_01.CustomDataGridCell;
using TestDataGrid_01.CustomDataGridCell.SOURCE;
using TestDataGrid_01.CustomDataGridCell.TARGET;
using TestDataGrid_01.Data;
using TestDataGrid_01.Database;
using TestDataGrid_01.Gestore_Xliff;
using TestDataGrid_01.Loader_Files;
using TestDataGrid_01.ViewModels;
using TestDataGrid_01.WordToXliff;

namespace TestDataGrid_01
{
    public static class Variabili
    {
        // VARIABILI SOURCE
        public static List<Inline> InlineSelezioneCorrente_SOURCE { get; set; } = new List<Inline>();
        public static TextPointer InizioSelezione_SOURCE { get; set; }
        public static TextPointer FineSelezione_SOURCE { get; set; }
        public static int InizioSelezione_INT_SOURCE { get; set; }
        public static int FineSelezione_INT_SOURCE { get; set; }
        public static TextPointer UltimaPosizioneCursoreSource { get; set; }
        public static CellaRichTextBoxSource LastSelectedInstanceSource { get; set; }
        
        // VARIABILI TARGET
        public static List<Inline> InlineSelezioneCorrente_TARGET { get; set; } = new List<Inline>();
        public static TextPointer InizioSelezione_TARGET { get; set; }
        public static TextPointer FineSelezione_TARGET { get; set; }
        public static int InizioSelezione_INT_TARGET { get; set; }
        public static int FineSelezione_INT_TARGET { get; set; }
        public static TextPointer UltimaPosizioneCursoreTarget { get; set; }
        public static CellaRichTextBoxTarget LastSelectedInstanceTarget { get; set; }
    }
    public static class Percorsi
    {
        public static string CurrentPath =
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public static string CsvFullPath { get; set; }
        public static string XliffFullPath { get; set; }
        public static string WorkingDir { get; set; }
        public static string PercorsoDatabase { get; set; }
        public static string PercorsoFileWord { get; set; }
        
        // DataGrid
        public static int NumeroRiga { get; set; }
        public static int NumeroRigaCorrente { get; set; }

    }

    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        private MainViewModel _mainViewModel; // Declare a MainViewModel instance
        public System.Windows.Controls.DataGrid ExposedDocumentDataGrid => DocumentDataGrid;
        private DatabaseService _databaseService = null!;

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            DocumentDataGrid.SelectionChanged += DocumentDataGrid_SelectionChanged;
            DocumentDataGrid.GotFocus += DocumentDataGrid_GotFocus;
        }

        private void DocumentDataGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            if (cell != null && cell.Content is CellaRichTextBoxSource rtbs)
            {
                rtbs?.Focus();
            }
            else if (cell != null && cell.Content is CellaRichTextBoxTarget rtbt)
            {
                rtbt?.Focus();
            }
        }

        private void DocumentDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DocumentDataGrid.SelectedItem != null)
            {
                var NumeroRigaCorrente = HelperComuni.EstraiNumeroDiRiga(DocumentDataGrid);

                Console.WriteLine($"Selected Row Index: {NumeroRigaCorrente}");
                
                // CANCELLA SELEZIONE QUANDO CAMBIO RIGA
                if (Variabili.LastSelectedInstanceSource != null)
                {
                    HelperComuni.ClearSelectionSource(Variabili.LastSelectedInstanceSource);
                }
                if (Variabili.LastSelectedInstanceTarget != null)
                {
                    HelperComuni.ClearSelectionTarget(Variabili.LastSelectedInstanceTarget);
                }
            }
        }

        private void Carica_CSV(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                Multiselect = false
            };
            if (openFileDialog.ShowDialog() == true)
            {
                Percorsi.CsvFullPath = openFileDialog.FileName;
                var fileName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                Percorsi.WorkingDir = Path.Combine(Percorsi.CurrentPath, fileName);
                Directory.CreateDirectory(Percorsi.WorkingDir);
                Percorsi.PercorsoDatabase = Path.Combine(Percorsi.WorkingDir,
                    Path.GetFileNameWithoutExtension(Percorsi.CsvFullPath) + ".db");

                var dataModels = Loader_Csv.LoadDataFromCsv(Percorsi.CsvFullPath);

                if (Database_Helper.DatabaseExists())
                {
                    DataModelRepository.UpdateDatabase(dataModels);
                }
                else
                {
                    Database_Helper.InitializeDatabase();
                    DataModelRepository.UpdateDatabase(dataModels);
                }

                // Load data from the database into the ViewModel
                var mainViewModel = DataContext as MainViewModel;
                if (mainViewModel != null)
                {
                    mainViewModel.DataItems.Clear();
                    using (var context = new AppDbContext())
                    {
                        var loadedData = context.DataModels.ToList();
                        foreach (var d in loadedData)
                        {
                            mainViewModel.DataItems.Add(new DocumentViewModel
                            {
                                ID = d.ID,
                                Source = d.Source,
                                Target = d.Target
                            });
                        }
                    }
                }
            }
        }

        private void Carica_Xliff(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "mqxliff files (*.mqxliff)|*.mqxliff|All files (*.*)|*.*",
                Multiselect = false
            };
            if (openFileDialog.ShowDialog() == true)
            {
                Percorsi.XliffFullPath = openFileDialog.FileName;

                Percorsi.WorkingDir = Path.Combine(Percorsi.CurrentPath,
                    Path.GetFileNameWithoutExtension(openFileDialog.FileName));
                Directory.CreateDirectory(Percorsi.WorkingDir);

                Percorsi.PercorsoDatabase = Path.Combine(Percorsi.WorkingDir,
                    Path.GetFileNameWithoutExtension(Percorsi.XliffFullPath) + ".db");

                var dataModels = Parser_Xliff.Parse_Xliff(Percorsi.XliffFullPath);

                if (Database_Helper.DatabaseExists())
                {
                    DataModelRepository.UpdateDatabase(dataModels);
                }
                else
                {
                    Database_Helper.InitializeDatabase();
                    DataModelRepository.UpdateDatabase(dataModels);
                }

                // Load data from the database into the ViewModel
                var mainViewModel = DataContext as MainViewModel;
                if (mainViewModel != null)
                {
                    mainViewModel.DataItems.Clear();
                    using (var context = new AppDbContext())
                    {
                        var loadedData = context.DataModels.ToList();
                        foreach (var d in loadedData)
                        {
                            mainViewModel.DataItems.Add(new DocumentViewModel
                            {
                                ID = d.ID,
                                Source = d.Source,
                                Target = d.Target
                            });
                        }
                    }
                }
            }
        }

        #region Gestore tasti Datagrid
        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string _previousCellContent = null;
            int _previousCellIndex = 0;

            var cellaRichTextBox_Selected = GetCurrentCellaRichTextBox();
            if (cellaRichTextBox_Selected != null)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.P)
                {
                    e.Handled = true;
                    if (cellaRichTextBox_Selected is CellaRichTextBoxTarget CellaTarget)
                    {
                        var selezionesource = Variabili.InlineSelezioneCorrente_SOURCE;
                        HelperComuni.StampaInline(selezionesource);
                        
                        // Find the target cell in the same row
                        var content = DocumentDataGrid.Columns[2].GetCellContent(DocumentDataGrid.SelectedItem);
                        var currentCell = HelperComuni.OttieniDataGridCell(DocumentDataGrid, DocumentDataGrid.CurrentCell);
                        if (currentCell != null)
                        {
                            HelperComuni.InserisciTestoSourceToTarget(Variabili.LastSelectedInstanceSource, CellaTarget, currentCell);
                        }
                    }
                }

                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                    e.Key == Key.C)
                {
                    e.Handled = true;
                    sender = cellaRichTextBox_Selected;
                    if (cellaRichTextBox_Selected is CellaRichTextBoxSource sourceRTB)
                    {
                        CellaRichTextBoxSource.OnCopyCommand(sender, null);
                    }
                    else if (cellaRichTextBox_Selected is CellaRichTextBoxTarget targetRTB)
                    {
                        CellaRichTextBoxTarget.OnCopyCommand(sender, null);
                    }
                }

                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                    e.Key == Key.V)
                {
                    e.Handled = true;
                    sender = cellaRichTextBox_Selected;
                    if (cellaRichTextBox_Selected is CellaRichTextBoxSource sourceRTB)
                    {
                        CellaRichTextBoxSource.OnPasteCommand(sender, null);
                    }
                    else if (cellaRichTextBox_Selected is CellaRichTextBoxTarget targetRTB)
                    {
                        CellaRichTextBoxTarget.OnPasteCommand(sender, null);
                    }
                }

                if (e.Key == Key.Tab)
                {
                    e.Handled = true;
                    var IndiceColonna = DocumentDataGrid.CurrentCell.Column.DisplayIndex;

                    // Determine the next column to move
                    int IndiceColonnaSuccessiva = IndiceColonna == 1 ? 2 : 1;

                    // Set the current cell to the next column
                    DocumentDataGrid.CurrentCell = new DataGridCellInfo(
                        DocumentDataGrid.Items[DocumentDataGrid.SelectedIndex],
                        DocumentDataGrid.Columns[IndiceColonnaSuccessiva]);
                    CellaRichTextBoxSource.NumeroRigaPrecedenteSource = Percorsi.NumeroRigaCorrente;
                    // Find the CellaRichTextBox by the name
                    HelperComuni.ImpostaFocusSuCella(DocumentDataGrid, IndiceColonnaSuccessiva);
                }
            }

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                (e.Key == Key.Down || e.Key == Key.Up))
            {
                e.Handled = true;
                int columnIndex = DocumentDataGrid.CurrentCell.Column.DisplayIndex;

                if (DocumentDataGrid.CurrentItem != null)
                {
                    // Store the content of the current cell
                    var currentContent = DocumentDataGrid.Columns[columnIndex]
                        .GetCellContent(DocumentDataGrid.SelectedItem);
                    if (currentContent is ContentPresenter currentContentPresenter)
                    {
                        string currentControlName = columnIndex == 1 ? "CellaSource" : "CellaTarget";
                        var currentCellaRichTextBox =
                            currentContentPresenter.ContentTemplate.FindName(currentControlName,
                                    currentContentPresenter) as
                                RichTextBox;

                        if (currentCellaRichTextBox != null &&
                            currentCellaRichTextBox is CellaRichTextBoxSource currentCellaRichTextBoxSource)
                        {
                            currentCellaRichTextBoxSource.OnUpdateTextRequested();
                            _previousCellContent = currentCellaRichTextBoxSource.TestoCella;
                            _previousCellIndex = DocumentDataGrid.SelectedIndex;
                        }
                        else if (currentCellaRichTextBox != null &&
                                 currentCellaRichTextBox is CellaRichTextBoxTarget currentCellaRichTextBoxTarget)
                        {
                            currentCellaRichTextBoxTarget.OnUpdateTextRequested();
                            _previousCellContent = currentCellaRichTextBoxTarget.TestoCella;
                            _previousCellIndex = DocumentDataGrid.SelectedIndex;
                        }
                    }

                    // Determine the direction to move
                    int direction = e.Key == Key.Down ? 1 : -1;

                    // Calculate the new index, prevent cycling through when reaching the top or bottom
                    int newIndex = DocumentDataGrid.SelectedIndex + direction;
                    if (newIndex >= 0 && newIndex <= DocumentDataGrid.Items.Count)
                    {
                        // Move to the new cell
                        DocumentDataGrid.SelectedIndex = newIndex;
                        DocumentDataGrid.CurrentCell = new DataGridCellInfo(
                            DocumentDataGrid.Items[DocumentDataGrid.SelectedIndex],
                            DocumentDataGrid.Columns[columnIndex]);
                        DocumentDataGrid.BeginEdit();

                        HelperComuni.ImpostaFocusSuCella(DocumentDataGrid, columnIndex);


                        // Update the database using the previous cell's content and index
                        Console.WriteLine("Previous cell index: " + _previousCellIndex + "Column index: " +
                                          columnIndex + "Cell content: " + _previousCellContent);
                        //HelperMethods.UpdateCsvCell(_previousCellIndex, columnIndex, _previousCellContent);
                        //DataModelRepository.UpdateDatabaseRealTime(DocumentDataGrid.DataContext, _previousCellIndex,
                        //columnIndex, _previousCellContent);
                    }
                }
            }
        }
        #endregion
        
        // Ottieni la cella corrente
        private RichTextBox GetCurrentCellaRichTextBox()
        {
            int columnIndex = DocumentDataGrid.CurrentCell.Column.DisplayIndex;

            var content = DocumentDataGrid.Columns[columnIndex].GetCellContent(DocumentDataGrid.SelectedItem);
            if (content is ContentPresenter contentPresenter)
            {
                if (columnIndex == 1) 
                {
                    var cellaRichTextBoxSource =
                        contentPresenter.ContentTemplate.FindName("CellaSource", contentPresenter) as
                            CellaRichTextBoxSource;
                    return cellaRichTextBoxSource;
                }
                else if (columnIndex == 2) 
                {
                    var cellaRichTextBoxTarget =
                        contentPresenter.ContentTemplate.FindName("CellaTarget", contentPresenter) as
                            CellaRichTextBoxTarget;
                    return cellaRichTextBoxTarget;
                }
            }
            return null;
        }
        
        private void Open_Doc_Docx(object sender, RoutedEventArgs e)
        {
            // Open File Dialog to load a .doc or .docx file
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Word Documents|*.doc;*.docx",
                Multiselect = false
            };
            // Store the word filename into a variable
            if (openFileDialog.ShowDialog() == true)
            {
                Percorsi.PercorsoFileWord = openFileDialog.FileName;
                var NomeFileWord = Path.GetFileNameWithoutExtension(Percorsi.PercorsoFileWord);
                Percorsi.WorkingDir = Path.Combine(Percorsi.CurrentPath,
                    NomeFileWord);
                Directory.CreateDirectory(Percorsi.WorkingDir);
                Percorsi.PercorsoDatabase = Path.Combine(Percorsi.WorkingDir,
                    NomeFileWord + ".db");
                var NomeFileXliff = NomeFileWord + ".xliff";
                var PercorsoXliff = Path.Combine(Percorsi.WorkingDir, NomeFileXliff);

                ConvertitoreWord_Xliff.CreaXliff(Percorsi.PercorsoFileWord, PercorsoXliff);

                var dataModels = Parser_Xliff.Parse_Xliff(PercorsoXliff);

                if (Database_Helper.DatabaseExists())
                {
                    DataModelRepository.UpdateDatabase(dataModels);
                }
                else
                {
                    Database_Helper.InitializeDatabase();
                    DataModelRepository.UpdateDatabase(dataModels);
                }

                // Load data from the database into the ViewModel
                var mainViewModel = DataContext as MainViewModel;
                if (mainViewModel != null)
                {
                    mainViewModel.DataItems.Clear();
                    using (var context = new AppDbContext())
                    {
                        var loadedData = context.DataModels.ToList();
                        foreach (var d in loadedData)
                        {
                            mainViewModel.DataItems.Add(new DocumentViewModel
                            {
                                ID = d.ID,
                                Source = d.Source,
                                Target = d.Target
                            });
                        }
                    }
                }
            }
        }
    }
}





