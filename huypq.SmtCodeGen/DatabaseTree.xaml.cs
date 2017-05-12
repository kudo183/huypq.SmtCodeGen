using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace huypq.SmtCodeGen
{
    /// <summary>
    /// Interaction logic for DatabaseTree.xaml
    /// </summary>
    public partial class DatabaseTree : UserControl
    {
        public DatabaseTree()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as DatabaseTreeVM;
            vm.DbTables = new List<DbTable>(DatabaseUtils.FromDB(vm.DBName));
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as DatabaseTreeVM;
            foreach (var table in vm.DbTables)
            {
                table.IsSelected = true;
            }
        }

        private void btnDeselectAll_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as DatabaseTreeVM;
            foreach (var table in vm.DbTables)
            {
                table.IsSelected = false;
            }
        }

        private void btnToogleSelect_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as DatabaseTreeVM;
            foreach (var table in vm.DbTables)
            {
                table.IsSelected = !table.IsSelected;
            }
        }

        private void btnExpandAll_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as DatabaseTreeVM;
            foreach (var table in vm.DbTables)
            {
                table.IsExpanded = true;
            }
        }

        private void btnCollapseAll_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as DatabaseTreeVM;
            foreach (var table in vm.DbTables)
            {
                table.IsExpanded = false;
            }
        }
    }
}
