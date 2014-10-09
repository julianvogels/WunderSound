using Microsoft.Kinect;
using KinectControl.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KinectControl.UI
{
    class HandCursor
    {
        private Texture2D texture;
        private Kinect kinect;
        public Vector2 position{get; set;}

        public HandCursor()
        {

        }

        public void Initialize(Kinect kinect)
        {
            this.kinect = kinect;
        }

        public void LoadContent(ContentManager Content)
        {
            texture = Content.Load<Texture2D>("Textures/cursor2");
        }

        public void Update(GameTime gameTime)
        {
            Joint RightHand = kinect.GetCursorPosition();
            position = new Vector2(RightHand.Position.X, RightHand.Position.Y);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height), null, Color.White, 
                0, new Vector2(texture.Width/2, texture.Height/2), SpriteEffects.None, 0);
        }
    }
}
