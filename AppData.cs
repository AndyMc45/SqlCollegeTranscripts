using Microsoft.VisualBasic;
using System.Text.Json;

namespace SqlCollegeTranscripts
{
    public class connectionString
    {
        public string comboString { get; set; } // Connection string with all variable for use in comboBox.
        public string server { get; set; }
        public string user { get; set; }
        public string databaseName { get; set; } // MsSql, MySql 
        public string databaseType { get; set; } // MsSql, MySql 
        public bool readOnly { get; set; }

        public connectionString(string comboString, string server, string user, string databaseName, string databaseType, bool readOnly)
        {
            this.server = server;
            this.user = user;
            this.comboString = comboString;
            this.databaseName = databaseName;
            this.databaseType = databaseType;
            this.readOnly = readOnly;
        }
    }

    public static class AppData
    {
        private static string appName = "SqlCollegeTranscripts";

        public static void storeConnectionStringList(List<connectionString> csList)
        {
            // Create List
            List<string> strCsList = new List<string>();
            foreach(connectionString cs in csList) 
            {
                strCsList.Add(JsonSerializer.Serialize<connectionString>(cs));
            }
            // Store list - this first deletes old list and stores new list
            regitStoreList(strCsList, "ConnectionList");
        }

        public static connectionString? GetFirstConnectionStringOrNull()
        {
            string jsonString = Interaction.GetSetting(appName, "ConnectionList", 0.ToString(), "_end");
            if (jsonString == "_end")
            {
                return default;  // This is the way to return null
            }
            connectionString cs = JsonSerializer.Deserialize<connectionString>(jsonString);
            return cs;
        }

        public static List<connectionString> GetConnectionStringList()
        {
            List<string> jsonStringList = regitGetList("ConnectionList");
            List<connectionString> csList = new List<connectionString>();
            foreach (string str in jsonStringList)
            {
                connectionString cs = JsonSerializer.Deserialize<connectionString>(str);
                csList.Add(cs);
            }
            return csList;
        }

        public static bool areEqual(connectionString value1, connectionString value2)
        {
            string v1 = JsonSerializer.Serialize<connectionString>(value1);
            string v2 = JsonSerializer.Serialize<connectionString>(value2);
            if (v1 == v2) { return true; }
            return false;
        }



        public static void SaveKeyValue(string key, string keyValue)
        {
            Interaction.SaveSetting(appName, "SingleValue", key, keyValue);
        }

        public static string GetKeyValue(string key)
        {
            return Interaction.GetSetting(appName, "SingleValue", key, string.Empty);
        }

        private static List<string> regitGetList(string section)
        {
            List<string> strList = new List<string>();
            int i = 0;
            string currentValue = Interaction.GetSetting(appName, section, 0.ToString(), "_end");
            while (currentValue != "_end")
            {
                strList.Add(currentValue);
                i++;
                currentValue = Interaction.GetSetting(appName, section, i.ToString(), "_end");
            }
            return strList;
        }

        private static void regitStoreList(List<string> strList, string section)
        { 
            //Delete old list
            regitDeleteList(section);
            //Store the new list
            int i = 0;
            foreach (string str in strList) 
            {
                Interaction.SaveSetting(appName, section, i.ToString(), str);
                i++;
            }
        }

        private static void regitDeleteList(string section)
        {
            //Interaction.DeleteSetting(appName, section, i.ToString());
            int i = 0;
            string currentValue = string.Empty;
            while (currentValue != "_end")
            {
                currentValue = Interaction.GetSetting(appName, section, i.ToString(), "_end");
                if (currentValue != "_end") { Interaction.DeleteSetting(appName, section, i.ToString()); }
                i++;
            }
        }
    }
}
