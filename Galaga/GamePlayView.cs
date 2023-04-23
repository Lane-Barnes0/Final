using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Galaga.Input;

//
// Added to support serialization
using System.IO;
using System.IO.IsolatedStorage;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System;
using System.Reflection.Metadata;
using static System.Formats.Asn1.AsnWriter;

namespace Galaga
{
    public class GamePlayView : GameStateView
    {

        private bool saving = false;

        private bool m_wait;
        private Texture2D m_bulletTex;
        private double gameOverTime;
        private bool m_pause;
        private int bulletsFired;
        bool newGame = true;
        bool m_quit;
        private List<Rectangle> bullets;
        private int bulletWidth;
        private float bulletSpeed = 900.0f / 1000.0f;
        ContentManager m_contentManager;
        private SpriteFont m_font;
        private Texture2D m_squareTexture;
        ParticleEmitter m_emitter1;
        private double m_score;
        private const float SPRITE_MOVE_PIXELS_PER_MS = 600.0f / 1000.0f;
        private const int Wall_THICKNESS = 30;
        private KeyboardInput m_inputKeyboard;
        private bool m_gameOver;
        Rectangle m_player;
        Rectangle leftWall;
        Rectangle rightWall;
        private Texture2D m_background;
        private Texture2D m_playerTex;

        private string[] PauseState =
        {
            "Resume",
            "Exit",
        };
        private SpriteFont m_fontMenu;
        private SpriteFont m_fontMenuSelect;
        private int m_selection;
        private bool m_waitforkey;

        
        public void initializeNewGameState()
        {
            //Setup Input
            m_inputKeyboard = new KeyboardInput();
            m_inputKeyboard.registerCommand(Keys.Left, false, new InputDeviceHelper.CommandDelegate(onMoveLeft));
            m_inputKeyboard.registerCommand(Keys.Right, false, new InputDeviceHelper.CommandDelegate(onMoveRight));
            m_inputKeyboard.registerCommand(Keys.Escape, false, new InputDeviceHelper.CommandDelegate(onEscape));
            m_inputKeyboard.registerCommand(Keys.Space, true, new InputDeviceHelper.CommandDelegate(onSpace));
            bullets = new List<Rectangle>();
            bulletWidth = 10;
            m_player = new Rectangle(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight - 100, 30, 30);
            gameOverTime = 5;
            newGame = true;
            m_waitforkey = false;
            m_selection = 0;
            m_score = 0;
            m_pause = false;
            m_quit = false;
            bulletsFired = 0;
            m_gameOver = false;
            rightWall = new Rectangle(m_graphics.PreferredBackBufferWidth - 300, 0, Wall_THICKNESS, m_graphics.PreferredBackBufferHeight);
            leftWall = new Rectangle(270, 0, Wall_THICKNESS, m_graphics.PreferredBackBufferHeight);



            Random rand = new Random();


        }
        public override void loadContent(ContentManager contentManager)
        {
            m_contentManager = contentManager;
            m_background = contentManager.Load<Texture2D>("Images/background");
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
            m_squareTexture = contentManager.Load<Texture2D>("Images/square");
            m_fontMenu = contentManager.Load<SpriteFont>("Fonts/menu");
            m_fontMenuSelect = contentManager.Load<SpriteFont>("Fonts/menu-select");
            m_playerTex = contentManager.Load<Texture2D>("Images/Player");
            m_bulletTex = contentManager.Load<Texture2D>("Images/bullet");
            
        }

        public override GameStateEnum processInput(GameTime gameTime)
        {

            if (gameOverTime <= 0)
            {
                newGame = true;
                saveScore();
                return GameStateEnum.MainMenu;
            }

            if (m_quit)
            {
              newGame = true;
              return GameStateEnum.MainMenu;
                
            }
            return GameStateEnum.NewGame;
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();
            m_spriteBatch.Draw(m_background, new Rectangle(0, 0, m_graphics.PreferredBackBufferWidth, m_graphics.PreferredBackBufferHeight), Color.White);
            if (!m_quit)
            {
               
                if (m_pause)
                {
                    renderMenu();
                }
                else
                {

                    drawScore();
                    m_spriteBatch.Draw(m_playerTex, m_player, Color.White);
                    drawBullets();


                    if (m_gameOver)
                    {
                        m_emitter1.draw(m_spriteBatch);
                        Vector2 stringSize = m_font.MeasureString("Game Over" + m_score.ToString());
                        m_spriteBatch.DrawString(
                           m_font,
                            "Game Over",
                           new Vector2((m_graphics.PreferredBackBufferWidth - stringSize.X) / 2, m_graphics.PreferredBackBufferHeight / 2),
                           Color.Red);
                    }

                }

            }
            m_spriteBatch.End();

        }

