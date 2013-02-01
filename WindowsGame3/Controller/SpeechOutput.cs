using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Speech.Synthesis;
using Microsoft.Xna.Framework;

namespace WindowsGame3.Controller
{
    public class SpeechOutput
    {
        private static SpeechSynthesizer SpeechSynthesizer = null;
        private static String ToSay = "";
        public int Rate
        {
            // an int from -10 to 10
            get { return SpeechSynthesizer.Rate; }
            set { SpeechSynthesizer.Rate = value; }
        }
        public static int Volume
        {
            // an int from 0 to 100
            get { return SpeechSynthesizer.Volume; }
            set { SpeechSynthesizer.Volume = value; }
        }

        public SpeechOutput()
        {
            SpeechSynthesizer = new SpeechSynthesizer();
        }

        public static void Stop()
        {
            SpeechSynthesizer.SpeakAsyncCancelAll();
        }

        public static void Say(string textToSpeak)
        {
            SpeechSynthesizer.SpeakAsync(textToSpeak);
            Console.WriteLine(textToSpeak + " ye\n"+SpeechSynthesizer);
        }

        //these threaded calls are potentially unsafe!
        public static void SayThreaded(string textToSpeak)
        {
            ToSay = textToSpeak;
            var oThread = new Thread(SayThread);
            oThread.Start();
        }

        private static void SayThread()
        {
            SpeechSynthesizer = new SpeechSynthesizer();
            SpeechSynthesizer.SpeakAsync(ToSay);
        }
    }
}
