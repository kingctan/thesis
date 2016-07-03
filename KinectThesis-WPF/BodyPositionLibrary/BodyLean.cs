namespace BodyPositionLibrary
{
    /// <summary>
    /// All the lean information.
    /// </summary>
    public class BodyLean
    {
        public float TrackingId
        {
            get;
            set;
        }

        /// <summary>
        /// Vertical lean between [-1,1].
        /// </summary>
        public float X
        {
            get;
            set;
        }

        /// <summary>
        /// Horizontal lean between [-1,1].
        /// </summary>
        public float Y
        {
            get;
            set;
        }
    }
}