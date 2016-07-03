using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Utils;

namespace BodyPositionLibrary
{
    /// <summary>
    /// Library that captures all the data of the body.
    /// </summary>
    public class BodyMovementLib : DisposableBase
    {
        #region Private Properties

        private KinectSensor _kinectSensor;
        private ulong CurrentTrackingId { get; set; }
        private IObservable<BodyFrame> _bodyFrameObservable;
        private IObservable<FaceFrame>[] faceTrackingObservables;
        private Body[] _bodies = null;
        private BodyFrameReader _bodyFrameReader = null;

        /// <summary>
        /// Maximum amount of bodies that can be tracked
        /// </summary>
        private int bodyCount = 6;

        /// <summary>
        /// Face frame sources
        /// </summary>
        private FaceFrameSource[] faceFrameSources = null;

        /// <summary>
        /// Face frame readers
        /// </summary>
        private FaceFrameReader[] faceFrameReaders = null;

        /// <summary>
        /// Storage for face frame results
        /// </summary>
        private FaceFrameResult[] faceFrameResults = null;

        #endregion Private Properties

        #region Public Properties

        public IObservable<List<BodyPosition>> TrackingObservable_Position { get; private set; }
        public IObservable<List<BodyLean>> TrackingObservable_Lean { get; private set; }
        public IObservable<List<FaceEngagement>> TrackingObservable_FaceEngagement { get; private set; }

        #endregion Public Properties

        #region Ctor

        public BodyMovementLib(KinectSensor kinectSensor)
        {
            if (kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }
            _kinectSensor = kinectSensor;
            CreateBodyFrameObservable();
            CreatePositionObservable();
            CreateLeanObservable();
            CreateFaceObservable();
            SubscribeToFaceTrackingObservables();
            CreateFaceEngagementObservable();
        }

        #endregion Ctor

        #region Private methods

