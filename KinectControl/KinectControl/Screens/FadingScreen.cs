using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using KinectControl.UI;

namespace KinectControl.Screens
{
    /// <summary>
    /// This class creates a screen that fades in and out. The screen has a white background.
    /// </summary>
    class FadingScreen : GameScreen
    {
        private bool first;
        private float fading;
        private Texture2D black;
        private Texture2D logo;
        private string path;
        private float scale;
        private float rotation;
        private float xPositionPercentOffset;
        private float yPositionPercentOffset;
        private bool done;

        public bool Done
        {
            set
            {
                done = value;
            }
            get
            {
                return done;
            }
        }

        /// <summary>
        /// Creates a new instance of FadingScreen.
        /// </summary>
        /// <param name="path">Path of the image that will be added to the middle of the screen.</param>
        /// <param name="logoScale">Scaling ratio of the image according to the screen's width.</param>
        /// <param name="rotation">Rotation for the image.</param>
        /// <param name="xPositionPercentOffset">X-axis position offset of the image.</param>
        /// <param name="yPositionPercentOffset">Y-axis position offset of the image.</param>
        public FadingScreen(string path, float logoScalePercentage, float rotation, float xPositionPercentOffset,
            float yPositionPercentOffset)
        {
            this.path = path;
            this.rotation = rotation;
            this.xPositionPercentOffset = xPositionPercentOffset;
            this.yPositionPercentOffset = yPositionPercentOffset;
            first = true;
            fading = 1f;
            scale = logoScalePercentage;
            done = false;
            showAvatar = false;
        }
        /// <summary>
        /// Loads the content of this screen.
        /// </summary>
        public override void LoadContent()
        {
            black = ScreenManager.Game.Content.Load<Texture2D>(@"Textures/black");
            logo = ScreenManager.Game.Content.Load<Texture2D>(@"" + path);
            scale = ScreenManager.GraphicsDevice.Viewport.Width * scale / logo.Width;
            this.xPositionPercentOffset = ScreenManager.GraphicsDevice.Viewport.Width * xPositionPercentOffset;
            this.yPositionPercentOffset = ScreenManager.GraphicsDevice.Viewport.Height * yPositionPercentOffset;
        }

        /// <summary>
        /// Updates the content of this screen.
        /// </summary>
        /// <param name="gameTime">Represents the time of the game.</param>
        public override void Update(GameTime gameTime)
        {
            if ((fading <= 0.01f) || !first)
            {
                if (first)
                {
                    first = false;
                    Thread.Sleep(1500);
                }

                fading /= 0.96f;
                if (fading > 0.999)
                    Done = true;
            }
            else
                fading *= 0.97f;
        }
        /// <summary>
        /// Draws the content of the fading screen.
        /// </summary>
        /// <param name="gameTime">Represents the time of the game.</param>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.Draw(logo, new Vector2(((ScreenManager.GraphicsDevice.Viewport.Width
                - logo.Width * scale) / 2) + xPositionPercentOffset, ((ScreenManager.GraphicsDevice.Viewport.Height
                - (logo.Height) * scale) / 2) + yPositionPercentOffset), null, Color.White, rotation, new Vector2(0, 0),
                new Vector2(scale, scale), SpriteEffects.None, 0);

            ScreenManager.SpriteBatch.Draw(black, Vector2.Zero, null, Color.White * fading, 0, new Vector2(0, 0),
                new Vector2(ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height),
                SpriteEffects.None, 0);
            ScreenManager.SpriteBatch.End();
        }

    }
}
