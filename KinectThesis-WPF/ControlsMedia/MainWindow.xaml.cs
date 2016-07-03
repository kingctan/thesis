namespace Microsoft.Samples.Kinect.ControlsBasics
{
    using ArgsLibrary;
    using Csv;
    using GestureLibrary;
    using HandTrackLibrary;
    using HandtrackLibraryUtilities;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Wpf.Controls;
    using Microsoft.Samples.Kinect.ControlsBasics.DataModel;
    using Microsoft.Speech.Recognition;
    using SpeecheLibrary;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using TextToSpeechLibrary;
    using Utils;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    ///
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
        Justification = "")]
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Private properties

        private const double MAX_SPEECH_CONFIDENCE_TRESHOLD = 0.3;
        private const int SCROLL_OFFSET = 500;
        private const int VIDEOPOSITION_OFFSET = 100;

        /// <summary> Active Kinect sensor </summary>
        private KinectSensor kinectSensor = null;

        private HandTrackLib handtracklib;
        private SpeechLib speechlib;
        private GestureLib gesturelib;

        private CsvWriter testWriter;
        private CsvWriter actionCsvWriter;

        /// <summary>
        /// Timer that shows/hides the speech commando's the user can say
        /// </summary>
        private DispatcherTimer showUITimer;

        private bool listeningUI = false;

        private List<int> EstimatedRatingGestures;

        private bool isHandUpMethod = false;
        private string username;

        /// <summary>
        /// The context programm will be started in a new process, not on the UI thread so the application won't block
        /// </summary>
        private Process ContextServiceProcess;

        private Process EmotionRecognitionProcess;
        private Process FacialRecognitionProcess;
        private bool hasSetPreviousRating;

        #endregion Private properties

        #region Public properties

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// So when the user doesn't accept the rating the previous rating can be set
        /// </summary>
        public int PreviousRating
        {
            get;
            set;
        }

        #endregion Public properties

        #region ctor

        public MainWindow()
        {
            this.InitializeComponent();
            // ConsoleManager.Show();
            this.kinectSensor = KinectSensor.GetDefault();
            this.kinectSensor.Open();
            this.DataContext = this;
            Username = "Guest";
            TextToSpeechLib.Speak("Welcome " + Username);

            showUITimer = new DispatcherTimer();
            showUITimer.Tick += new EventHandler(showUITimer_Tick);
            showUITimer.Interval = new TimeSpan(0, 0, 10);
            listeningUI = false;

            EstimatedRatingGestures = new List<int>();
            CurrentConfidenceThreshold = MAX_SPEECH_CONFIDENCE_TRESHOLD;

            WRITE_TO_CSV = true;
            Create_libraries();
            gesturelib.IsPaused = true;
            App app = ((App)Application.Current);
            app.KinectRegion = kinectRegion;
            // Use the default sensor
            this.kinectRegion.KinectSensor = this.kinectSensor;
            //// Add in display content
            this.itemsControl.ItemsSource = SampleDataSource.GetGroup("Group-1");

            backButton.Click += GoBack;
            KinectRegion.SetKinectRegion(this, kinectRegion);
            PopupLabel.Content = "See the text in green? You can say it. " + Environment.NewLine
                             + "Scroll home|up|down|top|bottom" + Environment.NewLine
                             + "Scroll left|right|begin|end";

            if (WRITE_TO_CSV)
            {
                Create_CsvWriters();
            }
            if (IS_TESTING)
            {
                testWriter = new CsvWriter(testCsvFile, 1);
                MyComboBox.Visibility = Visibility.Visible;
            }
            StartContextService();
        }

        /// <summary>
        /// Property for pausing the gesture detectors in the gesture lib
        /// </summary>
        public bool GestureLibIsPaused
        {
            get
            {
                return gesturelib.IsPaused;
            }
            set
            {
                if (gesturelib.IsPaused != value)
                {
                    gesturelib.IsPaused = value;
                }
            }
        }

        /// <summary>
        /// Starts the context service
        /// </summary>
        private void StartContextService()
        {
            ContextServiceProcess = new Process();
            ContextServiceProcess.StartInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory.Replace("ControlsMedia", "ContextService") + "ContextService.exe";
            ContextServiceProcess.StartInfo.UseShellExecute = false;
            ContextServiceProcess.StartInfo.CreateNoWindow = false;
            ContextServiceProcess.StartInfo.RedirectStandardInput = true;
            ContextServiceProcess.Start();
        }

        /// <summary>
        /// Create the libraries
        /// </summary>
        private void Create_libraries()
        {
            this.speechlib = new SpeechLib(this.kinectSensor, this.SpeechRecognized, this.SpeechRejected);
            this.gesturelib = new GestureLib(this.kinectSensor, this.UpdateGesture);
            this.handtracklib = new HandTrackLib(this.kinectSensor, this.HandsController_HandsDetected);
        }

        private void Create_CsvWriters()
        {
            //string day = DateTime.Now.ToString("ddMMyy_hh_mm_ss");
            string day = "";
            actionCsvWriter = new CsvWriter("Action" + day + ".csv", new string[] { "action" });
        }

        /// <summary>
        /// Gets or sets the name of the user
        /// </summary>
        public string Username
        {
            get
            {
                return this.username;
            }

            private set
            {
                this.username = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("Username"));
            }
        }

        #endregion ctor

        #region Public methods

        public double CurrentConfidenceThreshold
        {
            get;
            set;
        }

        public bool WRITE_TO_CSV
        {
            get;
            private set;
        }

        public bool IS_TESTING
        {
            get;
            private set;
        }

        /// <summary>
        /// Remove functionality back button
        /// </summary>
        public void RemoveGoBackMethod()
        {
            backButton.Click -= GoBack;
        }

        /// <summary>
        /// Add default functionality back button
        /// </summary>
        public void AddGoBackMethod()
        {
            backButton.Click += GoBack;
        }

        #endregion Public methods

        #region Private methods

        /// <summary>
        /// Handle a button click from the wrap panel.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)e.OriginalSource;
            SampleDataItem sampleDataItem = button.DataContext as SampleDataItem;

            if (sampleDataItem != null && sampleDataItem.NavigationPage != null)
            {
                backButton.Visibility = System.Windows.Visibility.Visible;
                generalSettingsButton.Visibility = System.Windows.Visibility.Collapsed;
                navigationRegion.Content = Activator.CreateInstance(sampleDataItem.NavigationPage, kinectSensor, sampleDataItem.Rating, sampleDataItem.Content, sampleDataItem.Title);
                //set item in region
            }
            else
            {
                var selectionDisplay = new SelectionDisplay(sampleDataItem.Title);
                this.kinectRegionGrid.Children.Add(selectionDisplay);

                // Selection dialog covers the entire interact-able area, so the current press interaction
                // should be completed. Otherwise hover capture will allow the button to be clicked again within
                // the same interaction (even whilst no longer visible).
                selectionDisplay.Focus();

                // Since the selection dialog covers the entire interact-able area, we should also complete
                // the current interaction of all other pointers.  This prevents other users interacting with elements
                // (that's possible when you're engaging with two persons)
                // that are no longer visible.
                this.kinectRegion.InputPointerManager.CompleteGestures();

                e.Handled = true;
            }
        }

        /// <summary>
        /// Starts the emotion recognition proces
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EmotionRecognitionButtonClick(object sender, RoutedEventArgs e)
        {
            if (navigationRegion.Content is VideoPage)
            {
                ((VideoPage)navigationRegion.Content).Stop();
            }
            speechlib.RemoveHandlers();
            EmotionRecognitionProcess = new Process();
            EmotionRecognitionProcess.StartInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory.Replace("ControlsMedia", "EmotionRecognition") + "EmotionRecognition.exe";
            EmotionRecognitionProcess.StartInfo.UseShellExecute = false;
            EmotionRecognitionProcess.StartInfo.RedirectStandardOutput = true;
            EmotionRecognitionProcess.StartInfo.CreateNoWindow = false;
            EmotionRecognitionProcess.Start();
            EmotionRecognitionProcess.WaitForExit();
            speechlib.AddHandlers();
        }

        /// <summary>
        /// Starts the facial recognition proces
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FacialRecognitionButtonClick(object sender, RoutedEventArgs e)
        {
            if (navigationRegion.Content is VideoPage)
            {
                ((VideoPage)navigationRegion.Content).Stop();
            }
            speechlib.RemoveHandlers();
            FacialRecognitionProcess = new Process();
            FacialRecognitionProcess.StartInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory.Replace("ControlsMedia", "FacialRecognition") + "FacialRecognition.exe";
            FacialRecognitionProcess.StartInfo.UseShellExecute = false;
            FacialRecognitionProcess.StartInfo.RedirectStandardOutput = true;
            FacialRecognitionProcess.StartInfo.CreateNoWindow = false;
            FacialRecognitionProcess.Start();
            while (!FacialRecognitionProcess.StandardOutput.EndOfStream)
            {
                Username = FacialRecognitionProcess.StandardOutput.ReadLine();
            }
            speechlib.AddHandlers();
            if (Username != null && Username.Equals("Guest"))
            {
                if (WRITE_TO_CSV)
                    actionCsvWriter.WriteRow(new string[] { "loged in as guest" });
            }
            else
            {
                if (WRITE_TO_CSV)
                    actionCsvWriter.WriteRow(new string[] { "loged in as user" });
            }

            TextToSpeechLib.Speak("Welcome " + Username);
        }

        /// <summary>
        /// Open instuctuction window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HelpButtonClick(object sender, RoutedEventArgs e)
        {
            if (navigationRegion.Content is VideoPage)
            {
                ((VideoPage)navigationRegion.Content).Stop();
            }
            speechlib.RemoveHandlers();
            (new InstructionsWindow()).ShowDialog();
            speechlib.AddHandlers();
            if (WRITE_TO_CSV)
                actionCsvWriter.WriteRow(new string[] { "HelpButton" });
        }

        /// <summary>
        /// Opens generalsettings page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GeneralSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            backButton.Visibility = System.Windows.Visibility.Visible;
            generalSettingsButton.Visibility = System.Windows.Visibility.Collapsed;
            navigationRegion.Content = Activator.CreateInstance(typeof(GeneralSettings));
        }

        /// <summary>
        /// Handle the back button click
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void GoBack(object sender, RoutedEventArgs e)
        {
            generalSettingsButton.Visibility = System.Windows.Visibility.Visible;
            backButton.Visibility = System.Windows.Visibility.Hidden;
            navigationRegion.Content = this.kinectRegionGrid;
        }

        #endregion Private methods

        #region fingertracking

        /// <summary>
        /// Add the estimated fingers to the EstimatedRatingGestures list
        /// </summary>
        /// <param name="fingers"></param>
        private void FingerTrack(int fingers)
        {
            if (navigationRegion.Content is VideoPage)
            {
                EstimatedRatingGestures.Add(fingers);
            }
        }

        /// <summary>
        /// Handler for the tracked hands from handrack library
        /// Note: Dispatcher needed because the hand tracking calculation is done on another thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandsController_HandsDetected(object sender, HandCollection e)
        {
            bool result = false;
            this.Dispatcher.Invoke(() => result = navigationRegion.Content is VideoPage);

            if (result)
            {
                //Call method that will draw the fingertips on the canvas from the video page
                this.Dispatcher.Invoke(() => ((VideoPage)navigationRegion.Content).HandsController_HandsDetected(e));
                if (e.HandLeft != null)
                {
                    if (e.HandLeft.Fingers.Count != 0)
                    {
                        this.Dispatcher.Invoke(() => FingerTrack(e.HandLeft.Fingers.Count));
                    }
                }

                if (e.HandRight != null)
                {
                    if (e.HandRight.Fingers.Count != 0)
                    {
                        this.Dispatcher.Invoke(() => FingerTrack(e.HandRight.Fingers.Count));
                    }
                }
            }
        }

        /// <summary>
        /// Property that decides if the handtrack lib detects the left hand
        /// </summary>
        public bool HandTrackLibDetectLeftHand
        {
            get
            {
                return handtracklib.DetectLeftHand;
            }
            set
            {
                if (handtracklib.DetectLeftHand != value)
                {
                    handtracklib.DetectLeftHand = value;
                }
            }
        }

        /// <summary>
        /// Property that decides if the handtrack lib detects the right hand
        /// </summary>
        public bool HandTrackLibDetectRightHand
        {
            get
            {
                return handtracklib.DetectRightHand;
            }
            set
            {
                if (handtracklib.DetectRightHand != value)
                {
                    handtracklib.DetectRightHand = value;
                }
            }
        }

        #endregion fingertracking

        /// <summary>
        /// Region with methods who tests the components (speech library, gesture library,...) and writing the results to csv.
        /// This is depricated becaus the number gestures are not used anymore.
        /// </summary>

        #region Testing for developer

        private String testCsvFile = @"test.csv";

        //Testing purpose
        private void Test(object sender, RoutedEventArgs e)
        {
            String activeGestureDatabase = Convert.ToString(((Button)sender).Tag);
            activeGestureDatabase.Replace("Nr ", "");
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory.Replace("ControlsMedia", "DatasetPlayer") + "DatasetPlayer.exe";
            startInfo.Arguments = activeGestureDatabase + " " + testCsvFile;
            ThreadStart ths = new ThreadStart(() => Process.Start(startInfo));
            Thread th = new Thread(ths);
            th.Start();
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            MyComboBox.ItemsSource = new List<string> { "Nr 1", "Nr 2", "Nr 3", "Test all" };
        }

        #endregion Testing for developer

        #region speech recognition

        private bool ListenVideoPage()
        {
            return navigationRegion.Content is VideoPage && ((VideoPage)navigationRegion.Content).ListenToCommands;
        }

        /// <summary>
        /// Handler for recognized speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // CurrentConfidenceThreshold:Speech utterance confidence below which we treat speech as if it hadn't been heard
            if (e.Result.Confidence >= CurrentConfidenceThreshold)
            {
                Console.Write("SpeechRecognized ");

                switch (e.Result.Semantics.Value.ToString())
                {
                    case "kinect":
                        switchGrammar();
                        //  Console.WriteLine("Kinect");
                        if (WRITE_TO_CSV)
                        {
                            actionCsvWriter.WriteRow(new string[] { "speech kinect" });
                        }
                        break;

                    case "Play":
                        if (ListenVideoPage())
                        {
                            ((VideoPage)navigationRegion.Content).Play();
                            // Console.WriteLine("Play");
                            if (WRITE_TO_CSV)
                            {
                                actionCsvWriter.WriteRow(new string[] { "speech play" });
                            }
                        }
                        break;

                    case "Pause":
                        if (ListenVideoPage())
                        {
                            ((VideoPage)navigationRegion.Content).Pause();
                            // Console.WriteLine("Pause");
                            if (WRITE_TO_CSV)
                            {
                                actionCsvWriter.WriteRow(new string[] { "speech pause" });
                            }
                        }
                        break;

                    case "Fast Forward":
                        if (ListenVideoPage())
                        {
                            ((VideoPage)navigationRegion.Content).FastForward();
                            //Console.WriteLine("Fast forward");
                            if (WRITE_TO_CSV)
                            {
                                actionCsvWriter.WriteRow(new string[] { "speech fastforward" });
                            }
                        }
                        break;

                    case "Rewind":
                        if (ListenVideoPage())
                        {
                            ((VideoPage)navigationRegion.Content).Rewind();
                            //Console.WriteLine("Rewind");
                            if (WRITE_TO_CSV)
                            {
                                actionCsvWriter.WriteRow(new string[] { "speech rewind" });
                            }
                        }
                        break;

                    case "Stop":
                        if (ListenVideoPage())
                        {
                            ((VideoPage)navigationRegion.Content).Stop();
                            //Console.WriteLine("Stop");
                            if (WRITE_TO_CSV)
                            {
                                actionCsvWriter.WriteRow(new string[] { "speech stop" });
                            }
                        }
                        break;

                    case "louder":
                        if (ListenVideoPage())
                        {
                            ((VideoPage)navigationRegion.Content).VolumeUp();
                            //Console.WriteLine("Volme up");
                            if (WRITE_TO_CSV)
                            {
                                actionCsvWriter.WriteRow(new string[] { "speech volume up" });
                            }
                        }
                        break;

                    case "dim":
                        if (ListenVideoPage())
                        {
                            ((VideoPage)navigationRegion.Content).VolumeDown();
                            //Console.WriteLine("Volme down");
                            if (WRITE_TO_CSV)
                            {
                                actionCsvWriter.WriteRow(new string[] { "speech volume down" });
                            }
                        }
                        break;

                    case "first star":
                        if (ListenVideoPage())
                        {
                            ((VideoPage)navigationRegion.Content).Rating = 1;
                            //  Console.WriteLine("Rate one");
                            if (WRITE_TO_CSV)
                            {
                                actionCsvWriter.WriteRow(new string[] { "speech rate one" });
                            }
                        }
                        break;

                    case "second star":
                        if (ListenVideoPage())
                        {
                            ((VideoPage)navigationRegion.Content).Rating = 2;
                            // Console.WriteLine("Rate two");
                            if (WRITE_TO_CSV)
                            {
                                actionCsvWriter.WriteRow(new string[] { "speech rate two" });
                            }
                        }

                        break;

                    case "third star":
                        if (ListenVideoPage())
                        {
                            ((VideoPage)navigationRegion.Content).Rating = 3;
                            //Console.WriteLine("Rate three");
                            if (WRITE_TO_CSV)
                            {
                                actionCsvWriter.WriteRow(new string[] { "speech rate three" });
                            }
                        }

                        break;

                    case "fourth star":
                        if (ListenVideoPage())
                        {
                            ((VideoPage)navigationRegion.Content).Rating = 4;
                            // Console.WriteLine("Rate fore");
                        }
                        if (WRITE_TO_CSV)
                        {
                            actionCsvWriter.WriteRow(new string[] { "speech rate fore" });
                        }
                        break;

                    case "fifth star":
                        if (ListenVideoPage())
                        {
                            ((VideoPage)navigationRegion.Content).Rating = 5;
                            //Console.WriteLine("Rate five");
                            if (WRITE_TO_CSV)
                            {
                                actionCsvWriter.WriteRow(new string[] { "speech rate five" });
                            }
                        }
                        break;

                    case "scroll up":
                        if (!(navigationRegion.Content is VideoPage))
                        {
                            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - SCROLL_OFFSET);
                            //Console.WriteLine("Scroll up");
                            if (WRITE_TO_CSV)
                            {
                                actionCsvWriter.WriteRow(new string[] { "speech scroll up" });
                            }
                        }
                        break;

                    case "scroll down":
                        if (!(navigationRegion.Content is VideoPage))
                        {
                            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + SCROLL_OFFSET);
                            //Console.WriteLine("Scroll down");
                            if (WRITE_TO_CSV)
                            {
                                actionCsvWriter.WriteRow(new string[] { "speech scroll down" });
                            }
                        }
                        break;

                    case "scroll top":
                        if (!(navigationRegion.Content is VideoPage))
                        {
                            scrollViewer.ScrollToTop();
                            //Console.WriteLine("Scroll top");
                            if (WRITE_TO_CSV)
                            {
                                actionCsvWriter.WriteRow(new string[] { "speech scroll top" });
                            }
                        }
                        break;

                    case "scroll bottom":
                        if (!(navigationRegion.Content is VideoPage))
                        {
                            scrollViewer.ScrollToBottom();
                            //Console.WriteLine("Scroll bottom");
                            if (WRITE_TO_CSV)
                            {
                                actionCsvWriter.WriteRow(new string[] { "speech scroll bottom" });
                            }
                        }
                        break;

                    case "scroll home":
                        if (!(navigationRegion.Content is VideoPage))
                        {
                            scrollViewer.ScrollToHome();
                            //Console.WriteLine("Scroll home");
                            if (WRITE_TO_CSV)
                            {
                                actionCsvWriter.WriteRow(new string[] { "speech scroll home" });
                            }
                        }
                        break;

                    case "scroll left":
                        if (!(navigationRegion.Content is VideoPage))
                        {
                            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - SCROLL_OFFSET);
                            //Console.WriteLine("Scroll left");
                            if (WRITE_TO_CSV)
                            {
                                actionCsvWriter.WriteRow(new string[] { "speech scroll left" });
                            }
                        }

                        break;

                    case "scroll right":
                        if (!(navigationRegion.Content is VideoPage))
                        {
                            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + SCROLL_OFFSET);
                            //Console.WriteLine("Scroll right");
                            if (WRITE_TO_CSV)
                            {
                                actionCsvWriter.WriteRow(new string[] { "speech scroll right" });
                            }
                        }

                        break;

                    case "scroll begin":
                        if (!(navigationRegion.Content is VideoPage))
                        {
                            scrollViewer.ScrollToLeftEnd();
                            if (WRITE_TO_CSV)
                            {
                                actionCsvWriter.WriteRow(new string[] { "speech scroll begin" });
                            }
                        }

                        break;

                    case "scroll end":
                        if (!(navigationRegion.Content is VideoPage))
                        {
                            scrollViewer.ScrollToRightEnd();
                            if (WRITE_TO_CSV)
                            {
                                actionCsvWriter.WriteRow(new string[] { "speech scroll end" });
                            }
                        }

                        break;

                    default:
                        Console.WriteLine("Default case");
                        break;
                }
            }
        }

        /// <summary>
        /// Handler for rejected speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            if (WRITE_TO_CSV)
                actionCsvWriter.WriteRow(new string[] { "speech rejected" });
        }

        #endregion speech recognition

        #region Gestures

        /// <summary>
        /// Handler for the gesture library
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateGesture(object sender, UpdateGestureEventArgs e)
        {
            switch (e.GestureName)
            {
                case "scroll_left":
                    if (!(navigationRegion.Content is VideoPage))
                    {
                        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - SCROLL_OFFSET);
                        if (WRITE_TO_CSV)
                        {
                            actionCsvWriter.WriteRow(new string[] { "gesture scroll l lib" });
                        }
                    }
                    else
                    {
                        ((VideoPage)navigationRegion.Content).VideoControl.videoPlayer.Position -= TimeSpan.FromMilliseconds(VIDEOPOSITION_OFFSET);
                        if (WRITE_TO_CSV)
                        {
                            actionCsvWriter.WriteRow(new string[] { "gesture scroll l vid" });
                        }
                    }

                    break;

                case "scroll_right":
                    if (!(navigationRegion.Content is VideoPage))
                    {
                        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + SCROLL_OFFSET);
                        if (WRITE_TO_CSV)
                            actionCsvWriter.WriteRow(new string[] { "gesture scroll r lib" });
                    }
                    else
                    {
                        ((VideoPage)navigationRegion.Content).VideoControl.videoPlayer.Position += TimeSpan.FromMilliseconds(VIDEOPOSITION_OFFSET);
                        if (WRITE_TO_CSV)
                            actionCsvWriter.WriteRow(new string[] { "gesture scroll r vid" });
                    }
                    break;

                case "scroll_up":
                    if (!(navigationRegion.Content is VideoPage))
                    {
                        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - SCROLL_OFFSET);
                        if (WRITE_TO_CSV)
                            actionCsvWriter.WriteRow(new string[] { "gesture scroll up lib" });
                    }

                    break;

                case "scroll_down":
                    if (!(navigationRegion.Content is VideoPage))
                    {
                        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + SCROLL_OFFSET);
                        if (WRITE_TO_CSV)
                            actionCsvWriter.WriteRow(new string[] { "gesture scroll down lib" });
                    }

                    break;

                case "HandUp":
                    if (navigationRegion.Content is VideoPage && isHandUpMethod)
                    {
                        EstimatedRatingGestures.Add(1);
                        if (WRITE_TO_CSV)
                            actionCsvWriter.WriteRow(new string[] { "handUp" });
                    }

                    break;

                default:
                    break;
            }
        }

        private void StopVideoPageDataComplete(DataCompleteEventArgs e)
        {
            if (e.Method.Equals(MethodTypes.HandUp))
            {
                GestureLibIsPaused = true;
                isHandUpMethod = false;
            }
            else if (e.Method.Equals(MethodTypes.FingerTopTracking))
            {
                handtracklib.Stop();
                ///Set the estimation as the resulting score
                if (navigationRegion.Content is VideoPage)
                {
                    if (EstimatedRatingGestures.Count != 0 && !hasSetPreviousRating)
                    {
                        ((VideoPage)navigationRegion.Content).Rating = EstimatedRatingGestures.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
                        EstimatedRatingGestures.Clear();
                    }
                }
            }

            hasSetPreviousRating = false;
        }

        private void StartVideoPageDataComplete(DataCompleteEventArgs e)
        {
            //standard set rating in the beginning of a method to zero
            ((VideoPage)navigationRegion.Content).Rating = 0;
            if (e.Method.Equals(MethodTypes.HandUp))
            {
                //Enables the desture detectors in the gesture library
                GestureLibIsPaused = false;
                isHandUpMethod = true;
                EstimatedRatingGestures.Clear();
                if (WRITE_TO_CSV)
                    actionCsvWriter.WriteRow(new string[] { "HandUp" });
            }
            else if (e.Method.Equals(MethodTypes.FingerTopTracking))
            {
                handtracklib.Start();
                EstimatedRatingGestures.Clear();
                if (WRITE_TO_CSV)
                    actionCsvWriter.WriteRow(new string[] { "HandTracking" });
            }
            else if (e.Method.Equals(MethodTypes.DragAndDrop))
            {
                //Just log the action
                if (WRITE_TO_CSV)
                    actionCsvWriter.WriteRow(new string[] { "DragAndDrop" });
            }
        }

        /// <summary>
        /// Update the rating and change it to the estimation
        /// </summary>
        /// <param name="e"></param>
        private void UpdateVideoPageDataComplete(DataCompleteEventArgs e)
        {
            int currentRating = 0;
            if (EstimatedRatingGestures.Count != 0)
            {
                currentRating = EstimatedRatingGestures.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
                ((VideoPage)navigationRegion.Content).Rating = currentRating;
            }

            TextToSpeechLib.Speak("Score estimation " + currentRating);
        }

        /// <summary>
        /// Only when the hand is up for a few seconds add one to the rating
        /// </summary>
        /// <param name="e"></param>
        private void TimerTickVideoPageDataComplete(DataCompleteEventArgs e)
        {
            int currentRating = 0;

            if (EstimatedRatingGestures.Count >= 50)
            {
                ((VideoPage)navigationRegion.Content).Rating = ((VideoPage)navigationRegion.Content).Rating + 1;
            }

            currentRating = ((VideoPage)navigationRegion.Content).Rating;

            EstimatedRatingGestures.Clear();
            TextToSpeechLib.Speak("Score estimation " + currentRating);
        }

        public void VideoPageDataComplete(object sender, DataCompleteEventArgs e)
        {
            if (e.Title.Equals(Recordings.Stop))
            {
                StopVideoPageDataComplete(e);
            }
            else if (e.Title.Equals(Recordings.Start))
            {
                StartVideoPageDataComplete(e);
            }
            else if (e.Title.Equals(Recordings.Update) && e.Method.Equals(MethodTypes.FingerTopTracking))
            {
                UpdateVideoPageDataComplete(e);
            }
            else if (e.Title.Equals(Recordings.TimerTick))
            {
                TimerTickVideoPageDataComplete(e);
            }
        }

        public void setPreviousRating()
        {
            ((VideoPage)navigationRegion.Content).Rating = PreviousRating;
            hasSetPreviousRating = true;
        }

        #endregion Gestures

        #region Speech UI video

        /// <summary>
        /// Switch grammer to listening ui without the special word Kinect needed or to basic with the special word kinect needed
        /// </summary>
        private void switchGrammar()
        {
            if (!listeningUI)
            {
                this.speechlib.SetListeningGrammar();
                listeningUI = true;
                ShowListeningUi();
                showUITimer.Start();
            }
            else
            {
                this.speechlib.SetBasicGrammar();
                HideListeningUi();
                listeningUI = false;
            }
        }

        /// <summary>
        /// Shows speech command possibilities
        /// </summary>
        private void ShowListeningUi()
        {
            MyPopup.IsOpen = true;
            if (navigationRegion.Content is VideoPage)
            {
                ((VideoPage)navigationRegion.Content).ShowListeningUi();
            }
        }

        /// <summary>
        /// Hide speech command possibilities
        /// </summary>
        private void HideListeningUi()
        {
            MyPopup.IsOpen = false;
            if (navigationRegion.Content is VideoPage)
            {
                ((VideoPage)navigationRegion.Content).HideListeningUi();
            }
        }

        /// <summary>
        /// After timer elapsed hide speech command possibilities
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showUITimer_Tick(object sender, EventArgs e)
        {
            switchGrammar();
            showUITimer.IsEnabled = false;
        }

        #endregion Speech UI video

        #region Window Closing

        /// <summary>
        /// Execute un-initialization tasks.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (ContextServiceProcess != null && !ContextServiceProcess.HasExited)
            {
                ContextServiceProcess.StandardInput.Close();
            }
            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }

            if (this.handtracklib != null)
            {
                handtracklib.Dispose();
                handtracklib = null;
            }

            if (this.speechlib != null)
            {
                speechlib.Dispose();
                speechlib = null;
            }
            if (this.gesturelib != null)
            {
                gesturelib.Dispose();
                gesturelib = null;
            }

            TextToSpeechLib.Stop();
            if (WRITE_TO_CSV)
                WriteCsvFiles();

            if (ContextServiceProcess != null && !ContextServiceProcess.HasExited)
            {
                //It can take long for the process to Exit becaus all the cs files will be written to
                ContextServiceProcess.WaitForExit();
            }

            if (FacialRecognitionProcess != null && !FacialRecognitionProcess.HasExited)
            {
                FacialRecognitionProcess.WaitForExit();
            }

            if (EmotionRecognitionProcess != null && !EmotionRecognitionProcess.HasExited)
            {
                EmotionRecognitionProcess.WaitForExit();
            }
        }

        private void WriteCsvFiles()
        {
            actionCsvWriter.WriteToFile();
        }

        #endregion Window Closing
    }
}