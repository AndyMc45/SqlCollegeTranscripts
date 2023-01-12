using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessFreeData2
{
    internal static class Helper
    {

        // Push first_value onto the stack and push all others down 1 uto max.
        internal static void regitPush(string section, string key, string firstValue, int max)
        {
            string appName = "AccessFreeData";
            int i = 0;
            // Get old first and save new as first value
            string demote_value = Interaction.GetSetting(appName, section, key + "0", "end");
            Interaction.SaveSetting(appName, section, key + "0", firstValue);
            // Continue if not at end or maximum or repeat of firstValue 
            while (demote_value !="end" & i < max & demote_value != firstValue) 
            { 
                i++;
                //get next demote value 
                string next_demote_value = Interaction.GetSetting(appName, section, key + i.ToString().Trim(), "end");
                //push demote_value onto stack
                Interaction.SaveSetting(appName, section, key, demote_value);
                demote_value = next_demote_value;
            }
        }

        //Delete the ith key from the register and move all others forward
        internal static void regitDelete(string section, string key, int i)
        {
            string appName = "AccessFreeData";
            Interaction.DeleteSetting(appName, section, key + i.ToString().Trim());
            string advance_value = Interaction.GetSetting(appName, section, key + i.ToString().Trim(), "end");
            while (advance_value != "end")
            {
                Interaction.SaveSetting(appName, section, key + i.ToString().Trim(), advance_value);
                i++;
                advance_value = Interaction.GetSetting(appName, section, key + i.ToString().Trim(), "end");
            }
        }


    }
}
