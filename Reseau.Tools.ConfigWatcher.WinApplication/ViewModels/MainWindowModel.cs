using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Reseau.Tools.ConfigWatcher.WinApplication.Annotations;
using Reseau.Tools.ConfigWatcher.WinApplication.FileSystem;

namespace Reseau.Tools.ConfigWatcher.WinApplication.ViewModels
{
    public class MainWindowModel : INotifyPropertyChanged
    {

        private string _workingPath;
        private string _console;

        public event PropertyChangedEventHandler PropertyChanged;


        [NotNull]
        public String WorkingPath
        {
            get { return _workingPath; }
            set
            {
                _workingPath = value;
                RiseChangedProperty();
            }
        }

        [NotNull]
        public String Console
        {
            get { return _console; }
            set
            {
                _console = value;
                RiseChangedProperty();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void RiseChangedProperty([NotNull,CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void PrependToConsole([NotNull] string message)
        {
            Console = new StringBuilder(message)
                .AppendLine(Console)
                .ToString();
        }

        public async void RearrangeConfigs()
        {
            try
            {
                var configRearranger = new ConfigRearranger(WorkingPath);
                var log = await configRearranger.Rearrange();
                
                var message = log.Length == 0 
                    ? "[???] Nothing to do in current folder" + Environment.NewLine
                    : log.ToString();

                PrependToConsole(message);
            }
            catch (Exception e)
            {
                var stringBuilder = new StringBuilder("[ERROR] ").AppendLine(e.Message);
                PrependToConsole(stringBuilder.ToString());
            }

            
        }
    }
}