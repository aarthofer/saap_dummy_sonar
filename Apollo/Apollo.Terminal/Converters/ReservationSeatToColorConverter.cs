using Apollo.BLInterface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace Apollo.Terminal.Converters
{
    public class ReservationSeatToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value?.GetType() != typeof(ReservationSeat)) { return "";  }
            ReservationSeat seat = (ReservationSeat)value;
            if (seat.state == ReservationSeat.State.corridor) { return Brushes.Transparent; }
            return seat.state == ReservationSeat.State.free ? Brushes.Green : Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
