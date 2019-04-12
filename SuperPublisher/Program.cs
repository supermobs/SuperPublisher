using Qiniu.Common;
using Qiniu.Http;
using Qiniu.IO;
using Qiniu.IO.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Serialization;

namespace SuperMobs.ShellPublish
{
    public class Program
    {
        [Serializable]
        public class State
        {
            static State _instance = null;
            public static State ins
            {
                get
                {
                    if (_instance == null)
                    {
                        try
                        {
                            FileStream fs = File.OpenRead(www_root + "state.xml");
                            XmlSerializer xsl = new XmlSerializer(typeof(State));
                            _instance = xsl.Deserialize(fs) as State;
                            fs.Close();
                        }
                        catch
                        {
                            _instance = new State();
                        }
                    }
                    return _instance;
                }
            }

            public static void Save()
            {
                File.Delete(www_root + "state.xml");
                FileStream fs = File.OpenWrite(www_root + "state.xml");
                XmlSerializer xsl = new XmlSerializer(typeof(State));
                xsl.Serialize(fs, _instance);
                fs.Flush();
                fs.Close();
            }


            public List<string> packageTypes = new List<string>();
            public List<string> packageVers = new List<string>();
            public List<string> packageExtVers = new List<string>();

            public string GetVer(string type)
            {
                int index = packageTypes.IndexOf(type);
                if (index == -1)
                    return string.Empty;
                return packageVers[index];
            }
            public string GetExtVer(string type)
            {
                int index = packageTypes.IndexOf(type);
                if (index == -1)
                    return string.Empty;
                return packageExtVers[index];
            }
            public void SetVer(string type, string ver, string exVer)
            {
                int index = packageTypes.IndexOf(type);
                if (index == -1)
                {
                    packageTypes.Add(type);
                    packageVers.Add(ver);
                    packageExtVers.Add(exVer);
                }
                else
                {
                    packageVers[index] = ver;
                    packageExtVers[index] = exVer;
                }
            }

            public static string FillPackInfo(string template, string packType)
            {
                string platform = packType.Substring(0, 3) == "ipa" ? "ios" : "android";
                string type = packType.Substring(4);
                string ver = ins.GetVer(packType);
                string exver = ins.GetExtVer(packType);
                string urlname = type + "." + ver + (string.IsNullOrEmpty(exver) ? "" : ("." + exver));
                string name = string.IsNullOrEmpty(ver) ? "none" : (type + (string.IsNullOrEmpty(exver) ? "" : ("." + exver)) + (platform == "ios" ? ".ipa" : ".apk"));
                string install = platform == "ios" ?
                    "itms-services://?action=download-manifest&amp;url=" + ConfigurationManager.AppSettings["qiniu_url_root"].ToString() + urlname + ".plist"
                    : ConfigurationManager.AppSettings["url_root"].ToString() + urlname + ".apk";
                string url = platform == "ios" ?
                    ConfigurationManager.AppSettings["url_root"].ToString() + urlname + ".ipa"
                    : ConfigurationManager.AppSettings["url_root"].ToString() + urlname + ".apk";

                return template.Replace("[PACK_NAME]", name)
                    .Replace("[PACK_INSTALL_URL]", install)
                    .Replace("[PACK_URL]", url)
                    .Replace("[PACK_URL_ENCODE]", HttpUtility.UrlEncode(install))
                    .Replace("[PLATFORM]", platform)
                    .Replace("[PACK_VER]", ver);
            }
        }

        static string Join(string[] arr, string split, int start, int len)
        {
            try
            {
                StringBuilder sb = new StringBuilder(arr[start]);
                for (int i = start + 1; i < start + len; i++)
                {
                    sb.Append(split);
                    sb.Append(arr[i]);
                }
                return sb.ToString();
            }
            catch
            {
                return "empty";
            }
        }

