﻿using Microsoft.Win32;
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

        public MainWindow()
        {
            InitializeComponent();

            DataContext = vm;

            vm.DatabaseTreeVM.DBName = Properties.Settings.Default.DbName;
            vm.EntityPath = Properties.Settings.Default.EntityPath;
            vm.ControllerPath = Properties.Settings.Default.ControllerPath;
            vm.DtoPath = Properties.Settings.Default.DtoPath;
            vm.ViewPath = Properties.Settings.Default.ViewPath;
            vm.ViewModelPath = Properties.Settings.Default.ViewModelPath;
            vm.TextPath = Properties.Settings.Default.TextPath;

            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.DbName = vm.DatabaseTreeVM.DBName;
            Properties.Settings.Default.EntityPath = vm.EntityPath;
            Properties.Settings.Default.ControllerPath = vm.ControllerPath;
            Properties.Settings.Default.DtoPath = vm.DtoPath;
            Properties.Settings.Default.ViewPath = vm.ViewPath;
            Properties.Settings.Default.ViewModelPath = vm.ViewModelPath;
            Properties.Settings.Default.TextPath = vm.TextPath;

            Properties.Settings.Default.Save();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "CodeGen Settings|*.gen";
            if (sfd.ShowDialog() == true)
            {
                vm.Save(sfd.FileName);
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "CodeGen Settings|*.gen";
            if (ofd.ShowDialog() == true)
            {
                vm.Load(ofd.FileName);
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
                case "Text":
                    GenText(vm.TextPath);
                    break;
                case "Controller":
                    GenController(vm.ControllerPath);
                    break;
                case "Dto":
                    GenDto(vm.DtoPath);
                    break;
                case "Entity":
                    GenEntity(vm.EntityPath);
                    break;
                case "All":
                    GenAllCode();
                    break;
            }

            vm.Messages.Add(string.Format("{0} | Done.", DateTime.Now));
        }

        private void GenAllCode()
        {
            GenView(vm.ViewPath);
            GenViewModel(vm.ViewModelPath);
            GenText(vm.TextPath);
            GenController(vm.ControllerPath);
            GenDto(vm.DtoPath);
            GenEntity(vm.EntityPath);
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
                case "Text":
                    OpenPath(vm.TextPath);
                    break;
                case "Controller":
                    OpenPath(vm.ControllerPath);
                    break;
                case "Dto":
                    OpenPath(vm.DtoPath);
                    break;
                case "Entity":
                    OpenPath(vm.EntityPath);
                    break;
            }
        }

        private void OpenPath(string path)
        {
            System.Diagnostics.Process.Start(path);
        }

        private void GenView(string path)
        {
            vm.Messages.Add(string.Format("{0} | Generating View ...", DateTime.Now));

            ViewCodeGenerator.GenViewCode(vm.DatabaseTreeVM.SelectedTables, path);
            ViewCodeGenerator.GenViewXamlCode(vm.DatabaseTreeVM.SelectedTables, path);
        }

        private void GenViewModel(string path)
        {
            vm.Messages.Add(string.Format("{0} | Generating ViewModel ...", DateTime.Now));

            ViewModelCodeGenerator.GenViewModelCode(vm.DatabaseTreeVM.SelectedTables, path);
        }

        private void GenText(string path)
        {
            vm.Messages.Add(string.Format("{0} | Generating Text ...", DateTime.Now));

            TextManagerCodeGenerator.GenTextManagerCode(vm.DatabaseTreeVM.SelectedTables, path);
        }

        private void GenController(string path)
        {
            vm.Messages.Add(string.Format("{0} | Generating Controller ...", DateTime.Now));

            ControllersCodeGenerator.GenControllersClass(vm.DatabaseTreeVM.SelectedTables, path);
        }

        private void GenDto(string path)
        {
            vm.Messages.Add(string.Format("{0} | Generating Dto ...", DateTime.Now));

            DtosCodeGenerator.GenDtosClass(vm.DatabaseTreeVM.SelectedTables, path);
        }

        private void GenEntity(string path)
        {
            vm.Messages.Add(string.Format("{0} | Generating Entity ...", DateTime.Now));

            EntitiesCodeGenerator.GenDbContextClass(vm.DatabaseTreeVM.SelectedTables, path);
            EntitiesCodeGenerator.GenEntitiesClass(vm.DatabaseTreeVM.SelectedTables, path);
        }
    }
}
