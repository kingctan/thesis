using Microsoft.Kinect.Input;
using Microsoft.Kinect.Toolkit.Input;
using Microsoft.Kinect.Wpf.Controls;
using System;
using System.Windows.Controls;

namespace Utils
{
    public class ElementManipulation : Decorator, IKinectControl
    {
        private EventHandler<KinectManipulationUpdatedEventArgs> onManipulationUpdated;

        public bool IsManipulatable
        {
            get
            {
                return true;
            }
        }

        public bool IsPressable
        {
            get
            {
                return false;
            }
        }

        public EventHandler<KinectManipulationUpdatedEventArgs> OnManipulationUpdated
        {
            get
            {
                return onManipulationUpdated;
            }

            set
            {
                onManipulationUpdated = value;
            }
        }

        public IKinectController CreateController(IInputModel inputModel, KinectRegion kinectRegion)
        {
            return new ElementManipulationController(inputModel, kinectRegion, this.onManipulationUpdated);
        }
    }
}