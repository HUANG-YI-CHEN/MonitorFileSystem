using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace FileSystemMonitor.Base
{
    public static class LogHelper
    {
        private static readonly object LockFile = new object();
        /// <summary>
        /// 建立 Log 紀錄, 路徑: <Execution Path>\Log\<yyyyMMdd.txt>
        /// </summary>
        /// <param name="Message"></param>
        public static void LogTrace(string Message)
        {
            LogTrace(string.Empty, Message);
        }
        /// <summary>
        /// 建立 Log 紀錄, 路徑: <Execution Path>\Log\<UserDirName>\<yyyyMMdd.txt>
        /// </summary>
        /// <param name="UserDirName"></param>
        /// <param name="Message"></param>
        public static void LogTrace(string UserDirName, string Message)
        {
            DateTime GetNow = DateTime.Now;
            string DirBase = AppDomain.CurrentDomain.BaseDirectory + @"\Log\";
            string DirName = DirBase + (string.IsNullOrEmpty(UserDirName) ? String.Empty : UserDirName + @"\");
            string FileName = DirName + GetNow.ToString("yyyy") + @"\" + GetNow.ToString("MM") + @"\" + GetNow.ToString("yyyyMMdd") + ".txt";
            if (!Directory.Exists(DirBase))
                Directory.CreateDirectory(DirBase);
            if (!Directory.Exists(DirName))
                Directory.CreateDirectory(DirName);
            if (!Directory.Exists(DirName + GetNow.ToString("yyyy")))
                Directory.CreateDirectory(DirName + GetNow.ToString("yyyy"));
            if (!Directory.Exists(DirName + GetNow.ToString("yyyy") + @"\" + GetNow.ToString("MM")))
                Directory.CreateDirectory(DirName + GetNow.ToString("yyyy") + @"\" + GetNow.ToString("MM"));
            if (!File.Exists(FileName))
                using (var f = File.Create(FileName)) { f.Close(); }

            using (var fs = new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                using (var log = new StreamWriter(fs, Encoding.Default))
                {
                    lock (LockFile)
                    {
                        log.WriteLine("[" + GetNow.ToString("yyyy/MM/dd HH:mm:ss.fff") + "] " + Message);
                    }
                }
            }
        }
    }

    public static class DirectorExpansion
    {
		[DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
		private static extern IntPtr FindFirstFile(string pFileName, ref WIN32_FIND_DATA pFindFileData);

		[DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
		private static extern bool FindNextFile(IntPtr hndFindFile, ref WIN32_FIND_DATA lpFindFileData);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool FindClose(IntPtr hndFindFile);

		[Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto), BestFitMapping(false)]
		internal struct WIN32_FIND_DATA
		{
			public FileAttributes dwFileAttributes;
			public uint ftCreationTime_dwLowDateTime;
			public uint ftCreationTime_dwHighDateTime;
			public uint ftLastAccessTime_dwLowDateTime;
			public uint ftLastAccessTime_dwHighDateTime;
			public uint ftLastWriteTime_dwLowDateTime;
			public uint ftLastWriteTime_dwHighDateTime;
			public uint nFileSizeHigh;
			public uint nFileSizeLow;
			public int dwReserved0;
			public int dwReserved1;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string cFileName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
			public string cAlternateFileName;
		}
		private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
		public static IEnumerable<string> EnumerateFiles(string path, string searchPattern = "*.*", SearchOption searchOption = SearchOption.AllDirectories)
		{
			IntPtr hFind = INVALID_HANDLE_VALUE;
			WIN32_FIND_DATA FindFileData = default(WIN32_FIND_DATA);

			hFind = FindFirstFile(Path.Combine(path, searchPattern), ref FindFileData);
			if (hFind != INVALID_HANDLE_VALUE)
			{
				do
				{
					if (FindFileData.cFileName.Equals(@".") || FindFileData.cFileName.Equals(@".."))
						continue;

					if (searchOption == SearchOption.AllDirectories && ((FindFileData.dwFileAttributes & FileAttributes.Directory) == FileAttributes.Directory))
					{
						foreach (var file in EnumerateFiles(Path.Combine(path, FindFileData.cFileName)))
							yield return file;
					}
					else
					{
						yield return Path.Combine(path, FindFileData.cFileName);
					}
				}
				while (FindNextFile(hFind, ref FindFileData));
			}
			FindClose(hFind);
		}

	}
}
