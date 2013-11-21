using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using Windows.UI;
using Windows.UI.Xaml;

namespace Bubbling
{
    public class BubblingSettings
    {
        static BubblingSettings()
        {
            Instance = Load();
        }
        public static BubblingSettings Instance { get; set; }
        public void Save()
        {/*
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream stream = null;
            try
            {
                stream = storage.CreateFile(AppSettingsFileName);

                XmlSerializer xml = new XmlSerializer(GetType());
                xml.Serialize(stream, this);
            }
            catch (Exception ex)
            { }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }*/
        }
        private static BubblingSettings Load()
        {/*
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            BubblingSettings tmpSettings;

            if (storage.FileExists(AppSettingsFileName))
            {
                IsolatedStorageFileStream stream = null;
                try
                {
                    stream = storage.OpenFile(AppSettingsFileName, FileMode.Open);
                    XmlSerializer xml = new XmlSerializer(typeof(BubblingSettings));

                    tmpSettings = xml.Deserialize(stream) as BubblingSettings;
                }
                catch (Exception ex)
                {
                    tmpSettings = new BubblingSettings();
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                        stream.Dispose();
                    }
                }
            }
            else
            {
                tmpSettings = new BubblingSettings();
            }

            return tmpSettings;*/
            return new BubblingSettings();
        }

        protected static string AppSettingsFileName = "settings";
        private int _bubbles = 500;
        private Color _color = Colors.Blue;
        private DateTime _firstUse = DateTime.Now;

        public int Bubbles
        {
            get { return _bubbles; }
            set { _bubbles = value; }
        }

        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public DateTime FirstUse
        {
            get { return _firstUse; }
            set { _firstUse = value; }
        }
    }
}