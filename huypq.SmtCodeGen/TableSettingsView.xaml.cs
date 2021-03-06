﻿using SimpleDataGrid;
using System.Collections.Generic;
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
        private Dictionary<string, int> orderByOptions = new Dictionary<string, int>()
        {
            [""] = 0,
            ["Asc"] = 1,
            ["Des"] = 2
        };

        public TableSettingsView()
        {
            InitializeComponent();
        }

        private void tableSetingGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ColumnSetting.Width):
                case nameof(ColumnSetting.IsTabStop):
                case nameof(ColumnSetting.IsReadOnly):
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
                case nameof(ColumnSetting.OrderBy):
                    {
                        var column = new DataGridComboBoxColumnExt();
                        column.Header = nameof(ColumnSetting.OrderBy);
                        column.DisplayMemberPath = "Key";
                        column.SelectedValuePath = "Value";
                        column.SelectedValueBinding = new Binding(nameof(ColumnSetting.OrderBy));
                        column.ItemsSource = orderByOptions;
                        e.Column = column;
                    }
                    break;
                default:
                    e.Cancel = true;
                    break;
            }
        }
    }
}
