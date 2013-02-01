using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Kinect;
using WindowsGame3.View;

namespace WindowsGame3.Controller
{
    public class KinectVideoStream
    {
        #region Properties: VideoStream Variables
        Texture2D ColorVideo;
        int[] VideoStreamPosition;
        int[] VideoStreamSize;
        private ColorImageFrame ColorVideoFrame;
        #endregion

        #region Properties: High-level variables
        private readonly Color Color;
        private GraphicsDeviceManager Graphics;
        private MainProgram Main;
        #endregion

        #region Properties: Black-line variables
        private Rectangle VideoScreenSize;
        #endregion

        #region Properties: Converting video format variables
        private Byte[] BgraPixelData, PixelData;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs a new KinectViceoStream
        /// </summary>
        public KinectVideoStream()
        {
            Main = MainProgram.Main;
            Graphics = Main.Display.Graphics;
            int screenWidth = Graphics.PreferredBackBufferWidth;
            int screenHeight = Graphics.PreferredBackBufferHeight;
            int width = (int)(screenWidth * 0.225f);
            int height = (int)(width * 3.0 / 4);
            int x = (int)(screenWidth * 0.98f - width);
            int y = (int)(screenHeight * 0.98f - height );
            VideoStreamPosition = new int[] { x, y };
            VideoStreamSize = new int[] { width, height };
            Color = Color.White;
        }
        #endregion

        #region Methods: public
        /// <summary>
        /// Loads the content required in KinectVideoStream.
        /// </summary>
        public void LoadContent()
        {
            VideoScreenSize = new Rectangle(VideoStreamPosition[0], VideoStreamPosition[1], VideoStreamSize[0],
                                            VideoStreamSize[1]);
        }

        /// <summary>
        /// Draws the KinectVideoStream in the bottom right corner of the screen.
        /// </summary>
        /// <param name="gameTime">GameTime gameTime</param>
        public void Draw(GameTime gameTime)
        {
            if (ColorVideo == null) 
                return;

            //Begin the spriteBatch
            TheDisplay.SpriteBatch.Begin();
            //Draw the KinectVideoScreen
            TheDisplay.SpriteBatch.Draw(ColorVideo, VideoScreenSize, Color);
            
            TheDisplay.SpriteBatch.End();
        }

        /// <summary>
        /// Sets the data of ColorVideo.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="colorImageFrame"></param>
        public void KinectColorFrameReady(object sender, ColorImageFrameReadyEventArgs colorImageFrame)
        {
            //Get raw image
            ColorVideoFrame = colorImageFrame.OpenColorImageFrame();

            if (ColorVideoFrame != null)
            {
                //Create array for pixel data and copy it from the image frame
                PixelData = new Byte[ColorVideoFrame.PixelDataLength];
                ColorVideoFrame.CopyPixelDataTo(PixelData);

                //Convert RGBA to BGRA, Kinect and XNA uses different color-formats.
                BgraPixelData = new Byte[ColorVideoFrame.PixelDataLength];
                for (int i = 0; i < PixelData.Length; i += 4)
                {
                    BgraPixelData[i] = PixelData[i + 2];
                    BgraPixelData[i + 1] = PixelData[i + 1];
                    BgraPixelData[i + 2] = PixelData[i];
                    BgraPixelData[i + 3] = (Byte)255; //The video comes with 0 alpha so it is transparent
                }

                // Create a texture and assign the realigned pixels
                ColorVideo = new Texture2D(Graphics.GraphicsDevice, ColorVideoFrame.Width, ColorVideoFrame.Height);
                ColorVideo.SetData(BgraPixelData);
                ColorVideoFrame.Dispose();
            }
        }
        #endregion
    }
}
