using System;
using System.Windows;
using System.Windows.Controls;

namespace MasaSam.Controls
{
    /// <summary>
    /// Interaction logic for RatingControl.xaml
    /// </summary>
    public partial class RatingControl : UserControl
    {
        /// <summary>
        /// Maximum amout of stars
        /// </summary>
        private int _maxValue = 5;

        /// <summary>
        /// The rating of the control
        /// </summary>
        public static readonly DependencyProperty RatingValueProperty =
            DependencyProperty.Register("RatingValue", typeof(int), typeof(RatingControl),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(RatingValueChanged)));

        /// <summary>
        /// Fills all checkboxes till the new rating value and unfill all the rest
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void RatingValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            RatingControl parent = sender as RatingControl;
            int ratingValue = (int)e.NewValue;
            UIElementCollection children = ((Grid)(parent.Content)).Children;
            StarButton button = null;
            //button[0] is star with ratingvalue 1
            for (int i = 0; i < ratingValue; i++)
            {
                button = children[i] as StarButton;
                if (button != null)
                    button.IsFilled = true;
            }

            for (int i = ratingValue; i < children.Count; i++)
            {
                button = children[i] as StarButton;
                if (button != null)
                    button.IsFilled = false;
            }
        }

        /// <summary>
        /// (En)able or dissable all the stars
        /// </summary>
        /// <param name="enable"></param>
        public void Enable(bool enable)
        {
            StarButton button = null;
            UIElementCollection children = ((Grid)(this.Content)).Children;
            for (int i = 0; i < children.Count; i++)
            {
                button = children[i] as StarButton;
                if (button != null)
                    button.IsEnabled = enable;
            }
        }

        public int RatingValue
        {
            get { return (int)GetValue(RatingValueProperty); }
            set
            {
                if (value < 0)
                {
                    SetValue(RatingValueProperty, 0);
                }
                else if (value > _maxValue)
                {
                    SetValue(RatingValueProperty, _maxValue);
                }
                else
                {
                    SetValue(RatingValueProperty, value);
                }
            }
        }

        public RatingControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Only needed to xave the rating value, check property is not linked with coloring the stars
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Checked(Object sender, RoutedEventArgs e)
        {
            StarButton button = sender as StarButton;
            int newRating = int.Parse((String)button.Tag);
            RatingValue = newRating;
            e.Handled = true;
        }
    }
}