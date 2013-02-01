using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame3.Models
{
    public class Axis
    {
        public Color XColor { get; set; }
        public Color YColor { get; set; }
        public Color ZColor { get; set; }
        public Vector3 Position = new Vector3(100f, 65f, 0f);
        public float Length { get; set; }


        private BasicEffect Effect;
        private VertexPositionColor[] Vertices;

        private readonly GraphicsDevice Device;

        public Axis(GraphicsDevice device)
        {
            XColor = Color.Red;
            YColor = Color.Green;
            ZColor = Color.Blue;

            Length = 10F;

            Device = device;

            SetVertices();
        }

        private void SetVertices()
        {
            if (Device == null) return;
            Vertices = new VertexPositionColor[6];
            Vertices[0].Color = Vertices[1].Color = XColor;
            Vertices[2].Color = Vertices[3].Color = YColor;
            Vertices[4].Color = Vertices[5].Color = ZColor;
            Vertices[0].Position = new Vector3(Position.X - Length, Position.Y, Position.Z);
            Vertices[1].Position = new Vector3(Position.X + Length, Position.Y, Position.Z);
            Vertices[2].Position = new Vector3(Position.X, Position.Y + Length, Position.Z);
            Vertices[3].Position = new Vector3(Position.X, Position.Y - Length, Position.Z);
            Vertices[4].Position = new Vector3(Position.X, Position.Y, Position.Z - Length);
            Vertices[5].Position = new Vector3(Position.X, Position.Y, Position.Z + Length);

            Effect = new BasicEffect(Device);
            Effect.VertexColorEnabled = true;
        }

        public void Draw(Matrix view, Matrix projection)
        {
            Effect.Projection = projection;
            Effect.View = view;
            Effect.CurrentTechnique.Passes[0].Apply();
            Device.DrawUserPrimitives(PrimitiveType.LineList, Vertices, 0, 3);
        }
    }
}
