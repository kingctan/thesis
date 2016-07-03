using System;

namespace ArgsLibrary
{
    /// <summary>
    /// Event arguments when the face has been captured. All properties of the face.
    /// </summary>
    public class FaceBasicEventArgs : EventArgs
    {
        public bool FaceEngagement
        {
            get;
            set;
        }

        public bool Glasses
        {
            get;
            set;
        }

        public bool Happy
        {
            get;
            set;
        }

        public bool LeftEyeClosed
        {
            get;
            set;
        }

        public bool RightEyeClosed
        {
            get;
            set;
        }

        public bool LookingAway
        {
            get;
            set;
        }

        public bool MouthMoved
        {
            get;
            set;
        }

        public bool MouthOpen
        {
            get;
            set;
        }

        public int Yaw
        {
            get;
            set;
        }

        public int Pitch
        {
            get;
            set;
        }

        public int Roll
        {
            get;
            set;
        }
    }
}