using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using KSshooter.Classes;


namespace KSshooter.Classes
{
    public class Player : CharacterObject
    {
        const int DEFAULTSPEED = 100;

        //Paint location - should be set to center of the screen
        public readonly Vector2 screenPosition;

        //teleport variables
        const double TELEPORTCOOLDOWN = 5000;//milliseconds
        const int TELEPORTABILITYLEVEL = 1;
        const float TELEPORTDISTANCEMAX = 180;//pixels
        double lastTeleport;

        //Pet
        Pet pet;
        const double PETSUMMONCOOLDOWN = 40000; //milliseconds
        double lastPetSummon;
        const double PETDURATION = 20000;
        public const int SUMMONABILITYLEVEL = 2;

        //Experience points & level
        int experiencePoints; //  1   2   3   4    5    6
        int[] XPneededForlevel = {0, 10, 30, 100, 200, 100000000 }; //level 1, 2, 3...etc
        int currentLevel;

        //Abilities
        string[] abilities = { "Teleport", "Summon Pet", "Bullshit", "Nothing" };

        public delegate void LevelUpEventHandler(Player sender, LevelUpEventArgs e);
        public event LevelUpEventHandler LevelUp;

        //Constructors
        public Player(Texture2D tex, Vector2 screenCenter)
            : base(tex)
        {
            screenPosition = screenCenter;
            health = 10;
            maxHealth = health;
            currentLevel = 1;
            experiencePoints = 0;
            lastTeleport = TELEPORTCOOLDOWN;
            speed = DEFAULTSPEED;
            onscreen = true;
            pet = null;
            lastPetSummon = PETSUMMONCOOLDOWN;
        }

        //Properties
        public int Speed
        {
            get { return speed; }
        }

        public override Vector2 Position
        {
            set
            {
                position = value;
                hitRec.X = Convert.ToInt32(position.X);
                hitRec.Y = Convert.ToInt32((int)position.Y);
            }
            get
            {
                return position;
            }
        }

        public double TeleportCoolDownRemaining
        {
            get
            {
                if (lastTeleport >= TELEPORTCOOLDOWN)
                {
                    return 0;
                }
                else
                {
                    return TELEPORTCOOLDOWN - lastTeleport;
                }
            }
        }

        public double SummonCooldownRemaining
        {
            get
            {
                if (lastPetSummon >= PETSUMMONCOOLDOWN)
                {
                    return 0;
                }
                else
                {
                    return PETSUMMONCOOLDOWN - lastPetSummon;
                }
            }
        }

        public Pet Pet
        {
            get { return pet; }
        }

        public int ExperiencePoints
        {
            get { return experiencePoints; }
            set
            {
                experiencePoints = value;
                if (experiencePoints >= XPneededForlevel[currentLevel])
                {
                    currentLevel++;
                    if(LevelUp!=null)
                        LevelUp(this, new LevelUpEventArgs(currentLevel, ExperiencePoints));
                }
            }
        }

        public int CharacterLevel
        {
            get { return currentLevel; }
        }

        public int XPtoNextLevel
        {
            get { return XPneededForlevel[currentLevel] - experiencePoints; }
        }

        public String[] PlayerAbilities
        {
            get
            {
                return abilities;
            }
        }

        //Functions
        public string TeleportCoolDownRemainingAsString()
        {
            double time = TeleportCoolDownRemaining;
            if (time == 0)
                return "Ready!";
            time /= 1000;

            return time.ToString("0.0");
        }
        public string SummonCooldownRemainingAsString()
        {
            double time = SummonCooldownRemaining;
            if (time == 0)
                return "Ready!";
            time /= 1000;

            return time.ToString("0.0");
        }

        public bool Teleport(Vector2 toLocation)
        {
            return !this.HitDetectionWithTilesNoFixing(new Rectangle((int)toLocation.X, (int)toLocation.Y, Tile.TileWidth, Tile.TileHeight));
        }

        public bool SummonCreature(Pet aPet)
        {
            if (currentLevel < SUMMONABILITYLEVEL)
                return false;
            if (lastPetSummon >= PETSUMMONCOOLDOWN)
            {
                pet = aPet;
                //set location 1 pixel to the right of the character
                pet.Position = new Vector2(this.HitRectangle.Right + 1, this.HitRectangle.Top);
                pet.inRoom = this.inRoom;
                aPet.AquireNewTarget();
                lastPetSummon = 0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Usese the teleport ability gained at a certian level
        /// </summary>
        /// <param name="toLocation">Location to teleport to</param>
        /// <returns>true if the teleport succeded</returns>
        public bool TeleportAbility(Vector2 toLocation)
        {
            if (currentLevel < TELEPORTABILITYLEVEL || lastTeleport < TELEPORTCOOLDOWN)
                return false;
            float distance = Vector2.Distance(Position, toLocation);

            if (distance < TELEPORTDISTANCEMAX )
            {
                if (Teleport(toLocation) == true)
                {
                    lastTeleport = 0;
                    return true;
                }
            }

            return false;
        }

        public void ResetTeleportCooldown()
        {
            lastTeleport = TELEPORTCOOLDOWN;
        }

        public void Update(GameTime gameTime)
        {
            if(lastTeleport < TELEPORTCOOLDOWN)
                lastTeleport += gameTime.ElapsedGameTime.TotalMilliseconds;
            lastPetSummon += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (lastPetSummon >= PETDURATION)
                pet = null;
            if (Pet != null)
            {
                pet.Update(gameTime);
            }
        }

    }

    public class LevelUpEventArgs
    {
        public int levelAttained;
        public int XP;

        public LevelUpEventArgs(int level, int curXP) :base()
        {
            levelAttained = level;
            XP = curXP;
        }
    }
}
