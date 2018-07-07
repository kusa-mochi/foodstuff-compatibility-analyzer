using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScrapingKit
{
    public class HtmlData
    {
        public string Url { get; set; }
        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (value >= 0)
                {
                    _id = value;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }
        public string[] Data { get; set; }

        public HtmlData(string url, int id, string[] data)
        {
            Url = url;
            _id = id;
            data.CopyTo(Data, 0);
        }

        private int _id = -1;
    }
}
