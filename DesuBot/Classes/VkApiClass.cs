using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Text;
using System.Windows.Forms;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace DesuBot.Classes
{
    class VkApiClass
    {
        static string TwoFactorAuthorizationResult;
        public static bool Auth(string login, string password, System.Windows.Forms.Button loginButton) //Авторизация в вк
        {
            try
            {
                if (ParametersClass.AutostartValue)
                {
                    ParametersClass.vkapi.Authorize(new ApiAuthParams
                    {
                        ApplicationId = ParametersClass.appID,
                        Login = login,
                        Password = password,
                        Settings = ParametersClass.settings
                    });
                    ParametersClass.autch = true;
                    ParametersClass.token = ParametersClass.vkapi.Token;
                    IOClass.SaveGlobalJson(login, password);
                    loginButton.BackColor = Color.Lime;
                    ParametersClass.timertoautopost.Enabled = true;

                    ParametersClass.vkapi.Stats.TrackVisitor();
                }
                else
                {
                    ParametersClass.vkapi.Authorize(new ApiAuthParams
                    {
                        ApplicationId = ParametersClass.appID,
                        Login = login,
                        Password = password,
                        Settings = ParametersClass.settings,
                        TwoFactorAuthorization = () =>
                        {
                            using (Autch Autch = new Autch())
                            {
                                if (Autch.ShowDialog() == DialogResult.OK)
                                {
                                    TwoFactorAuthorizationResult = Autch.TheValue;
                                }
                            }
                            return TwoFactorAuthorizationResult;
                        }
                    });
                    MessageBox.Show("Успешная авторизация.");
                    ParametersClass.autch = true;
                    ParametersClass.token = ParametersClass.vkapi.Token;
                    IOClass.SaveGlobalJson(login, password);
                    loginButton.BackColor = Color.Lime;

                    ParametersClass.vkapi.Stats.TrackVisitor();
                }
                return true;

            }
            catch (Exception ex)
            {
                ParametersClass.autch = false;
                MessageBox.Show("Авторизация не удалась. \n" + ex);
                loginButton.BackColor = Color.Red;
                return false;
            }
        }

        public class IEphoto
        {
            IEnumerable<VkNet.Model.Attachments.MediaAttachment> IEMA;
            List<VkNet.Model.Attachments.MediaAttachment> _attachments = new List<VkNet.Model.Attachments.MediaAttachment>();

            public IEphoto(List<System.Collections.ObjectModel.ReadOnlyCollection<VkNet.Model.Attachments.Photo>> lIEphotoes)
            {
                foreach (var a in lIEphotoes)
                    foreach (var b in a)
                        _attachments.Add(b);

                IEMA = _attachments;
            }

            public IEnumerable<VkNet.Model.Attachments.MediaAttachment> getIEMA()
            {
                return IEMA;
            }
        }

        public static IEphoto Posts(int postMultiply, long ownerid, int groupid, string[] file)
        {
            WebClient wc = new WebClient();
            List<System.Collections.ObjectModel.ReadOnlyCollection<VkNet.Model.Attachments.Photo>> photos
                = new List<System.Collections.ObjectModel.ReadOnlyCollection<VkNet.Model.Attachments.Photo>>();

            for (int i = 0; i < postMultiply; i++)
            {
                string response = Encoding.ASCII.GetString(wc.UploadFile(ParametersClass.vkapi.Photo.GetWallUploadServer(groupid).UploadUrl, file[i])); //Загрузка файла на сервера вк
                photos.Add(ParametersClass.vkapi.Photo.SaveWallPhoto(response: response, userId: (ulong)ownerid, groupId: (ulong)groupid));
            }

            return new IEphoto(photos);
        }

        public static int GetLastPostponedId(int OwnerId)
        {
            var get = ParametersClass.vkapi.Wall.Get(new WallGetParams
            {
                OwnerId = OwnerId,
                Count = 0,
                Filter = VkNet.Enums.SafetyEnums.WallFilter.Postponed,
                Offset = 0
            });
            get = ParametersClass.vkapi.Wall.Get(new WallGetParams
            {
                OwnerId = OwnerId,
                Count = 1,
                Filter = VkNet.Enums.SafetyEnums.WallFilter.Postponed,
                Offset = get.TotalCount - 1
            });

            return Int32.Parse(get.WallPosts[0].ToString().Split('_')[1]);
        }

        public static List<PostInfo> GetAllPostponed(long OwnerId)
        {
            List<PostInfo> posts = new List<PostInfo>();
            var get = ParametersClass.vkapi.Wall.Get(new WallGetParams
            {
                OwnerId = -OwnerId,
                Count = 0,
                Filter = VkNet.Enums.SafetyEnums.WallFilter.Postponed,
                Offset = 0
            });

            if (get.TotalCount > 100)
            {
                get = ParametersClass.vkapi.Wall.Get(new WallGetParams
                {
                    OwnerId = -OwnerId,
                    Count = 100,
                    Filter = VkNet.Enums.SafetyEnums.WallFilter.Postponed,
                    Offset = 0
                });
                for(int i = 0; i < 100; i++)
                {
                    string[] splitter = get.WallPosts[i].ToString().Split('-');
                    int groupId = Int32.Parse(splitter[1].Split('_')[0]);
                    int postId = Int32.Parse(splitter[1].Split('_')[1]);

                    posts.Add(new PostInfo { groupId = groupId, postId = postId });
                }

                // last //
                get = ParametersClass.vkapi.Wall.Get(new WallGetParams
                {
                    OwnerId = -OwnerId,
                    Count = 100,
                    Filter = VkNet.Enums.SafetyEnums.WallFilter.Postponed,
                    Offset = 100
                });
                for (int i = 100, post = 0; i < (int)get.TotalCount; i++, post++)
                {
                    string[] splitter = get.WallPosts[i].ToString().Split('-');
                    int groupId = Int32.Parse(splitter[1].Split('_')[0]);
                    int postId = Int32.Parse(splitter[1].Split('_')[1]);

                    posts.Add(new PostInfo { groupId = groupId, postId = postId });
                }
            }
            else
            {
                get = ParametersClass.vkapi.Wall.Get(new WallGetParams
                {
                    OwnerId = -OwnerId,
                    Count = 100,
                    Filter = VkNet.Enums.SafetyEnums.WallFilter.Postponed,
                    Offset = 0
                });
                for (int i = 0; i < get.WallPosts.Count; i++)
                {
                    string[] splitter = get.WallPosts[i].ToString().Split('-');
                    int groupId = Int32.Parse(splitter[1].Split('_')[0]);
                    int postId = Int32.Parse(splitter[1].Split('_')[1]);

                    posts.Add(new PostInfo { groupId = groupId, postId = postId });
                }
            }

            return posts;
        }
    }
}
