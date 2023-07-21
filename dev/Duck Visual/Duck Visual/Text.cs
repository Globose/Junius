using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Duck_Visual
{
    internal class Text
    {
        public Vector2 Position { get; set; }

        public string Message { get; set; }
        public Text(Vector2 position, string message, SpriteFont sf) 
        {
            Message = message;

            Vector2 size = sf.MeasureString(message);
            Position = new Vector2(position.X + 86 - size.X / 2, position.Y + 39 - size.Y / 2);
        }
    }
}
