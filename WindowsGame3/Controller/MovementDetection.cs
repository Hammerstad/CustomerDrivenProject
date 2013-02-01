using System;

using Microsoft.Xna.Framework;
using Microsoft.Kinect;

namespace WindowsGame3.Controller
{
    public class MovementDetection
    {
        private static MovementDetection Active = null;
       
        /// <summary>
        /// Last position of your right hand. Used to calculate the difference.
        /// </summary>
        private Vector3 LastPositionHandRight = Vector3.Zero;

        /// <summary>
        /// Last position of your left hand. Used to calculate the difference.
        /// </summary>
        private Vector3 LastPositionHandLeft = Vector3.Zero;

        /// <summary>
        /// Factor when deciding how fast we rotate the camera sideways.
        /// </summary>
        private float RotationSpeedSideways = 75f;

        /// <summary>
        /// Factor when deciding how fast we rotate the camera upwards.
        /// </summary>
        private float RotationSpeedUpDown = 50f;

        /// <summary>
        /// Factor when deciding how fast we move the camera forwards.
        /// </summary>
        private float MovingSpeedForwards = 400f;

        /// <summary>
        /// Factor when deciding how fast we move the camera sideways.
        /// </summary>
        private float MovingSpeedSideways = 400f;
        
        /// <summary>
        /// Threshold to deciding if difference in hand position is to big relative to last tracked state
        /// </summary>
        public float HandJumpThreshold = MathHelper.ToRadians(10);

        /// <summary>
        /// The KinectSensor
        /// </summary>
        private KinectSensor Kinect;

        /// <summary>
        /// The skeleton data captured by the Kinect
        /// </summary>
        private Skeleton[] SkeletonData;

        /// <summary>
        /// The skeleton itself.
        /// </summary>
        private Skeleton Skeleton;

        #region Variables changed in every frame
        private Vector3 Position;
        private Vector3 PositionHandLeft;
        private Vector3 DifferenceLeft;
        private Vector3 PositionHandRight;
        private Vector3 DifferenceRight;
        #endregion

        /// <summary>
        /// Constructs a new movementDetection
        /// </summary>
        /// <param name="camera">The active camera</param>
        public MovementDetection()
        {
            Active = this;
        }

        /// <summary>
        /// Sets up the kinect sensor after it's ready
        /// </summary>
        /// <param name="kinect"></param>
        public void SetKinectSensor(KinectSensor kinect)
        {
            Kinect = kinect;
        }
      
        /// <summary>
        /// Extract poisition of single joint from skeleton.
        /// </summary>
        /// <param name="jointType">Joint</param>
        /// <returns>Vector3 with position of joint</returns>
        private Vector3 GetJointPosition(JointType jointType)
        {
            Position.X = Skeleton.Joints[jointType].Position.X;
            Position.Y = Skeleton.Joints[jointType].Position.Y;
            Position.Z = Skeleton.Joints[jointType].Position.Z;
            return Position;
        }

        /// <summary>
        /// Checks for motion in the left hand, and moves the camera accordingly.
        /// </summary>
        private void CheckMotionForCameraTranslation()
        {
            // Left Hand
            PositionHandLeft = GetJointPosition(JointType.HandLeft);
            DifferenceLeft = PositionHandLeft - LastPositionHandLeft;
            float shoulderY = Position.Y = Skeleton.Joints[JointType.ShoulderLeft].Position.Y;

            // if left hand is higher than the shoulder
            if (PositionHandLeft.Y > shoulderY)
            {
                // Make sure there is no hand jumping
                if (Math.Abs(DifferenceLeft.Z) > HandJumpThreshold)
                {
                    DifferenceLeft.Z = 0;
                }

                // Make sure there is no hand jumping and that we are not in follow mode
                if (DisplayController.ActiveDisplay.Following || Math.Abs(DifferenceLeft.X) > HandJumpThreshold)
                {
                    DifferenceLeft.X = 0;
                }

                // Translate plane
                if (DifferenceLeft.Z != 0 || DifferenceLeft.X != 0) 
                {
                    DisplayController.TranslateScene((-DifferenceLeft.Z * MovingSpeedForwards) ,0f ,(DifferenceLeft.X * MovingSpeedSideways));
                }
                
                LastPositionHandLeft = PositionHandLeft;
            }
        }

        /// <summary>
        /// Checks for motion in the right hand, and moves the camera accordingly.
        /// </summary>
        private void CheckMotionsForCameraRotation()
        {
            // Right Hand
            PositionHandRight = GetJointPosition(JointType.HandRight);
            DifferenceRight = PositionHandRight - LastPositionHandRight;
            float shoulderY = Position.Y = Skeleton.Joints[JointType.ShoulderRight].Position.Y;

            // if right hand is higher than the shoulder
            if (PositionHandRight.Y > shoulderY)
            {
                if (DisplayController.ActiveDisplay.Following)
                {
                    // Make sure there is no hand jumping
                    if(Math.Abs(DifferenceRight.X) > HandJumpThreshold)
                    {
                        DifferenceRight.X = 0;
                    }
                    // Make sure there is no hand jumping
                    if (Math.Abs(DifferenceRight.Z) > HandJumpThreshold)
                    {
                        DifferenceRight.Z = 0;
                    }
                    // Update rotation
                    if (DifferenceRight.X != 0 || DifferenceRight.Z != 0)
                    {
                        DisplayController.RotateCameraAroundSelectedPlane(-DifferenceRight.X, -DifferenceRight.Z);
                    }
                }
                else
                {
                    // Make sure there is no hand jumping
                    if (Math.Abs(DifferenceRight.X) > HandJumpThreshold)
                    {
                        DifferenceRight.X = 0;
                    }
                    // Make sure there is no hand jumping
                    if (Math.Abs(DifferenceRight.Z) > HandJumpThreshold)
                    {
                        DifferenceRight.Z = 0;
                    }

                    // Update rotation
                    if (DifferenceRight.X != 0 || DifferenceRight.Z != 0)
                    {
                        DisplayController.RotateCamera(-DifferenceRight.X * RotationSpeedSideways, DifferenceRight.Z * RotationSpeedUpDown);
                    }
                    
                }
                LastPositionHandRight = PositionHandRight;
            }
        }

        /// <summary>
        /// Updates the motiondetection. This implicitly checks for movement of the hands.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            // if a user is tracked this frame
            if (Skeleton != null)
            {
                // Check and do actions for the right hand
                CheckMotionsForCameraRotation();
                // Check and do actions for the left hand
                CheckMotionForCameraTranslation();
            }
        }


        // Reads the skeleton
        public void UpdateSkeleton(object sender, AllFramesReadyEventArgs imageFrames)
        {
            using (SkeletonFrame skeletonFrame = imageFrames.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if ((SkeletonData == null) || (SkeletonData.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        SkeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }
                    skeletonFrame.CopySkeletonDataTo(SkeletonData);
                }
            }

            if (SkeletonData != null)
            {
                //Console.WriteLine(skeletonData.Length);
                foreach (Skeleton skel in SkeletonData)
                {
                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        Skeleton = skel;
                    }
                }
            }
        }
    }
}
