using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duck_Visual
{
    internal class Card
    {
        public Vector2 Position { get; private set; }
        public Vector2 Target { get; private set; }
        public CardColor color { get; }
        public int value { get; }
        public bool selected { get; private set; }
        public bool visible { get; private set; }
        private Rectangle iRect;
        public Rectangle rect { get; private set; }

        public Card(Vector2 position, Vector2 Target, CardColor color, int value, bool visible)
        {
            Position = position;
            this.Target = Target;
            this.color = color;
            this.value = value;
            this.visible = visible;
            selected = false;
            rect = new Rectangle(300 * (value % 5), 420 * (value / 5), 300, 420);
            iRect = new Rectangle((int)position.X, (int)position.Y, 300, 420);
        }
        public bool Update(TimeSpan elapsedTime, float velocity)
        {
            Vector2 targetV = new Vector2(Target.X-Position.X, Target.Y-Position.Y);
            double movement = (float)elapsedTime.TotalMilliseconds * velocity+targetV.Length()*0.05f;
            if (movement > targetV.Length())
            {
                Position = new Vector2(Target.X, Target.Y);
                return false;
            }
            else
            {
                targetV = Vector2.Normalize(targetV);
                Position = new Vector2(Position.X+(float)(targetV.X*movement), Position.Y+(float)(targetV.Y*movement));
                return true;
            }
        }

        public void remove()
        {
            Target = new Vector2(2000, 400);
        }

        public void Show()
        {
            visible = true;
        }
        public void Unselect()
        {
            selected = !selected;
            if (selected)
            {
                Target = new Vector2(Target.X, Target.Y + 25);
            }
            else
            {
                Target = new Vector2(Target.X, Target.Y - 25);
            }
        }
        public bool Intersect(Vector2 v)
        {
            if (v.X > Position.X && v.X < Position.X + 300 && v.Y > Position.Y && v.Y < Position.Y + 420)
            {
                Unselect();
                return true;
            }
            return false;
        }
    }
}
