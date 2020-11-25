using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GOT.SharedKernel
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        public virtual string Title => string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}