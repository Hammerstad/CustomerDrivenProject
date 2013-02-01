using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WindowsGame3.Models;
using WindowsGame3.Controller;
using Microsoft.Kinect;

using System;
using WindowsGame3.View;

namespace WindowsGame3
{

    public class MainProgram : Game
    {
        /// <summary>
        /// This is different objects Main is going to use
        /// </summary>
        public static MainProgram Main;
        public KinectVideoStream KinectVideoStream;
        public TheDisplay Display { get; set; }
        public MovementDetection MovementDetection;
        public KinectSensor Kinect;
        public Grass Grass;
        public SpeechRecognizer SpeechRecognizer = new SpeechRecognizer();
        private CrashPathDetection CrashPathDetection;
        
        /// <summary>
        /// This is used in a testmode for the presentation to set two planes on crash course
        /// </summary>
        private Boolean TestModelPlaneCrashIsToggled = false;

        //FPS
        SpriteFont SpriteFont;

        /// <summary>
        /// Creates the main program. 
        /// </summary>
        public MainProgram()
        {
            //Set the static variable Main. Is called from other classes in order to get 
            //GraphicsDeviceManager and other necessary stuff
            Main = this;
            //The rootdirectory of content is the content folder
            Content.RootDirectory = "Content";
            //Create a camera looking at the origin, can be accessed staticly
            new DisplayController(250f, 270f, 250f, 250f, 270f, 249f);
            //Create the display
            Display = TheDisplay.Instance;
            //Create Feedback
            VoiceFeedback vfb = new VoiceFeedback();
            //Create the class for movement detection
            MovementDetection = new MovementDetection();
            //Create the class for the kinect video stream in the bottom right corner
            KinectVideoStream = new KinectVideoStream();
            //Create the speech recognizer
            SpeechRecognizer = new SpeechRecognizer();
            //Create crashpath detection
            CrashPathDetection = new CrashPathDetection();
        }

