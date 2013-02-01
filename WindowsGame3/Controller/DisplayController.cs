using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using WindowsGame3.Models;
using WindowsGame3.View;

[assembly: InternalsVisibleTo("ProjectTests")]
namespace WindowsGame3.Controller
{
    public class DisplayController
    {
        #region Properties: public static
        /// <summary>
        /// The camera, accessible from outside the class
        /// </summary>
        public static DisplayController ActiveDisplay;

        /// <summary>
        /// The Projection matrix
        /// </summary>
        public static Matrix Projection;

        /// <summary>
        /// The View matrix
        /// </summary>
        public static Matrix View;

        /// <summary>
        /// The World matrix
        /// </summary>
        public static Matrix World;

        #endregion

        #region Properties: private

        /// <summary>
        /// How far you are able to see upwards. In radians.
        /// </summary>
        private const float MaximumCameraPitch = 0.20f;

        /// <summary>
        /// How far you are able to see downwards. In radians.
        /// </summary>
        private const float MinimumCameraPitch = -0.50f;

        /// <summary>
        /// The camera can not go below this threshold
        /// </summary>
        private const int CameraHeightThreshold = 125;

        /// <summary>
        /// The current position of the camera
        /// </summary>
        private Vector3 CameraPosition { get; set; }

        /// <summary>
        /// The current position of the camera
        /// </summary>
        private const float MinimumFollowingDistance = 25f;

        /// <summary>
        /// The current position of the object the camera is looking at
        /// </summary>
        private Vector3 LookAt { get; set; }

        /// <summary>
        /// Which direction is up according to the camera
        /// </summary>
        private Vector3 Up;

        /// <summary>
        /// Which direction is forward according to the camera
        /// </summary>
        private Vector3 Forward;

        /// <summary>
        /// Which direction is right according to the camera
        /// </summary>
        private Vector3 Right;

        /// <summary>
        /// Matrix used for temporary calculations with the rotation
        /// </summary>
        private Matrix RotationMatrixX;

        /// <summary>
        /// Matrix used for temporary calculations with the rotation
        /// </summary>
        private Matrix RotationMatrixY;

        /// <summary>
        /// Vector used for temporary calculation
        /// </summary>
        private Vector3 CameraReference;

        /// <summary>
        /// Vector used for temporary calculation
        /// </summary>
        private Vector3 TransformedReference;

        #endregion

        #region Properties: Follow Airplane

        /// <summary>
        /// Whether we are following a plane or not
        /// </summary>
        public Boolean Following = false;

        /// <summary>
        /// Whether we are closing in on the plane or not
        /// </summary>
        private Boolean ClosingIn = false;

        /// <summary>
        /// Whether we are turning towards a plane or not
        /// </summary>
        private Boolean Turning = false;

        /// <summary>
        /// Which plane we are following
        /// </summary>
        private Airplane FollowingAirplane = null;

        /// <summary>
        /// Vector used for temporary calculations
        /// </summary>
        private Vector3 DifferenceBetweenLookAtAndObjectInRotation = Vector3.Zero;

        /// <summary>
        /// Vector used for temporary calculations
        /// </summary>
        private Vector3 DifferenceBetweenLookAtAndObjectInDistance = Vector3.Zero;

        /// <summary>
        /// Vector used for temporary calculations
        /// </summary>
        private Vector3 DifferencePerTic = Vector3.Zero;

        /// <summary>
        /// The old position of the plane
        /// </summary>
        private Vector3 OldPlanePosition = Vector3.Zero;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor for Camera class.
        /// </summary>
        /// <param name="x">X-position of Camera.</param>
        /// <param name="y">Y-position of Camera.</param>
        /// <param name="z">Z-position of Camera.</param>
        /// <param name="lookAtX">X-position of where the Camera looks.</param>
        /// <param name="lookAtY">Y-position of where the Camera looks.</param>
        /// <param name="lookAtZ">Z-position of where the Camera looks.</param>
        public DisplayController(float x = 0f, float y = 0f, float z = 0f, float lookAtX = 0f, float lookAtY = 0f, float lookAtZ = 0f)
        {
            this.CameraPosition = new Vector3(x, y, z);
            this.LookAt = new Vector3(lookAtX, lookAtY, lookAtZ);
            CalculateVectors();
            ActiveDisplay = this;
        }
        #endregion

