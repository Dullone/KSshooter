using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using KSshooter.Classes;

namespace KSshooter.Classes
{
    public class CharacterObject : MobileObject
    {
        protected float health;
        protected float maxHealth;
        protected int speed;
        private bool alive;
        protected float damage;

        //Events
        public delegate void DamagedEventHandler(Object sender, DamageEventArgs e);
        public event DamagedEventHandler Damaged;
        public delegate void ObjectDeathEventHandler(object Sender, ObjectDeathEventArgs e);
        public event ObjectDeathEventHandler ObjectDeath;

        //constructors
        public CharacterObject(Texture2D tex)
            : base(tex)
        {
            alive = true;
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
                        ObjectDeath(this, new ObjectDeathEventArgs());
                }
            }
        }

        public float Health
        {
            get { return health; }
            set 
            {
                health = value;
                if (health <= 0)
                {
                    Alive = false;
                }
            }
        }

        public float MaxHealth
        {
            get { return maxHealth; }
        }

        public virtual void takeDamage(float amount)
        {
            Health -= amount;
            if (Damaged != null)
                Damaged(this, new DamageEventArgs(amount));
        }
    }

    /// <summary>
    /// Event args for Object death
    /// </summary>
    public class ObjectDeathEventArgs : EventArgs
    {
        //nothing here right now
    }

    public class DamageEventArgs : EventArgs
    {
        public float damageTaken;
        public DamageEventArgs(float damageAmount)
            : base()
        {
            damageTaken = -1 * damageAmount;
        }
    }
}
