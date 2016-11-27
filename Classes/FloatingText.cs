using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KSshooter.Classes
{
    class FloatingText
    {
        string _text;
        SpriteFont font;
        Vector2 position;
        double duration; //milliseconds
        double age; //millisconds
        bool onscreen;
        public Color _color;
        float _rotation;
        Vector2 _origin;
        float _scale;
        SpriteEffects spriteEffects;

        //Font Effects
        public enum FontEffects { None, shadow, fade };
        FontEffects fontEffects;
        //shadow
        Vector2 shadowOffset;
        Color shadowColor;

        //Text movement
        bool textMovement;
        Vector2 textEndLocation;
        Vector2 textStartLocation;
        Vector2 MovementDelta;
        Vector2 movementSoFar;
        public enum TextMovemenType {linear, sin }
        TextMovemenType textMovementType;
        
        public delegate void DurationExpiredEventHandler(object sender, EventArgs e);
        public event DurationExpiredEventHandler DurationExpired;

        public FloatingText(String text, SpriteFont aFont, Vector2 location, double durationMilliseconds, FontEffects effects = FontEffects.None, float drawLayer = 0)
        {
            age = 0;
            _text = text;
            font = aFont;
            position = location;
            duration = durationMilliseconds;
            _color = Color.White;
            _origin = new Vector2(0, 0);
            _scale = 1;
            _rotation = 0f;
            spriteEffects = SpriteEffects.None;
            onscreen = true;
            fontEffects = effects;
        }

        public string Text
        {
            set
            {
                _text = value;
            }
            get { return Text; }
        }

        public Vector2 StartLocation
        {
            get { return position; }
        }

        public void AddTextMovement(Vector2 endLocation, TextMovemenType movementType)
        {
            textEndLocation = endLocation;
            textStartLocation = new Vector2(position.X, position.Y);
            MovementDelta = endLocation - textStartLocation;
            textMovementType = movementType;
            textMovement = true;
        }

        public void AddShadowEffect(Vector2 offset, Color color)
        {
            fontEffects = FontEffects.shadow;
            shadowOffset = offset;
            shadowColor = color;
        }

        public void update(GameTime gameTime)
        {
            age += gameTime.ElapsedGameTime.Milliseconds;
            if (age >= duration)
            {
                if (DurationExpired != null)
                    DurationExpired(this, new EventArgs());
            }
            if (textMovement)
            {
                switch (textMovementType)
                {
                    case TextMovemenType.linear:
                        movementSoFar = MovementDelta * (float)(age / duration);
                        position = textStartLocation + movementSoFar;
                        break;
                    case TextMovemenType.sin:
                        movementSoFar = (float)Math.Sin((age / duration) * (Math.PI / 2)) * MovementDelta;
                        position = textStartLocation + movementSoFar;
                        break;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offset, Rectangle veiwPort)
        {
            Vector2 screenPosition = position + offset;
            if (fontEffects == FontEffects.shadow)
            {
                spriteBatch.DrawString(font, _text, screenPosition + shadowOffset, shadowColor, _rotation, _origin, _scale, spriteEffects, 0f);
            }
            if (fontEffects == FontEffects.None || fontEffects == FontEffects.shadow)
            {
                spriteBatch.DrawString(font, _text, screenPosition, _color, _rotation, _origin, _scale, spriteEffects, 0f);
            }
            else if (fontEffects == FontEffects.fade)
            {
                float l = (float)((duration - age) / duration);
                Color colorr = new Color(
                    (byte)((float)_color.R * l),
                    (byte)((float)_color.G * l),
                    (byte)((float)_color.B * l),
                    (byte)((float)_color.A * l));
                spriteBatch.DrawString(font, _text, screenPosition, colorr, _rotation, _origin, _scale, spriteEffects, 0f);
            }
        }
    }
}
