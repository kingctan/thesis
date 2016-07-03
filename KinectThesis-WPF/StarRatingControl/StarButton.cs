using System.Windows;
using System.Windows.Controls;

namespace MasaSam.Controls
{ /// <summary>
/// Star with property that decides if the star is filled.
/// </summary>
    public class StarButton : CheckBox
    {
        public static readonly DependencyProperty IsFilledProperty = DependencyProperty.Register("IsFilled", typeof(bool), typeof(StarButton), new FrameworkPropertyMetadata(false));

        public bool IsFilled
        {
            get { return (bool)GetValue(IsFilledProperty); }
            set { SetValue(IsFilledProperty, value); }
        }
    }
}