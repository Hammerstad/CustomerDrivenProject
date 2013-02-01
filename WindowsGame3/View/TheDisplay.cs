using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WindowsGame3.Controller;
using WindowsGame3.Models;
using System;

namespace WindowsGame3.View
{
    public class TheDisplay
    {
        #region Properties: public attributes
        public GraphicsDeviceManager Graphics { get; private set; }
        public static SpriteBatch SpriteBatch;
        public static SpriteFont SprFont;
        public static string FollowMode = "";
        #endregion

        #region Properties: private attributes
        /// <summary>
        /// The aspect ratio, the ratio between height and width of the screen
        /// </summary>
        private float AspectRatio;

        /// <summary>
        /// The boolean specifying if the possible commands should be displayed
        /// </summary>
        public Boolean ShowPossibleCommands { get; set; }

        /// <summary>
        /// The string specifying which command has been called last and draws
        /// this on the screen.
        /// </summary>
        private string Command ="";

        #endregion

        #region Properties: fps
        /// <summary>
        /// The currently calculated frames per second
        /// </summary>
        private float Fps;

        /// <summary>
        /// Counter for frames, adds one for every time Draw() is called
        /// </summary>
        private float TotalFrames;

        /// <summary>
        /// How long time it has passed since we updated what FPS was.
        /// Sets Fps = TotalFrames;TotalFrames=0 and itself to 0 once
        /// it is above 1000 (ms, 1 sec).
        /// </summary>
        private float ElapsedTime;
        #endregion

        #region Properties: Internal models
        /// <summary>
        /// The grass on the ground
        /// </summary>
        private Grass Grass;

        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor for the display
        /// </summary>
        private static TheDisplay instance;

        private TheDisplay()
        {
            Graphics = new GraphicsDeviceManager(MainProgram.Main);
            Graphics.IsFullScreen = false;
            decimal screenUse = (Graphics.IsFullScreen) ? 1.0m : 0.9m;
            Graphics.PreferredBackBufferHeight = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height * screenUse);
            Graphics.PreferredBackBufferWidth = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width * screenUse);
            ShowPossibleCommands = true;
        }

        public static TheDisplay Instance
        {
            get 
            {
                if (instance == null)
                {
                    instance = new TheDisplay();
                }
                return instance;
            }
        }

        #endregion

        #region Methods: public
        public void LoadDisplay(Grass grass, SpriteFont sprFont)
        {
            Grass = grass;
            SprFont = sprFont;
            SpriteBatch = new SpriteBatch(Graphics.GraphicsDevice);
            Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            AspectRatio = Graphics.GraphicsDevice.Viewport.AspectRatio;
            DisplayController.CreateProjectionMatrix(AspectRatio);
        }

        public void Draw(GameTime gameTime)
        {
            Graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            Grass.Draw();

            // Drawing of sprites
            SpriteBatch.Begin();
            DrawFps();
            DrawPlaneInfo();
            DrawPossibleCommands();
            DrawCommandOnScreen();
            AirplaneController.Draw();
            DrawFollowMode();
            SpriteBatch.End();
        }

        public void Update(GameTime gameTime)
        {

            // Update
            ElapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;



            // 1 Second has passed
            if (ElapsedTime >= 1000.0f)
            {
                Fps = TotalFrames;
                TotalFrames = 0;
                ElapsedTime = 0;
            }

        }

        public void SetCommandToDrawOnScreen(String command)
        {
            Command = command;
        }

        #endregion

        #region Methods: private
        
        private void DrawFps()
        {
            SpriteBatch.DrawString(SprFont, string.Format("FPS={0}", Fps),
                new Vector2(Graphics.PreferredBackBufferWidth-90.0f, 20.0f), Color.White);
            TotalFrames++;
        }

        private void DrawFollowMode()
        {
            SpriteBatch.DrawString(SprFont, FollowMode,
                new Vector2(Graphics.PreferredBackBufferWidth / 2 - SprFont.MeasureString(FollowMode).X/2, 20.0f), Color.White);
        }

        // Draws information about the selected plane to screen.
        private void DrawPlaneInfo()
        {
            Airplane selected = AirplaneController.SelectedPlane;

            string planeInfo = "";
            if (selected != null)
            {
                planeInfo = "Information about plane "
                    + selected.Name + ":"
                    + "\nSpeed:     " + Math.Round(selected.Velocity,0) + " km/h"
                    + "\nFuel left: " + Math.Round(selected.Fuel,0) + " l"
                    + "\nHeight:    " + Math.Round(selected.Position.Y,0)
                    + "\nLongitude: " + Math.Round(selected.Position.X, 0)
                    + "\nLatitude:  " + Math.Round(selected.Position.Z,0)
                    + "\nPitch:     " + Math.Round(MathHelper.ToDegrees(selected.Pitch),0)
                    + "\nHeading:   " + Math.Round(MathHelper.ToDegrees(selected.Yaw), 0);
            }
            else
            {
                planeInfo = "No plane is selected!";
            }
            // The placement of the text on the screen should be changed.
            SpriteBatch.DrawString(SprFont, planeInfo, new Vector2(10.0f, 20.0f), Color.White);
        }

        // Draws the possible commands to screen
        private void DrawPossibleCommands()
        {
            Airplane selected = AirplaneController.SelectedPlane;
            string possibleComands;
            if (ShowPossibleCommands)
            {
                possibleComands = "The possible commands are: ";
                if (selected != null)
                {
                    possibleComands
                        += "\n'Alter heading <N/S/E/W/Angle>' "
                        + "\n'Alter pitch <Angle>' "
                        + "\n'Alter speed <Speed>' "
                        + "\n'Select <planeName>'"
                        + "\n'Deselect'"
                        + "\n'Go to'"
                        + "\n" + ((DisplayController.ActiveDisplay.Following == true) ? "'Unfollow'" : "'Follow'");
                }
                else
                {
                    possibleComands += "\n'Select <planeName>' ";
                }
                possibleComands += "\n'Toggle names/coordinates/lines" + "\n'Toggle help/voice(feedback)' ";
            }
            else
            {
                possibleComands = "Say 'Toggle help' to see possible commads";
            }
            // The placement of the text on the screen should be changed.
            SpriteBatch.DrawString(SprFont, possibleComands, 
                new Vector2(10.0f, Graphics.PreferredBackBufferHeight - 100f - SprFont.MeasureString(possibleComands).Y), Color.White);
        }

        private void DrawCommandOnScreen()
        {
            SpriteBatch.DrawString(SprFont, Command,
                new Vector2(10.0f, Graphics.PreferredBackBufferHeight - 40f), Color.White);
        }
        #endregion
    }
}
