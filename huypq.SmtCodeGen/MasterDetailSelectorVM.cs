using SimpleDataGrid;
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
    public class MasterDetailSelectorVM : INotifyPropertyChanged
    {
        private ObservableCollectionEx<DbTable> tables;

        public ObservableCollectionEx<DbTable> Tables
        {
            get { return tables; }
            set
            {
                if (tables != value)
                {
                    tables = value;
                    OnPropertyChanged();
                }
            }
        }

        private List<MasterDetail> masterDetailList;

        public List<MasterDetail> MasterDetailList
        {
            get { return masterDetailList; }
            set
            {
                if (masterDetailList != value)
                {
                    masterDetailList = value;
                    OnPropertyChanged();
                }
            }
        }

        public MasterDetailSelectorVM()
        {
            masterDetailList = new List<MasterDetail>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MasterDetail : INotifyPropertyChanged
    {
        private List<string> levels;

        public List<string> Levels
        {
            get { return levels; }
            set
            {
                if (levels != value)
                {
                    levels = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool canDeleteLevel;

        public bool CanDeleteLevel
        {
            get { return canDeleteLevel; }
            set
            {
                if (canDeleteLevel != value)
                {
                    canDeleteLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        private string viewName;

        public string ViewName
        {
            get { return viewName; }
            set
            {
                if (viewName != value)
                {
                    viewName = value;
                    OnPropertyChanged();
                }
            }
        }

        public MasterDetail()
        {
            levels = new List<string>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
