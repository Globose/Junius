using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Duck_Visual
{
    public enum CardColor
    {
        Hearts = 0,
        Spades = 1,
        Aces = 2,
        Clubs = 3
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D card;
        Texture2D cardHearts;
        float VELOCITY = 0.8f;
        List<Card> cards;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 980;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            cards = new List<Card>();
            cards.Add(new Card(new Vector2(50,600), new Vector2(100,50), CardColor.Hearts, 8, true));
            cards.Add(new Card(new Vector2(50,600), new Vector2(430,50), CardColor.Hearts, 9, true));
            cards.Add(new Card(new Vector2(50,600), new Vector2(760,50), CardColor.Hearts, 10, true));
            cards.Add(new Card(new Vector2(50,600), new Vector2(1090,50), CardColor.Hearts, 11, true));
            cards.Add(new Card(new Vector2(50,600), new Vector2(1420,50), CardColor.Hearts, 12, false));
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            card = Content.Load<Texture2D>("card_back");
            cardHearts = Content.Load<Texture2D>("card_hearts");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        int tickCounter = 0;
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            foreach (Card c in cards)
            {
                c.Update(gameTime.ElapsedGameTime, VELOCITY);
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            GraphicsDevice.Clear(Color.CornflowerBlue);

            foreach (Card c in cards)
            {
                if (!c.visible)
                {
                    spriteBatch.Draw(card, c.Position, Color.White);
                }
                else
                {
                    Rectangle rect = new Rectangle(300 * (c.value%5), 420 * (c.value/5), 300, 420);
                    spriteBatch.Draw(cardHearts, c.Position, rect, Color.White, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                }
            }
            //spriteBatch.Draw(card, new Vector2(400, 0), Color.White);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
