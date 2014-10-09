using System;
using KinectControl.Common;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KinectControl.UI
{
    public class Button
    {

        private Texture2D texture;
        private Vector2 position;
        public Vector2 Position { get { return position; } set { position = value; } }
        private string texturePath;
        private int textureWidth, textureHeight;

        private Kinect kinect;
        private float timer = 0;
        private const float CLICK_TIME_OUT = 1400;

        private Rectangle boundingRectangle;
        private bool textureBoundsSet = false;

        private bool clicked;
        private Color hoverColor;

        //Event firing
        public delegate void ClickedEventHandler(object sender, EventArgs a);
        public event ClickedEventHandler Clicked;

        public Button()
        {

        }

        public void Initialize(string Path, Kinect kinect, Vector2 Position)
        {
            texturePath = Path;
            this.kinect = kinect;
            position = Position;
            textureBoundsSet = false;
            hoverColor = new Color(255, 255, 255, 220);
        }

        public void Initialize(string Path, Kinect kinect, Vector2 Position, int buttonWidth, int buttonHeight)
        {
            texturePath = Path;
            this.kinect = kinect;
            position = Position;
            textureWidth = buttonWidth;
            textureHeight = buttonHeight;
            textureBoundsSet = true;
            hoverColor = new Color(255, 255, 255, 220);
        }

        public void LoadContent(ContentManager Content)
        {
            texture = Content.Load<Texture2D>(texturePath);
            if (!textureBoundsSet)
            {
                textureWidth = texture.Width;
                textureHeight = texture.Height;
                boundingRectangle = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
            }
            else
                boundingRectangle = new Rectangle((int)position.X, (int)position.Y, textureWidth, textureHeight);
        }

        public void Update(GameTime gameTime)
        {
            Joint rightHand = kinect.GetCursorPosition();
            Point handPoint = new Point((int)rightHand.Position.X, (int)rightHand.Position.Y);

            if (boundingRectangle.Contains(handPoint))
            {
                if (clicked)
                {
                    timer -= 200;
                }
                else
                {
                    timer += gameTime.ElapsedGameTime.Milliseconds;
                }

                if (timer >= CLICK_TIME_OUT)
                {
                    timer -= 100;
                    if (Clicked != null)
                        Clicked(this, null);
                    clicked = true;
                }
                else if (timer <= 0)
                    clicked = false;

            }
            else
            {
                timer = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, boundingRectangle, Color.White);

            if (timer > 0 && timer < CLICK_TIME_OUT)
            {
                float overlayHeight = 0f;
                overlayHeight = ((timer / CLICK_TIME_OUT) * texture.Height);
                spriteBatch.Draw(texture, position, new Rectangle(0, 0, texture.Width, (int)overlayHeight), hoverColor);
            }
        }


    }
}
