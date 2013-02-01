using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WindowsGame3.Controller;

namespace WindowsGame3.Models
{
    public abstract class AbstractModel
    {
        public Vector3 Scale = new Vector3(1f, 1f, 1f);
        public Model Model { get; set; }
        public Vector3 Position { get; set; }
        public Boolean IsSelected = false;
       
        public float Pitch; // Vertical direction; 
        public float Yaw; // Horisontal direction;
        public float Roll = 0f; 
        public Matrix ModelRotationAndScaleMatrix;

        //Used when drawing
        private Matrix View, Projection;
        private Matrix[] Transforms;

        protected Matrix World;

        public void UpdateModelRotationAndScaleMatrix()
        {
            ModelRotationAndScaleMatrix = Matrix.CreateScale(Scale.X, Scale.Y, Scale.Z) * Matrix.CreateRotationX(Roll) * Matrix.CreateRotationZ(Pitch) * Matrix.CreateRotationY(Yaw);
        }

        public void Draw()
        {
            View = DisplayController.View;
            Projection = DisplayController.Projection;
            if (Model == null) return;
            // Copy any parent transforms.
            Transforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(Transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in Model.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    if (IsSelected)
                    {
                        effect.AmbientLightColor = Color.Red.ToVector3();
                    }
                    World = Transforms[mesh.ParentBone.Index] *
                                   ModelRotationAndScaleMatrix *
                                   Matrix.CreateTranslation(Position);
                    effect.World = World;
                    effect.View = View;
                    effect.Projection = Projection;
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
            SubclassDraw();
        }

        protected virtual void SubclassDraw() {}
    }
}
