using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Duck_Visual
{
    public enum CardColor
    {
        Hearts = 0, Spades = 1, Diamonds = 2, Clubs =3
    }

    public enum State {Menu, FirstDeal, Reset, SecondDeal}

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D card;
        Texture2D cardHearts;
        Texture2D cardSpades;
        Texture2D cardDiamonds;
        Texture2D cardClubs;
        Texture2D btn;
        Texture2D head;

        List<int> allNums;

        Button btnBet;
        Button btnDeal;
        Button btnSaldo;

        int counter = 0;
        bool waitFlip = false;
        bool lockScreen;
        bool newGame;
        float VELOCITY = 0.7f;
        decimal insats = 0.5m;
        decimal saldo = 100;
        Vector2 zv;
        Vector2 headPos;
        List<Card> cards;
        List<Button> btns;
        SpriteFont sFont;
        State gameState;
        Random rand;

        int[] cardPosX = new int[] { 135, 465, 795, 1125, 1455 };

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 980;
            IsMouseVisible = true;
            lockScreen = false;
            newGame = false;
            gameState = State.Menu;
            rand = new Random();
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
            cards.Add(new Card(new Vector2(50, 600), new Vector2(135, 250), CardColor.Hearts, 8, true));
            cards.Add(new Card(new Vector2(50, 600), new Vector2(465, 250), CardColor.Clubs, 9, true));
            cards.Add(new Card(new Vector2(50, 600), new Vector2(795, 250), CardColor.Hearts, 10, true));
            cards.Add(new Card(new Vector2(50, 600), new Vector2(1125, 250), CardColor.Hearts, 11, true));
            cards.Add(new Card(new Vector2(50, 600), new Vector2(1455, 250), CardColor.Hearts, 12, true));

            zv = new Vector2(0, 0);
            headPos = new Vector2(100,30);
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
            cardHearts = Content.Load<Texture2D>("cards_hearts");
            cardSpades = Content.Load<Texture2D>("cards_spades");
            cardDiamonds = Content.Load<Texture2D>("cards_diamonds");
            cardClubs = Content.Load<Texture2D>("cards_clubs");
            btn = Content.Load<Texture2D>("btn");
            head = Content.Load<Texture2D>("head");
            sFont = Content.Load<SpriteFont>("font");

            btns = new List<Button>();
            btnDeal = new Button("Dela ut", new Vector2(1400, 800), true, sFont);
            btnBet = new Button(insats.ToString() + " $", new Vector2(1100, 800), true, sFont);
            btnSaldo = new Button(saldo.ToString()+" $", new Vector2(800, 800), false, sFont);
            btns.Add(btnDeal);
            btns.Add(btnBet);
            btns.Add(btnSaldo);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            bool cardsUpdated = false;
            foreach (Card c in cards)
            {
                if (c.Update(gameTime.ElapsedGameTime, VELOCITY))
                {
                    cardsUpdated = true;
                }
            }
            if (cardsUpdated)
            {
                return;
            }

            TouchCollection tc = TouchPanel.GetState();

            if (gameState == State.Menu)
            {
                if (tc.Count > 0 && !lockScreen)
                {
                    TouchLocation tl = tc[0];
                    lockScreen = true;

                    if (btnDeal.Intersect(tl.Position))
                    {
                        if (saldo > insats)
                        {
                            gameState = State.FirstDeal;
                            foreach (Card c in cards)
                            {
                                c.remove();
                            }
                            btnDeal.pressed = false;
                            newGame = true;
                        }
                    }
                    else if (btnBet.Intersect(tl.Position))
                    {
                        insats += 0.5m;
                        if (insats > 2)
                        {
                            insats = 0.5m;
                        }
                        btnBet.text = insats.ToString() + " $";
                    }
                }
                else if (lockScreen && tc.Count == 0)
                {
                    lockScreen = false;
                    btnDeal.pressed = false;
                    btnBet.pressed = false;
                }
            }
            else if (gameState == State.FirstDeal)
            {
                if (newGame)
                {
                    cards = new List<Card>();
                    allNums = new List<int>();

                    for (int i = 0; i < 52; i++)
                    {
                        allNums.Add(i);
                    }
                    for (int j = 0; j < 5; j++)
                    {
                        int randNr = rand.Next(allNums.Count);
                        int cardNr = allNums[randNr];
                        allNums.RemoveAt(randNr);

                        Card card = new Card(new Vector2(-310, 500), new Vector2(135 + 330 * j, 250), (CardColor)(cardNr / 13), cardNr % 13, false);
                        cards.Add(card);
                    }
                    lockScreen = true;
                    newGame = false;
                    waitFlip = true;
                    counter = 1;
                }
                else if (tc.Count > 0 && !lockScreen)
                {
                    TouchLocation tl = tc[0];
                    lockScreen = true;

                    foreach (Card c in cards)
                    {
                        if (c.Intersect(tl.Position)) break;
                    }
                    if (btnDeal.Intersect(tl.Position))
                    {
                        foreach (Card c in cards)
                        {
                            if (!c.selected)
                            {
                                c.remove();
                            }
                        }
                        gameState = State.SecondDeal;
                        newGame = true;
                    }
                }
                else if (lockScreen && tc.Count == 0)
                {
                    lockScreen = false;
                    btnDeal.pressed = false;
                    btnBet.pressed = false;
                }
                if (counter % 5 == 0)
                {
                    bool upd = false;
                    foreach (Card c in cards) 
                    { 
                        if (!c.visible)
                        {
                            c.Show();
                            upd = true;
                            break;
                        }
                    }
                    if (!upd)
                    {
                        waitFlip = false;
                    }
                    counter++;
                }
                if (waitFlip) counter++;
            }
            else if (gameState == State.SecondDeal)
            {
                if (newGame)
                {
                    int c1 = 0;
                    foreach (Card c in cards.ToList())
                    { 
                        if (!c.selected)
                        {
                            int randNr = rand.Next(allNums.Count);
                            int cardNr = allNums[randNr];
                            allNums.RemoveAt(randNr);

                            Card card = new Card(new Vector2(-310, 500), new Vector2(cardPosX[c1], 250), (CardColor)(cardNr / 13), cardNr % 13, false);
                            cards.Add(card);
                            cards.Remove(c);
                        }
                        else
                        {
                            c.Unselect();
                        }
                        c1++;
                    }
                    newGame = false;
                    waitFlip = true;
                    counter = 1;
                }
                else if (counter % 5 == 0)
                {
                    bool upd = false;
                    foreach (Card c in cards)
                    {
                        if (!c.visible)
                        {
                            c.Show();
                            upd = true;
                            break;
                        }
                    }
                    if (!upd)
                    {
                        waitFlip = false;
                        gameState = State.Menu;
                    }
                }
                counter++;
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
            GraphicsDevice.Clear(Color.DarkGray);

            foreach (Card c in cards)
            {
                Texture2D tx = cardHearts;
                switch (c.color)
                {
                    case CardColor.Spades:
                        tx = cardSpades;
                        break;   
                    case CardColor.Diamonds:
                        tx = cardDiamonds;
                        break;
                    case CardColor.Clubs:
                        tx = cardClubs;
                        break;
                }

                if (!c.visible) spriteBatch.Draw(card, c.Position, Color.White);
                else spriteBatch.Draw(tx, c.Position, c.rect, Color.White, 0f, zv, 1f, SpriteEffects.None, 0);
            }

            //spriteBatch.Draw(card, new Vector2(400, 0), Color.White);

            foreach (Button b in btns)
            {
                spriteBatch.Draw(btn, b.Position, b.getRect(), Color.White, 0f, zv, 1f, SpriteEffects.None, 0);
                spriteBatch.DrawString(sFont, b.text, b.TextPosition, Color.Black);
            }

            spriteBatch.Draw(head, headPos, Color.White);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
