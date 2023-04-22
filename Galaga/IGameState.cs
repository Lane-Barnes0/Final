using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Galaga
{
    public interface IGameState
    {
        void initialize(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics);
        void loadContent(ContentManager contentManager);
        GameStateEnum processInput(GameTime gameTime);
        void update(GameTime gameTime);
        void render(GameTime gameTime);
    }
}
