using System;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Kinect;
using System.Threading;
using System.Linq;
namespace KinectControl.Common
{
   
    public class Kinect
    {
        #region Gesture's variables
        private GestureController gestureController;
        private string _gesture;
        public Device[] devices;
        private string[] commands;
        private VoiceCommands _voiceCommands;
        public event PropertyChangedEventHandler PropertyChanged;
        private static int framesCount;
        public CommunicationManager comm;
        public short[] RawDepthData { get; private set; }
        public Microsoft.Xna.Framework.Color[] DepthData { get; private set; }
        public static int FramesCount
        {
            get { return framesCount; }
            set { framesCount = value; }
        }
        public VoiceCommands voiceCommands
        {
            get { return _voiceCommands; }
            set { _voiceCommands = value; }
        }
        public String Gesture
        {
            get { return _gesture; }

            set
            {
                if (_gesture == value)
                    return;

                _gesture = value;

                //Debug.WriteLine("Gesture = " + _gesture);

                if (this.PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Gesture"));
            }
        }
        #endregion

        private Skeleton[] skeletons;
        public KinectSensor nui;

        //Tracked Skeleton
        public Skeleton trackedSkeleton;
        private int ScreenWidth, ScreenHeight;
        //Used for scaling
        private const float SkeletonMaxX = 0.60f;
        private const float SkeletonMaxY = 0.40f;


        public Kinect(int screenWidth, int screenHeight)
        {
            skeletons = new Skeleton[0];
            trackedSkeleton = null;
            //swapHand = new SwapHand();
            ScreenHeight = screenHeight;
            ScreenWidth = screenWidth; 
            this.InitializeNui();
        }


        /// <summary>
        /// Handle insertion of Kinect sensor.
        /// </summary>
        private void InitializeNui()
        {
            _gesture = "";
            var index = 0;
            while (this.nui == null && index < KinectSensor.KinectSensors.Count)
            {
                this.nui = KinectSensor.KinectSensors[index];
                this.nui.Start();
            }
            try
            {
                this.skeletons = new Skeleton[this.nui.SkeletonStream.FrameSkeletonArrayLength];
                var parameters = new TransformSmoothParameters
                {
                    Smoothing = 0.75f,
                    Correction = 0.0f,
                    Prediction = 0.0f,
                    JitterRadius = 0.05f,
                    MaxDeviationRadius = 0.04f
                };
                this.nui.SkeletonStream.Enable(parameters);
                this.nui.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                this.nui.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            }
            catch (Exception)
            { return; }
            this.nui.SkeletonFrameReady += this.OnSkeletonFrameReady;
            this.nui.DepthFrameReady += this.SensorDepthFrameReady;
            RawDepthData = new short[nui.DepthStream.FramePixelDataLength];
            DepthData = new Microsoft.Xna.Framework.Color[nui.DepthStream.FramePixelDataLength];
            gestureController = new GestureController();
            InitializeGestures();
            InitializeDevices();
            gestureController.GestureRecognized += OnGestureRecognized;
            InitializeVoiceGrammar();
        }
        /// <summary>
        /// Handler for the Kinect sensor's DepthFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="depthImageFrameReadyEventArgs">event arguments</param>
        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs depthImageFrameReadyEventArgs)
        {
            // Even though we un-register all our event handlers when the sensor
            // changes, there may still be an event for the old sensor in the queue
            // due to the way the nui delivers events.  So check again here.
            if (this.nui != sender)
            {
                return;
            }

            int pd = 0;
            if (trackedSkeleton != null)
            {
                var sp = trackedSkeleton.Joints.OrderBy(j => j.Position.Z).Last().Position;
                var dp = nui.CoordinateMapper.MapSkeletonPointToDepthPoint(sp, DepthImageFormat.Resolution320x240Fps30);
                pd = dp.Depth;
            }

            using (DepthImageFrame frame = depthImageFrameReadyEventArgs.OpenDepthImageFrame())
            {
                if (frame != null)
                {
                    frame.CopyPixelDataTo(RawDepthData);

                    // does some processing on the depth data using a Parallel For Loop
                    // the result in a Color array where the Red is the normalized depth value (ie: depth / max depth)
                    // and in case of "unknown depth" it assumes it has a dpeth of 1
                    // the green component is a player mask (1 of there is no player index, 0 otherwise)
                    System.Threading.Tasks.Parallel.For(0, RawDepthData.Length, i =>
                    {
                        var gray = RawDepthData[i] == -8 || RawDepthData[i] == 32760 ?
                            (byte)255 : (byte)(RawDepthData[i] / 125);
                        var pid = RawDepthData[i] % 8;

                        DepthData[i].A = 255;
                        DepthData[i].G = gray;
                        if (pid != 0 && RawDepthData[i] / 8 < pd)
                        {
                            DepthData[i].R = 0;
                            DepthData[i].B = 0;
                        }
                        else
                        {
                            DepthData[i].R = gray;
                            DepthData[i].B = gray;
                        }
                    });

                    //lock (this)
                    //{
                    //    // Make sure the depth texture is not assigned to the GPU
                    //    Game.GraphicsDevice.Textures[2] = null;
                    //    // Update the Depth Texture with the new Depth Data
                    //    DepthTex.SetData(depthData);
                    //}
                }
            }
        }

