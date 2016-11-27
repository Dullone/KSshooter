using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KSshooter.Classes
{
    public class Bullet : MobileObject
    {
        int MOVESPEED; //pixels per second
        int duration; //milliseconds
        public int damage;

        private double age;
        private bool alive;
        private Vector2 movementDirection;

        public delegate void ObjectDeathEventHandler(object Sender, ObjectDeathEventArgs e);
        public event ObjectDeathEventHandler ObjectDeath;

        public Bullet(Texture2D tex)
            : base(tex)
        {
            age = 0;
            Alive = true;
            rotateAround.X = tex.Width / 2;
            rotateAround.Y = tex.Height / 2;
            damage = 1;
        }

        //Properties
        public bool Alive
        {
            get
            {
                return alive;
            }
            set
            {
                alive = value;
                if (alive == false)
                {
                    if (ObjectDeath != null)
                    {
                        ObjectDeath(this, new ObjectDeathEventArgs());
                    }
                }
            }
        }

        public void ShootBullet(Vector2 startPosition, Vector2 direction, int moveSpeed = 200, int bulletDuration = 2000)
        {
            Alive = true;
            age = 0;
            Position = startPosition;
            direction.Normalize();
            movementDirection = direction;
            rotation = (float)Math.Atan2(movementDirection.Y, movementDirection.X);
            MOVESPEED = moveSpeed;
            duration = bulletDuration;
        }

        private void UpdatePosition(GameTime gameTime)
        {
            float newX = movementDirection.X * MOVESPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
            float newY = movementDirection.Y * MOVESPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
            PositionX += newX;
            PositionY += newY;

        }

        public void AttackTarget(CharacterObject target)
        {
            target.takeDamage(this.damage);
            this.Alive = false;
        }

        public void Update(GameTime gameTime)
        {
            UpdatePosition(gameTime);
            age += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (age > duration)
            {
                this.Alive = false;
            }
        }
    }
}
