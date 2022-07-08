using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Automatic_Program
{
    public class SessionSwitch
    {
        /// <summary>
        /// 解屏后执行的委托
        /// </summary>
        public Action SessionUnlockAction { get; set; }

        /// <summary>
        /// 锁屏后执行的委托
        /// </summary>
        public Action SessionLockAction { get; set; }

        public SessionSwitch()
        {
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        //析构，防止句柄泄漏
        ~SessionSwitch()
        {
            //Do this during application close to avoid handle leak
            SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
        }

        //当前登录的用户变化（登录、注销和解锁屏）
        public void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                //用户登录
                case SessionSwitchReason.SessionLogon:
                    BeginSessionUnlock();
                    break;
                //解锁屏
                case SessionSwitchReason.SessionUnlock:
                    BeginSessionUnlock();
                    break;
                //锁屏
                case SessionSwitchReason.SessionLock:
                    BeginSessionLock();
                    break;
                //注销
                case SessionSwitchReason.SessionLogoff:
                    break;
            }
        }

        /// <summary>
        /// 解屏、登录后执行
        /// </summary>
        private void BeginSessionUnlock()
        {
            //解屏、登录后执行
            if(SessionUnlockAction != null)
            {
                SessionUnlockAction.Invoke();
            }
        }

        /// <summary>
        /// 锁屏后执行
        /// </summary>
        private void BeginSessionLock()
        {
            //锁屏后执行
            if (SessionLockAction != null)
            {
                SessionLockAction.Invoke();
            }
        }
    }
}
