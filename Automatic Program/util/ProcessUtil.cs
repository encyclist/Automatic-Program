using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automatic_Program.util
{
    class ProcessUtil
    {
        // 判断进程是否运行
        public static bool IsRun(string ProcessName)
        {
            try
            {
                Process[] getProcessName = Process.GetProcesses();
                foreach (Process pro in getProcessName)
                {
                    if (pro.ProcessName == ProcessName)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // 启动外部进程
        public static void CallOutProcess(string path)
        {
            ProcessStartInfo pinfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = path
            };
            //启动进程
            _ = Process.Start(pinfo);
        }

        public static IntPtr GetMainWindowHandle(string ProcessName)
        {
            try
            {
                Process[] getProcessName = Process.GetProcesses();
                foreach (Process pro in getProcessName)
                {
                    if (pro.ProcessName == ProcessName)
                    {
                        return pro.Handle;
                    }
                }
                return (IntPtr)null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
