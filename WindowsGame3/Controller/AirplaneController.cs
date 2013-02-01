using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using WindowsGame3.Models;

namespace WindowsGame3.Controller
{
    class AirplaneController
    {
        #region Properties
        /// <summary>
        /// A list containing all the airplanes. Should not be accessed explicitly, use "All" instead.
        /// </summary>
        private static List<Airplane> Airplanes = new List<Airplane>();

        /// <summary>
        /// A list containing all the airplanes.
        /// </summary>
        public static List<Airplane> All { private set { Airplanes = value; } get { return Airplanes; } }

        /// <summary>
        /// The currently selected airplane. This should not be accessed explicitly, used "SelectedPlane" instead.
        /// </summary>
        private static Airplane PrivateSelectedPlane;

        /// <summary>
        /// The plane that should be removed from the All list, after other operation are finished
        /// </summary>
        private static Airplane PlaneToRemoveFromAll = null;

        /// <summary>
        /// The currently selected airplane.
        /// </summary>
        public static Airplane SelectedPlane
        {
            set
            {
                // Makes sure only one airplane is selected and unfollows the selected plane if it was followed.
                if (PrivateSelectedPlane != null)
                {
                    PrivateSelectedPlane.IsSelected = false;
                    DisplayController.Unfollow(false);
                }
                if (value != null)
                {
                    value.IsSelected = true;
                    FeedbackHandler.HandleFeedback("Plane " + value.Name + " is selected");
                }
                PrivateSelectedPlane = value;
            }
            get { return PrivateSelectedPlane; }
        }

        /// <summary>
        /// Whether or not to display the lines below the airplanes.
        /// </summary>
        public static Boolean ToggleLineUnderPlanes = false;

        /// <summary>
        /// Whether or not to display the names below the airplanes.
        /// </summary>
        public static Boolean ToggleNames = true;

        /// <summary>
        /// Whether or not to display the coordinates below the airplanes.
        /// </summary>
        public static Boolean ToggleCoordinates = true;

        #endregion

        #region Methods
        /// <summary>
        /// Selects an airplane by name.
        /// </summary>
        /// <param name="name">Name of the airplane</param>
        public static void SelectPlaneByName(String name)
        {
            foreach (var airplane in All.Where(airplane => airplane.Name.Equals(name)))
            {
                SelectedPlane = airplane;
                return;
            }

            FeedbackHandler.HandleFeedback("Plane "+ name +" not found");
        }

        /// <summary>
        /// Creates a new airplane and adds it to the list of airplanes.
        /// </summary>
        /// <param name="airplane">A new airplane</param>
        /// <returns>The new airplane</returns>
        public static Airplane Create(Airplane airplane)
        {
            if (All == null)
            {
                All = new List<Airplane>();
            }
            All.Add(airplane);
            return airplane;
        }

        /// <summary>
        /// Gets an airplane by its name.
        /// </summary>
        /// <param name="name">Name of the airplane</param>
        /// <returns>The airplane</returns>
        public static Airplane GetPlane(String name)
        {
            return All.Find(airplane => airplane.Name == name);
        }

        /// <summary>
        /// Draws all the airplanes.
        /// </summary>
        public static void Draw()
        {
            foreach (Airplane plane in All)
            {
                plane.Draw();
            }
        }
        
        /// <summary>
        /// Updates all the airplanes.
        /// </summary>
        /// <param name="gameTime">Gametime gameTime</param>
        public static void Update(GameTime gameTime)
        {
            foreach (Airplane plane in All)
            {
                plane.Update(gameTime);
                if (plane.Fuel <= 0f)
                {
                    if (!plane.ShowedFuelEmptyWarning)
                    {
                        plane.ShowedFuelEmptyWarning = true;
                        FeedbackHandler.HandleFeedback("Warning: Fuel is empty in plane " + plane.Name + ". The plane is falling");
                    }
                    SetPitch(-30f, plane, true);
                }
                if (!plane.ShowedFuelLowWarning && plane.Fuel < 1000.0)
                {
                    plane.ShowedFuelLowWarning = true;
                    FeedbackHandler.HandleFeedback("Warning: Fuel is getting low in plane " + plane.Name);
                }
                if (plane.Position.Y <= 0)
                {
                    PlaneToRemoveFromAll = plane;
                }
            }
            if (PlaneToRemoveFromAll != null)
            {
                All.Remove(PlaneToRemoveFromAll);
            }
        }

        /// <summary>
        /// Sets the pitch of an airplane. If the plane argument is omited, SelectedAirplane will be used.
        /// </summary>
        /// <param name="pitch">The pitch in radians.</param>
        /// <param name="plane">Optional plane.</param>
        /// <param name="muteVoice">Whether or not to mute the voice command.</param>
        public static void SetPitch(float pitch, Airplane plane = null, Boolean muteVoice = false)
        {
            if (plane == null)
            {
                plane = SelectedPlane;
            }
            if (plane == null)
            {
                return;
            }
            pitch = Math.Min(Airplane.MaxPlanePitch, Math.Max(pitch, -Airplane.MaxPlanePitch));//Prevents the plane from making a loop.

            if (!muteVoice)
            {
                FeedbackHandler.HandleFeedback("Altering the pitch of plane " + plane.Name + " to " + Math.Round(MathHelper.ToDegrees(pitch), 0) + " degrees");
            }
            SetRotation(pitch, plane.Yaw, false, plane);
        }

