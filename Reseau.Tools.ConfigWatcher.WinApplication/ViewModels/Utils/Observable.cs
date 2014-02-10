using System.ComponentModel;
using System.Runtime.CompilerServices;
using Reseau.Tools.ConfigWatcher.WinApplication.Annotations;

namespace Reseau.Tools.ConfigWatcher.WinApplication.ViewModels.Utils
{
    public interface IObservable
    {
        [NotifyPropertyChangedInvocator]
        void RiseChangedProperty([ CallerMemberName] string propertyName = null);
    }

    public class Observable: INotifyPropertyChanged, IObservable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void RiseChangedProperty([CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) return;

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}