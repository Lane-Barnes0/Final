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
using static System.Collections.Specialized.BitVector32;

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
        private const int CHARACTER_SIZE = 30;
        private int currentWave;
        private int m_lives;
        private int bulletsFired;
        private int bulletWidth;
        private float bulletSpeed = 700.0f / 1000.0f;
        private const float SPRITE_MOVE_PIXELS_PER_MS = 600.0f / 1000.0f;
        private float enemySpeed;

        private double gameOverTime;
        private double m_score;
        private double moreEnemies;
        private double nextWave;

        private SpriteFont m_font;
        private SpriteFont m_fontMenu;
        private SpriteFont m_fontMenuSelect;

        private ContentManager m_contentManager;
        private ParticleEmitter m_emitter1;
        
        private List<Rectangle> bullets;
        private List<Rectangle> deleteBullets;
        private List<Enemy> enemies;
        private List<Enemy> deleteEnemies;

        private int[,] topBeesPath;

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

            int center = m_graphics.PreferredBackBufferWidth / 2;
            topBeesPath = new int[,]
            {
            
            /*
             * Start Top Center Off right slightly
             * Travel Down and Left
             * Sop A little below half way and loop back up 
             * stop around 1/3 height above middle
             */
            {1000, 0},
            {500, 500},
            {800, 800 },
            {900, 500},
          
            };
            

            m_player = new Rectangle(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight - 100, CHARACTER_SIZE, CHARACTER_SIZE);
            m_lives = 3;

            gameOverTime = 5;
            m_selection = 0;
            m_score = 0;
            moreEnemies = 1;
            currentWave = 1;
            nextWave = 6;
            enemySpeed = SPRITE_MOVE_PIXELS_PER_MS / 2;

            //Bools
            newGame = true;
            m_waitforkey = false;
            m_pause = false;
            m_quit = false;
            m_gameOver = false;

            rightWall = new Rectangle(m_graphics.PreferredBackBufferWidth - 500, 0, Wall_THICKNESS, m_graphics.PreferredBackBufferHeight);
            leftWall = new Rectangle(470, 0, Wall_THICKNESS, m_graphics.PreferredBackBufferHeight);

        }
        public double computeDistance(int pt1x, int pt1y, int pt2x, int pt2y)
        {
            
            double dx2 = Math.Pow(pt2x - pt1x, 2);
            double dy2 = Math.Pow(pt2y - pt1y, 2);

            return Math.Sqrt(dx2 + dy2);
        }

        public double computeRotation(int pt1x, int pt1y, int pt2x, int pt2y)
        {
            double dx = pt2x - pt1x;
            double dy = pt2y - pt1y;

            if (dx == 0)
            {
                return 0;   // It actually isn't computable, but doing this because we need something
            }

            double angle = Math.Atan(dy / dx);
            if (pt2x < pt1x)
            {
                angle += Math.PI;
            }
            if (pt2y < pt1y)
            {
                angle -= Math.PI / 2.0;
            }

            return angle;
        }

        private void updateEnemyPath(Enemy enemy, GameTime elapsedTime, int[,] path)
        {
            if (enemy.pathIndex < path.GetLength(0) - 1)
            {
                // Compute distance traveled
                double distTraveled =  enemySpeed * elapsedTime.ElapsedGameTime.TotalMilliseconds;

                // Compute remaining distance on the current line segment
                double distRemaining = computeDistance(enemy.rectangle.X, enemy.rectangle.Y, path[enemy.pathIndex + 1, 0], path[enemy.pathIndex + 1, 1]);

                if (distTraveled > distRemaining)
                {
                    distTraveled -= distRemaining;
                    // Move the ship to the end of the current line segment
                    enemy.rectangle.X = path[enemy.pathIndex + 1, 0];
                    enemy.rectangle.Y = path[enemy.pathIndex + 1, 1];

                    enemy.pathIndex += 1;
                }

                if (enemy.pathIndex < path.GetLength(0) - 1)
                {
                    // Now, handle the distance along the current line segment
                    // Start by computing the direction vector of the line
                    double dirX = path[enemy.pathIndex + 1, 0] - enemy.rectangle.X;
                    double dirY = path[enemy.pathIndex + 1, 1] - enemy.rectangle.Y;
                    // Normalize the vector
                    double dirMag = Math.Sqrt(dirX * dirX + dirY * dirY);
                    dirX /= dirMag;
                    dirY /= dirMag;
                    // See how far along that vector the ship moved
                    double moveX = distTraveled * dirX;
                    double moveY = distTraveled * dirY;
                    // Update the ship position with the movement distance
                    enemy.rectangle.X += (int)moveX;
                    enemy.rectangle.Y += (int)moveY;

                    enemy.rotation = computeRotation(enemy.rectangle.X, enemy.rectangle.Y, path[enemy.pathIndex + 1, 0], path[enemy.pathIndex + 1, 1]);
                    

                }
            }
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
                    
                    generateEnemies(currentWave, gameTime);
                        

                    //Update Enemies
                    updateEnemies(gameTime);


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

                        
                        
                        if (circleIntersect(enemy.rectangle, bullet))
                        {
                            deleteBullets.Add(bullet);
                            enemy.lives -= 1;

                        } 
                    }
                }
               
            }
            
        }

        private bool circleIntersect(Rectangle one, Rectangle two)
        {

            double radiusOne = one.Right - one.Center.X;
            double radiustwo = two.Right - two.Center.X;

            double distance = computeDistance(one.Center.X, one.Center.Y, two.Center.X, two.Center.Y);
            return distance <= radiusOne + radiustwo;
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
                if (enemies[i].enemyTexture == m_boss && enemies[i].lives == 1) {
                    
                    m_spriteBatch.Draw(enemies[i].enemyTexture, enemies[i].rectangle, null, Color.Blue, (float)(enemies[i].rotation), new Vector2(enemies[i].enemyTexture.Width /2, enemies[i].enemyTexture.Height / 2),
                    SpriteEffects.None,0);
                    
                } else
                {
                    m_spriteBatch.Draw(enemies[i].enemyTexture, enemies[i].rectangle, null, Color.White, (float)(enemies[i].rotation), new Vector2(enemies[i].enemyTexture.Width / 2, enemies[i].enemyTexture.Height / 2),
                    SpriteEffects.None, 0);
                }
                
            }
        }
        private void updateEnemies(GameTime gameTime)
        {
            foreach (Enemy enemy in enemies)
            {
                
                if(enemy.pathIndex < topBeesPath.GetLength(0) - 1)
                {
                    updateEnemyPath(enemy, gameTime, topBeesPath);
                } else
                {
                    enemy.rotation = 0;
                    double distTraveled = enemy.directionX * enemySpeed * gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (intersect(leftWall, enemy.rectangle) || intersect(rightWall, enemy.rectangle))
                    {

                        enemy.directionX *= -1;
                        enemy.rectangle = new Rectangle((int)(enemy.rectangle.X - distTraveled * 2), (enemy.rectangle.Y), CHARACTER_SIZE, CHARACTER_SIZE);
                    }
                    else
                    {
                        enemy.rectangle = new Rectangle((int)(enemy.rectangle.X + distTraveled), (enemy.rectangle.Y), CHARACTER_SIZE, CHARACTER_SIZE);
                    }

                }
            }
        }


        private void generateEnemies(int wave, GameTime gameTime)
        {

            moreEnemies -= gameTime.ElapsedGameTime.TotalSeconds;
            nextWave -= gameTime.ElapsedGameTime.TotalSeconds;
            if (moreEnemies < 0)
            {
                moreEnemies = 0.45;
                
                //First Wave, Second Wave, Challenge Wave
                if (wave == 1)
                {
                    if (enemies.Count < 10)
                    {
                        firstEnemyWave();
                    }
                    
                }
                else if (wave == 2)
                {
                    if (enemies.Count < 10)
                    {
                        secondEnemyWave();
                    }
                        
                }
                else if (wave == 3)
                {
                    if (enemies.Count < 10)
                    {
                        challengeWave();
                    }
                       
                }  
            }

            if(nextWave < 0 )
            {
                currentWave += 1;
                if (currentWave > 3)
                {
                    currentWave = 1;
                }
                nextWave = 10;
            }
            
        }
        private void firstEnemyWave()
        {
            
         enemies.Add(new Enemy(m_bee, 1, new Rectangle(topBeesPath[0, 0], topBeesPath[0, 1], CHARACTER_SIZE, CHARACTER_SIZE)));
     
            
        }
        private void secondEnemyWave()
        {
            enemies.Add(new Enemy(m_butterfly, 1, new Rectangle(topBeesPath[0, 0], topBeesPath[0, 1], CHARACTER_SIZE, CHARACTER_SIZE)));
        }
        private void challengeWave()
        {
            enemies.Add(new Enemy(m_boss, 2, new Rectangle(topBeesPath[0, 0], topBeesPath[0, 1], CHARACTER_SIZE, CHARACTER_SIZE)));
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
            public int directionX;
            public int directionY;
            public int pathIndex = 0;
            public double rotation = 0;
            public Enemy(Texture2D enemyTexture, int lives, Rectangle rectangle)
            {
                this.enemyTexture = enemyTexture;
                this.lives = lives;
                this.rectangle = rectangle;
                directionX = 1;
                directionY = 1;
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