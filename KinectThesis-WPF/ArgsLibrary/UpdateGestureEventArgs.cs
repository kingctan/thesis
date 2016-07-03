using System;

namespace ArgsLibrary
{
    /// <summary>
    /// Event arguments for the recognition of a gesture.
    /// </summary>
    public class UpdateGestureEventArgs : EventArgs
    {
        public bool Detected
        {
            get;
            set;
        }

        /// <summary>
        /// Only for discrete gestures.
        /// </summary>
        public float Confidence
        {
            get;
            set;
        }

        public ulong TrackingId
        {
            get;
            set;
        }

        public object Sender
        {
            get;
            set;
        }

        public string GestureName
        {
            get;
            set;
        }
    }
}