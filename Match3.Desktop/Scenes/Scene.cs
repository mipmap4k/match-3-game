using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Match3.Desktop.Scenes;

public abstract class Scene {
    protected Game1 Game { get; }

    protected Scene(Game1 game) {
        Game = game;
    }

    public virtual void LoadContent() { }
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
}
