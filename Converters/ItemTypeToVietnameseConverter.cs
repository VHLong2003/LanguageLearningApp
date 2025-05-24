using Microsoft.Maui.Controls;
using LanguageLearningApp.Models;

namespace LanguageLearningApp.Converters
{
    public class ItemTypeToVietnameseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ItemType itemType)
            {
                return itemType switch
                {
                    ItemType.Avatar => "Avatar",
                    ItemType.Theme => "Giao diện",
                    ItemType.PowerUp => "Vật phẩm hỗ trợ",
                    ItemType.Customization => "Tùy chỉnh",
                    ItemType.Decoration => "Trang trí",
                    ItemType.Special => "Đặc biệt",
                    _ => "Không xác định"
                };
            }
            return "Không xác định";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
