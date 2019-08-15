using System;
using System.Threading.Tasks;
using System.Windows;

namespace iTunesLyricOverlay
{
    internal static class CrashReport
    {
        public static void Init()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) => Error(e.ExceptionObject as Exception);
            TaskScheduler.UnobservedTaskException += (s, e) => Error(e.Exception);
            Application.Current.DispatcherUnhandledException += (s, e) => Error(e.Exception);
            Application.Current.Dispatcher.UnhandledException += (s, e) => Error(e.Exception);
        }

        public static void Error(Exception ex)
        {
        }
    }
}
