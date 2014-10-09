using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using KinectControl.Common;
using System.Collections.Generic;
using System;
using KinectControl.Screens;

namespace KinectControl.UI
{
    #region ScreenState
    /// <summary>
    /// Represents the screen states.
    /// </summary>
    public enum ScreenState
    {
        Active,
        Frozen,
        Hidden
    }
    #endregion

    /// <summary>
    /// This class represents a screen.
    /// </summary>
    public abstract class GameScreen
    {
        private ContentManager content;
        private SpriteBatch spriteBatch;
        public bool enablePause;
        public bool screenPaused;
        private SpriteFont font;
        Texture2D depthTex;
        int? lastSkelId;
        double depthTimer;
        private int frameNumber;
        public int FrameNumber
        {
            get
            {
                return frameNumber;
            }
            set
            {
            }
        }
        private UserAvatar userAvatar;
        public UserAvatar UserAvatar
        {
            get { return userAvatar; }
            set { userAvatar = value; }
        }
        private VoiceCommands voiceCommands;

        public bool IsFrozen
        {
            get
            {
                return screenState == ScreenState.Frozen;
            }
        }

        private ScreenState screenState;
        public ScreenState ScreenState
        {
            get { return screenState; }
            set { screenState = value; }
        }
        private ScreenManager screenManager;
        public ScreenManager ScreenManager
        {
            get { return screenManager; }
            set { screenManager = value; }
        }

        public bool IsActive
        {
            get
            {
                return screenState == ScreenState.Active;
            }
        }
        public bool showAvatar = true;
        /// <summary>
        /// LoadContent will be called only once before drawing and it's the place to load
        /// all of your content.
        /// </summary>
        public virtual void LoadContent()
        {
            frameNumber = Kinect.FramesCount;
            content = ScreenManager.Game.Content;
            spriteBatch = ScreenManager.SpriteBatch;
            depthTex = new Texture2D(screenManager.GraphicsDevice, 320, 240);
            font = content.Load<SpriteFont>("SpriteFont1");
            MediaPlayer.Stop(); // stop current audio playback 
            // generate a random valid index into Albums
            voiceCommands = ScreenManager.Kinect.voiceCommands;
            if (showAvatar)
            {
                userAvatar = new UserAvatar(ScreenManager.Kinect, content, ScreenManager.GraphicsDevice, spriteBatch);
                userAvatar.LoadContent();
            }
        }
        /// <summary>
        /// Initializes the GameScreen.
        /// </summary
        public virtual void Initialize()
        {
            screenPaused = false;
        }

        /// <summary>
        /// Unloads the content of GameScreen.
        /// </summary>
        public virtual void UnloadContent() {
        }
        /// <summary>
        /// Allows the game screen to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public virtual void Update(GameTime gameTime)
        {
            if (showAvatar)
            {
                userAvatar.Update(gameTime);
                if (!IsFrozen)
                if (enablePause)
                {
                    if (userAvatar.Avatar == userAvatar.AllAvatars[0])
                    {
                        //Freeze Screen, Show pause Screen\
                        screenPaused = true;
                        ScreenManager.AddScreen(new PauseScreen());
                        this.FreezeScreen();
                    }
                    else if (userAvatar.Avatar.Equals(userAvatar.AllAvatars[2]) && screenPaused == true)
                    {
                        //exit pause screen, unfreeze screen
                        this.UnfreezeScreen();
                    }
                }
            }

            if (frameNumber % 360 == 0 && voiceCommands!=null)
            {
                voiceCommands.HeardString = "";
            }
               var currSkel = screenManager.Kinect.trackedSkeleton;
                int? currId = null;
                if (currSkel != null) currId = currSkel.TrackingId;
                
                if (lastSkelId != currId)
                {
                    depthTimer = 2;
                    lastSkelId = currId;
                }

                if (depthTimer > 0)
                    depthTimer -= gameTime.ElapsedGameTime.TotalSeconds;
        
            frameNumber++;
            if (voiceCommands != null)
            {
                switch (voiceCommands.HeardString)
                {
                    case "One":
                        if (ScreenManager.Kinect.devices[0].IsSwitchedOn)
                            ScreenManager.Kinect.devices[0].switchOff(ScreenManager.Kinect.comm);
                        else
                            ScreenManager.Kinect.devices[0].switchOn(ScreenManager.Kinect.comm);
                        //this.FreezeScreen();
                        screenManager.AddScreen(new PauseScreen(ScreenManager.Kinect.devices[0].Name +" is "+ScreenManager.Kinect.devices[0].Status,300));
                        voiceCommands.HeardString = "";
                        break;
                 /*   case "Open":
                        if (ScreenManager.Kinect.devices[1].IsSwitchedOn)
                            ScreenManager.Kinect.devices[1].switchOff(ScreenManager.Kinect.comm);
                        else
                            ScreenManager.Kinect.devices[1].switchOn(ScreenManager.Kinect.comm);
                        screenManager.AddScreen(new PauseScreen(ScreenManager.Kinect.devices[1].Name + " is " + ScreenManager.Kinect.devices[1].Status,300));
                        //this.FreezeScreen();
                        voiceCommands.HeardString = "";
                        break;
                  * */
                    //case "volume up":
                    //    MediaPlayer.Volume++;
                    //    voiceCommands.HeardString="";
                    //    break;
                    //case "volume down":
                    //    MediaPlayer.Volume--;
                    //    voiceCommands.HeardString = "";
                    //    break;
                    default: break;
                }
            }
        }
             

        /// <summary>
        /// Removes the current screen.
        /// </summary>
        public virtual void Remove()
        {
            screenManager.RemoveScreen(this);
        }

        /// <summary>
        /// This is called when the game screen should draw itself.
        /// </summary>
        public virtual void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            if (showAvatar&& screenManager.Kinect.DepthData != null)
            {
                depthTex.SetData(screenManager.Kinect.DepthData);

                var a = MathHelper.Clamp((float)depthTimer, 0, 1);
                spriteBatch.Draw(depthTex, new Vector2(1280 - 320, 0), new Color(1, 1, 1, a));
                //userAvatar.Draw(gameTime);
            }
            if(voiceCommands!=null && !voiceCommands.HeardString.Equals(""))
            spriteBatch.DrawString(font,"voice command recognized: " + voiceCommands.HeardString + screenManager.Kinect.comm.msg, new Vector2(300,300), Color.Orange);
            if(screenManager.Kinect.comm != null)
            if((screenManager.Kinect.comm.msg != null) && (!screenManager.Kinect.comm.msg.Equals("")))
            spriteBatch.DrawString(font, "Arduino says: " + screenManager.Kinect.comm.msg, new Vector2(300, 600), Color.Orange);
            spriteBatch.End();
            
        }
        public void FreezeScreen()
        {
            screenState = ScreenState.Frozen;
        }

        public void UnfreezeScreen()
        {
            screenState = ScreenState.Active;
        }


    }
}