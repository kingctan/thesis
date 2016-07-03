namespace EmotionRecognition
{
    /// <summary>
    /// The 6 different emotions who can be captured, all the properties are string representation of float.
    /// </summary>
    public class Emotions
    {
        public string Angry
        {
            get;
            set;
        }

        public string Disgust
        {
            get;
            set;
        }

        public string Fear
        {
            get;
            set;
        }

        public string Happy
        {
            get;
            set;
        }

        public string Sad
        {
            get;
            set;
        }

        public string Surprise
        {
            get;
            set;
        }

        /// <summary>
        /// String method used to show the results to the users
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Angry: " + Angry + " Happy: " + Happy + " Sad: " + Sad + " Fear: " + Fear + " Surprise: " + Surprise + " Disgust: " + Disgust;
        }
    }
}