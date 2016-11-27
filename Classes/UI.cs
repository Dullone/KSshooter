using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace KSshooter.Classes
{
    class UI
    {
        Level _level;
        SpriteFont spritefont;
        Viewport viewport;
        ContentManager Content;
        //background
        Texture2D cooldownBackground;
        int backgroundPadding = 5;

        Vector2 CooldownDrawSpot;

        //Floating texts
        List<FloatingText> Texts;
        List<FloatingText> ExpiredTexts;

        //Darken/lighten
        Texture2D darkenTexture;
        bool darkenScreen = false;

        //CharacterSheet
        bool showCharacterSheet = false;
        Player CharacterSheet;

        //health
        Texture2D health;
        Texture2D healthRed;

        int CooldownTextPaddingX = 14;
        int CooldownTextPaddingY = -5;

        public UI(Level level, SpriteFont font, Viewport viewPort, ContentManager content)
        {
            spritefont = font;
            _level = level;
            viewport = viewPort;
            CooldownDrawSpot = new Vector2(40, viewPort.Height - 70);
            Texts = new List<FloatingText>(20);
            ExpiredTexts = new List<FloatingText>(10);
            Content = content;
            darkenTexture = Content.Load<Texture2D>("DarkBlend");
            health = content.Load<Texture2D>("PlayerHealth");
            healthRed = content.Load<Texture2D>("PlayerHealthR");
        }

        //Propterties
        public Texture2D CooldownBackground
        {
            set { cooldownBackground = value; }
        }

        public bool ShowCharacterSheet
        {
            get { return showCharacterSheet; }
        }

        public void AddFloatingText(FloatingText aText)
        {
            Texts.Add(aText);
            aText.DurationExpired += new FloatingText.DurationExpiredEventHandler(aText_DurationExpired);
        }

        void aText_DurationExpired(object sender, EventArgs e)
        {
            ExpiredTexts.Add(sender as FloatingText);
        }

        public void Update(GameTime gameTime)
        {
            foreach (FloatingText text in Texts)
            {
                text.update(gameTime);
            }
            foreach (FloatingText text in ExpiredTexts)
            {
                Texts.Remove(text);
            }
            ExpiredTexts.Clear();
        }

        public void ChangedRoom()
        {
            //clear floating texts
            Texts.Clear();
        }

        public void DarkenScreen()
        {
            //test
            darkenScreen = true;
        }

        public void UnDarkenScreen()
        {
            darkenScreen = false;
        }

        public void ToggleCharacterSheet()
        {
            showCharacterSheet = !showCharacterSheet;
            CharacterSheet = _level.player;
            darkenScreen = !darkenScreen;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 veiwPortOffSet)
        {
            //draw Cooldown time left
            string teleport = "1. Teleport";
            string summon = "2. Summon";
            Vector2 teleportStringSize = spritefont.MeasureString(teleport);
            Vector2 summonStringSize = spritefont.MeasureString(summon);

            int boxHeight = (int)(teleportStringSize.Y * 2) + backgroundPadding + (int)CooldownTextPaddingY;
            int boxWidth = (int)teleportStringSize.X + backgroundPadding * 2;

            if (cooldownBackground != null)
            {
                spriteBatch.Draw(cooldownBackground,
                    new Rectangle((int)CooldownDrawSpot.X - backgroundPadding, (int)CooldownDrawSpot.Y, boxWidth, boxHeight),
                    Color.White);
                boxWidth = (int)summonStringSize.X + backgroundPadding * 2;
            }

            //Teleport coolodwn
            spriteBatch.DrawString(spritefont, teleport, CooldownDrawSpot, Color.Red);
            spriteBatch.DrawString
                (spritefont,
                _level.player.TeleportCoolDownRemainingAsString(),
                new Vector2(CooldownDrawSpot.X, CooldownDrawSpot.Y + teleportStringSize.Y + CooldownTextPaddingY),
                Color.White);
            //pet summon cooldown
            if (_level.player.CharacterLevel >= Player.SUMMONABILITYLEVEL)
            {
                spriteBatch.Draw(cooldownBackground, new Rectangle((int)(CooldownDrawSpot.X + teleportStringSize.X + CooldownTextPaddingX - backgroundPadding), (int)CooldownDrawSpot.Y, boxWidth, boxHeight), Color.White);
                spriteBatch.DrawString(spritefont,
                    summon,
                    new Vector2(CooldownDrawSpot.X + teleportStringSize.X + CooldownTextPaddingX, CooldownDrawSpot.Y),
                    Color.Red);
                spriteBatch.DrawString(spritefont,
                    _level.player.SummonCooldownRemainingAsString(),
                    new Vector2(CooldownDrawSpot.X + teleportStringSize.X + CooldownTextPaddingX, CooldownDrawSpot.Y + summonStringSize.Y + CooldownTextPaddingY),
                    Color.White);
            }

            //Draw Floating texts
            foreach (FloatingText t in Texts)
            {
                t.Draw(spriteBatch, veiwPortOffSet, viewport.Bounds);
            }
            if (darkenScreen == true)
            {
                spriteBatch.Draw(darkenTexture, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.White);
            }
            if (showCharacterSheet == true)
            {
                Texture2D sheet = Content.Load<Texture2D>("CharacterSheet");
                Vector2 sheetLoc = new Vector2(viewport.Width / 2 - sheet.Width / 2, viewport.Height / 2 - sheet.Height / 2);
                spriteBatch.Draw(sheet, sheetLoc, Color.White);
                spriteBatch.DrawString(spritefont, _level.player.CharacterLevel.ToString(), sheetLoc + new Vector2(63, 13), Color.White);
                spriteBatch.DrawString(spritefont, _level.player.ExperiencePoints.ToString(), sheetLoc + new Vector2(175, 55), Color.White);
                spriteBatch.DrawString(spritefont, _level.player.XPtoNextLevel.ToString(), sheetLoc + new Vector2(100, 82), Color.White);

                int i = 0;
                foreach (string abil in _level.player.PlayerAbilities)
                {
                    if(_level.player.CharacterLevel > i)
                        spriteBatch.DrawString(spritefont,(i+1).ToString() + ". " + abil, sheetLoc + new Vector2(20, 130 + i * teleportStringSize.Y + 10), Color.White);
                    i++;
                }
            }
            DrawPlayerHealth(spriteBatch);

        }

        private void DrawPlayerHealth(SpriteBatch spriteBatch)
        {
            float healthPercent = _level.player.Health/_level.player.MaxHealth;
            int paintW = (int)Math.Round((float)health.Width - health.Width*healthPercent);
            int paintX = health.Width - paintW;
            spriteBatch.Draw(health, new Vector2(0, 0), Color.White);
            spriteBatch.Draw(healthRed, new Vector2(paintX, 0), new Rectangle((int)paintX, 0, (int)paintW, healthRed.Height), Color.White);
        }
    }
}
