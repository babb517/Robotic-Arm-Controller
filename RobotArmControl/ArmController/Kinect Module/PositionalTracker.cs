using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;

namespace RobotArmControl.Kinect_Module
{
    class PositionalTracker
    {
        #region Constants
        /**********************************************************************/
        /* Constants */
        /**********************************************************************/

        #endregion Constants

        #region Private Members
        /**********************************************************************/
        /* Private Members */
        /**********************************************************************/

        /// <summary>
        /// The kinect sensor.
        /// </summary>
        private KinectSensorChooser _snsr;

        /// <summary>
        /// The skeleton data from the kinect.
        /// </summary>
        private Skeleton[] _skeletonData;

        /// <summary>
        /// The rendering object used to display rendering output
        /// </summary>
        private SkeletonRenderer _renderer;

        /// <summary>
        /// A handler used to handle kinect data once it's ready.
        /// </summary>
        private EventHandler<SkeletonFrameReadyEventArgs> _skeletonReadyHandler;

        /// <summary>
        /// A member used to track the player we're currently working with.
        /// A value of null indicates that a player hasn't been found and then next available player will be registered as player one.
        /// </summary>
        public Skeleton _playerOne;

        #endregion Private Members

        #region Public Members
        /**********************************************************************/
        /* Private Members */
        /**********************************************************************
        

        /// <summary>
        /// A virtual member used to access the canvas we're drawing on.
        /// </summary>
        public DrawingGroup Canvas
        {
            get
            {
                return _renderer.Canvas;
            }

            set
            {
                _renderer.Canvas = value;
            }
        }

        /// <summary>
        /// A virtual member to get the status of the kinect sensor.
        /// </summary>
        public ChooserStatus Status
        {
            get
            {
                return _snsr.Status;
            }
        }


        #endregion Private Members

        #region Constructors
        /**********************************************************************/
        /* Constructors */
        /**********************************************************************/

        /// <summary>
        /// Initializes the tracker object.
        /// </summary>
        /// <param name="canvas">The canvas to draw vision data onto if applicable, null otherwise.</param>
        public PositionalTracker(DrawingGroup canvas = null)
        {
            Debug.WriteLine("Initializing the positional tracker with canvas: " + canvas);

            _playerOne = null;

            // setup the sensor
            _snsr = new KinectSensorChooser();
            _snsr.PropertyChanged += new PropertyChangedEventHandler(OnKinectPropertyChanged);
            _skeletonReadyHandler = new EventHandler<SkeletonFrameReadyEventArgs>(OnKinectDataReady); 


            // setup the renderer
            _renderer = new SkeletonRenderer(SkeletonPointToScreen, canvas);

           
        }

        #endregion Constructors

        #region Public Methods
        /**********************************************************************/
        /* Public Methods */
        /**********************************************************************/



        public void Start()
        {
            Debug.WriteLine("Starting the kinect...");
            StartKinectST();
        }

        public void Stop()
        {
            Debug.WriteLine("Stopping the kinect...");
            StopKinectST();
        }

        #endregion Public Methods

        #region Event Handling
        /**********************************************************************/
        /* Event Handling */
        /**********************************************************************/

        /// <summary>
        /// A handler to take care of the sensor changing during operation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKinectSensorChanged(object sender, KinectChangedEventArgs e)
        {
            Debug.WriteLine("Detected a change in the active Kinect sensor from " + e.OldSensor + " to " + e.NewSensor);

            // Release the old sensor
            if (e.OldSensor != null)
            {
                e.OldSensor.SkeletonFrameReady -= _skeletonReadyHandler;
            }

            // Setup the new sensor
            if (e.NewSensor != null)
            {
                SetupKinect(e.NewSensor);
            }
        }

        /// <summary>
        /// A handler used to track the status of the kinect chooser interface and report it to the debug console.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKinectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine("Got a property changed event.");
            // Listen for the status and output it to the debug console.
            if (e.PropertyName == "Status")
            {
                Debug.WriteLine(("Sensor Status Changed: " + ((KinectSensorChooser)sender).Status.ToString()));

                // If the sensor is ready make sure we set it up.
                if (((KinectSensorChooser)sender).Status == ChooserStatus.SensorStarted)
                {
                    SetupKinect(((KinectSensorChooser)sender).Kinect);
                }
            }
        }

