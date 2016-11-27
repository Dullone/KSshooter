using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace KSshooter.Classes
{
    public class Tile :HitableObject
    {
        Texture2D sprite;
        private Vector2 location;
        //private Rectangle screenPosition;
        Texture2D overlay;

        public const int TileHeight = 30;
        public const int TileWidth = 30;

        bool onscreen;

        //constructor
        public Tile(Texture2D tex)
        {
            sprite = tex;
            hitRec = new Rectangle((int)location.X, (int)location.Y, tex.Width, tex.Height);
            //screenPosition = new Rectangle((int)location.X, (int)location.Y, tex.Width, tex.Height);
            Hit = false;
            onscreen = false;
            overlay = null;
        }

        //Methods
        public Vector2 Location
        {
            set
            {
                location = value;
                hitRec.X = Convert.ToInt32(value.X);
                hitRec.Y = Convert.ToInt32(value.Y);
            }
            get
            {
                return location;
            }
        }

        //properites
        public Texture2D Overlay
        {
            get { return overlay; }
            set { overlay = value; }
        }
        
        public override bool HitDetection(Rectangle rec, Vector2 offset)
        {
            if (hit == true)
            {
                Rectangle temprec = new Rectangle(rec.X + (int)offset.X, rec.Y + (int)offset.Y, rec.Width, rec.Height);
                return this.HitRectangle.Intersects(temprec);
            }
            return false;
        }

        public void draw(SpriteBatch spriteBatch, Vector2 offset, Rectangle veiwPort)
        {
            Vector2 screenPosition = location + offset;
            //if (Game1.CheckIfOnscreen(screenPosition, HitRectangle.Width, HitRectangle.Height, veiwPort) == true)
            //{
                spriteBatch.Draw(sprite, screenPosition, Color.White);
                if (overlay != null) //draw overlay if one exists
                    spriteBatch.Draw(overlay, screenPosition, Color.White);

            //}
           
        }
        public void drawOverlay(SpriteBatch spriteBatch, Vector2 offset, Rectangle veiwPort)
        {
            if (overlay != null) //draw overlay if one exists
            {
                Vector2 screenPosition = location + offset;
                spriteBatch.Draw(overlay, screenPosition, Color.White);
            }
        }
    }
}
