using System;
using System.Text;
using Crestron.SimplSharp;              // For Basic SIMPL# Classes
using Crestron.SimplSharp.CrestronIO;   // For File Classes
using System.Collections.Generic;       // For Collection Classes
using System.Text.RegularExpressions;   // For Regex;
using System.Linq;

namespace RoomConfig
{
    public class Config
    {
        public string RoomName;
        private Dictionary<string, string> Params;
        private Dictionary<string, string> EditParams;
        private string FilePath;

        public UInt16 ParamCount
        {
            get
            {
                return (UInt16)Params.Count;
            }
        }

        public UInt16 EditParamCount
        {
            get
            {
                return (UInt16)EditParams.Count;
            }
        }
        
        public Config()
        {
            
        }

        public void Init(string filePath)
        {
            RoomName = "";
            Params = new Dictionary<string, string>();
            EditParams = new Dictionary<string, string>();
            FilePath = filePath;
            CrestronConsole.Print("System / Room Config... Config Path = {0}\r\n", FilePath);
        }

        public void ParamKeyAdd(string key)
        {
            if (!Params.ContainsKey(key))
            {
                Params.Add(key, "0");
            }
        }

        private string GetFileContents()
        {
            string text;
            if (File.Exists(FilePath))
            {
                StreamReader file = new StreamReader(FilePath);
                text = file.ReadToEnd();
                file.Close();
                return text;
            }
            else
            {
                text = GenerateDefaultFileContents();
                SaveFileContents(text);
            }

            return text;
        }

        private string GenerateDefaultFileContents()
        {
            string text = "Config (Auto Generated)\r\nMike Jobson - Control Designs Software Ltd\r\n";

            if (Params.ContainsKey(@"0d"))
            {
                Params.Remove(@"0d");
            }

            foreach (KeyValuePair<string, string> Param in Params)
            {
                string keyFormat = string.Format(@"[{0}]", Param.Key);
                text = string.Format("{0}\r\n{1}\r\n{2}\r\n", text, keyFormat, Param.Value);
            }

            return text;
        }

        private void SaveFileContents(string fileContents)
        {
            StreamWriter file = new StreamWriter(FilePath, false);
            file.Write(fileContents);
            file.Close();
        }

        public void Load()
        {
            string text = GetFileContents();
            MatchCollection regexMatches = Regex.Matches(text, @"\[([\w]+)\]\s+([^\r\n]*)");
            CrestronConsole.Print("Matches = {0}\r\n", regexMatches.Count);
            foreach (Match match in regexMatches)
            {
                try
                {
                    string key = match.Groups[1].Value;
                    string value = match.Groups[2].Value;
                    Params[key] = value;
                    EditParams[key] = value;
                }
                catch
                {
                    CrestronConsole.Print("Error processing a config parameter... check config syntax?!\r\n");
                }
            }
            CrestronConsole.Print("Params Loaded... ({0})\r\n", Params.Count);
            foreach (KeyValuePair<string, string> param in Params)
            {
                CrestronConsole.Print("  {0}: {1}\r\n", param.Key, param.Value);
            }
            EditParams = EditParams.OrderBy(k => k.Key).ToDictionary(k=>k.Key,k=>k.Value);
        }

        public string ParamGet(string key)
        {
            if (Params.ContainsKey(key))
            {
                string value = Params[key];
                value = value.Replace(@"|", "\r");
                return value;
            }

            return "";
        }

        public void ParamSet(string key, string value)
        {
            Params[key] = value;

            string text = GetFileContents();

            string searchPattern = string.Format(@"\[({0})\]\s+([^\r\n]*)", key);
            string replacement = string.Format("[$1]\r\n{0}", value);

            text = Regex.Replace(text, searchPattern, replacement);

            SaveFileContents(text);
        }

        public string EditParamGet(string key)
        {
            if (EditParams.ContainsKey(key))
            {
                return EditParams[key];
            }

            return "";
        }

        public void EditParamSet(string key, string value)
        {
            EditParams[key] = value;
        }

        public void EditParamsSave()
        {
            string text = GetFileContents();

            foreach (KeyValuePair<string, string> Param in EditParams)
            {
                if (Params.ContainsKey(Param.Key))
                {
                    Params[Param.Key] = Param.Value;

                    string searchPattern = string.Format(@"\[({0})\]\s+([^\r\n]*)", Param.Key);
                    string replacement = string.Format("[$1]\r\n{0}", Param.Value);

                    text = Regex.Replace(text, searchPattern, replacement);
                }
            }

            SaveFileContents(text);
        }

        public string EditParamKeyAtIndex(int index)
        {
            string keyName;

            try
            {
                keyName = EditParams.ElementAt(index).Key;
            }
            catch
            {
                keyName = "";
            }

            return keyName;
        }
    }
}