        /// <summary>
        /// Acquires the skeletal data from the kinect frame, processes the data, and publishes the relevent data to the virtual bus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">An event object containing the most recent skeletal frame.</param>
        private void OnKinectDataReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Debug.WriteLine("Kinect Data is ready, processing it...");

            // unpackage the skeletal data
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null && this._skeletonData != null)         // check that a frame is available
                {
                    skeletonFrame.CopySkeletonDataTo(this._skeletonData);        // get the skeletal information in this frame
                }
            }

            SelectPlayerOne();

            ProcessSkeletalData();
            PublishSkeletalData();
        }

        #endregion Event Handling

        #region Private Methods
        /**********************************************************************/
        /* Private Methods */
        /**********************************************************************/

        /// <summary>
        /// Starts the kinect sensor and starts listening for the kinect publishing data.
        /// This method is taken almost directly from the MSDN example: http://msdn.microsoft.com/en-us/library/jj131025.aspx
        /// </summary>
        private void StartKinectST()
        {
            if (_snsr.Kinect != null) _snsr.Kinect.SkeletonFrameReady += _skeletonReadyHandler;
            _snsr.Start();
        }

        /// <summary>
        /// Stops the kinect sensor.
        /// </summary>
        private void StopKinectST()
        {
            _snsr.Kinect.SkeletonFrameReady -= _skeletonReadyHandler;
            _snsr.Stop();
        }

        /// <summary>
        /// Sets up a kinect sensor for used.
        /// Currently only one sensor may be in use at a time.
        /// </summary>
        /// <param name="snsr">The kinect sensor to setup.</param>
        private void SetupKinect(KinectSensor snsr)
        {
            try
            {
                snsr.SkeletonStream.Enable();                                                    // Enable skeletal tracking
            }
            catch (InvalidOperationException e)
            {
                // These are caused by a conflict between multiple applications trying to control the same kinect.
                _snsr.Stop();
                _snsr.Start();
            }

            _skeletonData = new Skeleton[snsr.SkeletonStream.FrameSkeletonArrayLength];      // Allocate ST data
            snsr.SkeletonFrameReady += _skeletonReadyHandler;                                // Get Ready for Skeleton Ready Events        
            snsr.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;                  // Change the sensor mode to seated.
        }

        /// <summary>
        /// Scans through our skeletons and selects player one based on our previous selection and the position of each skeleton.
        /// </summary>
        private void SelectPlayerOne()
        {
            bool foundPreviousPlayerOne = false;
            Skeleton closest = null;

            // found our tracked skeletons
            foreach (Skeleton skel in _skeletonData)
            {
                // check if we've found our previous selection
                if (skel.TrackingState == SkeletonTrackingState.Tracked) {
                    if (_playerOne != null && skel.TrackingId == _playerOne.TrackingId)
                    {
                        foundPreviousPlayerOne = true;
                        break;
                    } else {
                        // keep track of the closest skeleton we've found
                        if (closest == null || skel.Position.Z < closest.Position.Z)
                            closest = skel;
                    }
                }
            }

            // If our previous player has left select the closest candidate as the new player one.
            if (!foundPreviousPlayerOne) _playerOne = closest;

        }

        /// <summary>
        /// Prepocesses the skeleton data giving us information like the appropriate joint angles.
        /// </summary>
        private void ProcessSkeletalData()
        {
            if (_playerOne == null)
            {
                Debug.WriteLine("Player not found...");
            } 
            else if (_playerOne.TrackingState == SkeletonTrackingState.Tracked)
            {


            }
        }

        /// <summary>
        /// Publishes the skeleton data.
        /// </summary>
        private void PublishSkeletalData()
        {
            if (_renderer.IsReady) _renderer.DrawSkeletons(_skeletonData);
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            if (!_snsr.Status.HasFlag(ChooserStatus.SensorStarted))
            {
                throw new InvalidOperationException("Cannot convert a skeletal point to the screen as the kinect isn't ready.");
            }

            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = _snsr.Kinect.MapSkeletonPointToDepth(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        #endregion Private Methods

        #region Member Classes
        /**********************************************************************/
        /* Member Classes */
        /**********************************************************************/

        #endregion Member Classes
    }
}