        static string www_root = string.Empty;
        static void Main(string[] args)
        {
            WebClient client = new WebClient();
            www_root = ConfigurationManager.AppSettings["www_root"].ToString();
            Console.WriteLine("www_root = " + www_root);

            // 资源版本号
            string resver_ios = File.ReadAllText(www_root + "default.ios/version.txt");
            Console.WriteLine("resver_ios = " + resver_ios);
            string resver_android = File.ReadAllText(www_root + "default.android/version.txt");
            Console.WriteLine("resver_android = " + resver_android);

            // 下载ipa
            string[] ipa_verline = Encoding.UTF8.GetString(client.DownloadData(ConfigurationManager.AppSettings["ipa_ver_download_url"].ToString())).Split('\n');
            if (ipa_verline.Length > 1)
            {
                string[] ipa_ver_arr = ipa_verline[0].Split('.');
                string ipa_type = Join(ipa_ver_arr, ".", 4, ipa_ver_arr.Length - 4);
                string ipa_ver = Join(ipa_ver_arr, ".", 0, 4);
                string ipa_exver = ipa_verline[1];
                string ipa_name = ipa_type + "." + ipa_ver + (string.IsNullOrEmpty(ipa_exver) ? "" : ("." + ipa_exver));
                Console.WriteLine("ipa_ver = " + ipa_ver + ", ipa_type = " + ipa_type);
                string old_ipa_ver = State.ins.GetVer("ipa." + ipa_type);
                string old_ipa_exver = State.ins.GetExtVer("ipa." + ipa_type);
                if (old_ipa_ver != ipa_ver || old_ipa_exver != ipa_exver)
                {
                    string oldfile = www_root + ipa_type + "." + old_ipa_ver + (string.IsNullOrEmpty(old_ipa_exver) ? "" : ("." + old_ipa_exver)) + ".ipa";
                    if (File.Exists(oldfile))
                    {
                        Console.WriteLine("del old ipa");
                        File.Delete(oldfile);
                    }

                    Console.WriteLine("start download ipa");
                    client.DownloadFile(ConfigurationManager.AppSettings["ipa_download_url"].ToString(), www_root + ipa_name + ".ipa");
                    Console.WriteLine("ipa download complete! size = " + (new FileInfo(www_root + ipa_name + ".ipa").Length / 1024 / 1024f).ToString("0.0") + "M");
                    State.ins.SetVer("ipa." + ipa_type, ipa_ver, ipa_exver);

                    // 上传ios安装说明
                    string plistContent = File.ReadAllText(www_root + "plist.template");
                    plistContent = plistContent.Replace("[IPA_DOWNLOAD_URL]", ConfigurationManager.AppSettings["url_root"].ToString() + ipa_name + ".ipa");
                    plistContent = plistContent.Replace("[IPA_BUNDLE_ID]", "com.supermobs.demo");
                    plistContent = plistContent.Replace("[IPA_BUNDLE_NAME]", ipa_name);
                    // 上传策略 http://developer.qiniu.com/article/developer/security/put-policy.html
                    PutPolicy putPolicy = new PutPolicy();
                    putPolicy.Scope = ConfigurationManager.AppSettings["qiniu_bucket"].ToString();
                    putPolicy.setExpires(3600);
                    putPolicy.DeleteAfterDays = 30;
                    // 生成上传凭证 http://developer.qiniu.com/article/developer/security/upload-token.html            
                    string token = UploadManager.createUploadToken(new Mac(ConfigurationManager.AppSettings["qiniu_accesskey"].ToString(), ConfigurationManager.AppSettings["qiniu_secretkey"].ToString()), putPolicy);
                    // 上传
                    SimpleUploader su = new SimpleUploader();
                    HttpResult result = su.uploadData(Encoding.UTF8.GetBytes(plistContent), ipa_name + ".plist", token);
                    Console.WriteLine("upload plist, result = " + result);
                }
                else
                {
                    Console.WriteLine("ipa do not change, skip download");
                }
            }

            // 下载apk
            string[] apk_verline = Encoding.UTF8.GetString(client.DownloadData(ConfigurationManager.AppSettings["apk_ver_download_url"].ToString())).Split('\n');
            if (apk_verline.Length > 1)
            {
                string[] apk_ver_arr = apk_verline[0].Split('.');
                string apk_type = Join(apk_ver_arr, ".", 4, apk_ver_arr.Length - 4);
                string apk_ver = Join(apk_ver_arr, ".", 0, 4);
                string apk_exver = apk_verline[1];
                string apk_name = apk_type + "." + apk_ver + (string.IsNullOrEmpty(apk_exver) ? "" : ("." + apk_exver));
                Console.WriteLine("apk_ver = " + apk_ver + ", apk_type = " + apk_type);
                string old_apk_ver = State.ins.GetVer("apk." + apk_type);
                string old_apk_exver = State.ins.GetExtVer("apk." + apk_type);
                if (old_apk_ver != apk_ver || old_apk_exver != apk_exver)
                {
                    string oldfile = www_root + apk_type + "." + old_apk_ver + (string.IsNullOrEmpty(old_apk_exver) ? "" : ("." + old_apk_exver)) + ".apk";
                    if (File.Exists(oldfile))
                    {
                        Console.WriteLine("del old apk");
                        File.Delete(oldfile);
                    }

                    Console.WriteLine("start download apk");
                    client.DownloadFile(ConfigurationManager.AppSettings["apk_download_url"].ToString(), www_root + apk_name + ".apk");
                    Console.WriteLine("apk download complete! size = " + (new FileInfo(www_root + apk_name + ".apk").Length / 1024 / 1024f).ToString("0.0") + "M");
                    State.ins.SetVer("apk." + apk_type, apk_ver, apk_exver);
                }
                else
                {
                    Console.WriteLine("apk do not change, skip download");
                }
            }

            // 输出测试服 ios 更新配置文件
            string ipa_default_ver = State.ins.GetVer(ConfigurationManager.AppSettings["default_ios_package_type"].ToString());
            string ipa_default_exver = State.ins.GetExtVer(ConfigurationManager.AppSettings["default_ios_package_type"].ToString());
            if (!string.IsNullOrEmpty(ipa_default_ver))
                File.WriteAllText(www_root + "default.ios.html", File.ReadAllText(www_root + "default.ios.template")
                        .Replace("[VER]", ipa_default_ver.Substring(0, ipa_default_ver.LastIndexOf(".")) + "." + resver_ios)
                        .Replace("[PACK_INSTALL_URL]", "itms-services://?action=download-manifest&amp;url=" + ConfigurationManager.AppSettings["qiniu_url_root"].ToString() + ConfigurationManager.AppSettings["default_ios_package_type"].ToString().Substring(4) + "." + ipa_default_ver + (string.IsNullOrEmpty(ipa_default_exver) ? "" : ("." + ipa_default_exver)) + ".plist"));
            // 输出测试服 android 更新配置文件
            string apk_default_ver = State.ins.GetVer(ConfigurationManager.AppSettings["default_android_package_type"].ToString());
            string apk_default_exver = State.ins.GetExtVer(ConfigurationManager.AppSettings["default_android_package_type"].ToString());
            if (!string.IsNullOrEmpty(apk_default_ver))
                File.WriteAllText(www_root + "default.android.html", File.ReadAllText(www_root + "default.android.template")
                        .Replace("[VER]", apk_default_ver.Substring(0, apk_default_ver.LastIndexOf(".")) + "." + resver_android)
                        .Replace("[PACK_INSTALL_URL]", ConfigurationManager.AppSettings["url_root"].ToString() + ConfigurationManager.AppSettings["default_ios_package_type"].ToString().Substring(4) + "." + apk_default_ver + (string.IsNullOrEmpty(apk_default_exver) ? "" : ("." + apk_default_exver)) + ".apk"));

            // 输出网页文件
            string url_root = ConfigurationManager.AppSettings["url_root"].ToString();
            string ipaurl = "itms-services://?action=download-manifest&amp;url=" + ConfigurationManager.AppSettings["qiniu_url_root"].ToString() + "[IPA_NAME].ipa.plist";
            string[] webtemplate = File.ReadAllLines(www_root + "index.template");
            StringBuilder web = new StringBuilder();
            // 测试服开头
            web.AppendLine(webtemplate[0]);
            // 测试服ios
            web.AppendLine(State.FillPackInfo(webtemplate[1], ConfigurationManager.AppSettings["default_ios_package_type"].ToString()).Replace("[RES_VER]", resver_ios));
            // 测试服android
            web.AppendLine(State.FillPackInfo(webtemplate[1], ConfigurationManager.AppSettings["default_android_package_type"].ToString()).Replace("[RES_VER]", resver_android));
            // 测试服结尾
            web.AppendLine(webtemplate[2]);
            // 所有包列表开头
            web.AppendLine(webtemplate[3]);
            // 所有包列表
            foreach (string type in State.ins.packageTypes)
                web.AppendLine(State.FillPackInfo(webtemplate[4], type));
            // 所有包结尾
            web.AppendLine(webtemplate[5]);
            // 写入web
            File.WriteAllText(www_root + "index.html", web.ToString());

            // 结束
            State.Save();
            Console.WriteLine("publish complete!");
        }
    }
}
