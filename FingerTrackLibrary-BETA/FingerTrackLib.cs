using Metrilus.Aiolos.Core;
using Metrilus.Aiolos.Kinect;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Threading;
using Utils;

namespace FingerTrackLibrary
{
    public class FingerTrackLib : DisposableBase
    {
        private KinectSensor kinectSensor;
        private int width;
        private int height;
        private MultiSourceFrameReader reader;
        private ushort[] depthFrameData;
        private ushort[] irFrameData;
        private Body[] bodies;
        private CoordinateMapper coordinateMapper;
        private object updateLock = new object();
        private KinectEngine engine;
        private List<List<string>> rows;
        private MultiSourceFrameReference frameReference;
        private static AutoResetEvent dataAvailable = new AutoResetEvent(false);
        private Boolean exitedBeforeWait;
        private int totalHands;
        private int[] fingers = new int[5];
        private const int handsNeeded = 2;

        public FingerTrackLib(KinectSensor _kinectSensor)
        {
            if (_kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }
            this.kinectSensor = _kinectSensor;
            engine = new KinectEngine();
            rows = new List<List<string>>();
            FrameDescription frameDescription = kinectSensor.DepthFrameSource.FrameDescription;
            width = frameDescription.Width;
            height = frameDescription.Height;
            // allocate space to put the pixels being received and converted
            depthFrameData = new ushort[frameDescription.Width * frameDescription.Height];
            irFrameData = new ushort[frameDescription.Width * frameDescription.Height];
            coordinateMapper = kinectSensor.CoordinateMapper;
        }

        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            lock (updateLock)
            {
                frameReference = e.FrameReference;
            }
            dataAvailable.Set();
        }

