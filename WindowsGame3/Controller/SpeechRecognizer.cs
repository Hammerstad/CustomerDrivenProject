using System;
using System.IO;
using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;

namespace WindowsGame3.Controller
{
    public class SpeechRecognizer
    {
        private KinectSensor Sensor;

        private RecognizerInfo RecognizerInfo;
        private SpeechRecognitionEngine SpeechEngine;
        private Grammar Grammar;

        private const double ConfidenceThreshold = 0.65;

        /// <summary>
        /// Default constructor, does nothing.
        /// </summary>
        public SpeechRecognizer()
        {
        }

        /// <summary>
        /// Method name reflects where it is supposed to be called. Loads/initializes all the resources
        /// of our speech recognizer.
        /// </summary>
        public void LoadContent()
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.Sensor = potentialSensor;
                    break;
                }
            }
            if (this.Sensor != null)
            {
                try
                {
                    // Start the sensor!
                    this.Sensor.Start();
                }
                catch (IOException)
                {
                    // Some other application is streaming from the same Kinect sensor
                    this.Sensor = null;
                }
            }

            if (this.Sensor == null)
            {
                return;
            }

            RecognizerInfo = GetKinectRecognizer();
            if (RecognizerInfo != null)
            {

                this.SpeechEngine = new SpeechRecognitionEngine(RecognizerInfo.Id);

                SetupGrammar();

                SpeechEngine.SetInputToAudioStream(
                    Sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                SpeechEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
        }

        /// <summary>
        /// Method name reflects where it is supposed to be called. Unloads/stops all the resources
        /// of our speech recognizer.
        /// </summary>
        public void UnloadContent()
        {
            if (this.Sensor != null)
            {
                this.Sensor.AudioSource.Stop();

                this.Sensor.Stop();
                this.Sensor = null;
            }

            if (this.SpeechEngine != null)
            {
                SpeechEngine.SpeechRecognized -= SpeechRecognized;
                SpeechEngine.SpeechHypothesized -= SpeechHypothesized;
                SpeechEngine.SpeechRecognitionRejected -= SpeechRecognitionRejected;
                this.SpeechEngine.RecognizeAsyncStop();
            }
        }

        /// <summary>
        /// Loads the grammar from the CustomGrammar.cs and tells our program that
        /// this is the vocabulary it is allowed to use.
        /// </summary>
        private void SetupGrammar()
        {

            Grammar = CustomGrammar.CreateGrammar(RecognizerInfo.Culture);

            SpeechEngine.LoadGrammar(Grammar);

            SpeechEngine.SpeechRecognized += SpeechRecognized;
            SpeechEngine.SpeechHypothesized += SpeechHypothesized;
            SpeechEngine.SpeechRecognitionRejected += SpeechRecognitionRejected;
        }

        /// <summary>
        /// Gets the metadata for the speech recognizer (acoustic model) most suitable to
        /// process audio from Kinect device.
        /// </summary>
        /// <returns>
        /// RecognizerInfo if found, <code>null</code> otherwise.
        /// </returns>
        private static RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
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

        /// <summary>
        /// Processes speech which has been rejected.
        /// </summary>
        private static void SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            // Console.WriteLine("Rejected: " + e.Result.Text);
        }

        /// <summary>
        /// Processes speech which the kinect is uncertain about.
        /// </summary>
        private static void SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            //Console.WriteLine("Hypothesized: " + e.Result.Text);
        }

        /// <summary>
        /// Processes speech which has been recognized.
        /// </summary>
        private static void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence > ConfidenceThreshold)
            {
                SpeechHandler.HandleSpeech(e.Result.Text, e.Result.Confidence);
            }
        }
    }
}
