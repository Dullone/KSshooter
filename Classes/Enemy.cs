using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using KSshooter.Classes;

namespace KSshooter
{
    public class Enemy : CharacterObject
    {
        const int DEFAULTSPEED = 85;
        const int CHASEDISTANCE = 450;
        int xpValue;

        //Target
        CharacterObject target;
        protected bool targetHostile;
        protected int chaseDistance;

        //color variables
        Color paintColor;
        Color defaultPaintColor;
        double newColorDuration;
        double newColorAge;
        bool newPaintColor;

        //attack
        double attackCooldown;
        double lastAttack;

        //AI
        bool enableAI;
        List<Vector2> path;
        double lastPathUpdate;
        double pathUpdateInterval = 100;
        public enum AItype { dumb, AstarPathfindingMelee };
        AItype aiType;
        AItype currentAI;
        double lastAIswitch = 0;
        const double AIswitchInterval = 10000;

        //idle
        List<Vector2> idlePath;
        int currentWaypoint=0;

        //shoot varialbles
        double lastShot;
        const double ShotCooldown = 1000; //miliseconds
        const float shootRange = 500;

        //Constructors
        public Enemy(Texture2D tex, float hp, float damageDealt, int experienceValue, CharacterObject AttackTarget, Room room, AItype ai = AItype.AstarPathfindingMelee, double attackInterval = 300):base(tex)
        {
            health = hp;
            damage = damageDealt;
            xpValue = experienceValue;
            speed = DEFAULTSPEED;
            enableAI = true;
            defaultPaintColor = paintColor = Color.White;
            attackCooldown = attackInterval;
            lastAttack = attackCooldown;
            targetHostile = true;
            chaseDistance = CHASEDISTANCE;
            //AIUpdateMethod = MoveTowardTarget;
            target = AttackTarget;
            inRoom = room;
            path = new List<Vector2>();
            lastPathUpdate = pathUpdateInterval;
            aiType = ai;
            currentAI = AItype.AstarPathfindingMelee;
            lastShot = ShotCooldown;
            idlePath = new List<Vector2>();
        }

        //Properties
        public int XPvalue
        {
            get { return xpValue; }
        }

        public CharacterObject Target
        {
            set 
            {
                target = value;
            }
            get { return target; }
        }
        public bool EnableAI
        {
            set { enableAI = value; }
            get { return enableAI; }
        }
        public float DamageDealt
        {
            get { return damage; }
        }

        //Fuctions
        public void addIdlePathWaypoint(Vector2 waypoint)
        {
            idlePath.Add(waypoint);
        }

        public void TemporaryPaintColor(Color newColor, double duration)
        {
            newColorDuration = duration;
            newColorAge = 0;
            paintColor = newColor;
            newPaintColor = true;
        }

        public virtual void Update(GameTime gameTime, ref bool AstarUpdated)
        {           
            lastAttack += gameTime.ElapsedGameTime.TotalMilliseconds;
            lastPathUpdate += gameTime.ElapsedGameTime.TotalMilliseconds;
            lastAIswitch += gameTime.ElapsedGameTime.TotalMilliseconds;
            lastShot += gameTime.ElapsedGameTime.TotalMilliseconds;

            //if (enableAI == true)
            //    AIUpdateMethod(gameTime);

            if (path.Count > 15)
                pathUpdateInterval = 1000;
            else if (path.Count > 10)
                pathUpdateInterval = 500;
            else if (path.Count > 6)
                pathUpdateInterval = 100;
            else
                pathUpdateInterval = 50;

            UpdateAI(gameTime, ref AstarUpdated);

            if (newPaintColor)
            {
                newColorAge += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (newColorAge >= newColorDuration)
                {
                    newPaintColor = false;
                    paintColor = defaultPaintColor;
                }
            }
        }

        private void UpdateAI(GameTime gameTime, ref bool AstarUpdated)
        {
            //we only chase the target it they are less chasedistance
            if (Vector2.Distance(this.position, target.Position) <= chaseDistance && Target != null)
            {
                if (currentAI == AItype.dumb)
                {
                    MoveTowardTarget(gameTime, Target.Position);
                }
                else if (currentAI == AItype.AstarPathfindingMelee)
                {
                    if (lastAIswitch > AIswitchInterval)
                    {
                        lastAIswitch = 0;
                        //currentAI = AItype.dumb;
                    }
                    GetPath(ref AstarUpdated);
                    if (path == null || path.Count == 0)
                        return;
                    MoveTowardTarget(gameTime, path[path.Count - 1]);
                    if (position == path[path.Count - 1])
                        path.RemoveAt(path.Count - 1);
                }
            }
            else if (idlePath.Count > 0)
            {
                if(position == idlePath[currentWaypoint])
                {
                    currentWaypoint++;
                    if (currentWaypoint > idlePath.Count - 1)
                        currentWaypoint = 0;
                }
                MoveTowardTarget(gameTime, idlePath[currentWaypoint]);
            }

        }

