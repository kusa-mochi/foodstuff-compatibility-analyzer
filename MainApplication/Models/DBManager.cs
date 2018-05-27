using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainApplication.Models
{
    public class DBManager
    {
        private string _filePath = null;

        public DBManager(string filePath = @"data.db")
        {
            if (string.IsNullOrEmpty(filePath) == true)
            {
                throw new ArgumentNullException("dbFileName");
            }

            _filePath = filePath;

            // DBファイルが新規作成された場合
            if (!File.Exists(filePath))
            {
                // 空のテーブルを作成する。
                this.CreateNewTables();
            }
        }

        ~DBManager()
        {
        }

        public void AddRecord(RecipeRecord r)
        {
            using (var conn = new SQLiteConnection("Data Source=" + _filePath))
            {
                conn.Open();
                using (SQLiteCommand command = conn.CreateCommand())
                {
                    foreach (string foodStuff in r.FoodStuffList)
                    {

                        command.CommandText = "select count(*) from 食材 where 食材名='" + foodStuff + "'";
                        // 新たな食材が含まれている場合
                        if (Convert.ToInt32(command.ExecuteScalar()) == 0)
                        {
                            // 食材テーブルに追加する。
                            command.CommandText = "insert into 食材 (食材名) values ('" + foodStuff + "')";
                            command.ExecuteNonQuery();
                        }
                    }

                    foreach (string foodStuff1 in r.FoodStuffList)
                    {
                        foreach (string foodStuff2 in r.FoodStuffList)
                        {
                            // 同じ食材同士の場合はスキップ
                            if (foodStuff1 == foodStuff2) continue;

                            // 食材IDを取得する
                            long id1 = 0L, id2 = 0L;
                            SQLiteDataReader dataReader = null;
                            command.CommandText = "select * from 食材 where 食材名='" + foodStuff1 + "'";
                            dataReader = command.ExecuteReader();
                            while (dataReader.Read())
                            {
                                id1 = (long)dataReader.GetValue(0);
                            }
                            dataReader.Close();

                            command.CommandText = "select * from 食材 where 食材名='" + foodStuff2 + "'";
                            dataReader = command.ExecuteReader();
                            while (dataReader.Read())
                            {
                                id2 = (long)dataReader.GetValue(0);
                            }
                            dataReader.Close();

                            // id1 > id2 の場合にカウントするのでスキップ
                            if (id1 <= id2) continue;

                            command.CommandText = "select count(*) from 食材相性表 where 食材ID1=" + id1 + " and 食材ID2=" + id2;
                            // 新たな食材の組合せの場合
                            if (Convert.ToInt32(command.ExecuteScalar()) == 0)
                            {
                                // 食材相性表テーブルに追加する。
                                command.CommandText = "insert into 食材相性表 (食材ID1,食材ID2) values (" + id1 + "," + id2 + ")";
                                command.ExecuteNonQuery();
                            }

                            // 食材の組合せ出現回数をインクリメントする
                            command.CommandText = "update 食材相性表 ";
                            command.CommandText += "set 回数=回数+1 ";
                            command.CommandText += "where 食材ID1=" + id1 + " and 食材ID2=" + id2;
                            command.ExecuteNonQuery();
                        }
                    }

                    command.CommandText = "update 収集情報 ";
                    command.CommandText += "set 最後に収集したID=" + r.Id + " ";
                    command.CommandText += "where id=1";
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        public void AddErrorRecord(int page)
        {
            using (var conn = new SQLiteConnection("Data Source=" + _filePath))
            {
                conn.Open();
                using (SQLiteCommand command = conn.CreateCommand())
                {
                    command.CommandText = "insert into 取得エラー履歴 ";
                    command.CommandText += "(page) values ";
                    command.CommandText += "(";
                    command.CommandText += page.ToString();
                    command.CommandText += ")";
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        private void CreateNewTables()
        {
            using (var conn = new SQLiteConnection("Data Source=" + _filePath))
            {
                conn.Open();
                using (SQLiteCommand command = conn.CreateCommand())
                {
                    command.CommandText = "create table 食材 (";
                    command.CommandText += "id integer not null primary key autoincrement";
                    command.CommandText += ",";
                    command.CommandText += "食材名 text";
                    command.CommandText += ")";
                    command.ExecuteNonQuery();

                    command.CommandText = "create table 食材相性表 (";
                    command.CommandText += "id integer not null primary key autoincrement";
                    command.CommandText += ",";
                    command.CommandText += "食材ID1 integer not null";
                    command.CommandText += ",";
                    command.CommandText += "食材ID2 integer not null";
                    command.CommandText += ",";
                    command.CommandText += "回数 integer default 0";
                    command.CommandText += ")";
                    command.ExecuteNonQuery();

                    command.CommandText = "create table 収集情報 (";
                    command.CommandText += "id integer not null primary key autoincrement";
                    command.CommandText += ",";
                    command.CommandText += "最後に収集したID integer default 0";
                    command.CommandText += ")";
                    command.ExecuteNonQuery();

                    command.CommandText = "insert into 収集情報 (最後に収集したID) values (0)";
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }
    }
}
