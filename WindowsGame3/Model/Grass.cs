using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame3.Models
{
    public class Grass : AbstractModel
    {
      
        public String Name { get; set; }

        public Grass(Vector3 position, Model model = null)
        {
            Model = model;
            Position = position;
            Scale = new Vector3(1f, 1f, 1f);
            UpdateModelRotationAndScaleMatrix();
        } 
        
        public void Update(GameTime gameTime)
        {
        }

    }
}
