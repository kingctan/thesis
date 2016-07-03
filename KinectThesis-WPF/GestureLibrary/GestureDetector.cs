using ArgsLibrary;
using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Gesture Detector class which listens for VisualGestureBuilderFrame events from the service
/// and fires the UpdateGesture event.
/// </summary>
namespace GestureLibrary
{
    internal class GestureDetector
    {
        /// <summary>
        /// Method for handeling the gestures outside the detector
        /// </summary>
        private event EventHandler<UpdateGestureEventArgs> UpdateGesture;

        /// <summary> Path to the gesture database that was trained with VGB </summary>
        private readonly string standardGestureDatabase = @"Database\scrolling.gbd";

        /// <summary> Path to the gesture database that was trained with VGB </summary>
        private readonly string numberGestureDatabase = @"Database\HandUp.gba";

        /// <summary> Name of the standard gestures in the database that we want to track </summary>
        private readonly ArrayList GestureNames = new ArrayList { "scroll_left", "scroll_right", "scroll_up", "scroll_down" };

        /// <summary> Name of the discrete number gestures in the database that we want to track </summary>
        private readonly ArrayList DiscreteNumbersGestureNames = new ArrayList { "HandUp" };

        /// <summary> Gesture frame source which should be tied to a body tracking ID </summary>
        private VisualGestureBuilderFrameSource vgbFrameSource = null;

        /// <summary> Gesture frame reader which will handle gesture events coming from the sensor </summary>
        private VisualGestureBuilderFrameReader vgbFrameReader = null;

        /// <summary>
        /// Initializes a new instance of the GestureDetector class along with the gesture frame source and reader
        /// </summary>
        /// <param name="kinectSensor"></param>
        /// <param name="_updateGesture"></param>
        public GestureDetector(KinectSensor kinectSensor, EventHandler<UpdateGestureEventArgs> _updateGesture)
        {
            this.UpdateGesture = _updateGesture;

            if (kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }

            // create the vgb source. The associated body tracking ID will be set when a valid body frame arrives from the sensor.
            this.vgbFrameSource = new VisualGestureBuilderFrameSource(kinectSensor, 0);
            this.vgbFrameSource.TrackingIdLost += this.Source_TrackingIdLost;

            // open the reader for the vgb frames
            this.vgbFrameReader = this.vgbFrameSource.OpenReader();
            if (this.vgbFrameReader != null)
            {
                this.vgbFrameReader.IsPaused = true;
                this.vgbFrameReader.FrameArrived += Reader_GestureFrameArrived;
            }

            // load the 'Standard' gesture from the gesture database
            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.standardGestureDatabase))
            {
                String text = Assembly.GetEntryAssembly().Location;
                vgbFrameSource.AddGestures(database.AvailableGestures);
            }

            // load the 'Number' gesture from the gesture database
            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.numberGestureDatabase))
            {
                String text = Assembly.GetEntryAssembly().Location;
                vgbFrameSource.AddGestures(database.AvailableGestures);
            }
        }

        /// <summary>
        /// Gets or sets the body tracking ID associated with the current detector
        /// The tracking ID can change whenever a body comes in/out of scope
        /// </summary>
        public ulong TrackingId
        {
            get
            {
                return this.vgbFrameSource.TrackingId;
            }

            set
            {
                if (this.vgbFrameSource.TrackingId != value)
                {
                    this.vgbFrameSource.TrackingId = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the detector is currently paused
        /// If the body tracking ID associated with the detector is not valid, then the detector should be paused
        /// </summary>
        public bool IsPaused
        {
            get
            {
                return this.vgbFrameReader.IsPaused;
            }

            set
            {
                if (this.vgbFrameReader.IsPaused != value)
                {
                    this.vgbFrameReader.IsPaused = value;
                }
            }
        }

        /// <summary>
        /// Disposes all unmanaged resources for the class
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the VisualGestureBuilderFrameSource and VisualGestureBuilderFrameReader objects
        /// </summary>
        /// <param name="disposing">True if Dispose was called directly, false if the GC handles the disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.vgbFrameReader != null)
                {
                    this.vgbFrameReader.FrameArrived -= Reader_GestureFrameArrived;
                    this.vgbFrameReader.Dispose();
                    this.vgbFrameReader = null;
                }

                if (this.vgbFrameSource != null)
                {
                    // this.vgbFrameSource.TrackingIdLost -= Source_TrackingIdLost;
                    this.vgbFrameSource.Dispose();
                    this.vgbFrameSource = null;
                }
            }
        }

        /// <summary>
        /// Handles gesture detection results arriving from the sensor for the associated body tracking Id
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_GestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            VisualGestureBuilderFrameReference frameReference = e.FrameReference;
            using (VisualGestureBuilderFrame frame = frameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    // get the discrete gesture results which arrived with the latest frame
                    IReadOnlyDictionary<Gesture, DiscreteGestureResult> discreteResults = frame.DiscreteGestureResults;
                    IReadOnlyDictionary<Gesture, ContinuousGestureResult> continuousResult = frame.ContinuousGestureResults;
                    if (discreteResults != null)
                    {
                        // we only have one gesture in this source object, but you can get multiple gestures
                        foreach (Gesture gesture in this.vgbFrameSource.Gestures)
                        {
                            if (DiscreteNumbersGestureNames.Contains(gesture.Name) && gesture.GestureType == GestureType.Discrete)
                            {
                                DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);

                                if (result != null)
                                {
                                    //updates with new eventhandler
                                    if (result.Detected)
                                    {
                                        UpdateGesture(this, new UpdateGestureEventArgs { TrackingId = TrackingId, Sender = sender, GestureName = gesture.Name, Confidence = result.Confidence });
                                        Console.WriteLine("discrete " + gesture.Name + " " + result.Confidence);
                                    }
                                }
                            }
                        }

                        foreach (Gesture gesture in this.vgbFrameSource.Gestures)
                        {
                            if (GestureNames.Contains(gesture.Name) && gesture.GestureType == GestureType.Discrete)
                            {
                                DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);

                                if (result != null)
                                {
                                    if (result.Detected && result.Confidence > 0.5)
                                    {
                                        UpdateGesture(this, new UpdateGestureEventArgs { TrackingId = TrackingId, Sender = sender, GestureName = gesture.Name, Confidence = result.Confidence });
                                        Console.WriteLine("Discrete " + gesture.Name + " " + result.Confidence);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the TrackingIdLost event for the VisualGestureBuilderSource object
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Source_TrackingIdLost(object sender, TrackingIdLostEventArgs e)
        {
            //Wanneer er geen lichaam meer wordt getracked
            // update the GestureResultView object to show the 'Not Tracked' image in the UI
            // this.GestureResultView.UpdateGestureResult(false, false, 0.0f);
        }
    }
}