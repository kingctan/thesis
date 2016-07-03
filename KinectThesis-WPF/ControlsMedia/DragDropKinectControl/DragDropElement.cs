using Microsoft.Kinect.Toolkit.Input;
using Microsoft.Kinect.Wpf.Controls;
using System.Windows.Controls;

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    internal class DragDropElement : Decorator, IKinectControl
    {
        #region Public properties

        /// <summary>
        /// Decides if control receives manipulation gestures
        /// </summary>
        public bool IsManipulatable
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Decides if control receives pressable gestures
        /// </summary>
        public bool IsPressable
        {
            get
            {
                return false;
            }
        }

        public double X
        {
            get;
            set;
        }

        public double Y
        {
            get;
            set;
        }

        #endregion Public properties

        /// <summary>
        /// Creates in the background the controller for the customised control.
        /// Called when the KinectRegion detects the custom control is within the XAML that it is currently displaying
        /// </summary>
        /// <param name="inputModel">Holds a reference to the custom control itself so that we can manipulate it using the controller.</param>
        /// <param name="kinectRegion"></param>
        /// <returns></returns>
        public IKinectController CreateController(IInputModel inputModel, KinectRegion kinectRegion)
        {
            return new DragDropElementController(inputModel, kinectRegion);
        }
    }
}