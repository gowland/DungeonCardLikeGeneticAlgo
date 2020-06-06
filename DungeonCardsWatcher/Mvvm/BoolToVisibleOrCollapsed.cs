using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace DungeonCardsWatcher.Mvvm
{
    public class BoolToVisibleOrCollapsed : MarkupExtension, IValueConverter
    {
        public bool Invert { get; set; }

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool bValue = (bool)value;
            if (bValue)
                return this.Invert ? Visibility.Hidden : Visibility.Visible;
            else
                return this.Invert ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return (object) this;
        }
    }
}