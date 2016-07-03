using System.Windows;

namespace ControlsMedia.DialogControl
{
    /// <summary>
    /// Interaction logic for RatingsDialog.xaml
    /// </summary>
    public partial class RatingsDialog : Window
    {
        public RatingsDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Only when clicked on yes change DialogResult to true
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}