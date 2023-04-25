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
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata.Ecma335;

namespace Galaga
{
    public class GamePlayView : GameStateView
    {
        
        private bool saving = false;
        private bool m_wait;
        private bool m_pause;
        public bool newGame;
        bool m_quit;
        private bool m_waitforkey;
        private int m_selection;
        private bool playerDeath;

        private (int, int) deathLocation;
        private const int Wall_THICKNESS = 30;
        private const int CHARACTER_SIZE = 30;
        private int currentWave;
        private int m_lives;
        private int bulletsFired;
        private int bulletWidth;
        private float bulletSpeed = 700.0f / 1000.0f;
        private const float SPRITE_MOVE_PIXELS_PER_MS = 600.0f / 1000.0f;
        public const int WAVE_SPAWN_RATE = 3;
        private double playerDeathTimer;
        private int enemiesHit;
        private int nextShip;
        private double m_score;
        private double moreEnemies;
        private double clearScores;
        private double switchAnimation;
        private double hitRatio;
        private double gameStartTimer;
        private double gameOverTimer;
        private double newGameTimer;
        private SpriteFont m_font;
        private SpriteFont m_fontMenu;
        private SpriteFont m_fontMenuSelect;
        private SpriteFont m_enemyScoreFont;
        private bool m_canPlayMusic;
        private ContentManager m_contentManager;
        private ParticleEmitter m_emitter1;
        
        private List<Rectangle> bullets;
        private List<Rectangle> deleteBullets;
        private List<Rectangle> enemyBullets;
        private List<Rectangle> deleteEnemyBullets;
        private List<Enemy> enemies;
        private List<Enemy> deleteEnemies;
        private List<(int, int, int)> destroyedEnemyScores;
        private int enemiesCreated;

        private int[,] topRightPath;
        private int[,] topLeftPath;
        private int[,] topRightPathUpper;
        private int[,] topLeftPathUpper;
        private int[,] bottomLeftPath;
        private int[,] bottomLeftPathUpper;
        private int[,] bottomRightPath;
        private int[,] bottomRightPathUpper;
        private int[,] challengeTopLeft;
        private int[,] challengeTopRight;
        private int[,] challengeBottomRight;
        private int[,] challengeBottomLeft;
        Rectangle m_player;
        Rectangle leftWall;
        Rectangle rightWall;

        private KeyboardInput m_inputKeyboard;
        private Texture2D m_bulletTex;
        private Texture2D m_squareTexture;
        private Texture2D m_background;
        private Texture2D m_playerTex;
        private List<Texture2D> m_bee;
        private List<Texture2D> m_butterfly;
        private List<Texture2D> m_boss;
        private Texture2D m_explosion;
        private Song m_backgroundMusic;
        private SoundEffect m_shot;
        private SoundEffect enemyHitSound;
        private SoundEffect playerDeathSound;


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
            enemyBullets = new List<Rectangle>();
            deleteEnemyBullets = new List<Rectangle>();
            bulletWidth = 10;
            bulletsFired = 0;

            enemies = new List<Enemy>();
            deleteEnemies= new List<Enemy>();

            int center = m_graphics.PreferredBackBufferWidth / 2;
            topRightPath = new int[,]
            {
            {1000, 0},
            {750, 250},
            {500, 500},
            {650, 650},
            {800, 800 },
            {850, 800},
            {850, 500},
            {900, 500},
            };

            topLeftPath = new int[,]
            {
            {900, 0},
            {1350, 500},
            {1050, 800 },
            {900, 450},
            };

            topRightPathUpper = new int[,]
            {
            {1000, -50},
            {750, 200},
            {500, 450},
            {650, 600},
            {800, 750 },
            {850, 750},
            {850, 450},
            {900, 400},
            };

            topLeftPathUpper = new int[,]
            {
            {900, -50},
            {1350, 450},
            {1050, 750 },
            {900, 500},
            };

            bottomLeftPath = new int[,]
            {
            {500, 900},
            {700, 800},
            {800, 500 },
            {900, 350},
            };
            bottomLeftPathUpper = new int[,]
            {
                {500, 850},
                {700, 750},
                {800, 450 },
                {900, 350},
            };
            bottomRightPath = new int[,]
            {
            {1500, 900},
            {1350, 800},
            {1050, 500 },
            {900, 600},
            };

