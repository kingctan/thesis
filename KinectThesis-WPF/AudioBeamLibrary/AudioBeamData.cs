namespace AudioLibrary
{
    /// <summary>
    /// Audio data captured.
    /// </summary>
    public class AudioBeamData
    {
        /// <summary>
        /// Angle of the focus.
        /// </summary>
        public float BeamAngleInDeg
        {
            get;
            set;
        }

        /// <summary>
        /// Confidence of the beam angle.
        /// </summary>
        public float BeamAngleConfidence
        {
            get;
            set;
        }

        /// <summary>
        /// Energy in dB.
        /// </summary>
        public float Energy
        {
            get;
            set;
        }

        /// <summary>
        /// Normalizes value to the range [0, 1].
        /// </summary>
        public float NormalizedEnergy
        {
            get;
            set;
        }
    }
}