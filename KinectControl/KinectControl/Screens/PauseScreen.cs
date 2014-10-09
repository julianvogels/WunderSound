using KinectControl.UI;
using KinectControl.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KinectControl.Screens
{
    public class PauseScreen : GameScreen
    {
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private GraphicsDevice graphics;
        private int screenWidth;
        private int screenHeight;
        private int counter;
        private ContentManager content;
        private string message;
        private Texture2D gradientTexture;

        public PauseScreen() { message = "No user detected, Game paused"; counter = 1; }
        public PauseScreen(string message) { this.message = message; counter = 1; }
        public PauseScreen(string message, int counter) { this.message = message; this.counter = counter; }

        public override void LoadContent()
        {
            content = ScreenManager.Game.Content;
            graphics = ScreenManager.GraphicsDevice;
            spriteBatch = ScreenManager.SpriteBatch;
            screenHeight = graphics.Viewport.Height;
            screenWidth = graphics.Viewport.Width;
            gradientTexture = content.Load<Texture2D>("Textures\\gradient");
            font = content.Load<SpriteFont>("SpriteFont1");
            base.LoadContent();
        }
        public override void Update(GameTime gameTime)
        {
            counter--;
            if (counter == 0)
            {
                this.Remove();
                //UnfreezeScreen();
            }
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            Vector2 viewportSize = new Vector2(screenWidth, screenHeight);
            Vector2 textSize = font.MeasureString(message);
            Vector2 textPosition = (viewportSize - textSize) / 2;
            int hPad = Constants.hPad;
            int vPad = Constants.vPad;
            Rectangle backgroundRectangle = new Rectangle((int)textPosition.X - hPad,
                                                          (int)textPosition.Y - vPad,
                                                          (int)textSize.X + hPad * 2,
                                                          (int)textSize.Y + vPad * 2);

            spriteBatch.Begin();
            spriteBatch.Draw(gradientTexture, backgroundRectangle, Color.White);
            spriteBatch.DrawString(font, message, textPosition, Color.Orange);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