        public void InitializeDevices()
        {
            devices = new Device[1];
            devices[0] = new Device("LED1", "1", "0");
            //devices[1] = new Device("LED2", "2", "9");
            foreach(Device d in devices)
            {
                d.switchOff(comm);
            }
        }
        public void InitializeVoiceGrammar()
        {
            commands = new string[2];
            commands[0] = "Open";
            commands[1] = "One";
            _voiceCommands = new VoiceCommands(nui, commands);
            var voiceThread = new Thread(_voiceCommands.StartAudioStream);
            voiceThread.Start();
        }

        /// <summary>
        /// Handler for skeleton ready handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void OnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            // Get the frame.
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
          
                if(frame != null)
                try
                {
                    // Copy the skeleton data from the frame to an array used for temporary storage
                    frame.CopySkeletonDataTo(this.skeletons);
                    var accelerometerReading = this.nui.AccelerometerGetCurrentReading();

                    // Hand data to Interaction framework to be processed
                    //this.interactionStream.ProcessSkeleton(this.skeletons, accelerometerReading, skeletonFrame.Timestamp);
                    for (int i = 0; i < this.skeletons.Length; i++)
                    {
                        this.trackedSkeleton = this.skeletons.OrderBy(s => s.Position.Z)
                            .FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);
                    }
                    //  trackedSkeleton = skeletons[0];
                    framesCount++;
                    if (trackedSkeleton != null)
                    {
                        //if ((trackedSkeleton.Joints[JointType.HipCenter].TrackingState == JointTrackingState.Inferred) ||
                        //        (trackedSkeleton.Joints[JointType.HipCenter].TrackingState == JointTrackingState.NotTracked))
                        //   EnableNearModeSkeletalTracking();
                        //else if (this.nui.SkeletonStream.EnableTrackingInNearRange == true)
                        //  DisableNearModeSkeletalTracking();
                        //  EnableNearModeSkeletalTracking();
                        if (GenerateDepth() > 140)
                        {
                            //this.interactionStream.InteractionFrameReady += this.InteractionFrameReady;
                            gestureController.UpdateAllGestures(trackedSkeleton);
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    // SkeletonFrame functions may throw when the sensor gets
                    // into a bad state.  Ignore the frame in that case.
                }
                
            }
        }

        public Skeleton[] requestSkeleton()
        {
            return skeletons;
        }

        /// <summary>
        /// Returns right hand position scaled to screen.
        /// </summary>
        public Joint GetCursorPosition()
        {
            if (trackedSkeleton != null)
                return trackedSkeleton.Joints[JointType.HandRight].ScaleTo(ScreenWidth, ScreenHeight, SkeletonMaxX, SkeletonMaxY);
            else
                return new Joint();
        }
     
