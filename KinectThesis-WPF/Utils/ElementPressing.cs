using Microsoft.Kinect.Input;
using Microsoft.Kinect.Toolkit.Input;
using Microsoft.Kinect.Wpf.Controls;
using System;
using System.Windows.Controls;

namespace Utils
{
    public class ElementPressing : Decorator, IKinectControl
    {
        private EventHandler<KinectTappedEventArgs> onTapped;

        public bool IsManipulatable
        {
            get
            {
                return false;
            }
        }

        public bool IsPressable
        {
            get
            {
                return true;
            }
        }

        public EventHandler<KinectTappedEventArgs> OnTapped
        {
            get
            {
                return onTapped;
            }

            set
            {
                onTapped = value;
            }
        }

        public IKinectController CreateController(IInputModel inputModel, KinectRegion kinectRegion)
        {
            return new ElementPressingController(inputModel, kinectRegion, OnTapped);
        }
    }
}