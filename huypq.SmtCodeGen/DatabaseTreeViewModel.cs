using huypq.wpf.Utils;
using SimpleDataGrid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace huypq.SmtCodeGen
{
    public class DbTable : BindableObject
    {
        public string TableName { get; set; }

        public ObservableCollection<DbTableColumn> Columns { get; set; }

        public ObservableCollection<ForeignKey> ForeignKeys { get; set; }

        public ObservableCollection<Index> Indexes { get; set; }

        public ObservableCollection<RequiredMaxLength> RequiredMaxLengths { get; set; }

        public ObservableCollection<DefaultValue> DefaultValues { get; set; }

        public ObservableCollection<HasColumnType> HasColumnTypes { get; set; }

        public ObservableCollection<Reference> ReferencesToThisTable { get; set; }

        public int ReferenceLevel { get; set; }

        private bool isSelected = true;
        public bool IsSelected { get { return isSelected; } set { SetField(ref isSelected, value); } }

        private bool isExpanded;
        public bool IsExpanded { get { return isExpanded; } set { SetField(ref isExpanded, value); } }
    }

    public class Reference
    {
        public string PropertyName { get; set; }
        public string ReferenceTableName { get; set; }
    }

    public class HasColumnType
    {
        public string PropertyName { get; set; }
        public string TypeName { get; set; }
    }

    public class RequiredMaxLength
    {
        public string PropertyName { get; set; }
        public bool NeedIsRequired { get; set; }
        public int MaxLength { get; set; }
    }

    public class DefaultValue
    {
        public string PropertyName { get; set; }
        public string Value { get; set; }
    }

    public class Index
    {
        public string PropertyName { get; set; }
        public string IX_Name { get; set; }

        /// <summary>
        /// None = 0, DriPrimaryKey = 1, DriUniqueKey = 2
        /// </summary>
        public int IndexType { get; set; }
    }

    public class ForeignKey
    {
        public string PropertyName { get; set; }
        public string FK_Name { get; set; }
        public string ForeignKeyTableName { get; set; }

        /// <summary>
        /// Restrict = 0, SetNull = 1, Cascade = 2
        /// </summary>
        public int DeleteAction { get; set; }
    }

    public class DbTableColumn
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
        public bool IsIdentity { get; set; }
        public string ForeignKeyTableName { get; set; }
    }

    public class DatabaseTreeVM : BindableObject
    {
        private string dBServer;
        public string DBServer { get { return dBServer; } set { SetField(ref dBServer, value); } }

        private string user;
        public string User { get { return user; } set { SetField(ref user, value); } }

        private string dBName;
        public string DBName { get { return dBName; } set { SetField(ref dBName, value); } }

        private DateTime connectTime;
        public DateTime ConnectTime { get { return connectTime; } set { SetField(ref connectTime, value); } }

        private List<DbTable> dbTables;

        public List<DbTable> DbTables
        {
            get { return dbTables; }
            set
            {
                var temp = new List<DbTable>();
                foreach (var item in value)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                    item.PropertyChanged += Item_PropertyChanged;
                    if (item.IsSelected == true)
                    {
                        temp.Add(item);
                    }
                }
                selectedTables.Reset(temp);
                SetFieldWithoutCheckEqual(ref dbTables, value);
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DbTable.IsSelected))
            {
                var table = sender as DbTable;
                if (table.IsSelected == true)
                {
                    SelectedTables.Add(table);
                }
                else
                {
                    SelectedTables.Remove(table);
                }
            }
        }

        private ObservableCollectionEx<DbTable> selectedTables;
        public ObservableCollectionEx<DbTable> SelectedTables { get { return selectedTables; } set { SetField(ref selectedTables, value); } }

        public DatabaseTreeVM()
        {
            selectedTables = new ObservableCollectionEx<DbTable>();
        }
    }
}
