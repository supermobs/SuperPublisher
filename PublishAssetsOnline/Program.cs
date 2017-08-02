using System;
using System.Configuration;
using System.IO;

namespace PublishAssetsOnline
{
    class Program
    {
        static void CopyDirectory(string srcDir, string tgtDir)
        {
            DirectoryInfo source = new DirectoryInfo(srcDir);
            DirectoryInfo target = new DirectoryInfo(tgtDir);

            if (target.FullName.StartsWith(source.FullName, StringComparison.CurrentCultureIgnoreCase))
                throw new Exception("父目录不能拷贝到子目录！");

            if (!source.Exists)
                return;

            if (!target.Exists)
                target.Create();

            foreach (FileInfo fi in source.GetFiles())
                File.Copy(fi.FullName, target.FullName + @"\" + fi.Name, true);
            foreach (DirectoryInfo di in source.GetDirectories())
                CopyDirectory(di.FullName, target.FullName + @"\" + di.Name);
        }

        static void DiffCopyDirectory(string srcDir, string tgtDir, string comDir)
        {
            DirectoryInfo source = new DirectoryInfo(srcDir);
            DirectoryInfo target = new DirectoryInfo(tgtDir);
            DirectoryInfo com = new DirectoryInfo(comDir);

            if (target.FullName.StartsWith(source.FullName, StringComparison.CurrentCultureIgnoreCase))
                throw new Exception("父目录不能拷贝到子目录！");

            if (!source.Exists)
                return;


            foreach (FileInfo fi in source.GetFiles())
            {
                if (!File.Exists(com.FullName + @"\" + fi.Name))
                {
                    if (!target.Exists) target.Create();
                    File.Copy(fi.FullName, target.FullName + @"\" + fi.Name, true);
                }
            }
            foreach (DirectoryInfo di in source.GetDirectories())
                CopyDirectory(di.FullName, target.FullName + @"\" + di.Name);
        }

        static void Main(string[] args)
        {
            // 完整版本路径留下一个最新的做对比
            DirectoryInfo androidLastVersion = null, iosLastVersion = null;
            foreach (DirectoryInfo di in new DirectoryInfo(ConfigurationManager.AppSettings["full_version_path"].ToString()).GetDirectories())
            {
                string platform = di.Name.Split('_')[0];
                DirectoryInfo cur = platform == "ios" ? iosLastVersion : androidLastVersion;
                if (cur == null || di.LastAccessTime > cur.LastAccessTime)
                {
                    if (cur != null) cur.Delete(true);
                    cur = di;
                }
                if (platform == "ios")
                    iosLastVersion = cur;
                else
                    androidLastVersion = cur;
            }

            // 拷贝最新版本
            string androidFloderName = "android_" + File.ReadAllLines(ConfigurationManager.AppSettings["build_version_path_android"].ToString() + @"\version.txt")[0];
            string iosFloderName = "ios_" + File.ReadAllLines(ConfigurationManager.AppSettings["build_version_path_ios"].ToString() + @"\version.txt")[0];
            CopyDirectory(ConfigurationManager.AppSettings["build_version_path_android"].ToString(),
                ConfigurationManager.AppSettings["full_version_path"].ToString() + @"\" + androidFloderName);
            CopyDirectory(ConfigurationManager.AppSettings["build_version_path_ios"].ToString(),
                ConfigurationManager.AppSettings["full_version_path"].ToString() + @"\" + iosFloderName);

            // 对比结果的路径
            string diffpath = ConfigurationManager.AppSettings["diff_version_path"].ToString() + @"\" + string.Format("{0:D4}-{1:D2}-{2:D2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            if (!Directory.Exists(diffpath)) Directory.CreateDirectory(diffpath);
            int order = 1;
            while (true)
            {
                string path = diffpath + @"\" + order + @"\client";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                if (new DirectoryInfo(path).GetDirectories().Length == 0)
                {
                    diffpath = path;
                    break;
                }
                order++;
            }

            // 对比文件并且拷贝进去
            DiffCopyDirectory(ConfigurationManager.AppSettings["full_version_path"].ToString() + @"\" + androidFloderName,
                diffpath + @"\" + androidFloderName, androidLastVersion != null ? androidLastVersion.FullName : @"C:\");
            DiffCopyDirectory(ConfigurationManager.AppSettings["full_version_path"].ToString() + @"\" + iosFloderName,
                diffpath + @"\" + iosFloderName, iosLastVersion != null ? iosLastVersion.FullName : @"C:\");
        }
    }
}