        public override void update(GameTime gameTime)
        {
            if (newGame)
            {
                initializeNewGameState();
                newGame = false;
            }
            if (!m_pause)
            {
                    if (!m_gameOver)
                    {
                       
                        m_inputKeyboard.Update(gameTime);
                        m_score += gameTime.ElapsedGameTime.Milliseconds;



                    //Update Each bullet Position
                    for (int i = 0; i < bullets.Count; i ++)
                    {
                        int moveDistance = (int)(gameTime.ElapsedGameTime.TotalMilliseconds * bulletSpeed);
                        bullets[i] = new Rectangle(bullets[i].X, bullets[i].Y - moveDistance, bulletWidth, bulletWidth);
                    }

                    //Delete Any bullets off Screen
                    List<Rectangle> newBullets = new List<Rectangle>();
                    for (int i = 0; i < bullets.Count; i++)
                    {
                        if (bullets[i].Y > 0)
                        {
                            newBullets.Add(bullets[i]);
                        }
                        
                    }

                    bullets = newBullets;


                } else
                    {
                        
                        m_emitter1.update(gameTime);
                        gameOverTime -= gameTime.ElapsedGameTime.TotalSeconds;
                    }
                }
            else
            {
                pauseInput();
            }
        }

        
        private void drawScore()
        {
            Vector2 stringSize = m_font.MeasureString("Score " + m_score.ToString());
            m_spriteBatch.DrawString(
               m_font,
                "Score " + (m_score / 1000).ToString(),
               new Vector2((m_graphics.PreferredBackBufferWidth - stringSize.X) / 2, 10),
               Color.Red);


            stringSize = m_font.MeasureString("Bullets Fired: " + m_score.ToString());
            m_spriteBatch.DrawString(
               m_font,
                "Bullets Fired: " + (bulletsFired).ToString(),
               new Vector2(m_graphics.PreferredBackBufferWidth - stringSize.X, m_graphics.PreferredBackBufferHeight - 100),
               Color.Yellow);

        }

        private void onMoveLeft(GameTime gameTime, float scale)
        {

            int moveDistance = (int)(gameTime.ElapsedGameTime.TotalMilliseconds * SPRITE_MOVE_PIXELS_PER_MS * scale);
            if (!intersect(leftWall, m_player))
            {
                m_player.X = m_player.X - moveDistance;
            }


        }

        private void onMoveRight(GameTime gameTime, float scale)
        {

            int moveDistance = (int)(gameTime.ElapsedGameTime.TotalMilliseconds * SPRITE_MOVE_PIXELS_PER_MS * scale);
            if (!intersect(rightWall, m_player))
            {
                m_player.X = m_player.X + moveDistance;
            }


        }

        private void onEscape(GameTime gameTime, float scale)
        {
            m_pause = !m_pause;
        }

        private void onSpace(GameTime gameTime, float scale)
        {
            //Fire Bullets 
            bullets.Add(new Rectangle(m_player.Center.X - bulletWidth/2, m_player.Y - 5, bulletWidth, bulletWidth));
            bulletsFired += 1;

        }

        private void drawBullets()
        {
            foreach(Rectangle bullet in bullets)
            {
                m_spriteBatch.Draw(m_bulletTex, bullet, Color.White);
            }

        }
        private bool intersect(Rectangle r1, Rectangle r2)
        {
            bool theyDo = !(
            r2.Left > r1.Right ||
            r2.Right < r1.Left ||
            r2.Top > r1.Bottom ||
            r2.Bottom < r1.Top);
            return theyDo;
        }

  


        private float drawMenuItem(SpriteFont font, string text, float y, Color color)
        {
            Vector2 stringSize = font.MeasureString(text);
            m_spriteBatch.DrawString(
                font,
                text,
                new Vector2(m_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, y),
                color);

            return y + stringSize.Y;
        }


        private void renderMenu()
        {
            Vector2 stringSize = m_font.MeasureString("Game Paused");
            m_spriteBatch.DrawString(
               m_font,
                "Game Paused",
               new Vector2(m_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, 100),
               Color.White);

            float bottom = drawMenuItem(
                m_selection == 0 ? m_fontMenuSelect : m_fontMenu,
                "Resume",
                200,
                m_selection == 0 ? Color.Yellow : Color.Blue);
            drawMenuItem(m_selection == 1 ? m_fontMenuSelect : m_fontMenu, "Quit", bottom, m_selection == 1 ? Color.Yellow : Color.Blue);

        }

        public void pauseInput()
        {
            // This is the technique I'm using to ensure one keypress makes one menu navigation move
            if (!m_waitforkey)
            {
                // Arrow keys to navigate the menu
                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                {

                    if (m_selection != 1)
                    {
                        m_selection = m_selection + 1;
                    }

                    m_waitforkey = true;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Up))
                {
                    if (m_selection != 0)
                    {
                        m_selection = m_selection - 1;
                    }

                    m_waitforkey = true;
                }

                // If enter is pressed, return the appropriate new state
                if (Keyboard.GetState().IsKeyDown(Keys.Enter) && m_selection == 0)
                {
                    m_pause = false;
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.Enter) && m_selection == 1)
                {
                    m_quit = true;
                    m_waitforkey = true;
                }
            }
            else if (Keyboard.GetState().IsKeyUp(Keys.Down) && Keyboard.GetState().IsKeyUp(Keys.Up))
            {
                m_waitforkey = false;
            }


        }

        private void saveScore()
        {
            lock (this)
            {
                if (!this.saving)
                {
                    this.saving = true;
                    //
                    // Create something to save
                    Scoring myState = new Scoring((int)m_score/1000);
                    finalizeSaveAsync(myState);
                }
            }
        }

        private async void finalizeSaveAsync(Scoring state)
        {
            await Task.Run(() =>
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    try
                    {
                        using (IsolatedStorageFileStream fs = storage.OpenFile("HighScores.xml", FileMode.Create))
                        {
                            if (fs != null)
                            {
                                XmlSerializer mySerializer = new XmlSerializer(typeof(Scoring));
                                mySerializer.Serialize(fs, state);
                            }
                        }
                    }
                    catch (IsolatedStorageException)
                    {
                        // Ideally show something to the user, but this is demo code :)
                    }
                }

                this.saving = false;
            });
        }

    }
}