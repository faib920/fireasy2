// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Management;
using System.Net;
using System.Net.Sockets;
#if NETSTANDARD
using System.Runtime.InteropServices;
#endif

namespace Fireasy.App.Licence
{
    internal class MachineKeyHelper
    {
        internal static string GetMachineKey(MachineKeyKinds kind)
        {
            switch (kind)
            {
                case MachineKeyKinds.MacAddress:
                    return GetMacAddress();
                case MachineKeyKinds.CPUSerialNumber:
                    return GetCPUSerialNumber();
                case MachineKeyKinds.DiskSerialNumber:
                    return GetDiskSerialNumber();
                case MachineKeyKinds.USBSerialNumber:
                    return GetUSBSerialNumber();
                case MachineKeyKinds.HostName:
                    return GetHostName();
                case MachineKeyKinds.IpAddress:
                    return GetIpAddress();
                default:
                    return string.Empty;
            }
        }

        private static string GetMacAddress()
        {
#if NETSTANDARD
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return string.Empty;
            }
#endif
            using (var mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
            using (var moc = mc.GetInstances())
            {
                foreach (var mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        return mo["MacAddress"].ToString();
                    }
                }
            }

            return string.Empty;
        }

        private static string GetCPUSerialNumber()
        {
#if NETSTANDARD
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return string.Empty;
            }
#endif
            using (var mc = new ManagementClass("Win32_Processor"))
            using (var moc = mc.GetInstances())
            {
                foreach (var mo in moc)
                {
                    return mo["ProcessorId"].ToString();
                }
            }

            return string.Empty;
        }

        private static string GetDiskSerialNumber()
        {
#if NETSTANDARD
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return string.Empty;
            }
#endif
            using (var mc = new ManagementClass("Win32_DiskDrive"))
            using (var moc = mc.GetInstances())
            {
                foreach (var mo in moc)
                {
                    if (mo["Name"].ToString() == @"\\.\PHYSICALDRIVE0")
                    {
                        return mo["SerialNumber"].ToString();
                    }
                }
            }

            return string.Empty;
        }

        private static string GetUSBSerialNumber()
        {
#if NETSTANDARD
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return string.Empty;
            }
#endif
            using (var mc = new ManagementClass("Win32_DiskDrive"))
            using (var moc = mc.GetInstances())
            {
                foreach (var mo in moc)
                {
                    if (mo["InterfaceType"].ToString() == "USB")
                    {
                        return mo["SerialNumber"].ToString();
                    }
                }
            }

            return string.Empty;
        }

        private static string GetHostName()
        {
            return Dns.GetHostName();
        }

        private static string GetIpAddress()
        {
            var ips = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in ips.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return string.Empty;
        }
    }
}
