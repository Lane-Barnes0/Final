using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Serialization;

namespace Galaga
{
    internal class ControlsView : GameStateView
    {
        private SpriteFont m_font;
        private Texture2D m_background;
        private SpriteFont m_fontMenu;
        private SpriteFont m_fontMenuSelect;
        private bool makingSelection;
        private double startTimer;
        private bool saving = false;
        private bool loading = false;
        private List<Keys> m_keys;
        private KeyboardState m_previousKeyboard;
        private enum keyBindsState
        {
            Space,
            Left,
            Right,
            Exit
        }

        private keyBindsState m_currentSelection = keyBindsState.Space;
        private bool m_waitForKeyRelease = false;

        public override void loadContent(ContentManager contentManager)
        {
            loadSomething();
            m_keys = new List<Keys>()
            {
                Keys.Space,
                Keys.Left,
                Keys.Right
            };
            startTimer = 0.1;
            makingSelection = false;
            m_fontMenu = contentManager.Load<SpriteFont>("Fonts/menu");
            m_fontMenuSelect = contentManager.Load<SpriteFont>("Fonts/menu-select");
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
            m_background = contentManager.Load<Texture2D>("Images/background");
        }

        public override GameStateEnum processInput(GameTime gameTime)
        {
            
            // This is the technique I'm using to ensure one keypress makes one menu navigation move
            if (!m_waitForKeyRelease)
            {
                // Arrow keys to navigate the menu
                if (Keyboard.GetState().IsKeyDown(Keys.Down) && !makingSelection)
                {

                    if (m_currentSelection != keyBindsState.Exit)
                    {
                        m_currentSelection = m_currentSelection + 1;
                    }

                    m_waitForKeyRelease = true;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Up) && !makingSelection)
                {
                    if (m_currentSelection != keyBindsState.Space)
                    {
                        m_currentSelection = m_currentSelection - 1;
                    }

                    m_waitForKeyRelease = true;
                }

                // If enter is pressed, return the appropriate new state
                if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !m_previousKeyboard.IsKeyDown(Keys.Enter) && !makingSelection)
                {
                    makingSelection = true;
                    m_waitForKeyRelease = true;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !m_previousKeyboard.IsKeyDown(Keys.Enter) && m_currentSelection == keyBindsState.Exit)
                {
                    saveInput();
                    startTimer = 0.1;
                    return GameStateEnum.MainMenu;
                }
            }
            else if (Keyboard.GetState().IsKeyUp(Keys.Down) && Keyboard.GetState().IsKeyUp(Keys.Up) && Keyboard.GetState().IsKeyUp(Keys.Enter))
            {
                m_waitForKeyRelease = false;
            }

            
            return GameStateEnum.Controls;
        }

        public override void render(GameTime gameTime)
        {
            
            m_spriteBatch.Begin();
            m_spriteBatch.Draw(m_background, new Rectangle(0, 0, m_graphics.PreferredBackBufferWidth, m_graphics.PreferredBackBufferHeight), Color.White);
            
            if(startTimer <= 0 )
            {
                if (makingSelection)
                {
                    Vector2 stringSize = m_font.MeasureString("The next Key you select will overwrite the current key selected");
                    m_spriteBatch.DrawString(
                       m_font,
                        "The next Key you select will overwrite the current key",
                       new Vector2((m_graphics.PreferredBackBufferWidth - stringSize.X) / 2, 10),
                       Color.Red);
                }

                // I split the first one's parameters on separate lines to help you see them better
                float bottom = drawMenuItem(
                    m_currentSelection == keyBindsState.Space ? m_fontMenuSelect : m_fontMenu,
                    "Fire: " + m_keys[0].ToString(),
                    200,
                    m_currentSelection == keyBindsState.Space ? Color.Yellow : Color.Blue);
                bottom = drawMenuItem(m_currentSelection == keyBindsState.Left ? m_fontMenuSelect : m_fontMenu, "Move Left: " + m_keys[1].ToString(), bottom, m_currentSelection == keyBindsState.Left ? Color.Yellow : Color.Blue);
                bottom = drawMenuItem(m_currentSelection == keyBindsState.Right ? m_fontMenuSelect : m_fontMenu, "Move Right: " + m_keys[2], bottom, m_currentSelection == keyBindsState.Right ? Color.Yellow : Color.Blue);

                drawMenuItem(m_currentSelection == keyBindsState.Exit ? m_fontMenuSelect : m_fontMenu, "Save and Quit", bottom, m_currentSelection == keyBindsState.Exit ? Color.Yellow : Color.Blue);


            }



            m_spriteBatch.End();
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

        public override void update(GameTime gameTime)
        {
            if (m_loadedState != null)
            {
                if (m_loadedState.keyBinds.Count > 0)
                {
                    m_keys = m_loadedState.keyBinds;
                }
                
            } else
            {
                saveInput();
                loadSomething();

            }

            if(startTimer <= 0 )
            {
                if (m_currentSelection == keyBindsState.Space && makingSelection && !m_waitForKeyRelease)
                {
                    if (Keyboard.GetState().GetPressedKeys().Length > 0)
                    {
                        if(m_loadedState != null)
                        {
                            m_loadedState.keyBinds[0] = Keyboard.GetState().GetPressedKeys()[0];
                        }
                       
                        makingSelection = false;
                    }

                }
                else if (m_currentSelection == keyBindsState.Left && makingSelection && !m_waitForKeyRelease)
                {
                    if (Keyboard.GetState().GetPressedKeys().Length > 0)
                    {
                        if (m_loadedState != null)
                        {
                            m_loadedState.keyBinds[1] = Keyboard.GetState().GetPressedKeys()[0];
                        }
                        makingSelection = false;
                    }

                }
                else if (m_currentSelection == keyBindsState.Right && makingSelection && !m_waitForKeyRelease)
                {
                    if (Keyboard.GetState().GetPressedKeys().Length > 0)
                    {
                        if (m_loadedState != null)
                        {
                            m_loadedState.keyBinds[2] = Keyboard.GetState().GetPressedKeys()[0];
                        }
                        makingSelection = false;
                    }

                }

            } else
            {
                startTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                makingSelection = false;
            }
            
        }


        private void saveInput()
        {
            lock (this)
            {
                if (!this.saving)
                {
                    this.saving = true;
                    //
                    // Create something to save
                    UserInput userInput = new UserInput(m_keys);
                    finalizeSaveAsync(userInput);
                }
            }
        }

        private async void finalizeSaveAsync(UserInput input)
        {
            await Task.Run(() =>
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    try
                    {
                        using (IsolatedStorageFileStream fs = storage.OpenFile("UserInputs.xml", FileMode.Create))
                        {
                            if (fs != null)
                            {
                                XmlSerializer mySerializer = new XmlSerializer(typeof(UserInput));
                                mySerializer.Serialize(fs, input);
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


        private void loadSomething()
        {
            lock (this)
            {
                if (!this.loading)
                {
                    this.loading = true;
                    finalizeLoadAsync();
                }
            }
        }

        private UserInput m_loadedState = null;

        private async void finalizeLoadAsync()
        {
            await Task.Run(() =>
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    try
                    {
                        if (storage.FileExists("UserInputs.xml"))
                        {
                            using (IsolatedStorageFileStream fs = storage.OpenFile("UserInputs.xml", FileMode.Open))
                            {
                                if (fs != null)
                                {
                                    XmlSerializer mySerializer = new XmlSerializer(typeof(UserInput));
                                    m_loadedState = (UserInput)mySerializer.Deserialize(fs);
                                }
                            }
                        }
                    }
                    catch (IsolatedStorageException)
                    {
                        // Ideally show something to the user, but this is demo code :)
                    }
                }

                this.loading = false;
            });
        }
        }
    }

