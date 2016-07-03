namespace GestureLibrary
{
    using ArgsLibrary;
    using Microsoft.Kinect;
    using System;
    using System.Collections.Generic;
    using Utils;

    /// <summary>
    /// Library that recognizes gestures.
    /// </summary>
    public class GestureLib : DisposableBase
    {
        #region Private properties

        /// <summary> Active Kinect sensor </summary>
        private KinectSensor kinectSensor = null;

        /// <summary> Array for the bodies (Kinect will track up to 6 people simultaneously) </summary>
        private Body[] bodies = null;

        /// <summary> Reader for body frames </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary> List of gesture detectors, there will be one detector created for each potential body (max of 6) </summary>
        private List<GestureDetector> gestureDetectorList = null;

        /// <summary>
        /// Field for the pause status for the gesturedetectors in the gesture library
        /// </summary>
        private bool isPaused;

        #endregion Private properties

        #region ctor

        /// <summary>
        /// Initializes a new instance of the MainWindow class
        /// </summary>
        public GestureLib(KinectSensor _kinectSensor, EventHandler<UpdateGestureEventArgs> _updateGesture)
        {
            if (_kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }
            // only one sensor is currently supported
            this.kinectSensor = _kinectSensor;
            // initialize the gesture detection objects for our gestures
            this.gestureDetectorList = new List<GestureDetector>();
            int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount;
            for (int i = 0; i < maxBodies; ++i)
            {
                GestureDetector detector = new GestureDetector(this.kinectSensor, _updateGesture);
                this.gestureDetectorList.Add(detector);
            }

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // set the BodyFramedArrived event notifier
            this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;
        }

        #endregion ctor

        #region Implementation

        public bool IsPaused
        {
            get
            {
                return isPaused;
            }
            set
            {
                if (isPaused != value)
                {
                    isPaused = value;
                    for (int i = 0; i < 6; i++)
                    {
                        gestureDetectorList[i].TrackingId = 0;
                        gestureDetectorList[i].IsPaused = value;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the body frame data arriving from the sensor and updates the associated gesture detector object for each body
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        // creates an array of 6 bodies, which is the max number of bodies that Kinect can track simultaneously
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                // we may have lost/acquired bodies, so update the corresponding gesture detectors
                if (this.bodies != null)
                {
                    // loop through all bodies to see if any of the gesture detectors need to be updated
                    int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount;
                    for (int i = 0; i < maxBodies; ++i)
                    {
                        Body body = this.bodies[i];
                        ulong trackingId = body.TrackingId;
                        // if the current body TrackingId changed, update the corresponding gesture detector with the new value
                        if (trackingId != this.gestureDetectorList[i].TrackingId)
                        {
                            if (!IsPaused)
                            {
                                // if the current body is tracked, unpause its detector to get VisualGestureBuilderFrameArrived events
                                // if the current body is not tracked, pause its detector so we don't waste resources trying to get invalid gesture results
                                this.gestureDetectorList[i].TrackingId = trackingId;
                                this.gestureDetectorList[i].IsPaused = trackingId == 0;
                            }
                        }
                    }
                }
            }
        }

        #endregion Implementation

        #region DisposableBase Implementation

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (kinectSensor != null)
                {
                    //Don't close the kinect it is neeeded throughout the application
                    //kinectSensor.Close();
                    this.kinectSensor = null;
                }
                if (this.bodyFrameReader != null)
                {
                    // BodyFrameReader is IDisposable
                    this.bodyFrameReader.FrameArrived -= this.Reader_BodyFrameArrived;
                    this.bodyFrameReader.Dispose();
                    this.bodyFrameReader = null;
                }

                if (this.gestureDetectorList != null)
                {
                    // The GestureDetector contains disposable members (VisualGestureBuilderFrameSource and VisualGestureBuilderFrameReader)
                    foreach (GestureDetector detector in this.gestureDetectorList)
                    {
                        detector.Dispose();
                    }

                    this.gestureDetectorList.Clear();
                    this.gestureDetectorList = null;
                }
            }
        }

        ~GestureLib()
        {
            Dispose(false);
        }

        #endregion DisposableBase Implementation
    };
}