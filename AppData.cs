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
        public static void storeConnectionString(connectionString csObject)
        {
            string jsonString = JsonSerializer.Serialize<connectionString>(csObject);
            regitPushOntoList("ConnectionList", jsonString, 9);
        }

        public static connectionString? GetFirstConnectionStringOrNull()
        {
            string appName = "SqlCollegeTranscripts";
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

        public static void DeleteConnectionStringFromList(int i)
        {
            string appName = "SqlCollegeTranscripts";
            regitDeleteFromList("ConnectionList", i);
        }

        public static void SaveKeyValue(string key, string keyValue)
        {
            string appName = "SqlCollegeTranscripts";
            Interaction.SaveSetting(appName, "SingleValue", key, keyValue);
        }

        public static string GetKeyValue(string key)
        {
            string appName = "SqlCollegeTranscripts";
            return Interaction.GetSetting(appName, "SingleValue", key, string.Empty);
        }

        // Push first_value onto the stack and push all others down 1 upto max.
        private static void regitPushOntoList(string section, string firstValue, int max)
        {
            string appName = "SqlCollegeTranscripts";
            // Push everything done 1 spot
            int i = 0;
            string originalValueI = string.Empty;
            string newValueI = firstValue;
            while (newValueI != "_end" && i < max)
            {
                originalValueI = Interaction.GetSetting(appName, section, i.ToString(), "_end");
                Interaction.SaveSetting(appName, section, i.ToString(), newValueI);
                i++;
                newValueI = originalValueI;
            }
        }

        //Delete the ith key from the register and move all others forward
        private static void regitDeleteFromList(string section, int i)
        {
            string appName = "SqlCollegeTranscripts";
            //Interaction.DeleteSetting(appName, section, i.ToString());
            i++;
            string currentValue = Interaction.GetSetting(appName, section, i.ToString(), "_end");
            while (currentValue != "_end")
            {
                Interaction.SaveSetting(appName, section, (i - 1).ToString(), currentValue);
                i++;
                currentValue = Interaction.GetSetting(appName, section, i.ToString(), "_end");
            }
            Interaction.DeleteSetting(appName, section, (i - 1).ToString());

        }
        private static List<string> regitGetList(string section)
        {
            List<string> strList = new List<string>();
            string appName = "SqlCollegeTranscripts";
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

    }
}
