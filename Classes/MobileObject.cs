using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using KSshooter.Classes;

namespace KSshooter
{
    public class MobileObject : HitableObject
    {
        public int id { get; private set; }
        protected Vector2 position; //position in world
        //protected Rectangle screenPosition; //where we draw on screen
        //rotation variables
        protected float rotation;
        protected Vector2 rotateAround;

        protected bool onscreen;
        protected bool visible;
        Texture2D sprite;
        protected Room room;

        //correct mode
        public const int correctDontMove = 0;
        public const int correctSnapToEdge = 1;

        public MobileObject(Texture2D tex)
        {
            sprite = tex;
            visible = true;
            position = new Vector2(0, 0);
            hitRec = new Rectangle((int)position.X, (int)position.Y, tex.Width, tex.Height);
            onscreen = false;
            rotation = 0f;
            rotateAround = new Vector2(0, 0);
        }

        //Properties
        public Room inRoom
        {
            get { return room; }
            set { room = value; }
        }

        public Texture2D Sprite
        {
            get { return sprite; }
        }

        public virtual Vector2 Position
        {
            set
            {
                PositionX = value.X;
                PositionY = value.Y;
            }
            get
            {
                return position;
            }
        }

        public float PositionX
        {
            get { return position.X; }
            set 
            {
                position.X = value;
                hitRec.X = (int)value;
            }
        }

        public float PositionY
        {
            get { return position.Y; }
            set
            {
                position.Y = value;
                hitRec.Y = (int)value;
            }
        }

        public virtual void draw(SpriteBatch spriteBatch, Vector2 offset, Rectangle veiwPort)
        {
            Vector2 screenPosition = position + offset;
            if (visible == false)
                return;
            spriteBatch.Draw(sprite, screenPosition, null, Color.White, rotation, rotateAround, 1f,SpriteEffects.None, 0);
        }


        public override bool HitDetection(Rectangle rec)
        {
            return hitRec.Intersects(rec);
        }

        public bool HitDetectionWithTiles(Rectangle moveTo, ref Vector2 moveAmount)
        {
            bool hitDetected = false;
            //check for hit with tiles
            int xLeft = moveTo.X / Tile.TileWidth;
            int xRight = (moveTo.X + Tile.TileWidth) / Tile.TileWidth;
            int yUP = (moveTo.Y / Tile.TileHeight);
            int yLower = (moveTo.Y + Tile.TileHeight) / Tile.TileHeight;
            try
            {
                if (inRoom.Tiles[xLeft, yUP].HitDetection(moveTo))
                {
                    hitDetected = true;
                    this.fixHitOverlap(inRoom.Tiles[xLeft, yUP].HitRectangle, ref moveAmount);
                }
                if (inRoom.Tiles[xLeft, yLower].HitDetection(moveTo))
                {
                    hitDetected = true;
                    this.fixHitOverlap(inRoom.Tiles[xLeft, yLower].HitRectangle, ref moveAmount);
                }
                if (inRoom.Tiles[xRight, yUP].HitDetection(moveTo))
                {
                    hitDetected = true;
                    this.fixHitOverlap(inRoom.Tiles[xRight, yUP].HitRectangle, ref moveAmount);
                }
                if (inRoom.Tiles[xRight, yLower].HitDetection(moveTo))
                {
                    hitDetected = true;
                    this.fixHitOverlap(inRoom.Tiles[xRight, yLower].HitRectangle, ref moveAmount);
                }
            }
            catch (IndexOutOfRangeException)
            {
                moveAmount.X = moveAmount.Y = 0;
            }
            return hitDetected;
        }

        public bool HitDetectionWithTilesNoFixing(Rectangle moveTo)
        {
            //check for hit with tiles
            int xLeft = moveTo.X / Tile.TileWidth;
            int xRight = (moveTo.X + Tile.TileWidth) / Tile.TileWidth;
            int yUP = (moveTo.Y / Tile.TileHeight);
            int yLower = (moveTo.Y + Tile.TileHeight) / Tile.TileHeight;
            try
            {
                if (inRoom.Tiles[xLeft, yUP].HitDetection(moveTo))
                {
                    return true;
                }
                if (inRoom.Tiles[xLeft, yLower].HitDetection(moveTo))
                {
                    return true;
                }
                if (inRoom.Tiles[xRight, yUP].HitDetection(moveTo))
                {
                    return true;
                }
                if (inRoom.Tiles[xRight, yLower].HitDetection(moveTo))
                {
                    return true;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// snaps this moveable object back to the edge of the hitable area
        /// </summary>
        /// <param name="boundingRec">Bounding rectangle of the object we hit </param>
        /// <param name="moveAmount"></param>
        public void fixHitOverlap(Rectangle boundingRec, ref Vector2 moveAmount, int correctType = correctSnapToEdge )
        {
            bool hitFound = false;
            //debug
            Vector2 oldVector = new Vector2(moveAmount.X, moveAmount.Y);
            //end deubug
            Rectangle hitRecX = new Rectangle((int)(this.PositionX + moveAmount.X), (int)(this.PositionY), this.hitRec.Width, this.hitRec.Height);

            if (boundingRec.Intersects(hitRecX) == true)
            {
                if (correctType == correctSnapToEdge)
                {
                    hitFound = true;
                    if (Math.Abs(this.HitRectangle.Left - boundingRec.Right) > Math.Abs(this.HitRectangle.Right - boundingRec.Left)) //snap to left //old:moveAmount.X > 0
                    {
                        moveAmount.X = boundingRec.Left - this.PositionX - this.hitRec.Width;
                    }
                    else if (Math.Abs(this.HitRectangle.Left - boundingRec.Right) < Math.Abs(this.HitRectangle.Right - boundingRec.Left)) //snap to right 
                    {
                        moveAmount.X = boundingRec.Right - this.PositionX;
                    }
                }
                else if (correctType == correctDontMove)
                {
                    moveAmount.X = 0;
                }
            }
            Rectangle hitRecY = new Rectangle((int)(this.PositionX), (int)(this.PositionY + moveAmount.Y), this.hitRec.Width, this.hitRec.Height);
            if (boundingRec.Intersects(hitRecY) == true)
            {
                if (correctType == correctSnapToEdge)
                {
                    hitFound = true;
                    if (Math.Abs(this.HitRectangle.Bottom - boundingRec.Top) < Math.Abs(this.HitRectangle.Top - boundingRec.Bottom))//snap to top
                    {
                        moveAmount.Y = boundingRec.Top - this.PositionY - this.hitRec.Height;
                    }
                    else if (Math.Abs(this.HitRectangle.Bottom - boundingRec.Top) > Math.Abs(this.HitRectangle.Top - boundingRec.Bottom))//snap to bottom
                    {
                        moveAmount.Y = boundingRec.Bottom - this.PositionY;
                    }
                }
                else if (correctType == correctDontMove)
                {
                    moveAmount.Y = 0;
                }
            }

            if (hitFound == false) //rare case were exactly one point overlaps
            {
                //give it a little shove, next iteration should fix it.
                //moveAmount.Y = 0;
                hitFound = false;
            }
            if(moveAmount.Length() > 5)
            {
                bool pause = true;
            }
        }
    }
}
