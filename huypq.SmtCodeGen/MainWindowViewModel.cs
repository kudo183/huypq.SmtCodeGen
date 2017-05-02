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
            fs.Flush();
            fs.Close();
        }
    }
}
