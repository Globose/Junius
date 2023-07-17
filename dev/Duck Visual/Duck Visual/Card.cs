using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public CardColor color { get;}
        public int value { get; }
        public bool visible { get; private set; }

        public Card(Vector2 position, Vector2 Target, CardColor color, int value, bool visible)
        {
            Position = position;
            this.Target = Target;
            this.color = color;
            this.value = value;
            this.visible = visible;
        }
        public void Update(TimeSpan elapsedTime, float velocity)
        {
            Vector2 targetV = new Vector2(Target.X-Position.X, Target.Y-Position.Y);
            double movement = (float)elapsedTime.TotalMilliseconds * velocity;
            if (movement > targetV.Length())
            {
                Position = new Vector2(Target.X, Target.Y);
            }
            else
            {
                targetV = Vector2.Normalize(targetV);
                Position = new Vector2(Position.X+(float)(targetV.X*movement), Position.Y+(float)(targetV.Y*movement));
            }
        }

        public void Show()
        {
            visible = true;
        }
    }
}
