using System.Speech.Synthesis;

namespace TextToSpeechLibrary
{
    /// <summary>
    /// Singelton class for the SpeecheSynthesizer, there is only one needed for the application.
    /// </summary>
    public class TextToSpeechLib
    {
        private class SpeecheSynthesizerCreator
        {
            /// <summary>
            /// Instance that does the text to speech conversion.
            /// </summary>
            internal static SpeechSynthesizer uniqueInstance = new SpeechSynthesizer();
        }

        public static SpeechSynthesizer UniqueInstance
        {
            get
            {
                if (SpeecheSynthesizerCreator.uniqueInstance == null)
                {
                    SpeecheSynthesizerCreator.uniqueInstance = new SpeechSynthesizer();
                }
                return SpeecheSynthesizerCreator.uniqueInstance;
            }

            private set
            {
                SpeecheSynthesizerCreator.uniqueInstance = value;
            }
        }

        public TextToSpeechLib(int volume, int rate)
        {
            UniqueInstance.Volume = volume;
            UniqueInstance.Rate = rate;
        }

        public static int Volume
        {
            get
            {
                return UniqueInstance.Volume;
            }

            set
            {
                UniqueInstance.Volume = value;
            }
        }

        public static int Rate
        {
            get
            {
                return UniqueInstance.Rate;
            }

            set
            {
                UniqueInstance.Rate = value;
            }
        }

        /// <summary>
        /// Start speak commando and cancell other speak commandos
        /// </summary>
        /// <param name="text"></param>
        public static void Speak(string text)
        {
            // cancels anything that's playing
            UniqueInstance.SpeakAsyncCancelAll();
            UniqueInstance.SpeakAsync(text);
        }

        /// <summary>
        /// Start speak commando and don't cancell other speak commandos
        /// </summary>
        /// <param name="text"></param>
        public static void SpeakNotInterrupt(string text)
        {
            UniqueInstance.SpeakAsync(text);
        }

        /// <summary>
        /// Cancell other speak commandos
        /// </summary>
        public static void Stop()
        {
            UniqueInstance.SpeakAsyncCancelAll();
            UniqueInstance.Dispose();
            UniqueInstance = null;
        }
    }
}