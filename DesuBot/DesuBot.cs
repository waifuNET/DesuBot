using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Net;
using VkNet;
using VkNet.Model;
using VkNet.Enums.Filters;
using VkNet.Model.RequestParams;
using VkNet.Enums.SafetyEnums;
using System.Drawing;
using System.Threading;
using Microsoft.Win32;
using System.Collections;
using System.Threading.Tasks;

namespace DesuBot
{
    public partial class DesuBot : Form
    {
        public DesuBot()
        {
            InitializeComponent();
        }

        static ulong appID = 6648036;

        static VkApi vkapi = new VkApi();
        Settings settings = Settings.All;
        static string logIpass = System.Windows.Forms.Application.StartupPath + @"\LogIpass.json";
        static List<string> file = new List<string>();

        static bool autch = false;

        static string token;
        static string ip;

        static bool AutostartValue = false;

        public void EDGUI(bool status)
        {
            Invoke(new Action(() =>
            {
                Start.Enabled = status;
                patchPublic.Enabled = status;
                patchToPublic.Enabled = status;
                patchToPublic.Enabled = status;
                data1.Enabled = status;
                data2.Enabled = status;
                data3.Enabled = status;
                time1.Enabled = status;
                time2.Enabled = status;
                SpaseTime.Enabled = status;
                CoutPost.Enabled = status;
                textboxImageInPost.Enabled = status;
                Saves.Enabled = status;
                Hesh.Enabled = status;
            }));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists(logIpass))
                ReadJson();
            else
            {
                using (FileStream fs = File.Create(logIpass))
                {
                    fs.Close();
                    fs.Dispose();
                }

                WriteJson();
            }

            AutoPostStop.Enabled = false;
            if (AutostartValue)
            {
                this.ShowInTaskbar = false;
                this.WindowState = FormWindowState.Minimized;
                this.FormBorderStyle = FormBorderStyle.SizableToolWindow;

                notifyIcon1.Icon = this.Icon;
                notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon1.BalloonTipText = "DesuBot работает в автоматическом режиме. Работа начнётся через 10 минут.";
                notifyIcon1.BalloonTipTitle = "DesuBot";
                notifyIcon1.ShowBalloonTip(12);

                AutoPostStop.Enabled = true;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.Sizable;
            }
            try
            {
                label11.Text = "Количество вайлов в папке: " + Directory.GetFiles(patchToPublic.Text).Length;
            }
            catch
            {

            }

            string nowIP = new WebClient().DownloadString("http://bot.whatismyipaddress.com");
            if (ip != nowIP)
            {
                ip = nowIP;
                if (AutostartValue)
                    Auth();
            }
            else if (AutostartValue == true && ip == nowIP)
            {
                Auth();
            }
            else
            {
                try
                {
                    vkapi.Authorize(new ApiAuthParams
                    {
                        AccessToken = token
                    });
                    if (AutostartValue)
                        timertoautopost.Enabled = true;
                    else
                        MessageBox.Show("Успешная авторизация через токен.");
                    autch = true;
                    ButtonLogin.BackColor = Color.Lime;
                }
                catch
                {
                    autch = false;
                    ButtonLogin.BackColor = Color.Red;
                    if (AutostartValue)
                        Auth();
                    else
                        MessageBox.Show("Авторизация через токен не удалась.");
                }
            }
        }

