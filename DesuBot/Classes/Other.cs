using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesuBot.Classes
{
    public class Other
    {
        public static void LoadAndFixLogs(long groupId)
        {
            if (File.Exists(ParametersClass.logsPath))
            {
                ParametersClass.logs = IOClass.ReadLogsJson();
                List<Logs> UpdateLogs = new List<Logs>();

                PostInfo[] posts = VkApiClass.GetAllPostponed(groupId).ToArray();

                for (int i = 0; i < ParametersClass.logs.Count; i++)
                {
                    for (int p = 0; p < posts.Length; p++)
                    {
                        if (ParametersClass.logs[i].groupId == posts[p].groupId)
                        {
                            if (ParametersClass.logs[i].postId == posts[p].postId)
                            {
                                UpdateLogs.Add(ParametersClass.logs[i]);
                            }
                        }
                    }
                }
                ParametersClass.logs = UpdateLogs;
            }
        }

        public static void PostsBack(long groupId, int localGroupId)
        {
            LoadAndFixLogs(groupId);

            Random rnd = new Random();
            for (int i = 0; i < ParametersClass.logs.Count; i++)
            {
                if (File.Exists(ParametersClass.logs[i].file))
                {
                    string[] splitter = Path.GetFileName(ParametersClass.logs[i].file).Split('_');
                    var delete = ParametersClass.vkapi.Wall.Delete(-groupId, ParametersClass.logs[i].postId);

                    string file = ParametersClass.groups[localGroupId].groupItem.ForPublic + "\\" + "backed_" + rnd.Next(0, 11) + "_" + splitter[splitter.Length - 1];
                    if (delete)
                        File.Move(ParametersClass.logs[i].file, file);
                }
            }

            LoadAndFixLogs(groupId);
            IOClass.SaveLogsJson(ParametersClass.logs);
            MessageBox.Show("Посты успешно откачены!");
        }
    }
}
