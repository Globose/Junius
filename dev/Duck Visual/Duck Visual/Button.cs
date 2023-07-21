using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duck_Visual
{
    internal class Button
    {
        public string text { get; set; }
        public Vector2 Position { get; }
        public Vector2 TextPosition { get; private set; }
        public bool pressed { get; set; }
        public bool clickable { get; }
        private Rectangle[] rectangles { get; }
        
        public Button(string text, Vector2 position, bool clickable, SpriteFont sf)
        {
            Position = position;
            setText(text, sf);
            pressed = false;
            this.clickable = clickable;
            rectangles = new Rectangle[] {new Rectangle(0,0,230,115), 
                new Rectangle(230,0,230,115), new Rectangle(460,0,230,115)};
        }

        public void setText(string text, SpriteFont sf)
        {
            Vector2 size = sf.MeasureString(text);
            TextPosition = new Vector2(Position.X + 115 - size.X / 2, Position.Y + 57 - size.Y / 2);
            this.text = text;
        }

        public Rectangle getRect()
        {
            if (!clickable) return rectangles[2];
            if (pressed) return rectangles[1];
            return rectangles[0];
        }
        public bool Intersect(Vector2 v)
        {
            if (v.X > Position.X && v.X < Position.X + 230 && v.Y > Position.Y && v.Y < Position.Y + 115)
            {
                pressed = true;
            }
            else
            {
                pressed = false;
            }
            return pressed;
        }

    }
}