        private void OnGestureRecognized(object sender, GestureEventArgs e)
        {
            Debug.WriteLine(e.GestureType);
            framesCount=0;
            switch (e.GestureType)
            {
                case GestureType.SwipeUp:
                    Gesture = "SwipeUp";
                    //2,9
                    if (!devices[0].IsSwitchedOn)
                    {
                        devices[0].switchOn(comm);
                    }
                    break;
                case GestureType.SwipeDown:
                    Gesture = "SwipeDown";
                    if(devices[0].IsSwitchedOn)
                    devices[0].switchOff(comm);
                    break;
                default: Gesture = e.GestureType.ToString();
                    break;
            }
        }
        public void InitializeGestures()
        {
            nui.ElevationAngle = 15;
            comm = new CommunicationManager("115200");
            IRelativeGestureSegment[] waveLeftSegments = new IRelativeGestureSegment[6];
            WaveLeftSegment1 waveLeftSegment1 = new WaveLeftSegment1();
            WaveLeftSegment2 waveLeftSegment2 = new WaveLeftSegment2();
            waveLeftSegments[0] = waveLeftSegment1;
            waveLeftSegments[1] = waveLeftSegment2;
            waveLeftSegments[2] = waveLeftSegment1;
            waveLeftSegments[3] = waveLeftSegment2;
            waveLeftSegments[4] = waveLeftSegment1;
            waveLeftSegments[5] = waveLeftSegment2;
            this.gestureController.AddGesture(GestureType.WaveLeft, waveLeftSegments);
            IRelativeGestureSegment[] JoinedHandsSegments = new IRelativeGestureSegment[20];
            JoinedHandsSegment1 JoinedHandsSegment = new JoinedHandsSegment1();
            for (int i = 0; i < 20; i++)
            {
                // gesture consists of the same thing 10 times 
                JoinedHandsSegments[i] = JoinedHandsSegment;
            }
            //JoinedHandsSegment2 JoinedHandsSegment2 = new JoinedHandsSegment2();
            //JoinedHandsSegments[20] = JoinedHandsSegment2;
            this.gestureController.AddGesture(GestureType.JoinedHands, JoinedHandsSegments);

            IRelativeGestureSegment[] swipeUpSegments = new IRelativeGestureSegment[3];
            swipeUpSegments[0] = new SwipeUpSegment1();
            swipeUpSegments[1] = new SwipeUpSegment2();
            swipeUpSegments[2] = new SwipeUpSegment3();
            gestureController.AddGesture(GestureType.SwipeUp, swipeUpSegments);

            IRelativeGestureSegment[] swipeDownSegments = new IRelativeGestureSegment[3];
            swipeDownSegments[0] = new SwipeDownSegment1();
            swipeDownSegments[1] = new SwipeDownSegment2();
            swipeDownSegments[2] = new SwipeDownSegment3();
            gestureController.AddGesture(GestureType.SwipeDown, swipeDownSegments);

            IRelativeGestureSegment[] swipeLeftSegments = new IRelativeGestureSegment[3];
            swipeLeftSegments[0] = new SwipeLeftSegment1();
            swipeLeftSegments[1] = new SwipeLeftSegment2();
            swipeLeftSegments[2] = new SwipeLeftSegment3();
            gestureController.AddGesture(GestureType.SwipeLeft, swipeLeftSegments);

            IRelativeGestureSegment[] swipeRightSegments = new IRelativeGestureSegment[3];
            swipeRightSegments[0] = new SwipeRightSegment1();
            swipeRightSegments[1] = new SwipeRightSegment2();
            swipeRightSegments[2] = new SwipeRightSegment3();
            gestureController.AddGesture(GestureType.SwipeRight, swipeRightSegments);
            IRelativeGestureSegment[] menuSegments = new IRelativeGestureSegment[20];
            MenuSegment1 menuSegment = new MenuSegment1();
            for (int i = 0; i < 20; i++)
            {
                // gesture consists of the same thing 20 times 
                menuSegments[i] = menuSegment;
            }
            gestureController.AddGesture(GestureType.Menu, menuSegments);

            IRelativeGestureSegment[] joinedHandsSegments = new IRelativeGestureSegment[10];
             JoinedHandsSegment1 joinedHandsSegment = new JoinedHandsSegment1();
             for (int i = 0; i < 10; i++)
             {
                 // gesture consists of the same thing 10 times 
                 JoinedHandsSegments[i] = JoinedHandsSegment;
             }
             this.gestureController.AddGesture(GestureType.JoinedHands, JoinedHandsSegments);


            /*IRelativeGestureSegment[] joinedZoom = new IRelativeGestureSegment[13];
            JoinedHandsSegment1 joinedHandsSegment = new JoinedHandsSegment1();
            for (int i = 0; i < 10; i++)
            {
                joinedZoom[i] = joinedHandsSegment;
                //joinedHandsSegments[i] = joinedHandsSegment;
            }
            //this.gestureController.AddGesture(GestureType.JoinedHands, joinedHandsSegments);

            joinedZoom[10] = new ZoomSegment1();
            joinedZoom[11] = new ZoomSegment2();
            joinedZoom[12] = new ZoomSegment3();
            gestureController.AddGesture(GestureType.JoinedZoom, joinedZoom);
            */

            IRelativeGestureSegment[] zoomOutSegments = new IRelativeGestureSegment[3];
            zoomOutSegments[0] = new ZoomSegment3();
            zoomOutSegments[1] = new ZoomSegment2();
            zoomOutSegments[2] = new ZoomSegment1();
            gestureController.AddGesture(GestureType.ZoomOut, zoomOutSegments);

            IRelativeGestureSegment[] zoomInSegments = new IRelativeGestureSegment[3];
            zoomInSegments[0] = new ZoomSegment1();
            zoomInSegments[1] = new ZoomSegment2();
            zoomInSegments[2] = new ZoomSegment3();
            gestureController.AddGesture(GestureType.ZoomIn, zoomInSegments);

            gestureController.GestureRecognized += OnGestureRecognized;
            
        }

        
        /// <returns>
        /// Int number which is the calculated depth.
        /// </returns>
        public int GenerateDepth()
        {
            try
            {
                return (int)(100 * this.trackedSkeleton.Joints[JointType.HipCenter].Position.Z);
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
