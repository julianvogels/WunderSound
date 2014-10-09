using KinectControl.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KinectControl.Screens
{
    /// <summary>
    /// This class Represents the Terasoft screen.
    /// </summary>
    class IntroScreen : FadingScreen
    {
        private float scale;
        private Texture2D Logo;
        /// <summary>
        /// Creates a new instance of the TeraSoftScreen.
        /// </summary>
        public IntroScreen()
            : base("Textures/Wundersound", 0.6f, 0, 0, -0.06f)
        {
            scale = 0.3f;
        }

        /// <summary>
        /// Loads the content of this screen.
        /// </summary>
        public override void LoadContent()
        {
            Logo = ScreenManager.Game.Content.Load<Texture2D>(@"Textures/Wundersound");
            base.LoadContent();
        }
        /// <summary>
        /// Updates the content of this screen.
        /// </summary>
        /// <remarks><para>AUTHOR: Ahmed Badr.</para></remarks>
        /// <param name="gameTime">Represents the time of the game.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (Done)
            {
                base.Remove();
                ScreenManager.AddScreen(new MainScreen());
            }
        }

        /// <summary>
        /// Draws the content of this screen.
        /// </summary>
        /// <remarks><para>AUTHOR: Ahmed Badr.</para></remarks>
        /// <param name="gameTime">Represents the time of the game.</param>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.White);
            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.Draw(Logo, new Vector2(((ScreenManager.GraphicsDevice.Viewport.Width
                - Logo.Width * scale) / 1.1f), ((ScreenManager.GraphicsDevice.Viewport.Height - (Logo.Height)
                * scale) / 1.1f)), null, Color.White, 0, new Vector2(0, 0), new Vector2(scale, scale),
                SpriteEffects.None, 0);
            ScreenManager.SpriteBatch.End();
            base.Draw(gameTime);

        }
    }
}
