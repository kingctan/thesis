namespace Microsoft.Samples.Kinect.ControlsBasics
{
    using Microsoft.Kinect.Wpf.Controls;
    using System;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        internal KinectRegion KinectRegion { get; set; }

        /// <summary>
        /// Logs unhandled exceptions
        /// </summary>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.ToString(), "Main: Unhandled exception, shutting down... :(");
        }

        /// <summary>
        /// Displays stack trace on unhandled exceptions
        /// </summary>
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString(), "Main: Unhandled exception, shutting down... :(");
        }
    }
}