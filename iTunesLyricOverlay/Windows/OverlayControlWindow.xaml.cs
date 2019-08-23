using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace iTunesLyricOverlay.Windows
{
    public partial class OverlayControlWindow : Window
    {
        private IntPtr m_handle;

        public OverlayControlWindow()
        {
            this.InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var owner = this.Owner;
            if (owner != null)
            {
                this.Left = this.Owner.Left + this.Owner.Width - this.Width;
                this.Top = this.Owner.Top;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            this.Owner.Hide();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var helper = new WindowInteropHelper(this);
            helper.EnsureHandle();

            this.m_handle = helper.Handle;

            var exStyle = NativeMethods.GetWindowLong(this.m_handle, NativeMethods.GWL_EXSTYLE);
            NativeMethods.SetWindowLong(this.m_handle, NativeMethods.GWL_EXSTYLE, exStyle | NativeMethods.WS_EX_NOACTIVATE | NativeMethods.WS_EX_LAYERED);

            var hs = HwndSource.FromHwnd(this.m_handle);
            hs.AddHook(this.WndProcHook);
        }

        private IntPtr WndProcHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != NativeMethods.WM_NCHITTEST)
                return IntPtr.Zero;

            var x = (short)(lParam.ToInt32() & 0xFFFF);
            var y = (short)(lParam.ToInt32() >> 16);
            
            if (VisualTreeHelper.HitTest(this, this.PointFromScreen(new Point(x, y))) != null)
            {
                handled = true;
                return new IntPtr(NativeMethods.HTCLIENT);
            }

            return new IntPtr(NativeMethods.HTNOWHERE);
        }

        private void CtlClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CtlMove_Click(object sender, RoutedEventArgs e)
        {
            this.ctlMove.ReleaseMouseCapture();
            this.DragMove();
        }

        private void CtlTrackPrev_Click(object sender, RoutedEventArgs e)
        {
            MainModel.Instance.ITunes.PreviousTrack();
        }

        private void CtlTrackNext_Click(object sender, RoutedEventArgs e)
        {
            MainModel.Instance.ITunes.NextTrack();
        }

        private void CtlTrackPlay_Click(object sender, RoutedEventArgs e)
        {
            MainModel.Instance.ITunes.Play();
        }

        private void CtlTrackStop_Click(object sender, RoutedEventArgs e)
        {
            MainModel.Instance.ITunes.Pause();
        }
    }
}
