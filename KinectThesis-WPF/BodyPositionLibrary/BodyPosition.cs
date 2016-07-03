namespace BodyPositionLibrary
{
    /// <summary>
    /// 3D joint positions.
    /// </summary>
    public class BodyPosition
    {
        public float TrackingId
        {
            get;
            set;
        }

        public float HeadX
        {
            get;
            set;
        }

        public float HeadY
        {
            get;
            set;
        }

        public float HeadZ
        {
            get;
            set;
        }

        public float ShoulderLeftZ
        {
            get;
            set;
        }

        public float ShoulderRightZ
        {
            get;
            set;
        }
    }
}