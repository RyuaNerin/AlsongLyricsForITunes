using System.IO;
using System.Reflection;
using System.Windows;
using iTunesLyricOverlay.Database;
using LiteDB;

namespace iTunesLyricOverlay
{
    public partial class App : Application
    {
        private static LiteDatabase m_database;
        public static LiteCollection<LyricArchive> LyricCollection;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var asm = Assembly.GetExecutingAssembly();

            CrashReport.Init();

            m_database = new LiteDatabase(Path.GetFileNameWithoutExtension(asm.Location) + ".db");
            m_database.Engine.UserVersion = 0;

            Config.Load(m_database);

            LyricCollection = m_database.GetCollection<LyricArchive>("lyrics");
            LyricCollection.EnsureIndex(le => le.TrackID);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Config.Save();

            m_database.Shrink();
            m_database.Dispose();
        }
    }
}
