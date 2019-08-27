using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using LiteDB;

namespace iTunesLyricOverlay
{
    public class Config : INotifyPropertyChanged
    {
        public static Config Instance { get; } = new Config();

        private readonly static PropertyInfo[] Properties;
        static Config()
        {
            Properties = typeof(Config).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty);
        }

        private static LiteCollection<Record> RecordCollection;

        public static void Load(LiteDatabase database)
        {
            RecordCollection = database.GetCollection<Record>("config");
            RecordCollection.EnsureIndex(e => e.Name);
            
            foreach (var record in RecordCollection.FindAll())
            {
                var prop = Properties.FirstOrDefault(e => string.Equals(record.Name, e.Name, StringComparison.CurrentCultureIgnoreCase));
                if (prop == null)
                    continue;

                var value = Decode(record.Value, prop);
                if (value == null)
                    continue;

                prop.SetValue(Instance, value);
            }
        }
        public static void Save()
        {
            foreach (var prop in Properties)
            {
                var value = Encode(prop.GetValue(Instance));
                if (value == null)
                    continue;

                RecordCollection.Upsert(new Record
                {
                    Name  = prop.Name,
                    Value = value,
                });
            }
        }

        private static string Encode(object value)
        {
            if (value == null) return null;

            switch (value)
            {
                case bool       v: return v ? "true" : "false";
                case double     v: return v.ToString();
                case string     v: return v;
                case Color      v: return $"{v.A},{v.R},{v.G},{v.B}";
                case FontFamily v: return v.Source;
            }

            return null;
        }
        private static object Decode(string value, PropertyInfo prop)
        {
            if (value == null) return null;

            switch (Type.GetTypeCode(prop.PropertyType))
            {
                case TypeCode.Boolean:
                    return value == "true";

                case TypeCode.Double:
                    if (double.TryParse(value, out var v))
                        return v;
                    break;

                case TypeCode.String:
                    return value;

                default:
                    if (prop.PropertyType == typeof(Color))
                    {
                        var ss = value.Split(',');
                        return Color.FromArgb(byte.Parse(ss[0]), byte.Parse(ss[1]), byte.Parse(ss[2]), byte.Parse(ss[3]));
                    }
                    break;
            }

            return null;
        }

        private class Record
        {
            [BsonId]
            public string Name  { get; set; }
            public string Value { get; set; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = "")
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public Config Clone()
        {
            var newConfig = new Config();

            foreach (var prop in Properties)
                prop.SetValue(newConfig, prop.GetValue(this));

            return newConfig;
        }
        public void CopyFrom(Config config)
        {
            foreach (var prop in Properties)
                prop.SetValue(this, prop.GetValue(config));
        }

        #region Apply Lyrics To iTunes

        private bool m_applyLyricsToITunes;
        public bool ApplyLyricsToITunes
        {
            get => this.m_applyLyricsToITunes;
            set
            {
                this.m_applyLyricsToITunes = value;
                this.OnPropertyChanged();
            }
        }

        private bool m_applyLyricsToITunes_WithTime;
        public bool ApplyLyricsToITunes_WithTime
        {
            get => this.m_applyLyricsToITunes_WithTime;
            set
            {
                this.m_applyLyricsToITunes_WithTime = value;
                this.OnPropertyChanged();
            }
        }

