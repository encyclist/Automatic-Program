using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;

namespace Automatic_Program
{
    /// <summary>
    /// USB控制设备类型
    /// </summary>
    public struct USBControllerDevice
    {
        /// <summary>
        /// USB控制器设备ID
        /// </summary>
        public string Antecedent;

        /// <summary>
        /// USB即插即用设备ID
        /// </summary>
        public string Dependent;
    }


    public class DeviceNotifier
    {
        /// <summary>
        /// USB插入事件监视
        /// </summary>
        private ManagementEventWatcher insertWatcher = null;

        /// <summary>
        /// USB拔出事件监视
        /// </summary>
        private ManagementEventWatcher removeWatcher = null;

        /// <summary>
        /// 添加USB事件监视器
        /// </summary>
        /// <param name="usbInsertHandler">USB插入事件处理器</param>
        /// <param name="usbRemoveHandler">USB拔出事件处理器</param>
        /// <param name="withinInterval">发送通知允许的滞后时间</param>
        public bool AddUSBEventWatcher(EventArrivedEventHandler usbInsertHandler, EventArrivedEventHandler usbRemoveHandler, TimeSpan withinInterval)
        {
            try
            {
                ManagementScope Scope = new ManagementScope("root\\CIMV2");
                Scope.Options.EnablePrivileges = true;

                // USB插入监视
                if (usbInsertHandler != null)
                {
                    var insertQuery = GetUsbQueryParamter("__InstanceCreationEvent", withinInterval);
                    insertWatcher = new ManagementEventWatcher(Scope, insertQuery);
                    insertWatcher.EventArrived += usbInsertHandler;
                    insertWatcher.Start();
                }

                // USB拔出监视
                if (usbRemoveHandler != null)
                {
                    var removeQuery = GetUsbQueryParamter("__InstanceDeletionEvent", withinInterval);
                    removeWatcher = new ManagementEventWatcher(Scope, removeQuery);
                    removeWatcher.EventArrived += usbRemoveHandler;
                    removeWatcher.Start();
                }

                return true;
            }

            catch (Exception)
            {
                RemoveUSBEventWatcher();
                return false;
            }
        }

        private WqlEventQuery GetUsbQueryParamter(string eventClassName, TimeSpan withinInterval)
        {
            return new WqlEventQuery(eventClassName, withinInterval, "TargetInstance isa 'Win32_USBControllerDevice'");
        }

        /// <summary>
        /// 移去USB事件监视器
        /// </summary>
        public void RemoveUSBEventWatcher()
        {
            if (insertWatcher != null)
            {
                insertWatcher.Stop();
                insertWatcher.Dispose();
                insertWatcher = null;
            }

            if (removeWatcher != null)
            {
                removeWatcher.Stop();
                removeWatcher.Dispose();
                removeWatcher = null;
            }
        }

        /// <summary>
        /// 定位发生插拔的USB设备
        /// </summary>
        /// <param name="e">USB插拔事件参数</param>
        /// <returns>发生插拔现象的USB控制设备ID</returns>
        public static USBControllerDevice[] WhoUSBControllerDevice(EventArrivedEventArgs e)
        {
            ManagementBaseObject mbo = e.NewEvent["TargetInstance"] as ManagementBaseObject;
            if (mbo != null && mbo.ClassPath.ClassName == "Win32_USBControllerDevice")
            {
                string Antecedent = (mbo["Antecedent"] as string).Replace("\"", string.Empty).Split(new Char[] { '=' })[1];
                string Dependent = (mbo["Dependent"] as string).Replace("\"", string.Empty).Split(new Char[] { '=' })[1];
                return new USBControllerDevice[] { new USBControllerDevice { Antecedent = Antecedent, Dependent = Dependent } };
            }

            return null;
        }
    }
}
