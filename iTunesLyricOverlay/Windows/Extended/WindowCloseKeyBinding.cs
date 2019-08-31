using System.Windows;
using System.Windows.Input;

namespace iTunesLyricOverlay.Windows.Extended
{
    public class WindowCloseKeyBinding : KeyBinding
    {
        public static ICommand ICommand { get; } = new RelayCommand(o => ((Window)o).Close());

        public WindowCloseKeyBinding()
            : base(ICommand, Key.Escape, ModifierKeys.None)
        {
        }
    }
}
