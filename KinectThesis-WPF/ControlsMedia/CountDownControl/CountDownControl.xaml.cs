using ArgsLibrary;
using Microsoft.Kinect;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using TextToSpeechLibrary;
using Utils;

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    /// <summary>
    /// Interaction logic for CountDownControl.xaml
    /// </summary>
    ///
    //Implemented interface INotifyPropertyChanged and not abstract class BindableBase because of the class already inherited from the class UserControl
    public partial class CountDownControl : UserControl, INotifyPropertyChanged, IDisposable
    {
        #region Private properties

        private DispatcherTimer countDownTimer = new DispatcherTimer();

        /// <summary>
        /// Seconds before the gesture start
        /// </summary>
        private int countdownSeconds;

        /// <summary>
        /// The amount of seconds for the action
        /// </summary>
        private int actionSeconds;

        private bool countdownDone = false;
        private DateTime startTime;

        /// <summary>
        /// The actual type of the action
        /// </summary>
        private MethodTypes method;

        private int elapsed;
        private bool stopped;

        /// <summary>
        /// Textual representation of the counter
        /// </summary>
        private string counter;

        private InfraredFrameReader infraredFrameReader = null;

        /// <summary>
        /// Maximum value (as a float) that can be returned by the InfraredFrame
        /// </summary>
        private const float InfraredSourceValueMaximum = (float)ushort.MaxValue;

        /// <summary>
        /// The value by which the infrared source data will be scaled
        /// </summary>
        private const float InfraredSourceScale = 0.75f;

        /// <summary>
        /// Smallest value to display when the infrared data is normalized
        /// </summary>
        private const float InfraredOutputValueMinimum = 0.01f;

        /// <summary>
        /// Largest value to display when the infrared data is normalized
        /// </summary>
        private const float InfraredOutputValueMaximum = 1.0f;

        /// <summary>
        /// Description (width, height, etc) of the infrared frame data
        /// </summary>
        private FrameDescription infraredFrameDescription = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap infraredBitmap = null;

        private KinectSensor kinectSensor;

        #endregion Private properties

        #region Ctor

        public CountDownControl(KinectSensor _kinectSensor)
        {
            InitializeComponent();
            if (_kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }
            // only one sensor is currently supported
            this.kinectSensor = _kinectSensor;
            this.DataContext = this;
            countDownTimer.Interval = TimeSpan.FromSeconds(1);
            countDownTimer.Tick += countDownTimer_Tick;
            // get FrameDescription from InfraredFrameSource
            this.infraredFrameDescription = this.kinectSensor.InfraredFrameSource.FrameDescription;
            // create the bitmap to display
            this.infraredBitmap = new WriteableBitmap(this.infraredFrameDescription.Width, this.infraredFrameDescription.Height, 96.0, 96.0, PixelFormats.Gray32Float, null);
        }

        /// <summary>
        /// Start of an action and countdown
        /// </summary>
        /// <param name="method"></param>
        public void Start(MethodTypes method)
        {
            this.method = method;
            infraredFrameReader = this.kinectSensor.InfraredFrameSource.OpenReader();
            infraredFrameReader.FrameArrived += Reader_InfraredFrameArrived;
            stopped = false;
            countdownDone = false;
            countdownSeconds = 5;
            if (method.Equals(MethodTypes.DragAndDrop))
            {
                actionSeconds = 10;
            }
            else
            {
                actionSeconds = 17;
            }

            Counter = countdownSeconds.ToString();
            startTime = DateTime.Now;
            canvas.Children.Clear();
            countDownTimer.Start();
        }

        /// <summary>
        /// Timer tick of the countdown control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void countDownTimer_Tick(object sender, EventArgs e)
        {
            //Current time - start time = elapsed time
            elapsed = Convert.ToInt32(DateTime.Now.Subtract(startTime).TotalSeconds);
            //countdownSeconds is the seconds to countdown in the begining or for the action
            if (elapsed >= countdownSeconds)
            {
                if (!countdownDone)
                {
                    //Start the action
                    countdownDone = true;
                    countdownSeconds = actionSeconds;
                    startTime = DateTime.Now;
                    Counter = "GO!";
                    TextToSpeechLib.Speak("Start");
                    OnDataComplete(new DataCompleteEventArgs { Title = Recordings.Start, Method = method });
                }
                else
                {
                    //Stop the action
                    Stop();
                }
            }
            else
            {
                int secs = countdownSeconds - elapsed;
                Counter = secs.ToString();
                if (countdownDone)
                {
                    //Updates for calculations of ratings in between
                    if (method.Equals(MethodTypes.HandUp) && secs % 3 == 0)
                    {
                        OnDataComplete(new DataCompleteEventArgs { Title = Recordings.TimerTick, Method = method });
                    }
                    else if (!method.Equals(MethodTypes.HandUp) && secs % 5 == 0)
                    {
                        OnDataComplete(new DataCompleteEventArgs { Title = Recordings.Update, Method = method });
                    }
                }
            }
        }

        public void Stop()
        {
            if (!stopped)
            {
                stopped = true;
                Counter = String.Empty;
                countDownTimer.Stop();
                OnDataComplete(new DataCompleteEventArgs { Title = Recordings.Stop, Method = method });
                if (infraredFrameReader != null)
                {
                    infraredFrameReader.FrameArrived -= Reader_InfraredFrameArrived;
                    infraredFrameReader.Dispose();
                }
            }
        }

        #endregion Ctor

        #region infrared image

        /// <summary>
        /// Handles the infrared frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_InfraredFrameArrived(object sender, InfraredFrameArrivedEventArgs e)
        {
            // InfraredFrame is IDisposable
            using (InfraredFrame infraredFrame = e.FrameReference.AcquireFrame())
            {
                if (infraredFrame != null)
                {
                    // the fastest way to process the infrared frame data is to directly access
                    // the underlying buffer
                    using (Microsoft.Kinect.KinectBuffer infraredBuffer = infraredFrame.LockImageBuffer())
                    {
                        // verify data and write the new infrared frame data to the display bitmap
                        if (((this.infraredFrameDescription.Width * this.infraredFrameDescription.Height) == (infraredBuffer.Size / this.infraredFrameDescription.BytesPerPixel)) &&
                            (this.infraredFrameDescription.Width == this.infraredBitmap.PixelWidth) && (this.infraredFrameDescription.Height == this.infraredBitmap.PixelHeight))
                        {
                            this.ProcessInfraredFrameData(infraredBuffer.UnderlyingBuffer, infraredBuffer.Size);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Directly accesses the underlying image buffer of the InfraredFrame to
        /// create a displayable bitmap.
        /// This function requires the /unsafe compiler option as we make use of direct
        /// access to the native memory pointed to by the infraredFrameData pointer.
        /// </summary>
        /// <param name="infraredFrameData">Pointer to the InfraredFrame image data</param>
        /// <param name="infraredFrameDataSize">Size of the InfraredFrame image data</param>
        private unsafe void ProcessInfraredFrameData(IntPtr infraredFrameData, uint infraredFrameDataSize)
        {
            if (method.Equals(MethodTypes.FingerTopTracking))
            {
                canvas.Children.Clear();
            }
            // infrared frame data is a 16 bit value
            ushort* frameData = (ushort*)infraredFrameData;

            // lock the target bitmap
            this.infraredBitmap.Lock();

            // get the pointer to the bitmap's back buffer
            float* backBuffer = (float*)this.infraredBitmap.BackBuffer;

            // process the infrared datael
            for (int i = 0; i < (int)(infraredFrameDataSize / this.infraredFrameDescription.BytesPerPixel); ++i)
            {
                // since we are displaying the image as a normalized grey scale image, we need to convert from
                // the ushort data (as provided by the InfraredFrame) to a value from [InfraredOutputValueMinimum, InfraredOutputValueMaximum]
                backBuffer[i] = Math.Min(InfraredOutputValueMaximum, (((float)frameData[i] / InfraredSourceValueMaximum * InfraredSourceScale) * (1.0f - InfraredOutputValueMinimum)) + InfraredOutputValueMinimum);
            }

            // mark the entire bitmap as needing to be drawn
            this.infraredBitmap.AddDirtyRect(new Int32Rect(0, 0, this.infraredBitmap.PixelWidth, this.infraredBitmap.PixelHeight));

            // unlock the bitmap
            this.infraredBitmap.Unlock();
        }

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSourceInfrared
        {
            get
            {
                return this.infraredBitmap;
            }
        }

        public void DrawEllipse(DepthSpacePoint point, Brush brush, double radius)
        {
            Ellipse ellipse = new Ellipse
            {
                Width = radius,
                Height = radius,
                Fill = brush
            };

            canvas.Children.Add(ellipse);

            Canvas.SetLeft(ellipse, point.X - radius / 2.0);
            Canvas.SetTop(ellipse, point.Y - radius / 2.0);
        }

        #endregion infrared image

        #region countdownevent

        public string Counter
        {
            get
            {
                return counter;
            }
            set
            {
                counter = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event EventHandler<DataCompleteEventArgs> DataComplete;

        public virtual void OnDataComplete(DataCompleteEventArgs e)
        {
            if (DataComplete != null)
            {
                DataComplete(this, e);
            }
        }

        #endregion countdownevent

        #region Explicit dispose

        public void Dispose()
        {
            //If the countdown is not already stopped is checked in the method
            Stop();
            //Don't stop the kinect here because when the videopage is out off focus and then back in fous in the kinect will not be opend again becaus there is no new video page who makes the countdown control
            //it is just back in focus. There won't be a new CountDownControl in that situation
            if (infraredFrameReader != null)
            {
                infraredFrameReader.Dispose();
                infraredFrameReader = null;
            }
        }

        #endregion Explicit dispose
    }
}