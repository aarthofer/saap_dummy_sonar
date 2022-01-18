using Apollo.BLInterface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Apollo.Terminal.Converters
{
    class SeatStateToBGImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(ReservationSeat.State)) { return ""; }

            ReservationSeat.State state = (ReservationSeat.State)value;
            var image = new Image();
            switch (state)
            {
                case ReservationSeat.State.free:
                    return new ImageSourceConverter().ConvertFromString("images\\cinemaSeatFree.png");

                case ReservationSeat.State.reserved:
                    return new ImageSourceConverter().ConvertFromString("images\\cinemaSeatTaken.png");

                case ReservationSeat.State.selected:
                    return new ImageSourceConverter().ConvertFromString("images\\cinemaSeatSelected.png");

                default:
                    return new ImageSourceConverter().ConvertFromString("images\\corridor.png");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
