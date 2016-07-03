using Csv;
using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EmotionRecognition
{
    public partial class MainWindow : Window
    {
        #region Private properties

        /// <summary>
        /// Urls when the server wicaweb9.intec.ugent.be is used.
        /// </summary>

        private const string WEBSERVICE_URL = "http://wicaweb9.intec.ugent.be:5000/predict?";
        private const string WEBSERVICE_BLOCK_URL = "http://wicaweb9.intec.ugent.be:5000/predict/block?";
        private const string WEBSERVICE_URL_PROBA = "http://wicaweb9.intec.ugent.be:5000/predict/proba?";
        private const string WEBSERVICE_BLOCK_URL_PROBA = "http://wicaweb9.intec.ugent.be:5000/predict/proba/block?";

        /// <summary>
        /// Urls when the local sever is used.
        /// </summary>
        ///

        private const string WEBSERVICE_LOCAL_URL = "http://127.0.0.1:5000/predict?";
        private const string WEBSERVICE_LOCAL_BLOCK_URL = "http://127.0.0.1:5000/predict/block?";
        private const string WEBSERVICE_LOCAL_URL_PROBA = "http://127.0.0.1:5000/predict/proba?";
        private const string WEBSERVICE_LOCAL_BLOCK_URL_PROBA = "http://127.0.0.1:5000/predict/proba/block?";

        /// <summary>
        /// Needed to build the url
        /// </summary>
        private StringBuilder sb;

        /// <summary>
        /// The emotion recognition part in his own thread so that the UI doesn't block
        /// </summary>
        private BackgroundWorker backgroundWorker;

        private KinectSensor _kinectSensor = null;

        /// <summary>
        /// Counts the amount of samples in a block, can send 20 samples in one resuest
        /// </summary>
        private int dataCount = 1;

        /// <summary>
        /// The size of a block of samples, there are less http requests
        /// </summary>
        private const int BLOCK_SIZE = 20;

        private BodyFrameSource _bodySource = null;

        private BodyFrameReader _bodyReader = null;

        private HighDefinitionFaceFrameSource _faceSource = null;

        private HighDefinitionFaceFrameReader _faceReader = null;

        private FaceAlignment _faceAlignment = null;

        private FaceModel _faceModel = null;

        /// <summary>
        /// Points of the face
        /// </summary>
        private List<Ellipse> _points;

        /// <summary>
        /// Depricated, Action Units for training the classifiers
        /// </summary>
        private CsvWriter writer;

        /// <summary>
        /// Depricated. The name of the current emotion. Writing the real-time action units to this csv.
        /// </summary>
        private string emotion = "ActionUnits";

        #endregion Private properties

        #region Ctor

        public MainWindow()
        {
            InitializeComponent();
            _kinectSensor = KinectSensor.GetDefault();
            _kinectSensor.Open();
            if (_kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }
            //Sets the separator to "." for every thread. This is needed for the action units in the csv file and python scripts.
            CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentUICulture = customCulture;
            Thread.CurrentThread.CurrentCulture = customCulture;
            CultureInfo.DefaultThreadCurrentCulture = customCulture;
            CultureInfo.DefaultThreadCurrentUICulture = customCulture;

            sb = new StringBuilder();
            _points = new List<Ellipse>();
            writer = new CsvWriter(emotion + ".csv", 17);
            //In this situation writing 17 features to ActionUnits.csv
            //CurrentSituation = Situation = RecognitionEnum.WRITE_CSV;
            CurrentSituation = Situation = RecognitionEnum.BLOCK;
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += bw_DoWork;
            backgroundWorker.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
        }

        /// <summary>
        /// Create all the kinect sources and readers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _bodySource = _kinectSensor.BodyFrameSource;
            _bodyReader = _bodySource.OpenReader();
            _bodyReader.FrameArrived += BodyReader_FrameArrived;

            _faceSource = new HighDefinitionFaceFrameSource(_kinectSensor);

            _faceReader = _faceSource.OpenReader();
            _faceReader.FrameArrived += FaceReader_FrameArrived;

            _faceModel = new FaceModel();
            _faceAlignment = new FaceAlignment();
        }

        /// <summary>
        /// Closes all streams and readers and write to all the csv files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            writer.WriteToFile();
            if (_kinectSensor != null)
            {
                _kinectSensor.Close();
                _kinectSensor = null;
            }

            if (_faceModel != null)
            {
                _faceModel.Dispose();
                _faceModel = null;
            }
            if (backgroundWorker != null)
            {
                backgroundWorker.Dispose();
                backgroundWorker = null;
            }

            GC.SuppressFinalize(this);
        }

        #endregion Ctor

        #region Public properties

        /// <summary>
        /// How the emotion results will be interpreted
        /// </summary>
        public RecognitionEnum Situation
        {
            get;
            private set;
        }

        /// <summary>
        /// How the emotion results will be interpreted
        /// </summary>
        public RecognitionEnum CurrentSituation
        {
            get;
            private set;
        }

        #endregion Public properties

        #region Private methods

        /// <summary>
        /// Event fired when other interpretation method has been chosen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = (sender as ComboBox).SelectedIndex;
            if (index == 0)
            {
                Situation = RecognitionEnum.BLOCK;
            }
            else if (index == 1)
            {
                TextBlock2.Text = string.Empty;
                Situation = RecognitionEnum.BLOCK_PROBA;
            }
            else if (index == 2)
            {
                TextBlock2.Text = string.Empty;
                Situation = RecognitionEnum.REAL_TIME;
            }
            else if (index == 3)
            {
                TextBlock2.Text = string.Empty;
                Situation = RecognitionEnum.REAL_TIME_PROBA;
            }
        }

        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    Body[] bodies = new Body[frame.BodyCount];
                    frame.GetAndRefreshBodyData(bodies);

                    Body body = bodies.Where(b => b.IsTracked).FirstOrDefault();

                    if (!_faceSource.IsTrackingIdValid)
                    {
                        if (body != null)
                        {
                            _faceSource.TrackingId = body.TrackingId;
                        }
                    }
                }
            }
        }

        private void FaceReader_FrameArrived(object sender, HighDefinitionFaceFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null && frame.IsFaceTracked)
                {
                    frame.GetAndRefreshFaceAlignmentResult(_faceAlignment);
                    UpdateFacePoints();
                }
            }
        }

        /// <summary>
        /// Send Http get request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private List<string> GET(string url)
        {
            HttpWebRequest httpWebRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;

            using (HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse)
            {
                if (httpWebResponse.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception(string.Format("Server error (HTTP {0}: {1}).",
                        httpWebResponse.StatusCode, httpWebResponse.StatusDescription));
                }

                StreamReader reader = new StreamReader(httpWebResponse.GetResponseStream());
                string result = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<List<string>>(result);
            }
        }

        /// <summary>
        /// Send Http get request resulting in emotion probabilities
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private List<Emotions> GET_PROBA(string url)
        {
            HttpWebRequest httpWebRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;

            using (HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse)
            {
                if (httpWebResponse.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception(string.Format("Server error (HTTP {0}: {1}).",
                        httpWebResponse.StatusCode, httpWebResponse.StatusDescription));
                }

                StreamReader reader = new StreamReader(httpWebResponse.GetResponseStream());
                string result = reader.ReadToEnd();
                List<Emotions> res = JsonConvert.DeserializeObject<List<Emotions>>(result);

                return res;
            }
        }

        /// <summary>
        /// Send GEt-request to server with current weights for AUs in another thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            //Change current Situation to the new one if there are not the same
            if (CurrentSituation != Situation)
            {
                CurrentSituation = Situation;
            }
            float[] data = { _faceAlignment.AnimationUnits[FaceShapeAnimations.JawOpen],
                _faceAlignment.AnimationUnits[FaceShapeAnimations.JawSlideRight],
                _faceAlignment.AnimationUnits[FaceShapeAnimations.LeftcheekPuff],
                _faceAlignment.AnimationUnits[FaceShapeAnimations.LefteyebrowLowerer],
                _faceAlignment.AnimationUnits[FaceShapeAnimations.LefteyeClosed],
                _faceAlignment.AnimationUnits[FaceShapeAnimations.RighteyebrowLowerer],
                _faceAlignment.AnimationUnits[FaceShapeAnimations.RighteyeClosed],
                _faceAlignment.AnimationUnits[FaceShapeAnimations.LipCornerDepressorLeft],
                _faceAlignment.AnimationUnits[FaceShapeAnimations.LipCornerDepressorRight],
                _faceAlignment.AnimationUnits[FaceShapeAnimations.LipCornerPullerLeft],
                _faceAlignment.AnimationUnits[FaceShapeAnimations.LipCornerPullerRight],
                _faceAlignment.AnimationUnits[FaceShapeAnimations.LipPucker],
                _faceAlignment.AnimationUnits[FaceShapeAnimations.LipStretcherLeft],
                _faceAlignment.AnimationUnits[FaceShapeAnimations.LipStretcherRight],
                _faceAlignment.AnimationUnits[FaceShapeAnimations.LowerlipDepressorLeft],
                _faceAlignment.AnimationUnits[FaceShapeAnimations.LowerlipDepressorRight],
                _faceAlignment.AnimationUnits[FaceShapeAnimations.RightcheekPuff]};
            string[] dataString = Array.ConvertAll<float, string>(data, System.Convert.ToString);

            if (CurrentSituation == RecognitionEnum.REAL_TIME)
            {
                e.Result = GET(WEBSERVICE_URL + "data=" + String.Join(",", dataString))[0];
            }
            else if (CurrentSituation == RecognitionEnum.REAL_TIME_PROBA)
            {
                e.Result = GET_PROBA(WEBSERVICE_URL_PROBA + "data=" + String.Join(",", dataString))[0].ToString();
            }
            else if (CurrentSituation == RecognitionEnum.BLOCK || CurrentSituation == RecognitionEnum.BLOCK_PROBA)
            {
                sb.Append("data").Append(dataCount).Append("=").Append(string.Join(",", dataString)).Append("&");
                if (dataCount == BLOCK_SIZE)
                {
                    if (CurrentSituation == RecognitionEnum.BLOCK)
                    {
                        e.Result = GET(WEBSERVICE_BLOCK_URL + sb.ToString());
                    }
                    else
                    {
                        e.Result = GET_PROBA(WEBSERVICE_BLOCK_URL_PROBA + sb.ToString());
                    }

                    sb.Clear();
                    dataCount = 0;
                }
                dataCount++;
            }
            else if (CurrentSituation == RecognitionEnum.WRITE_CSV)
            {
                e.Result = dataString;
            }
        }

        /// <summary>
        /// This event handler deals with the results of the background operation that sends http GET request with the data of the face.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Result == null) return;
                if (CurrentSituation == RecognitionEnum.REAL_TIME)
                {
                    TextBlock1.Text = Convert.ToString(e.Result);
                }
                else if (CurrentSituation == RecognitionEnum.REAL_TIME_PROBA)
                {
                    TextBlock1.Text = Convert.ToString(e.Result);
                }
                else if (CurrentSituation == RecognitionEnum.BLOCK || CurrentSituation == RecognitionEnum.BLOCK_PROBA)
                {
                    if (CurrentSituation == RecognitionEnum.BLOCK)
                    {
                        List<string> res = (List<string>)e.Result;
                        TextBlock2.Text = res.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
                        TextBlock1.Text = String.Join(",", res);
                    }
                    else
                    {
                        List<Emotions> res = (List<Emotions>)e.Result;
                        string text = String.Empty;
                        foreach (Emotions emotions in res)
                        {
                            text += emotions.ToString() + Environment.NewLine;
                        }
                        TextBlock1.Text = text;
                    }
                }
                else if (CurrentSituation == RecognitionEnum.WRITE_CSV)
                {
                    writer.WriteRow((string[])e.Result);
                }
            }
            catch (TargetInvocationException exception)
            {//Could be: 1) an exception for asking a get_proba to a classifier who doesn't have that functionality
             //      2) an exception because of the server is down
                TextBlock2.Text = String.Empty;

                if (exception.InnerException.Message.Equals("De externe server heeft een fout geretourneerd: (500) Interne serverfout."))
                {
                    TextBlock1.Text = "The classifier on the server can't give probabilities.";
                }
                else
                {
                    TextBlock1.Text = "Server is not reachable (contact iMinds-WiCa).";
                }
            }
        }

        /// <summary>
        /// Draw face points and do request when background worker is not handeling another request
        /// </summary>
        private void UpdateFacePoints()
        {
            if (_faceModel == null) return;

            var vertices = _faceModel.CalculateVerticesForAlignment(_faceAlignment);
            //Only a new request when the backgroundworker is ready
            if (backgroundWorker.IsBusy != true)
            {
                backgroundWorker.RunWorkerAsync();
            }
            if (vertices.Count > 0)
            {
                if (_points.Count == 0)
                {
                    for (int index = 0; index < vertices.Count; index++)
                    {
                        Ellipse ellipse = new Ellipse
                        {
                            Width = 2.0,
                            Height = 2.0,
                            Fill = new SolidColorBrush(Colors.Blue)
                        };

                        _points.Add(ellipse);
                    }

                    foreach (Ellipse ellipse in _points)
                    {
                        canvas.Children.Add(ellipse);
                    }
                }

                for (int index = 0; index < vertices.Count; index++)
                {
                    CameraSpacePoint vertice = vertices[index];
                    DepthSpacePoint point = _kinectSensor.CoordinateMapper.MapCameraPointToDepthSpace(vertice);

                    if (float.IsInfinity(point.X) || float.IsInfinity(point.Y)) return;

                    Ellipse ellipse = _points[index];

                    Canvas.SetLeft(ellipse, point.X);
                    Canvas.SetTop(ellipse, point.Y);
                }
            }
        }

        #endregion Private methods
    }
}