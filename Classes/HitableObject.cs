using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KSshooter.Classes
{
    public class HitableObject
    {
        protected Rectangle hitRec;
        protected bool Hit;

        public delegate void HitChangedEventHandler(HitableObject sender, HitChangedEventArgs e);
        public event HitChangedEventHandler HitChanged; //only fired if hit changes form true to false or vice versa

        public Rectangle HitRectangle
        {
            get { return hitRec; }
        }

        public bool hit
        {
            set
            {
                bool prevValue = hit;
                Hit = value;
                if(HitChanged != null && prevValue != Hit)
                    HitChanged(this, new HitChangedEventArgs(prevValue));
            }
            get { return Hit; }
        }

        public virtual bool HitDetection(Rectangle rec, Vector2 offset)
        {
            Rectangle temprec = new Rectangle(rec.X + (int)offset.X, rec.Y + (int)offset.Y, rec.Width, rec.Height);
            return this.HitRectangle.Intersects(temprec);
        }

        public virtual bool HitDetection(Rectangle rec)
        {
            if (hit == true)
            {
                return this.hitRec.Intersects(rec);
            }
            return false;
        }
    }

    public class HitChangedEventArgs : EventArgs
    {
        public bool previousHit;
        public HitChangedEventArgs(bool prev)
            : base()
        {
            previousHit = prev;
        }
    }
}
