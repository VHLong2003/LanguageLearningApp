using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageLearningApp.Models
{
    public class ShopItem
    {
        public string ItemId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public int Price { get; set; }
        public ItemType Type { get; set; }
    }

    public enum ItemType
    {
        Avatar,
        Theme,
        SpecialItem,
        PowerUp
    }

}
