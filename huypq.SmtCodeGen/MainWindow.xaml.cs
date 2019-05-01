using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace huypq.SmtCodeGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel vm = new MainWindowViewModel();
        string defaultSaveFileName = "default.gen";

        public MainWindow()
        {
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata()
            {
                DefaultValue = System.Windows.Markup.XmlLanguage.GetLanguage(System.Threading.Thread.CurrentThread.CurrentCulture.Name)
            });

            InitializeComponent();

            DataContext = vm;

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            vm.MasterDetailSelectorVM.Tables = vm.DatabaseTreeVM.SelectedTables;
            vm.TableSettingsVM.Tables = vm.DatabaseTreeVM.SelectedTables;
            if (System.IO.File.Exists(defaultSaveFileName) == true)
            {
                vm.LoadJson(defaultSaveFileName);
                masterDetailSelector.UpdateUI();
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            vm.SaveJson(defaultSaveFileName);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "CodeGen Settings|*.gen";
            if (sfd.ShowDialog() == true)
            {
                vm.SaveJson(sfd.FileName);
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "CodeGen Settings|*.gen";
            if (ofd.ShowDialog() == true)
            {
                vm.LoadJson(ofd.FileName);
                masterDetailSelector.UpdateUI();
            }
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null)
                return;

            switch (btn.Tag.ToString())
            {
                case "View":
                    GenView(vm.ViewPath);
                    break;
                case "ViewModel":
                    GenViewModel(vm.ViewModelPath);
                    break;
                case "DataModel":
                    GenDataModel(vm.DataModelPath);
                    break;
                case "Text":
                    GenText(vm.TextPath);
                    break;
                case "Dto":
                    GenDto(vm.DtoPath);
                    break;
                case "Controller":
                    GenController(vm.ControllerPath);
                    break;
                case "Entity":
                    GenEntity(vm.EntityPath);
                    break;
                case "Angular2":
                    GenAngular2(vm.Angular2Path);
                    break;
                case "ComplexView":
                    GenComplexView(vm.ViewPath);
                    break;
                case "All":
                    GenAllCode();
                    break;
                case "Client":
                    GenView(vm.ViewPath);
                    GenViewModel(vm.ViewModelPath);
                    GenDataModel(vm.DataModelPath);
                    GenText(vm.TextPath);
                    GenDto(vm.DtoPath);
                    GenComplexView(vm.ViewPath);
                    break;
                case "Server":
                    GenDto(vm.DtoPath);
                    GenController(vm.ControllerPath);
                    GenEntity(vm.EntityPath);
                    break;
            }

            vm.Messages.Add(string.Format("{0} | Done.", DateTime.Now));
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            masterDetailSelector.AddView();
        }

        private void ResetTableSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            vm.TableSettingsVM.Reset();
        }

        private void ResetSelectedTableSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            vm.TableSettingsVM.ResetSelectedTableSetting();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null)
                return;

            switch (btn.Tag.ToString())
            {
                case "View":
                    OpenPath(vm.ViewPath);
                    break;
                case "ViewModel":
                    OpenPath(vm.ViewModelPath);
                    break;
                case "DataModel":
                    OpenPath(vm.DataModelPath);
                    break;
                case "Text":
                    OpenPath(vm.TextPath);
                    break;
                case "Dto":
                    OpenPath(vm.DtoPath);
                    break;
                case "Controller":
                    OpenPath(vm.ControllerPath);
                    break;
                case "Entity":
                    OpenPath(vm.EntityPath);
                    break;
                case "Angular2":
                    OpenPath(vm.Angular2Path);
                    break;
            }
        }

        private void GenAllCode()
        {
            GenView(vm.ViewPath);
            GenViewModel(vm.ViewModelPath);
            GenDataModel(vm.DataModelPath);
            GenText(vm.TextPath);
            GenDto(vm.DtoPath);
            GenController(vm.ControllerPath);
            GenEntity(vm.EntityPath);
            GenComplexView(vm.ViewPath);
            GenAngular2(vm.Angular2Path);
        }

        private void OpenPath(string path)
        {
            vm.Messages.Add(string.Format("{0} | OpenPath: {1} ...", DateTime.Now, path));
            try
            {
                System.Diagnostics.Process.Start(path);
            }
            catch (Exception ex)
            {
                vm.Messages.Add(string.Format("{0} | Exception:  {1}", DateTime.Now, ex.Message));
            }
        }

        private void GenView(string path)
        {
            vm.Messages.Add(string.Format("{0} | Generating View ...", DateTime.Now));
            try
            {
                ViewCodeGenerator.GenViewCode(vm.TableSettingsVM.TableSettings, path);
                vm.Messages.Add(string.Format("{0} | Done", DateTime.Now));
            }
            catch (Exception ex)
            {
                vm.Messages.Add(string.Format("{0} | Exception:  {1}", DateTime.Now, ex.Message));
            }
        }

        private void GenViewModel(string path)
        {
            vm.Messages.Add(string.Format("{0} | Generating ViewModel ...", DateTime.Now));
            try
            {
                ViewModelCodeGenerator.GenViewModelCode(vm.TableSettingsVM.TableSettings, path);
                vm.Messages.Add(string.Format("{0} | Done", DateTime.Now));
            }
            catch (Exception ex)
            {
                vm.Messages.Add(string.Format("{0} | Exception:  {1}", DateTime.Now, ex.Message));
            }
        }

        private void GenDataModel(string path)
        {
            vm.Messages.Add(string.Format("{0} | Generating DataModel ...", DateTime.Now));
            try
            {
                DataModelCodeGenerator.GenDataModelClass(vm.TableSettingsVM.TableSettings, path);
                vm.Messages.Add(string.Format("{0} | Done", DateTime.Now));
            }
            catch (Exception ex)
            {
                vm.Messages.Add(string.Format("{0} | Exception:  {1}", DateTime.Now, ex.Message));
            }
        }

        private void GenText(string path)
        {
            vm.Messages.Add(string.Format("{0} | Generating Text ...", DateTime.Now));
            try
            {
                TextManagerCodeGenerator.GenTextManagerCode(vm.TableSettingsVM.TableSettings, path);
                vm.Messages.Add(string.Format("{0} | Done", DateTime.Now));
            }
            catch (Exception ex)
            {
                vm.Messages.Add(string.Format("{0} | Exception:  {1}", DateTime.Now, ex.Message));
            }
        }

        private void GenDto(string path)
        {
            vm.Messages.Add(string.Format("{0} | Generating Dto ...", DateTime.Now));
            try
            {
                DtosCodeGenerator.GenDtosClass(vm.TableSettingsVM.TableSettings, path);
                vm.Messages.Add(string.Format("{0} | Done", DateTime.Now));
            }
            catch (Exception ex)
            {
                vm.Messages.Add(string.Format("{0} | Exception:  {1}", DateTime.Now, ex.Message));
            }
        }

        private void GenController(string path)
        {
            vm.Messages.Add(string.Format("{0} | Generating Controller ...", DateTime.Now));
            try
            {
                ControllersCodeGenerator.GenControllersClass(vm.TableSettingsVM.TableSettings, path);
                vm.Messages.Add(string.Format("{0} | Done", DateTime.Now));
            }
            catch (Exception ex)
            {
                vm.Messages.Add(string.Format("{0} | Exception:  {1}", DateTime.Now, ex.Message));
            }
        }

        private void GenEntity(string path)
        {
            vm.Messages.Add(string.Format("{0} | Generating Entity ...", DateTime.Now));
            try
            {
                EntitiesCodeGenerator.GenDbContextAndEntitiesClass(vm.TableSettingsVM.TableSettings, path);
                vm.Messages.Add(string.Format("{0} | Done", DateTime.Now));
            }
            catch (Exception ex)
            {
                vm.Messages.Add(string.Format("{0} | Exception:  {1}", DateTime.Now, ex.Message));
            }
        }

        private void GenComplexView(string path)
        {
            vm.Messages.Add(string.Format("{0} | Generating Complex View ...", DateTime.Now));
            try
            {
                ComplexViewCodeGenerator.GenComplexViewCode(vm.MasterDetailSelectorVM.MasterDetailList, vm.TableSettingsVM.TableSettings, path);
                vm.Messages.Add(string.Format("{0} | Done", DateTime.Now));
            }
            catch (Exception ex)
            {
                vm.Messages.Add(string.Format("{0} | Exception:  {1}", DateTime.Now, ex.Message));
            }
        }

        private void GenAngular2(string path)
        {
            vm.Messages.Add(string.Format("{0} | Generating Angular 2 ...", DateTime.Now));
            try
            {
                Angular2HtmlCodeGenerator.GenAngular2Html(vm.TableSettingsVM.TableSettings, path);
                Angular2TSCodeGenerator.GenAngular2TS(vm.TableSettingsVM.TableSettings, path);
                vm.Messages.Add(string.Format("{0} | Done", DateTime.Now));
            }
            catch (Exception ex)
            {
                vm.Messages.Add(string.Format("{0} | Exception:  {1}", DateTime.Now, ex.Message));
            }
        }
    }
}
