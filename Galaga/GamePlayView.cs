﻿using Microsoft.Xna.Framework.Content;
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
        private bool m_pause;
        bool newGame;
        bool m_quit;
        private bool m_gameOver;
        private bool m_waitforkey;
        private int m_selection;


        private const int Wall_THICKNESS = 30;
        private int m_lives;
        private int bulletsFired;
        private int bulletWidth;
        private float bulletSpeed = 900.0f / 1000.0f;
        private const float SPRITE_MOVE_PIXELS_PER_MS = 600.0f / 1000.0f;

        private double gameOverTime;
        private double m_score;

        private SpriteFont m_font;
        private SpriteFont m_fontMenu;
        private SpriteFont m_fontMenuSelect;

        private ContentManager m_contentManager;
        private ParticleEmitter m_emitter1;
        
        private List<Rectangle> bullets;
        private List<Rectangle> deleteBullets;
        private List<Enemy> enemies;
        private List<Enemy> deleteEnemies;

        Rectangle m_player;
        Rectangle leftWall;
        Rectangle rightWall;

        private KeyboardInput m_inputKeyboard;
        private Texture2D m_bulletTex;
        private Texture2D m_squareTexture;
        private Texture2D m_background;
        private Texture2D m_playerTex;
        private Texture2D m_bee;
        private Texture2D m_boss;
        private Texture2D m_butterfly;

        private string[] PauseState =
        {
            "Resume",
            "Exit",
        };
        
        public void initializeNewGameState()
        {
            //Setup Input
            m_inputKeyboard = new KeyboardInput();
            m_inputKeyboard.registerCommand(Keys.Left, false, new InputDeviceHelper.CommandDelegate(onMoveLeft));
            m_inputKeyboard.registerCommand(Keys.Right, false, new InputDeviceHelper.CommandDelegate(onMoveRight));
            m_inputKeyboard.registerCommand(Keys.Escape, false, new InputDeviceHelper.CommandDelegate(onEscape));
            m_inputKeyboard.registerCommand(Keys.Space, true, new InputDeviceHelper.CommandDelegate(onSpace));

            //Bullets, Enemies, Player
            bullets = new List<Rectangle>();
            deleteBullets = new List<Rectangle>();
            bulletWidth = 10;
            bulletsFired = 0;

            enemies = new List<Enemy>();
            deleteEnemies= new List<Enemy>();

            //Test Delete Later
            enemies.Add(new Enemy(m_bee, 1, new Rectangle(600, 200, 30, 30)));
            enemies.Add(new Enemy(m_boss, 1, new Rectangle(700, 200, 30, 30)));
            enemies.Add(new Enemy(m_bee, 1, new Rectangle(800, 200, 30, 30)));
            enemies.Add(new Enemy(m_butterfly, 1, new Rectangle(1000, 200, 30, 30)));

            m_player = new Rectangle(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight - 100, 30, 30);
            m_lives = 3;

            gameOverTime = 5;
            m_selection = 0;
            m_score = 0;
            

            //Bools
            newGame = true;
            m_waitforkey = false;
            m_pause = false;
            m_quit = false;
            m_gameOver = false;

            rightWall = new Rectangle(m_graphics.PreferredBackBufferWidth - 300, 0, Wall_THICKNESS, m_graphics.PreferredBackBufferHeight);
            leftWall = new Rectangle(270, 0, Wall_THICKNESS, m_graphics.PreferredBackBufferHeight);

        }
        public override void loadContent(ContentManager contentManager)
        {
            initializeNewGameState();
            m_contentManager = contentManager;
            m_background = contentManager.Load<Texture2D>("Images/background");
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
            m_squareTexture = contentManager.Load<Texture2D>("Images/square");
            m_fontMenu = contentManager.Load<SpriteFont>("Fonts/menu");
            m_fontMenuSelect = contentManager.Load<SpriteFont>("Fonts/menu-select");
            m_playerTex = contentManager.Load<Texture2D>("Images/Player");
            m_bulletTex = contentManager.Load<Texture2D>("Images/bullet");
            m_bee = contentManager.Load<Texture2D>("Images/bee");
            m_boss = contentManager.Load<Texture2D>("Images/boss");
            m_butterfly = contentManager.Load<Texture2D>("Images/butterfly");
            
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
                drawLives();
                if (m_pause)
                {
                    renderMenu();
                }
                else
                {
                    drawScore();
                    m_spriteBatch.Draw(m_playerTex, m_player, Color.White);
                    drawBullets();

                    drawEnemies();


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
                        

                    //Update Each bullet Position
                    for (int i = 0; i < bullets.Count; i ++)
                    {
                        int moveDistance = (int)(gameTime.ElapsedGameTime.TotalMilliseconds * bulletSpeed);
                        bullets[i] = new Rectangle(bullets[i].X, bullets[i].Y - moveDistance, bulletWidth, bulletWidth);
                    }

                    //Delete Any bullets off Screen
                    
                    for (int i = 0; i < bullets.Count; i++)
                    {
                        if (bullets[i].Y < 0)
                        {
                            deleteBullets.Add(bullets[i]);
                        }
                        
                    }
                    

                    //Check Bullet Collisions
                    checkBulletCollisions();
                    checkEnemyLives();

                    //Clean Up Dead Enemies and Off screen Bullets

                    if(deleteBullets.Count > 0)
                    {
                        foreach (Rectangle bullet in deleteBullets)
                        {
                            bullets.Remove(bullet);
                        }

                        deleteBullets.Clear();
                    }

                    if(deleteEnemies.Count > 0 )
                    {
                        foreach (Enemy enemy in deleteEnemies)
                        {
                            enemies.Remove(enemy);
                        }

                        deleteEnemies.Clear();
                    }

                } else
                    {
                        //Death
                        m_emitter1.update(gameTime);
                        gameOverTime -= gameTime.ElapsedGameTime.TotalSeconds;
                    }
                }
            else
            {
                pauseInput();
            }
        }

        private void checkBulletCollisions()
        {
            
            //Check Collisions for each bullet and each Enemy
            if (bullets.Count > 0 && enemies.Count > 0)
            {
                foreach (Rectangle bullet in bullets)
                {
                    foreach (Enemy enemy in enemies)
                    {
                        if (intersect(enemy.rectangle, bullet))
                        {
                            deleteBullets.Add(bullet);
                            enemy.lives -= 1;

                        } 
                    }
                }
               
            }
            
        }
        private void checkEnemyLives()
        {
            
            foreach(Enemy enemy in enemies)
            {
                if(enemy.lives <= 0)
                {
                    deleteEnemies.Add(enemy);
                    if(enemy.enemyTexture == m_bee)
                    {
                        m_score += 50;
                    } else if (enemy.enemyTexture == m_boss)
                    {
                        m_score += 150;
                    } else if (enemy.enemyTexture == m_butterfly)
                    {
                        m_score += 80;
                    }
                }
            }
        }
        private void drawScore()
        {
            Vector2 stringSize = m_font.MeasureString("Score " + m_score.ToString());
            m_spriteBatch.DrawString(
               m_font,
                "Score " + (m_score).ToString(),
               new Vector2((m_graphics.PreferredBackBufferWidth - stringSize.X) / 2, 10),
               Color.Red);


            stringSize = m_font.MeasureString("Bullets Fired: " + bulletsFired.ToString());
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
        private void drawLives()
        {
            for (int i = 0; i < m_lives; i++)
            {
                m_spriteBatch.Draw(m_playerTex, new Rectangle(Wall_THICKNESS + 50 * i, m_graphics.PreferredBackBufferHeight - 100, 30, 30), Color.White);
            }

        }

        private void drawEnemies()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                m_spriteBatch.Draw(enemies[i].enemyTexture, enemies[i].rectangle, Color.White);
            }
        }
        private void updateEnemies()
        {

            //Do Something
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

        class Enemy
        {

            public Texture2D enemyTexture;
            public int lives;
            public Rectangle rectangle;

            public Enemy(Texture2D enemyTexture, int lives, Rectangle rectangle)
            {
                this.enemyTexture = enemyTexture;
                this.lives = lives;
                this.rectangle = rectangle;
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
                    Scoring myState = new Scoring((int)m_score);
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