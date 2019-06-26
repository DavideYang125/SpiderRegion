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
                System.Threading.Thread.Sleep(10);
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
                    if (!string.IsNullOrEmpty(referer)) requestMessage.Headers.Add("Referer", referer);
                    var response = httpClient.SendAsync(requestMessage).Result;
                    content = response.Content.ReadAsStringAsync().Result;
                    using (var sr = new StreamReader(response.Content.ReadAsStreamAsync().Result, Encoding.GetEncoding("GB2312")))
                    {
                        content = sr.ReadToEnd();
                    }

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

    }
}
