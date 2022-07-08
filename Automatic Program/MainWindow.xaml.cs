using Automatic_Program.util;
using Notifications.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using ToolsCommon;
using trip.util;
using UsbMonitor;
using WindowsInput;
using WindowsInput.Native;

namespace Automatic_Program
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private DeviceNotifier deviceNotifier = new DeviceNotifier();
        private bool adbWifiRunning = false;

        public MainWindow()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            if (AdminUtil.IsRunAsAdmin())
                RegUtil.SelfRunning();

            ReadConfig();

            // 开始监听USB事件
            deviceNotifier.AddUSBEventWatcher(HandleDeviceConnect, HandleDeviceDisConnect, TimeSpan.FromMilliseconds(3000));
            // 监听解锁屏幕
            SessionSwitch SessionCheck = new SessionSwitch
            {
                SessionUnlockAction = DeviceUnlock
            };

            InitialTray();
        }

        private void ReadConfig()
        {
            try
            {
                bool autoAdbWifi = Boolean.Parse(IniFile.GetInstance().IniReadValue("default", "AutoAdbWifi", "false"));
                bool autoOpenQq = Boolean.Parse(IniFile.GetInstance().IniReadValue("default", "AutoOpenQq", "false"));
                bool autoOpenTim = Boolean.Parse(IniFile.GetInstance().IniReadValue("default", "AutoOpenTim", "false"));
                bool autoOpenWeChat = Boolean.Parse(IniFile.GetInstance().IniReadValue("default", "AutoOpenWeChat", "false"));
                bool AutoOpenWxWork = Boolean.Parse(IniFile.GetInstance().IniReadValue("default", "AutoOpenWxWork", "false"));
                Check_AdbWifi.IsChecked = autoAdbWifi;
                Check_Qq.IsChecked = autoOpenQq;
                Check_Tim.IsChecked = autoOpenTim;
                Check_WeChat.IsChecked = autoOpenWeChat;
                Check_WxWork.IsChecked = AutoOpenWxWork;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
            }
        }

        #region 最小化系统托盘
        private void InitialTray()
        {
            //隐藏主窗体
            this.Visibility = Visibility.Hidden;
            //设置托盘的各个属性
            NotifyIcon _notifyIcon = new NotifyIcon
            {
                BalloonTipText = "自动程序已最小化运行",//托盘气泡显示内容
                Text = "自动程序",
                Visible = true,//托盘按钮是否可见
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath)//托盘中显示的图标
            };

            _notifyIcon.ShowBalloonTip(2000);//托盘气泡显示时间
            _notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
            //窗体状态改变时触发
            this.StateChanged += MainWindow_StateChanged;
        }
        #endregion

        #region 窗口状态改变
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal; // 取消最小化的操作
                this.Visibility = Visibility.Hidden; // 隐藏窗口
            }
        }
        #endregion

        #region 托盘图标鼠标单击事件
        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (this.Visibility == Visibility.Visible)
                {
                    this.Visibility = Visibility.Hidden;
                }
                else
                {
                    this.Visibility = Visibility.Visible;
                    this.Activate();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
            }
        }

        #endregion

        // 处理设备连接的事件
        private void HandleDeviceConnect(object sender, EventArrivedEventArgs e)
        {
            Console.WriteLine("有设备插入");
            var watcher = sender as ManagementEventWatcher;
            watcher.Stop();
            bool autoAdbWifi = Boolean.Parse(IniFile.GetInstance().IniReadValue("default", "AutoAdbWifi", "false"));
            if (autoAdbWifi && !adbWifiRunning)
            {
                adbWifiRunning = true;
                Task.Run(async delegate
                {
                    await Task.Delay(3000);
                    AutoAdbWifi();
                    adbWifiRunning = false;
                });
            }
            watcher.Start();
        }
        // 处理设备离开的事件
        private void HandleDeviceDisConnect(object sender, EventArrivedEventArgs e)
        {
            Console.WriteLine("有设备离开");
        }

        private void AutoAdbWifi_Changed(object sender, RoutedEventArgs e)
        {
            bool autoAdbWifi = Check_AdbWifi.IsChecked == true;
            IniFile.GetInstance().IniWriteValue("default", "AutoAdbWifi", autoAdbWifi.ToString());
        }

        private void AutoOpenQq_Changed(object sender, RoutedEventArgs e)
        {
            bool autoOpenQq = Check_Qq.IsChecked == true;
            IniFile.GetInstance().IniWriteValue("default", "AutoOpenQq", autoOpenQq.ToString());
        }

        private void AutoOpenTim_Changed(object sender, RoutedEventArgs e)
        {
            bool autoOpenTim = Check_Tim.IsChecked == true;
            IniFile.GetInstance().IniWriteValue("default", "AutoOpenTim", autoOpenTim.ToString());
        }

        private void AutoOpenWeChat_Changed(object sender, RoutedEventArgs e)
        {
            bool autoOpenWeChat = Check_WeChat.IsChecked == true;
            IniFile.GetInstance().IniWriteValue("default", "AutoOpenWeChat", autoOpenWeChat.ToString());
        }

        private void AutoOpenWxWork_Changed(object sender, RoutedEventArgs e)
        {
            bool autoOpenWxWork = Check_WxWork.IsChecked == true;
            IniFile.GetInstance().IniWriteValue("default", "AutoOpenWxWork", autoOpenWxWork.ToString());
        }

        // 自动把手机的ADB WIFI调试打开
        private void AutoAdbWifi()
        {
            List<String> devices = GetAndroidDevices();
            foreach (String device in devices)
            {
                if (device.Contains(":"))
                {
                    // 有:的是IP地址，代表已经连接了的设备
                    continue;
                }
                string ip = GetAndroidIp(device);
                int connectResult = ConnectAndroidAdbWifi(ip);
                if (connectResult == 0)
                {
                    // 连接失败 打开WIFI调试再试
                    OpenAndroidAdbWifi(device);
                    if (ConnectAndroidAdbWifi(ip) != 0)
                    {
                        ShowToast("连接成功", "已经连接到：" + ip);
                    }
                }else if(connectResult == 1)
                {
                    // 以前打开了WIFI调试但没连接，现在连接成功了
                    ShowToast("连接成功", "已经连接到：" + ip);
                }
            }
        }

        // 获取安卓设备列表
        private List<String> GetAndroidDevices()
        {
            List<String> devices = new List<string>();
            try
            {
                string result = ExecuteCmd("adb devices");
                Console.WriteLine(result);
                string[] lines = result.Split('\n');
                if (lines.Length > 5)
                {
                    for (int i = 5; i < lines.Length; i++)
                    {
                        string line = lines[i];
                        if (line.Trim().Length < 1)
                        {
                            continue;
                        }
                        Console.WriteLine(line);
                        string[] pair = line.Split('\t');
                        devices.Add(pair[0]);
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
            }
            return devices;
        }

        // 打开安卓的WIFI调试
        private void OpenAndroidAdbWifi(string device)
        {
            try
            {
                string result = ExecuteCmd("adb -s " + device + " tcpip 5555");
                Console.WriteLine(result);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
            }
        }

        // 获取安卓设备IP地址
        private string GetAndroidIp(string deviceId)
        {
            try
            {
                string result = ExecuteCmd("adb -s " + deviceId + " shell ifconfig wlan0");
                Console.WriteLine(result);
                string[] lines = result.Split('\n');
                if (lines.Length > 5)
                {
                    string line = lines[5];
                    string ip = line.Trim().Split(' ')[1].Split(':')[1];
                    Console.WriteLine("deviceIp:" + ip);
                    return ip;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
            }

            return "";
        }

        // 通过WIFI连接到安卓手机调试
        // 0 连接失败 1连接成功 2已经连接了
        private int ConnectAndroidAdbWifi(string ip)
        {
            try
            {
                string result = ExecuteCmd("adb connect " + ip);
                Console.WriteLine(result);
                if (result.Contains("already connected to"))
                {
                    return 2;
                }
                else if (result.Contains("connected to"))
                {
                    return 1;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
            }
            return 0;
        }

        // 执行CMD命令
        private string ExecuteCmd(string strInput)
        {
            Process p = new Process();
            //设置要启动的应用程序
            p.StartInfo.FileName = "cmd.exe";
            //是否使用操作系统shell启动
            p.StartInfo.UseShellExecute = false;
            // 接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardInput = true;
            //输出信息
            p.StartInfo.RedirectStandardOutput = true;
            // 输出错误
            p.StartInfo.RedirectStandardError = true;
            //不显示程序窗口
            p.StartInfo.CreateNoWindow = true;
            //启动程序
            p.Start();

            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(strInput + "&exit");
            p.StandardInput.AutoFlush = true;
            //获取输出信息
            string strOuput = p.StandardOutput.ReadToEnd();
            //等待程序执行完退出进程
            p.WaitForExit();
            p.Close();

            return strOuput;
        }

        // 发送Toast通知
        private void ShowToast(string title, string message)
        {
            var notificationManager = new NotificationManager();

            notificationManager.Show(new NotificationContent
            {
                Title = title,
                Message = message,
                Type = NotificationType.Information
            });
        }

        // 设备解锁
        private void DeviceUnlock()
        {
            bool autoOpenQq = Boolean.Parse(IniFile.GetInstance().IniReadValue("default", "AutoOpenQq","false"));
            bool autoOpenTim = Boolean.Parse(IniFile.GetInstance().IniReadValue("default", "AutoOpenTim", "false"));
            bool autoOpenWeChat = Boolean.Parse(IniFile.GetInstance().IniReadValue("default", "AutoOpenWeChat", "false"));
            bool AutoOpenWxWork = Boolean.Parse(IniFile.GetInstance().IniReadValue("default", "AutoOpenWxWork", "false"));

            if (autoOpenQq && !ProcessUtil.IsRun("QQ"))
            {
                ProcessUtil.CallOutProcess("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\腾讯软件\\QQ\\腾讯QQ.lnk");
            }
            if (autoOpenWeChat && !ProcessUtil.IsRun("WeChat"))
            {
                ProcessUtil.CallOutProcess("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\微信\\微信.lnk");
            }
            if (autoOpenTim && !ProcessUtil.IsRun("TIM"))
            {
                ProcessUtil.CallOutProcess("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\腾讯软件\\TIM\\TIM.lnk");
            }
            if (AutoOpenWxWork && !ProcessUtil.IsRun("WXWork"))
            {
                ProcessUtil.CallOutProcess("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\企业微信\\企业微信.lnk");
            }
        }

        [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }

}