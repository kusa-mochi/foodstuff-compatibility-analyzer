using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using HtmlAgilityPack;

using Livet;

namespace MainApplication.Models
{
    public class DataCollector : NotificationObject
    {
        /*
         * NotificationObjectはプロパティ変更通知の仕組みを実装したオブジェクトです。
         */

        void CollectRecord(int id, IProgress<RecipeRecord> progress, CancellationToken cancelToken)
        {
            while (true)
            {
                //docがHtmlのコードを文書として保持します
                var doc = new HtmlDocument();

                //webを通してHtmlのコードを取得します
                var web = new System.Net.WebClient();

                //HtmlをUTF-8で取得します。
                //特に指定しない場合、Htmlの文字コードによっては文字化けする場合があります
                web.Encoding = Encoding.UTF8;

                //web.DownloadString(URL)のURL先のHtmlを取得します。
                try
                {
                    doc.LoadHtml(web.DownloadString(@"https://cookpad.com/recipe/" + id.ToString()));
                }
                catch
                {
                    // 次のIDを指定して再度データを取得しに行く。
                    progress.Report(new RecipeRecord(
                        0,  // 「データが得られなかった」という情報を元スレッドに投げる。
                        null,
                        ""));
                    Cancellation(cancelToken);
                    id++;
                    continue;
                }

                //取得したHtmlコードからどの部分を取得するのかをXPath形式で指定します
                string targetPath = "//div[@class='ingredient_name']/span";

                //指定したXPathをもとに文を取得しています
                var targetCollection = doc.DocumentNode.SelectNodes(targetPath);

                // データが得られなかった場合
                if (targetCollection == null)
                {
                    // 次のIDを指定して再度データを取得しに行く。
                    progress.Report(new RecipeRecord(
                        0,  // 「データが得られなかった」という情報を元スレッドに投げる。
                        null,
                        ""));
                    Cancellation(cancelToken);
                    id++;
                    continue;
                }

                string[] foodStuffList = new string[targetCollection.Count];

                try
                {
                    foodStuffList = GetFoodStuffList(targetCollection);
                }
                catch
                {
                    // データの取得に失敗した場合

                    // 次のIDを指定して再度データを取得しに行く。
                    progress.Report(new RecipeRecord(
                        -(id++),    // 「データが読み込めない形式だった」という情報を元スレッドに投げる。
                        null,
                        ""));
                    Cancellation(cancelToken);
                    continue;
                }

                // 画面のデータグリッドに情報を表示する。
                progress.Report(new RecipeRecord(
                    id++,
                    foodStuffList,
                    @"https://cookpad.com/recipe/" + id.ToString()));

                Cancellation(cancelToken);
            }
        }

        public Task CollectRecordAsync(int id, IProgress<RecipeRecord> progress, CancellationToken cancelToken)
        {
            return Task.Run(() =>
            {
                CollectRecord(id, progress, cancelToken);
            });
        }

        public void Cancellation(CancellationToken cancelToken)
        {
            try
            {
                // キャンセル要求があれば例外を発生させタスクを終了させる.
                cancelToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                throw new OperationCanceledException(cancelToken);
            }
        }

        private string[] GetFoodStuffList(HtmlNodeCollection c)
        {
            string[] output = new string[c.Count];
            for (int iStuff = 0; iStuff < c.Count; iStuff++)
            {
                output[iStuff] = c[iStuff].InnerText;
            }

            return output;
        }
    }
}
