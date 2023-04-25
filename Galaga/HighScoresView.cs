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
    public class HighScoresView : GameStateView
    {
        private SpriteFont m_font;
        private Texture2D m_background;
<<<<<<< HEAD
        
        private bool loading = false;
=======

>>>>>>> 710a2d0c8ae097906dd48b2275ad08b37e4d3da1
        private const string MESSAGE = "These are the high scores";
        private bool loading = false;
        private List<int> highscores;

        public override void loadContent(ContentManager contentManager)
        {
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
            m_background = contentManager.Load<Texture2D>("Images/background");
            loadSomething();
            highscores = new List<int>();
        }

        public override GameStateEnum processInput(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                return GameStateEnum.MainMenu;
            }

            return GameStateEnum.HighScores;
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();

            m_spriteBatch.DrawString(m_font, "HIGH SCORES", new Vector2(700, 110), Color.Yellow);

            if (highscores.Count > 0)
            {
                for (int i = 0; i < (highscores.Count > 4 ? 5 : highscores.Count); i++)
                {
                    m_spriteBatch.DrawString(m_font, highscores[i].ToString(), new Vector2(700, 210 + (i * 75)), Color.Yellow);
                }


            }

            m_spriteBatch.End();

        }

        public override void update(GameTime gameTime)
        {
            loadSomething();
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
        private Scoring m_loadedState = null;

        private async void finalizeLoadAsync()
        {
            await Task.Run(() =>
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    try
                    {
                        if (storage.FileExists("HighScores.xml"))
                        {
                            using (IsolatedStorageFileStream fs = storage.OpenFile("HighScores.xml", FileMode.Open))
                            {
                                if (fs != null)
                                {
                                    XmlSerializer mySerializer = new XmlSerializer(typeof(Scoring));
                                    m_loadedState = (Scoring)mySerializer.Deserialize(fs);
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