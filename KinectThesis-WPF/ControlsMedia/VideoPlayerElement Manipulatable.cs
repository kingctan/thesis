using Microsoft.Kinect.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Kinect.Toolkit.Input;
using Microsoft.Kinect.Input;

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    class VideoPlayerElement : Decorator, IKinectControl
    {
        EventHandler<KinectManipulationUpdatedEventArgs> onManipulationUpdated;
        EventHandler<KinectPressingUpdatedEventArgs> onPressingUpdated;
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
                return true;
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

        public EventHandler<KinectPressingUpdatedEventArgs> OnPressingUpdated
        {
            get
            {
                return onPressingUpdated;
            }

            set
            {
                onPressingUpdated = value;
            }
        }

        public IKinectController CreateController(IInputModel inputModel, KinectRegion kinectRegion)
        {
            return new VideoPlayerElementController(inputModel, kinectRegion, this.onManipulationUpdated, this.onPressingUpdated);
        }
    }
}
