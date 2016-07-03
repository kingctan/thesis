namespace Microsoft.Samples.Kinect.ControlsBasics
{
    using ArgsLibrary;
    using ControlsMedia.DialogControl;
    using Csv;
    using HandtrackLibraryUtilities;
    using Microsoft.Kinect;
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using TextToSpeechLibrary;
    using Utils;

    /// <summary>
    /// Interaction logic for VideoPage
    /// </summary>
    public partial class VideoPage : UserControl
    {
        #region Private Properties

        private MainWindow parentWindow;
        private KinectSensor kinectSensor;
        private CountDownControl CountDown;
        private TextBlock TopTextBlock;
        private CsvWriter actionCsvWriter;
        private bool WRITE_TO_CSV;
        private bool manuallyStop;
        private int previousRating;
        private bool isSetPrevious;
        private double heightDragCanvas;
        private double widthDragCanvas;

        #endregion Private Properties

        #region ctor

        public VideoPage(KinectSensor _sensor, int rating, Uri content, string title)
        {
            if (_sensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }
            this.InitializeComponent();
            this.DataContext = this;
            VideoControl = new VideoControl();
            VideoControl.Source = content;
            ChangeContentToVideo();
            kinectSensor = _sensor;
            CountDown = new CountDownControl(_sensor);
            CountDown.Width = 800;
            CountDown.HorizontalAlignment = HorizontalAlignment.Center;
            Rating = rating;
            Title = title;
            this.Loaded += UserControl_Loaded;
            this.Unloaded += UserControl_Unloaded;
            //string day = DateTime.Now.ToString("ddMMyy_hh_mm_ss");
            string day = "";
            actionCsvWriter = new CsvWriter("actionvideopage" + day + ".csv", new string[] { "action" });
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            parentWindow = (MainWindow)Window.GetWindow((DependencyObject)sender);
            parentWindow.saveButton.Click += GoBackAndSaveEstimation;

            parentWindow.PopupLabel.Content = "See the text in green? You can say it. "
                + Environment.NewLine + "1st|2nd|3rd|4th|5th star"
                + Environment.NewLine + "louder"
                + Environment.NewLine + "dim";
            CountDown.DataComplete += CountDownDataComplete;
            CountDown.DataComplete += parentWindow.VideoPageDataComplete;
            WRITE_TO_CSV = parentWindow.WRITE_TO_CSV;
            heightDragCanvas = dragCanvas.ActualHeight;
            widthDragCanvas = dragCanvas.ActualWidth;
            AddTextBlockToDragDropCanvas();
            TextToSpeechLib.Speak("Watching " + Title);
            ListenToCommands = true;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            CountDown.DataComplete -= CountDownDataComplete;
            CountDown.DataComplete -= parentWindow.VideoPageDataComplete;
            parentWindow.saveButton.Click -= GoBackAndSaveEstimation;
            parentWindow.PopupLabel.Content = "See the text in green? You can say it. " + Environment.NewLine
                             + "Scroll home|up|down|top|bottom" + Environment.NewLine
                             + "Scroll left|right|begin|end";
            CountDown.Dispose();
            if (WRITE_TO_CSV)
                actionCsvWriter.WriteToFile();
        }

        /// <summary>
        /// Adding text block to the drag and drop canvas
        /// </summary>
        private void AddTextBlockToDragDropCanvas()
        {
            //Top TextBlock needed for calculatiion drag and drop canvas
            TopTextBlock = AddText(0, 0, "0", (Color)ColorConverter.ConvertFromString("Purple"));
            AddText(0, heightDragCanvas / 6, "1", (Color)ColorConverter.ConvertFromString("Red"));
            AddText(0, 2 * heightDragCanvas / 6, "2", (Color)ColorConverter.ConvertFromString("Blue"));
            AddText(0, 3 * heightDragCanvas / 6, "3", (Color)ColorConverter.ConvertFromString("Orange"));
            AddText(0, 4 * heightDragCanvas / 6, "4", (Color)ColorConverter.ConvertFromString("Yellow"));
            AddText(0, 5 * heightDragCanvas / 6, "5", (Color)ColorConverter.ConvertFromString("Green"));
        }

        private TextBlock AddText(double x, double y, string text, Color color)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Height = heightDragCanvas / 6;
            textBlock.Width = widthDragCanvas;
            textBlock.Background = new SolidColorBrush(color);
            textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.FontSize = 80;
            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            dragCanvas.Children.Add(textBlock);
            return textBlock;
        }

        public bool ListenToCommands
        {
            get;
            set;
        }

        #endregion ctor

        #region Using VideoControl

        public void ShowListeningUi()
        {
            VideoControl.ShowListeningUi();
        }

        public void HideListeningUi()
        {
            VideoControl.HideListeningUi();
        }

        public void Play()
        {
            VideoControl.Play();
            if (WRITE_TO_CSV)
                actionCsvWriter.WriteRow("play");
        }

        public void Pause()
        {
            VideoControl.Pause();
            if (WRITE_TO_CSV)
                actionCsvWriter.WriteRow("pause");
        }

        public void VolumeUp()
        {
            VideoControl.VolumeUp();
            if (WRITE_TO_CSV)
                actionCsvWriter.WriteRow("Increase sound");
        }

        public void VolumeDown()
        {
            VideoControl.VolumeDown();
            if (WRITE_TO_CSV)
                actionCsvWriter.WriteRow("Decrease sound");
        }

        public int Rating
        {
            get
            {
                return ratingControl.RatingValue;
            }

            set
            {
                ratingControl.RatingValue = value;
            }
        }

        public string Title { get; private set; }

        public VideoControl VideoControl
        {
            get;
            set;
        }

        public void FastForward()
        {
            VideoControl.FastForward();
            if (WRITE_TO_CSV)
                actionCsvWriter.WriteRow("fast forward");
        }

        public void Rewind()
        {
            VideoControl.Rewind();
            if (WRITE_TO_CSV)
                actionCsvWriter.WriteRow("rewind");
        }

        public void Stop()
        {
            VideoControl.Stop();
            if (WRITE_TO_CSV)
                actionCsvWriter.WriteRow("stop");
        }

        #endregion Using VideoControl

        #region Private methods

        private void GoBack(object sender, RoutedEventArgs e)
        {
            manuallyStop = true;
            isSetPrevious = true;
            CountDown.Stop();
        }

        private void GoBackAndSaveEstimation(object sender, RoutedEventArgs e)
        {
            manuallyStop = true;
            CountDown.Stop();
        }

        private void ChangeContentToVideo()
        {
            contentControl.Content = VideoControl;
        }

        private void ChangeContentToCountDown()
        {
            contentControl.Content = CountDown;
        }

        private void HideOptionsExceptSave()
        {
            parentWindow.facialRecognitionButton.Visibility = Visibility.Hidden;
            parentWindow.emotionRecognitionButton.Visibility = Visibility.Hidden;
            parentWindow.helpButton.Visibility = Visibility.Hidden;
            parentWindow.saveButton.Visibility = Visibility.Visible;
        }

        private void ShowOptionsExceptSave()
        {
            parentWindow.facialRecognitionButton.Visibility = Visibility.Visible;
            parentWindow.emotionRecognitionButton.Visibility = Visibility.Visible;
            parentWindow.helpButton.Visibility = Visibility.Visible;
            parentWindow.saveButton.Visibility = Visibility.Collapsed;
        }

        private void StartCountdown(MethodTypes method)
        {
            Canvas.SetLeft(dragDropElement, 0);
            Canvas.SetTop(dragDropElement, 0);
            parentWindow.PreviousRating = Rating;
            ratingControl.Enable(false);
            CountDown.Start(method);
            ChangeContentToCountDown();
            HideOptionsExceptSave();
            parentWindow.RemoveGoBackMethod();
            parentWindow.backButton.Click += GoBack;
            ListenToCommands = false;
            if (method.Equals(MethodTypes.DragAndDrop))
            {
                dragCanvas.Visibility = Visibility.Visible;
                ControlPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                ButtonsPanel.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Set previous rating only for drag and drop because calculation is done here
        /// </summary>
        private void setPreviousRating()
        {
            Rating = previousRating;
        }

        private void SetPrevious(DataCompleteEventArgs e)
        {
            //Stopped with backbutton
            if (isSetPrevious)
            {
                if (!e.Method.Equals(MethodTypes.DragAndDrop))
                {
                    parentWindow.setPreviousRating();
                }
                else
                {
                    setPreviousRating();
                }
                isSetPrevious = false;
            }
        }

        private void StopCountDownDataComplete(DataCompleteEventArgs e)
        {
            ratingControl.Enable(true);
            ChangeContentToVideo();
            ShowOptionsExceptSave();
            parentWindow.AddGoBackMethod();
            parentWindow.backButton.Click -= GoBack;
            ListenToCommands = true;
            if (e.Method.Equals(MethodTypes.DragAndDrop))
            {
                dragCanvas.Visibility = Visibility.Collapsed;
                ControlPanel.Visibility = Visibility.Visible;
                Rating = CalculateDragAndDropRating();
            }
            else
            {
                ButtonsPanel.Visibility = Visibility.Visible;
            }

            //Stopped with backbutton or with save button
            if (manuallyStop)
            {
                SetPrevious(e);
                manuallyStop = false;
            }
            else
            {//Timer has elapsed not manually stopped
                TextToSpeechLib.Speak("Do you want to save this rating?");

                parentWindow.Hide();
                RatingsDialog dialog = new RatingsDialog();
                dialog.ShowDialog();

                if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
                {
                }
                else
                {
                    isSetPrevious = true;
                    SetPrevious(e);
                }
                parentWindow.Show();
            }
        }

        private void CountDownDataComplete(object sender, DataCompleteEventArgs e)
        {
            if (e.Title.Equals(Recordings.Stop))
            {
                StopCountDownDataComplete(e);
            }
            else if (e.Title.Equals(Recordings.Update) && e.Method.Equals(MethodTypes.DragAndDrop))
            {
                Rating = CalculateDragAndDropRating();
                TextToSpeechLib.Speak("Score estimation " + Rating);
            }
        }

        private int CalculateDragAndDropRating()
        {
            double midRectY = dragDropElement.Y + rectangle.ActualHeight / 2;
            double topCanvY = Canvas.GetTop(TopTextBlock);
            if (midRectY < topCanvY + 1 * heightDragCanvas / 6)
            {
                return 0;
            }
            else if (midRectY < topCanvY + 2 * heightDragCanvas / 6)
            {
                return 1;
            }
            else if (midRectY < topCanvY + 3 * heightDragCanvas / 6)
            {
                return 2;
            }
            else if (midRectY < topCanvY + 4 * heightDragCanvas / 6)
            {
                return 3;
            }
            else if (midRectY < topCanvY + 5 * heightDragCanvas / 6)
            {
                return 4;
            }
            else if (midRectY < topCanvY + heightDragCanvas)
            {
                return 5;
            }

            return 0;
        }

        private void HandUp(object sender, RoutedEventArgs e)
        {
            StartCountdown(MethodTypes.HandUp);
        }

        private void FingerTopTracking(object sender, RoutedEventArgs e)
        {
            StartCountdown(MethodTypes.FingerTopTracking);
        }

        private void DragAndDrop(object sender, RoutedEventArgs e)
        {
            previousRating = Rating;
            StartCountdown(MethodTypes.DragAndDrop);
        }

        /// <summary>
        /// Called when the handtracklib from the main menu track a hand
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void HandsController_HandsDetected(HandCollection e)
        {
            if (e.HandLeft != null)
            {
                foreach (var finger in e.HandLeft.Fingers)
                {
                    // Finger tip in the 2D depth space.
                    var depthPoint = finger.DepthPoint;
                    CountDown.DrawEllipse(depthPoint, Brushes.Red, 5.0);
                }

                foreach (var point in e.HandLeft.Range)
                {
                    CountDown.DrawEllipse(point, Brushes.Blue, 10.0);
                }
                CountDown.DrawEllipse(e.HandLeft.HandPointDepth, Brushes.Blue, e.HandLeft.Radius);
            }

            if (e.HandRight != null)
            {
                foreach (var finger in e.HandRight.Fingers)
                {
                    // Finger tip in the 2D depth space.
                    var depthPoint = finger.DepthPoint;
                    CountDown.DrawEllipse(depthPoint, Brushes.Red, 5.0);
                }

                foreach (var point in e.HandRight.Range)
                {
                    CountDown.DrawEllipse(point, Brushes.Blue, 10.0);
                }
                CountDown.DrawEllipse(e.HandRight.HandPointDepth, Brushes.Blue, e.HandRight.Radius);
            }
        }

        private void videoSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            parentWindow.navigationRegion.Content = Activator.CreateInstance(typeof(VideoPageSettings), this);
            actionCsvWriter.WriteRow(new String[] { "videosettings" });
        }

        #endregion Private methods
    }
}