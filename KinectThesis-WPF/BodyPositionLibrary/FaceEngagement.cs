namespace BodyPositionLibrary
{
    /// <summary>
    /// Engagement values of the face.
    /// </summary>
    public class FaceEngagement
    {
        public float TrackingId
        {
            get;
            set;
        }

        /// <summary>
        /// Yes, maybe or no value: estimated of head orientation.
        /// </summary>
        public string Engagement
        {
            get;
            set;
        }

        /// <summary>
        /// Yes, maybe or no value= estimated of Engagement property + eyes closed/open.
        /// </summary>
        public string LookingAway
        {
            get;
            set;
        }
    }
}