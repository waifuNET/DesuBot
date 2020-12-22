using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.RequestParams;

namespace DesuBot.Classes
{
    public class Group
    {
        public string GroupName = "";
        public TimeClass time;

        public Group(string GroupName, TimeClass time)
        {
            this.GroupName = GroupName;
            this.time = time;
        }

        public List<string> file = new List<string>();

        public GroupItem groupItem = new GroupItem();

        public int CoutPost = 0;

        public long csid;
        public string captchaUrl;
        public string captchaKey;

        public bool check(bool force = false)
        {
            if (ParametersClass.autch)
            {
                if (ParametersClass.AutostartValue && groupItem.AutostartValue && !force)
                {
                    CoutPost = groupItem.MaxPostInAuto;
                }
                if (CoutPost > 0)
                {
                    if (Directory.Exists(groupItem.ForPublic))
                    {
                        if (Directory.Exists(groupItem.Public))
                        {
                            if (CoutPost * groupItem.ImageInPost > Directory.GetFiles(groupItem.ForPublic).Length)
                            {
                                MessageBox.Show(GroupName + ": " + "В папке всего: " + Directory.GetFiles(groupItem.ForPublic).Length + ", а не " + CoutPost * groupItem.ImageInPost + " файла.");
                                return false;
                            }
                            else
                            {
                                if (CoutPost > 0)
                                {
                                    if (groupItem.ImageInPost > 0)
                                    {
                                        if (groupItem.ImageInPost <= 10)
                                        {
                                            return true;
                                        }
                                        else
                                        {
                                            MessageBox.Show(GroupName + ": " + "В посте не может быть больше 10 изображений");
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show(GroupName + ": " + "Количество изображений не может быть 0 или меньше");
                                        return false;
                                    }
                                }
                                else
                                {
                                    MessageBox.Show(GroupName + ": " + "Постов не может быть 0 или меньше");
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show(GroupName + ": " + "Папка не найдена || Введите корректный путь в поле <<Куда убирать>>.");
                            return false;
                        }
                    }
                    else
                    {
                        MessageBox.Show(GroupName + ": " + "Папка не найдена || Введите корректный путь в поле <<Откуда брать>>.");
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show(GroupName + ": " + "Введите количество постов.");
                    return false;
                }
            }
            else
            {
                MessageBox.Show(GroupName + ": " + "Вы не авторизовались!");
                return false;
            }
        }

        public async Task start(bool force = false)
        {
            await Task.Run(() =>
            {
                Post(force);
            });
        }

        public void Post(bool force = false) //Создание поста
        {
            int postMultiply = 0;
            int wallid = 0;
            ulong CoutPosts = 0;
            int FileCount = 0;

            postMultiply = groupItem.ImageInPost;
            wallid = -groupItem.WallId;
            FileCount = Directory.GetFiles(groupItem.ForPublic).Length;

            var vkpost = ParametersClass.vkapi.Wall.Get(new WallGetParams
            {
                OwnerId = wallid,
                Filter = WallFilter.Postponed,
            });

            if (!ParametersClass.AutostartValue || force)
                CoutPosts = (ulong)CoutPost;
            else
                CoutPosts = (ulong)groupItem.MaxPostInAuto;

            if (vkpost.TotalCount + (ulong)(CoutPost * groupItem.ImageInPost) > 150)
            {
                CoutPosts = 150 - vkpost.TotalCount;
            }

            if (CoutPosts > 0)
            {
                if ((int)CoutPosts * postMultiply > FileCount)
                {
                    MessageBox.Show("В папке всего: " + FileCount + ", а не " + (int)CoutPosts * postMultiply + " файла.");
                }
                else
                {
                    if ((CoutPosts + vkpost.TotalCount) <= 150)
                    {
                        for (int i = 0; i < (int)CoutPosts; i++) //Сколько будет постов
                        {
                            time.Time();

                            DateTime datenow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 00);
                            DateTime date = new DateTime(time.year, time.month, time.day, time.hour, time.minute, 00);

                            if (date < datenow)
                            {
                                MessageBox.Show("Мы не можем отправлять посты в прошлое.");
                            }

                            file = IOClass.GetFile(groupItem.ForPublic);
                            long ownerid = ParametersClass.vkapi.Users.Get(new long[] { })[0].Id;
                            try
                            {
                                var get = ParametersClass.vkapi.Wall.Post(new WallPostParams //Создание поста
                                {
                                    OwnerId = -groupItem.WallId,
                                    FromGroup = true,
                                    Message = groupItem.Hesh != null ? groupItem.Hesh : "",
                                    Attachments = VkApiClass.Posts(postMultiply, ownerid, groupItem.WallId, file.ToArray()).getIEMA(),
                                    PublishDate = new DateTime(time.year, time.month, time.day, time.hour, time.minute, 0)
                                });

                                for (int f = 0; f < postMultiply; f++)
                                {
                                    string new_path = groupItem.Public + @"\" + DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Second + "_" + DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year + "_" + "f" + f + "_" + (Path.GetFileName(file[f]).Split('_').Length > 1 ? Path.GetFileName(file[f]).Split('_')[Path.GetFileName(file[f]).Split('_').Length - 1] : Path.GetFileName(file[f]));
                                    File.Move(file[f], new_path); //Перемещение файла в папку после загрузки
                                    IOClass.SaveGroupJson(this); //Сохраняем переменные json
                                    
                                    ParametersClass.logs.Add(new Logs { file = new_path, groupId = groupItem.WallId, postId = get });
                                    IOClass.SaveLogsJson(ParametersClass.logs);
                                    
                                    DesuBot.progressBarPlus(Program.desuBot);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (ex is VkNet.Exception.PostLimitException)
                                {
                                    MessageBox.Show(date + " : Возможно в данное время уже запланирован пост или вы достигли лимита 150 постов в отложенных записях. error: P1. \n" + ex.Message);
                                }
                                else if (ex is VkNet.Exception.CaptchaNeededException cap)
                                {
                                    csid = cap.Sid;
                                    captchaUrl = cap.Img.AbsoluteUri;
                                    using (Captcha captcha = new Captcha())
                                    {
                                        captcha.Show();

                                        if (captcha.ShowDialog() == DialogResult.OK)
                                        {
                                            captchaKey = captcha.TheValue;
                                        }
                                    }
                                    System.Windows.Forms.Application.Exit();
                                }
                                else
                                {
                                    MessageBox.Show(ex.Message);
                                }
                            }
                        }
                    }
                    else
                        MessageBox.Show("В отложенные записи нельзя добавить больше 150 постов.");
                }
            }
        }
    }

    public class GroupManager
    {
        public static Group CreateGroup(string name, bool status = false)
        {
            TimeClass time = new TimeClass(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 1);
            Group group = new Group(name, time);
            group.CoutPost = 0;
            group.groupItem.PostSpace = 1;
            group.groupItem.WallId = 0;
            group.groupItem.Public = "";
            group.groupItem.ForPublic = "";
            group.groupItem.ImageInPost = 1;
            group.groupItem.Hesh = "";
            group.groupItem.AutostartValue = false;
            group.groupItem.groupStatus = status;

            return group;
        }

        public static void SaveAllGroup()
        {
            for (int i = 0; i < ParametersClass.groups.Count; i++)
            {
                IOClass.SaveGroupJson(ParametersClass.groups[i]);
            }
        }
    }
}
