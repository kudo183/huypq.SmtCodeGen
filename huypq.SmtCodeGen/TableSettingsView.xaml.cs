using SimpleDataGrid;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace huypq.SmtCodeGen
{
    /// <summary>
    /// Interaction logic for TableSettingsView.xaml
    /// </summary>
    public partial class TableSettingsView : UserControl
    {
        public TableSettingsView()
        {
            InitializeComponent();
        }

        private void tableSetingGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ColumnSetting.DbColumn):
                case nameof(ColumnSetting.OldOrder):
                case nameof(ColumnSetting.DataGridColumnTypeList):
                case nameof(ColumnSetting.IsNeedReferenceData):
                    e.Cancel = true;
                    break;
                case nameof(ColumnSetting.ColumnName):
                    e.Column.IsReadOnly = true;
                    break;
                case nameof(ColumnSetting.DataGridColumnType):
                    {
                        var column = new DataGridComboBoxColumnExt();
                        column.Header = nameof(ColumnSetting.DataGridColumnType);
                        column.SelectedItemBinding = new Binding(nameof(ColumnSetting.DataGridColumnType));
                        BindingOperations.SetBinding(column, ComboBox.ItemsSourceProperty, new Binding(nameof(ColumnSetting.DataGridColumnTypeList)));
                        e.Column = column;
                    }
                    break;
                case nameof(ColumnSetting.Order):
                    {
                        var table = (sender as DataGrid).DataContext as TableSetting;
                        var column = new DataGridComboBoxColumnExt();
                        column.Header = nameof(ColumnSetting.Order);
                        column.SelectedItemBinding = new Binding(nameof(ColumnSetting.Order));
                        column.ItemsSource = Enumerable.Range(0, table.ColumnSettings.Count).ToList();
                        e.Column = column;
                    }
                    break;
            }
        }
    }
}
