using Csv;
using System;
using System.Diagnostics;
using System.IO;

namespace DatasetPlayer
{
    /// <summary>
    /// Depricated, was used to test the number gestures of the gesture library.
    /// </summary>
    internal class Program
    {
        #region Private Properties

        /// <summary>
        /// The local directory for the test files
        /// </summary>
        private static readonly string datasetDirectory = @"C:\Users\Miguel\Repository";

        /// <summary>
        /// The process for the kinect studio commandline tool KSUtil.exe
        /// </summary>
        private static Process myProcess;

        /// <summary>
        /// The currently activated gesture database who will be tested against the video files
        /// </summary>
        private static string activeGestureDatabase;

        private static CsvWriter testWriter;

        /// <summary>
        /// The amount of processed test videos
        /// </summary>
        private static int countProcess = 0;

        /// <summary>
        /// The amount of tests
        /// </summary>
        private static int filesToProcess = 20;

        #endregion Private Properties

        private static void Main(string[] args)
        {
            try
            {
                activeGestureDatabase = args[0];
                testWriter = new CsvWriter(args[1], 1);
                ProcessDirectory(datasetDirectory);
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("This project was used for testing and is depricated.");
            }
        }

        // Process all files in the directory passed in, recurse on any directories
        // that are found, and process the files they contain.
        private static void ProcessDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory, "*.xef");

            foreach (string fileName in fileEntries)
            {
                if (countProcess == filesToProcess)
                {
                    break;
                }

                ProcessFile(fileName);
                //Write the true labels of the predictions
                if (activeGestureDatabase.Equals("Test all"))
                {//test the gestures in a gesture database who can recognise all number gestures
                    //gets the name/number where the clip is in
                    string res = Translate(targetDirectory.Substring(targetDirectory.Length - 1));
                    testWriter.WriteRow("");
                    testWriter.WriteRow(res);
                    testWriter.WriteRow("----");
                    testWriter.WriteToFile();
                }
                else if (targetDirectory.Contains(activeGestureDatabase))
                {
                    testWriter.WriteRow("");
                    testWriter.WriteRow("1");
                    testWriter.WriteRow("----");
                    testWriter.WriteToFile();
                }
                else
                {
                    testWriter.WriteRow("");
                    testWriter.WriteRow("0");
                    testWriter.WriteRow("----");
                    testWriter.WriteToFile();
                }
            }

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }

        /// <summary>
        /// Video clip will be played
        /// </summary>
        /// <param name="path"></param>
        private static void ProcessFile(string path)
        {
            //Kinect Studio Utility Tool
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "KSUtil.exe";
            startInfo.Arguments = "-play \"" + path + "\"";
            myProcess = Process.Start(startInfo);
            myProcess.WaitForExit();
            Console.WriteLine("Im done playing");
            countProcess++;
        }

        private static string Translate(string nr)
        {
            switch (nr)
            {
                case "1":
                    return "one";

                case "2":
                    return "two";

                case "3":
                    return "three";

                default:
                    return "";
            }
        }
    }
}