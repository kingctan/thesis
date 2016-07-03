using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using System;
using System.Linq;
using System.Reactive.Linq;
using Utils;

namespace GazeTrackLibrary
{
    /// <summary>
    /// Captures the gaze of the user.
    /// </summary>
    public class GazeTrackLib : DisposableBase
    {
        #region Private Fields

        private KinectSensor _kinectSensor;
        private IObservable<BodyFrame> _bodyFrameObservable;
        private IObservable<FPoint> _hdFaceTrackingObservable;
        private Body[] _bodies = null;
        private Body _currentTrackedBody;
        private BodyFrameReader _bodyFrameReader = null;
        private HighDefinitionFaceFrameSource _highDefinitionFaceFrameSource = null;
        private HighDefinitionFaceFrameReader _highDefinitionFaceFrameReader = null;
        private FaceAlignment _currentFaceAlignment = null;
        private IDisposable _bodyFrameSubscription;
        private IDisposable _hdFaceTrackingSubscription;

        #endregion Private Fields

        #region Constructor

        public GazeTrackLib(KinectSensor kinectSensor, double containerWidth, double containerHeight)
        {
            if (kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }
            _kinectSensor = kinectSensor;
            ContainerWidth = containerWidth;
            ContainerHeight = containerHeight;
            CreateBodyFrameObservable();
            CreateHDFaceObservable();
            CreatePixelsObservable();

            _bodyFrameSubscription = _bodyFrameObservable.Subscribe();
            _hdFaceTrackingSubscription = _hdFaceTrackingObservable.Subscribe();
        }

        #endregion Constructor

        #region Public Properties

        public double ContainerWidth
        {
            get;
            set;
        }

        public double ContainerHeight
        {
            get;
            set;
        }

        public IObservable<FPoint> TrackingXYObservable_Pixels { get; private set; }

        #endregion Public Properties

        #region Private Properties

        private ulong CurrentTrackingId { get; set; }

        #endregion Private Properties

        #region Private Methods

        /// <summary>
        /// BodyFrame event that returns the closest body and sets the tracking id of that body
        /// on the tracking id of the highDefinitionFaceFrameSource
        /// </summary>
        private void CreateBodyFrameObservable()
        {
            _bodyFrameReader = _kinectSensor.BodyFrameSource.OpenReader();
            _bodyFrameObservable = Observable.FromEventPattern<BodyFrameArrivedEventArgs>(_bodyFrameReader, "FrameArrived")
                .Select(f => f.EventArgs.FrameReference.AcquireFrame())
                .Where(bodyFrame => bodyFrame != null)
                .Select(bodyFrame =>
                {
                    if (bodyFrame != null)
                    {
                        if (_bodies == null)
                        {
                            // initialise to number of bodies supported by the Kinect
                            _bodies = new Body[bodyFrame.BodyFrameSource.BodyCount];
                        }
                        else
                        {
                            bodyFrame.GetAndRefreshBodyData(_bodies);
                        }
                        bodyFrame.Dispose();
                    }

                    if (_bodies != null)
                    {
                        _currentTrackedBody = FindBodyWithTrackingId(CurrentTrackingId, _bodies);
                        if (_currentTrackedBody == null)
                        {
                            SelectClosestBody();
                        }

                        // set the HighDefinitionFaceFrameSource tracking Id to the current (closest) person's tracking Id
                        if (this._highDefinitionFaceFrameSource != null)
                        {
                            this._highDefinitionFaceFrameSource.TrackingId = CurrentTrackingId;
                        }
                    }

                    return bodyFrame;
                });
        }

        /// <summary>
        /// Gets the x and y data from the highDefinitionFaceFrame, with the head as center point
        /// </summary>
        private void CreateHDFaceObservable()
        {
            _currentFaceAlignment = new FaceAlignment();

            _highDefinitionFaceFrameSource = new HighDefinitionFaceFrameSource(_kinectSensor);
            _highDefinitionFaceFrameReader = _highDefinitionFaceFrameSource.OpenReader();

            _hdFaceTrackingObservable = Observable.FromEventPattern<HighDefinitionFaceFrameArrivedEventArgs>(_highDefinitionFaceFrameReader, "FrameArrived")
               .Select(frame => frame.EventArgs.FrameReference.AcquireFrame())
               .Where(frame => frame != null)
               .Select(frame =>
               {
                   if (frame.IsFaceTracked)
                   {
                       // update our face alignment data
                       frame.GetAndRefreshFaceAlignmentResult(_currentFaceAlignment);
                   }
                   //Can not be disposed reactive famework throws error and still needs the frame
                   //frame.Dispose();
                   return _currentFaceAlignment;
               })
               .Select(faceAlignment => new FPoint
               {
                   X = faceAlignment.FaceOrientation.X,
                   Y = faceAlignment.FaceOrientation.Y,
               });
        }

