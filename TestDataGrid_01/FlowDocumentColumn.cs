using System.Windows;
using System.Windows.Controls;
using TestDataGrid_01.CustomDataGridCell;

namespace TestDataGrid_01
{
    public class FlowDocumentColumn : DataGridBoundColumn
    {
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var flowDocumentCell = new FlowDocumentCell();
            flowDocumentCell.SetBinding(FlowDocumentCell.DocumentXamlProperty, Binding);
            cell.Selected += (sender, e) =>
            {
                flowDocumentCell.DocumentEditor.Focus();
            };
            return flowDocumentCell;
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            var flowDocumentCell = new FlowDocumentCell();
            flowDocumentCell.SetBinding(FlowDocumentCell.DocumentXamlProperty, Binding);
            cell.Selected += (sender, e) =>
            {
                flowDocumentCell.DocumentEditor.Focus();
            };
            return flowDocumentCell;
        }
    }
}