using System;
using System.Globalization;
using LanguageLearningApp.Models;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Converters
{
    public class QuestionTypeToVietnameseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is QuestionType type)
            {
                return type switch
                {
                    QuestionType.MultipleChoice => "Trắc nghiệm",
                    QuestionType.TrueFalse => "Đúng/Sai",
                    QuestionType.FillInTheBlank => "Điền chỗ trống",
                    QuestionType.Matching => "Ghép đôi",
                    QuestionType.ShortAnswer => "Trả lời ngắn",
                    QuestionType.VoiceRecording => "Ghi âm",
                    QuestionType.Arrangement => "Sắp xếp",
                    _ => "Khác"
                };
            }
            return "Khác";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
