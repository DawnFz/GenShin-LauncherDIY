﻿using GenShin_LauncherDIY.Config;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;

namespace GenShin_LauncherDIY
{
    /// <summary>
    /// setWindow.xaml 的交互逻辑
    /// </summary>
    public partial class setWindow : MetroWindow
    {
        public setWindow()
        {
            InitializeComponent();
            this.Closing += Window_Closing;
            GamePath.Text = Config.IniGS.gamePath;//游戏路径
            Config.Settings.GameMovePath = Config.IniGS.gamePath;
            if (!Config.IniGS.isAutoSize)//全屏
                FullF.IsChecked = true;
            else
                FullT.IsChecked = true;
            if (Config.IniGS.isUnFPS == true)
            {
                isUnFPS.IsChecked = true;
                MaxFPS.IsEnabled = true;
                MaxFPS.Text = Convert.ToString(Config.IniGS.MaxFps);
            }
            else
                isUnFPS.IsChecked = false;
            if (PopupUP.IsChecked != false)//无边框
                Config.IniGS.isPopup = false;
            if (!Config.IniGS.isPopup)
                PopupUP.IsChecked = false;
            else
                PopupUP.IsChecked = true;
            GHeight.Text = Config.IniGS.Height.ToString();//高度
            GWidth.Text = Config.IniGS.Width.ToString();//宽度
            IsGlobal();
            //读取服务器
            if (Config.IniGS.BiOrMi == 1)
            { 
                MiS.IsChecked = true;
                GlobalS.IsEnabled = false;
            }
            else if (Config.IniGS.BiOrMi == 2)
            {
                BIliS.IsChecked = true;
                GlobalS.IsEnabled = false;
            }
            else
                GlobalS.IsChecked = true;
            if (GamePath.Text == "")
                ToGlobal.IsEnabled = false;
            //设置绑定分辨率数据源
            {
                List<Utils.Display_list> list = new List<Utils.Display_list>();
                list.Add(new Utils.Display_list { Name = "3840×2160-16:9", ID = 0, X = 1 });
                list.Add(new Utils.Display_list { Name = "2560×1080-21:9", ID = 0, X = 2 });
                list.Add(new Utils.Display_list { Name = "1920×1080-16:9", ID = 1, X = 3 });
                list.Add(new Utils.Display_list { Name = "1600×900-16:9", ID = 2, X = 4 });
                list.Add(new Utils.Display_list { Name = "1360×768-16:9", ID = 3, X = 5 });
                list.Add(new Utils.Display_list { Name = "1280×1024-4:3", ID = 4, X = 6 });
                list.Add(new Utils.Display_list { Name = "1280×720-16:9", ID = 5, X = 7 });
                GameXY.ItemsSource = list;
                GameXY.DisplayMemberPath = "Name";
                GameXY.SelectedValuePath = "X";
                GameXY.SelectedIndex = -1;
            }
            ReadUser();
            IsSDK();
        }
        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            this.DragMove();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (bqload.Visibility == Visibility.Hidden)
            {
                e.Cancel = false;
            }
            else
            {
                this.ShowMessageAsync("警告", "请等待文件解压缩完毕，否则可能出现致命错误！！", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
                e.Cancel = true;
            }
        }
        private void setSave_Click(object sender, RoutedEventArgs e)
        {
            //游戏路径保存
            if (string.IsNullOrWhiteSpace(GamePath.Text))
            {
                this.ShowMessageAsync("警告", "游戏路径为空,请输入游戏路径！", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
                return;
            }
            else if (Directory.Exists(GamePath.Text) == false)
            {
                this.ShowMessageAsync("错误", "游戏路径不存在,请输入正确的游戏路径！", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
                return;
            }
            else
            {
                Config.IniGS.gamePath = GamePath.Text;
                Config.Settings.GamePath = GamePath.Text;
            }
            //游戏分辨率保存
            if (!Utils.checkTool.IsNumber(GWidth.Text) || !Utils.checkTool.IsNumber(GHeight.Text))
            {
                this.ShowMessageAsync("警告", "窗口大小错误, 请输入正确的分辨率！", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
                return;
            }
            else
            {
                Settings.Height = GHeight.Text;
                Settings.Width = GWidth.Text;
                Config.IniGS.Width = Convert.ToUInt16(GWidth.Text);
                Config.IniGS.Height = Convert.ToUInt16(GHeight.Text);
            }
            //是否全屏
            if (FullF.IsChecked == true)
            {
                Settings.FullS = "0";
                Config.IniGS.isAutoSize = false;
            }
            else if (FullT.IsChecked == true)
            {
                Settings.FullS = "1";
                Config.IniGS.isAutoSize = true;
            }
            //B服或官服
            if (MiS.IsChecked == true)
            {
                Config.IniGS.BiOrMi = 1;
                BOM.Sub_channel("1");
                BOM.Channel("1");
                BOM.Cps("mihoyo");
            }
            else if (BIliS.IsChecked == true)
            {
                Config.IniGS.BiOrMi = 2;
                BOM.Sub_channel("0");
                BOM.Channel("14");
                BOM.Cps("bilibili");
            }
            else
            {
                Config.IniGS.BiOrMi = 3;
            }
            //无边框窗口化
            if (PopupUP.IsChecked == true)
                Config.IniGS.isPopup = true;
            else
                Config.IniGS.isPopup = false;
            //解锁帧率启动
            if (isUnFPS.IsChecked == true)
            {
                if (!Utils.checkTool.IsNumber(MaxFPS.Text))
                {
                    this.ShowMessageAsync("警告", "帧率错误, 请输入正确的数字！", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
                    return;
                }
                else
                {
                    Config.IniGS.MaxFps = MaxFPS.Text;
                    Config.IniGS.isUnFPS = true;
                }
            }
            else
            {
                Config.IniGS.isUnFPS = false;
                Config.IniGS.MaxFps = "144";
            }
            //选择启动的账号
            WriteUser();
            Config.setConfig.checkini();
            this.Close();
        }
        private void Button_Click21_9(object sender, RoutedEventArgs e)
        {
            if (GHeight.Text == "" && GWidth.Text != "")
            {
                int x = Convert.ToInt32(GWidth.Text);
                int y = x * 9 / 21;
                GWidth.Text = Convert.ToString(x);
                GHeight.Text = Convert.ToString(y);
            }
            else if (GWidth.Text == "" && GHeight.Text != "")
            {
                int y = Convert.ToInt32(GHeight.Text);
                int x = y * 21 / 9;
                GWidth.Text = Convert.ToString(x);
                GHeight.Text = Convert.ToString(y);
            }
            else
            {
                this.ShowMessageAsync("提醒", "在上面随便一个框填上想要的宽或者高另一个框留空使用本按钮自动取21:9比例分辨率", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
            }
        }
        private async void UnFps_Click(object sender, RoutedEventArgs e)
        {
            if (isUnFPS.IsChecked == true)
            {
                if ((await this.ShowMessageAsync("超级警告", "此操作涉及修改游戏客户端进程，我也不知道会不会出现封号风险，出现问题请自行承担后果！如之前没使用过UnlockFPS的建议不要使用！\r\n\r\n只解锁到160(大部分人屏幕应该都是144Hz)", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings() { AffirmativeButtonText = "不同意", NegativeButtonText = "同意" })) != MessageDialogResult.Affirmative)
                {
                    isUnFPS.IsChecked = true;
                    MaxFPS.IsEnabled = true;
                    MaxFPS.Text = Config.IniGS.MaxFps;
                }

                else
                {
                    isUnFPS.IsChecked = false;
                    MaxFPS.IsEnabled = false;
                }
            }
            else
            {
                MaxFPS.IsEnabled = false;
                MaxFPS.Text = "";
            }
        }
        private void IsSDK()
        {
            if (GamePath.Text == "")
            {
                SDKlive.Content = "SDK:未知";
                Fixbtn.IsEnabled = false;
            }
            else
            {
                if (File.Exists(GamePath.Text + "\\Genshin Impact Game\\YuanShen_Data\\Plugins\\PCGameSDK.dll") == true)
                {
                    SDKlive.Content = "SDK:存在";
                    Fixbtn.IsEnabled = false;
                }
                else if (File.Exists(GamePath.Text + "\\Genshin Impact Game\\GenshinImpact_Data\\Plugins\\PCGameSDK.dll") != true)
                {
                    SDKlive.Content = "SDK:无需";
                    Fixbtn.IsEnabled = false;
                }
                else
                {
                    SDKlive.Content = "SDK:缺失";
                    Fixbtn.IsEnabled = true;
                }
            }
        }
        private async void Fix_Click(object sender, RoutedEventArgs e)
        {
            if ((await this.ShowMessageAsync("提醒", "修复SDK仅用于官转哔服，国际服无需修复", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings() { AffirmativeButtonText = "取消", NegativeButtonText = "确定修复" })) != MessageDialogResult.Affirmative)
            {
                if (Directory.Exists(Config.Settings.GamePath + "//Genshin Impact Game//YuanShen_Data") == true)
                {
                    var sdkUri = "pack://application:,,,/Res/mihoyosdk.dll";
                    var uri = new Uri(sdkUri, UriKind.RelativeOrAbsolute);
                    var stream = Application.GetResourceStream(uri).Stream;
                    Utils.UtilsTools.StreamToFile(stream, GamePath.Text + "\\Genshin Impact Game\\YuanShen_Data\\Plugins\\PCGameSDK.dll");
                    IsSDK();
                }
                else
                {
                    this.ShowMessageAsync("提醒", "该端无需修复SDK文件", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
                }

            }
        }
        private async void ToGlobal_Click(object sender, RoutedEventArgs e)
        {
            if ((await this.ShowMessageAsync("警告！！", "转换或还原将会执行重命名，替换，删除等操作修改客户端文件，该过程大概率会触发杀软报毒！为了防止客户端损坏导致不完整，执行前检查杀软（包括 Windows Defender）是否完全关闭或将本启动器加入白名单，并检查游戏是否彻底关闭，否则可能将导致客户端文件缺失！！\r\n\r\n提示：如游戏大版本更新时请执行还原转换为国内服使用游戏自带启动器更新！", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings() { AffirmativeButtonText = "取消转换", NegativeButtonText = "确定转换" })) != MessageDialogResult.Affirmative)
            {

                String pkgfile = Utils.UtilsTools.MiddleText(Utils.UtilsTools.ReadHTML("https://www.cnblogs.com/DawnFz/p/7271382.html", "UTF-8"), "[$pkg$]", "[#pkg#]");
                if (ToGlobal.Content == "复原")
                {
                    Thread StartRe = new Thread(() => ReCnGame());
                    bqload.Visibility = Visibility.Visible;
                    setSave.IsEnabled = false;
                    ToGlobal.IsEnabled = false;
                    TimeStatus.Content = "当前状态：正在还原游戏";
                    StartRe.Start();
                }
                else if ("2.3.0-2" != pkgfile)
                {
                    this.ShowMessageAsync("提示", "国际服转换包有新版本：" + pkgfile + "\r\n访问密码：etxd  已复制到剪切板", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
                    Clipboard.SetText("etxd");
                    Thread.Sleep(2500);
                    Process.Start("https://pan.baidu.com/s/1-5zQoVfE7ImdXrn8OInKqg");
                }
                else if (Directory.Exists(@"GlobalFile") == true)
                {
                    bool error = false;
                    for (int i = 0; i < Settings.globalfiles.Length; i++)
                    {
                        if (File.Exists(@"GlobalFile//" + Settings.globalfiles[i]) == false)
                        {
                            LogBox.Content = Settings.globalfiles[i] + "文件不存在，请重新下载资源包或尝试重新操作";
                            error = true;
                            break;
                        }
                        LogBox.Content = Settings.globalfiles[i] + "存在";
                    }
                    if (!error)
                    {
                        Thread StartMove = new Thread(() => MoveFile());
                        setSave.IsEnabled = false;
                        ToGlobal.IsEnabled = false;
                        TimeStatus.Content = "当前状态：正在替换资源";
                        StartMove.Start();                                                                  
                    }
                    else
                    {
                        this.ShowMessageAsync("提示", "转换资源不完整\r\n请重新下载", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
                        bqload.Visibility = Visibility.Hidden;
                        setSave.IsEnabled = true;
                    }
                }
                else
                {
                    Thread StartUn = new Thread(() => UnFile());
                    bqload.Visibility = Visibility.Visible;
                    setSave.IsEnabled = false;
                    ToGlobal.IsEnabled = false;
                    TimeStatus.Content = "当前状态：正在解压资源";
                    StartUn.Start();
                }
            }
        }
        private void UnFile()
        {
            if (Utils.UtilsTools.UnZip(@"GlobalFile.pkg", @""))
            {
                bool error = false;
                for (int i = 0; i < Settings.globalfiles.Length; i++)
                {
                    if (File.Exists(@"GlobalFile//" + Settings.globalfiles[i]) == false)
                    {
                        this.Dispatcher.Invoke(new Action(delegate ()
                        {
                            LogBox.Content = Settings.globalfiles[i] + "文件不存在，请重新下载资源包或尝试重新操作";
                        }));
                        error = true;
                        break;
                    }
                    this.Dispatcher.Invoke(new Action(delegate ()
                    {
                        LogBox.Content = Settings.globalfiles[i] + "存在";
                    }));
                }
                if (!error)
                {
                    MoveFile();
                    this.Dispatcher.Invoke(new Action(delegate ()
                    {
                        bqload.Visibility = Visibility.Hidden;
                    }));
                }
                else
                {
                    this.Dispatcher.Invoke(new Action(delegate ()
                    {
                        this.ShowMessageAsync("提示", "资源解压完成但不完整\r\n请重新下载", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
                        bqload.Visibility = Visibility.Hidden;
                        setSave.IsEnabled = true;
                    }));
                }
            }
            else
            {
                this.Dispatcher.Invoke(new Action(delegate ()
                {

                    this.ShowMessageAsync("提示", "没有找到资源[GlobalFile.pkg]或解压失败\r\n请重试或前往下载转换资源包", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
                    bqload.Visibility = Visibility.Hidden;
                    ToGlobal.IsEnabled = true;
                    TimeStatus.Content = "当前状态：未找到资源包";
                    setSave.IsEnabled = true;
                }));
            }
        }
        private void MoveFile()
        {
            Computer redir = new Computer();
            this.Dispatcher.Invoke(new Action(delegate ()
            {
                TimeStatus.Content = "当前状态：正在备份原文件";
            }));
            for (int a = 0; a < Settings.cnfiles.Length; a++)
            {
                String newFileName = System.IO.Path.GetFileNameWithoutExtension(Config.Settings.GameMovePath + "//Genshin Impact Game//" + Settings.cnfiles[a]) + System.IO.Path.GetExtension(Config.Settings.GameMovePath + "//Genshin Impact Game//" + Settings.cnfiles[a]);
                if (File.Exists(Config.Settings.GameMovePath + "//Genshin Impact Game//" + Settings.cnfiles[a]) == true)
                {
                    redir.FileSystem.RenameFile(Config.Settings.GameMovePath + "//Genshin Impact Game//" + Settings.cnfiles[a], newFileName + ".bak");
                    this.Dispatcher.Invoke(new Action(delegate ()
                    {
                        LogBox.Content = newFileName + "备份成功";
                    }));
                }
                else
                {
                    this.Dispatcher.Invoke(new Action(delegate ()
                    {
                        LogBox.Content = newFileName + "备份失败，跳过";
                    }));
                }
            }
            this.Dispatcher.Invoke(new Action(delegate ()
            {
                TimeStatus.Content = "当前状态：开始替换资源";
            }));
            redir.FileSystem.RenameDirectory(Config.Settings.GameMovePath + "//Genshin Impact Game//YuanShen_Data", "GenshinImpact_Data");
            for (int i = 0; i < Settings.globalfiles.Length; i++)
            {
                File.Copy(@"GlobalFile//" + Settings.globalfiles[i], Config.Settings.GameMovePath + "//Genshin Impact Game//" + Settings.globalfiles[i], true);
                this.Dispatcher.Invoke(new Action(delegate ()
                {
                    LogBox.Content = Settings.globalfiles[i] + "替换成功";
                }));
            };
            this.Dispatcher.Invoke(new Action(delegate ()
            {
                TimeStatus.Content = "当前状态：无状态";
            }));
            if (File.Exists(Config.Settings.GameMovePath + "\\Genshin Impact Game\\GenshinImpact_Data\\Plugins\\PCGameSDK.dll") == true)
                File.Delete(Config.Settings.GameMovePath + "\\Genshin Impact Game\\GenshinImpact_Data\\Plugins\\PCGameSDK.dll");
            Config.IniGS.BiOrMi = 3;
            BOM.Sub_channel("0");
            BOM.Channel("1");
            this.Dispatcher.Invoke(new Action(delegate ()
            {
                ToGlobal.IsEnabled = true;
                IsSDK();
                IsGlobal();
                setSave.IsEnabled = true;
                this.ShowMessageAsync("提示", "转换完毕，尽情享受吧！~", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
            }));
        }
        private void IsGlobal()
        {
            if (File.Exists(Config.Settings.GamePath + "//Genshin Impact Game//YuanShen.exe") == true)
            {
                ToGlobal.Content = "转换";
                GlobalS.IsEnabled = false;
                BIliS.IsEnabled = true;
                MiS.IsEnabled = true;
                MiS.IsChecked = true;

            }
            else if (File.Exists(Config.Settings.GamePath + "//Genshin Impact Game//GenshinImpact.exe") == true)
            {
                ToGlobal.Content = "复原";
                GlobalS.IsChecked = true;
                GlobalS.IsEnabled = true;
                BIliS.IsEnabled = false;
                MiS.IsEnabled = false;
            }
        }
        private void ReCnGame()
        {
            Computer redir = new Computer();
            this.Dispatcher.Invoke(new Action(delegate ()
            {
                TimeStatus.Content = "当前状态：清理现存文件";
            }));
            for (int i = 0; i < Settings.globalfiles.Length; i++)
            {
                if (File.Exists(Config.Settings.GameMovePath + "//Genshin Impact Game//" + Settings.globalfiles[i]) == true)
                {
                    File.Delete(Config.Settings.GameMovePath + "//Genshin Impact Game//" + Settings.globalfiles[i]);
                    this.Dispatcher.Invoke(new Action(delegate ()
                    {
                        LogBox.Content = Settings.globalfiles[i] + "清理完毕";
                    }));
                }
                else
                {
                    this.Dispatcher.Invoke(new Action(delegate ()
                    {
                        LogBox.Content = Settings.globalfiles[i] + "文件不存在，已跳过";
                    }));
                }
            }
            this.Dispatcher.Invoke(new Action(delegate ()
            {
                TimeStatus.Content = "当前状态：正在还原文件";
            }));
            redir.FileSystem.RenameDirectory(Config.Settings.GameMovePath + "//Genshin Impact Game//GenshinImpact_Data", "YuanShen_Data");
            int whole = 0, success = 0;
            for (int a = 0; a < Settings.cnfiles.Length; a++)
            {
                String newFileName = System.IO.Path.GetFileNameWithoutExtension(Config.Settings.GameMovePath + "//Genshin Impact Game//" + Settings.cnfiles[a]) + System.IO.Path.GetExtension(Config.Settings.GameMovePath + "//Genshin Impact Game//" + Settings.cnfiles[a]); ;
                if (File.Exists(Config.Settings.GameMovePath + "//Genshin Impact Game//" + Settings.cnfiles[a] + ".bak") == true)
                {
                    redir.FileSystem.RenameFile(Config.Settings.GameMovePath + "//Genshin Impact Game//" + Settings.cnfiles[a] + ".bak", newFileName);
                    this.Dispatcher.Invoke(new Action(delegate ()
                    {
                        LogBox.Content = Settings.cnfiles[a] + "还原成功";
                    }));
                    success++;
                }
                else
                {
                    this.Dispatcher.Invoke(new Action(delegate ()
                    {
                        LogBox.Content = Settings.cnfiles[a] + "不存在，跳过还原";
                    }));
                    whole++;
                }
            }
            //复原SDK
            var sdkUri = "pack://application:,,,/Res/mihoyosdk.dll";
            var uri = new Uri(sdkUri, UriKind.RelativeOrAbsolute);
            var stream = Application.GetResourceStream(uri).Stream;
            Utils.UtilsTools.StreamToFile(stream, Config.Settings.GameMovePath + "\\Genshin Impact Game\\YuanShen_Data\\Plugins\\PCGameSDK.dll");
            this.Dispatcher.Invoke(new Action(delegate ()
            {
                IsSDK();
                GlobalS.IsChecked = false;
                GlobalS.IsEnabled = false;
                MiS.IsChecked = true;
                BIliS.IsEnabled = true;
                MiS.IsEnabled = true;
                TimeStatus.Content = "当前状态：无状态";
                Config.IniGS.BiOrMi = 1;
                BOM.Sub_channel("1");
                BOM.Channel("1");
                Config.setConfig.checkini();
                bqload.Visibility = Visibility.Hidden;
                setSave.IsEnabled = true;
                ToGlobal.Content = "转换";
                ToGlobal.IsEnabled = true;
                this.ShowMessageAsync("提示", "还原完毕，本次还原成功" + success + "个文件，失败或缺失" + whole + "个文件", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
            }));
        }

        private void DownRes_Click(object sender, RoutedEventArgs e)
        {
            this.ShowMessageAsync("提示", "访问密码：etxd\r\n已复制到剪切板", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
            Clipboard.SetText("etxd");
            Process.Start("https://pan.baidu.com/s/1-5zQoVfE7ImdXrn8OInKqg");
        }
        //读取保存的账户文件
        private void ReadUser()
        {
            DirectoryInfo TheFolder = new DirectoryInfo(@"UserData");
            foreach (FileInfo NextFile in TheFolder.GetFiles())
                UserList.Items.Add(NextFile.Name);
        }
        //修改启动时切换的用户
        private void WriteUser()
        {
            if (UserList.SelectedIndex != -1)
            {
                string name = (UserList as ListBox).SelectedItem.ToString();
                YSAccount acct = YSAccount.ReadFromDisk(name);
                acct.WriteToRegedit();
            }
        }
        private async void DelUser_Click(object sender, RoutedEventArgs e)
        {
            if (UserList.SelectedIndex != -1)
            {
                if ((await this.ShowMessageAsync("警告", "您确定删除账号：" + (UserList as ListBox).SelectedItem.ToString() + "吗？！", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings() { AffirmativeButtonText = "取消", NegativeButtonText = "删除" })) != MessageDialogResult.Affirmative)
                {
                    File.Delete(@"UserData\\" + (UserList as ListBox).SelectedItem.ToString());
                    UserList.Items.RemoveAt(UserList.Items.IndexOf(UserList.SelectedItem));
                }
            }
            else
            {
                this.ShowMessageAsync("错误", "请选择要删除的账户再进行操作", MessageDialogStyle.Affirmative, new MetroDialogSettings() { AffirmativeButtonText = "确定" });
            }
        }
        private void XY_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object a;
            a = GameXY.SelectedValue;
            switch (a)
            {
                case 1:
                    GWidth.Text = "3840";
                    GHeight.Text = "2160";
                    break;
                case 2:
                    GWidth.Text = "2560";
                    GHeight.Text = "1080";
                    break;
                case 3:
                    GWidth.Text = "1920";
                    GHeight.Text = "1080";
                    break;
                case 4:
                    GWidth.Text = "1600";
                    GHeight.Text = "900";
                    break;
                case 45:
                    GWidth.Text = "1360";
                    GHeight.Text = "768";
                    break;
                case 6:
                    GWidth.Text = "1280";
                    GHeight.Text = "1024";
                    break;
                case 7:
                    GWidth.Text = "1280";
                    GHeight.Text = "720";
                    break;
                default:
                    break;
            }
        }

    }
}
