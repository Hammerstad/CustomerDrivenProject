using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using WindowsGame3.Models;
using WindowsGame3.View;

[assembly: InternalsVisibleTo("ProjectTests")]
namespace WindowsGame3.Controller
{
    public class FeedbackHandler
    {
        #region Public static methods

        public static void HandleFeedback( String feedback){
            // Display on feedback line(command line)
            try
            {
                TheDisplay.Instance.SetCommandToDrawOnScreen(feedback);
                
                // Give voicefeedback if that is on
                if (VoiceFeedback.ActiveVoiceFeedback.IsOn)
                {
                    VoiceFeedback.ActiveVoiceFeedback.GiveVoiceFeedback(feedback);
                }
            }
            catch (Exception e)
            {
                //Trying to write before the display is initialized
            }
        }

        #endregion
    }
}