        /// <summary>
        /// Returns the index of the face frame source
        /// </summary>
        /// <param name="faceFrameSource">the face frame source</param>
        /// <returns>the index of the face source in the face source array</returns>
        private int GetFaceSourceIndex(FaceFrameSource faceFrameSource)
        {
            int index = -1;

            for (int i = 0; i < this.bodyCount; i++)
            {
                if (this.faceFrameSources[i] == faceFrameSource)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        /// <summary>
        /// Subscribe to every bodies facetrackig observable
        /// </summary>
        private void SubscribeToFaceTrackingObservables()
        {
            foreach (IObservable<FaceFrame> faceTrackingObersvable in faceTrackingObservables)
            {
                faceTrackingObersvable.Subscribe();
            }
        }

        #endregion Private methods

        #region observable creation

        /// <summary>
        /// BodyFrameArrived event that updates the data of the bodies.
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
                            // set the maximum number of bodies that would be tracked by Kinect
                            this.bodyCount = bodyFrame.BodyFrameSource.BodyCount;
                            // initialise to number of bodies supported by the Kinect
                            _bodies = new Body[this.bodyCount];
                        }
                        else
                        {
                            bodyFrame.GetAndRefreshBodyData(_bodies);
                        }
                        bodyFrame.Dispose();
                    }
                    return bodyFrame;
                });
        }

        /// <summary>
        /// Calculate the bodypositions with the bodyframeobservable
        /// </summary>
        private void CreatePositionObservable()
        {
            TrackingObservable_Position = _bodyFrameObservable.Select(e =>
            {
                var list = new List<BodyPosition>();

                if (_bodies != null)
                {
                    foreach (var body in _bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                Joint head = body.Joints[JointType.Head];
                                Joint shoulderLeft = body.Joints[JointType.ShoulderLeft];
                                Joint shoulderRight = body.Joints[JointType.ShoulderRight];
                                list.Add(new BodyPosition { TrackingId = body.TrackingId, HeadX = head.Position.X, HeadY = head.Position.Y, HeadZ = head.Position.Z, ShoulderLeftZ = shoulderLeft.Position.Z, ShoulderRightZ = shoulderRight.Position.Z });
                            }
                        }
                    }
                }
                return list;
            });
        }

        /// <summary>
        /// Calculate the body lean from the bodyframeobservable
        /// </summary>
        private void CreateLeanObservable()
        {
            TrackingObservable_Lean = _bodyFrameObservable.Select(e =>
            {
                var list = new List<BodyLean>();

                if (_bodies != null)
                {
                    foreach (var body in _bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked && body.LeanTrackingState == TrackingState.Tracked)
                            {
                                PointF leanAmount = body.Lean;
                                list.Add(new BodyLean { TrackingId = body.TrackingId, X = leanAmount.X, Y = leanAmount.Y });
                            }
                        }
                    }
                }
                return list;
            });
        }

        /// <summary>
        /// Create the FaceEngagementObservable from bodyobservable
        /// </summary>
        private void CreateFaceEngagementObservable()
        {
            TrackingObservable_FaceEngagement = _bodyFrameObservable.Select(e =>
            {
                var list = new List<FaceEngagement>();
                // iterate through each face source
                for (int i = 0; i < this.bodyCount; i++)
                {
                    // check if a valid face is tracked in this face source
                    if (this.faceFrameSources[i].IsTrackingIdValid)
                    {
                        // check if we have valid face frame results
                        if (this.faceFrameResults[i] != null)
                        {
                            list.Add(new FaceEngagement { TrackingId = faceFrameSources[i].TrackingId, LookingAway = faceFrameResults[i].FaceProperties[FaceProperty.LookingAway].ToString(), Engagement = faceFrameResults[i].FaceProperties[FaceProperty.Engaged].ToString() });
                        };
                    }
                    else
                    {
                        // check if the corresponding body is tracked
                        if (this._bodies[i].IsTracked)
                        {
                            // update the face frame source to track this body
                            this.faceFrameSources[i].TrackingId = this._bodies[i].TrackingId;
                        }
                    }
                }
                return list;
            });
        }

        /// <summary>
        /// Create faceframeResults to get information of the face and write it later
        /// </summary>
        private void CreateFaceObservable()
        {
            // create a face frame source + reader to track each face in the FOV
            this.faceFrameSources = new FaceFrameSource[this.bodyCount];
            this.faceFrameReaders = new FaceFrameReader[this.bodyCount];
            this.faceFrameResults = new FaceFrameResult[this.bodyCount];
            this.faceTrackingObservables = new IObservable<FaceFrame>[this.bodyCount];
            for (int i = 0; i < this.bodyCount; i++)
            {
                // create the face frame source with the required face frame features and an initial tracking Id of 0
                this.faceFrameSources[i] = new FaceFrameSource(_kinectSensor, 0, FaceFrameFeatures.LookingAway | FaceFrameFeatures.FaceEngagement);

                // open the corresponding reader
                this.faceFrameReaders[i] = this.faceFrameSources[i].OpenReader();
                // allocate storage to store face frame results for each face in the FOV

                faceTrackingObservables[i] = Observable.FromEventPattern<FaceFrameArrivedEventArgs>(this.faceFrameReaders[i], "FrameArrived")
               .Select(frame => frame.EventArgs.FrameReference.AcquireFrame())
               .Where(frame => frame != null)
               .Select(frame =>
               {
                   if (frame != null)
                   {
                       // get the index of the face source from the face source array
                       int index = this.GetFaceSourceIndex(frame.FaceFrameSource);
                       // store this face frame result to write later
                       this.faceFrameResults[index] = frame.FaceFrameResult;
                       frame.Dispose();
                   }

                   return frame;
               });
            }
        }

        #endregion observable creation

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

                if (this.faceFrameReaders != null)
                {
                    foreach (FaceFrameReader faceFrameReader in this.faceFrameReaders)
                    {
                        faceFrameReader.Dispose();
                    }
                    this.faceFrameReaders = null;
                }

                if (this._bodyFrameReader != null)
                {
                    this._bodyFrameReader.Dispose();
                    this._bodyFrameReader = null;
                }

                if (this.faceFrameSources != null)
                {
                    foreach (FaceFrameSource faceFrameSource in this.faceFrameSources)

                    {
                        faceFrameSource.Dispose();
                    }

                    this.faceFrameSources = null;
                }
            }
        }

        ~BodyMovementLib()
        {
            Dispose(false);
        }

        #endregion DisposableBase Implementation
    }
}