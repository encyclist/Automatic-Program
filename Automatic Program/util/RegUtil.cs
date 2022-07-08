using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trip.util
{
    class RegUtil
    {
        //开机自动启动
        public static void SelfRunning()
        {
            //可获得当前执行的exe的文件名。
            string str1 = Process.GetCurrentProcess().MainModule.FileName;
            //获取和设置当前目录(即该进程从中启动的目录)的完全限定路径。 备注 按照定义，如果该进程在本地或网络驱动器的根目录中启动，则此属性的值为驱动器名称后跟一个尾部反斜杠(如“C:\”)。如果该进程在子目录中启动，则此属性的值为不带尾部反斜杠的驱动器和子目录路径(如“C:\mySubDirectory”)。
            //string str2 = Environment.CurrentDirectory;
            //获取应用程序的当前工作目录。
            //string str3 = Directory.GetCurrentDirectory();
            ////获取基目录，它由程序集冲突解决程序用来探测程序集。
            ////string str4 = AppDomain.CurrentDomain.BaseDirectory;
            ////获取或设置包含该应用程序的目录的名称。
            ////string str7 = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            //RegistryKey rgkRun = null;
            //try
            //{
            //    rgkRun = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            //}
            //catch
            //{

            //}
            //if (rgkRun == null)
            //{
            //    rgkRun = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
            //}
            //rgkRun.SetValue("trip", str1);

            if (!IsExistRunKey("trip"))
            {
                RegRun(true, "trip", str1);
            }
        }

        //判断注册表键值对是否存在
        public static bool IsExistRunKey(string keyName)
        {
            try
            {
                bool _exist = false;
                RegistryKey local = Registry.LocalMachine;
                RegistryKey runs = local.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (runs == null)
                {
                    RegistryKey key2 = local.CreateSubKey("SOFTWARE");
                    RegistryKey key3 = key2.CreateSubKey("Microsoft");
                    RegistryKey key4 = key3.CreateSubKey("Windows");
                    RegistryKey key5 = key4.CreateSubKey("CurrentVersion");
                    RegistryKey key6 = key5.CreateSubKey("Run");
                    runs = key6;
                }
                string[] runsName = runs.GetValueNames();
                foreach (string strName in runsName)
                {
                    if (strName.ToUpper() == keyName.ToUpper())
                    {
                        _exist = true;
                        return _exist;
                    }
                }
                return _exist;

            }
            catch
            {
                return false;
            }
        }

        //写入/删除注册表键值对
        ///isStart--是否开机自启动
        ///exeName--应用程序名
        ///path--应用程序路径
        public static bool RegRun(bool isStart, string exeName, string path)
        {
            try
            {
                RegistryKey local = Registry.LocalMachine;
                RegistryKey key = local.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (key == null)
                {
                    local.CreateSubKey("SOFTWARE//Microsoft//Windows//CurrentVersion//Run");
                }
                if (isStart)//若开机自启动则添加键值对
                {
                    key.SetValue(exeName, path);
                    key.Close();
                }
                else//否则删除键值对
                {
                    string[] keyNames = key.GetValueNames();
                    foreach (string keyName in keyNames)
                    {
                        if (keyName.ToUpper() == exeName.ToUpper())
                        {
                            key.DeleteValue(exeName);
                            key.Close();
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
                //throw;
            }

            return true;
        }
    }
}
