using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace DesuBot.Classes
{
    public class PostInfo
    {
        public int groupId { get; set; }
        public int postId { get; set; }
    }

    public class Logs
    {
        public string file { get; set; }
        public int groupId { get; set; }
        public long postId { get; set; }
    }

    public class GroupItem //Переменные для сохранения в json
    {
        public int WallId { get; set; }
        public string ForPublic { get; set; }
        public string Public { get; set; }
        public string Hesh { get; set; }
        public int Yer { get; set; }
        public int Mounth { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int PostSpace { get; set; }
        public int ImageInPost { get; set; }
        public bool AutostartValue { get; set; }
        public bool groupStatus { get; set; }
        public int MaxPostInAuto { get; set; }
    }

    public class GlobalParametersJson //Переменные для сохранения в json
    {
        public string LoginT { get; set; }
        public string PassT { get; set; }
        public string Token { get; set; }
        public string Ip { get; set; }
        public bool AutoStart { get; set; }
    }

    class IOClass
    {
        public static void SaveLogsJson(List<Logs> logs) //Сохранение json
        {
            try
            {
                using (StreamWriter writer = File.CreateText(ParametersClass.logsPath))
                {
                    new JsonSerializer().Serialize(writer, logs.ToArray());
                    writer.Close();
                    writer.Dispose();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Неудалось сохранить файл. error: SLJ1");
                MessageBox.Show(e.ToString());
            }
        }

        public static List<Logs> ReadLogsJson() //Загрузка сохранение json
        {
            try
            {
                using (StreamReader reader = File.OpenText(ParametersClass.logsPath))
                {
                    List<Logs> logs = (List<Logs>)new JsonSerializer().Deserialize(reader, typeof(List<Logs>));
                    return logs;
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Не удалось загрузить файл конфигурации. error: RLJ1");
                return null;
            }
        }

        public static void SaveGlobalJson(string login, string password) //Сохранение json
        {
            try
            {
                GlobalParametersJson globalJson = new GlobalParametersJson();
                globalJson.LoginT = login;
                globalJson.PassT = password;
                globalJson.Token = ParametersClass.token;
                globalJson.Ip = ParametersClass.ip;
                globalJson.AutoStart = ParametersClass.AutostartValue;

                using (StreamWriter writer = File.CreateText(ParametersClass.GlobalJson))
                {
                    new JsonSerializer().Serialize(writer, globalJson);
                    writer.Close();
                    writer.Dispose();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Неудалось сохранить файл. error: GSJ1");
                MessageBox.Show(e.ToString());
            }
        }

        public static void SaveGroupJson(Group group) //Сохранение json
        {
            try
            {
                GroupItem item = new GroupItem();
                item.WallId = group.groupItem.WallId;
                item.Public = group.groupItem.Public;
                item.ForPublic = group.groupItem.ForPublic;
                item.Yer = group.time.year;
                item.Mounth = group.time.month;
                item.Day = group.time.day;
                item.Hour = group.time.hour;
                item.Minute = group.time.minute;
                item.PostSpace = group.groupItem.PostSpace;
                item.AutostartValue = group.groupItem.AutostartValue;
                item.ImageInPost = group.groupItem.ImageInPost;
                item.groupStatus = group.groupItem.groupStatus;
                item.MaxPostInAuto = group.groupItem.MaxPostInAuto;


                using (StreamWriter writer = File.CreateText(ParametersClass.getPathToSave(group.GroupName)))
                {
                    new JsonSerializer().Serialize(writer, item);

                    writer.Close();
                    writer.Dispose();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Неудалось сохранить файл. error: SGJ1");
                MessageBox.Show(e.ToString());
            }
        }
        public static GroupItem ReadGroupJson(string filePath) //Загрузка сохранение json
        {
            try
            {
                using (StreamReader reader = File.OpenText(filePath))
                {
                    GroupItem group = (GroupItem)new JsonSerializer().Deserialize(reader, typeof(GroupItem));
                    return group;
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Не удалось загрузить файл конфигурации. error: RGJ1");
                return null;
            }
        }
        public static GlobalParametersJson ReadGlobalJson() //Загрузка сохранение json
        {
            try
            {
                using (StreamReader reader = File.OpenText(ParametersClass.GlobalJson))
                {
                    GlobalParametersJson global = (GlobalParametersJson)new JsonSerializer().Deserialize(reader, typeof(GlobalParametersJson));
                    return global;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Не удалось загрузить файл конфигурации. error: RGJ1");
                return null;
            }
        }

        public static List<string> GetFile(string path) //Получение файлов из папки
        {
            List<string> file = new List<string>();
            int pathToPublic = Directory.GetFiles(path).Length;

            for (int i = 0; i < pathToPublic; i++)
            {
                file.Add(Directory.GetFiles(path)[i]);

                if (i > 150) break;
            }

            return file;
        }
    }
}
