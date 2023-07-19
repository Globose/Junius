using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Duck_Visual
{
    public enum CardColor
    {
        Hearts = 0, Spades = 1, Diamonds = 2, Clubs =3, Joker = 4
    }

    public enum State {Start, Remove, FirstDeal, SecondDeal, Flip1, Flip2, Reveal, Select, PostGame}

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D card;
        Texture2D[] cardTextures;
        Texture2D btn;
        Texture2D head;

        List<int> allNums;

        Button btnBet;
        Button btnDeal;
        Button btnSaldo;

        int touchHandle;
        int counter;
        float VELOCITY = 0.7f;
        decimal insats = 0.5m;
        decimal saldo = 100;
        Vector2 zv;
        List<Card> cards;
        List<Button> btns;
        List<Text> texts;
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
        }

        protected override void Initialize()
        {
            cards = new List<Card>();

            zv = new Vector2(0, 0);
            IsMouseVisible = true;
            gameState = State.Start;
            touchHandle = 0;
            counter = 0;
            rand = new Random();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            card = Content.Load<Texture2D>("card_back");
            cardTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("cards_hearts"),
                Content.Load<Texture2D>("cards_spades"),
                Content.Load<Texture2D>("cards_diamonds"),
                Content.Load<Texture2D>("cards_clubs"),
                Content.Load<Texture2D>("card_jokers")
            };
            
            btn = Content.Load<Texture2D>("btn");
            head = Content.Load<Texture2D>("head");
            sFont = Content.Load<SpriteFont>("font");

            btns = new List<Button>();
            btnDeal = new Button("Dela ut", new Vector2(1400, 800), true, sFont);
            btnBet = new Button(insats.ToString() + " $", new Vector2(1100, 800), true, sFont);
            btnSaldo = new Button(saldo.ToString()+" $", new Vector2(800, 800), false, sFont);
            SetBets();

            btns.Add(btnDeal);
            btns.Add(btnBet);
            btns.Add(btnSaldo);
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            bool animating = false;
            foreach (Card c in cards.ToList())
            {
                if (c.Update(gameTime.ElapsedGameTime, VELOCITY))
                {
                    animating = true;
                }
                if (c.Position.X > 1920) cards.Remove(c);
            }

            if (animating && (gameState != State.Start && gameState != State.Select))
            {
                return;
            }

            TouchCollection tc = TouchPanel.GetState();
            
            if (tc.Count > 0)
            {
                foreach (TouchLocation tl in tc)
                {
                    if (tl.Id > touchHandle)
                    {
                        Touch(tl.Position);
                        touchHandle = tl.Id;
                    }
                }
            }
            else
            {
                btnBet.pressed = false;
                btnDeal.pressed = false;
            }


            if (gameState == State.Remove && cards.Count == 0)
            {
                FirstDeal();
                gameState = State.FirstDeal;
            }
            else if (gameState == State.FirstDeal)
            {
                gameState = State.Flip1;
            }
            else if (gameState == State.SecondDeal)
            {
                gameState = State.Flip2;
            }
            else if (gameState == State.Flip1 || gameState == State.Flip2)
            {
                counter++;
                Flip();
            }
            else if (gameState == State.Reveal)
            {
                if (cards[5].Position.Y < 980)
                {
                    VELOCITY = 0.9f;
                    cards[5].moveDown();
                }
                else
                {
                    VELOCITY = 0.7f;
                    gameState = State.PostGame;
                    cards.RemoveAt(5);
                }
            }
            else if (gameState == State.PostGame)
            {
                WinCheck();
                gameState = State.Start;
            }
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            GraphicsDevice.Clear(Color.DarkGray);

            foreach (Card c in cards)
            {
                if (!c.visible) spriteBatch.Draw(card, c.Position, Color.White);
                else spriteBatch.Draw(cardTextures[(int)c.color], c.Position, c.rect, Color.White, 0f, zv, 1f, SpriteEffects.None, 0);
            }

            foreach (Button b in btns)
            {
                spriteBatch.Draw(btn, b.Position, b.getRect(), Color.White, 0f, zv, 1f, SpriteEffects.None, 0);
                spriteBatch.DrawString(sFont, b.text, b.TextPosition, Color.Black);
            }

            spriteBatch.Draw(head, zv, Color.White);
            foreach (Text t in texts)
            {
                spriteBatch.DrawString(sFont, t.Message, t.Position, Color.Black);
            }


            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void Touch(Vector2 pos)
        {
            if (btnDeal.Intersect(pos))
            {
                if (gameState == State.Start && saldo > insats)
                {
                    foreach (Card c in cards)
                    {
                        c.remove();
                    }
                    gameState = State.Remove;
                }
                else if (gameState == State.Select)
                {
                    if (btnDeal.Intersect(pos))
                    {
                        SelectDone();
                    }
                }
            }
            else if (btnBet.Intersect(pos) && gameState == State.Start)
            {
                insats += 0.5m;
                if (insats > 2)
                {
                    insats = 0.5m;
                }
                btnBet.text = insats.ToString() + " $";
                SetBets();
            }
            else if (gameState == State.Select)
            {
                foreach (Card c in cards)
                {
                    if (c.Intersect(pos)) break;
                }
            }
        }

        private void FirstDeal()
        {
            cards = new List<Card>();
            allNums = new List<int>();

            //for (int i = 0; i < 52; i++)
            //{
            //    allNums.Add(i);
            //}
            for (int i = 0; i < 5; i++)
            {
                allNums.Add(i);
            }
            allNums.Add(52);
            allNums.Add(51);
            allNums.Add(38);
            allNums.Add(25);
            allNums.Add(12);
            

            for (int j = 0; j < 5; j++)
            {
                NewCard(j);
            }
        }

        private void SelectDone()
        {
            foreach (Card c in cards)
            {
                if (!c.selected)
                {
                    c.remove();
                }
            }

            int c1 = 0;
            foreach (Card c in cards.ToList())
            {
                if (!c.selected)
                {
                    NewCard(c1);
                }
                else c.Unselect();
                c1++;
            }

            gameState = State.SecondDeal;
        }

        private void NewCard(int pos)
        {
            int randNr = rand.Next(allNums.Count);
            int cardNr = allNums[randNr];
            allNums.RemoveAt(randNr);

            Card card = new Card(new Vector2(-310, 500), new Vector2(cardPosX[pos], 250), (CardColor)(cardNr / 13), cardNr % 13, false, false);
            cards.Add(card);
        }

        private void Flip()
        {
            if (counter % 5 != 0) return;
            List<Card> fCards = new List<Card>();
            
            foreach (Card c in cards)
            {
                if (!c.visible)
                {
                    fCards.Add(c);
                }
            }

            if (fCards.Count == 0)
            {
                if (gameState == State.Flip1) gameState = State.Select;
                else gameState = State.PostGame;
            }
            else if (fCards.Count == 1)
            {
                fCards[0].Show();
                if (gameState == State.Flip1) gameState = State.Select;
                else
                {
                    gameState = State.Reveal;
                    cards.Add(new Card(fCards[0].Position, new Vector2(fCards[0].Position.X, fCards[0].Position.Y + 70), CardColor.Diamonds, 0, false, true));
                    VELOCITY = 0.04f;
                }
            }
            else
            {
                fCards[0].Show();
            }
        }

        private void SetBets()
        {
            decimal[] bets = new decimal[] { 1, 1, 2, 3, 4, 5, 20, 40, 80, 100 };
            texts = new List<Text>();
            for (int i = 0; i < bets.Length; i++)
            {
                decimal bet = bets[i] * insats;
                texts.Add(new Text(new Vector2(22 + 190 * i, 80), bet + " $", sFont));
            }
        }

        private void WinCheck()
        {
            int[] cCount = new int[14];
            int[] pCount = new int[3];
            int jokers = 0;
            
            foreach (Card c in cards)
            {
                if (c.color != CardColor.Joker)
                {
                    cCount[c.value]++;
                }
                else jokers++;
            }

            foreach (int i in cCount)
            {
                if (i > 1)
                {
                    pCount[i-2]++;
                }
            }

            bool twoPair = false;
            int stacked = 0;

            if (pCount[0] == 1)
            {
                stacked = 2;
            }
            if (pCount[0] == 2)
            {
                twoPair = true;
            }
            else if (pCount[1] == 1)
            {
                stacked += 3;
            }
            else if (pCount[2] == 1)
            {
                stacked = 4;
            }

            bool fullHouse = stacked == 5;
            stacked += jokers;

            Console.WriteLine(stacked);
            
            if (fullHouse)
            {
                Console.WriteLine("Full house");
            }
            else if (twoPair)
            {
                Console.WriteLine("Two pairs");
            }
            else if (stacked == 2)
            {
                Console.WriteLine("Pair");
            }
            else if (stacked == 3)
            {
                Console.WriteLine("Tri");
            }
            else if (stacked == 4)
            {
                Console.WriteLine("Quad");
            }
            else if (stacked == 5)
            {
                Console.WriteLine("Penta");
            }
            

        }
    }
}
