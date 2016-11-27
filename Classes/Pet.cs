using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using KSshooter.Classes;

namespace KSshooter.Classes
{
    public class Pet : Enemy
    {
        const double ATTACKCOOLODOWN = 1000;
        const float DAMAGEDEALT = 4;
        const int SPEED = 110;
        readonly float maxHP;

        //constructors
        public Pet(Texture2D tex, float hp, float damageDealt = DAMAGEDEALT)
            : base(tex, hp, DAMAGEDEALT, 0, null, null,AItype.AstarPathfindingMelee ,ATTACKCOOLODOWN)
        {
            visible = true;
            speed = SPEED;
            chaseDistance = 600;
            maxHP = hp;
        }
        //properties1
        public Room inRoom
        {
            get { return room; }
            set 
            { 
                room = value;
                AquireNewTarget();
            }
        }

        //functions
        public void Reset()
        {
            health = maxHP;
        }

        public void AquireNewTarget()
        {
            //aquire closest target
            Target = null;
            foreach (Enemy en in inRoom.Enemies)
            {
                if (en.Alive == true)
                {
                    Target = en;
                    break;
                }
            }
            if (Target == null)
            {
                Target = inRoom.level.player;
                targetHostile = false;
                return;
            }
            foreach (Enemy en in inRoom.Enemies)
            {
                if (Vector2.Distance(this.Position, en.Position) <= Vector2.Distance(this.position, this.Target.Position))
                {
                    if (en.Alive == true)
                    {
                        this.Target = en;
                        targetHostile = true;
                    }
                }
            }
            if (Vector2.Distance(Position, Target.Position) >= chaseDistance)
            {
                targetHostile= false;
                Target = inRoom.level.player;
            }
            this.Target.ObjectDeath += new ObjectDeathEventHandler(Target_ObjectDeath);
        }

        public void Update(GameTime gameTime)
        {
            //if the target is the player, trye to find an enemy target
            if (this.Target == inRoom.level.player)
                this.AquireNewTarget();
            bool temp = false;
            base.Update(gameTime, ref temp);
        }

        //Event Handlers
        void Target_ObjectDeath(object Sender, ObjectDeathEventArgs e)
        {
            AquireNewTarget();
        }
    }
}
