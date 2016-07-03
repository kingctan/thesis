using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    /// <summary>
    /// Interaction logic for VideoPageSettings.xaml
    /// </summary>
    public partial class VideoPageSettings : UserControl
    {
        private const double MAX_CONFIDENCE_TRESHOLD = 0.3;
        private const double MIN_CONFIDENCE_TRESHOLD = 0.1;
        private MainWindow parentWindow;
        private VideoPage videoPage;

        public VideoPageSettings(VideoPage _videoPage)
        {
            InitializeComponent();
            this.Loaded += VideoPageSettings_Loaded;
            this.videoPage = _videoPage;
            this.Unloaded += VideoPageSettings_Unloaded;
        }

        private void VideoPageSettings_Loaded(object sender, RoutedEventArgs e)
        {
            parentWindow = (MainWindow)Window.GetWindow((DependencyObject)sender);
            parentWindow.RemoveGoBackMethod();
            parentWindow.backButton.Click += GoBack;
            if (parentWindow.GestureLibIsPaused)
            {
                handGesturesOff.IsChecked = true;
            }
            else
            {
                handGesturesOn.IsChecked = true;
            }

            if (videoPage.VideoControl.videoPlayer.IsMuted)
            {
                muteOn.IsChecked = true;
            }
            else
            {
                muteOff.IsChecked = true;
            }

            if (parentWindow.HandTrackLibDetectLeftHand && parentWindow.HandTrackLibDetectRightHand)
            {
                handTrackBoth.IsChecked = true;
            }
            else if (parentWindow.HandTrackLibDetectLeftHand)
            {
                handTrackLeft.IsChecked = true;
            }
            else if (parentWindow.HandTrackLibDetectRightHand)
            {
                handTrackRight.IsChecked = true;
            }
        }

        private void VideoPageSettings_Unloaded(object sender, RoutedEventArgs e)
        {
            parentWindow.AddGoBackMethod();
            parentWindow.backButton.Click -= GoBack;

            if (handGesturesOn.IsChecked.HasValue && handGesturesOn.IsChecked.Value)
            {
                parentWindow.GestureLibIsPaused = false;
            }
            else if (handGesturesOff.IsChecked.HasValue && handGesturesOff.IsChecked.Value)
            {
                parentWindow.GestureLibIsPaused = true;
            }

            if (muteOn.IsChecked.HasValue && muteOn.IsChecked.Value)
            {
                videoPage.VideoControl.videoPlayer.IsMuted = true;
                parentWindow.CurrentConfidenceThreshold = MIN_CONFIDENCE_TRESHOLD;
            }
            else if (muteOff.IsChecked.HasValue && muteOff.IsChecked.Value)
            {
                videoPage.VideoControl.videoPlayer.IsMuted = false;
                parentWindow.CurrentConfidenceThreshold = MAX_CONFIDENCE_TRESHOLD;
            }

            if (handTrackBoth.IsChecked.HasValue && handTrackBoth.IsChecked.Value)
            {
                parentWindow.HandTrackLibDetectLeftHand = true;
                parentWindow.HandTrackLibDetectRightHand = true;
            }
            else if (handTrackLeft.IsChecked.HasValue && handTrackLeft.IsChecked.Value)
            {
                parentWindow.HandTrackLibDetectLeftHand = true;
                parentWindow.HandTrackLibDetectRightHand = false;
            }
            else if (handTrackRight.IsChecked.HasValue && handTrackRight.IsChecked.Value)
            {
                parentWindow.HandTrackLibDetectRightHand = true;
                parentWindow.HandTrackLibDetectLeftHand = false;
            }
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            parentWindow.navigationRegion.Content = videoPage;
        }
    }
}