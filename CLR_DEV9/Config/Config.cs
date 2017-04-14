﻿using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace CLRDEV9.Config
{
    [DataContract]
    class Settings
    {
        [DataMember]
        public bool EthEnable;
        [DataMember]
        public string Eth;
        [DataMember]
        public EthAPI EthType;
        [DataMember(EmitDefaultValue = false)]
        public ConfigDirectIP DirectConnectionSettings;
        [DataMember(EmitDefaultValue = false)]
        public ConfigSocketIP SocketConnectionSettings;

        [DataMember]
        public bool HddEnable;
        [DataMember]
        public string Hdd;
        [DataMember]
        public int HddSize;

        [OnDeserializing]
        void OnDeserializing(StreamingContext context)
        {
            Init();
        }

        private void Init()
        {
            Hdd = DEV9Header.HDD_DEF;
            HddSize = 8 * 1024;
            Eth = DEV9Header.ETH_DEF;
            EthType = EthAPI.Winsock;
            EthEnable = true;
            HddEnable = false;

            DirectConnectionSettings = new ConfigDirectIP();
            SocketConnectionSettings = new ConfigSocketIP();
        }

        public Settings()
        {
            Init();
        }

        public enum EthAPI : int
        {
            Null = -1,
            Winsock = 0, //Sockets
            Tap = 1,
            WinPcapBridged = 2,
            WinPcapSwitched = 3
        }

        public static void DoConfig(string iniFolderPath, string iniFileName)
        {
            ConfigForm cfgF = new ConfigForm();
            cfgF.iniFolder = iniFolderPath;
            cfgF.ShowDialog();
            cfgF.Dispose();
        }

        public static void SaveConf(string iniFolderPath, string iniFileName)
        {
            iniFolderPath = iniFolderPath.TrimEnd(Path.DirectorySeparatorChar);
            iniFolderPath = iniFolderPath.TrimEnd(Path.AltDirectorySeparatorChar);

            string filePath = iniFolderPath + Path.DirectorySeparatorChar + iniFileName;
            DataContractSerializer ConfSerializer = new DataContractSerializer(typeof(Settings));

            var settings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "\t"
            };

            FileStream fileWriter = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
            using (XmlWriter xmlWriter = XmlWriter.Create(fileWriter, settings))
            {
                ConfSerializer.WriteObject(xmlWriter, DEV9Header.config);
            }
            fileWriter.Close();
        }

        public static void LoadConf(string iniFolderPath, string iniFileName)
        {
            iniFolderPath = iniFolderPath.TrimEnd(Path.DirectorySeparatorChar);
            iniFolderPath = iniFolderPath.TrimEnd(Path.AltDirectorySeparatorChar);

            string filePath = iniFolderPath + Path.DirectorySeparatorChar + iniFileName;

            if (File.Exists(filePath))
            {
                DataContractSerializer ConfSerializer = new DataContractSerializer(typeof(Settings));
                FileStream Reader = new FileStream(filePath, FileMode.Open);

                DEV9Header.config = (Settings)ConfSerializer.ReadObject(Reader);

                Reader.Close();

                //Update from old config
                if (DEV9Header.config.Eth == "winsock")
                {
                    DEV9Header.config.Eth = DEV9Header.ETH_DEF;
                }
                return;
            }

            DEV9Header.config = new Settings();

            SaveConf(iniFolderPath, iniFileName);
        }
    }
}