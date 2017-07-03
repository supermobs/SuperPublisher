﻿using System;
using System.IO;
using System.Text;
using Qiniu.Common;
using Qiniu.Http;

namespace Qiniu.IO
{
    /// <summary>
    /// 空间文件下载，只提供简单下载逻辑
    /// 对于大文件下载、断点续下载等需求，可以根据实际情况自行实现
    /// </summary>
    public class DownloadManager
    {
        private Signature signature;
        private HttpManager httpManager;

        /// <summary>
        /// 下载管理
        /// </summary>
        /// <param name="mac">账户访问控制(密钥)</param>
        public DownloadManager(Mac mac)
        {
            signature = new Signature(mac);
            httpManager = new HttpManager();
        }

        /// <summary>
        /// 生成下载凭证
        /// </summary>
        /// <param name="url">资源URL</param>
        /// <returns>下载凭证</returns>
        public string createDownloadToken(string url)
        {
            return signature.sign(url);
        }

        /// <summary>
        /// 生成授权的下载链接(访问私有空间中的文件时需要使用)
        /// </summary>
        /// <param name="url">初始链接</param>
        /// <param name="expireInSeconds">下载有效时间(单位:秒)</param>
        /// <returns>包含过期时间的已授权的下载链接</returns>
        public string createSignedUrl(string url, int expireInSeconds = 3600)
        {
            string deadline = Util.StringHelper.calcUnixTimestamp(expireInSeconds);

            StringBuilder sb = new StringBuilder(url);
            if (url.Contains("?"))
            {
                sb.AppendFormat("&e={0}", deadline);
            }
            else
            {
                sb.AppendFormat("?e={0}", deadline);
            }
            string token = createDownloadToken(sb.ToString());
            sb.AppendFormat("&token={0}", token);

            return sb.ToString();
        }

        /// <summary>
        /// 下载(私有文件,附带token授权的链接)文件到本地
        /// </summary>
        /// <param name="signedUrl">(可访问的)链接</param>
        /// <param name="saveasFile">(另存为)本地文件名</param>
        /// <returns>下载资源(私有文件)的结果</returns>
        public HttpResult downloadPriv(string signedUrl,string saveasFile)
        {
            HttpResult result = new HttpResult();
             
            try
            {
                result = httpManager.get(signedUrl, null, true);
                if (result.Code == HttpHelper.STATUS_CODE_OK)
                {
                    using (FileStream fs = File.Create(saveasFile, result.Data.Length))
                    {
                        fs.Write(result.Data, 0, result.Data.Length);
                        fs.Flush();
                    }
                    result.RefText += string.Format("[Download] Success: (Remote file) ==> \"{0}\" @{1}\n",
                        saveasFile, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                }
                else
                {
                    result.RefText += string.Format("[Download] Error: @{0}\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                }
            }
            catch(Exception ex)
            {
                StringBuilder sb = new StringBuilder("Download Error: ");
                Exception e = ex;
                while (e != null)
                {
                    sb.Append(e.Message + " ");
                    e = e.InnerException;
                }

                sb.AppendFormat(" @{0}\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));

                result.RefCode = HttpHelper.STATUS_CODE_EXCEPTION;
                result.RefText += sb.ToString();
            }

            return result;
        }

        /// <summary>
        /// 下载文件(通用)
        /// </summary>
        /// <param name="url">公开可访问的资源下载URL</param>
        /// <param name="saveasFile">(另存为)本地文件名</param>
        /// <returns>下载资源(公开文件)的结果</returns>
        public static HttpResult download(string url,string saveasFile)
        {
            HttpResult result = new HttpResult();

            try
            {
                HttpManager httpManager = new HttpManager();

                result = httpManager.get(url, null, true);
                if (result.Code == HttpHelper.STATUS_CODE_OK)
                {
                    using (FileStream fs = File.Create(saveasFile, result.Data.Length))
                    {
                        fs.Write(result.Data, 0, result.Data.Length);
                        fs.Flush();
                    }
                    result.RefText += string.Format("[Download] Success: (Remote file) ==> \"{0}\" @{1}\n", 
                        saveasFile, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                }
                else
                {
                    result.RefText += string.Format("[Download] Error: @{0}\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Download Error: ");
                Exception e = ex;
                while (e != null)
                {
                    sb.Append(e.Message + " ");
                    e = e.InnerException;
                }

                sb.AppendFormat(" @{0}\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));

                result.RefCode = HttpHelper.STATUS_CODE_EXCEPTION;
                result.RefText += sb.ToString();
            }

            return result;
        }

    }
}
