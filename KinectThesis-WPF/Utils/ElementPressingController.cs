using Microsoft.Kinect.Input;
using Microsoft.Kinect.Toolkit.Input;
using Microsoft.Kinect.Wpf.Controls;
using System;
using System.Windows;

namespace Utils
{
    public class ElementPressingController : IKinectPressableController
    {
        private PressableModel pressableInputModel;
        private KinectRegion kinectRegion;
        private bool disposedValue;
        private ElementPressing videoPlayerElement;

        public ElementPressingController(IInputModel inputModel, KinectRegion kinectRegion, EventHandler<KinectTappedEventArgs> OnTapped)
        {
            this.pressableInputModel = inputModel as PressableModel;
            this.kinectRegion = kinectRegion;
            this.videoPlayerElement = inputModel.Element as ElementPressing;
            this.pressableInputModel.Tapped += OnTapped;
        }

        public FrameworkElement Element
        {
            get
            {
                return pressableInputModel.Element as FrameworkElement;
            }
        }

        public PressableModel PressableInputModel
        {
            get
            {
                return pressableInputModel;
            }
        }

        public void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                kinectRegion = null;
                pressableInputModel = null;
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