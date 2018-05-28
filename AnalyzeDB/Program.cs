using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzeDB
{
    class Program
    {
        static void Main(string[] args)
        {
            // 同一内容の食材名をまとめておく。
            List<List<string>> duplicationFoodStuffList = (new DuplicationManager()).DuplicationFoodStuffList;
        }
    }
}