        private void GetPath(ref Boolean AstarUpdated)
        {
            if (path.Count == 0) //get a path
            {
                path = inRoom.ASTARpathfinding.FindPath(getClosestTile(Position), getClosestTile(Target.Position));
            }
            else if(lastPathUpdate >= pathUpdateInterval && !AstarUpdated)//update path
            {
                path = inRoom.ASTARpathfinding.FindPath(getClosestTile(Position), getClosestTile(Target.Position));
                AstarUpdated = true;
                lastPathUpdate = 0;
            }
        }

        private Vector2 getClosestTile(Vector2 loc)
        {
            float modX = loc.X % 30;
            float modY = loc.Y % 30;
            Vector2 NewLoc = new Vector2();
            if (modX > 15)
                NewLoc.X = loc.X + 30 - modX;
            else
                NewLoc.X = loc.X - modX;
            if (modY > 15)
                NewLoc.Y = loc.Y + 30 - modY;
            else
                NewLoc.Y = loc.Y - modY;
            return NewLoc;
        }

        public bool Shoot(Vector2 atTarget, Bullet aBulletToShoot)
        {
            if (lastShot < ShotCooldown)
                return false;
            if (Vector2.Distance(this.Position, atTarget) > shootRange)
                return false;
            aBulletToShoot.ShootBullet(this.Position, Game1.PointToward(this.Position, atTarget), 120, 4000);
            inRoom.AddEnemyBullet(aBulletToShoot);
            lastShot = 0;
            return true;
        }

        private void MoveTowardTarget(GameTime gameTime, Vector2 moveTo)
        {
            if (moveTo == Position)
                return;
            //move
            Vector2 moveAmount = Game1.PointToward(this.Position, moveTo);

                moveAmount.Normalize();
                moveAmount.X = moveAmount.X * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                moveAmount.Y = moveAmount.Y * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            if (Vector2.Distance(position, position + moveAmount) >= Vector2.Distance(position, moveTo))
            {
                moveAmount = Game1.PointToward(this.Position, moveTo);
            }

            Rectangle tempRec = new Rectangle((int)(this.PositionX + moveAmount.X), (int)(this.PositionY + moveAmount.Y), this.HitRectangle.Width, this.HitRectangle.Height);

            //if (aiType == AItype.AstarPathfindingMelee && currentAI == AItype.dumb)
            //{
            //    if (this.HitDetectionWithTilesNoFixing(tempRec) == true)
            //    {
            //        currentAI = AItype.AstarPathfindingMelee;
            //        moveAmount.X = moveAmount.Y = 0;
            //    }
            //}
            //else
            //{
                //this.HitDetectionWithTiles(tempRec, ref moveAmount);
            //}

            //check for hit with other enemies
            foreach (Enemy en in inRoom.Enemies)
            {
                if (en != this && en != Target)
                {
                    if (en.HitDetection(tempRec) == true)
                    {
                        this.fixHitOverlap(en.HitRectangle, ref moveAmount);
                    }
                }
            }
            //check for hit with pet
            if (inRoom.level.player.Pet != null)
            {
                if (inRoom.level.player.Pet != this)
                    if (inRoom.level.player.Pet.HitDetection(tempRec) == true)
                        this.fixHitOverlap(inRoom.level.player.Pet.HitRectangle, ref moveAmount);
            }
            if (Target.HitDetection(tempRec) == true)
            {
                this.fixHitOverlap(Target.HitRectangle, ref moveAmount);
                if(targetHostile)
                    AttackCharacter(Target);
            }

            Position += moveAmount;
        }

        public void AttackCharacter(CharacterObject target)
        {
            if (lastAttack >= attackCooldown)
            {
                lastAttack = 0;
                target.takeDamage(this.DamageDealt);
            }
        }

        public override void draw(SpriteBatch spriteBatch, Vector2 offset, Rectangle veiwPort)
        {
            if (visible == false)
                return;
            Vector2 screenPosition = position + offset;
            //if (Game1.CheckIfOnscreen(screenPosition, this.HitRectangle.Width, this.HitRectangle.Height, veiwPort) == true)
            //{
                spriteBatch.Draw(this.Sprite, screenPosition, null, paintColor, rotation, rotateAround, 1f, SpriteEffects.None, 0);
            //}

            //draw path
            //if (path != null && path.Count > 0)
            //{
            //    foreach (Vector2 w in path)
            //    {
            //        spriteBatch.Draw(this.Sprite, w + offset, null, new Color(100, 100, 100, 100), rotation, rotateAround, 1f, SpriteEffects.None, 0);
            //    }
            //}
        }

        public override void takeDamage(float amount)
        {
            base.takeDamage(amount);
            TemporaryPaintColor(Color.Red, 100);
        }

    }

}