        static string TwoFactorAuthorizationResult;
        public bool Auth() //Авторизация в вк
        {
            try
            {
                if (AutostartValue)
                {
                    vkapi.Authorize(new ApiAuthParams
                    {
                        ApplicationId = appID,
                        Login = LoginBox.Text,
                        Password = PasswordBox.Text,
                        Settings = settings
                    });
                    autch = true;
                    token = vkapi.Token;
                    WriteJson();
                    ButtonLogin.BackColor = Color.Lime;
                    timertoautopost.Enabled = true;

                    vkapi.Stats.TrackVisitor();
                }
                else
                {
                    vkapi.Authorize(new ApiAuthParams
                    {
                        ApplicationId = appID,
                        Login = LoginBox.Text,
                        Password = PasswordBox.Text,
                        Settings = settings,
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
                    autch = true;
                    token = vkapi.Token;
                    WriteJson();
                    ButtonLogin.BackColor = Color.Lime;

                    vkapi.Stats.TrackVisitor();
                }
                return true;

            }
            catch
            {
                autch = false;
                MessageBox.Show("Авторизация не удалась.");
                ButtonLogin.BackColor = Color.Red;
                return false;
            }
        }

        class Item //Переменные для сохранения в json
        {
            public string LoginT { get; set; }
            public string PassT { get; set; }
            public string Id { get; set; }
            public string Public { get; set; }
            public string OnPublic { get; set; }
            public string Yer { get; set; }
            public string Mounth { get; set; }
            public string Day { get; set; }
            public string Chas { get; set; }
            public string Min { get; set; }
            public string PostSpace { get; set; }
            public string Token { get; set; }
            public string Ip { get; set; }
            public bool AutostartValue { get; set; }
        }

        static int hour;
        static int minute;
        static int year;
        static int month;
        static int day;
        static int space;

        public void Time() //Логика времени
        {
            int SpTime = 0;
            Invoke(new Action(() =>
            {
                SpTime = int.Parse(SpaseTime.Text);

                hour = int.Parse(time1.Text);
                minute = int.Parse(time2.Text);
                year = int.Parse(data3.Text);
                month = int.Parse(data2.Text);
                day = int.Parse(data1.Text);
                space = int.Parse(SpaseTime.Text);
            }));
            if (SpTime > 12)
            {
                MessageBox.Show("Промежуток между постами не может быть больше 12 часов.");
                System.Windows.Forms.Application.Exit();
            }
            else
            {
                int mbtime = hour + SpTime;
                if (mbtime >= 24)
                {
                    hour = 1;
                    day++;
                    Invoke(new Action(() =>
                    {
                        time1.Text = hour.ToString();
                        data1.Text = day.ToString();
                    }));
                }
                else if (space <= 0)
                {
                    MessageBox.Show("Post time spasce: <= 0!");
                }
                else
                {
                    hour += space;
                    Invoke(new Action(() =>
                    {
                        time1.Text = hour.ToString();
                    }));
                }
                if (day > DateTime.DaysInMonth(year, month))
                {
                    month++;
                    day = 1;
                    Invoke(new Action(() =>
                    {
                        data1.Text = day.ToString();
                        data2.Text = month.ToString();
                    }));
                }
                if (month > 12)
                {
                    month = 1;
                    year++;
                    Invoke(new Action(() =>
                    {
                        data3.Text = year.ToString();
                        data2.Text = month.ToString();
                    }));
                }
            }
        }
        long groupid;

        public long csid;
        public string captchaUrl;
        public string captchaKey;

        bool filemove;

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

        public IEphoto Posts(int postMultiply, long ownerid)
        {
            WebClient wc = new WebClient();
            List<System.Collections.ObjectModel.ReadOnlyCollection<VkNet.Model.Attachments.Photo>> photos 
                = new List<System.Collections.ObjectModel.ReadOnlyCollection<VkNet.Model.Attachments.Photo>>();

            for (int i = 0; i < postMultiply; i++)
            {
                string response = Encoding.ASCII.GetString(wc.UploadFile(vkapi.Photo.GetWallUploadServer(groupid).UploadUrl, file[i])); //Загрузка файла на сервера вк
                photos.Add(vkapi.Photo.SaveWallPhoto(response: response, userId: (ulong)ownerid, groupId: (ulong)groupid));
            }

            return new IEphoto(photos);
        }

        public void Post() //Создание поста
        {
            int postMultiply = 0;
            int wallid = 0;
            ulong CoutPosts = 0;
            int FileCount = 0;

            Invoke(new Action(() =>
            {
                postMultiply = Int32.Parse(textboxImageInPost.Text);
                progressBar.Maximum = int.Parse(CoutPost.Text);
                wallid = -int.Parse(WallId.Text);
                FileCount = Directory.GetFiles(patchToPublic.Text).Length;
            }));

            var vkpost = vkapi.Wall.Get(new WallGetParams
            {
                OwnerId = wallid,
                Filter = WallFilter.Postponed,
            });

            Invoke(new Action(() =>
            {
                if (!AutostartValue)
                    CoutPosts = ulong.Parse(CoutPost.Text);
                else
                    CoutPosts = 150 - vkpost.TotalCount;
            }));

            if ((int)CoutPosts * postMultiply > FileCount)
            {
                Invoke(new Action(() =>
                {
                    this.WindowState = FormWindowState.Normal;
                }));
                MessageBox.Show("В папке всего: " + FileCount + ", а не " + (int)CoutPosts * postMultiply + " файла.");
            }
            else
            {
                if ((CoutPosts + vkpost.TotalCount) <= 150)
                {
                    if (AutostartValue)
                        Invoke(new Action(() =>
                        {
                            progressBar.Maximum = (int)CoutPosts;
                        }));
                    for (int i = 0; i < (int)CoutPosts; i++) //Сколько будет постов
                    {
                        Time();

                        DateTime datenow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 00);
                        DateTime date = new DateTime(year, month, day, hour, minute, 00);

                        if (date < datenow)
                        {
                            if (AutostartValue)
                                Invoke(new Action(() =>
                                {
                                    this.WindowState = FormWindowState.Normal;
                                }));
                            MessageBox.Show("Мы не можем отправлять посты в прошлое.");
                        }

                        GetFile();
                        string HeshText = "";
                        Invoke(new Action(() =>
                        {
                            HeshText = Hesh.Text;
                            groupid = (long)int.Parse(WallId.Text);
                        }));
                        long ownerid = vkapi.Users.Get(new long[] { })[0].Id;

                        try
                        {
                            vkapi.Wall.Post(new WallPostParams //Создание поста
                            {
                                OwnerId = wallid,
                                FromGroup = true,
                                Message = HeshText,
                                Attachments = Posts(postMultiply, ownerid).getIEMA(),
                                PublishDate = new DateTime(year, month, day, hour, minute, 0)
                            });
                            filemove = true;
                        }
                        catch (Exception ex)
                        {
                            if (ex is VkNet.Exception.PostLimitException)
                            {
                                if (AutostartValue)
                                    Invoke(new Action(() =>
                                    {
                                        this.WindowState = FormWindowState.Normal;
                                    }));
                                MessageBox.Show(date + " : Возможно в данное время уже запланирован пост или вы достигли лимита 150 постов в отложенных записях. error: P1");
                                this.Close();
                            }
                            else if (ex is VkNet.Exception.CaptchaNeededException cap)
                            {
                                csid = cap.Sid;
                                captchaUrl = cap.Img.AbsoluteUri;
                                using (Captcha captcha = new Captcha())
                                {
                                    captcha.Owner = this;
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

                        if (filemove)
                        {
                            string patchPublicText = "";
                            Invoke(new Action(() =>
                            {
                                patchPublicText = patchPublic.Text;
                            }));
                            for (int f = 0; f < postMultiply; f++)
                            {
                                File.Move(file[f], patchPublicText + @"\" + DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Second + "_" + DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year + "_" + "f" + f + "_" + Path.GetFileName(file[0])); //Перемещение файла в папку после загрузки
                                WriteJson(); //Сохраняем переменные json
                            }
                            Invoke(new Action(() =>
                            {
                                progressBar.Value++;
                            }));
                        }
                    }
                    EDGUI(true);
                }
                else
                    MessageBox.Show("В отложенные записи нельзя добавить больше 150 постов.");
            }
        }

        public void WriteJson() //Сохранение json
        {
            try
            {
                Item item1 = new Item();
                Invoke(new Action(() =>
                {
                    item1.LoginT = LoginBox.Text;
                    item1.PassT = PasswordBox.Text;
                    item1.Id = WallId.Text;
                    item1.Public = patchPublic.Text;
                    item1.OnPublic = patchToPublic.Text;
                    item1.Yer = data3.Text;
                    item1.Mounth = data2.Text;
                    item1.Day = data1.Text;
                    item1.Chas = time1.Text;
                    item1.Min = time2.Text;
                    item1.PostSpace = SpaseTime.Text;
                    item1.Token = token;
                    item1.Ip = ip;
                    item1.AutostartValue = AutostartValue;
                }));
                Item item = item1;
                using (StreamWriter writer = File.CreateText(logIpass))
                {
                    new JsonSerializer().Serialize(writer, item);
                    writer.Close();
                    writer.Dispose();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Неудалось сохранить файл. error: WJ1");
                MessageBox.Show(e.ToString());
            }
        }
        public void ReadJson() //Загрузка сохранение json
        {
            try
            {
                Item item = JsonConvert.DeserializeObject<Item>(File.ReadAllText(logIpass));
                using (StreamReader reader = File.OpenText(logIpass))
                {
                    Item item2 = (Item)new JsonSerializer().Deserialize(reader, typeof(Item));
                    LoginBox.Text = item2.LoginT;
                    PasswordBox.Text = item2.PassT;
                    WallId.Text = item2.Id;
                    if (item2.Public != "")
                    {
                        patchPublic.Text = item2.Public;
                    }
                    else
                    {
                        patchPublic.Text = "";
                    }
                    if (item2.OnPublic != "")
                    {
                        patchToPublic.Text = item2.OnPublic;
                    }
                    else
                    {
                        patchToPublic.Text = "";
                    }
                    data3.Text = item2.Yer;
                    data2.Text = item2.Mounth;
                    data1.Text = item2.Day;
                    time1.Text = item2.Chas;
                    time2.Text = item2.Min;
                    token = item2.Token;
                    ip = item2.Ip;
                    AutostartValue = item2.AutostartValue;
                    reader.Close();
                    LoginBox.Text = item2.LoginT;
                    PasswordBox.Text = item2.PassT;
                    WallId.Text = item2.Id;
                    SpaseTime.Text = item2.PostSpace;
                    reader.Dispose();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Не удалось загрузить файл конфигурации. error: RJ1");
            }
        }
        public void GetFile() //Получение файлов из папки
        {
            file.Clear();
            int pathToPublic = 0;

            Invoke(new Action(() =>
            {
                pathToPublic = Directory.GetFiles(patchToPublic.Text).Length;
                listBoxfile.Items.Clear();
            }));
            for (int i = 0; i < pathToPublic; i++)
            {
                Invoke(new Action(() =>
                {
                    file.Add(Directory.GetFiles(patchToPublic.Text)[i]);
                    listBoxfile.Items.Add(Path.GetFileName(file[i]));
                }));

                if (i > 150) break;
            }
        }

        private void ButtonLogin_Click(object sender, EventArgs e)
        {
            if (!autch)
                Auth();
            else
                MessageBox.Show("Вы уже авторизованны.");
        }

        private void Saves_Click(object sender, EventArgs e)
        {
            WriteJson();
        }

        private async void Start_Click(object sender, EventArgs e)
        {
            if (autch)
            {
                if (CoutPost.Text != null && CoutPost.Text != "")
                {
                    if (Directory.Exists(patchToPublic.Text))
                    {
                        if (Directory.Exists(patchPublic.Text))
                        {
                            if (int.Parse(CoutPost.Text) * int.Parse(textboxImageInPost.Text) > Directory.GetFiles(patchToPublic.Text).Length)
                                MessageBox.Show("В папке всего: " + Directory.GetFiles(patchToPublic.Text).Length + ", а не " + int.Parse(CoutPost.Text) * int.Parse(textboxImageInPost.Text) + " файла.");
                            else
                            {
                                if (int.Parse(CoutPost.Text) > 0)
                                {
                                    if (int.Parse(textboxImageInPost.Text) > 0)
                                    {
                                        if (int.Parse(textboxImageInPost.Text) <= 10)
                                        {
                                            progressBar.Value = 0;
                                            new Thread(() =>
                                            {
                                                EDGUI(false);
                                                Post();
                                            }).Start();
                                        }
                                        else
                                            MessageBox.Show("В посте не может быть больше 10 изображений");
                                    }
                                    else
                                        MessageBox.Show("Количество изображений не может быть 0 или меньше");
                                }
                                else
                                    MessageBox.Show("Постов не может быть 0 или меньше");
                            }
                        }
                        else
                            MessageBox.Show("Папка не найдена || Введите корректный путь в поле <<Куда убирать>>.");
                    }
                    else
                        MessageBox.Show("Папка не найдена || Введите корректный путь в поле <<Откуда брать>>.");
                }
                else
                    MessageBox.Show("Введите количество постов.");
            }
            else
                MessageBox.Show("Вы не авторизовались!");
        }

        public void AutoPost()
        {
            //MessageBox.Show("Автоматическая авторизация прошла успешна");
            progressBar.Value = 0;
            Post();
            System.Windows.Forms.Application.Exit();
        }

        const string name = "DesuBot";
        public bool SetAutorunValue(bool autorun)
        {
            string ExePath = System.Windows.Forms.Application.ExecutablePath;
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            try
            {
                if (autorun)
                {
                    reg.SetValue(name, ExePath);
                    MessageBox.Show(name + " добавлен в автозагрузку.");
                }
                else
                {
                    reg.DeleteValue(name);
                    MessageBox.Show(name + " убран из автозагрузки.");
                }

                reg.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка SAV: " + e.ToString());
                return false;
            }
            return true;
        }

        private void Login_Click(object sender, EventArgs e)
        {
            //string Url = "https://scchat.000webhostapp.com/Command.php";

            //WebRequest reqMAIN = WebRequest.Create(Url + "?" + "Command=Get");
            //WebResponse respMAIN = reqMAIN.GetResponse();
            //Stream streamMAIN = respMAIN.GetResponseStream();
            //StreamReader srMAIN = new StreamReader(streamMAIN);
            //string OutMAIN = srMAIN.ReadToEnd();
            //srMAIN.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AutostartValue = true;
            WriteJson();
            SetAutorunValue(true);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AutostartValue = false;
            WriteJson();
            SetAutorunValue(false);
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.WindowState = FormWindowState.Normal;
        }

        private void AutoPostStop_Click(object sender, EventArgs e)
        {
            timertoautopost.Enabled = false;
            AutoPostStop.Enabled = false;
        }

        int time = 0;
        private void timertoautopost_Tick(object sender, EventArgs e)
        {
            if (time < 600)
                time++;
            else
            {
                if (AutostartValue)
                {
                    timertoautopost.Enabled = false;
                    AutoPostStop.Enabled = false;

                    AutoPost();
                }
                else
                    timertoautopost.Enabled = false;
            }
        }
    }
}