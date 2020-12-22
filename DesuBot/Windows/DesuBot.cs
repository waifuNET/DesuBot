using System;
using System.IO;
using System.Windows.Forms;
using System.Net;
using VkNet.Model;
using System.Drawing;
using Microsoft.Win32;
using DesuBot.Classes;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DesuBot
{
    public partial class DesuBot : Form
    {
        public DesuBot()
        {
            InitializeComponent();
        }

        public int LastIndex = 0;
        private void Form1_Load(object sender, EventArgs e)
        {
            ParametersClass.timertoautopost = timertoautopost;

            if (!Directory.Exists(System.Windows.Forms.Application.StartupPath + "\\Json"))
                Directory.CreateDirectory(System.Windows.Forms.Application.StartupPath + "\\Json");

            if (!Directory.Exists(System.Windows.Forms.Application.StartupPath + "\\Json" + "\\groups"))
                Directory.CreateDirectory(System.Windows.Forms.Application.StartupPath + "\\Json" + "\\groups");

            if (File.Exists(ParametersClass.GlobalJson))
            {
                GlobalParametersJson globalParametersJson = IOClass.ReadGlobalJson();
                LoginBox.Text = globalParametersJson.LoginT;
                PasswordBox.Text = globalParametersJson.PassT;

                ParametersClass.ip = globalParametersJson.Ip;
                ParametersClass.token = globalParametersJson.Token;
                ParametersClass.AutostartValue = globalParametersJson.AutoStart;
            }
            else
            {
                using (FileStream fs = File.Create(ParametersClass.GlobalJson))
                {
                    fs.Close();
                    fs.Dispose();
                }

                IOClass.SaveGlobalJson(LoginBox.Text, PasswordBox.Text);
            }

            string[] groups = Directory.GetFiles(System.Windows.Forms.Application.StartupPath + "\\Json" + "\\groups");
            for (int i = 0; i < groups.Length; i++)
            {
                GroupItem groupItem = IOClass.ReadGroupJson(groups[i]);
                TimeClass time = new TimeClass(groupItem.Hour, groupItem.Minute, groupItem.Yer, groupItem.Mounth, groupItem.Day, groupItem.PostSpace);

                Classes.Group group = new Classes.Group(Path.GetFileNameWithoutExtension(groups[i]), time);
                group.groupItem = groupItem;

                ParametersClass.groups.Add(group);
            }
            if (groups.Length == 0)
            {
                ParametersClass.groups.Add(GroupManager.CreateGroup("Default", true));
                GroupManager.SaveAllGroup();
                LastIndex = 0;
            }

            if (File.Exists(ParametersClass.logsPath))
                ParametersClass.logs = IOClass.ReadLogsJson();
            if (ParametersClass.logs == null)
                ParametersClass.logs = new List<Logs>();

                AutoPostStop.Enabled = false;
            if (ParametersClass.AutostartValue)
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
            catch { }

            string nowIP = new WebClient().DownloadString("http://bot.whatismyipaddress.com");
            if (ParametersClass.ip != nowIP)
            {
                ParametersClass.ip = nowIP;
                if (ParametersClass.AutostartValue)
                    VkApiClass.Auth(LoginBox.Text, PasswordBox.Text, ButtonLogin);
            }
            else if (ParametersClass.AutostartValue == true && ParametersClass.ip == nowIP)
            {
                VkApiClass.Auth(LoginBox.Text, PasswordBox.Text, ButtonLogin);
            }
            else
            {
                try
                {
                    ParametersClass.vkapi.Authorize(new ApiAuthParams
                    {
                        AccessToken = ParametersClass.token
                    });
                    if (ParametersClass.AutostartValue)
                        timertoautopost.Enabled = true;
                    else
                        MessageBox.Show("Успешная авторизация через токен.");
                    ParametersClass.autch = true;
                    ButtonLogin.BackColor = Color.Lime;
                }
                catch
                {
                    ParametersClass.autch = false;
                    ButtonLogin.BackColor = Color.Red;
                    if (ParametersClass.AutostartValue)
                        VkApiClass.Auth(LoginBox.Text, PasswordBox.Text, ButtonLogin);
                    else
                        MessageBox.Show("Авторизация через токен не удалась.");
                }
            }

            UpdateGroupList();
            groupList.SelectedIndex = LastIndex;
            UpdateGUI();
        }

        private void ButtonLogin_Click(object sender, EventArgs e)
        {
            if (!ParametersClass.autch)
                VkApiClass.Auth(LoginBox.Text, PasswordBox.Text, ButtonLogin);
            else
                MessageBox.Show("Вы уже авторизованны.");
        }

        private void Saves_Click(object sender, EventArgs e)
        {
            IOClass.SaveGlobalJson(LoginBox.Text, PasswordBox.Text);
            GroupManager.SaveAllGroup();
        }

        public static void progressBarPlus(DesuBot desuBot)
        {
            desuBot.Invoke(new Action(() => desuBot.progressBar.Value += 1));
        }

        public void GUIStatus(bool status)
        {
            WallId.Enabled = status;

            patchToPublic.Enabled = status;
            patchPublic.Enabled = status;

            data1.Enabled = status;
            data2.Enabled = status;
            data3.Enabled = status;

            time1.Enabled = status;
            time2.Enabled = status;

            SpaseTime.Enabled = status;
            textboxImageInPost.Enabled = status;
            CoutPost.Enabled = status;

            Hesh.Enabled = status;

            Start.Enabled = status;
            Saves.Enabled = status;

            buttonAdd.Enabled = status;
            buttonRemove.Enabled = status;

            NewGroupName.Enabled = status;
            checkBoxAuto.Enabled = status;
            AutoMaxPost.Enabled = status;
        }

        public async Task start(bool force = false)
        {
            GUIStatus(false);

            int max_value = 0;
            for (int i = 0; i < ParametersClass.groups.Count; i++)
            {
                if (!force)
                {
                    if (ParametersClass.groups[i].groupItem.groupStatus && !ParametersClass.AutostartValue)
                        max_value += ParametersClass.groups[i].groupItem.ImageInPost * ParametersClass.groups[i].CoutPost;
                    else if (ParametersClass.groups[i].groupItem.groupStatus && ParametersClass.AutostartValue && ParametersClass.groups[i].groupItem.AutostartValue)
                    {
                        max_value += ParametersClass.groups[i].groupItem.ImageInPost * ParametersClass.groups[i].groupItem.MaxPostInAuto;
                    }
                }
                else
                {
                    timertoautopost.Enabled = false;
                    AutoPostStop.Enabled = false;

                    if (ParametersClass.groups[i].groupItem.groupStatus)
                        max_value += ParametersClass.groups[i].groupItem.ImageInPost * ParametersClass.groups[i].CoutPost;
                }
            }
            progressBar.Minimum = 0;
            progressBar.Maximum = max_value;
            progressBar.Value = 0;

            for (int i = 0; i < ParametersClass.groups.Count; i++)
            {
                if (ParametersClass.groups[i].groupItem.groupStatus)
                {
                    if (!force)
                    {
                        if (ParametersClass.AutostartValue)
                        {
                            if (ParametersClass.groups[i].groupItem.AutostartValue)
                            {
                                if (!ParametersClass.groups[i].check())
                                {
                                    GUIStatus(true);
                                    return;
                                }
                            }
                        }
                        else if (!ParametersClass.groups[i].check())
                        {
                            GUIStatus(true);
                            return;
                        }
                    }
                    else
                    {
                        if (!ParametersClass.groups[i].check(true))
                        {
                            GUIStatus(true);
                            return;
                        }
                    }
                }
            }

            for (int i = 0; i < ParametersClass.groups.Count; i++)
            {
                if (ParametersClass.groups[i].groupItem.groupStatus)
                {
                    if (!force)
                    {
                        if (ParametersClass.AutostartValue)
                        {
                            if (ParametersClass.groups[i].groupItem.AutostartValue)
                            {
                                await ParametersClass.groups[i].start();
                            }
                        }
                        else
                        {
                            await ParametersClass.groups[i].start();
                        }
                    }
                    else
                    {
                        await ParametersClass.groups[i].start(true);
                    }
                }
            }

            UpdateGUI();
            GUIStatus(true);
        }

        private async void Start_Click(object sender, EventArgs e)
        {
            await start(true);
        }

        public async void AutoPost()
        {
            await start();
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

        private void button1_Click(object sender, EventArgs e)
        {
            ParametersClass.AutostartValue = true;
            IOClass.SaveGlobalJson(LoginBox.Text, PasswordBox.Text);
            SetAutorunValue(true);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ParametersClass.AutostartValue = false;
            IOClass.SaveGlobalJson(LoginBox.Text, PasswordBox.Text);
            SetAutorunValue(false);
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
                if (ParametersClass.AutostartValue)
                {
                    timertoautopost.Enabled = false;
                    AutoPostStop.Enabled = false;

                    AutoPost();
                }
                else
                    timertoautopost.Enabled = false;
            }
        }

        private void groupList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            string[] groups = Directory.GetFiles(System.Windows.Forms.Application.StartupPath + "\\Json" + "\\groups");
            for (int i = 0; i < groups.Length; i++)
            {
                if (Path.GetFileNameWithoutExtension(groups[i]).Trim().ToLower() == NewGroupName.Text.Trim().ToLower())
                {
                    MessageBox.Show("Группа с таким именем уже существует!");
                    return;
                }
            }

            ParametersClass.groups.Add(GroupManager.CreateGroup(NewGroupName.Text, true));
            groupList.Items.Add("[ON] " + NewGroupName.Text);

            GroupManager.SaveAllGroup();
        }

        public void UpdateGroupList()
        {
            groupList.Items.Clear();

            for (int i = 0; i < ParametersClass.groups.Count; i++)
            {
                string status = ParametersClass.groups[i].groupItem.groupStatus ? "[ON] " : "[OFF] ";
                groupList.Items.Add(status + ParametersClass.groups[i].GroupName);
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (groupList.SelectedIndex != -1)
            {
                string[] groups = Directory.GetFiles(System.Windows.Forms.Application.StartupPath + "\\Json" + "\\groups");
                File.Delete(groups[groupList.SelectedIndex]);
                ParametersClass.groups.RemoveAt(groupList.SelectedIndex);
            }

            UpdateGroupList();
        }

        public void UpdateGUI()
        {
            if (LastIndex != -1)
            {
                WallId.Text = ParametersClass.groups[LastIndex].groupItem.WallId.ToString();

                patchToPublic.Text = ParametersClass.groups[LastIndex].groupItem.ForPublic;
                patchPublic.Text = ParametersClass.groups[LastIndex].groupItem.Public;

                data1.Text = ParametersClass.groups[LastIndex].time.day.ToString();
                data2.Text = ParametersClass.groups[LastIndex].time.month.ToString();
                data3.Text = ParametersClass.groups[LastIndex].time.year.ToString();

                time1.Text = ParametersClass.groups[LastIndex].time.hour.ToString();
                time2.Text = ParametersClass.groups[LastIndex].time.minute.ToString();

                SpaseTime.Text = ParametersClass.groups[LastIndex].groupItem.PostSpace.ToString();
                textboxImageInPost.Text = ParametersClass.groups[LastIndex].groupItem.ImageInPost.ToString();
                CoutPost.Text = ParametersClass.groups[LastIndex].CoutPost.ToString();

                Hesh.Text = ParametersClass.groups[LastIndex].groupItem.Hesh;

                checkBoxAuto.Checked = ParametersClass.groups[LastIndex].groupItem.AutostartValue;
                AutoMaxPost.Text = ParametersClass.groups[LastIndex].groupItem.MaxPostInAuto.ToString();
            }
        }

        private void groupList_Click(object sender, EventArgs e)
        {
            LastIndex = groupList.SelectedIndex;

            UpdateGUI();
        }

        private void groupList_DoubleClick(object sender, EventArgs e)
        {
            if (groupList.SelectedIndex != -1)
                ParametersClass.groups[groupList.SelectedIndex].groupItem.groupStatus = ParametersClass.groups[groupList.SelectedIndex].groupItem.groupStatus ? false : true;
            UpdateGroupList();

            groupList.SelectedIndex = LastIndex;
        }

        ////////////////////// UPDATE GROUP PARAMETERS //////////////////////

        private void patchToPublic_TextChanged(object sender, EventArgs e)
        {
            ParametersClass.groups[LastIndex].groupItem.ForPublic = patchToPublic.Text;
        }

        private void patchPublic_TextChanged(object sender, EventArgs e)
        {
            ParametersClass.groups[LastIndex].groupItem.Public = patchPublic.Text;
        }

        private void data1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                ParametersClass.groups[LastIndex].time.day = Int32.Parse(data1.Text);
            }
            catch
            {
                ParametersClass.groups[LastIndex].time.day = DateTime.Now.Day;
            }
        }

        private void data2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                ParametersClass.groups[LastIndex].time.month = Int32.Parse(data2.Text);
            }
            catch
            {
                ParametersClass.groups[LastIndex].time.month = DateTime.Now.Month;
            }
        }

        private void data3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                ParametersClass.groups[LastIndex].time.year = Int32.Parse(data3.Text);
            }
            catch
            {
                ParametersClass.groups[LastIndex].time.year = DateTime.Now.Year;
            }
        }

        private void time1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                ParametersClass.groups[LastIndex].time.hour = Int32.Parse(time1.Text);
            }
            catch
            {
                ParametersClass.groups[LastIndex].time.hour = DateTime.Now.Hour;
            }
        }

        private void time2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                ParametersClass.groups[LastIndex].time.minute = Int32.Parse(time2.Text);
            }
            catch
            {
                ParametersClass.groups[LastIndex].time.minute = DateTime.Now.Minute;
            }
        }

        private void SpaseTime_TextChanged(object sender, EventArgs e)
        {
            try
            {
                ParametersClass.groups[LastIndex].groupItem.PostSpace = Int32.Parse(SpaseTime.Text);
            }
            catch
            {
                ParametersClass.groups[LastIndex].groupItem.PostSpace = 1;
            }
        }

        private void textboxImageInPost_TextChanged(object sender, EventArgs e)
        {
            try
            {
                ParametersClass.groups[LastIndex].groupItem.ImageInPost = Int32.Parse(textboxImageInPost.Text);
            }
            catch
            {
                ParametersClass.groups[LastIndex].groupItem.ImageInPost = 1;
            }
        }

        private void WallId_TextChanged(object sender, EventArgs e)
        {
            try
            {
                ParametersClass.groups[LastIndex].groupItem.WallId = Int32.Parse(WallId.Text);
            }
            catch
            {
                ParametersClass.groups[LastIndex].groupItem.WallId = 0;
            }
        }

        private void Hesh_TextChanged(object sender, EventArgs e)
        {
            ParametersClass.groups[LastIndex].groupItem.Hesh = Hesh.Text;
        }

        private void CoutPost_TextChanged(object sender, EventArgs e)
        {
            try
            {
                ParametersClass.groups[LastIndex].CoutPost = Int32.Parse(CoutPost.Text);
            }
            catch
            {
                ParametersClass.groups[LastIndex].CoutPost = 0;
            }
        }

        private void checkBoxAuto_CheckedChanged(object sender, EventArgs e)
        {
            ParametersClass.groups[LastIndex].groupItem.AutostartValue = checkBoxAuto.Checked;
        }

        private void AutoMaxPost_TextChanged(object sender, EventArgs e)
        {
            try
            {
                ParametersClass.groups[LastIndex].groupItem.MaxPostInAuto = Int32.Parse(AutoMaxPost.Text);
            }
            catch
            {
                ParametersClass.groups[LastIndex].groupItem.MaxPostInAuto = 50;
            }
        }

        private void PostsBack_Click(object sender, EventArgs e)
        {
            Other.PostsBack(ParametersClass.groups[LastIndex].groupItem.WallId, LastIndex);
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.WindowState = FormWindowState.Normal;

            this.Focus();
            this.Activate();
        }
    }
}