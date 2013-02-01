using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WindowsGame3.Controller;
using WindowsGame3.View;

[assembly: InternalsVisibleTo("ProjectTests")]
namespace WindowsGame3.Models
{
    public class Airplane : AbstractModel
    {
        #region Properties: public airplane attributes
        /// <summary>
        /// The name of the airplane.
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// The acceleration of the airplane.
        /// </summary>
        public float Acceleration = 0.5f;

        /// <summary>
        /// The value of the velocity of the airplane. The direction
        /// can be found in the vector DirectionalVelocity.
        /// </summary>
        public float Velocity;

        /// <summary>
        /// The fuel remaining in the airplane's tank.
        /// </summary>
        public float Fuel { get; set; }

        /// <summary>
        /// Booleans showing if fuel warnings is displayed
        /// </summary>
        public Boolean ShowedFuelLowWarning = false;
        public Boolean ShowedFuelEmptyWarning = false;

        #endregion

        #region Properties: gradual acceleration
        /// <summary>
        /// The new velocity of the plane, which we want to change to.
        /// </summary>
        public float NewVelocity;

        /// <summary>
        /// The vector which our plane is facing, not normalized, so it is the
        /// actual speed.
        /// </summary>
        public Vector3 DirectionalVelocity = Vector3.Zero;

        public float RadiansToTurn = 0f;
        public float RadiansToTurnHalfPoint = 0f;
        public float RadiansToTurnUp = 0f;
        public Boolean TurningUp = true;
        public Boolean TurningRight = true;
        private double RollDegrees;
        public Boolean VelocityUp = false;
        #endregion

        #region Properties: constants
        private const double Turningspeed = 0.001;
        private const double MaxRoll = 0.6f;
        /// <summary>
        /// Cannot be marked constant, as it is not a runtime constant, oh well.
        /// </summary>
        public static float MaxPlanePitch = (float)(MathHelper.ToRadians(15));
        #endregion

        #region Properties: Members for drawing 2D information and lines

        /// <summary>
        /// BasicEffect used for drawing the lines below the airplane.
        /// </summary>
        private BasicEffect Effect;

        /// <summary>
        /// The vertex used to draw the line below the airplane
        /// </summary>
        private VertexPositionColor[] Vertices;

        /// <summary>
        /// The GraphicsDevice, we need to know which device to draw on.
        /// </summary>
        private readonly GraphicsDevice Device;

        /// <summary>
        /// The position of the planes projected to the screen (Use the X and Y coordinates).
        /// Used for drawing the names and coordinates of the airplane.
        /// </summary>
        private Vector3 ProjectedPosition = Vector3.Zero;

        /// <summary>
        /// The string showing the coordinates of the airplane, which is displayed below
        /// the airplanes if toggled on.
        /// </summary>
        private string CoordinateString = "";
        #endregion

        #region Constructor
        public Airplane(GraphicsDevice device, Vector3 position, float pitch = 0f, float yaw = 0f, String name = "", float velocity = 600f, Model model = null, float fuel = 150000f)
        {
            Device = device;
            Position = position;
            Model = model;
            Name = name;
            Velocity = velocity;
            NewVelocity = velocity;
            Scale = new Vector3(0.4f, 0.4f, 0.4f);
            AirplaneController.SetRotation(pitch, yaw, true, this);
            InitializeLineFromPlaneToGround();
            Fuel = fuel;
        }
        #endregion

        #region Methods: Public
        /// <summary>
        /// The planes update function. Moves the plane according to its speed and direction.
        /// Also updates the line to follow the plane.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            Position += DirectionalVelocity * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000000;
            TurnGradually();
            UpdateVertices();
            UseFuel();
            AlterVelocityGradually();
        }

        /// <summary>
        /// This method draws something specified for this class, other
        /// than the object's model. In this case - the line below the plane.
        /// </summary>
        protected override void SubclassDraw()
        {
            if (AirplaneController.ToggleLineUnderPlanes)
            {
                Effect.Projection = DisplayController.Projection;
                Effect.View = DisplayController.View;
                Effect.CurrentTechnique.Passes[0].Apply();
                Device.DrawUserPrimitives(PrimitiveType.LineList, Vertices, 0, 1);
                if (AirplaneController.SelectedPlane == this)
                {
                    NorthDirection[0].Position = new Vector3(Position.X, Position.Y -2.5f, Position.Z);
                    NorthDirection[1].Position = new Vector3(Position.X + 5, Position.Y -2.5f , Position.Z);
                    Device.DrawUserPrimitives(PrimitiveType.LineList, NorthDirection, 0, 1);
                }
            }
            ProjectedPosition = Device.Viewport.Project(Position, DisplayController.Projection, DisplayController.View, Matrix.Identity);
            if (AirplaneController.ToggleNames)
            {
                DrawNames();
            }
            if (AirplaneController.ToggleCoordinates)
            {
                DrawCoordinates();
            }

        }

        /// <summary>
        /// This method updates the plane's actual speed in the different direction
        /// according to its velocity and direction.
        /// </summary>
        public void UpdateDirectionalVelocity()
        {
            DirectionalVelocity.Y = (float)Math.Round((float)(Velocity * Math.Sin(Pitch)), 4);
            DirectionalVelocity.X = (float)Math.Round((float)(Velocity * Math.Cos(Pitch)) * Math.Cos(Yaw), 4);
            DirectionalVelocity.Z = (float)Math.Round((float)-(Velocity * Math.Cos(Pitch)) * Math.Sin(Yaw), 4);
        }
        #endregion

