using Microsoft.Xna.Framework.Graphics;
using KinectControl.UI;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using KinectControl.Common;
using System.Text;

namespace KinectControl.Screens
{
    class MainScreen : GameScreen
    {
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private SpriteFont font2;
        private Kinect kinect;
        private string gesture;
        private GraphicsDevice graphics;
        private int screenWidth,screenHeight;
        private Button button;
        private HandCursor hand;
        private ContentManager content;
        private Texture2D gradientTexture;
        private string textToDraw;
        private string text;
        private Rectangle textBox;
        private Vector2 textPosition;
        public string Text
        {
            get { return text; }
            set { text = value; }
        }
        public MainScreen()
        {
            
        }

        public override void Initialize()
        {
            showAvatar = true;
            enablePause = false;
            button = new Button();
            hand = new HandCursor();
            hand.Initialize(ScreenManager.Kinect);
            button.Initialize("Buttons/ok", this.ScreenManager.Kinect, new Vector2(820, 350));
            button.Clicked += new Button.ClickedEventHandler(button_Clicked);
            Text = "- Welcome to Wundersound, You will see your depth image colored in green if you're \n standing in the kinect's field of view, smile and wave. \n -to switch on an LED say \"One\" or do the swipe up/down gestures. \n - You can also try the joined hands, waveleft and zoom in/out gestures just for the hell of it. \n - Press the Ok button to exit.";
            textPosition = new Vector2(75, 145);
            textBox = new Rectangle((int)textPosition.X, (int)textPosition.Y, 1020, 455);
            base.Initialize();
        }
        void button_Clicked(object sender, System.EventArgs a)
        {
           // ScreenManager.Kinect.comm.ClosePort();
            //this.Remove();
            this.ScreenManager.Game.Exit();
        }
        public override void LoadContent()
        {
            kinect = ScreenManager.Kinect;
            gesture = kinect.Gesture;
            content = ScreenManager.Game.Content;
            graphics = ScreenManager.GraphicsDevice;
            spriteBatch = ScreenManager.SpriteBatch;
            screenHeight = graphics.Viewport.Height;
            screenWidth = graphics.Viewport.Width;
            gradientTexture = content.Load<Texture2D>("Textures/gradientTexture");
            font = content.Load<SpriteFont>("SpriteFont1");
            font2 = content.Load<SpriteFont>("Fontopo");
            //font2.LineSpacing = 21;
            hand.LoadContent(content);
            button.LoadContent(content);
            textToDraw = WrapText(font2, text, 9000);
            base.LoadContent();
        }
        public override void Update(GameTime gameTime)
        {
            hand.Update(gameTime);
            button.Update(gameTime);
            button.Clicked += button_Clicked;
            gesture = kinect.Gesture;
            if (FrameNumber % 240 == 0)
                kinect.Gesture = "";
            base.Update(gameTime);
        }
          public string WrapText(SpriteFont spriteFont, string text, float maxLineWidth)
        {
            string[] words = text.Split(' ');

            StringBuilder builder = new StringBuilder();

            float lineWidth = 0f;

            float spaceWidth = spriteFont.MeasureString(" ").X;

            foreach (string word in words)
            {
                Vector2 size = spriteFont.MeasureString(word);

                if (lineWidth + size.X < maxLineWidth)
                {
                    builder.Append(word + " ");
                    lineWidth += size.X + spaceWidth;
                }
                else
                {
                    builder.Append("\n" + word + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }

            return builder.ToString();
        }
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(gradientTexture, new Rectangle(0, 0, 1280, 720), Color.White);
            if (!(gesture.Equals("")))
            spriteBatch.DrawString(font, "gesture recognized: " + gesture, new Vector2(500,500), Color.Orange);
            spriteBatch.DrawString(font2, textToDraw, textPosition, Color.White);
            button.Draw(spriteBatch);
            hand.Draw(spriteBatch);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
