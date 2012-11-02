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

namespace RobotArmControl.Kinect_Module
{
    class PositionalTracker
    {
        /**********************************************************************/
        /* Constants */
        /**********************************************************************/

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
            _snsr = new KinectSensorChooser(); 
            _renderer = new SkeletonRenderer(SkeletonPointToScreen, canvas);

            _skeletonReadyHandler = new EventHandler<SkeletonFrameReadyEventArgs>(OnKinectDataReady); 
        }

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


        /**********************************************************************/
        /* Private Methods */
        /**********************************************************************/

        /// <summary>
        /// Starts the kinect sensor and starts listening for the kinect publishing data.
        /// This method is taken almost directly from the MSDN example: http://msdn.microsoft.com/en-us/library/jj131025.aspx
        /// </summary>
        private void StartKinectST()
        {
            _snsr.Start();
        }

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
                e.NewSensor.SkeletonStream.Enable();                                                    // Enable skeletal tracking
                _skeletonData = new Skeleton[e.NewSensor.SkeletonStream.FrameSkeletonArrayLength];      // Allocate ST data
                e.NewSensor.SkeletonFrameReady += _skeletonReadyHandler;                                // Get Ready for Skeleton Ready Events        
                e.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;                  // Change the sensor mode to seated.
            }
        }

        /// <summary>
        /// Stops the kinect sensor.
        /// </summary>
        private void StopKinectST()
        {
            _snsr.Stop();
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

            ProcessSkeletalData();
            PublishSkeletalData();
        }

        /// <summary>
        /// Prepocesses the skeleton data giving us information like the appropriate joint angles.
        /// </summary>
        private void ProcessSkeletalData()
        {
            // TODO
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

        /**********************************************************************/
        /* Member Classes */
        /**********************************************************************/

    }
}
