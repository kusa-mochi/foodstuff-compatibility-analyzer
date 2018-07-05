using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnalyzeDB
{
    // TODO: 特定の食材を中心にした図を作図するモードを追加する。
    class Program
    {
        private static string _jouken = "where 回数 > 8";
        private static int _numResult = 40;

        private static string _dbFilePath = "data.db";
        private static List<List<string>> _duplicationFoodStuffList = null;
        private static Random _rand = new Random(DateTime.Now.Millisecond);

        static void Main(string[] args)
        {
            // 重複する（同一内容の）食材名をまとめておく。
            _duplicationFoodStuffList = (new DuplicationManager()).DuplicationFoodStuffList;

            // 重複する食材名を，DB内で同じ統一する。
            UnifyNames();
        }

        private static void UnifyNames()
        {
            // CSVファイルに結果を出力するときに使うバッファ。
            string outputBuffer = "@startuml food_stuff_graph.svg\n";

            using (var conn = new SQLiteConnection("Data Source=" + _dbFilePath))
            {
                conn.Open();
                using (SQLiteCommand command = conn.CreateCommand())
                {
                    // 食材テーブルの中の重複項目の食材名を統一する。
                    for (int i = 0; i < _duplicationFoodStuffList.Count; i++)
                    {
                        if (_duplicationFoodStuffList[i].Count < 2) continue;

                        command.CommandText = "update 食材 ";
                        command.CommandText += "set 食材名='" + _duplicationFoodStuffList[i][0] + "' ";
                        command.CommandText += "where ";

                        // j=1～Count番目の名前をj=0番目の名前に変換する。
                        for (int j = 1; j < _duplicationFoodStuffList[i].Count; j++)
                        {
                            command.CommandText += "食材名='" + _duplicationFoodStuffList[i][j] + "'";
                            if (j < _duplicationFoodStuffList[i].Count - 1)
                            {
                                command.CommandText += " or ";
                            }
                        }

                        command.ExecuteNonQuery();
                    }

                    for (int i = 0; i < _duplicationFoodStuffList.Count; i++)
                    {
                        if (_duplicationFoodStuffList[i].Count < 2) continue;

                        // 食材テーブルの中の重複する食材のIDを取得する。
                        command.CommandText = "select id from 食材 where 食材名='" + _duplicationFoodStuffList[i][0] + "'";
                        var reader = command.ExecuteReader();
                        List<long> ids = new List<long>();
                        while (reader.Read())
                        {
                            ids.Add((long)reader.GetValue(0));
                        }
                        reader.Close();

                        // 食材相性表テーブルの中の重複する食材のIDを統一する。
                        command.CommandText = "update 食材相性表 set 食材ID1=" + ids[0].ToString() + " ";
                        command.CommandText += "where ";

                        for (int j = 1; j < ids.Count; j++)
                        {
                            command.CommandText += "食材ID1=" + ids[j].ToString();
                            if (j < ids.Count - 1)
                            {
                                command.CommandText += " or ";
                            }
                        }

                        command.ExecuteNonQuery();

                        command.CommandText = "update 食材相性表 set 食材ID2=" + ids[0].ToString() + " ";
                        command.CommandText += "where ";

                        for (int j = 1; j < ids.Count; j++)
                        {
                            command.CommandText += "食材ID2=" + ids[j].ToString();
                            if (j < ids.Count - 1)
                            {
                                command.CommandText += " or ";
                            }
                        }

                        command.ExecuteNonQuery();
                    }

                    // 食材テーブルの情報を配列に取得する。
                    List<string> foodStuffList = new List<string>();
                    foodStuffList.Add("");  // インデックスを1から始めるために，0番目要素を挿入しておく。
                    command.CommandText = "select * from 食材";
                    var foodStuffReader = command.ExecuteReader();
                    for (int i = 1; foodStuffReader.Read(); i++)
                    {
                        string foodStuffName = (string)foodStuffReader.GetValue(1);
                        if (string.IsNullOrEmpty(foodStuffName))
                        {
                            foodStuffName = "***NoName***";
                        }

                        // 食材名をリストに追加する。
                        foodStuffList.Add(foodStuffName);
                    }
                    foodStuffReader.Close();

                    // 食材相性表テーブルの重複する組合せの回数データをそれぞれ合計し，新たなテーブルとして追加する。
                    command.CommandText = "create table 食材相性集計表 as select id,食材ID1,食材ID2,sum(回数) as 回数 from 食材相性表 group by 食材ID1,食材ID2 order by 回数 desc";
                    command.ExecuteNonQuery();

                    long resultCount = 0L;
                    command.CommandText = "select count(*) from 食材相性集計表";
                    var resultCountReader = command.ExecuteReader();
                    while (resultCountReader.Read())
                    {
                        resultCount = (long)resultCountReader.GetValue(0);
                    }
                    resultCountReader.Close();

                    //// 出現回数を絞って食材の組合せデータを取得する。
                    command.CommandText = "select * from 食材相性集計表" + (string.IsNullOrEmpty(_jouken) ? "" : " " + _jouken);
                    var resultReader = command.ExecuteReader();

                    List<FoodStuffCountRecord> resultList = new List<FoodStuffCountRecord>();

                    while (resultReader.Read())
                    {
                        long count = (long)resultReader.GetValue(3);

                        int foodStuffId1 = (int)resultReader.GetValue(1);
                        int foodStuffId2 = (int)resultReader.GetValue(2);
                        resultList.Add(
                            new FoodStuffCountRecord()
                            {
                                FoodStuff1 = foodStuffId1,
                                FoodStuff2 = foodStuffId2,
                                Count = count
                            }
                            );
                    }
                    resultReader.Close();

                    List<int> foodStuffIds = new List<int>();
                    for (int i = 0; i < _numResult && resultList.Count > 0; i++)
                    {
                        int randIdx = _rand.Next(0, resultList.Count - 1);
                        int id1 = resultList[randIdx].FoodStuff1;
                        int id2 = resultList[randIdx].FoodStuff2;

                        if (foodStuffIds.FindIndex(n => n == id1) < 0)
                        {
                            outputBuffer += "agent \"" + foodStuffList[id1] + "\" as agent" + id1.ToString() + "\n";
                            foodStuffIds.Add(id1);
                        }
                        if (foodStuffIds.FindIndex(n => n == id2) < 0)
                        {
                            outputBuffer += "agent \"" + foodStuffList[id2] + "\" as agent" + id2.ToString() + "\n";
                            foodStuffIds.Add(id2);
                        }

                        long count = resultList[randIdx].Count;

                        outputBuffer += "agent" + id1.ToString() + (count > 10 ? " -- " : " .. ") + "agent" + id2.ToString() + " : " + count.ToString() + "\n";

                        resultList.RemoveAt(randIdx);
                    }
                }
                conn.Close();
            }
            outputBuffer += "@enduml\n";

            // PlantUML形式で結果を出力する。
            using (StreamWriter s = new StreamWriter("result.puml"))
            {
                s.Write(outputBuffer);
            }

            Console.WriteLine("完了！");
            Console.ReadKey();
        }
    }
}
