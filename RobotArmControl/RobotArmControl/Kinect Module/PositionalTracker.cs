using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

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
        private KinectSensor _snsr;

        /// <summary>
        /// The skeleton data from the kinect.
        /// </summary>
        private Skeleton[] _skeletonData;

        /// <summary>
        /// The rendering object used to display rendering output
        /// </summary>
        private SkeletonRenderer _renderer;

        /// <summary>
        /// A flag to determine if we want to output debugging data.
        /// </summary>
        private bool Debug { get; set; }

        /// <summary>
        /// A virtual member used to access the canvas we're drawing on.
        /// </summary>
        private DrawingContext Canvas {
            get {
                return _renderer.Canvas;
            }

            set
            {
                _renderer.Canvas = value;
            }
        }


        /**********************************************************************/
        /* Constructors */
        /**********************************************************************/

        /// <summary>
        /// Initializes the tracker object.
        /// </summary>
        /// <param name="canvas">The canvas to draw vision data onto if applicable, null otherwise.</param>
        /// <param name="debug">Whether to output debugging data or not.</param>
        public PositionalTracker(DrawingContext canvas = null, bool debug = false)
        {
            _renderer = new SkeletonRenderer(canvas);
            Debug = debug;
        }

        /**********************************************************************/
        /* Public Methods */
        /**********************************************************************/

        public void Start() {
            StartKinectST();
        }

        public void Stop() {
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
            _snsr = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected);             // Get first Kinect Sensor
            _snsr.SkeletonStream.Enable();                                                                          // Enable skeletal tracking

            _skeletonData = new Skeleton[_snsr.SkeletonStream.FrameSkeletonArrayLength];                             // Allocate ST data

            _snsr.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(OnKinectDataReady);                        // Get Ready for Skeleton Ready Events

            _snsr.Start(); // Start Kinect sensor
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
            if (_renderer) _renderer.DrawSkeletons(_skeletonData);
            //Debug
            // TODO
        }



        /**********************************************************************/
        /* Member Classes */
        /**********************************************************************/

        /// <summary>
        /// This class provides us with a means to render a skeleton on a screen.
        /// </summary>
        public class SkeletonRenderer
        {
            /**********************************************************************/
            /* Private Members */
            /**********************************************************************/
            /// <summary>
            /// The canvas to render the skeleton on.
            /// </summary>
            private DrawingContext _context;


            /**********************************************************************/
            /* Public Methods */
            /**********************************************************************/


            /**********************************************************************/
            /* Public Methods */
            /**********************************************************************/

            /// <summary>
            /// Draws all the skeletons we're tracking on a canvas.
            /// </summary>
            public void DrawSkeletons(Skeleton[] data)
            {
                foreach (Skeleton skeleton in data)
                {
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        DrawTrackedSkeletonJoints(skeleton.Joints);
                        RenderClippedEdges(skeleton);
                    }
                    else if (skeleton.TrackingState == SkeletonTrackingState.PositionOnly)
                    {
                        DrawSkeletonPosition(skeleton.Position);
                    }
                }
            }

            /**********************************************************************/
            /* Private Methods */
            /**********************************************************************/


            private void DrawTrackedSkeletonJoints(JointCollection jointCollection)
            {
                // Render Head and Shoulders
                DrawBone(jointCollection[JointType.Head], jointCollection[JointType.ShoulderCenter]);
                DrawBone(jointCollection[JointType.ShoulderCenter], jointCollection[JointType.ShoulderLeft]);
                DrawBone(jointCollection[JointType.ShoulderCenter], jointCollection[JointType.ShoulderRight]);

                // Render Left Arm
                DrawBone(jointCollection[JointType.ShoulderLeft], jointCollection[JointType.ElbowLeft]);
                DrawBone(jointCollection[JointType.ElbowLeft], jointCollection[JointType.WristLeft]);
                DrawBone(jointCollection[JointType.WristLeft], jointCollection[JointType.HandLeft]);

                // Render Right Arm
                DrawBone(jointCollection[JointType.ShoulderRight], jointCollection[JointType.ElbowRight]);
                DrawBone(jointCollection[JointType.ElbowRight], jointCollection[JointType.WristRight]);
                DrawBone(jointCollection[JointType.WristRight], jointCollection[JointType.HandRight]);

            }

            private void DrawBone(Joint jointFrom, Joint jointTo)
            {

                if (jointFrom.TrackingState == JointTrackingState.NotTracked ||
                jointTo.TrackingState == JointTrackingState.NotTracked)
                {
                    return; // nothing to draw, one of the joints is not tracked
                }

                if (jointFrom.TrackingState == JointTrackingState.Inferred ||
                jointTo.TrackingState == JointTrackingState.Inferred)
                {
                    DrawNonTrackedBoneLine(jointFrom.Position, jointTo.Position);  // Draw thin lines if either one of the joints is inferred
                }

                if (jointFrom.TrackingState == JointTrackingState.Tracked &&
                jointTo.TrackingState == JointTrackingState.Tracked)
                {
                    DrawTrackedBoneLine(jointFrom.Position, jointTo.Position);  // Draw bold lines if the joints are both tracked
                }
            }
        }


        private void DrawTrackedBoneLine(SkeletonPoint from, SkeletonPoint to) 
        {
            // TODO
        }

        private void DrawNonTrackedBoneLine(SkeletonPoint from, SkeletonPoint to)
        {
            // TODO
        }


    }
}
