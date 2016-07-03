using Microsoft.Kinect.Input;
using Microsoft.Kinect.Toolkit.Input;
using Microsoft.Kinect.Wpf.Controls;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    internal class DragDropElementController : IKinectManipulatableController
    {
        #region Private properties

        private ManipulatableModel inputModel;
        private KinectRegion kinectRegion;
        private DragDropElement dragDropElement;

        #endregion Private properties

        #region Ctor

        public DragDropElementController(IInputModel inputModel, KinectRegion kinectRegion)
        {
            this.inputModel = inputModel as ManipulatableModel;
            this.kinectRegion = kinectRegion;
            this.dragDropElement = inputModel.Element as DragDropElement;
            this.inputModel.ManipulationUpdated += OnManipulationUpdated;
        }

        public FrameworkElement Element
        {
            get
            {
                return inputModel.Element as FrameworkElement;
            }
        }

        public ManipulatableModel ManipulatableInputModel
        {
            get
            {
                return inputModel;
            }
        }

        #endregion Ctor

        #region Gesture method

        /// <summary>
        /// Dragging with hand pointer event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnManipulationUpdated(object sender, KinectManipulationUpdatedEventArgs e)
        {
            Canvas parent = dragDropElement.Parent as Canvas;
            if (parent != null)
            {
                var d = e.Delta.Translation;
                var y = Canvas.GetTop(dragDropElement);
                var x = Canvas.GetLeft(dragDropElement);
                var width = dragDropElement.ActualWidth;
                var height = dragDropElement.ActualHeight;
                if (double.IsNaN(y)) y = 0;
                if (double.IsNaN(x)) x = 0;

                // Delta value is between 0.0 and 1.0 so they need to be scaled within the kinect region.
                var yD = d.Y * kinectRegion.ActualHeight;
                var xD = d.X * kinectRegion.ActualWidth;
                //Element coordinates between the canvas boundaries
                if (x + xD < 0) { x = 0; xD = 0; }
                if (x + xD + width > parent.ActualWidth) { x = parent.ActualWidth - width; xD = 0; }
                if (y + yD < 0) { y = 0; yD = 0; }
                if (y + yD + height > parent.ActualHeight) { y = parent.ActualHeight - height; yD = 0; }
                dragDropElement.X = x + xD;
                dragDropElement.Y = y + yD;
                Canvas.SetTop(dragDropElement, y + yD);
                Canvas.SetLeft(dragDropElement, x + xD);
            }
        }

        #endregion Gesture method

        #region Disposable implementation

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                kinectRegion = null;
                inputModel = null;
                dragDropElement = null;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }

        #endregion Disposable implementation
    }
}