            bottomRightPathUpper = new int[,]
            {
            {1500, 850},
            {1350, 750},
            {1050, 450 },
            {900, 300},
            };


            challengeTopRight = new int[,]
            {
            {1050, 0},
            {1050, 500},
            {900, 600 },
            {700, 800},
            {600, 700},
            {2400, 300},
            };

            challengeTopLeft = new int[,]
            {
            {900, 0},
            {900, 500},
            {1050, 600 },
            {1200, 800},
            {1300, 700},
            {-1110, 300},
            };

            challengeBottomLeft = new int[,]
            {
            {400, 900},
            {600, 900},
            {1000, 500},

            {900, 400},
            {800, 500},
            {800, 600},
            {2000, 300 },
            
            };

            challengeBottomRight = new int[,]
            {
            {1500, 900},
            {1200, 900},
            {800, 500 },

            {900, 400},
            {1000, 500},
            {1000, 600},
            {-1000, 300 }
            };

            m_player = new Rectangle(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight - 100, CHARACTER_SIZE, CHARACTER_SIZE);
            destroyedEnemyScores = new List<(int, int, int)>();
            m_lives = 3;

            m_selection = 0;
            m_score = 0;
            moreEnemies = 1;
            currentWave = 1;
            enemiesCreated = 0;
            clearScores = 0.25;
            switchAnimation = 0.75;
            deathLocation = (0, 0);
            playerDeathTimer = 0;
            enemiesHit = 0;
            hitRatio= 0;
            nextShip = 2000;
            gameStartTimer = 5;
            gameOverTimer = 5;
            newGameTimer = 0;
            //Bools
            newGame = true;
            m_waitforkey = false;
            m_pause = false;
            m_quit = false;
            m_canPlayMusic = true;
            playerDeath = false;
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

            
            double dx = pt1x - pt2x;
            double dy = pt1y - pt2y;

            if (dx == 0)
            {
                return 0;   // It actually isn't computable, but doing this because we need something
            }

            double angle = Math.Atan(dy / dx);
            
            if (pt1x < pt2x)
            {
                angle -= Math.PI;
            }

            if (pt2y < pt1y)
            {
                angle += Math.PI / 2.0;
            }
            if (pt2y > pt1y)
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
                double distTraveled =  enemy.speed * elapsedTime.ElapsedGameTime.TotalMilliseconds;

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

                    enemy.rotation = computeRotation(enemy.rectangle.X, enemy.rectangle.Y, path[enemy.pathIndex, 0], path[enemy.pathIndex, 1]);
                    

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
            m_explosion = contentManager.Load<Texture2D>("Images/explosion");
            m_bee = new List<Texture2D>
            {
                contentManager.Load<Texture2D>("Images/bee"),
                contentManager.Load<Texture2D>("Images/bee2")
            };
            m_boss = new List<Texture2D>
            {
                contentManager.Load<Texture2D>("Images/boss"),
                contentManager.Load<Texture2D>("Images/boss2")
            };
            m_butterfly = new List<Texture2D>
            {
                contentManager.Load<Texture2D>("Images/butterfly"),
                contentManager.Load<Texture2D>("Images/butterfly2")
            };
            m_enemyScoreFont = contentManager.Load<SpriteFont>("Fonts/enemyScore");
            m_backgroundMusic = contentManager.Load<Song>("Audio/backgroundMusic");
            m_shot = contentManager.Load<SoundEffect>("Audio/shot");
            enemyHitSound = contentManager.Load<SoundEffect>("Audio/hit");
            playerDeathSound = contentManager.Load<SoundEffect>("Audio/playerDeath");
        }

