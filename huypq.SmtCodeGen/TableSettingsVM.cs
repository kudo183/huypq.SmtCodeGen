using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace huypq.SmtCodeGen
{
    public class TableSettingsVM : INotifyPropertyChanged
    {
        private ObservableCollection<DbTable> tables;

        public ObservableCollection<DbTable> Tables
        {
            get { return tables; }
            set
            {
                if (tables != value)
                {
                    if (tables != null)
                    {
                        tables.CollectionChanged -= Tables_CollectionChanged;
                    }
                    tables = value;
                    if (tables != null)
                    {
                        tables.CollectionChanged += Tables_CollectionChanged;
                    }
                    foreach (var item in tables)
                    {
                        var tableSetting = tableSettings.FirstOrDefault(p => p.TableName == item.TableName);
                        if (tableSetting != null)
                        {
                            tableSetting.DbTable = item;
                        }
                        else
                        {
                            TableSettings.Add(new TableSetting() { TableName = item.TableName, DbTable = item });
                        }
                    }
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<TableSetting> tableSettings;

        public ObservableCollection<TableSetting> TableSettings
        {
            get { return tableSettings; }
            set
            {
                if (tableSettings != value)
                {
                    tableSettings = value;
                    if (tables != null)
                    {
                        foreach (var item in tables)
                        {
                            var tableSetting = tableSettings.FirstOrDefault(p => p.TableName == item.TableName);
                            if (tableSetting != null)
                            {
                                tableSetting.DbTable = item;
                            }
                        }
                    }
                    OnPropertyChanged();
                }
            }
        }

        private TableSetting selectedTableSetting;

        public TableSetting SelectedTableSetting
        {
            get { return selectedTableSetting; }
            set
            {
                if (selectedTableSetting != value)
                {
                    selectedTableSetting = value;
                    OnPropertyChanged();
                }
            }
        }

        public TableSettingsVM()
        {
            tableSettings = new ObservableCollection<TableSetting>();
        }

        public void Reset()
        {
            tableSettings.Clear();
            foreach (var item in tables)
            {
                tableSettings.Add(new TableSetting() { TableName = item.TableName, DbTable = item });
            }
        }

        public void ResetSelectedTableSetting()
        {
            if (selectedTableSetting == null)
            {
                return;
            }
            selectedTableSetting.ColumnSettings.Clear();
            foreach (var item in selectedTableSetting.DbTable.Columns)
            {
                var column = new ColumnSetting() { ColumnName = item.ColumnName, DbColumn = item };
                column.InitColumnSettingFromDbColumn();
                column.Order = selectedTableSetting.ColumnSettings.Count;
                selectedTableSetting.ColumnSettings.Add(column);
            }
        }

        private void Tables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    {
                        var table = e.NewItems[0] as DbTable;
                        var i = 0;
                        for (; i < TableSettings.Count; i++)
                        {
                            if (TableSettings[i].TableName == table.TableName)
                            {
                                TableSettings[i].DbTable = table;
                                break;
                            }
                        }
                        if (i == TableSettings.Count)
                        {
                            TableSettings.Add(new TableSetting() { TableName = table.TableName, DbTable = table });
                        }
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    {
                        var table = e.OldItems[0] as DbTable;
                        var i = 0;
                        for (; i < TableSettings.Count; i++)
                        {
                            if (TableSettings[i].TableName == table.TableName)
                            {
                                TableSettings.RemoveAt(i);
                                break;
                            }
                        }
                    }
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TableSetting : INotifyPropertyChanged
    {
        private string tableName;

        public string TableName
        {
            get { return tableName; }
            set
            {
                if (tableName != value)
                {
                    tableName = value;
                    OnPropertyChanged();
                }
            }
        }

        private DbTable dbTable;

        public DbTable DbTable
        {
            get { return dbTable; }
            set
            {
                if (dbTable != value)
                {
                    dbTable = value;
                    foreach (var item in dbTable.Columns)
                    {
                        var columnSetting = columnSettings.FirstOrDefault(p => p.ColumnName == item.ColumnName);
                        if (columnSetting != null)
                        {
                            columnSetting.DbColumn = item;
                            columnSetting.DataGridColumnTypeList.Clear();
                            if (item.IsIdentity)
                            {
                                columnSetting.DataGridColumnTypeList.Add("DataGridTextColumnExt");
                            }
                            else if (item.IsForeignKey)
                            {
                                columnSetting.DataGridColumnTypeList.Add("DataGridComboBoxColumnExt");
                                columnSetting.DataGridColumnTypeList.Add("DataGridForeignKeyColumn");
                                columnSetting.DataGridColumnTypeList.Add("DataGridTextColumnExt");
                            }
                            else
                            {
                                switch (item.DataType)
                                {
                                    case "int":
                                    case "int?":
                                    case "long":
                                    case "long?":
                                        columnSetting.DataGridColumnTypeList.Add("DataGridRightAlignTextColumn");
                                        break;
                                    case "System.DateTime":
                                    case "System.DateTime?":
                                        columnSetting.DataGridColumnTypeList.Add("DataGridDateColumn");
                                        break;
                                    case "bool":
                                    case "bool?":
                                        columnSetting.DataGridColumnTypeList.Add("DataGridCheckBoxColumnExt");
                                        break;
                                    case "System.TimeSpan":
                                    case "System.TimeSpan?":
                                    case "string":
                                        columnSetting.DataGridColumnTypeList.Add("DataGridTextColumnExt");
                                        break;
                                }
                            }
                        }
                        else
                        {
                            var column = new ColumnSetting() { ColumnName = item.ColumnName, DbColumn = item };
                            column.InitColumnSettingFromDbColumn();
                            column.Order = ColumnSettings.Count;
                            ColumnSettings.Add(column);
                        }
                    }

                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<ColumnSetting> columnSettings;

        public ObservableCollection<ColumnSetting> ColumnSettings
        {
            get { return columnSettings; }
            set
            {
                if (columnSettings != value)
                {
                    if (columnSettings != null)
                    {
                        columnSettings.CollectionChanged -= ColumnSettings_CollectionChanged;
                    }
                    columnSettings = value;
                    if (columnSettings != null)
                    {
                        columnSettings.CollectionChanged += ColumnSettings_CollectionChanged;
                    }
                    foreach (var item in columnSettings)
                    {
                        item.PropertyChanged -= Item_PropertyChanged;
                        item.PropertyChanged += Item_PropertyChanged;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private void ColumnSettings_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    if (e.NewItems[0] != null)
                    {
                        (e.NewItems[0] as ColumnSetting).PropertyChanged += Item_PropertyChanged;
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    if (e.OldItems[0] != null)
                    {
                        (e.OldItems[0] as ColumnSetting).PropertyChanged -= Item_PropertyChanged;
                    }
                    break;
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ColumnSetting.Order))
            {
                var column = sender as ColumnSetting;
                //swap
                for (int i = 0; i < ColumnSettings.Count; i++)
                {
                    var item = ColumnSettings[i];
                    if (column.ColumnName != item.ColumnName && column.Order == item.Order)
                    {
                        item.PropertyChanged -= Item_PropertyChanged;
                        item.Order = column.OldOrder;
                        item.PropertyChanged += Item_PropertyChanged;
                    }
                }
                //insert
                //if (column.OldOrder < column.Order)
                //{
                //    for (int i = 0; i < ColumnSettings.Count; i++)
                //    {
                //        var item = ColumnSettings[i];
                //        if (column.ColumnName != item.ColumnName && item.Order > column.OldOrder && item.Order <= column.Order)
                //        {
                //            item.PropertyChanged -= Item_PropertyChanged;
                //            item.Order = item.Order - 1;
                //            item.PropertyChanged += Item_PropertyChanged;
                //        }
                //    }
                //}
                //else if (column.OldOrder > column.Order)
                //{
                //    for (int i = 0; i < ColumnSettings.Count; i++)
                //    {
                //        var item = ColumnSettings[i];
                //        if (column.ColumnName != item.ColumnName && item.Order < column.OldOrder && item.Order >= column.Order)
                //        {
                //            item.PropertyChanged -= Item_PropertyChanged;
                //            item.Order = item.Order + 1;
                //            item.PropertyChanged += Item_PropertyChanged;
                //        }
                //    }
                //}
            }
        }

        public TableSetting()
        {
            ColumnSettings = new ObservableCollection<ColumnSetting>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ColumnSetting : INotifyPropertyChanged
    {
        public bool IsNeedReferenceData
        {
            get
            {
                return dataGridColumnType == "DataGridComboBoxColumnExt";
            }
        }

        private ObservableCollection<string> dataGridColumnTypeList;

        public ObservableCollection<string> DataGridColumnTypeList
        {
            get { return dataGridColumnTypeList; }
            set
            {
                if (dataGridColumnTypeList != value)
                {
                    dataGridColumnTypeList = value;
                    OnPropertyChanged();
                }
            }
        }

        private string columnName;

        public string ColumnName
        {
            get { return columnName; }
            set
            {
                if (columnName != value)
                {
                    columnName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string dataGridColumnType;

        public string DataGridColumnType
        {
            get { return dataGridColumnType; }
            set
            {
                if (dataGridColumnType != value)
                {
                    dataGridColumnType = value;
                    if (dataGridColumnType == "DataGridForeignKeyColumn")
                    {
                        IsTabStop = false;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private int width;

        public int Width
        {
            get { return width; }
            set
            {
                if (width != value)
                {
                    width = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool isTabStop;

        public bool IsTabStop
        {
            get { return isTabStop; }
            set
            {
                if (isTabStop != value)
                {
                    isTabStop = value;
                    if (isTabStop == true)
                    {
                        IsReadOnly = false;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private bool isReadOnly;

        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set
            {
                if (isReadOnly != value)
                {
                    isReadOnly = value;
                    if (isReadOnly == true)
                    {
                        IsTabStop = false;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public int OldOrder { get; set; }

        private int order;

        public int Order
        {
            get { return order; }
            set
            {
                if (order != value)
                {
                    OldOrder = order;
                    order = value;
                    OnPropertyChanged();
                }
            }
        }


        private int orderBy;

        public int OrderBy
        {
            get { return orderBy; }
            set
            {
                if (orderBy != value)
                {
                    orderBy = value;
                    OnPropertyChanged();
                }
            }
        }

        private DbTableColumn dbColumn;

        public DbTableColumn DbColumn
        {
            get { return dbColumn; }
            set
            {
                if (dbColumn != value)
                {
                    dbColumn = value;
                    OnPropertyChanged();
                }
            }
        }

        public ColumnSetting()
        {
            dataGridColumnTypeList = new ObservableCollection<string>();
            isTabStop = true;
        }

        public void InitColumnSettingFromDbColumn()
        {
            if (dbColumn == null)
                return;

            if (dbColumn.ColumnName == "TenantID"
                || dbColumn.ColumnName == "CreateTime"
                || dbColumn.ColumnName == "LastUpdateTime")
            {
                IsReadOnly = true;
            }

            if (dbColumn.IsIdentity)
            {
                IsReadOnly = true;
                Width = 80;
                DataGridColumnType = "DataGridTextColumnExt";
                DataGridColumnTypeList.Add("DataGridTextColumnExt");
            }
            else if (dbColumn.IsForeignKey)
            {
                DataGridColumnTypeList.Add("DataGridComboBoxColumnExt");
                DataGridColumnTypeList.Add("DataGridForeignKeyColumn");
                DataGridColumnTypeList.Add("DataGridTextColumnExt");
                DataGridColumnType = "DataGridForeignKeyColumn";
            }
            else
            {
                switch (dbColumn.DataType)
                {
                    case "int":
                    case "int?":
                    case "long":
                    case "long?":
                        DataGridColumnTypeList.Add("DataGridRightAlignTextColumn");
                        DataGridColumnType = "DataGridRightAlignTextColumn";
                        break;
                    case "System.DateTime":
                    case "System.DateTime?":
                        DataGridColumnTypeList.Add("DataGridDateColumn");
                        DataGridColumnType = "DataGridDateColumn";
                        break;
                    case "bool":
                    case "bool?":
                        DataGridColumnTypeList.Add("DataGridCheckBoxColumnExt");
                        DataGridColumnType = "DataGridCheckBoxColumnExt";
                        break;
                    case "System.TimeSpan":
                    case "System.TimeSpan?":
                    case "string":
                        DataGridColumnTypeList.Add("DataGridTextColumnExt");
                        DataGridColumnType = "DataGridTextColumnExt";
                        break;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