        #region Methods: public static
        /// <summary>
        /// Creates the Projection matrix. Must be called after Display.LoadContent()
        /// </summary>
        public static void CreateProjectionMatrix(float aspectRatio)
        {
            //Must be called after Display.LoadContent() for AspectRatio to be != 0
            Projection = Matrix.CreatePerspectiveFieldOfView(
                       MathHelper.ToRadians(45.0f), aspectRatio,
                       1.0f, 10000.0f);
        }

        /// <summary>
        /// Translates the scene according to input.
        /// </summary>
        /// <param name="translateForward">Moves the camera forward.</param>
        /// <param name="translateUp">Moves the camera up (see param: localUp).</param>
        /// <param name="translateRight">Moves the camera to the right.</param>
        public static void TranslateScene(float translateForward = 0f, float translateUp = 0f, float translateRight = 0f)
        {
            // Make sure camera does not get too close in follow mode
            if (translateForward > 0 && ActiveDisplay.Following)
            {
                Vector3 difference = ActiveDisplay.CameraPosition - AirplaneController.SelectedPlane.Position;
                float distance = (float)Math.Round((Math.Sqrt(Math.Pow(difference.X, 2) + Math.Pow(difference.Y, 2) + Math.Pow(difference.Z, 2))),4);
                if (distance < MinimumFollowingDistance)
                    return;
            }

            //Calculate the camera vector, normalized.
            ActiveDisplay.CameraReference = ActiveDisplay.CameraPosition - ActiveDisplay.LookAt;
            ActiveDisplay.CameraReference = ActiveDisplay.CameraReference / ActiveDisplay.CameraReference.Length();
            ActiveDisplay.TransformedReference = ActiveDisplay.CameraReference * (translateForward * -1) + translateRight * ActiveDisplay.Right + translateUp * ActiveDisplay.Up;
            // Add the transformed vector to the position we are looking at and the position of the camera.
            if (ActiveDisplay.CameraPosition.Y + ActiveDisplay.TransformedReference.Y < CameraHeightThreshold)
            {
                ActiveDisplay.TransformedReference = new Vector3(ActiveDisplay.TransformedReference.X, 0, ActiveDisplay.TransformedReference.Z);
            }
            ActiveDisplay.LookAt += ActiveDisplay.TransformedReference;
            ActiveDisplay.CameraPosition += ActiveDisplay.TransformedReference;
            ActiveDisplay.AvoidRoundingErrors();
        }
        /// <summary>
        /// Translates the scene to look at given position
        /// </summary>
        /// <param name="positionToLookAt">Change the camera LookAt to positionToLookAt, and moves camera close to positionToLookAt.</param>
        public static void GoTo(Vector3 positionToLookAt)
        {
            //Calculate the camera vector, normalized.
            ActiveDisplay.CameraReference = ActiveDisplay.CameraPosition - ActiveDisplay.LookAt;
            ActiveDisplay.CameraReference = ActiveDisplay.CameraReference / ActiveDisplay.CameraReference.Length();
            ActiveDisplay.CameraPosition = positionToLookAt + ActiveDisplay.CameraReference * 200;
            ActiveDisplay.LookAt = positionToLookAt;
            ActiveDisplay.AvoidRoundingErrors();
        }

        /// <summary>
        /// Translates around the selected plane in follow mode
        /// </summary>
        /// <param name="incAngleHor">The amount the angle will be decreased</param>
        public static void RotateCameraAroundSelectedPlane(float incAngleHor, float incAngleVer)
        {
            if (!ActiveDisplay.Following)
                return;

            // Calculate distance between camera and selcted plane
            ActiveDisplay.CameraReference = ActiveDisplay.CameraPosition - AirplaneController.SelectedPlane.Position;
            float distance = (float)Math.Round((Math.Sqrt(Math.Pow(ActiveDisplay.CameraReference.X, 2) + Math.Pow(ActiveDisplay.CameraReference.Y, 2) + Math.Pow(ActiveDisplay.CameraReference.Z, 2))),4);

            // Calculate pitch angle, and increase it
            float followingPitch = (float)Math.Asin(ActiveDisplay.CameraReference.Y / distance) + incAngleVer;
            followingPitch = (float)Math.Round(Math.Min(MathHelper.Pi / 4, Math.Max(followingPitch, -MathHelper.Pi / 4)),4);

            // Calculate horizonotal distance
            float distanceHor = (float)Math.Round(distance * Math.Cos(followingPitch),4);

            // Caluclate heading angle, and increase it
            float followingHeading;
            if (ActiveDisplay.CameraReference.Z <= 0)
            {
                followingHeading = (float)Math.Round((Math.Acos(ActiveDisplay.CameraReference.X / distanceHor) + incAngleHor),4);
            }
            else
            {
                followingHeading = (float)Math.Round((-Math.PI + Math.Acos(-ActiveDisplay.CameraReference.X / distanceHor) + incAngleHor), 4);
            }
            
            // Calculate new positions of the camera
            Vector3 incPos = Vector3.Zero;
            incPos.Y = (float)Math.Round((float)(distance * Math.Sin(followingPitch)), 4);
            incPos.X = (float)Math.Round((float)distanceHor * Math.Cos(followingHeading), 4);
            incPos.Z = (float)Math.Round((float)-distanceHor * Math.Sin(followingHeading), 4);

            // Make sure there is no Not an Number error for the doubles. That can happend in extreme cases(dividing by zero)
            if (double.IsNaN(incPos.X) ||double.IsNaN(incPos.Y) ||double.IsNaN(incPos.Z))
            {
                return;
            }

            // Update camera
            ActiveDisplay.CameraPosition = AirplaneController.SelectedPlane.Position + incPos;
            ActiveDisplay.CalculateVectors();
        }

