using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace huypq.SmtCodeGen
{
    /// <summary>
    /// Interaction logic for MasterDetailSelector.xaml
    /// </summary>
    public partial class MasterDetailSelector : UserControl
    {
        public MasterDetailSelector()
        {
            InitializeComponent();
        }

        public void AddView()
        {
            var vm = DataContext as MasterDetailSelectorVM;
            var viewIndex = vm.MasterDetailList.Count;

            var spMasterDetail = CreateMasterDetailStackPanel(viewIndex);
            spMasterDetail.Children.Add(CreateLevelStackPanel(viewIndex, 0));
            spMasterDetail.Children.Add(CreateRightArrowSymbolTextBlock());
            spMasterDetail.Children.Add(CreateLevelStackPanel(viewIndex, 1));
            spRoot.Children.Add(spMasterDetail);

            var md = new MasterDetail();
            md.Levels.Add(string.Empty);
            md.Levels.Add(string.Empty);
            vm.MasterDetailList.Add(md);
        }

        public void UpdateUI()
        {
            spRoot.Children.Clear();
            var vm = DataContext as MasterDetailSelectorVM;
            for (var viewIndex = 0; viewIndex < vm.MasterDetailList.Count; viewIndex++)
            {
                var spMasterDetail = CreateMasterDetailStackPanel(viewIndex);
                spMasterDetail.Children.Add(CreateLevelStackPanel(viewIndex, 0));
                for (var levelIndex = 1; levelIndex < vm.MasterDetailList[viewIndex].Levels.Count; levelIndex++)
                {
                    spMasterDetail.Children.Add(CreateRightArrowSymbolTextBlock());
                    spMasterDetail.Children.Add(CreateLevelStackPanel(viewIndex, levelIndex));
                }
                spRoot.Children.Add(spMasterDetail);
            }
        }

        private StackPanel CreateMasterDetailStackPanel(int viewIndex)
        {
            var spMasterDetail = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            var btnDelete = new Button()
            {
                Content = "x"
            };
            btnDelete.Click += DeleteViewButton_Click;

            var txtViewName = new TextBox()
            {
                Width = 80,
                FontWeight = FontWeights.Bold
            };
            txtViewName.SetBinding(TextBox.TextProperty, new Binding(string.Format("MasterDetailList[{0}].ViewName", viewIndex)));

            spMasterDetail.Children.Add(btnDelete);
            spMasterDetail.Children.Add(txtViewName);
            return spMasterDetail;
        }

        private TextBlock CreateRightArrowSymbolTextBlock()
        {
            return new TextBlock()
            {
                Text = "➡",
                Margin = new Thickness(5, 0, 5, 0)
            };
        }

        private StackPanel CreateLevelStackPanel(int viewIndex, int levelIndex)
        {
            var btnDelete = new Button()
            {
                Content = "x"
            };
            btnDelete.SetBinding(Button.IsEnabledProperty, new Binding(string.Format("MasterDetailList[{0}].CanDeleteLevel", viewIndex)));
            btnDelete.Click += DeleteLevelButton_Click;

            var btnAdd = new Button()
            {
                Content = "+"
            };
            btnAdd.Click += AddLevelBtn_Click;

            var combobox = new ComboBox()
            {
                IsEditable = false
            };

            combobox.SetBinding(ComboBox.ItemsSourceProperty, new Binding("Tables"));
            combobox.SetBinding(ComboBox.SelectedValueProperty, new Binding(string.Format("MasterDetailList[{0}].Levels[{1}]", viewIndex, levelIndex)));

            var stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            stackPanel.Children.Add(btnDelete);
            stackPanel.Children.Add(combobox);
            stackPanel.Children.Add(btnAdd);

            return stackPanel;
        }

        private void DeleteViewButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var viewSp = btn.Parent as StackPanel;
            int viewIndex = -1;
            for (int i = 0; i < spRoot.Children.Count; i++)
            {
                if (spRoot.Children[i] == viewSp)
                {
                    viewIndex = i;
                    break;
                }
            }
            spRoot.Children.RemoveAt(viewIndex);

            var vm = DataContext as MasterDetailSelectorVM;
            vm.MasterDetailList.RemoveAt(viewIndex);
        }

        private void AddLevelBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            int viewIndex, levelIndex, stackPanelIndex;

            var viewSp = FindDeleteLevelButtonInex(btn, out viewIndex, out levelIndex, out stackPanelIndex);

            var vm = DataContext as MasterDetailSelectorVM;
            vm.MasterDetailList[viewIndex].Levels.Insert(levelIndex + 1, string.Empty);
            vm.MasterDetailList[viewIndex].CanDeleteLevel = (vm.MasterDetailList[viewIndex].Levels.Count > 2);
            viewSp.Children.Insert(stackPanelIndex + 1, CreateRightArrowSymbolTextBlock());
            viewSp.Children.Insert(stackPanelIndex + 2, CreateLevelStackPanel(viewIndex, levelIndex + 1));
        }

        private void DeleteLevelButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            int viewIndex, levelIndex, stackPanelIndex;

            var viewSp = FindDeleteLevelButtonInex(btn, out viewIndex, out levelIndex, out stackPanelIndex);

            var vm = DataContext as MasterDetailSelectorVM;
            vm.MasterDetailList[viewIndex].Levels.RemoveAt(levelIndex);
            vm.MasterDetailList[viewIndex].CanDeleteLevel = (vm.MasterDetailList[viewIndex].Levels.Count > 2);
            viewSp.Children.RemoveAt(stackPanelIndex);
            viewSp.Children.RemoveAt(stackPanelIndex);
        }

        private StackPanel FindDeleteLevelButtonInex(Button btn, out int viewIndex, out int levelIndex, out int stackPanelIndex)
        {
            var levelSp = btn.Parent as StackPanel;
            var viewSp = levelSp.Parent as StackPanel;
            int additionElementCount = 2;/*btnDelete & txtViewName*/
            viewIndex = -1;
            for (int i = 0; i < spRoot.Children.Count; i++)
            {
                if (spRoot.Children[i] == viewSp)
                {
                    viewIndex = i;
                    break;
                }
            }
            levelIndex = -1;
            for (int i = 0; i < viewSp.Children.Count; i++)
            {
                if (viewSp.Children[i] == levelSp)
                {
                    levelIndex = (i - additionElementCount) / 2;
                    break;
                }
            }
            stackPanelIndex = (levelIndex * 2) + additionElementCount;
            return viewSp;
        }
    }
}
