using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using iTunesLyricOverlay.Database;

namespace iTunesLyricOverlay.Windows
{
    public partial class LyricArchiveWindow : Window
    {
        public LyricArchiveWindow()
        {
            this.InitializeComponent();

            App.LyricCollection.CollectionUpdated += this.LyricCollection_CollectionUpdated;
        }

        private void LyricCollection_CollectionUpdated()
        {
            this.Dispatcher.BeginInvoke(new Action(this.UpdateList));
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

        private void UpdateList()
        {
            var lst = App.LyricCollection.FindAll().ToArray();

            foreach (var item in lst)
                Console.WriteLine(item.ToString());

            this.ctlList.ItemsSource = lst;
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
            var dep = (DependencyObject)e.OriginalSource;
            ListViewItem item = null;

            while (dep != null && item == null)
                item = (dep = VisualTreeHelper.GetParent(dep)) as ListViewItem;

            if (item != null)
                this.OpenPreview((LyricArchive)item.Content);
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
        }
    }
}