        #region Methods: private
        /// <summary>
        /// Changes the velocity of the airplane gradually according to the 
        /// acceleration of the airplane.
        /// </summary>
        internal void AlterVelocityGradually()
        {
            if (Velocity == NewVelocity)
            {
                return;
            }
            if (VelocityUp && Velocity > NewVelocity)
            {
                Velocity = NewVelocity;
            }
            else if (!VelocityUp && Velocity < NewVelocity)
            {
                Velocity = NewVelocity;
            }
            else if (Velocity > NewVelocity)
            {
                Velocity = Velocity - Acceleration;
            }
            else if (Velocity < NewVelocity)
            {
                Velocity = Velocity + Acceleration;
            }
            UpdateDirectionalVelocity();
        }

        /// <summary>
        /// This method makes sure the airplane turns gradually, and not instantly.
        /// </summary>
        internal void TurnGradually()
        {
            // Turn to the side
            if (RadiansToTurn != 0f)
            {
                // Make airplanne roll

                if (RadiansToTurn < RadiansToTurnHalfPoint)
                {
                    //dec roll
                    RollDegrees = (TurningRight) ? RollDegrees + Turningspeed : RollDegrees - Turningspeed;
                    Roll = (TurningRight) ? (float)Math.Max(-MaxRoll, Math.Min(0, RollDegrees)) : (float)Math.Min(MaxRoll, Math.Max(0, RollDegrees));
                }
                else
                {
                    // inc roll
                    RollDegrees = (TurningRight) ? RollDegrees - Turningspeed : RollDegrees + Turningspeed;
                    Roll = (TurningRight) ? (float)Math.Max(-MaxRoll, RollDegrees) : (float)Math.Min(MaxRoll, RollDegrees);
                }

                double ToTurn = Math.Min(RadiansToTurn, Turningspeed);
                Yaw = (Yaw + 2 * MathHelper.Pi) % (2 * MathHelper.Pi);
                Yaw += (TurningRight) ? (float)ToTurn : (float)-ToTurn;
                RadiansToTurn -= (float)ToTurn;
                if (RadiansToTurn == 0)
                {
                    Roll = 0f;
                    RollDegrees = 0f;
                }
                UpdateModelRotationAndScaleMatrix();
                UpdateDirectionalVelocity();
            }
            // Turn up
            if (RadiansToTurnUp != 0f)
            {
                float ToTurnUp = Math.Min((float)RadiansToTurnUp, (float)Turningspeed);
                Pitch += (TurningUp) ? ToTurnUp : -ToTurnUp;
                RadiansToTurnUp -= ToTurnUp;

                UpdateModelRotationAndScaleMatrix();
                UpdateDirectionalVelocity();
            }
        }

        /// <summary>
        /// Draws the names below the airplane.
        /// </summary>
        private void DrawNames()
        {
            if (ProjectedPosition.Z > 1.0f)
            {
                return;
            }
            TheDisplay.SpriteBatch.DrawString(TheDisplay.SprFont, "plane " + Name, new Vector2(ProjectedPosition.X - TheDisplay.SprFont.MeasureString("plane " + Name).X / 2, ProjectedPosition.Y), Color.White);
        }

        /// <summary>
        /// Draws the coordinates below the airplane.
        /// </summary>
        private void DrawCoordinates()
        {
            CoordinateString = "lo:" + Math.Round(Position.X, 0) + " la:" + Math.Round(Position.Z, 0);
            if (ProjectedPosition.Z > 1.0f)
            {
                return;
            }

            TheDisplay.SpriteBatch.DrawString(TheDisplay.SprFont, CoordinateString, new Vector2(ProjectedPosition.X - TheDisplay.SprFont.MeasureString(CoordinateString).X / 2, ProjectedPosition.Y + 20), Color.White);
        }
        VertexPositionColor[] NorthDirection;
        /// <summary>
        /// This method initializes everything which is needed for the line to be
        /// drawn from the airplane to the ground.
        /// </summary>
        private void InitializeLineFromPlaneToGround()
        {
            if (Device == null) return;

            Vertices = new VertexPositionColor[2];

            Vertices[0].Color = Vertices[1].Color = Color.Brown;
            UpdateVertices();

            NorthDirection = new VertexPositionColor[2];
            NorthDirection[0].Color = NorthDirection[1].Color = Color.Blue;
            Effect = new BasicEffect(Device) { VertexColorEnabled = true };

        }

        /// <summary>
        /// This method updates the line which follows under the airplane,
        /// should be called from the Update method.
        /// </summary>
        private void UpdateVertices()
        {
            Vertices[0].Position = Position;
            Vertices[1].Position = new Vector3(Position.X, -1, Position.Z);
        }

   
        /// <summary>
        /// This method updates the amount of fuel remaining.
        /// </summary>
        internal void UseFuel()
        {
            if (Fuel > 0.0f)
            {
                Fuel = Fuel - 0.01f;
            }
            else
            {

                Fuel = 0.0f;
            }

        }
        #endregion
    }
}
