using Microsoft.Kinect.Input;
using Microsoft.Kinect.Toolkit.Input;
using Microsoft.Kinect.Wpf.Controls;
using System;
using System.Windows;

namespace Utils
{
    public class ElementManipulationController : IKinectManipulatableController
    {
        private ManipulatableModel manipulatableInputModel;
        private KinectRegion kinectRegion;
        private ElementManipulation videoPlayerElement;
        private bool disposedValue;

        public ElementManipulationController(IInputModel inputModel, KinectRegion kinectRegion, EventHandler<KinectManipulationUpdatedEventArgs> OnManipulationUpdated)
        {
            this.manipulatableInputModel = inputModel as ManipulatableModel;
            this.kinectRegion = kinectRegion;
            this.videoPlayerElement = inputModel.Element as ElementManipulation;
            this.manipulatableInputModel.ManipulationUpdated += OnManipulationUpdated;
        }

        public FrameworkElement Element
        {
            get
            {
                return manipulatableInputModel.Element as FrameworkElement;
            }
        }

        public ManipulatableModel ManipulatableInputModel
        {
            get
            {
                return manipulatableInputModel;
            }
        }

        public void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                kinectRegion = null;
                manipulatableInputModel = null;
                videoPlayerElement = null;
                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }
    }
}