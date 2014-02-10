using System;
using System.Windows;
using Ookii.Dialogs.Wpf;
using Reseau.Tools.ConfigWatcher.WinApplication.Annotations;
using Reseau.Tools.ConfigWatcher.WinApplication.ViewModels;

namespace Reseau.Tools.ConfigWatcher.WinApplication
{
    public partial class MainWindow
    {
        private static readonly MainWindowModel Model = new MainWindowModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = Model;
        }

        private void OnGotFocus([NotNull] object sender,
                                [NotNull] RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if(dialog.ShowDialog() != true) return;

            Model.WorkingPath = dialog.SelectedPath;
        }

        private void OnRearrangeClick([NotNull] object sender,
                                      [NotNull] RoutedEventArgs e)
        {
            Model.RearrangeConfigs();
        }
    }
}