        /// <summary>
        /// Rotate the camera around its own axis.
        /// </summary>
        /// <param name="horizontal">Rotate around the x-axis (turn your head up/down).</param>
        /// <param name="vertical">Rotate around the y-axis (turn your head to the sides).</param>
        public static void RotateCamera(float horizontal = 0f, float vertical = 0f)
        {
            Vector3 temp = ActiveDisplay.LookAt;
            ActiveDisplay.RotationMatrixX = Matrix.CreateFromAxisAngle(ActiveDisplay.Up, MathHelper.ToRadians(horizontal));
            ActiveDisplay.RotationMatrixY = Matrix.CreateFromAxisAngle(ActiveDisplay.Right, MathHelper.ToRadians(vertical));
            //Calculate X-axis
            ActiveDisplay.CameraReference = ActiveDisplay.CameraPosition - ActiveDisplay.LookAt;
            ActiveDisplay.CameraReference = ActiveDisplay.CameraReference / ActiveDisplay.CameraReference.Length();
            ActiveDisplay.TransformedReference = Vector3.Transform(ActiveDisplay.CameraReference, ActiveDisplay.RotationMatrixX);
            ActiveDisplay.LookAt = ActiveDisplay.CameraPosition + ActiveDisplay.TransformedReference;
            //Calculate Y-axis
            ActiveDisplay.CameraReference = ActiveDisplay.CameraPosition - ActiveDisplay.LookAt;
            ActiveDisplay.CameraReference = ActiveDisplay.CameraReference / ActiveDisplay.CameraReference.Length();
            ActiveDisplay.TransformedReference = Vector3.Transform(ActiveDisplay.CameraReference, ActiveDisplay.RotationMatrixY);


            //Make sure the camera doesn't tilt
            if (ActiveDisplay.TransformedReference.Y > MinimumCameraPitch && ActiveDisplay.TransformedReference.Y < MaximumCameraPitch)
            {
                ActiveDisplay.LookAt = ActiveDisplay.CameraPosition + ActiveDisplay.TransformedReference;
            }
            else
            {
                ActiveDisplay.LookAt = temp;
            }
            ActiveDisplay.CalculateVectors();
        }

        /// <summary>
        /// Updates the camera. Creates a new view matrix and follows a plane
        /// if it is supposed to.
        /// </summary>
        public static void Update()
        {
            View = Matrix.CreateLookAt(ActiveDisplay.CameraPosition, ActiveDisplay.LookAt, Vector3.Up);
            ActiveDisplay.UpdateFollow();
        }

        /// <summary>
        /// Follows AirplaneController.SelectedPlane if it is not null.
        /// </summary>
        public static void Follow()
        {
            if (AirplaneController.SelectedPlane == null || ActiveDisplay.Following) return;
            ActiveDisplay.FollowingAirplane = AirplaneController.SelectedPlane;
            ActiveDisplay.Turning = true;
            ActiveDisplay.ClosingIn = true;
            ActiveDisplay.Following = true;
            ActiveDisplay.DifferenceBetweenLookAtAndObjectInRotation = Vector3.Zero;
            ActiveDisplay.DifferenceBetweenLookAtAndObjectInDistance = Vector3.Zero;
            ActiveDisplay.DifferencePerTic = Vector3.Zero;
            ActiveDisplay.OldPlanePosition = Vector3.Zero;
            FeedbackHandler.HandleFeedback("Now following plane " + AirplaneController.SelectedPlane.Name);
            TheDisplay.FollowMode = "Following plane " + ActiveDisplay.FollowingAirplane.Name;
        }

