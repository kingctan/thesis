using HandtrackLibraryUtilities;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Utils;

namespace HandTrackLibrary
{
    /// <summary>
    /// Library that captures the fingers by creating the controur of the hands.
    /// </summary>
    public class HandTrackLib : DisposableBase
    {
        #region Private properties

        private KinectSensor _kinectSensor;
        private DepthFrameReader _depthReader;
        private BodyFrameReader _bodyReader;

        // Create a new reference of a HandsController.
        private HandsController _handsController;

        private IList<Body> _bodies;
        private Body _body;
        private BackgroundWorker backgroundWorker;

        /// <summary>
        /// Used to update the fingers for drawing in the canvas
        /// </summary>
        private EventHandler<HandCollection> VideoPage_HandsController_HandsDetected;

        #endregion Private properties

        #region Ctor

        public HandTrackLib(KinectSensor kinectSensor, EventHandler<HandCollection> _videoPage_HandsController_HandsDetected)
        {
            if (kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }

            _kinectSensor = kinectSensor;
            VideoPage_HandsController_HandsDetected = _videoPage_HandsController_HandsDetected;

            // Initialize the HandsController and subscribe to the HandsDetected event.
            _handsController = new HandsController(false, true);

            _bodies = new Body[_kinectSensor.BodyFrameSource.BodyCount];

            _depthReader = _kinectSensor.DepthFrameSource.OpenReader();
            _depthReader.FrameArrived += DepthReader_FrameArrived;

            _bodyReader = _kinectSensor.BodyFrameSource.OpenReader();

            _bodyReader.FrameArrived += BodyReader_FrameArrived;

            _handsController.HandsDetected += VideoPage_HandsController_HandsDetected;

            _depthReader.IsPaused = true;
            _bodyReader.IsPaused = true;

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += worker_DoWork;
            backgroundWorker.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
        }

        private DepthFrame depthFrame;

        private KinectBuffer buffer;

        private void worker_DoWork(Object sender, DoWorkEventArgs e)

        {
            depthFrame = (DepthFrame)e.Argument;
            if (depthFrame != null)
            {
                //  Update the HandsController using the array (or pointer) of the depth depth data, and the tracked body.
                //   using (KinectBuffer buffer = frame.LockImageBuffer())
                //  {
                Console.WriteLine("update");
                buffer = depthFrame.LockImageBuffer();
                _handsController.Update(buffer.UnderlyingBuffer, _body);
                //   }
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            depthFrame.Dispose();
            buffer.Dispose();
        }

        public void Start()
        {
            _handsController.HandsDetected += VideoPage_HandsController_HandsDetected;
            _depthReader.IsPaused = false;
            _bodyReader.IsPaused = false;
        }

        #endregion Ctor

        #region Private methods

        private void DepthReader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            depthFrame = e.FrameReference.AcquireFrame();
            if (depthFrame != null)
            {
                if (!backgroundWorker.IsBusy)
                {
                    backgroundWorker.RunWorkerAsync(depthFrame);
                }
                else
                {
                    Console.WriteLine("BackgroundWorker is bussy.");
                }
            }
        }

        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    bodyFrame.GetAndRefreshBodyData(_bodies);

                    _body = _bodies.Where(b => b.IsTracked).FirstOrDefault();
                }
            }
        }

        #endregion Private methods

        #region Public properties

        public bool DetectLeftHand
        {
            get
            {
                return _handsController.DetectLeftHand;
            }
            set
            {
                _handsController.DetectLeftHand = value;
            }
        }

        public bool DetectRightHand
        {
            get
            {
                return _handsController.DetectRightHand;
            }
            set
            {
                _handsController.DetectRightHand = value;
            }
        }

        #endregion Public properties

        #region DisposableBase Implementation

        public void Stop()
        {
            if (_depthReader != null)
            {
                _depthReader.IsPaused = true;
            }
            if (_bodyReader != null)
            {
                _bodyReader.IsPaused = true; ;
            }
            if (_handsController != null)
            {
                _handsController.HandsDetected -= VideoPage_HandsController_HandsDetected;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_kinectSensor != null)
                {
                    _kinectSensor.Close();
                    this._kinectSensor = null;
                }
                Stop();
                if (_handsController != null)
                {
                    _handsController = null;
                }
                if (backgroundWorker != null)
                {
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                }
            }
        }

        ~HandTrackLib()
        {
            Dispose(false);
        }

        #endregion DisposableBase Implementation
    }
}