        private bool m_applyLyricsToITunes_WithBlankLine;
        public bool ApplyLyricsToITunes_WithBlankLine
        {
            get => this.m_applyLyricsToITunes_WithBlankLine;
            set
            {
                this.m_applyLyricsToITunes_WithBlankLine = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region Overlay Control

        private bool m_overlayControl_Visible = true;
        public bool OverlayControl_Visible
        {
            get => this.m_overlayControl_Visible;
            set
            {
                this.m_overlayControl_Visible = value;
                this.OnPropertyChanged();
            }
        }

        private bool m_overlayControl_TrackControlVisible = true;
        public bool OverlayControl_TrackControlVisible
        {
            get => this.m_overlayControl_TrackControlVisible;
            set
            {
                this.m_overlayControl_TrackControlVisible = value;
                this.OnPropertyChanged();
            }
        }

        private double m_overlayControl_Opacity = 1d;
        public double OverlayControl_Opacity
        {
            get => this.m_overlayControl_Opacity;
            set
            {
                this.m_overlayControl_Opacity = value;
                this.OnPropertyChanged();
            }
        }

        private double m_overlayControl_Size = 16;
        public double OverlayControl_Size
        {
            get => this.m_overlayControl_Size;
            set
            {
                this.m_overlayControl_Size = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region Overlay Location

        public double m_overlay_Left = 100;
        public double Overlay_Left
        {
            get => this.m_overlay_Left;
            set
            {
                this.m_overlay_Left = value;
                this.OnPropertyChanged();
            }
        }

        public double m_overlay_Top = 100;
        public double Overlay_Top
        {
            get => this.m_overlay_Top;
            set
            {
                this.m_overlay_Top = value;
                this.OnPropertyChanged();
            }
        }

        public double m_overlay_Width = 400;
        public double Overlay_Width
        {
            get => this.m_overlay_Width;
            set
            {
                this.m_overlay_Width = value;
                this.OnPropertyChanged();
            }
        }

        public double m_overlay_Height = 100;
        public double Overlay_Height
        {
            get => this.m_overlay_Height;
            set
            {
                this.m_overlay_Height = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region Overlay

        private double m_overlay_Opacity = 1d;
        public double Overlay_Opacity
        {
            get => this.m_overlay_Opacity;
            set
            {
                this.m_overlay_Opacity = value;
                this.OnPropertyChanged();
            }
        }

        private Color m_overlay_Background = (Color)ColorConverter.ConvertFromString("#55AAAAAA");
        public Color Overlay_Background
        {
            get => this.m_overlay_Background;
            set
            {
                this.m_overlay_Background = value;
                this.OnPropertyChanged();
            }
        }

        private double m_overlay_FontSize = 18;
        public double Overlay_FontSize
        {
            get => this.m_overlay_FontSize;
            set
            {
                this.m_overlay_FontSize = value;
                this.OnPropertyChanged();
            }
        }

        private string m_overlay_FontFamilly = "Malgun Gothic";
        public string Overlay_FontFamily
        {
            get => this.m_overlay_FontFamilly;
            set
            {
                this.m_overlay_FontFamilly = value;
                this.OnPropertyChanged();
            }
        }

        private bool m_overlay_FontBold = true;
        public bool Overlay_FontBold
        {
            get => this.m_overlay_FontBold;
            set
            {
                this.m_overlay_FontBold = value;
                this.OnPropertyChanged();
            }
        }

        private bool m_overlay_FontItalic = false;
        public bool Overlay_FontItalic
        {
            get => this.m_overlay_FontItalic;
            set
            {
                this.m_overlay_FontItalic = value;
                this.OnPropertyChanged();
            }
        }

        private Color m_overlay_FontColor = Colors.White;
        public Color Overlay_FontColor
        {
            get => this.m_overlay_FontColor;
            set
            {
                this.m_overlay_FontColor = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region Main Window

        private double m_mainWindow_FontSize = 14;
        public double MainWindow_FontSize
        {
            get => this.m_mainWindow_FontSize;
            set
            {
                this.m_mainWindow_FontSize = value;
                this.OnPropertyChanged();
            }
        }

        private string m_mainWindow_FontFamilly = "Malgun Gothic";
        public string MainWindow_FontFamily
        {
            get => this.m_mainWindow_FontFamilly;
            set
            {
                this.m_mainWindow_FontFamilly = value;
                this.OnPropertyChanged();
            }
        }

        private bool m_mainWindow_FontBold = false;
        public bool MainWindow_FontBold
        {
            get => this.m_mainWindow_FontBold;
            set
            {
                this.m_mainWindow_FontBold = value;
                this.OnPropertyChanged();
            }
        }

        private bool m_mainWindow_FontItalic = false;
        public bool MainWindow_FontItalic
        {
            get => this.m_mainWindow_FontItalic;
            set
            {
                this.m_mainWindow_FontItalic = value;
                this.OnPropertyChanged();
            }
        }

        private Color m_mainWindow_FontColor = Colors.Gray;
        public Color MainWindow_FontColor
        {
            get => this.m_mainWindow_FontColor;
            set
            {
                this.m_mainWindow_FontColor = value;
                this.OnPropertyChanged();
            }
        }

        
        private double m_mainWindow_FontSizeFocused = 14;
        public double MainWindow_FontSizeFocused
        {
            get => this.m_mainWindow_FontSizeFocused;
            set
            {
                this.m_mainWindow_FontSizeFocused = value;
                this.OnPropertyChanged();
            }
        }

        private bool m_mainWindow_FontBoldFocused = true;
        public bool MainWindow_FontBoldFocused
        {
            get => this.m_mainWindow_FontBoldFocused;
            set
            {
                this.m_mainWindow_FontBoldFocused = value;
                this.OnPropertyChanged();
            }
        }

        private bool m_mainWindow_FontItalicFocused = false;
        public bool MainWindow_FontItalicFocused
        {
            get => this.m_mainWindow_FontItalicFocused;
            set
            {
                this.m_mainWindow_FontItalicFocused = value;
                this.OnPropertyChanged();
            }
        }

        private Color m_mainWindow_FontColorFocused = Colors.Black;
        public Color MainWindow_FontColorFocused
        {
            get => this.m_mainWindow_FontColorFocused;
            set
            {
                this.m_mainWindow_FontColorFocused = value;
                this.OnPropertyChanged();
            }
        }


        private bool m_mainWindow_AutoScroll = true;
        public bool MainWindow_AutoScroll
        {
            get => this.m_mainWindow_AutoScroll;
            set
            {
                this.m_mainWindow_AutoScroll = value;
                this.OnPropertyChanged();
            }
        }

        #endregion
    }
}
