using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsGame3.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ProjectTests")]
namespace WindowsGame3.Controller
{
    class CrashPathDetection
    {
        #region Private variables

        /// <summary>
        /// These constan floats specifying the minimum distance between object before it is called a crash
        /// </summary>
        private const float CrashDistanceBetweenPlanes = 30f;
        private const float CrashDistanceToGround = 50f;

        /// <summary>
        /// These constan floats specifying timing variables
        /// </summary>
        private const int SecondsInFutureToCheck = 60 * 5;
        private const int CrashCheckIntervall = 30;
        
        /// <summary>
        /// The elapsed time since last crash detection check
        /// </summary>
        public double ElapsedTime;

        /// <summary>
        /// List of the airplanes we are going to check.
        /// </summary>
        private static List<Airplane> Airplanes = null;

        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor for the CrashPathDetection
        /// </summary>
        public CrashPathDetection() 
        {
            ElapsedTime = CrashCheckIntervall;
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Starts a crashdetection test every 'CrashCheckIntervall' second
        /// <param gameTime="gameTime">Time since last frame</param>
        /// </summary>
        public void Update(GameTime gameTime)
        {
            ElapsedTime += (double)gameTime.ElapsedGameTime.TotalSeconds;
            if (ElapsedTime >= CrashCheckIntervall)
            {
                ElapsedTime = 0;
                CheckCollisionPathAllPlanes();
            }

        }

        /// <summary>
        /// Starts all crashdetection test for every plane
        /// </summary>
        public static void CheckCollisionPathAllPlanes()
        {
            // Get the list of all airplanes
            Airplanes = AirplaneController.All;
            String feedback;
            // Check collision between all pair of planes
            for (int i = 0; i < Airplanes.Count - 1; i++)
            {
                for (int j = i + 1; j < Airplanes.Count; j++)
                {
                    feedback = CheckCollisionPathBetweenTwoPlanes(Airplanes.ElementAt(i), Airplanes.ElementAt(j));
                    if (feedback != null)
                        FeedbackHandler.HandleFeedback(feedback);
                }
            }

            // Check collison against the ground
            for (int i = 0; i < Airplanes.Count; i++)
            {
                feedback = CheckCollisionPathPlaneAgainstGround(Airplanes.ElementAt(i));
                if (feedback != null)
                    FeedbackHandler.HandleFeedback(feedback);
            }
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Check that the plane is not heading into the ground
        /// <param name="plane">The plane to check</param>
        /// </summary>
        internal static String CheckCollisionPathPlaneAgainstGround(Airplane plane)
        {
            float VelY = plane.DirectionalVelocity.Y;
            // If this the plane is going up, and not going to crash
            if (VelY >= 0)
                return null;

            float Height = plane.Position.Y;
            
            for (int t = 0; t < SecondsInFutureToCheck; t += 5)
            {
                float future = (Height + (VelY * t / 1000));

                if (future < CrashDistanceToGround)
                {
                    return "Warning: Plane " + plane.Name + " is going to hit the ground in less than " + (int)(1 + t / 60) + " minute" + (((int)(1 + t / 60) == 1) ? "" : "s");
                }
            }
            return null;
        }

        /// <summary>
        /// Check that the planes is not going to crash in the future
        /// <param name="plane1">The first plane</param>
        /// <param name="plane2">The second plane</param>
        /// </summary>
        internal static String CheckCollisionPathBetweenTwoPlanes(Airplane plane1, Airplane plane2)
        {
            for (int t = 0; t < SecondsInFutureToCheck; t += 5)
            {
                Vector3 fut1 = (plane1.Position + (plane1.DirectionalVelocity * t / 1000));
                Vector3 fut2 = (plane2.Position + (plane2.DirectionalVelocity * t) / 1000);
                Vector3 dif = fut1 - fut2;
                float distance = (float)Math.Round((Math.Sqrt(Math.Pow(dif.X, 2) + Math.Pow(dif.Y, 2) + Math.Pow(dif.Z, 2))), 4);
                if (distance < CrashDistanceBetweenPlanes)
                {
                    return "Warning: Plane " + plane1.Name + " and plane " + plane2.Name + " will crash in less than " + (int)(1 + t / 60) + " minute" + (((int)(1 + t / 60) == 1) ? "" : "s");
                } 
            }
            return null;
        }

        #endregion

    }
}