        public override GameStateEnum processInput(GameTime gameTime)
        {

            if (m_quit == true && gameOverTimer <= 0)
            {
                m_quit = false;
                newGame = true;
                m_pause = false;
                newGameTimer = 0.2;
                saveScore();
                MediaPlayer.Stop();
                return GameStateEnum.MainMenu;
            } else
            {
                return GameStateEnum.NewGame;
            }

            
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();
            m_spriteBatch.Draw(m_background, new Rectangle(0, 0, m_graphics.PreferredBackBufferWidth, m_graphics.PreferredBackBufferHeight), Color.White);
            if (!m_quit && newGameTimer <= 0)
            {
                drawLives();
                if (gameStartTimer <= 0)
                {
                    
                    if (m_pause)
                    {
                        renderMenu();
                    }
                    else
                    {
                        if (playerDeath)
                        {
                            m_spriteBatch.Draw(m_explosion, new Rectangle(deathLocation.Item1, deathLocation.Item2, CHARACTER_SIZE * 2, CHARACTER_SIZE * 2), Color.White);
                        }
                        drawScore();
                        m_spriteBatch.Draw(m_playerTex, m_player, Color.White);
                        drawBullets();

                        drawEnemies();
                        drawDestroyedEnemyScore();


                        if(moreEnemies > WAVE_SPAWN_RATE)
                        {
                            if(currentWave == 3)
                            {
                                Vector2 stringSize = m_font.MeasureString("Challenge Wave Incoming");
                                m_spriteBatch.DrawString(
                                   m_font,
                                    "Challenge Wave Incoming",
                                   new Vector2((m_graphics.PreferredBackBufferWidth - stringSize.X) / 2, 100),
                                   Color.Teal);
                            } else
                            {
                                Vector2 stringSize = m_font.MeasureString("Wave " + currentWave.ToString() + " Incoming");
                                m_spriteBatch.DrawString(
                                   m_font,
                                    "Wave " + currentWave.ToString() + " Incoming",
                                   new Vector2((m_graphics.PreferredBackBufferWidth - stringSize.X) / 2, 100),
                                   Color.Teal);
                            }
                           

                        }
                        
                    }


                } else
                {
                    Vector2 stringSize = m_font.MeasureString("Player One ");
                    m_spriteBatch.DrawString(
                       m_font,
                        "Player One ",
                       new Vector2((m_graphics.PreferredBackBufferWidth - stringSize.X) / 2, m_graphics.PreferredBackBufferHeight / 2 - 100),
                       Color.Teal);

                   stringSize = m_font.MeasureString("New Ship After Every " + nextShip.ToString() + " Points");
                    m_spriteBatch.DrawString(
                       m_font,
                        "New Ship After Every " + nextShip.ToString() + " Points",
                       new Vector2((m_graphics.PreferredBackBufferWidth - stringSize.X) / 2, m_graphics.PreferredBackBufferHeight / 2 - 25),
                       Color.Teal);
                }
                
            } else
            {
                if(gameOverTimer > 0 )
                {
                    drawScore();
                    Vector2 stringSize = m_font.MeasureString("Bullets Fired: " + bulletsFired.ToString());
                    m_spriteBatch.DrawString(
                       m_enemyScoreFont,
                        "Bullets Fired: " + (bulletsFired).ToString(),
                       new Vector2((m_graphics.PreferredBackBufferWidth - stringSize.X) / 2, m_graphics.PreferredBackBufferHeight / 2 - 150),
                       Color.Yellow);

                    stringSize = m_font.MeasureString("Enemies Hit: " + enemiesHit.ToString());
                    m_spriteBatch.DrawString(
                       m_enemyScoreFont,
                        "Enemies Hit: " + (enemiesHit).ToString(),
                       new Vector2((m_graphics.PreferredBackBufferWidth - stringSize.X) / 2, m_graphics.PreferredBackBufferHeight / 2 - 200),
                       Color.Yellow);

                    if (enemiesHit != 0)
                    {
                        stringSize = m_font.MeasureString("Ratio: " + hitRatio.ToString());
                        m_spriteBatch.DrawString(
                           m_enemyScoreFont,
                            "Ratio: " + hitRatio.ToString() + "%",
                           new Vector2((m_graphics.PreferredBackBufferWidth - stringSize.X) / 2, m_graphics.PreferredBackBufferHeight / 2 - 100),
                           Color.Yellow);
                    }
                    else
                    {
                        stringSize = m_font.MeasureString("Ratio: 0%");
                        m_spriteBatch.DrawString(
                           m_enemyScoreFont,
                            "Ratio: 0%",
                           new Vector2((m_graphics.PreferredBackBufferWidth - stringSize.X) / 2, m_graphics.PreferredBackBufferHeight / 2 - 100),
                           Color.Yellow);
                    }

                }
                
            }
            m_spriteBatch.End();
        }

