using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Utils;

namespace AudioLibrary
{
    /// <summary>
    /// Library that captures the audio data.
    /// </summary>
    public class AudioBeamLib : DisposableBase
    {
        #region Private properties

        /// <summary>
        /// Number of bytes in each Kinect audio stream sample (32-bit IEEE float).
        /// </summary>
        private const int BytesPerSample = sizeof(float);

        /// <summary>
        /// Number of audio samples represented by each column of pixels in wave bitmap.
        /// </summary>
        private const int SamplesPerColumn = 40;

        /// <summary>
        /// Minimum energy of audio to display (a negative number in dB value, where 0 dB is full scale)
        /// </summary>
        private const int MinEnergy = -90;

        /// <summary>
        /// Will be allocated a buffer to hold a single sub frame of audio data read from audio stream.
        /// </summary>
        private readonly byte[] audioBuffer = null;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Reader for audio frames
        /// </summary>
        private AudioBeamFrameReader reader = null;

        /// <summary>
        /// Last observed audio beam angle in radians, in the range [-pi/2, +pi/2]
        /// </summary>
        private float beamAngle = 0;

        /// <summary>
        /// Last observed audio beam angle confidence, in the range [0, 1]
        /// </summary>
        private float beamAngleConfidence = 0;

        /// <summary>
        /// Sum of squares of audio samples being accumulated to compute the next energy value.
        /// </summary>
        private float accumulatedSquareSum;

        /// <summary>
        /// Number of audio samples accumulated so far to compute the next energy value.
        /// </summary>
        private int accumulatedSampleCount;

        /// <summary>
        /// The source for the audio.
        /// </summary>
        private AudioSource audioSource;

        #endregion Private properties

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the AudioBeamLib
        /// </summary>
        public AudioBeamLib(KinectSensor _kinectSensor)
        {
            if (_kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }
            // Only one Kinect Sensor is supported
            this.kinectSensor = _kinectSensor;

            // Get its audio source
            audioSource = this.kinectSensor.AudioSource;

            // Allocate 1024 bytes to hold a single audio sub frame. Duration sub frame
            // is 16 msec, the sample rate is 16khz, which means 256 samples per sub frame.
            // With 4 bytes per sample, that gives us 1024 bytes.
            this.audioBuffer = new byte[audioSource.SubFrameLengthInBytes];

            // Uncomment these two lines to overwrite the automatic mode of the audio beam.
            // It will change the beam mode to manual and set the desired beam angle.
            // In this example, point it straight forward.
            // Furthermore, setting these values is an asynchronous operation --
            // it may take a short period of time for the beam to adjust.
            /*
            audioSource.AudioBeams[0].AudioBeamMode = AudioBeamMode.Manual;
            audioSource.AudioBeams[0].BeamAngle = 0;
            */
            CreateAudioBeamDataObservable();
        }

        #endregion Ctor

        #region Public Properties

        public IObservable<List<AudioBeamData>> TrackingObservable_AudioBeamData { get; private set; }

        #endregion Public Properties

        #region Private methods

        /// <summary>
        /// Creates AudioFrameArrived observable
        /// </summary>
        private void CreateAudioBeamDataObservable()
        {
            // Open the reader for the audio frames
            this.reader = audioSource.OpenReader();
            TrackingObservable_AudioBeamData = Observable.FromEventPattern<AudioBeamFrameArrivedEventArgs>(reader, "FrameArrived")
                .Select(f => f.EventArgs.FrameReference.AcquireBeamFrames())
                .Where(frameList => frameList != null)
                .Select(frameList =>
                {
                    var list = new List<AudioBeamData>();
                    // AudioBeamFrameList is IDisposable
                    using (frameList)
                    {
                        // Only one audio beam is supported. Get the sub frame list for this beam
                        IReadOnlyList<AudioBeamSubFrame> subFrameList = frameList[0].SubFrames;
                        // Loop over all sub frames, extract audio buffer and beam information
                        foreach (AudioBeamSubFrame subFrame in subFrameList)
                        {
                            this.beamAngle = subFrame.BeamAngle;
                            this.beamAngleConfidence = subFrame.BeamAngleConfidence;
                            // Convert from radians to degrees
                            float beamAngleInDeg = this.beamAngle * 180.0f / (float)Math.PI;

                            // Process audio buffer
                            subFrame.CopyFrameDataToArray(this.audioBuffer);

                            for (int i = 0; i < this.audioBuffer.Length; i += BytesPerSample)
                            {
                                // Extract the 32-bit IEEE float sample from the byte array
                                float audioSample = BitConverter.ToSingle(this.audioBuffer, i);

                                this.accumulatedSquareSum += audioSample * audioSample;
                                ++this.accumulatedSampleCount;

                                if (this.accumulatedSampleCount < SamplesPerColumn)
                                {
                                    continue;
                                }

                                float meanSquare = this.accumulatedSquareSum / SamplesPerColumn;

                                if (meanSquare > 1.0f)
                                {
                                    // A loud audio source right next to the sensor may result in mean square values
                                    // greater than 1.0. Cap it at 1.0f
                                    meanSquare = 1.0f;
                                }

                                // Calculate energy in dB, in the range [MinEnergy, 0], where MinEnergy < 0
                                float energy = MinEnergy;

                                if (meanSquare > 0)
                                {
                                    energy = (float)(10.0 * Math.Log10(meanSquare));
                                }

                                //* Normalize values to the range [0, 1]
                                float normalizedEnergy = (MinEnergy - energy) / MinEnergy;
                                this.accumulatedSquareSum = 0;
                                this.accumulatedSampleCount = 0;
                                list.Add(new AudioBeamData { BeamAngleInDeg = beamAngleInDeg, Energy = energy, BeamAngleConfidence = beamAngleConfidence, NormalizedEnergy = normalizedEnergy });
                            }
                        }
                    }
                    return list;
                });
        }

        #endregion Private methods

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
                if (this.reader != null)
                {
                    // AudioBeamFrameReader is IDisposable
                    this.reader.Dispose();
                    this.reader = null;
                }
            }
        }

        ~AudioBeamLib()
        {
            Dispose(false);
        }

        #endregion DisposableBase Implementation
    }
}