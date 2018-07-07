using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebScrapingKit
{
    public class HtmlDataGetter
    {
        public bool IsCollecting { get; private set; } = false;

        // 連続アクセスするときの時間間隔[msec]
        private int _sleepInterval = 2000;

        // 連続アクセスでデータ取得に失敗したときの時間間隔[msec]
        private int _sleepIntervalWhenError = 200;

        public string[] GetDataFromHtml(string url, string xpath)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url");
            if (string.IsNullOrEmpty(xpath)) throw new ArgumentNullException("xpath");

            var doc = new HtmlDocument();
            var web = new WebClient();
            web.Encoding = Encoding.UTF8;
            try
            {
                doc.LoadHtml(web.DownloadString(url));
            }
            catch (Exception ex)
            {
                throw new Exception("GetHtmlメソッドで，HTML取得時にエラーが発生しました。", ex);
            }

            var targetCollection = doc.DocumentNode.SelectNodes(xpath);
            if (targetCollection == null)
            {
                throw new MissingXPathException("GetHtmlメソッドで，指定したxpathに該当するHTML要素が見つかりませんでした。");
            }

            string[] output = GetInnerTexts(targetCollection);
            return output;
        }

        public Task GetDataFromHtmlAsync(string url, string xpath, int idFrom, int idTo, string urlFooter, IProgress<HtmlData> progress, CancellationToken cancelToken)
        {
            return Task.Run(() =>
            {
                GetDataFromHtml(url, xpath, idFrom, idTo, urlFooter, progress, cancelToken);
            });
        }

        public Task GetDataFromHtmlAsync(string[] urls, string xpath, IProgress<HtmlData> progress, CancellationToken cancelToken)
        {
            return Task.Run(() =>
            {
                GetDataFromHtml(urls, xpath, progress, cancelToken);
            });
        }

        public HtmlDataGetter(int sleepInterval = 2000, int sleepIntervalWhenError = 200)
        {
            _sleepInterval = sleepInterval;
            _sleepIntervalWhenError = sleepIntervalWhenError;
        }

        private void GetDataFromHtml(string url, string xpath, int idFrom, int idTo, string urlFooter, IProgress<HtmlData> progress, CancellationToken cancelToken)
        {
            IsCollecting = true;
            var doc = new HtmlDocument();
            var web = new WebClient();
            web.Encoding = Encoding.UTF8;
            for (int id = idFrom; id <= idTo && IsCollecting; id++)
            {
                string wholeUrl = url + id.ToString() + urlFooter;
                try
                {
                    doc.LoadHtml(web.DownloadString(wholeUrl));
                }
                catch
                {
                    // 次のIDを指定して再度データを取得しに行く。
                    // データが取得できなかった旨を呼び出し元に通知する。
                    progress.Report(new HtmlData(
                        null,
                        0,
                        null
                        ));
                    Cancellation(cancelToken);
                    Thread.Sleep(_sleepIntervalWhenError);
                    continue;
                }

                //指定したXPathをもとに文を取得する。
                var targetCollection = doc.DocumentNode.SelectNodes(xpath);

                // データが得られなかった場合
                if (targetCollection == null)
                {
                    // 次のIDを指定して再度データを取得しに行く。
                    // データが取得できなかった旨を呼び出し元に通知する。
                    progress.Report(new HtmlData(
                        null,
                        0,
                        null
                        ));
                    Cancellation(cancelToken);
                    Thread.Sleep(_sleepIntervalWhenError);
                    continue;
                }

                string[] data = new string[targetCollection.Count];

                try
                {
                    data = GetInnerTexts(targetCollection);
                }
                catch
                {
                    // データの取得に失敗した場合
                    // データが取得できなかった旨を呼び出し元に通知する。
                    progress.Report(new HtmlData(
                        null,
                        0,
                        null
                        ));
                    Cancellation(cancelToken);
                    Thread.Sleep(_sleepIntervalWhenError);
                    continue;
                }

                // 画面のデータグリッドに情報を表示する。
                progress.Report(new HtmlData(
                    wholeUrl,
                    id,
                    data));

                Cancellation(cancelToken);
                Thread.Sleep(_sleepInterval);
            }
        }

        private void GetDataFromHtml(string[] urls, string xpath, IProgress<HtmlData> progress, CancellationToken cancelToken)
        {
            IsCollecting = true;
            var doc = new HtmlDocument();
            var web = new WebClient();
            web.Encoding = Encoding.UTF8;
            foreach (string url in urls)
            {
                if (!IsCollecting) break;

                try
                {
                    doc.LoadHtml(web.DownloadString(url));
                }
                catch
                {
                    // 次のIDを指定して再度データを取得しに行く。
                    // データが取得できなかった旨を呼び出し元に通知する。
                    progress.Report(new HtmlData(
                        null,
                        0,
                        null
                        ));
                    Cancellation(cancelToken);
                    Thread.Sleep(_sleepIntervalWhenError);
                    continue;
                }

                //指定したXPathをもとに文を取得する。
                var targetCollection = doc.DocumentNode.SelectNodes(xpath);

                // データが得られなかった場合
                if (targetCollection == null)
                {
                    // 次のIDを指定して再度データを取得しに行く。
                    // データが取得できなかった旨を呼び出し元に通知する。
                    progress.Report(new HtmlData(
                        null,
                        0,
                        null
                        ));
                    Cancellation(cancelToken);
                    Thread.Sleep(_sleepIntervalWhenError);
                    continue;
                }

                string[] data = new string[targetCollection.Count];

                try
                {
                    data = GetInnerTexts(targetCollection);
                }
                catch
                {
                    // データの取得に失敗した場合
                    // データが取得できなかった旨を呼び出し元に通知する。
                    progress.Report(new HtmlData(
                        null,
                        0,
                        null
                        ));
                    Cancellation(cancelToken);
                    Thread.Sleep(_sleepIntervalWhenError);
                    continue;
                }

                // 画面のデータグリッドに情報を表示する。
                progress.Report(new HtmlData(
                    url,
                    -1,
                    data));

                Cancellation(cancelToken);
                Thread.Sleep(_sleepInterval);
            }
        }

        private void Cancellation(CancellationToken cancelToken)
        {
            try
            {
                // キャンセル要求があれば例外を発生させタスクを終了させる.
                cancelToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                IsCollecting = false;
            }
        }

        private string[] GetInnerTexts(HtmlNodeCollection c)
        {
            string[] output = new string[c.Count];
            for (int i = 0; i < c.Count; i++)
            {
                output[i] = c[i].InnerText.Replace("\r", "").Replace("\n", "");
            }

            return output;
        }
    }
}