        private void Update()
        {
            //Wait until data is available for amount of time
            dataAvailable.WaitOne(3000, exitedBeforeWait);
            if (!exitedBeforeWait)
            {
                MultiSourceFrame multiSourceFrame = null;
                DepthFrame depthFrame = null;
                InfraredFrame irFrame = null;
                BodyFrame bodyFrame = null;
                lock (updateLock)
                {
                    try
                    {
                        if (frameReference != null)
                        {
                            multiSourceFrame = frameReference.AcquireFrame();

                            if (multiSourceFrame != null)
                            {
                                DepthFrameReference depthFrameReference = multiSourceFrame.DepthFrameReference;
                                InfraredFrameReference irFrameReference = multiSourceFrame.InfraredFrameReference;
                                BodyFrameReference bodyFrameReference = multiSourceFrame.BodyFrameReference;
                                depthFrame = depthFrameReference.AcquireFrame();
                                irFrame = irFrameReference.AcquireFrame();

                                if ((depthFrame != null) && (irFrame != null))
                                {
                                    FrameDescription depthFrameDescription = depthFrame.FrameDescription;
                                    FrameDescription irFrameDescription = irFrame.FrameDescription;

                                    int depthWidth = depthFrameDescription.Width;
                                    int depthHeight = depthFrameDescription.Height;
                                    int irWidth = irFrameDescription.Width;
                                    int irHeight = irFrameDescription.Height;

                                    // verify data and write the new registered frame data to the display bitmap
                                    if (((depthWidth * depthHeight) == depthFrameData.Length) &&
                                        ((irWidth * irHeight) == irFrameData.Length))
                                    {
                                        depthFrame.CopyFrameDataToArray(depthFrameData);
                                        irFrame.CopyFrameDataToArray(irFrameData);
                                    }

                                    if (bodyFrameReference != null)
                                    {
                                        bodyFrame = bodyFrameReference.AcquireFrame();

                                        if (bodyFrame != null)
                                        {
                                            if (bodies == null || bodies.Length < bodyFrame.BodyCount)
                                            {
                                                bodies = new Body[bodyFrame.BodyCount];
                                            }
                                            using (bodyFrame)
                                            {
                                                bodyFrame.GetAndRefreshBodyData(bodies);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //  reader.MultiSourceFrameArrived -= Reader_MultiSourceFrameArrived;
                                    //  reader.Dispose();
                                    Console.WriteLine("No IR/DEPTH frame detected");
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignore if the frame is no longer available
                        Console.WriteLine("Frame not available");
                    }
                    finally
                    {
                        if (depthFrame != null)
                        {
                            depthFrame.Dispose();
                            depthFrame = null;
                        }

                        if (irFrame != null)
                        {
                            irFrame.Dispose();
                            irFrame = null;
                        }
                        if (bodyFrame != null)
                        {
                            bodyFrame.Dispose();
                            bodyFrame = null;
                        }
                        if (multiSourceFrame != null)
                        {
                            multiSourceFrame = null;
                        }
                    }
                }
            }
        }

        public void RequestStop()
        {
            stop = true;
        }

        private List<int> result;

        // Volatile is used as hint to the compiler that this data
        // member will be accessed by multiple threads.
        private volatile bool stop;

        public List<int> CountFingers()
        {
            result = new List<int>();
            // open the reader for the depth frames
            reader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
            if (reader != null)
            {
                reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }

            //stopCouting = !stopCouting;
            fingers = new int[5];
            Console.WriteLine("Started fingertracking");
            try
            {
                //bool noHandsFound = true;
                //    while (!stopCouting && !madeEstimation && noHandsFoundCount <= 10)
                stop = false;
                while (!stop)
                {
                    Update();
                    //noHandsFound = true;
                    if (!exitedBeforeWait)
                    {
                        KinectHand[] hands = engine.DetectFingerJoints(width, height, irFrameData, depthFrameData, bodies, coordinateMapper);

                        for (int i = 0; i < hands.Length; i++)
                        {
                            if (hands[i] == null)
                                continue;
                            // Print the finger data.
                            PrintHand(hands[i]);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No Frame detected");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("CountFingers exception:" + ex.Message);
            }

            if (reader != null)
            {
                reader.MultiSourceFrameArrived -= Reader_MultiSourceFrameArrived;
                reader.Dispose();
                reader = null;
            }

            return result;
        }

        //thumb,index,middle,ring,pinky
        /// <summary>
        /// Print the hand to the console.
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="handNumber"></param>
        /// <param name="outputPos"></param>
        private void PrintHand(KinectHand hand)
        {
            for (int fingerIdx = 0; fingerIdx < 5; fingerIdx++)
            {
                try
                {
                    Array f = Enum.GetValues(typeof(Hand.FingerJointType));
                    Array fNames = Enum.GetNames(typeof(Hand.FingerJointType));
                    int idxInEnum = fingerIdx * 3;
                    Microsoft.Kinect.DepthSpacePoint[] p = new Microsoft.Kinect.DepthSpacePoint[3];
                    string[] jointNames = new string[3];
                    for (int j = 0; j < 3; j++)
                    {
                        Hand.FingerJointType jt = (Hand.FingerJointType)f.GetValue(idxInEnum + j);
                        p[j] = hand.FingerJoints[jt];
                        jointNames[j] = (string)fNames.GetValue(idxInEnum + j);
                        //   Console.WriteLine("Good position");
                        if (jt.ToString().Contains("Pinky"))
                        {
                            fingers[4] |= 1;
                        }
                        else if (jt.ToString().Contains("Thumb"))
                        {
                            fingers[0] |= 1;
                        }
                        else if (jt.ToString().Contains("Ring"))
                        {
                            fingers[3] |= 1;
                        }
                        else if (jt.ToString().Contains("Index"))
                        {
                            fingers[1] |= 1;
                        }
                        else
                        {
                            fingers[2] |= 1;
                        }
                    }
                }
                catch (Exception)
                {
                    //Finger not found
                }
            }

            if (totalHands == handsNeeded)
            {
                totalHands = 0;
                int fingerCount = 0;
                for (int i = 0; i < fingers.Length; i++)
                {
                    if (fingers[i] != 0)
                    {
                        fingerCount++;
                    }
                }

                result.Add(fingerCount);
            }

            totalHands++;
        }

        #region DisposableBase Implementation

        protected override void Dispose(bool disposing)
        {
            if (kinectSensor != null)
            {
                //Don't close the kinect needed throughout the application
                kinectSensor.Close();
                this.kinectSensor = null;
            }
            if (this.reader != null)
            {
                this.reader.MultiSourceFrameArrived -= this.Reader_MultiSourceFrameArrived;
                this.reader.Dispose();
                this.reader = null;
            }
        }

        ~FingerTrackLib()
        {
            Dispose(false);
        }

        #endregion DisposableBase Implementation
    }
}