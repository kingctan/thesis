using Microsoft.Kinect.Input;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TextToSpeechLibrary;

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    /// <summary>
    /// Interaction logic for VideoControl.xaml
    /// </summary>
    public partial class VideoControl : UserControl, INotifyPropertyChanged
    {
        #region Private properties

        /// <summary>
        /// Value extra volume up or down
        /// </summary>
        private const double DELTA_VOLUME = 0.05;

        /// <summary>
        /// Timer that decides the position in the video for the slider
        /// </summary>
        private DispatcherTimer videoTimeTimer;

        /// <summary>
        /// Timer that decides the fast forward or rewind speed
        /// </summary>
        private DispatcherTimer speedTimer;

        /// <summary>
        /// Timer that decides if the video controls are visible or not
        /// </summary>
        private DispatcherTimer showVideControlsTimer;

        /// <summary>
        /// The total length of the video
        /// </summary>
        private TimeSpan totalVideoTimeSpan;

        private Object playOrPauseLock = new object();
        private string playButtonContent;
        private bool isPlaying;
        private bool flagRewind;
        private bool flagFastForward;

        #endregion Private properties

        #region Ctor

        public VideoControl()
        {
            InitializeComponent();
            this.DataContext = this;

            // Create a timer that will update the time slider
            videoTimeTimer = new DispatcherTimer();
            videoTimeTimer.Interval = TimeSpan.FromSeconds(1);
            videoTimeTimer.Tick += new EventHandler(timer_Tick);
            videoTimeTimer.Start();

            slider.PreviewMouseUp += new MouseButtonEventHandler(timeSlider_MouseLeftButtonUp);
            slider.PreviewMouseDown += new MouseButtonEventHandler(timeSlider_MouseLeftButtonDown);

            speedTimer = new DispatcherTimer();
            speedTimer.Interval = TimeSpan.FromMilliseconds(100.0);
            speedTimer.Tick += new EventHandler(speedTimer_Tick);

            showVideControlsTimer = new DispatcherTimer();
            showVideControlsTimer.Tick += new EventHandler(showVideControlsTimer_Tick);
            showVideControlsTimer.Interval = new TimeSpan(0, 0, 10);
        }

        private void media_MediaOpened(object sender, System.Windows.RoutedEventArgs e)
        {
            if (videoPlayer.NaturalDuration.HasTimeSpan)
            {
                //total length of video o-available when media opened
                totalVideoTimeSpan = videoPlayer.NaturalDuration.TimeSpan;
            }
        }

        private void videoPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            //None of the video content is rendered before the play method is called.So even if you set the position past the first frame you wont get any data rendered.
            videoPlayer.Play();
            videoPlayer.Pause();
            PlayButtonContent = "Play";
            isPlaying = false;
            videoPlayerElementManipulation.OnManipulationUpdated += OnManipulationUpdated;
            videoPlayerElementPressing.OnTapped += OnTapped;
        }

        #endregion Ctor

        #region Private method

        private void timer_Tick(object sender, EventArgs e)
        {
            // Check if the movie is finished calculate it's totalVideoTimeSpan time
            if (totalVideoTimeSpan.TotalSeconds > 0)
            {
                // Updating time slider
                slider.Value = videoPlayer.Position.TotalSeconds / totalVideoTimeSpan.TotalSeconds;
            }
        }

        private bool atEnd()
        {
            return videoPlayer.Position.TotalSeconds == totalVideoTimeSpan.TotalSeconds;
        }

        private bool atBegin()
        {
            return videoPlayer.Position.TotalSeconds == 0;
        }

        public void Stop()
        {
            if (!atBegin())
            {
                //Normal tempo when tempo was fast forwarding of rewinding
                if (flagFastForward || flagRewind)
                {
                    speedTimer.Stop();
                    flagRewind = false;
                    flagFastForward = false;
                }
                isPlaying = false;
                PlayButtonContent = "Play";
                videoPlayer.Stop();
                TextToSpeechLib.Speak("End video");
            }
        }

        /// <summary>
        /// Changes the position of the video to the position of the slider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timeSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (totalVideoTimeSpan.TotalSeconds > 0)
            {
                videoPlayer.Position = TimeSpan.FromSeconds(slider.Value * totalVideoTimeSpan.TotalSeconds);
                videoPlayer.Play();
                videoTimeTimer.Start();
            }
        }

        private void timeSlider_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            videoPlayer.Pause();
            videoTimeTimer.Stop();
            StopSpeedTimer();
            if (speedTimer.IsEnabled)
                speedTimer.IsEnabled = false;
        }

        /// <summary>
        /// When manipulation/scrolling the video change the position of the video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnManipulationUpdated(object sender, KinectManipulationUpdatedEventArgs e)
        {
            videoPlayer.Position += TimeSpan.FromSeconds(e.Cumulative.Translation.X);
        }

        /// <summary>
        /// When pressing the video play or pause the video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTapped(object sender, KinectTappedEventArgs e)
        {
            PlayOrPause();
        }

        private void videoPlayer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PlayOrPause();
        }

        /// <summary>
        /// Change the position every timer tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void speedTimer_Tick(object sender, EventArgs e)
        {
            if (flagRewind)
            {
                videoPlayer.Position -= TimeSpan.FromMilliseconds(50.0);
            }
            else if (flagFastForward)
            {
                videoPlayer.Position += TimeSpan.FromMilliseconds(50.0);
            }
        }

        private void videoPlayer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                VolumeUp();
            }
            else
            {
                VolumeDown();
            }
        }

        #endregion Private method

        /// <summary>
        /// Shows the UI that is speech and click enabled
        /// </summary>
        public void ShowListeningUi()
        {
            videoControls.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Hides the UI that is speech and click enabled
        /// </summary>
        public void HideListeningUi()
        {
            videoControls.Visibility = System.Windows.Visibility.Hidden;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // usual OnPropertyChanged implementation
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public string PlayButtonContent
        {
            get
            {
                return playButtonContent;
            }
            set
            {
                if (value != playButtonContent)
                {
                    playButtonContent = value;
                    OnPropertyChanged("PlayButtonContent");
                }
            }
        }

        public Uri Source
        {
            get
            {
                return videoPlayer.Source;
            }
            set
            {
                videoPlayer.Source = value;
            }
        }

        public double Volume
        {
            get
            {
                return videoPlayer.Volume;
            }
            set
            {
                videoPlayer.Volume = value;
            }
        }

        private void StopSpeedTimer()
        {
            if (flagFastForward || flagRewind)
            {
                speedTimer.Stop();
                flagRewind = false;
                flagFastForward = false;
            }
        }

        public void Pause()
        {
            if (isPlaying)
            {
                StopSpeedTimer();
                videoPlayer.Pause();
                isPlaying = false;
                PlayButtonContent = "Play";
                TextToSpeechLib.Speak("Interrupt video");
            }
        }

        public void Play()
        {
            if (!isPlaying)
            {
                StopSpeedTimer();
                videoPlayer.Play();
                isPlaying = true;
                PlayButtonContent = "Pause";
                TextToSpeechLib.Speak("Watch video");
            }
        }

        public void PlayOrPause()
        {
            //Normal tempo when tempo was fast forwarding of rewinding
            lock (playOrPauseLock)
            {
                if (!isPlaying)
                {
                    Play();
                }
                else
                {
                    Pause();
                }
            }
        }

        public void VolumeUp()
        {
            if (videoPlayer.Volume < 1)
            {
                videoPlayer.Volume += DELTA_VOLUME;
                TextToSpeechLib.Speak("Volume up");
            }
        }

        public void VolumeDown()
        {
            if (videoPlayer.Volume > 0)
            {
                videoPlayer.Volume -= DELTA_VOLUME;
                TextToSpeechLib.Speak("restrain volume");
            }
        }

        /// <summary>
        /// Fast forward only when before at normal tempo
        /// </summary>
        public void FastForward()
        {
            if (!flagFastForward && !flagRewind && !atEnd())
            {
                Pause();
                flagFastForward = true;
                speedTimer.Start();
                TextToSpeechLib.Speak("Advance video");
            }
        }

        /// <summary>
        /// Rewind only when before at normal tempo
        /// </summary>
        public void Rewind()
        {
            if (!flagFastForward && !flagRewind && !atBegin())
            {
                Pause();
                flagRewind = true;
                speedTimer.Start();
                TextToSpeechLib.Speak("Reverse video");
            }
        }

        #region Controlbuttons events

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            PlayOrPause();
        }

        private void FastForwardButton_Click(object sender, RoutedEventArgs e)
        {
            FastForward();
        }

        private void RewindButton_Click(object sender, RoutedEventArgs e)
        {
            Rewind();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void videoPlayer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (videoControls.Visibility == Visibility.Hidden)
            {
                showVideControlsTimer.Start();
                videoControls.Visibility = Visibility.Visible;
            }
        }

        private void showVideControlsTimer_Tick(object sender, EventArgs e)
        {
            videoControls.Visibility = Visibility.Hidden;
            showVideControlsTimer.IsEnabled = false;
        }

        #endregion Controlbuttons events
    }
}