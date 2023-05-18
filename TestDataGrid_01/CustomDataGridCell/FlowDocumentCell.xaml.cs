using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using TestDataGrid_01.Helper;

namespace TestDataGrid_01.CustomDataGridCell
{
    public partial class FlowDocumentCell : UserControl
    {
        public System.Windows.Controls.DataGrid ParentDataGrid { get; set; }
        
        public FlowDocumentCell()
        {
            InitializeComponent();
            DocumentEditor.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(DocumentEditor_MouseLeftButtonDown), true);
            DocumentEditor.MouseEnter += DocumentEditor_MouseEnter;
            DocumentEditor.MouseLeave += DocumentEditor_MouseLeave;
        }

        private void DocumentEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DocumentEditor_MouseLeave(object sender, MouseEventArgs e)
        {
            DocumentEditor.Cursor = Cursors.Arrow;
        }
        private void DocumentEditor_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!DocumentEditor.IsFocused)
            {
                DocumentEditor.Cursor = Cursors.Arrow;
            }
        }
        private void DocumentEditor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                DocumentEditor.Cursor = Cursors.IBeam;
                DocumentEditor.Focus();
                DocumentEditor.CaretPosition = DocumentEditor.GetPositionFromPoint(e.GetPosition(DocumentEditor), true);
            }
            else if (e.ClickCount == 2)
            {
                TextPointer textPointer = DocumentEditor.GetPositionFromPoint(e.GetPosition(DocumentEditor), false);
                if (textPointer == null)
                {
                    textPointer = DocumentEditor.GetPositionFromPoint(e.GetPosition(DocumentEditor), true);
                }

                if (textPointer != null)
                {
                    TextRange wordRange = textPointer.GetWordRange();
                    if (wordRange != null)
                    {
                        DocumentEditor.Selection.Select(wordRange.Start, wordRange.End);
                    }
                }
            }
        }

        public static readonly DependencyProperty DocumentXamlProperty =
            DependencyProperty.Register("DocumentXaml", typeof(string), typeof(FlowDocumentCell),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDocumentXamlChanged));
        public string DocumentXaml
        {
            get => (string)GetValue(DocumentXamlProperty);
            set => SetValue(DocumentXamlProperty, value);
        }
        
        private static void OnDocumentXamlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FlowDocumentCell)d;
            var DocumentXaml = (string)e.NewValue;
            //Console.WriteLine(DocumentXaml);
            try
            {
                if (!string.IsNullOrEmpty(DocumentXaml))
                {
                    control.DocumentEditor.Document = (FlowDocument)XamlReader.Parse(DocumentXaml);
                }
                else
                {
                    control.DocumentEditor.Document = new FlowDocument();
                }
            }
            catch (Exception)
            {
                // Provide a default empty FlowDocument in case of an error
                control.DocumentEditor.Document = new FlowDocument();
            }
        }
   }
}


 