        private void switchEnemyAnimation()
        {
            if (switchAnimation < 0)
            {
                if (enemies.Count > 0)
                {
                    switchAnimation = 0.75;
                    foreach (Enemy enemy in enemies)
                    {
                        //Bee
                        if (enemy.enemyTexture == m_bee[0])
                        {
                            enemy.enemyTexture = m_bee[1];
                            continue;
                        }
                        else if (enemy.enemyTexture == m_bee[1])
                        {
                            enemy.enemyTexture = m_bee[0];
                            continue;
                        }

                        //Butterfly
                        if (enemy.enemyTexture == m_butterfly[0])
                        {
                            enemy.enemyTexture = m_butterfly[1];
                            continue;
                        }
                        else if (enemy.enemyTexture == m_butterfly[1])
                        {
                            enemy.enemyTexture = m_butterfly[0];
                            continue;
                        }


                        //Boss
                        if (enemy.enemyTexture == m_boss[0])
                        {
                            enemy.enemyTexture = m_boss[1];
                            continue;
                        }
                        else if (enemy.enemyTexture == m_boss[1])
                        {
                            enemy.enemyTexture = m_boss[0];
                            continue;
                        }
                    }

                }

            }
        }
        public override void update(GameTime gameTime)
        {
            
            
            if (newGame && newGameTimer <= 0.05)
            {
                initializeNewGameState();
                newGame = false;
            } else
            {
                newGameTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            }


            if (m_canPlayMusic)
            {
                MediaPlayer.Play(m_backgroundMusic);
                m_canPlayMusic = false;
            }

            if (gameStartTimer <= 0)
            {


                if (playerDeath)
                {
                    m_player.X = m_graphics.PreferredBackBufferWidth / 2;
                }
                if (playerDeathTimer < 0)
                {
                    m_player.Y = m_graphics.PreferredBackBufferHeight - 100;
                    playerDeath = false;
                }
                else
                {
                    playerDeathTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                }
                
                if (!m_pause)
                {
                    if (!m_quit)
                    {

                        switchAnimation -= gameTime.ElapsedGameTime.TotalSeconds;

                        switchEnemyAnimation();

                        m_inputKeyboard.Update(gameTime);

                        generateEnemies(currentWave, gameTime);


                        //Update Enemies
                        updateEnemies(gameTime);
                        updateEnemyBullets(gameTime);

                        //Update Each bullet Position
                        for (int i = 0; i < bullets.Count; i++)
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
                        checkCollisions();
                        checkEnemyLives();

                        //Clean Up Dead Enemies and Off screen Bullets

                        if (deleteBullets.Count > 0)
                        {
                            foreach (Rectangle bullet in deleteBullets)
                            {
                                bullets.Remove(bullet);
                            }

                            deleteBullets.Clear();
                        }

                        if (deleteEnemyBullets.Count > 0)
                        {
                            foreach (Rectangle bullet in deleteEnemyBullets)
                            {
                                enemyBullets.Remove(bullet);
                            }

                            deleteEnemyBullets.Clear();
                        }

                        if (deleteEnemies.Count > 0)
                        {
                            foreach (Enemy enemy in deleteEnemies)
                            {
                                enemies.Remove(enemy);
                            }

                            deleteEnemies.Clear();
                        }

                        //Clean Up Scores
                        if (destroyedEnemyScores.Count > 0)
                        {
                            clearScores -= gameTime.ElapsedGameTime.TotalSeconds;
                            if (clearScores < 0) { clearScores = 0.25; destroyedEnemyScores.Clear(); }
                        }


                    }
                    else
                    {
                        //Death
                        gameOverTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                    }
                }
                else
                {
                    pauseInput();
                }

            } else
            {
                gameStartTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            }

            
        }

