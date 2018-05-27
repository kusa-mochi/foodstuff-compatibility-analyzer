using System;

namespace MainApplication.Models
{
    public struct RecipeRecord
    {
        public int Id { get; set; }                 // レシピID
        public string[] FoodStuffList { get; set; } // 食材リスト
        public string Url { get; set; }             // 参照URL

        public RecipeRecord(
            int id,
            string[] foodStuffList,
            string url
            ) : this()
        {
            this.Id = id;
            this.FoodStuffList = foodStuffList;
            this.Url = url;
        }
    }
}
