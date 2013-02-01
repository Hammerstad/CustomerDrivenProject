using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WindowsGame3.View;

namespace WindowsGame3.Controller
{

    /// <summary>
    /// This class handles the strings which are recognized by the SpeechRecognizer and maps them to the appropriate commands.
    /// </summary>
    class SpeechHandler
    {
        /// <summary>
        /// A dictionary containing all the different numbers we might recognize, and their corresponding value.
        /// Used to figure out the total value of the number we are saying.
        /// </summary>
        private static readonly Dictionary<string, int> ValueMap = new Dictionary<string, int>
                                                            {
                                                                 {"zero",0},
                                                                 {"one",1},
                                                                 {"two",2},
                                                                 {"three",3},
                                                                 {"four",4},
                                                                 {"five",5},
                                                                 {"six",6},
                                                                 {"seven",7},
                                                                 {"eight",8},
                                                                 {"nine",9},
                                                                 
                                                                 {"eleven",11},
                                                                 {"twelve",12},
                                                                 {"thirteen",13},
                                                                 {"fourteen",14},
                                                                 {"fifteen",15},

                                                                 {"ten",10},
                                                                 {"twenty",20},
                                                                 {"thirty",30},
                                                                 {"fourty",40},
                                                                 {"fifty",50},
                                                                 {"sixty",60},
                                                                 {"seventy",70},
                                                                 {"eighty",80},
                                                                 {"ninety",90},
                                                                 
                                                                 {"hundred",100},
                                                                 {"onehundred",100},
                                                                 {"twohundred",200},
                                                                 {"threehundred",300},
                                                                 {"fourhundred",400},
                                                                 {"fivehundred",500},
                                                                 {"sixhundred",600},
                                                                 {"sevenhundred",700},
                                                                 {"eighthundred",800},
                                                                 {"ninehundred",900},
                                                                 
                                                                 {"north",0},
                                                                 {"west",90},
                                                                 {"south",180},
                                                                 {"east",270},
                                                                 
                                                                 {"and",0},
                                                                 {"minus",0},
                                                             };

        /// <summary>
        /// This function takes a string as an input and maps it to the proper functions.
        /// </summary>
        /// <param name="word">The phrase which has been recognized.</param>
        public static void HandleSpeech(String word, float confidence)
        {
            Console.WriteLine(word);
            string[] command = word.Split(' ');
            switch (command[0])
            {
                case "select":
                    SelectPlane(command[2]);
                    break;
                case "alter":
                    switch(command[1])
                    {
                        case "heading":
                            AlterHeading(command);
                            break;
                        case "pitch":
                            AlterPitch(command);
                            break;
                        case "speed":
                            AlterSpeed(command);
                            break;
                    }
                    break;
                case "exit":
                    //MainProgram.Game.Exit();
                    break;
                case "toggle":
                    switch (command[1])
                    {
                        case "names":
                            ToggleNames();
                            break;
                        case "lines":
                            ToggleLinesBelowThePlanes();
                            break;
                        case "coordinates":
                            ToggleCoordinates();
                            break;
                        case "help":
                            ToggleHelpForCommands();
                            break;
                        case "voice":
                            ToggleVoiceFeedback();
                            break;
                    }
                    break;
                case "go":
                    if(command[1]=="too")
                    {
                        GoTo();
                    }
                    break;
                case "follow":
                        Follow();
                    break;
                case "unfollow":
                    Unfollow();
                    break;
                case "deselect":
                    Deselect();
                    break;
            }
        }

        private static void Unfollow()
        {
            if(AirplaneController.SelectedPlane != null)
            {
                DisplayController.Unfollow();
            }
        }

        private static void Deselect()
        {
            if (AirplaneController.SelectedPlane != null)
            {
                FeedbackHandler.HandleFeedback("Plane is no longer selected.");
                AirplaneController.SelectedPlane = null;
            }
        }

        /// <summary>
        /// Selects a plane, by the name of the plane.
        /// </summary>
        /// <param name="planeNumber">The name of the plane.</param>
        private static void SelectPlane(string planeNumber)
        {
            AirplaneController.SelectPlaneByName(planeNumber);
        }

        /// <summary>
        /// Alters the heading of the selected plane.
        /// </summary>
        /// <param name="degrees">The entire phrase being recognized.</param>
        private static void AlterHeading(string[] degrees)
        {
            //First we want to know where to turn the plane towards.
            int number = 0;
            //The first two elements in degrees are "alter" and "heading"
            for(int i = 2; i < degrees.Length; i++)
            {
                if (ValueMap.ContainsKey(degrees[i]))
                    number += ValueMap[degrees[i]];
            }
            //Let's not turn too much
            if (number>360)
            {
                number -= 360;
            }
            //Make the plane turn
            AirplaneController.SetYaw(MathHelper.ToRadians(number));
        }

