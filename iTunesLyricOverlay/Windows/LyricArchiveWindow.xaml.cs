using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using iTunesLyricOverlay.Database;

namespace iTunesLyricOverlay.Windows
{
    public partial class LyricArchiveWindow : Window
    {
        public LyricArchiveWindow()
        {
            this.InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        public new void Show()
        {
            this.UpdateList();
            base.Show();
        }

        private void ctlRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.UpdateList();
        }

        private void UpdateList()
        {
            this.ctlList.ItemsSource = App.LyricCollection.FindAll().ToArray();
        }

        private void cmdOpen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ctlList.SelectedItems.Count == 1;
        }
        private void cmdOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.OpenPreview((LyricArchive)this.ctlList.SelectedItem);
        }
        private void CtlList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.OpenPreview((LyricArchive)this.ctlList.SelectedItem);
        }
        private void OpenPreview(LyricArchive archive)
        {
            var win = new LyricArchiveViewWindow(archive);
            win.Owner = this;
            win.ShowDialog();
        }

        private void cmdDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ctlList.SelectedItems.Count > 0;
        }
        private void cmdDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show(this, "정말 저장된 가사를 삭제할까요?", this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                return;

            foreach (var item in this.ctlList.SelectedItems)
                App.LyricCollection.Delete(((LyricArchive)item).LyricArchiveId);

            this.UpdateList();
        }
    }
}
