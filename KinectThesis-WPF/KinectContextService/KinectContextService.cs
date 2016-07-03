using AudioLibrary;
using BodyPositionLibrary;
using Csv;
using GazeTrackLibrary;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace KinectContextService
{
    public partial class KinectContextService : ServiceBase
    {
        #region Private Properies

        /// <summary>
        /// Suscriptions for capturing the different context data
        /// </summary>

        private IDisposable _trackingXYZSubscription;
        private IDisposable _trackingFaceEngagementaSubscription;
        private IDisposable _trackingPositionSubscription;
        private IDisposable _trackingLeanSubscription;
        private IDisposable _trackingAudioBeamDataSubscription;

        /// <summary>
        /// Libraries that capture the context data
        /// </summary>

        private GazeTrackLib gazetracklib;
        private BodyMovementLib bodymovementlib;
        private AudioBeamLib audiobeamlib;

        /// <summary>
        /// Writing the context data to csv files
        /// </summary>
        ///
        private CsvWriter bodyMovementCsvWriter;

        private CsvWriter gazeTrackCsvWriter;
        private CsvWriter bodyLeanCsvWriter;
        private CsvWriter faceEngagementCsvWriter;
        private CsvWriter audioBeamDataCsvWriter;

        /// <summary> Active Kinect sensor </summary>
        private KinectSensor kinectSensor = null;

        private int WINDOW_WIDTH = 1616;
        private int WINDOW_HEIGHT = 876;
        private bool WRITE_TO_CSV;

        #endregion Private Properies

        public KinectContextService()
        {
            InitializeComponent();
            Create_CsvWriters();
            // Create_libraries();
        }

        protected override void OnStart(string[] args)
        {
            // Subscribe();
        }

        protected override void OnStop()
        {
            /*   if (kinectSensor != null)
               {
                   kinectSensor.Close();
                   kinectSensor = null;
               }
               if (_trackingXYZSubscription != null) { _trackingXYZSubscription.Dispose(); }
               if (_trackingFaceEngagementaSubscription != null) { _trackingFaceEngagementaSubscription.Dispose(); }
               if (_trackingAudioBeamDataSubscription != null) { _trackingAudioBeamDataSubscription.Dispose(); }
               if (_trackingLeanSubscription != null) { _trackingLeanSubscription.Dispose(); }
               if (_trackingPositionSubscription != null) { _trackingPositionSubscription.Dispose(); }
               if (gazetracklib != null)
               {
                   gazetracklib.Dispose();
                   gazetracklib = null;
               }
               if (bodymovementlib != null)
               {
                   bodymovementlib.Dispose();
                   bodymovementlib = null;
               }
                  */
            WriteCsvFiles();
        }

        /// <summary>
        /// Create all csv files and writers
        /// </summary>
        private void Create_CsvWriters()
        {
            string day = DateTime.Now.ToString("ddMMyy_hh_mm_ss");
            bodyMovementCsvWriter = new CsvWriter("bodymovement" + day + ".csv", new string[] { "trackingId", "HeadpositionX", "HeadPositionY", "HeadPositionZ", "RightShoulderZ", "LeftShoulderZ" });
            gazeTrackCsvWriter = new CsvWriter("gazetrack" + day + ".csv", new string[] { "trackingId", "HeadpositionX", "HeadPositionY", "HeadPositionZ", "RightShoulderZ", "LeftShoulderZ" });
            gazeTrackCsvWriter = new CsvWriter("gazetrack" + day + ".csv", new string[] { "trackingId", "HeadpositionX", "HeadPositionY", "HeadPositionZ", "RightShoulderZ", "LeftShoulderZ" });
            gazeTrackCsvWriter = new CsvWriter("gazetrack" + day + ".csv", new string[] { "x", "y" });
            bodyLeanCsvWriter = new CsvWriter("bodylean" + day + ".csv", new string[] { "trackingId", "x", "y" });
            faceEngagementCsvWriter = new CsvWriter("Engagement" + day + ".csv", new string[] { "trackingId", "LookingAway", "Enagegement" });
            audioBeamDataCsvWriter = new CsvWriter("help.csv", new string[] { "Energy", "NormalizedEnergy", "BeamAngleInDeg", "BeamAngleConfidence" });
        }

        /// <summary>
        /// Write every data to the csv files
        /// </summary>
        private void WriteCsvFiles()
        {
            gazeTrackCsvWriter.writeToFile();
            bodyMovementCsvWriter.writeToFile();
            bodyLeanCsvWriter.writeToFile();
            audioBeamDataCsvWriter.writeToFile();
            faceEngagementCsvWriter.writeToFile();
        }

        /// <summary>
        /// Create libraries and get Kinect sensor
        /// </summary>
        private void Create_libraries()
        {
            kinectSensor = KinectSensor.GetDefault();
            kinectSensor.Open();

            gazetracklib = new GazeTrackLib(kinectSensor, WINDOW_WIDTH, WINDOW_HEIGHT);
            audiobeamlib = new AudioBeamLib(kinectSensor);
            bodymovementlib = new BodyMovementLib(kinectSensor);
        }

        /// <summary>
        /// Subscribe to every observable to get all the contect data
        /// </summary>
        private void Subscribe()
        {
            _trackingXYZSubscription = gazetracklib.TrackingXYObservable_Pixels.Subscribe(
               obj =>
               {
                   //    Console.WriteLine("Gaze X= " + obj.X.ToString() + " Y=" + obj.Y.ToString());
                   if (WRITE_TO_CSV)
                   {
                       gazeTrackCsvWriter.writeRow(new String[] { obj.X.ToString(), obj.Y.ToString() });
                   }
               }
               );

            _trackingAudioBeamDataSubscription = audiobeamlib.TrackingObservable_AudioBeamData.Subscribe(
                  list =>
                  {
                      if (list.Any())
                      {
                          list.ForEach(p =>
                          {
                              //  Console.WriteLine("AudioBeamData Energy=" + p.Energy + " NormalizedEnergy=" + p.NormalizedEnergy + " BeamAngleInDeg=" + p.BeamAngleInDeg + " BeamAngleConfidence=" + p.BeamAngleConfidence);
                              if (WRITE_TO_CSV)
                              {
                                  audioBeamDataCsvWriter.writeRow(new String[] { p.Energy.ToString(), p.NormalizedEnergy.ToString(), p.BeamAngleInDeg.ToString(), p.BeamAngleConfidence.ToString() });
                              }
                          });
                      }
                  }
                 );

            _trackingPositionSubscription = bodymovementlib.TrackingObservable_Position.Subscribe(
                list =>
                {
                    if (list.Any())
                    {
                        list.ForEach(p =>
                        {
                            //  Console.WriteLine("TrackingId=" + p.TrackingId + "HeadPosition x=" + p.HeadX + " y=" + p.HeadY + " z=" + p.HeadZ + "ShoulderPosition right z=" + p.ShoulderRightZ + " left z=" + p.ShoulderLeftZ);
                            if (WRITE_TO_CSV)
                            {
                                bodyMovementCsvWriter.writeRow(new String[] { p.TrackingId.ToString(), p.HeadX.ToString(), p.HeadY.ToString(), p.HeadZ.ToString(), p.ShoulderLeftZ.ToString(), p.ShoulderRightZ.ToString() });
                            }
                        });
                    }
                }
                );

            _trackingLeanSubscription = bodymovementlib.TrackingObservable_Lean.Subscribe(
               list =>
               {
                   if (list.Any())
                   {
                       list.ForEach(p =>
                       {
                           // Console.WriteLine("BodyLean TrackingId=" + p.TrackingId + " x=" + p.X + " y=" + p.Y);
                           if (WRITE_TO_CSV)
                           {
                               bodyLeanCsvWriter.writeRow(new String[] { p.TrackingId.ToString(), p.X.ToString(), p.Y.ToString() });
                           }
                       });
                   }
               }
               );

            _trackingFaceEngagementaSubscription = bodymovementlib.TrackingObservable_FaceEngagement.Subscribe(
                 list =>
                 {
                     if (list.Any())
                     {
                         list.ForEach(p =>
                         {
                             //  Console.WriteLine("TrackingId=" + p.TrackingId + " LookingAway=" + p.LookingAway + " Engagement=" + p.Engagement);
                             if (WRITE_TO_CSV)
                             {
                                 faceEngagementCsvWriter.writeRow(new String[] { p.TrackingId.ToString(), p.LookingAway, p.Engagement });
                             }
                         });
                     }
                 }
                );
        }
    }
}