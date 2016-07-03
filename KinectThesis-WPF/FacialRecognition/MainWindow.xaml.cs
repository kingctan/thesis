using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using Newtonsoft.Json;
using Sacknet.KinectFacialRecognition;
using Sacknet.KinectFacialRecognition.KinectFaceModel;
using Sacknet.KinectFacialRecognition.ManagedEigenObject;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TextToSpeechLibrary;

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    /// <summary>
    /// Interaction logic for FacialRecognitionWindow.xaml
    /// </summary>
    public partial class FacialRecognitionWindow : Window
    {
        #region Private properties

        private bool takeTrainingImage = false;
        private KinectFacialRecognitionEngine engine;

        private IRecognitionProcessor activeProcessor;
        private KinectSensor kinectSensor;
        private FacialRecognitionWindowViewModel viewModel = new FacialRecognitionWindowViewModel();
        private bool speakLeftViews = true;
        private bool speakRightViews = true;
        private bool speakUpViews = true;
        private bool speakFrontViews = true;
        private bool speakNoViews = true;
        private TrackedFace face;
        private String username = "Guest";
        private DispatcherTimer textToSpeechTimer;
        private const string normalText = "Enter your name and press capture - a training picture will be taken in 2 seconds.You must have at least 2 training images to enable recognition.";
        private const string firstEnterNameText = "First enter your name";

        #endregion Private properties

        #region Ctor

        public FacialRecognitionWindow()
        {
            this.InitializeComponent();
            this.DataContext = this.viewModel;
            //trainname enabled with start
            this.kinectSensor = KinectSensor.GetDefault();
            if (kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }
            this.kinectSensor.Open();
            this.viewModel.EnterText = normalText;
            this.viewModel.TrainName = "Enter name here...";
            this.viewModel.TrainButtonClicked = new ActionCommand(this.Train);
            this.viewModel.GoToLibraryClicked = new ActionCommand(this.GoToLibrary);
            this.viewModel.GoToLibraryDirectlyClicked = new ActionCommand(this.GoToLibraryDirectly);
            this.viewModel.TrainNameEnabled = true;
            this.LoadProcessor();
            textToSpeechTimer = new DispatcherTimer();
            textToSpeechTimer.Interval = new TimeSpan(0, 0, 15);
            textToSpeechTimer.Tick += (s2, e2) =>
            {
                speakLeftViews = true;
                speakRightViews = true;
                speakUpViews = true;
                speakFrontViews = true;
            };
            textToSpeechTimer.Start();
        }

        #endregion Ctor

        #region Creating bitmap

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        /// <summary>
        /// Loads a bitmap into a bitmap source
        /// </summary>
        private static BitmapSource LoadBitmap(Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                bs.Freeze();
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }

        #endregion Creating bitmap

        #region Placeholder textbox

        private void textBox_GotFocus(object sender, RoutedEventArgs e)
        {
            textBox.Text = "";
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textBox.Text))
                textBox.Text = "Enter name here...";
        }

        #endregion Placeholder textbox

        /// <summary>
        /// Loads the processor
        /// </summary>
        private void LoadProcessor()
        {
            this.activeProcessor = new FaceModelRecognitionProcessor();
            this.LoadAllTargetFaces();
            this.UpdateTargetFaces();

            if (this.engine == null)
            {
                this.engine = new KinectFacialRecognitionEngine(this.kinectSensor, this.activeProcessor);
                this.engine.RecognitionComplete += this.Engine_RecognitionComplete;
            }

            this.engine.Processors = new List<IRecognitionProcessor> { this.activeProcessor };
        }

        /// <summary>
        /// Handles recognition complete events
        /// </summary>
        private void Engine_RecognitionComplete(object sender, RecognitionResult e)
        {
            face = null;

            if (e.Faces != null)
            {
                face = e.Faces.FirstOrDefault();
            }

            using (var processedBitmap = (Bitmap)e.ColorSpaceBitmap.Clone())
            {
                if (face == null)
                {
                    //no face detected no training possible
                    this.viewModel.ReadyForTraining = false;
                }
                else
                {//Only when there is a face in the frame training is possible
                    using (var g = Graphics.FromImage(processedBitmap))
                    {
                        var rect = face.TrackingResult.FaceRect;
                        var faceOutlineColor = Color.Green;
                        if (face.TrackingResult.ConstructedFaceModel == null)
                        {
                            faceOutlineColor = Color.Red;

                            if (face.TrackingResult.BuilderStatus == FaceModelBuilderCollectionStatus.Complete)
                                faceOutlineColor = Color.Orange;
                        }

                        var scale = (rect.Width + rect.Height) / 6;
                        var midX = rect.X + (rect.Width / 2);
                        var midY = rect.Y + (rect.Height / 2);

                        if ((face.TrackingResult.BuilderStatus & FaceModelBuilderCollectionStatus.LeftViewsNeeded) == FaceModelBuilderCollectionStatus.LeftViewsNeeded)
                        {
                            g.FillRectangle(new SolidBrush(Color.Red), rect.X - (scale * 2), midY, scale, scale);
                            if (speakLeftViews)
                            {
                                TextToSpeechLib.SpeakNotInterrupt("Left views needed");
                                speakLeftViews = false;
                            }
                        }

                        if ((face.TrackingResult.BuilderStatus & FaceModelBuilderCollectionStatus.RightViewsNeeded) == FaceModelBuilderCollectionStatus.RightViewsNeeded)
                        {
                            g.FillRectangle(new SolidBrush(Color.Red), rect.X + rect.Width + (scale * 2), midY, scale, scale);
                            if (speakRightViews)
                            {
                                TextToSpeechLib.SpeakNotInterrupt("Right views needed");
                                speakRightViews = false;
                            }
                        }

                        if ((face.TrackingResult.BuilderStatus & FaceModelBuilderCollectionStatus.TiltedUpViewsNeeded) == FaceModelBuilderCollectionStatus.TiltedUpViewsNeeded)
                        {
                            g.FillRectangle(new SolidBrush(Color.Red), midX, rect.Y - (scale * 2), scale, scale);
                            if (speakUpViews)
                            {
                                TextToSpeechLib.SpeakNotInterrupt("Up views needed");
                                speakUpViews = false;
                            }
                        }
                        if ((face.TrackingResult.BuilderStatus & FaceModelBuilderCollectionStatus.FrontViewFramesNeeded) == FaceModelBuilderCollectionStatus.FrontViewFramesNeeded)
                        {
                            g.FillRectangle(new SolidBrush(Color.Red), midX, midY, scale, scale);
                            if (speakFrontViews)
                            {
                                TextToSpeechLib.SpeakNotInterrupt("Front views needed");
                                speakFrontViews = false;
                            }
                        }

                        this.viewModel.ReadyForTraining = faceOutlineColor == Color.Green;

                        g.DrawPath(new Pen(faceOutlineColor, 5), face.TrackingResult.GetFacePath());
                        if (faceOutlineColor == Color.Green && speakNoViews)
                        {
                            TextToSpeechLib.SpeakNotInterrupt("Ready to train and log in");
                            speakNoViews = false;
                        }

                        if (!string.IsNullOrEmpty(face.Key))
                        {
                            username = face.Key;
                            this.viewModel.TrainName = username;
                            this.viewModel.GoToLibraryClickedEnabled = true;
                            var score = Math.Round(face.ProcessorResults.First().Score, 2);
                            // Write the key on the image...
                            // g.DrawString(face.Key + ": " + score, new Font("Arial", 100), Brushes.Red, new System.Drawing.Point(rect.Left, rect.Top - 25));
                            g.DrawString(face.Key, new Font("Arial", 100), Brushes.Red, new System.Drawing.Point(rect.Left, rect.Top - 25));
                        }
                    }

                    if (this.takeTrainingImage)
                    {
                        var fmResult = (FaceModelRecognitionProcessorResult)face.ProcessorResults.SingleOrDefault(x => x is FaceModelRecognitionProcessorResult);

                        var bstf = new BitmapSourceTargetFace();
                        if (this.viewModel.TrainName.Equals("Enter name here..."))
                        {
                            this.viewModel.EnterText = firstEnterNameText;
                        }
                        else
                        {
                            this.viewModel.EnterText = normalText;
                            bstf.Key = this.viewModel.TrainName;

                            bstf.Image = face.TrackingResult.GetCroppedFace(e.ColorSpaceBitmap);

                            if (fmResult != null)
                            {
                                bstf.Deformations = fmResult.Deformations; //add a target face to the list
                                bstf.HairColor = fmResult.HairColor;
                                bstf.SkinColor = fmResult.SkinColor;
                            }

                            this.viewModel.TargetFaces.Add(bstf);

                            this.SerializeBitmapSourceTargetFace(bstf);
                            //write to file

                            this.takeTrainingImage = false;

                            this.UpdateTargetFaces();
                        }
                    }
                }

                this.viewModel.CurrentVideoFrame = LoadBitmap(processedBitmap);
            }

            // Without an explicit call to GC.Collect here, memory runs out of control :(
            //ask garbage collection explicit
            GC.Collect();
        }

        /// <summary>
        /// Saves the target face to disk
        /// </summary>
        private void SerializeBitmapSourceTargetFace(BitmapSourceTargetFace bstf)
        {
            var filenamePrefix = "TF_" + DateTime.Now.Ticks.ToString();
            var suffix = ".fmb";
            //writes the fmb file to the x86 directiory on disk
            System.IO.File.WriteAllText(filenamePrefix + suffix, JsonConvert.SerializeObject(bstf));
            bstf.Image.Save(filenamePrefix + ".png"); //save image to disk
        }

        /// <summary>
        /// Loads all BSTFs from the current directory
        /// </summary>
        private void LoadAllTargetFaces()
        {
            this.viewModel.TargetFaces.Clear();
            var result = new List<BitmapSourceTargetFace>();
            var suffix = ".fmb";
            foreach (var file in Directory.GetFiles(".", "TF_*" + suffix)) //gets all files from disk
            {
                var bstf = JsonConvert.DeserializeObject<BitmapSourceTargetFace>(File.ReadAllText(file));
                bstf.Image = (Bitmap)Bitmap.FromFile(file.Replace(suffix, ".png"));
                this.viewModel.TargetFaces.Add(bstf);
            }
        }

        /// <summary>
        /// Updates the target faces
        /// </summary>
        private void UpdateTargetFaces()
        {
            if (this.viewModel.TargetFaces.Count > 1)
                this.activeProcessor.SetTargetFaces(this.viewModel.TargetFaces); //add target faces to face processor
            this.viewModel.TrainName = this.viewModel.TrainName.Replace(this.viewModel.TargetFaces.Count.ToString(), (this.viewModel.TargetFaces.Count + 1).ToString()); // add target faces to list
        }

        /// <summary>
        /// Starts the training image countdown
        /// </summary>
        private void Train()
        {
            this.viewModel.TrainingInProcess = true;

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += (s2, e2) =>
            {
                timer.Stop();
                this.viewModel.TrainingInProcess = false;
                takeTrainingImage = true;
            };
            timer.Start();
        }

        /// <summary>
        /// Target face with a BitmapSource accessor for the face
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        public class BitmapSourceTargetFace : IEigenObjectTargetFace, IFaceModelTargetFace
        {
            private BitmapSource bitmapSource;

            /// <summary>
            /// Gets the BitmapSource version of the face
            /// </summary>
            public BitmapSource BitmapSource
            {
                get
                {
                    if (this.bitmapSource == null)
                        this.bitmapSource = FacialRecognitionWindow.LoadBitmap(this.Image);

                    return this.bitmapSource;
                }
            }

            /// <summary>
            /// Gets or sets the key returned when this face is found
            /// </summary>
            [JsonProperty]
            public string Key { get; set; }

            /// <summary>
            /// Gets or sets the grayscale, 100x100 target image
            /// </summary>
            public Bitmap Image { get; set; }

            /// <summary>
            /// Gets or sets the detected hair color of the face
            /// </summary>
            [JsonProperty]
            public Color HairColor { get; set; }

            /// <summary>
            /// Gets or sets the detected skin color of the face
            /// </summary>
            [JsonProperty]
            public Color SkinColor { get; set; }

            /// <summary>
            /// Gets or sets the detected deformations of the face
            /// </summary>
            [JsonProperty]
            public IReadOnlyDictionary<FaceShapeDeformations, float> Deformations { get; set; }
        }

        private void GoToLibrary()
        {
            Console.WriteLine(username);
            this.Close();
        }

        private void GoToLibraryDirectly()
        {
            Console.WriteLine("Guest");
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TextToSpeechLib.Stop();
        }
    }
}