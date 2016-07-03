using AudioLibrary;
using BodyPositionLibrary;
using Csv;
using GazeTrackLibrary;
using Microsoft.Kinect;
using System;
using System.Linq;

namespace ContextService
{
    /// <summary>
    /// Captures the context of the user with the libraries.
    /// </summary>
    internal class Program
    {
        #region Private Properies

        /// <summary>
        /// Suscriptions for capturing the different context data
        /// </summary>

        private static IDisposable _trackingXYZSubscription;
        private static IDisposable _trackingFaceEngagementaSubscription;
        private static IDisposable _trackingPositionSubscription;
        private static IDisposable _trackingLeanSubscription;
        private static IDisposable _trackingAudioBeamDataSubscription;

        /// <summary>
        /// Libraries that capture the context data
        /// </summary>

        private static GazeTrackLib gazetracklib;
        private static BodyMovementLib bodymovementlib;
        private static AudioBeamLib audiobeamlib;

        /// <summary>
        /// Writers to write the context data to csv files
        /// </summary>
        ///

        private static CsvWriter bodyMovementCsvWriter;
        private static CsvWriter gazeTrackCsvWriter;
        private static CsvWriter bodyLeanCsvWriter;
        private static CsvWriter faceEngagementCsvWriter;
        private static CsvWriter audioBeamDataCsvWriter;

        /// <summary> Active Kinect sensor </summary>
        private static KinectSensor kinectSensor = null;

        private static int WINDOW_WIDTH = 1616;
        private static int WINDOW_HEIGHT = 876;

        #endregion Private Properies

        #region main

        private static void Main(string[] args)
        {
            Create_CsvWriters();
            Create_libraries();
            Subscribe();
            Console.WriteLine("Context capturing started!");
            Console.Read();
            Exit();
        }

        private static void Exit()
        {
            if (kinectSensor != null)
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
            if (audiobeamlib != null)
            {
                audiobeamlib.Dispose();
                audiobeamlib = null;
            }

            WriteCsvFiles();
        }

        #endregion main

        #region Private methods

        /// <summary>
        /// Create all csv files and writers
        /// </summary>
        private static void Create_CsvWriters()
        {
            //string day = DateTime.Now.ToString("ddMMyy_hh_mm_ss");
            string day = "";
            bodyMovementCsvWriter = new CsvWriter("bodymovement" + day + ".csv", new string[] { "trackingId", "HeadpositionX", "HeadPositionY", "HeadPositionZ", "LeftShoulderZ", "RightShoulderZ", });
            gazeTrackCsvWriter = new CsvWriter("gazetrack" + day + ".csv", new string[] { "x", "y" });
            bodyLeanCsvWriter = new CsvWriter("bodylean" + day + ".csv", new string[] { "trackingId", "x", "y" });
            faceEngagementCsvWriter = new CsvWriter("Engagement" + day + ".csv", new string[] { "trackingId", "LookingAway", "Enagegement" });
            audioBeamDataCsvWriter = new CsvWriter("AudioData" + day + ".csv", new string[] { "Energy", "NormalizedEnergy", "BeamAngleInDeg", "BeamAngleConfidence" });
        }

        /// <summary>
        /// Write every data to the csv files
        /// </summary>
        private static void WriteCsvFiles()
        {
            gazeTrackCsvWriter.WriteToFile();
            bodyMovementCsvWriter.WriteToFile();
            bodyLeanCsvWriter.WriteToFile();
            audioBeamDataCsvWriter.WriteToFile();
            faceEngagementCsvWriter.WriteToFile();
        }

        /// <summary>
        /// Create libraries and get Kinect sensor
        /// </summary>
        private static void Create_libraries()
        {
            kinectSensor = KinectSensor.GetDefault();
            kinectSensor.Open();

            gazetracklib = new GazeTrackLib(kinectSensor, WINDOW_WIDTH, WINDOW_HEIGHT);
            audiobeamlib = new AudioBeamLib(kinectSensor);
            bodymovementlib = new BodyMovementLib(kinectSensor);
        }

        /// <summary>
        /// Subscribe to every observable to get all the context data
        /// </summary>
        private static void Subscribe()
        {
            _trackingXYZSubscription = gazetracklib.TrackingXYObservable_Pixels.Subscribe(
               obj =>
               {
                   //    Console.WriteLine("Gaze X= " + obj.X.ToString() + " Y=" + obj.Y.ToString());
                   gazeTrackCsvWriter.WriteRow(new String[] { obj.X.ToString(), obj.Y.ToString() });
               }
               );

            _trackingAudioBeamDataSubscription = audiobeamlib.TrackingObservable_AudioBeamData.Subscribe(
                  list =>
                  {
                      if (list.Any())
                      {
                          list.ForEach(p =>
                          {
                              //Console.WriteLine("AudioBeamData Energy=" + p.Energy + " NormalizedEnergy=" + p.NormalizedEnergy + " BeamAngleInDeg=" + p.BeamAngleInDeg + " BeamAngleConfidence=" + p.BeamAngleConfidence);
                              audioBeamDataCsvWriter.WriteRow(new String[] { p.Energy.ToString(), p.NormalizedEnergy.ToString(), p.BeamAngleInDeg.ToString(), p.BeamAngleConfidence.ToString() });
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
                            //Console.WriteLine("TrackingId=" + p.TrackingId + "HeadPosition x=" + p.HeadX + " y=" + p.HeadY + " z=" + p.HeadZ + "ShoulderPosition right z=" + p.ShoulderRightZ + " left z=" + p.ShoulderLeftZ);
                            bodyMovementCsvWriter.WriteRow(new String[] { p.TrackingId.ToString(), p.HeadX.ToString(), p.HeadY.ToString(), p.HeadZ.ToString(), p.ShoulderLeftZ.ToString(), p.ShoulderRightZ.ToString() });
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
                           //Console.WriteLine("BodyLean TrackingId=" + p.TrackingId + " x=" + p.X + " y=" + p.Y);
                           bodyLeanCsvWriter.WriteRow(new String[] { p.TrackingId.ToString(), p.X.ToString(), p.Y.ToString() });
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
                             // Console.WriteLine("TrackingId=" + p.TrackingId + " LookingAway=" + p.LookingAway + " Engagement=" + p.Engagement);
                             faceEngagementCsvWriter.WriteRow(new String[] { p.TrackingId.ToString(), p.LookingAway, p.Engagement });
                         });
                     }
                 }
                );
        }

        #endregion Private methods
    }
}