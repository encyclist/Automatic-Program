using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace ToolsCommon
{
    /// <summary>
    /// INI文件文件操作
    /// </summary>
    public class IniFile
    {
        private static readonly IniFile Instance = new IniFile(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\config.ini");
        public static IniFile GetInstance()
        {
            return Instance;
        }





        public string path { get; private set; }

        private IniFile(string INIPath)
        {
            path = INIPath;
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);


        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defVal, Byte[] retVal, int size, string filePath);


        /// <summary>
                /// 写INI文件
                /// </summary>
                /// <param name="Section"></param>
                /// <param name="Key"></param>
                /// <param name="Value"></param>
        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, this.path);
        }

        /// <summary>
                /// 读取INI文件
                /// </summary>
                /// <param name="Section"></param>
                /// <param name="Key"></param>
                /// <returns></returns>
        public string IniReadValue(string Section, string Key, string defaultValue)
        {
            string temp = IniReadValue(Section, Key);
            if (temp == null || temp.Length < 1)
            {
                return defaultValue;
            }
            else
            {
                return temp;
            }
        }
        public string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, this.path);
            return temp.ToString();
        }
        public byte[] IniReadValues(string section, string key)
        {
            byte[] temp = new byte[255];
            int i = GetPrivateProfileString(section, key, "", temp, 255, this.path);
            return temp;

        }


        /// <summary>
                /// 删除ini文件下所有段落
                /// </summary>
        public void ClearAllSection()
        {
            IniWriteValue(null, null, null);
        }
        /// <summary>
        /// 删除ini文件下personal段落下的所有键
        /// </summary>
        /// <param name="Section"></param>
        public void ClearSection(string Section)
        {
            IniWriteValue(Section, null, null);
        }

    }
}