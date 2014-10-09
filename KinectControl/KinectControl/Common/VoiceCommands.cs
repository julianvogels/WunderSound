using System;
using System.Linq;
using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;


namespace KinectControl.Common
{
     public class VoiceCommands
    {
        private KinectAudioSource kinectAudio;
        private SpeechRecognitionEngine speechRecognitionEngine;
        private readonly KinectSensor kinect;
        private string heardString;
        public string HeardString
        {
            get { return heardString; }
            set { heardString = value; }
        }

        public VoiceCommands(KinectSensor kinect, string[] commands)
        {
            this.kinect = kinect;
            InitalizeKinectAudio(commands);
        }

        private void InitalizeKinectAudio(string[] commands)
        {
             heardString = "";
            RecognizerInfo recognizerInfo = GetKinectRecognizer();
            if(recognizerInfo != null)
            speechRecognitionEngine = new SpeechRecognitionEngine(recognizerInfo.Id);
            var choices = new Choices();
            foreach (var command in commands)
            {
                choices.Add(command);
            }
            var grammarBuilder = new GrammarBuilder { Culture = recognizerInfo.Culture };
            grammarBuilder.Append(choices);
            var grammar = new Grammar(grammarBuilder);
            speechRecognitionEngine.LoadGrammar(grammar);
            speechRecognitionEngine.SpeechRecognized += SpeechRecognitionEngineSpeechRecognized;
        }
        
        public void StartAudioStream()
        {
            try
            {
                kinectAudio = kinect.AudioSource;
                kinectAudio.BeamAngleMode = BeamAngleMode.Manual;
                kinectAudio.ManualBeamAngle = Math.PI / 180.0 * 10.0; //angle in radians
                //kinectAudio.BeamAngleMode = BeamAngleMode.Adaptive;
                kinect.AudioSource.EchoCancellationMode = EchoCancellationMode.None;
                kinect.AudioSource.AutomaticGainControlEnabled = false;
                var stream = kinectAudio.Start();
                speechRecognitionEngine.SetInputToAudioStream(stream,
                                                              new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1,
                                                                                        32000, 2, null));
                speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch
            {

            }
        }

        public bool GetHeard(string expectedString)
        {
            return expectedString.Equals(heardString);
        }

        private void SpeechRecognitionEngineSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence >= 0.57)
            {
              Kinect.FramesCount = 0;
                heardString = e.Result.Text;
            }
        }

        private static RecognizerInfo GetKinectRecognizer()
        {
            Func<RecognizerInfo, bool> matchingFunc = matchFunction =>
            {
                string value;
                matchFunction.AdditionalInfo.TryGetValue("Kinect", out value);
                return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) && "en-US".Equals(matchFunction.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
            };
            return SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
        }

    }
}