        /// <summary>
        /// Alters the pitch of the selected plane (going up or down).
        /// </summary>
        /// <param name="degrees">The entire phrase being recognized.</param>
        private static void AlterPitch(string[] degrees)
        {
            //First we want to know how many degrees we going up/down.
            int number = 0;
            //Also neat to know if we are going up or down
            int minus = 1;
            //The first two elements in degrees are "alter" and "pitch"
            for (int i = 2; i < degrees.Length; i++)
            {
                //If we detect a minus, we want the plane to go down
                if(degrees[i].Equals("minus"))
                {
                    minus = -1;
                    //Jump to the next number, no need to map "minus" to 0.
                    continue;
                }
                if(ValueMap.ContainsKey(degrees[i]))
                    number += ValueMap[degrees[i]];
            }
            //Make sure we are going down if we are supposed to go down.
            number *= minus;
            //Make the plane turn
            AirplaneController.SetPitch(MathHelper.ToRadians(number));
            
        }
        /// <summary>
        /// Alters the speed of the selected airplane.
        /// </summary>
        /// <param name="speed">The entire phrase being recognized.</param>
        private static void AlterSpeed(string[] speed)
        {
            //First we want to know how fast the plane should go
            int number = 0;
            //The first two elements of speed are "alter" and "speed"
            for (int i = 2; i < speed.Length; i++)
            {
                if (ValueMap.ContainsKey(speed[i]))
                    number += ValueMap[speed[i]];
            }
            //Change the speed of the plane

            AirplaneController.SetVelocity(number);
        }

        /// <summary>
        /// Function that toggles the display of names on/off.
        /// </summary>
        private static void ToggleNames()
        {
            FeedbackHandler.HandleFeedback("Toggling names, now " + ((!AirplaneController.ToggleNames) ? "on" : "off"));
            AirplaneController.ToggleNames = !AirplaneController.ToggleNames;
        }

        /// <summary>
        /// Function that toggles the display of coordinates on/off.
        /// </summary>
        private static void ToggleCoordinates()
        {
            FeedbackHandler.HandleFeedback("Toggling coordinates, now " + ((!AirplaneController.ToggleCoordinates) ? "on" : "off"));
            AirplaneController.ToggleCoordinates = !AirplaneController.ToggleCoordinates;
        }

        /// <summary>
        /// Function that toggles showing all commands on the screen.
        /// </summary>
        private static void ToggleHelpForCommands()
        {
            FeedbackHandler.HandleFeedback("Toggling help for commands, now " + ((!TheDisplay.Instance.ShowPossibleCommands) ? "on" : "off"));
            TheDisplay.Instance.ShowPossibleCommands = !TheDisplay.Instance.ShowPossibleCommands;
        }

        /// <summary>
        /// Function that toggles if the system will give voice feedback 
        /// </summary>
        private static void ToggleVoiceFeedback()
        {
            Boolean current = VoiceFeedback.ActiveVoiceFeedback.IsOn;
            VoiceFeedback.ActiveVoiceFeedback.IsOn = true;
            FeedbackHandler.HandleFeedback("Toggling voice feedback, now " + ((!current) ? "on" : "off"));
            VoiceFeedback.ActiveVoiceFeedback.IsOn = !current;
        }

        /// <summary>
        /// Function that toggles the display of lines below the plane on/off.
        /// </summary>
        private static void ToggleLinesBelowThePlanes()
        {
            FeedbackHandler.HandleFeedback("Toggling line under planes, now " + ((!AirplaneController.ToggleLineUnderPlanes) ? "on" : "off"));
            AirplaneController.ToggleLineUnderPlanes = !AirplaneController.ToggleLineUnderPlanes;
        }

        /// <summary>
        /// Goes to the selected plane if there is one, else nothing happens.
        /// </summary>
        private static void GoTo()
        {
            if (AirplaneController.SelectedPlane != null)
            {
                FeedbackHandler.HandleFeedback("Going to plane " + AirplaneController.SelectedPlane.Name);
                DisplayController.GoTo(AirplaneController.SelectedPlane.Position);
            }
        }

        /// <summary>
        /// Follows the selected plane if there is one, else nothing happens.
        /// </summary>
        private static void Follow()
        {
            if (AirplaneController.SelectedPlane != null)
            {
                DisplayController.Follow();
            }
        }
    }
}
