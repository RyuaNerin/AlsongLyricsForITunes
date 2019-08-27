using System;
using System.Runtime.InteropServices;

namespace iTunesLyricOverlay
{
    internal static class NativeMethods
    {
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_NOACTIVATE  = 0x08000000;
        public const int WS_EX_LAYERED     = 0x00080000;
        public const int GWL_EXSTYLE       = (-20)     ;

        public const int WM_NCHITTEST = 0x0084;

        public const int HTNOWHERE = 0;
        public const int HTCAPTION = 2;
        public const int HTCLOSE   = 20;
        public const int HTCLIENT  = 1;

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
    }
}
