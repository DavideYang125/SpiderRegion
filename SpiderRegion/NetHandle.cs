using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace SpiderRegion
{
    public class NetHandle
    {

        /// <summary>
        /// get content
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameter"></param>
        /// <param name="referer"></param>
        /// <returns></returns>
        public static Tuple<HttpStatusCode, string> GetHtmlContent(string url, string parameter = "", string referer = "")
        {
            Tuple<HttpStatusCode, string> htmlResult = new Tuple<HttpStatusCode, string>(HttpStatusCode.Gone, string.Empty);
            string content = string.Empty;
            try
            {
                System.Threading.Thread.Sleep(100);
                var clientHandler = new HttpClientHandler();
                if (clientHandler.SupportsAutomaticDecompression)
                {
                    clientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }
                using (var httpClient = new HttpClient(clientHandler))
                {
                    var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                    httpClient.Timeout = TimeSpan.FromSeconds(25);
                    requestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.89 Safari/537.36");

                    //requestMessage.Headers.Add("If-None-Match", "1c98-580baa54b4840-gzip");
                    //requestMessage.Headers.Add("host", "www.stats.gov.cn");
                    //requestMessage.Headers.Add("Cookie", "AD_RS_COOKIE=20082855; _trs_uv=jxbeuhbs_6_b8l");

                    if (!string.IsNullOrEmpty(referer)) requestMessage.Headers.Add("Referer", referer);
                    var response = httpClient.SendAsync(requestMessage).Result;
                    content = response.Content.ReadAsStringAsync().Result;
                    // System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    using (var sr = new StreamReader(response.Content.ReadAsStreamAsync().Result, Encoding.GetEncoding("GB2312")))
                    {
                        content = sr.ReadToEnd();
                    }

                    //content = Utf8ToGB2312(content);
                    htmlResult = new Tuple<HttpStatusCode, string>(response.StatusCode, content);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogs("url-" + url, "exception_log");
                Console.WriteLine(ex.ToString());
            }
            return htmlResult;
        }
        public static string Utf8ToGB2312(string utf8String)
        {
            Encoding fromEncoding = Encoding.UTF8;
            Encoding toEncoding = Encoding.GetEncoding("gb2312");
            return EncodingConvert(utf8String, fromEncoding, toEncoding);
        }
        public static string EncodingConvert(string fromString, Encoding fromEncoding, Encoding toEncoding)
        {
            byte[] fromBytes = fromEncoding.GetBytes(fromString);
            byte[] toBytes = Encoding.Convert(fromEncoding, toEncoding, fromBytes);

            string toString = toEncoding.GetString(toBytes);
            return toString;
        }

        /// <summary>
        /// download file
        /// </summary>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool DownFileMethod(string url, string path)
        {
            var myWebClient = new WebClientMy();
            myWebClient.Timeout = 1000 * 20;
            bool isSucess = false;
            if (!string.IsNullOrEmpty(url) && url.Trim().StartsWith("//"))
            {
                url = "http:" + url;
            }
            try
            {
                myWebClient.Headers.Set("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                myWebClient.DownloadFile(url.Trim(), path);
                File.SetAttributes(path, FileAttributes.Normal);
                isSucess = true;
            }
            catch (Exception) { }
            return isSucess;
        }
    }

    public class WebClientMy : WebClient
    {
        public int Timeout { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            request.Timeout = Timeout;
            return request;
        }
    }
}
