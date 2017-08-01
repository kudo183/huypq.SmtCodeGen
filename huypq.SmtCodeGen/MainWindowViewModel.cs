using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace huypq.SmtCodeGen
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public DatabaseTreeVM DatabaseTreeVM { get; set; }

        public MasterDetailSelectorVM MasterDetailSelectorVM { get; set; }

        public TableSettingsVM TableSettingsVM { get; set; }

        private string viewPath;

        public string ViewPath
        {
            get { return viewPath; }
            set
            {
                if (viewPath != value)
                {
                    viewPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private string viewModelPath;

        public string ViewModelPath
        {
            get { return viewModelPath; }
            set
            {
                if (viewModelPath != value)
                {
                    viewModelPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private string textPath;

        public string TextPath
        {
            get { return textPath; }
            set
            {
                if (textPath != value)
                {
                    textPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private string controllerPath;

        public string ControllerPath
        {
            get { return controllerPath; }
            set
            {
                if (controllerPath != value)
                {
                    controllerPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private string dtoPath;

        public string DtoPath
        {
            get { return dtoPath; }
            set
            {
                if (dtoPath != value)
                {
                    dtoPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private string entityPath;

        public string EntityPath
        {
            get { return entityPath; }
            set
            {
                if (entityPath != value)
                {
                    entityPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public IEnumerable<CultureInfo> LanguageNameList { get; set; }

        public ObservableCollection<string> Messages { get; set; }

        public MainWindowViewModel()
        {
            DatabaseTreeVM = new DatabaseTreeVM();
            MasterDetailSelectorVM = new MasterDetailSelectorVM();
            TableSettingsVM = new TableSettingsVM();
            LanguageNameList = CultureInfo.GetCultures(CultureTypes.AllCultures).OrderBy(p => p.DisplayName, System.StringComparer.OrdinalIgnoreCase);
            Messages = new ObservableCollection<string>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SaveJson(string path)
        {
            var json = new JsonViewModel();

            json.DBName = DatabaseTreeVM.DBName;
            json.DbTableList = DatabaseTreeVM.DbTables;

            json.ControllerPath = controllerPath;
            json.DtoPath = dtoPath;
            json.EntityPath = entityPath;
            json.TextPath = textPath;
            json.ViewModelPath = viewModelPath;
            json.ViewPath = viewPath;

            json.MasterDetailList = new List<JsonMasterDetail>();
            foreach (var item in MasterDetailSelectorVM.MasterDetailList)
            {
                var md = new JsonMasterDetail();
                md.ViewName = item.ViewName;
                md.Levels = new List<string>();
                foreach (var level in item.Levels)
                {
                    md.Levels.Add(level);
                }
                json.MasterDetailList.Add(md);
            }

            json.TableSettingList = new List<JsonTableSetting>();
            foreach (var item in TableSettingsVM.TableSettings)
            {
                var ts = new JsonTableSetting();
                ts.ColumnSettingList = new List<JsonColumnSetting>();
                ts.TableName = item.TableName;
                foreach (var column in item.ColumnSettings)
                {
                    ts.ColumnSettingList.Add(new JsonColumnSetting()
                    {
                        ColumnName = column.ColumnName,
                        DataGridColumnType = column.DataGridColumnType,
                        IsReadOnly = column.IsReadOnly,
                        IsTabStop = column.IsTabStop,
                        Width = column.Width,
                        Order = column.Order,
                        OrderBy = column.OrderBy
                    });
                }
                json.TableSettingList.Add(ts);
            }
            
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(json, Newtonsoft.Json.Formatting.Indented);
            System.IO.File.WriteAllText(path, output);
        }

        public void LoadJson(string path)
        {
            var text = System.IO.File.ReadAllText(path);
            var json = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonViewModel>(text);

            DatabaseTreeVM.DBName = json.DBName;
            DatabaseTreeVM.DbTables = json.DbTableList;

            ViewPath = json.ViewPath;
            ViewModelPath = json.ViewModelPath;
            TextPath = json.TextPath;
            ControllerPath = json.ControllerPath;
            DtoPath = json.DtoPath;
            EntityPath = json.EntityPath;

            int masterDetailListCount = json.MasterDetailList.Count;
            var masterDetailList = new List<MasterDetail>(masterDetailListCount);
            for (int i = 0; i < masterDetailListCount; i++)
            {
                int levelsCount = json.MasterDetailList[i].Levels.Count;
                var md = new MasterDetail();
                for (int j = 0; j < levelsCount; j++)
                {
                    md.Levels.Add(json.MasterDetailList[i].Levels[j]);
                }
                md.ViewName = json.MasterDetailList[i].ViewName;
                md.CanDeleteLevel = (levelsCount > 2);
                masterDetailList.Add(md);
            }
            MasterDetailSelectorVM.MasterDetailList = masterDetailList;

            int tableSettingsCount = json.TableSettingList.Count;
            var tableSettings = new ObservableCollection<TableSetting>();
            for (int i = 0; i < tableSettingsCount; i++)
            {
                int columnsCount = json.TableSettingList[i].ColumnSettingList.Count;
                var table = new TableSetting();
                for (int j = 0; j < columnsCount; j++)
                {
                    var column = json.TableSettingList[i].ColumnSettingList[j];
                    table.ColumnSettings.Add(new ColumnSetting()
                    {
                        ColumnName = column.ColumnName,
                        DataGridColumnType = column.DataGridColumnType,
                        IsReadOnly = column.IsReadOnly,
                        IsTabStop = column.IsTabStop,
                        Width = column.Width,
                        Order = column.Order,
                        OrderBy = column.OrderBy
                    });
                }
                table.TableName = json.TableSettingList[i].TableName;
                tableSettings.Add(table);
            }
            TableSettingsVM.TableSettings = tableSettings;
        }

        class JsonViewModel
        {
            public string DBName { get; set; }
            public string ViewPath { get; set; }
            public string ViewModelPath { get; set; }
            public string TextPath { get; set; }
            public string ControllerPath { get; set; }
            public string DtoPath { get; set; }
            public string EntityPath { get; set; }
            public List<JsonMasterDetail> MasterDetailList { get; set; }
            public List<JsonTableSetting> TableSettingList { get; set; }
            public List<DbTable> DbTableList { get; set; }

            public JsonViewModel()
            {
                MasterDetailList = new List<JsonMasterDetail>();
                TableSettingList = new List<JsonTableSetting>();
                DbTableList = new List<DbTable>();
            }
        }
        class JsonMasterDetail
        {
            public string ViewName { get; set; }
            public List<string> Levels { get; set; }
        }
        class JsonTableSetting
        {
            public string TableName { get; set; }
            public List<JsonColumnSetting> ColumnSettingList { get; set; }
        }
        class JsonColumnSetting
        {
            public string ColumnName { get; set; }
            public string DataGridColumnType { get; set; }
            public bool IsReadOnly { get; set; }
            public bool IsTabStop { get; set; }
            public int Width { get; set; }
            public int Order { get; set; }
            public int OrderBy { get; set; }
        }
    }
}