        /// <summary>
        /// Allows the program to perform any initialization it needs to before starting to run.
        /// This is where we query for any required services and load any non-graphic
        /// related content.
        /// </summary>
        protected override void Initialize()
        {
            //Initializes the Kinect device
            InitializeKinect();
            //Creates the grass and dummy-planes
            InitializeObjects();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of the program's content.
        /// </summary>
        protected override void LoadContent()
        {
            //Load the different resources
            SpriteFont = Content.Load<SpriteFont>("Courier New");
            var airPlaneModel = Content.Load<Model>("airplanemodel");
            var grassModel = Content.Load<Model>("grassTerrain");

            //Set the different models
            Grass.Model = grassModel;
            foreach (Airplane element in AirplaneController.All)
            {
                element.Model = airPlaneModel;
            }

            //Load content of the display class
            Display.LoadDisplay(Grass, SpriteFont);
            //Load content of the kinect video stream
            KinectVideoStream.LoadContent();
            //Load content of the speech recognizer
            SpeechRecognizer.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all of the program's content.
        /// </summary>
        protected override void UnloadContent()
        {
            //Unload the resources used by the speech recognizer
            SpeechRecognizer.UnloadContent();
            //Stop all queued voice feedback and unload VoiceFeedback
            VoiceFeedback.Unload();
        }
        
        /// <summary>
        /// Allows the program to run logic such as updating the world,
        /// movement detection, user input and speech recognition.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            // Remove these after adding to kinect
            if (Keyboard.GetState().IsKeyDown(Keys.F1) && !TestModelPlaneCrashIsToggled)
            {
                TestModelPlaneCrashIsToggled = true;
                CrashPathDetection.ElapsedTime = 18;
                AirplaneController.SetYaw(MathHelper.ToRadians(180+45),AirplaneController.GetPlane("seven"));
                AirplaneController.SetYaw(MathHelper.ToRadians(180-45), AirplaneController.GetPlane("eight"));
            }

            //Update the Display
            Display.Update(gameTime);
            //Updates all the airplanes (the airplane controller takes care of all the planes)
            AirplaneController.Update(gameTime);
            //Checks if we have pressed any of the keys we want to.
            UpdateCameraToDetectUserInput();
            //Updates the camera
            DisplayController.Update();
            //Create crashpath detection
            CrashPathDetection.Update(gameTime);

            if (MovementDetection != null)
            {
                MovementDetection.Update(gameTime);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the program should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Display.Draw(gameTime);
            KinectVideoStream.Draw(gameTime);
            Display.Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            base.Draw(gameTime);
        }

        #region Self-made functions
        private void UpdateCameraToDetectUserInput()
        {
            //Test for camera roation
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad6))
            {
                if (DisplayController.ActiveDisplay.Following)
                {
                    DisplayController.RotateCameraAroundSelectedPlane(0.01f, 0f);
                }
                else
                {
                    DisplayController.RotateCamera(-1f);
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad4))
            {
                if (DisplayController.ActiveDisplay.Following)
                {
                    DisplayController.RotateCameraAroundSelectedPlane(-0.01f, 0f);
                }
                else
                {
                    DisplayController.RotateCamera(1f);
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad2))
            {
                if (DisplayController.ActiveDisplay.Following)
                {
                    DisplayController.RotateCameraAroundSelectedPlane(0f, -0.01f);
                }
                else
                {
                    DisplayController.RotateCamera(vertical: -1f);
                }
                
            }
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad8))
            {
                if (DisplayController.ActiveDisplay.Following)
                {
                    DisplayController.RotateCameraAroundSelectedPlane(0f, 0.01f);
                }
                else
                {
                    DisplayController.RotateCamera(vertical: 1f);
                }
            }
            //Camera translate
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad1))
            {
                DisplayController.TranslateScene(1f);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad3))
            {
                DisplayController.TranslateScene(-1f);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad7) && !DisplayController.ActiveDisplay.Following)
            {
                DisplayController.TranslateScene(0f, 0f, -1f);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad9) && !DisplayController.ActiveDisplay.Following)
            {
                DisplayController.TranslateScene(0f, 0f, 1f);
            }
        }

        private void InitializeObjects()
        {

            Grass = new Grass(new Vector3(0f, 0f, 0f));
            float deg5 = MathHelper.ToRadians(5);
            float deg10 = MathHelper.ToRadians(10);
            float deg15 = MathHelper.ToRadians(15);
            float deg90 = MathHelper.ToRadians(90);
            float deg180 = MathHelper.ToRadians(180);
            
            AirplaneController.Create(new Airplane(Display.Graphics.GraphicsDevice, new Vector3(100f, 270f, -400f), 0f, 0f, "one", 550f));
            AirplaneController.Create(new Airplane(Display.Graphics.GraphicsDevice, new Vector3(500f, 250f, 600f), 0f, -deg90, "two", 800f));
            AirplaneController.Create(new Airplane(Display.Graphics.GraphicsDevice, new Vector3(150f, 300f, -100f), deg5, deg180 * 0.9f, "three", 600f));
            AirplaneController.Create(new Airplane(Display.Graphics.GraphicsDevice, new Vector3(-200f, 150f, 105f), 0f, deg180 * 0.4f, "four", 500f));
            AirplaneController.Create(new Airplane(Display.Graphics.GraphicsDevice, new Vector3(-200f, 100f, -120f), 0.0f, deg180 * 0.7f, "five", 400f));
            AirplaneController.Create(new Airplane(Display.Graphics.GraphicsDevice, new Vector3(-50f, 280f, 190f), deg5, deg180 * 0.3f, "six", 600f));
            AirplaneController.Create(new Airplane(Display.Graphics.GraphicsDevice, new Vector3(400f, 250f, 200f), 0f, deg180, "seven", 600f));
            AirplaneController.Create(new Airplane(Display.Graphics.GraphicsDevice, new Vector3(400f, 250f, 350f), 0f, deg180 , "eight", 600f));
            //AirplaneController.Create(new Airplane(Display.Graphics.GraphicsDevice, new Vector3(600f, 500f, 0f), 0f, deg15, "nine", 500f, fuel: 1150f));
            //AirplaneController.Create(new Airplane(Display.Graphics.GraphicsDevice, new Vector3(500f, 300f, 0f), 0f, -deg90 * 0.9f, "ten", 800f));
            //AirplaneController.Create(new Airplane(Display.Graphics.GraphicsDevice, new Vector3(150f, 700f, 800f), -deg10, -deg90 * 0.6f, "eleven", 600f));
            //AirplaneController.Create(new Airplane(Display.Graphics.GraphicsDevice, new Vector3(-100f, 400f, 105f), 0f, -deg90 * 0.8f, "twelve", 500f));
            //AirplaneController.Create(new Airplane(Display.Graphics.GraphicsDevice, new Vector3(-500f, 360f, -600f), 0.0f, -deg90 * 0.65f, "thirteen", 550f));
            //AirplaneController.Create(new Airplane(Display.Graphics.GraphicsDevice, new Vector3(-500f, 320f, 700f), 0.0f, deg180 *0.7f, "fourteen", 600f));
            //AirplaneController.Create(new Airplane(Display.Graphics.GraphicsDevice, new Vector3(700f, 400f, 700f), 0f, deg90 * 0.5f, "fifteen", 500f));
       
        }

        private void InitializeKinect()
        {
            try
            {
                Kinect = KinectSensor.KinectSensors[0];
                Kinect.SkeletonStream.Enable();
                Kinect.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(MovementDetection.UpdateSkeleton);
                Kinect.Start();
                Kinect.ElevationAngle = 27;
                Kinect.ColorStream.Enable(ColorImageFormat.YuvResolution640x480Fps15);
                Kinect.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(KinectVideoStream.KinectColorFrameReady);
                MovementDetection.SetKinectSensor(Kinect);

            }
            catch (Exception e)
            {
                Console.WriteLine("No Kinect Device found. Please connect a Kinect, and restart.");
            }
        }
        #endregion
    }
}
