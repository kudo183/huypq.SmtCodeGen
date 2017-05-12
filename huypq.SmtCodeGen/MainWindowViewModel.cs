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
            LanguageNameList = CultureInfo.GetCultures(CultureTypes.AllCultures).OrderBy(p => p.DisplayName, System.StringComparer.OrdinalIgnoreCase);
            Messages = new ObservableCollection<string>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Save(string path)
        {
            var fs = new System.IO.FileStream(path, System.IO.FileMode.Create);
            var bw = new System.IO.BinaryWriter(fs);
            bw.Write(DatabaseTreeVM.DBName);
            bw.Write(ViewPath);
            bw.Write(ViewModelPath);
            bw.Write(TextPath);
            bw.Write(ControllerPath);
            bw.Write(DtoPath);
            bw.Write(EntityPath);
            bw.Write(MasterDetailSelectorVM.MasterDetailList.Count);
            foreach (var item in MasterDetailSelectorVM.MasterDetailList)
            {
                bw.Write(item.Levels.Count);
                foreach (var level in item.Levels)
                {
                    bw.Write(level);
                }
                bw.Write(item.ViewName);
            }
            fs.Flush();
            fs.Close();
        }

        public void Load(string path)
        {
            var fs = new System.IO.FileStream(path, System.IO.FileMode.Open);
            var br = new System.IO.BinaryReader(fs);
            DatabaseTreeVM.DBName = br.ReadString();
            ViewPath = br.ReadString();
            ViewModelPath = br.ReadString();
            TextPath = br.ReadString();
            ControllerPath = br.ReadString();
            DtoPath = br.ReadString();
            EntityPath = br.ReadString();
            int masterDetailListCount = br.ReadInt32();
            var masterDetailList = new List<MasterDetail>(masterDetailListCount);
            for (int i = 0; i < masterDetailListCount; i++)
            {
                int levelsCount = br.ReadInt32();
                var md = new MasterDetail();
                for (int j = 0; j < levelsCount; j++)
                {
                    md.Levels.Add(br.ReadString());
                }
                md.ViewName = br.ReadString();
                md.CanDeleteLevel = (levelsCount > 2);
                masterDetailList.Add(md);
            }
            MasterDetailSelectorVM.MasterDetailList = masterDetailList;
            fs.Flush();
            fs.Close();
        }
    }
}
