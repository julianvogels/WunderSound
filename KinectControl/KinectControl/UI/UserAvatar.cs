using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using KinectControl.Common;
namespace KinectControl.UI
{

    /// <summary>
    /// This class represents an avatar which tracks a player's distance from the kinect device and changes its color according to this distance, its implemented to work for two players aswell since the kinect device can only track two players maximum at the same time
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class UserAvatar
    {
        private Kinect kinect;
        private GraphicsDevice graphics;
        private SpriteBatch spriteBatch;
        private int screenWidth;
        private int screenHeight;
        private ContentManager content;
        private Texture2D avatar;
        private Vector2 avatarPosition;
        private SpriteFont font;
        const int minDepth = 120;
        const int maxDepth = 350;
        private int depth;
        private Texture2D[] allAvatars;
        public Texture2D Avatar
        {
            get { return avatar; }
        }
        public Texture2D[] AllAvatars
        {
            get { return allAvatars; }
        }

        /// <summary>
        /// Class constructor for 1 player mode.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public UserAvatar(Kinect kinect, ContentManager content, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            this.kinect =  kinect;
            this.graphics = graphicsDevice;
            screenWidth = graphics.Viewport.Width;
            screenHeight = graphics.Viewport.Height;
            this.spriteBatch = spriteBatch;
            this.content = content;
            allAvatars = new Texture2D[4];
        }
        /// <summary>
        /// LoadContent will be called only once before drawing and it's the place to load
        /// all of your content.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public void LoadContent()
        {
            font = content.Load<SpriteFont>("spriteFont1");
            allAvatars[0] = content.Load<Texture2D>(@"Textures/avatar-dead");
            allAvatars[1] = content.Load<Texture2D>(@"Textures/avatar-white");
            allAvatars[2] = content.Load<Texture2D>(@"Textures/avatar-green");
            allAvatars[3] = content.Load<Texture2D>(@"Textures/avatar-red");
                avatar = allAvatars[0];
            avatarPosition = new Vector2((screenWidth + 25), (screenHeight / 3.4f));
           
        }

        /// <summary>
        /// This is called when the game screen should draw itself.
        /// </summary>
        /// <remarks>
        ///</remarks>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>    
        public void Draw(GameTime gameTime)
        {
                spriteBatch.Draw(avatar, avatarPosition, null, Color.White, 0,
                    new Vector2(avatar.Width, avatar.Height), 1f, SpriteEffects.None, 0);
        }


        /// <summary>
        /// Allows the game screen to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <remarks>
        ///</remarks>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
                if (kinect.trackedSkeleton!= null)
                    UpdateUser();
        }

        /// <summary>
        /// Takes the user's index in the users array and calculates the player's distance from the kinect device, and updates the notification message that should be printed if the user is not detected or too far away.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="ID">
        /// The user's index in users array.
        /// </param>
        public void UpdateUser()
        {
            depth = kinect.GenerateDepth();
            if (depth == 0)
            {
                avatar = allAvatars[0];
            }
            else
            {
                if (depth < minDepth)
                    avatar = allAvatars[3];
                else if (depth > maxDepth)
                    avatar = allAvatars[1];
                else if (depth < maxDepth)
                    avatar = allAvatars[2];
            }
        }

    }
}
