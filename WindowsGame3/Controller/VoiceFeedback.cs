using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Speech.Synthesis;

namespace WindowsGame3.Controller
{
    class VoiceFeedback
    {
        public static VoiceFeedback ActiveVoiceFeedback;

        private SpeechSynthesizer Tts;
        public bool IsOn { get; set; }

        public VoiceFeedback()
        {
            ActiveVoiceFeedback = this;
            Tts = new SpeechSynthesizer();
            IsOn = true;
        }

        public void GiveVoiceFeedback(String feedback)
        {
            Tts.SpeakAsync(feedback);
        }

        public void Clear()
        {
            Tts.SpeakAsyncCancelAll();
        }

        public static void Unload()
        {
            if (ActiveVoiceFeedback != null)
                ActiveVoiceFeedback.Clear();
        }
    }
}
