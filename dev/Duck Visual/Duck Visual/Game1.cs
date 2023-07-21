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
using System.Security.Permissions;

namespace Duck_Visual
{
    public enum CardColor
    {
        Hearts = 0, Spades = 1, Diamonds = 2, Clubs =3, Joker = 4
    }

    public enum State {Start, Remove, FirstDeal, SecondDeal, Flip1, Flip2, Reveal, Select, PostGame, MovingText, Counter}

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
        Texture2D blob;

        List<int> allNums;

        Button btnBet;
        Button btnDeal;
        Button btnSaldo;

        decimal targetSaldo;
        decimal[] bets = new decimal[] { 1, 1, 2, 3, 4, 5, 20, 40, 80, 100 };
        int touchHandle;
        int counter;
        float VELOCITY = 0.7f;
        decimal insats = 0.5m;
        decimal saldo = 100;
        Vector2 zv;
        Vector2 blobPos;
        List<Card> cards;
        List<Button> btns;
        List<Text> texts;
        SpriteFont sFont;
        State gameState;
        Random rand;

        Text movingText;
        Vector2 movingTextTarget = new Vector2(865, 850);

        int[] cardPosX = new int[] { 135, 465, 795, 1125, 1455 };

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            graphics.IsFullScreen = true;
        }

        protected override void Initialize()
        {
            cards = new List<Card>();

            zv = new Vector2(0, 0);
            blobPos = new Vector2(17+1 * 190, 78);
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

            blob = Content.Load<Texture2D>("blob");
            btn = Content.Load<Texture2D>("btn");
            head = Content.Load<Texture2D>("head");
            sFont = Content.Load<SpriteFont>("font");

            btns = new List<Button>();
            btnDeal = new Button("Dela ut", new Vector2(1400, 800), true, sFont);
            btnBet = new Button(insats.ToString() + " $", new Vector2(1100, 800), true, sFont);
            btnSaldo = new Button(saldo.ToString()+" $", new Vector2(800, 800), false, sFont);
            SetBets();
            movingText = new Text(new Vector2(-300, 0), "", sFont);

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
            if (gameState == State.MovingText)
            {
                Vector2 targetV = new Vector2(movingTextTarget.X - movingText.Position.X, movingTextTarget.Y - movingText.Position.Y);

                if (targetV.Length() < 15)
                {
                    movingText.Position = new Vector2(-300, 0);
                    gameState = State.Counter;
                    VELOCITY = 0.7f;
                }
                else
                {
                    double movement = (float)gameTime.ElapsedGameTime.TotalMilliseconds * VELOCITY + targetV.Length() * 0.05f;
                    targetV = Vector2.Normalize(targetV);
                    movingText.Position = new Vector2(movingText.Position.X + (float)(targetV.X * movement), movingText.Position.Y + (float)(targetV.Y * movement));
                    animating = true;
                }
            }
            else if (gameState == State.Counter)
            {
                if (saldo < targetSaldo)
                {
                    saldo += 0.5m;
                    animating = true;
                    btnSaldo.text = saldo + " $";
                }
                else gameState = State.Start;
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
            spriteBatch.Draw(blob, blobPos, Color.White);
            spriteBatch.DrawString(sFont, movingText.Message, movingText.Position, Color.Black);
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
                if (gameState == State.Start && saldo >= insats)
                {
                    saldo -= insats;
                    btnSaldo.setText(saldo + " $", sFont);
                    blobPos = new Vector2(-200, blobPos.Y);
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

            for (int i = 0; i < 55; i++)
            {
                allNums.Add(i);
            }



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
                    cards.Add(new Card(fCards[0].Position, new Vector2(fCards[0].Position.X, fCards[0].Position.Y + 40), CardColor.Diamonds, 0, false, true));
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
            texts = new List<Text>();
            for (int i = 0; i < bets.Length; i++)
            {
                decimal bet = bets[i] * insats;
                texts.Add(new Text(new Vector2(22 + 190 * i, 80), bet + " $", sFont));
            }
        }

        private void WinCheck()
        {
            int[] cCount = new int[15];
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

            bool kingAcesPair = cCount[0] == 2 || cCount[12] == 2;
            if ((cCount[0] == 1 || cCount[12] == 1) && jokers > 0) kingAcesPair = true;

            foreach (int i in cCount)
            {
                if (i > 1)
                {
                    pCount[i-2]++;
                }
            }

            bool fullHouse = false;
            bool twoPair = false;
            int stacked = 1;

            if (pCount[0] == 1)
            {
                stacked = 2;
            }
            if (pCount[0] == 2)
            {
                twoPair = true;
                if (jokers == 1) fullHouse = true;
            }
            else if (pCount[1] == 1)
            {
                fullHouse = stacked == 2;
                stacked = 3;
            }
            else if (pCount[2] == 1)
            {
                stacked = 4;
            }

            stacked += jokers;
            bool tri = false;
            bool quad = false;
            bool penta = false;


            if (stacked == 3) tri = true;
            else if (stacked == 4) quad = true;
            else if (stacked == 5 && !fullHouse) penta = true;

            List<Card> noJoker = new List<Card>();
            foreach (Card card in cards)
            {
                if (card.color != CardColor.Joker)
                {
                    noJoker.Add(card);
                }
            }
            
            CardColor color = noJoker[0].color;
            bool flush = true;
            foreach (Card card in noJoker)
            {
                if (card.color != color && card.color != CardColor.Joker)
                {
                    flush = false;
                }
            }

            bool royal = false;
            noJoker.Sort();
            if (noJoker.First().value > 8 && (noJoker.Last().value > 8 || noJoker.Last().value == 0))
            {
                if (noJoker.Last().value == 0)
                {
                    List<Card> newJoker = new List<Card>{noJoker.Last()};
                    newJoker.First().setAce();
                    for (int i = 0; i < noJoker.Count-1; i++) 
                    {
                        newJoker.Add(noJoker[i]);
                    }
                    noJoker = newJoker;
                }
                royal = true;
            }

            int gap = 0;
            for (int i = 0; i < noJoker.Count-1; i++)
            {
                int tGap = noJoker[i].getDistance(noJoker[i + 1]);
                if (tGap == 0) gap = 100;
                else gap += tGap-1;
            }
            foreach (Card card in noJoker)
            {
                if (card.value == 13) card.setAce();
            }

            bool straight = gap <= jokers;

            int winCounter = -1;
            if (penta) winCounter = 9;
            else if (flush && royal && straight) winCounter = 8;
            else if (flush && straight) winCounter = 7;
            else if (quad) winCounter = 6;
            else if (fullHouse) winCounter = 5;
            else if (flush) winCounter = 4;
            else if (straight) winCounter = 3;
            else if (tri) winCounter = 2;
            else if (twoPair) winCounter = 1;
            else if (kingAcesPair) winCounter = 0;

            if (winCounter != -1)
            {
                blobPos = new Vector2(17 + 190 * winCounter, blobPos.Y);
                movingText.Message = bets[winCounter] * insats + " $";
                movingText.Position = new Vector2(17 + 190 * winCounter, blobPos.Y + 10);
                targetSaldo = saldo + bets[winCounter] * insats;
                gameState = State.MovingText;
                VELOCITY = 0.1f;
            }
            else
            {
                targetSaldo = saldo;
                gameState = State.Start;
            }
        }
    }
}