        /// <summary>
        /// Sets the rotation of an airplane. if the plane argument is omited, SelectedAirplane will be used.
        /// </summary>
        /// <param name="yaw">The yaw in radians.</param>
        /// <param name="plane">Optional plane.</param>
        /// <param name="muteVoice">Whether or not to mute the voice command.</param>
        public static void SetYaw(float yaw, Airplane plane = null, Boolean muteVoice = false)
        {
            if (plane == null)
            {
                plane = SelectedPlane;
            }
            if (plane == null)
            {
                return;
            }
            if (!muteVoice)
            {
                FeedbackHandler.HandleFeedback("Altering the heading of plane " + plane.Name + " to " + Math.Round(MathHelper.ToDegrees(yaw), 0) + " degrees");
            }
            SetRotation(plane.Pitch, yaw, false, plane);
        }

        /// <summary>
        /// Changes the rotation of the plane around its own axis.
        /// </summary>
        /// <param name="pitch">Pitch in radians.</param>
        /// <param name="yaw">Yaw in radians.</param>
        public static void SetRelativeRotation(float pitch, float yaw, Airplane plane = null)
        {
            if (plane == null)
            {
                plane = SelectedPlane;
            }
            if (plane == null)
            {
                return;
            }
            SetRotation(plane.Pitch + pitch, plane.Yaw + yaw, true, plane);
        }

        /// <summary>
        /// Sets the rotation of an airplane. If the plane argument is omited, SelectedAirplane will be used.
        /// </summary>
        /// <param name="pitch">The pitch in radians.</param>
        /// <param name="yaw">The yaw in radians.</param>
        /// <param name="turnsInstant">Whether to turn instantly or not.</param>
        /// <param name="plane">Optional plane.</param>
        public static void SetRotation(float pitch = 0f, float yaw = 0f, Boolean turnsInstant = false, Airplane plane = null)
        {
            if (plane == null)
            {
                plane = SelectedPlane;
            }
            if (plane == null)
            {
                return;
            }

            plane.Yaw = (plane.Yaw + 2 * MathHelper.Pi) % (2 * MathHelper.Pi);
            yaw = (yaw + 2 * MathHelper.Pi) % (2 * MathHelper.Pi);
            pitch = Math.Min(Airplane.MaxPlanePitch, Math.Max(pitch, -Airplane.MaxPlanePitch));//Prevents the plane from making a loop.

            if (turnsInstant)
            {
                plane.Pitch = pitch;
                plane.Yaw = yaw;
                plane.UpdateModelRotationAndScaleMatrix();
                plane.UpdateDirectionalVelocity();
            }
            else
            {
                if (plane.Yaw != yaw)
                {
                    plane.RadiansToTurn = yaw - plane.Yaw;
                    if (plane.RadiansToTurn > MathHelper.Pi)
                    {
                        plane.RadiansToTurn -= 2 * MathHelper.Pi;
                    }
                    else if (plane.RadiansToTurn < -MathHelper.Pi)
                    {
                        plane.RadiansToTurn += 2 * MathHelper.Pi;
                    }
                    plane.TurningRight = (plane.RadiansToTurn > 0);
                    plane.RadiansToTurn = Math.Abs(plane.RadiansToTurn);
                    plane.RadiansToTurnHalfPoint = plane.RadiansToTurn / 2;
                }
                if (plane.Pitch != pitch)
                {
                    plane.RadiansToTurnUp = pitch - plane.Pitch;
                    plane.TurningUp = (plane.RadiansToTurnUp > 0);
                    plane.RadiansToTurnUp = Math.Abs(plane.RadiansToTurnUp);
                }
            }
        }

        /// <summary>
        /// Sets the velocity of the airplane. If the plane argument is omited, SelectedAirplane will be used.
        /// </summary>
        /// <param name="velocity">The velocity in km/h.</param>
        /// <param name="plane">Optional airplane.</param>
        public static void SetVelocity(float velocity, Airplane plane = null)
        {
            if (plane == null)
            {
                plane = SelectedPlane;
            }
            if (plane == null)
            {
                return;
            }
            plane.NewVelocity = velocity;
            plane.VelocityUp = velocity > plane.Velocity;
            FeedbackHandler.HandleFeedback("Altering the speed of plane "+ plane.Name +" to " + velocity + " km per hour");
        }

        public static void Delete(Airplane plane = null, String name = "")
        {
            if (plane == null)
            {
                plane = GetPlane(name);
            }
            if (plane == null)
            {
                plane = SelectedPlane;
            }
            if (plane == null)
            {
                return;
            }
            if (plane == SelectedPlane)
            {
                SelectedPlane = null;
            }
            All.Remove(plane);
        }
        #endregion
    }
}