        /// <summary>
        /// Stops following a plane.
        /// </summary>
        /// <param name="notify">Whether or not to notify that we unfollow. Not necessary when we select another plane.</param>
        public static void Unfollow(Boolean notify = true)
        {
            if (ActiveDisplay.Following && notify)
                 FeedbackHandler.HandleFeedback("Stopped following plane.");

            ActiveDisplay.Turning = false;
            ActiveDisplay.ClosingIn = false;
            ActiveDisplay.Following = false;
            ActiveDisplay.DifferenceBetweenLookAtAndObjectInRotation = Vector3.Zero;
            ActiveDisplay.DifferenceBetweenLookAtAndObjectInDistance = Vector3.Zero;
            ActiveDisplay.DifferencePerTic = Vector3.Zero;
            ActiveDisplay.OldPlanePosition = Vector3.Zero;
            ActiveDisplay.FollowingAirplane = null;
            
            TheDisplay.FollowMode = "";
        }

        #endregion

        #region Methods: private
        /// <summary>
        /// This method calculates the camera's forward, right and up vector. 
        /// Does not work if the camera can tilt sideways (roll).
        /// </summary>
        private void CalculateVectors()
        {
            Forward = Vector3.Normalize(LookAt - CameraPosition);
            Right = Vector3.Normalize(Vector3.Cross(Forward, Vector3.Up));
            Up = Vector3.Normalize(Vector3.Cross(Right, Forward));
            AvoidRoundingErrors();
        }

        /// <summary>
        /// A method for avoiding rounding errors (LookAt and CameraPosition).
        /// </summary>
        private void AvoidRoundingErrors()
        {
            // Round of LookAt's members to 3 significant digits
            this.LookAt = new Vector3((float)Math.Round(LookAt.X, 3), (float)Math.Round(LookAt.Y, 3), (float)Math.Round(LookAt.Z, 3));

            // Round of CameraPosition's members to 3 significant digits
            this.CameraPosition = new Vector3((float)Math.Round(CameraPosition.X, 3), (float)Math.Round(CameraPosition.Y, 3), (float)Math.Round(CameraPosition.Z, 3));
        }

        /// <summary>
        /// Continues to follow the airplane
        /// </summary>
        private void UpdateFollow()
        {
            //If we are not following, return
            if (!Following) return;

            //If we are still turning towards the plane, turn towards it
            if (Turning)
            {
                DifferenceBetweenLookAtAndObjectInRotation = Vector3.Subtract(FollowingAirplane.Position, LookAt);
                DifferenceBetweenLookAtAndObjectInRotation = Vector3.Divide(DifferenceBetweenLookAtAndObjectInRotation, 50);
                LookAt = Vector3.Add(LookAt, DifferenceBetweenLookAtAndObjectInRotation);
                if (DifferenceBetweenLookAtAndObjectInRotation.Length() < 0.05f)
                {
                    Turning = false;
                }
            }

            //We are not turning, but still following. Make sure we are looking at the plane all the time
            if (!Turning && Following)
            {
                LookAt = FollowingAirplane.Position;
            }

            //We haven't come as close as we wanted. Let's continue to close in.
            if (ClosingIn)
            {
                DifferenceBetweenLookAtAndObjectInDistance = Vector3.Subtract(CameraPosition, FollowingAirplane.Position);
                DifferenceBetweenLookAtAndObjectInDistance = Vector3.Divide(DifferenceBetweenLookAtAndObjectInDistance, 100);
                CameraPosition = Vector3.Subtract(CameraPosition, DifferenceBetweenLookAtAndObjectInDistance);
                if (DifferenceBetweenLookAtAndObjectInDistance.Length() < 1f)
                {
                    ClosingIn = false;
                }
            }

            //We are not closing in anymore, but still following - in other words, just following. Make sure we are!
            if (!ClosingIn && Following)
            {
                DifferencePerTic = Vector3.Subtract(FollowingAirplane.Position, OldPlanePosition);
                CameraPosition = Vector3.Add(CameraPosition, DifferencePerTic);
            }
            OldPlanePosition = FollowingAirplane.Position;
        }
        #endregion
    }
}