        /// <summary>
        /// Sets the point of the face to the middle of the screen and maps the face orientation from faceAlignment between the screen boundaries
        /// </summary>
        private void CreatePixelsObservable()
        {
            TrackingXYObservable_Pixels = _hdFaceTrackingObservable.Select(e =>
            {
                // set the limits for x and y axis
                double xLimit = .1d;
                double yLimit = .2d;

                // apply limits / normalise values between positive and negative limits
                double x = (e.X < xLimit ? e.X : xLimit);
                double y = (e.Y < yLimit ? e.Y : yLimit);

                x = x < -xLimit ? -xLimit : x;
                y = y < -yLimit ? -yLimit : y;

                //shift x between 0 and 0.2, shift y between 0 and 0.4
                x += xLimit;
                y += yLimit;

                // do the conversion between the steps between the limits and the steps for the container
                var xMulti = (ContainerHeight / 2) / (xLimit * 1000);
                var yMulti = (ContainerWidth / 2) / (yLimit * 1000);

                //X between 0 and containerheight, Y between 0 and container width
                x *= 1000 * xMulti;
                y *= 1000 * yMulti;

                // reverse values so that pixel 0,0 corresponds to kinect 0,0 and not pixel maxX,maxY
                x = ContainerHeight - x;
                y = ContainerWidth - y;

                // placing x into y and y into x as these are screen as opposed to 3d coordinates now
                return new FPoint { X = (float)Math.Truncate(y), Y = (float)Math.Truncate(x) };
            })
              .Buffer(TimeSpan.FromMilliseconds(400), TimeSpan.FromMilliseconds(10))
              .Select(listOfScreenPixels =>
              {
                  var result = new FPoint();
                  if (listOfScreenPixels.Any())
                  {
                      result.X = listOfScreenPixels.Average(p => p.X);
                      result.Y = listOfScreenPixels.Average(p => p.Y);
                  }
                  return result;
              });
        }

        #endregion Private Methods

        #region Utility functions from SDK

        private void SelectClosestBody()
        {
            _currentTrackedBody = FindClosestBody(_bodies);
            CurrentTrackingId = _currentTrackedBody == null ? 0 : _currentTrackedBody.TrackingId;
        }

        private Body FindBodyWithTrackingId(ulong trackingId, Body[] bodies)
        {
            return bodies.SingleOrDefault(b => b != null && b.IsTracked == true && b.TrackingId == trackingId);
        }

        private Body FindClosestBody(Body[] bodies)
        {
            Body result = null;
            double closestBodyDistance = double.MaxValue;

            foreach (var body in bodies)
            {
                if (body != null && body.IsTracked)
                {
                    var joints = body.Joints;
                    var currentJoint = joints[JointType.SpineBase];

                    var currentLocation = currentJoint.Position;

                    var currentDistance = VectorLength(currentLocation);

                    if (result == null || currentDistance < closestBodyDistance)
                    {
                        result = body;
                        closestBodyDistance = currentDistance;
                    }
                }
            }

            return result;
        }

        private static double VectorLength(CameraSpacePoint point)
        {
            return Math.Sqrt(Math.Pow(point.X, 2) + Math.Pow(point.Y, 2) + Math.Pow(point.Z, 2));
        }

        #endregion Utility functions from SDK

        #region DisposableBase Implementation

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_kinectSensor != null)
                {
                    _kinectSensor.Close();
                    this._kinectSensor = null;
                }
                if (_bodyFrameSubscription != null) { _bodyFrameSubscription.Dispose(); }
                if (_hdFaceTrackingSubscription != null) { _hdFaceTrackingSubscription.Dispose(); }
                //Bodyreader still needed for reactive can't explicitly dispose
                /*if (_bodyFrameReader != null)
                {
                    _bodyFrameReader.Dispose();
                    _bodyFrameReader = null;
                }
                */
            }
        }

        ~GazeTrackLib()
        {
            Dispose(false);
        }

        #endregion DisposableBase Implementation
    }
}