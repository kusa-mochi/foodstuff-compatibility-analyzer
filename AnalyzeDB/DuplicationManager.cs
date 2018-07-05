using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzeDB
{
    public class DuplicationManager
    {
        /// <summary>
        /// 食材の重複リスト
        /// </summary>
        public List<List<string>> DuplicationFoodStuffList = new List<List<string>> {
            new List<string> {"胡麻油", "ごま油", "ゴマ油", "ごまあぶら"},
            new List<string> {"酒", "さけ", "お酒", "おさけ", "日本酒", "清酒"},
            new List<string> {"砂糖", "さとう"},
            new List<string> {"塩", "しお"},
            new List<string> {"醤油", "しょうゆ", "しょう油"},
            new List<string> {"葱", "ねぎ", "ネギ", "長葱", "長ねぎ", "長ネギ", "青葱", "青ねぎ", "青ネギ"},
            new List<string> {"玉葱", "玉ねぎ", "タマネギ", "たまねぎ"},
            new List<string> {"鶏胸肉", "鶏むね肉", "とりむねにく"},
            new List<string> {"鶏腿肉", "鶏もも肉", "とりももにく"},
            new List<string> {"人参", "にんじん", "ニンジン"},
            new List<string> {"味噌", "みそ", "ミソ"},
            new List<string> {"味醂", "みりん", "ミリン"},
            new List<string> {"牛乳", "ミルク", "MILK"},
            new List<string> {"生姜", "しょうが", "ショウガ"},
            new List<string> {"にんにく", "ニンニク"}
        };

        public DuplicationManager()
        {
            for (int iDuplicationFoodStuff = 0; iDuplicationFoodStuff < DuplicationFoodStuffList.Count; iDuplicationFoodStuff++)
            {
                List<string> currentStuffList = new List<string>();
                for (int j = 0; j < DuplicationFoodStuffList[iDuplicationFoodStuff].Count; j++)
                {
                    // 元々の重複リストを保持する。
                    currentStuffList.Add(DuplicationFoodStuffList[iDuplicationFoodStuff][j]);
                }

                for (int iCurrentStuff = 0; iCurrentStuff < currentStuffList.Count; iCurrentStuff++)
                {
                    for (int iPrepend = 0; iPrepend < _prependList.Length; iPrepend++)
                    {
                        // 接頭辞を付加した文字列を重複リストに追加する。
                        DuplicationFoodStuffList[iDuplicationFoodStuff].Add(_prependList[iPrepend] + currentStuffList[iCurrentStuff]);
                    }
                }
            }
        }

        //////////////////////// 以下，private ////////////////////////////////////////////////

        /// <summary>
        /// 接頭辞：各食材の名前の先頭に付く可能性がある文字列
        /// </summary>
        private string[] _prependList = new string[]
        {
            "◎",
            "●",
            "〇",
            "◆",
            "◇",
            "★",
            "☆",
            "＊",
            "　",
            "A",
            "B",
            " "
        };
    }
}
