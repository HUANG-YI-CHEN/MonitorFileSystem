using System;
using System.IO;
using System.Windows.Forms;

namespace FileSystemMonitor
{
    using static FileSystemMonitor.Base.DirectorExpansion;
    public partial class Main : Form
    {      
        public Main()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string folderPath = "";
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;
                folderBrowserDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                folderBrowserDialog.Description = "Select the root path of the excutable file.";
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    folderPath = folderBrowserDialog.SelectedPath;
                else
                    folderPath = "";
            }
            if (!string.IsNullOrEmpty(folderPath))
            {
                foreach (var currentFile in EnumerateFiles(folderPath, "*.txt", SearchOption.TopDirectoryOnly))
                {
                    DirectoryInfo df = new DirectoryInfo(currentFile);
                    FileInfo f = new FileInfo(currentFile);
                    if ( (DateTime.Now - (f.CreationTime <= f.LastWriteTime ? f.CreationTime : f.LastWriteTime)).Days > 7)
                    {
                        string DirName = df.FullName.Replace(@"\" + df.Name, "");
                        string YearDir = DirName + @"\" + (f.CreationTime <= f.LastWriteTime ? f.CreationTime.ToString("yyyy") : f.LastWriteTime.ToString("yyyy"));
                        string MonthDir = YearDir + @"\" + (f.CreationTime <= f.LastWriteTime ? f.CreationTime.ToString("MM") : f.LastWriteTime.ToString("MM"));
                        string DateDir = MonthDir + @"\" + (f.CreationTime <= f.LastWriteTime ? f.CreationTime.ToString("yyyyMMdd") : f.LastWriteTime.ToString("yyyyMMdd"));

                        if (!Directory.Exists(YearDir))
                            Directory.CreateDirectory(YearDir);
                        if (!Directory.Exists(MonthDir))
                            Directory.CreateDirectory(MonthDir);
                        if (!Directory.Exists(DateDir))
                            Directory.CreateDirectory(DateDir);
                        if (!File.Exists(DateDir + @"\" + df.Name))
                            File.Move(currentFile, DateDir + @"\" + df.Name);
                    }
                }
            }

        }
    }
}
