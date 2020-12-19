using System.Collections.Generic;
using System.Windows.Forms;
using VkNet;
using VkNet.Enums.Filters;

namespace DesuBot.Classes
{
    class ParametersClass
    {
        public static ulong appID = 6648036;

        public static VkApi vkapi = new VkApi();
        public static Settings settings = Settings.All;
        public static string GlobalJson = Application.StartupPath + "\\Json" + "\\GlobalJson.json";
        public static List<string> file = new List<string>();
        public static List<Group> groups = new List<Group>();

        public static bool autch = false;

        public static List<Logs> logs = new List<Logs>();
        public static string logsPath = Application.StartupPath + "\\Json" + "\\Logs.json";

        public static string token;
        public static string ip;

        public static bool AutostartValue = false;

        public static Timer timertoautopost;

        public static string getPathToSave(string FileName)
        {
            return Application.StartupPath + "\\Json" + "\\groups" + "\\" + FileName + ".json";
        }
    }
}
