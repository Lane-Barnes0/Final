using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Metadata;

namespace Galaga
{
    public class MainMenuView : GameStateView
    {
        private SpriteFont m_fontMenu;
        private SpriteFont m_fontMenuSelect;
        private Texture2D m_background;
        private KeyboardState m_previousKeyboard;
        private enum MenuState
        {
            NewGame,
            HighScores,
            Controls,
            Credits,
            
            Exit
            
        }

        private MenuState m_currentSelection = MenuState.NewGame;
        private bool m_waitForKeyRelease = false;

        public override void loadContent(ContentManager contentManager)
        {
            m_background = contentManager.Load<Texture2D>("Images/background");
            m_fontMenu = contentManager.Load<SpriteFont>("Fonts/menu");
            m_fontMenuSelect = contentManager.Load<SpriteFont>("Fonts/menu-select");
        }
        public override GameStateEnum processInput(GameTime gameTime)
        {
            
            // This is the technique I'm using to ensure one keypress makes one menu navigation move
            if (!m_waitForKeyRelease)
            {
                // Arrow keys to navigate the menu
                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                {

                    if (m_currentSelection != MenuState.Exit)
                    {
                        m_currentSelection = m_currentSelection + 1;
                    }

                    m_waitForKeyRelease = true;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Up))
                {
                    if(m_currentSelection != MenuState.NewGame)
                    {
                        m_currentSelection = m_currentSelection - 1;
                    }
                    
                    m_waitForKeyRelease = true;
                }

                // If enter is pressed, return the appropriate new state
                if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !m_previousKeyboard.IsKeyDown(Keys.Enter) && m_currentSelection == MenuState.NewGame)
                {
                   
                    return GameStateEnum.NewGame;
                    
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !m_previousKeyboard.IsKeyDown(Keys.Enter) && m_currentSelection == MenuState.HighScores)
                {
                    return GameStateEnum.HighScores;
                }
   
                if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !m_previousKeyboard.IsKeyDown(Keys.Enter) && m_currentSelection == MenuState.Credits)
                {
                    return GameStateEnum.Credits;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !m_previousKeyboard.IsKeyDown(Keys.Enter) && m_currentSelection == MenuState.Controls)
                {
                    return GameStateEnum.Controls;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !m_previousKeyboard.IsKeyDown(Keys.Enter) && m_currentSelection == MenuState.Exit)
                {
                    return GameStateEnum.Exit;
                }
            }
            else if (Keyboard.GetState().IsKeyUp(Keys.Down) && Keyboard.GetState().IsKeyUp(Keys.Up))
            {
                m_waitForKeyRelease = false;
            }

            return GameStateEnum.MainMenu;
        }
        public override void update(GameTime gameTime)
        {
            m_previousKeyboard = Keyboard.GetState();
        }
        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();
            m_spriteBatch.Draw(m_background, new Rectangle(0, 0, m_graphics.PreferredBackBufferWidth, m_graphics.PreferredBackBufferHeight), Color.White);

            // I split the first one's parameters on separate lines to help you see them better
            float bottom = drawMenuItem(
                m_currentSelection == MenuState.NewGame ? m_fontMenuSelect : m_fontMenu,
                "New Game",
                200,
                m_currentSelection == MenuState.NewGame ? Color.Yellow : Color.Blue);
            bottom = drawMenuItem(m_currentSelection == MenuState.HighScores ? m_fontMenuSelect : m_fontMenu, "High Scores", bottom, m_currentSelection == MenuState.HighScores ? Color.Yellow : Color.Blue);
            bottom = drawMenuItem(m_currentSelection == MenuState.Controls ? m_fontMenuSelect : m_fontMenu, "Controls", bottom, m_currentSelection == MenuState.Controls ? Color.Yellow : Color.Blue);
            bottom = drawMenuItem(m_currentSelection == MenuState.Credits ? m_fontMenuSelect : m_fontMenu, "Credits", bottom, m_currentSelection == MenuState.Credits ? Color.Yellow : Color.Blue);
            
            drawMenuItem(m_currentSelection == MenuState.Exit ? m_fontMenuSelect : m_fontMenu, "Quit", bottom, m_currentSelection == MenuState.Exit ? Color.Yellow : Color.Blue);

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
    }
}