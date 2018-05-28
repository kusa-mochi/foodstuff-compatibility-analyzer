using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzeDB
{
    public class DuplicationManager
    {
        public List<List<string>> DuplicationFoodStuffList = new List<List<string>> {
            new List<string> {"醤油", "しょうゆ", "しょう油"},
            new List<string> {"味噌", "みそ", "ミソ"},
            new List<string> {"味醂", "みりん", "ミリン"},
            new List<string> {"砂糖", "さとう"},
            new List<string> {"塩", "しお"},
            new List<string> {"玉葱", "玉ねぎ", "タマネギ", "たまねぎ"},
            new List<string> {"胡麻油", "ごま油", "ゴマ油", "ごまあぶら"},
            new List<string> {"", "", ""},
            new List<string> {"", "", ""},
            new List<string> {"", "", ""},
            new List<string> {"", "", ""},
            new List<string> {"", "", ""},
            new List<string> {"", "", ""},
            new List<string> {"", "", ""},
            new List<string> {"", "", ""},
            new List<string> {"", "", ""},
            new List<string> {"", "", ""},
            new List<string> {"", "", ""},
            new List<string> {"", "", ""},
            new List<string> {"", "", ""}
        };

        private string[] _prependList = new string[]
        {
            "●",
            "〇",
            "◆",
            "◇",
            "★",
            "☆"
        };

        private string[] _appendList = new string[] {
        };

        public DuplicationManager()
        {

        }
    }
}
