using Microsoft.Win32;
using System;

namespace FileSystemMonitor.Base
{
    class OSUtil
    {
        public static string HKLMWinNTCurrent = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion";
        /// <summary>
        /// 取得 OS 版本
        /// </summary>
        /// <returns></returns>
        public static string GetOSVersion()
        {
            OperatingSystem os = Environment.OSVersion;
            //Get version information about the os.
            Version vs = os.Version;

            //Variable to hold our return value
            string operatingSystem = "";

            if (os.Platform == PlatformID.Win32Windows)
            {
                //This is a pre-NT version of Windows
                switch (vs.Minor)
                {
                    case 0:
                        operatingSystem = "95";
                        break;
                    case 10:
                        if (vs.Revision.ToString() == "2222A")
                            operatingSystem = "98SE";
                        else
                            operatingSystem = "98";
                        break;
                    case 90:
                        operatingSystem = "Me";
                        break;
                    default:
                        break;
                }
            }
            else if (os.Platform == PlatformID.Win32NT)
            {
                switch (vs.Major)
                {
                    case 3:
                        operatingSystem = "NT 3.51";
                        break;
                    case 4:
                        operatingSystem = "NT 4.0";
                        break;
                    case 5:
                        if (vs.Minor == 0)
                            operatingSystem = "2000";
                        else
                            operatingSystem = "XP";
                        break;
                    case 6:
                        if (vs.Minor == 0)
                            operatingSystem = "Vista";
                        else if (vs.Minor == 1)
                            operatingSystem = "7";
                        else if (vs.Minor == 2)
                            operatingSystem = "8";
                        else
                            operatingSystem = "8.1";
                        break;
                    case 10:
                        operatingSystem = "10";
                        break;
                    default:
                        break;
                }
            }
            //Make sure we actually got something in our OS check
            //We don't want to just return " Service Pack 2" or " 32-bit"
            //That information is useless without the OS version.
            if (operatingSystem != "")
            {
                //Got something.  Let's prepend "Windows" and get more info.
                operatingSystem = "Windows " + operatingSystem;
                //See if there's a service pack installed.
                if (os.ServicePack != "")
                {
                    //Append it to the OS name.  i.e. "Windows XP Service Pack 3"
                    operatingSystem += " " + os.ServicePack;
                }
                //Append the OS architecture.  i.e. "Windows XP Service Pack 3 32-bit"
                //operatingSystem += " " + getOSArchitecture().ToString() + "-bit";
            }
            //Return the information we've gathered.
            return operatingSystem.Trim();
        }
        /// <summary>
        /// 取得 OS 版本詳細資訊
        /// </summary>
        /// <returns></returns>
        public static string GetOSFullName()
        {
            string OSFullName = Registry.GetValue(HKLMWinNTCurrent, "productName", "").ToString() + " " + Environment.OSVersion.ServicePack;
            return OSFullName.Trim();
        }
        /// <summary>
        /// 取得 OS 架構
        /// </summary>
        /// <returns>回傳{x86/x64}</returns>
        public static string GetOSArchitecture()
        {
            return Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE", EnvironmentVariableTarget.Machine).Contains("64") ? "x64" : "x86";
        }
        /// <summary>
        /// 取得 OS Release資訊
        /// </summary>
        /// <returns></returns>
        public static string GetOSRelease()
        {
            string OSRelease = Registry.GetValue(HKLMWinNTCurrent, "ReleaseId", "").ToString();
            return OSRelease.Trim();
        }
        /// <summary>
        /// 取得 OS NT Version資訊
        /// </summary>
        /// <returns></returns>
        public static string GetOSNTVersion()
        {
            string OSVersion = Environment.OSVersion.Version.ToString();
            return OSVersion.Trim();
        }
        /// <summary>
        /// 取得 OS Build資訊
        /// </summary>
        /// <returns></returns>
        public static string GetOSBuild()
        {
            string OSBuild = Registry.GetValue(HKLMWinNTCurrent, "CurrentBuildNumber", "").ToString();
            return OSBuild.Trim();
        }
        /// <summary>
        /// 取得 OS UBR資訊
        /// </summary>
        /// <returns></returns>
        public static string GetOSUBR()
        {
            string OSUBR = Registry.GetValue(HKLMWinNTCurrent, "UBR", "").ToString();
            return OSUBR.Trim();
        }
    }
}