        private void checkCollisions()
        {
            
            //Check Collisions for each bullet
            if (bullets.Count > 0)
            {
                foreach (Rectangle bullet in bullets)
                {
                    if (enemies.Count > 0)
                    {
                        foreach (Enemy enemy in enemies)
                        {
                            if (circleIntersect(enemy.rectangle, bullet))
                            {
                                enemyHitSound.Play();
                                deleteBullets.Add(bullet);
                                enemiesHit += 1;
                                enemy.lives -= 1;
                                
                            }
                        }

                    }
                   
                } 
            }

            //Enemy Bullets and Player 

            if(enemyBullets.Count > 0)
            {
                foreach(Rectangle bullet in enemyBullets)
                {
                    if(circleIntersect(bullet, m_player))
                    {
                        if (circleIntersect(m_player, bullet))
                        {
                            m_lives -= 1;

                            
                            //Move Player Off Screen, Play Explosion, Wait 2 Seconds, Move Player Back onto the Screen
                            deathLocation = (m_player.X, m_player.Y);
                            m_player.Y = m_graphics.PreferredBackBufferHeight + 500;
                            m_player.X = m_graphics.PreferredBackBufferWidth / 2;
                            playerDeath = true;
                            playerDeathTimer = 2;
                            playerDeathSound.Play();
                            deleteEnemyBullets.Add(bullet);
                            if (m_lives == 0)
                            {
                                m_quit = true;
                            }
                        }
                    }
                }
            }

            //Check Enemy and Player collision
            foreach (Enemy enemy in enemies)
            {
                
                if (circleIntersect(enemy.rectangle, m_player))
                {
                    m_lives -= 1;
                    //Move Player Off Screen, Play Explosion, Wait 2 Seconds, Move Player Back onto the Screen
                    deathLocation = (m_player.X, m_player.Y);
                    m_player.Y = m_graphics.PreferredBackBufferHeight + 500;
                    m_player.X = m_graphics.PreferredBackBufferWidth / 2;
                    playerDeath = true;
                    playerDeathTimer = 2;
                    playerDeathSound.Play();

                    enemy.lives = 0;
                    if (m_lives == 0)
                    {
                        m_quit = true;
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
                    
                    if(enemy.enemyTexture == m_bee[0] || enemy.enemyTexture == m_bee[1])
                    {
                        m_score += 50;
                        destroyedEnemyScores.Add((enemy.rectangle.Right, enemy.rectangle.Top, 50));
                        
                    } else if (enemy.enemyTexture == m_boss[0] || enemy.enemyTexture == m_boss[1])
                    {
                        m_score += 150;
                        destroyedEnemyScores.Add((enemy.rectangle.Right, enemy.rectangle.Top, 150));
                    } else if (enemy.enemyTexture == m_butterfly[0] || enemy.enemyTexture == m_butterfly[1])
                    {
                        m_score += 80;
                        destroyedEnemyScores.Add((enemy.rectangle.Right, enemy.rectangle.Top, 80));
                    }
                }
            }

            if (m_score > nextShip)
            {
                m_lives += 1;
                nextShip += 2000;
            }
        }

        private void updateEnemyBullets(GameTime gameTime)
        {

            for (int i = 0; i < enemyBullets.Count; i++)
            {
                int moveDistance = (int)(gameTime.ElapsedGameTime.TotalMilliseconds * bulletSpeed / 2);
                enemyBullets[i] = new Rectangle(enemyBullets[i].X, enemyBullets[i].Y + moveDistance, bulletWidth, bulletWidth);

             
               if (enemyBullets[i].Y > m_graphics.PreferredBackBufferHeight)
               {
                        deleteEnemyBullets.Add(enemyBullets[i]);
               }

            }

        }
        
        
        private void drawDestroyedEnemyScore()
        {
            for (int i = 0; i < destroyedEnemyScores.Count; i++)
            {
                Vector2 stringSize = m_font.MeasureString(destroyedEnemyScores[i].Item3.ToString());
                m_spriteBatch.DrawString(
                   m_enemyScoreFont,
                   destroyedEnemyScores[i].Item3.ToString(),
                   new Vector2((destroyedEnemyScores[i].Item1 - stringSize.X), destroyedEnemyScores[i].Item2 - CHARACTER_SIZE),
                   Color.Red);

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
            if (!playerDeath)
            {
                //Fire Bullets 
                bullets.Add(new Rectangle(m_player.Center.X - bulletWidth / 2, m_player.Y - 5, bulletWidth, bulletWidth));
                bulletsFired += 1;
                m_shot.Play();
                hitRatio = ((double)enemiesHit / (double)bulletsFired) * 100;
                hitRatio = Math.Round(hitRatio, 2);
            }
            
            
        }

        private void drawBullets()
        {
            foreach(Rectangle bullet in bullets)
            {
                m_spriteBatch.Draw(m_bulletTex, bullet, Color.White);
            }

            foreach (Rectangle bullet in enemyBullets)
            {
                m_spriteBatch.Draw(m_bulletTex,bullet, null, Color.White, (float)(Math.PI), new Vector2(m_bulletTex.Width / 2, m_bulletTex.Height / 2),
                    SpriteEffects.None, 0);
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
                if ( (enemies[i].enemyTexture == m_boss[0] || enemies[i].enemyTexture == m_boss[1]) && enemies[i].lives == 1) {
                    
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
                //If off screen delete

                if(enemy.rectangle.X < 0 || 
                   enemy.rectangle.X > m_graphics.PreferredBackBufferWidth ||
                   enemy.rectangle.Y < 0 ||
                   enemy.rectangle.Y > m_graphics.PreferredBackBufferHeight) { deleteEnemies.Add(enemy);  continue; }


                enemy.timeAlive += gameTime.ElapsedGameTime.TotalSeconds;
                if(enemy.pathIndex < enemy.path.GetLength(0) - 1)
                {
                    updateEnemyPath(enemy, gameTime, enemy.path);
                } else if (enemy.timeAlive > 10 )
                {
                    //Throw Bullet
                    if (!enemy.shotBullet)
                    {
                        enemy.shotBullet = true;
                        enemyBullets.Add(new Rectangle(enemy.rectangle.Center.X, enemy.rectangle.Center.Y, bulletWidth, bulletWidth));
                    } 
                    
                    
                    enemy.realign -= gameTime.ElapsedGameTime.TotalSeconds;
                    enemy.speed = SPRITE_MOVE_PIXELS_PER_MS / 5;
                    //Change X direction Towards Player every 1.5 seconds
                    if (enemy.realign <= 0)
                    {
                        enemy.realign = 1.5;
                        //Player to the Right
                        if (m_player.X > enemy.rectangle.X)
                        {
                            enemy.directionX = -1;
                            enemy.rotation = 2.35619;
                        }
                        else
                        //Player to the Left
                        {
                            enemy.directionX = 1;
                            enemy.rotation = -2.35619;
                        }
                    }
                    
                    double distTraveledX = enemy.directionX * enemy.speed * gameTime.ElapsedGameTime.TotalMilliseconds;
                    double distTraveledY = enemy.speed * gameTime.ElapsedGameTime.TotalMilliseconds;
                    enemy.rectangle = new Rectangle((int)(enemy.rectangle.X - distTraveledX * 2), (int)(enemy.rectangle.Y + distTraveledY), CHARACTER_SIZE, CHARACTER_SIZE);


                } else
                {
                    enemy.speed = SPRITE_MOVE_PIXELS_PER_MS / 4;
                    enemy.rotation = 0;
                    double distTraveled = enemy.directionX * enemy.speed * gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (intersect(leftWall, enemy.rectangle) || intersect(rightWall, enemy.rectangle))
                    {

                        enemy.directionX *= -1;
                        enemy.rectangle = new Rectangle((int)(enemy.rectangle.X - distTraveled * 2), (enemy.rectangle.Y), CHARACTER_SIZE, CHARACTER_SIZE);
                    }
                    else
                    {
                        enemy.rectangle = new Rectangle((int)(enemy.rectangle.X + distTraveled), (enemy.rectangle.Y ), CHARACTER_SIZE, CHARACTER_SIZE);
                    }

                }
            }
        }


        private void generateEnemies(int wave, GameTime gameTime)
        {

            moreEnemies -= gameTime.ElapsedGameTime.TotalSeconds;
            if (moreEnemies < 0)
            {
                moreEnemies = 0.3;
                //First Wave, Second Wave, Challenge Wave
                if (wave == 1)
                { 
                firstEnemyWave();
                }
                else if (wave == 2)
                {                 
                secondEnemyWave();       
                }
                else if (wave == 3)
                {
                 challengeWave();     
                }  
            }

            if(enemiesCreated >= 41 )
            {
                enemiesCreated = 0;
                currentWave += 1;
                if (currentWave > 3)
                {
                    currentWave = 1;
                }
                
            }
            
        }
        private void firstEnemyWave()
        {
            
            if(enemiesCreated < 8)
            {
                enemies.Add(new Enemy(m_bee[0], 1, new Rectangle(topRightPath[0, 0], topRightPath[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), topRightPath, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
                enemies.Add(new Enemy(m_butterfly[0], 1, new Rectangle(topLeftPath[0, 0], topLeftPath[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), topLeftPath, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
                enemiesCreated += 2;

                if(enemiesCreated == 8)
                {
                    moreEnemies = WAVE_SPAWN_RATE;
                }
            } else if(enemiesCreated < 17)
            {
                if (enemiesCreated % 2 == 0)
                {
                    enemiesCreated++;
                    enemies.Add(new Enemy(m_boss[0], 2, new Rectangle(bottomLeftPath[0, 0], bottomLeftPath[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), bottomLeftPath, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
                    
                }
                else
                {
                    enemiesCreated++;
                    enemies.Add(new Enemy(m_butterfly[0], 1, new Rectangle(bottomLeftPath[0, 0], bottomLeftPath[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), bottomLeftPath, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
                }

                if (enemiesCreated == 17)
                {
                    moreEnemies = WAVE_SPAWN_RATE;
                }

            } else if (enemiesCreated < 25)
            {
                enemiesCreated++;
                enemies.Add(new Enemy(m_butterfly[0], 1, new Rectangle(bottomRightPath[0, 0], bottomRightPath[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), bottomRightPath, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
                if (enemiesCreated == 25)
                {
                    moreEnemies = WAVE_SPAWN_RATE;
                }
            }
            else if (enemiesCreated < 33)
            {
                enemiesCreated++;
                enemies.Add(new Enemy(m_bee[0], 1, new Rectangle(topRightPath[0, 0], topRightPath[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), topRightPath, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
                if (enemiesCreated == 33)
                {
                    moreEnemies = WAVE_SPAWN_RATE;
                }
            }
            else if (enemiesCreated < 41)
            {
                enemiesCreated++;
                enemies.Add(new Enemy(m_bee[0], 1, new Rectangle(topLeftPath[0, 0], topLeftPath[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), topLeftPath, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
                if (enemiesCreated == 41)
                {
                    //Give more time between Waves
                    moreEnemies = WAVE_SPAWN_RATE * 2;

                }
            }
            
        }
        private void secondEnemyWave()
        {
            if (enemiesCreated < 8)
            {
                enemies.Add(new Enemy(m_butterfly[0], 1, new Rectangle(topRightPath[0, 0], topRightPath[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), topRightPath, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
                enemies.Add(new Enemy(m_bee[0], 1, new Rectangle(topLeftPath[0, 0], topLeftPath[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), topLeftPath, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
                enemiesCreated += 2;

                if (enemiesCreated == 8)
                {
                    moreEnemies = WAVE_SPAWN_RATE;
                }
            }
            else if (enemiesCreated < 16)
            {
                  enemiesCreated+= 2;
                    enemies.Add(new Enemy(m_butterfly[0], 1, new Rectangle(bottomLeftPathUpper[0, 0], bottomLeftPathUpper[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), bottomLeftPathUpper, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
                    enemies.Add(new Enemy(m_boss[0], 2, new Rectangle(bottomLeftPath[0, 0], bottomLeftPath[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), bottomLeftPath, SPRITE_MOVE_PIXELS_PER_MS / 1.5));


                if (enemiesCreated == 16)
                {
                    moreEnemies = WAVE_SPAWN_RATE;
                }

            }
            else if (enemiesCreated < 24)
            {
                enemiesCreated+=2;
                enemies.Add(new Enemy(m_butterfly[0], 1, new Rectangle(bottomRightPathUpper[0, 0], bottomRightPathUpper[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), bottomRightPathUpper, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
                enemies.Add(new Enemy(m_butterfly[0], 1, new Rectangle(bottomRightPath[0, 0], bottomRightPath[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), bottomRightPath, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
                if (enemiesCreated == 24)
                {
                    moreEnemies = WAVE_SPAWN_RATE;
                }
            }
            else if (enemiesCreated < 32)
            {
                enemiesCreated+= 2;
                enemies.Add(new Enemy(m_bee[0], 1, new Rectangle(topRightPathUpper[0, 0], topRightPathUpper[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), topRightPathUpper, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
                enemies.Add(new Enemy(m_bee[0], 1, new Rectangle(topRightPath[0, 0], topRightPath[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), topRightPath, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
                if (enemiesCreated == 32)
                {
                    moreEnemies = WAVE_SPAWN_RATE;
                }
            }
            else if (enemiesCreated < 40)
            {
                enemiesCreated+= 2;
                enemies.Add(new Enemy(m_bee[0], 1, new Rectangle(topLeftPathUpper[0, 0], topLeftPathUpper[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), topLeftPathUpper, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
                enemies.Add(new Enemy(m_bee[0], 1, new Rectangle(topLeftPath[0, 0], topLeftPath[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), topLeftPath, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
                if (enemiesCreated == 40)
                {
                    enemiesCreated += 1;
                    moreEnemies = WAVE_SPAWN_RATE * 3;
                }
            }
        }
        private void challengeWave()
        {
            if (enemiesCreated < 8)
            {
                enemies.Add(new Enemy(m_bee[0], 1, new Rectangle(challengeTopLeft[0, 0], challengeTopLeft[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), challengeTopLeft, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
                enemies.Add(new Enemy(m_bee[0], 1, new Rectangle(challengeTopRight[0, 0], challengeTopRight[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), challengeTopRight, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
                enemiesCreated += 2;

                if (enemiesCreated == 8)
                {
                    moreEnemies = WAVE_SPAWN_RATE;
                }
            }
            else if (enemiesCreated < 16)
            {
                
                    if (enemiesCreated % 2 == 0)
                    {
                        enemiesCreated++;
                        enemies.Add(new Enemy(m_boss[0], 2, new Rectangle(challengeBottomLeft[0, 0], challengeBottomLeft[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), challengeBottomLeft, SPRITE_MOVE_PIXELS_PER_MS / 1.5));

                    }
                    else
                    {
                        enemiesCreated++;
                        enemies.Add(new Enemy(m_bee[0], 1, new Rectangle(challengeBottomLeft[0, 0], challengeBottomLeft[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), challengeBottomLeft, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
                    }

                    if (enemiesCreated == 16)
                    {
                        moreEnemies = WAVE_SPAWN_RATE;
                    }    

            }
            else if (enemiesCreated < 24)
            {
                enemiesCreated ++;
                enemies.Add(new Enemy(m_bee[0], 1, new Rectangle(challengeBottomRight[0, 0], challengeBottomRight[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), challengeBottomRight, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
                
                if (enemiesCreated == 24)
                {
                    moreEnemies = WAVE_SPAWN_RATE;
                }
            }
            else if (enemiesCreated < 32)
            {
                enemiesCreated++;
                enemies.Add(new Enemy(m_bee[0], 1, new Rectangle(challengeTopRight[0, 0], challengeTopRight[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), challengeTopRight, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
                if (enemiesCreated == 32)
                {
                    moreEnemies = WAVE_SPAWN_RATE;
                }
            }
            else if (enemiesCreated < 40)
            {
                enemiesCreated ++;
                enemies.Add(new Enemy(m_bee[0], 1, new Rectangle(challengeTopLeft[0, 0], challengeTopLeft[0, 1], CHARACTER_SIZE, CHARACTER_SIZE), challengeTopLeft, SPRITE_MOVE_PIXELS_PER_MS / 1.5));
               
                if (enemiesCreated == 40)
                {
                    enemiesCreated += 1;
                    moreEnemies = WAVE_SPAWN_RATE * 2;
                }
            }
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
                    gameOverTimer = 0;
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
            public int[,] path;
            public double speed;
            public double timeAlive = 0;
            public bool shotBullet = false;
            public double realign = 0;
            public Enemy(Texture2D enemyTexture, int lives, Rectangle rectangle, int[,] path, double speed)
            {
                this.enemyTexture = enemyTexture;
                this.lives = lives;
                this.rectangle = rectangle;
                directionX = 1;
                directionY = 1;
                this.path = path;
                this.speed = speed;

                Random random = new Random();
                if(random.NextDouble() < 0.50)
                {
                    shotBullet = true;
                }
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