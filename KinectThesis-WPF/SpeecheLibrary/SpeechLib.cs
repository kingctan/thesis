namespace SpeecheLibrary
{
    using Microsoft.Kinect;
    using Microsoft.Speech.AudioFormat;
    using Microsoft.Speech.Recognition;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Utils;

    /// <summary>
    /// Library that recognizes the speech commands.
    /// </summary>
    public class SpeechLib : DisposableBase
    {
        #region Private Properties

        /// <summary>
        /// Active Kinect sensor.
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Stream for 32b-16b conversion.
        /// </summary>
        private KinectAudioStream convertStream = null;

        /// <summary>
        /// Speech recognition engine using audio data from Kinect.
        /// </summary>
        private SpeechRecognitionEngine speechEngine = null;

        /// <summary>
        /// Handler for when a commanda is recognized
        /// </summary>
        private EventHandler<SpeechRecognizedEventArgs> speechRecognized;

        /// <summary>
        /// Handler for when a commanda is rejecrted
        /// </summary>
        private EventHandler<SpeechRecognitionRejectedEventArgs> speechRejected;

        private string[] buttonContent = new string[] { "Play", "Pause", "Rewind", "Fast Forward", "Stop" };
        private string[] ratingVaues = new string[] { "first", "second", "third", "fourth", "fifth" };
        private string[] extraCommandos = new string[] { "louder", "dim", "scroll left", "scroll right", "scroll end", "scroll begin", "scroll up", "scroll down", "scroll top", "scroll bottom", "scroll home" };
        private RecognizerInfo ri;

        #endregion Private Properties

        #region Ctor

        public SpeechLib(KinectSensor _kinectSensor, EventHandler<SpeechRecognizedEventArgs> _speechRecognized, EventHandler<SpeechRecognitionRejectedEventArgs> _speechRejected)
        {
            if (_kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }
            // Only one sensor is supported
            this.kinectSensor = _kinectSensor;
            // grab the audio stream
            IReadOnlyList<AudioBeam> audioBeamList = this.kinectSensor.AudioSource.AudioBeams;
            System.IO.Stream audioStream = audioBeamList[0].OpenInputStream();

            // create the convert stream
            this.convertStream = new KinectAudioStream(audioStream);

            ri = TryGetKinectRecognizer();

            if (null != ri)
            {
                this.speechEngine = new SpeechRecognitionEngine(ri.Id);
                SetBasicGrammar();
                this.speechRecognized = _speechRecognized;
                this.speechRejected = _speechRejected;
                this.speechEngine.SpeechRecognized += this.speechRecognized;
                this.speechEngine.SpeechRecognitionRejected += this.speechRejected;
                // let the convertStream know speech is going active
                this.convertStream.SpeechActive = true;

                // For long recognition sessions (a few hours or more), it may be beneficial to turn off adaptation of the acoustic model.
                // This will prevent recognition accuracy from degrading over time.
                speechEngine.UpdateRecognizerSetting("AdaptationOn", 0);

                this.speechEngine.SetInputToAudioStream(
                    this.convertStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                this.speechEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
            else
            {
                throw new ArgumentNullException("SpeechRecognitionEngine");
            }
        }

        public void AddHandlers()
        {
            this.speechEngine.SpeechRecognized += this.speechRecognized;
            this.speechEngine.SpeechRecognitionRejected += this.speechRejected;
        }

        public void RemoveHandlers()
        {
            this.speechEngine.SpeechRecognized -= this.speechRecognized;
            this.speechEngine.SpeechRecognitionRejected -= this.speechRejected;
        }

        #endregion Ctor

        #region Create grammars

        /// <summary>
        /// Sets the grammar with the commando's after the key word Kinect is recognized
        /// </summary>
        public void SetListeningGrammar()
        {
            Grammar grammar = null;
            if (ri != null)
            {
                var directions = new Choices();
                for (int i = 0; i < ratingVaues.Length; i++)
                {
                    directions.Add(new SemanticResultValue("kinect " + ratingVaues[i] + " star", ratingVaues[i] + " star"));
                    directions.Add(new SemanticResultValue("select " + ratingVaues[i] + " star", ratingVaues[i] + " star"));
                }
                foreach (String content in buttonContent)
                {
                    directions.Add(new SemanticResultValue("kinect " + content, content));
                    directions.Add(new SemanticResultValue(content, content));
                }
                foreach (String content in extraCommandos)
                {
                    directions.Add(new SemanticResultValue("kinect " + content, content));
                    directions.Add(new SemanticResultValue(content, content));
                }

                var gb = new GrammarBuilder { Culture = ri.Culture };
                gb.Append(directions);
                grammar = new Grammar(gb);
            }
            this.speechEngine.LoadGrammar(grammar);
        }

        public void SetBasicGrammar()
        {
            var directions = new Choices();

            directions.Add(new SemanticResultValue("kinect", "kinect"));
            for (int i = 0; i < ratingVaues.Length; i++)
            {
                directions.Add(new SemanticResultValue("kinect " + ratingVaues[i] + " star", ratingVaues[i] + " star"));
            }

            foreach (String content in buttonContent)
            {
                directions.Add(new SemanticResultValue("kinect " + content, content));
            }
            foreach (String content in extraCommandos)
            {
                directions.Add(new SemanticResultValue("kinect " + content, content));
            }
            var gb = new GrammarBuilder { Culture = ri.Culture };
            gb.Append(directions);

            var g = new Grammar(gb);
            this.speechEngine.LoadGrammar(g);
        }

        #endregion Create grammars

        /// <summary>
        /// Gets the metadata for the speech recognizer (acoustic model) most suitable to
        /// process audio from Kinect device.
        /// </summary>
        /// <returns>
        /// RecognizerInfo if found, <code>null</code> otherwise.
        /// </returns>
        private static RecognizerInfo TryGetKinectRecognizer()
        {
            IEnumerable<RecognizerInfo> recognizers;

            // This is required to catch the case when an expected recognizer is not installed.
            // By default - the x86 Speech Runtime is always expected.
            try
            {
                recognizers = SpeechRecognitionEngine.InstalledRecognizers();
            }
            catch (COMException)
            {
                return null;
            }

            foreach (RecognizerInfo recognizer in recognizers)
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }

        #region DisposableBase Implementation

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (kinectSensor != null)
                {
                    kinectSensor.Close();
                    this.kinectSensor = null;
                }

                if (null != this.convertStream)
                {
                    this.convertStream.SpeechActive = false;
                }

                if (null != this.speechEngine)
                {
                    RemoveHandlers();
                    this.speechEngine.RecognizeAsyncStop();
                }
            }
        }

        ~SpeechLib()
        {
            Dispose(false);
        }

        #endregion DisposableBase Implementation
    }
}