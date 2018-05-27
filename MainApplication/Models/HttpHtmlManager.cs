using System.Net;

namespace MainApplication.Models
{
    public class HttpHtmlManager
    {
        private WebClient _client = null;

        public HttpHtmlManager()
        {
            _client = new WebClient();
        }

        public string GetHtml(string url)
        {
            string output = "";

            try
            {
                output = _client.DownloadString(url);
            }
            catch (WebException exc)
            {
                output = exc.Message;
            }

            return output;
        }
    }
}
