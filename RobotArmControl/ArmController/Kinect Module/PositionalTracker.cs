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

using ArmController.Integration;

namespace ArmController.Kinect_Module
{
    class PositionalTracker : Module
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
        private Skeleton _playerOne;

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

        #region Event Handling
        /**********************************************************************/
        /* Event Handling */
        /**********************************************************************/

        protected override void OnInitialize()
        {
            Debug.WriteLine("Starting the kinect...");
            StartKinectST();
        }

        protected override void OnFinalize()
        {
            Debug.WriteLine("Stopping the kinect...");
            StopKinectST();
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
            //Debug.WriteLine("Kinect Data is ready, processing it...");

            // unpackage the skeletal data
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null && this._skeletonData != null)         // check that a frame is available
                {
                    skeletonFrame.CopySkeletonDataTo(this._skeletonData);        // get the skeletal information in this frame
                }
            }

            ProcessSkeletalData();
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
        /// <returns>True if a new player has been selected, false otherwise.</returns>
        private bool SelectPlayerOne()
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
            if (!foundPreviousPlayerOne)
            {
                _playerOne = closest;
                return true;
            }
            else return false;

        }

        /// <summary>
        /// Prepocesses the skeleton data giving us information like the appropriate joint angles.
        /// </summary>
        private void ProcessSkeletalData()
        {
            if (SelectPlayerOne() && _playerOne != null ) { Bus.Publish(BusNode.PLAYER_ONE_ID, (_playerOne != null) ? _playerOne.TrackingId : -1); }

            if (_playerOne == null || _playerOne.TrackingState != SkeletonTrackingState.Tracked)
            {
                Debug.WriteLine("Player not found...");
            } 
            else
            {
                // there is an active player 1, let's publish his information...

                // positions
                Bus.Publish(BusNode.POS_LEFT_SHOULDER, _playerOne.Joints[JointType.ShoulderLeft].Position);
                Bus.Publish(BusNode.POS_LEFT_ELBOW, _playerOne.Joints[JointType.ElbowLeft].Position);
                Bus.Publish(BusNode.POS_LEFT_WRIST, _playerOne.Joints[JointType.WristLeft].Position);
                Bus.Publish(BusNode.POS_LEFT_HAND, _playerOne.Joints[JointType.HandLeft].Position);

                Bus.Publish(BusNode.POS_RIGHT_SHOULDER, _playerOne.Joints[JointType.ShoulderRight].Position);
                Bus.Publish(BusNode.POS_RIGHT_ELBOW, _playerOne.Joints[JointType.ElbowRight].Position);
                Bus.Publish(BusNode.POS_RIGHT_WRIST, _playerOne.Joints[JointType.WristRight].Position);
                Bus.Publish(BusNode.POS_RIGHT_HAND, _playerOne.Joints[JointType.HandRight].Position);

                // directions
                Bus.Publish(BusNode.DIR_LEFT_UPPER_ARM, GetUnitDirection(JointType.ElbowLeft));
                Bus.Publish(BusNode.DIR_LEFT_LOWER_ARM, GetUnitDirection(JointType.WristLeft));
                Bus.Publish(BusNode.DIR_LEFT_HAND, GetUnitDirection(JointType.HandLeft));

                Bus.Publish(BusNode.DIR_RIGHT_UPPER_ARM, GetUnitDirection(JointType.ElbowRight));
                Bus.Publish(BusNode.DIR_RIGHT_LOWER_ARM, GetUnitDirection(JointType.WristRight));
                Bus.Publish(BusNode.DIR_RIGHT_HAND, GetUnitDirection(JointType.HandRight));

                // orientations
                Bus.Publish(BusNode.ORIENTATION_LEFT_UPPER_ARM, GetOrientation(JointType.ElbowLeft));
                Bus.Publish(BusNode.ORIENTATION_LEFT_LOWER_ARM, GetOrientation(JointType.WristLeft));
                Bus.Publish(BusNode.ORIENTATION_LEFT_HAND, GetOrientation(JointType.HandLeft));

                Bus.Publish(BusNode.ORIENTATION_RIGHT_UPPER_ARM, GetOrientation(JointType.ElbowRight));
                Bus.Publish(BusNode.ORIENTATION_RIGHT_LOWER_ARM, GetOrientation(JointType.WristRight));
                Bus.Publish(BusNode.ORIENTATION_RIGHT_HAND, GetOrientation(JointType.HandRight));

            }  

            // render the skeleton.
            if (_renderer.IsReady) _renderer.DrawSkeletons(_skeletonData);

            // trigger a tick.
            Bus.Publish<object>(BusNode.POSITION_TICK, null);
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

        /// <summary>
        /// Gets the (relative) direction of the bone identified by the provided terminal joint.
        /// </summary>
        /// <param name="terminus">The joint at which the bone terminates.</param>
        /// <returns>A 3d point representing a unit vector in the relative direction of the bone.</returns>
        private SkeletonPoint GetUnitDirection(JointType terminus)
        {
            SkeletonPoint res = new SkeletonPoint();
            Matrix4 r = _playerOne.BoneOrientations[terminus].HierarchicalRotation.Matrix;

            float inv_mag = 1 / (float)Math.Sqrt(r.M21 * r.M21 + r.M22 * r.M22 + r.M23 * r.M23);

            res.X = r.M21 * inv_mag;
            res.Y = r.M22 * inv_mag;
            res.Z = r.M23 * inv_mag;

            return res;
        }

        /// <summary>
        /// Gets the (relative) orientation of the bone identified by the provided terminal joint.
        /// </summary>
        /// <param name="terminus">The joint at which the bone terminates.</param>
        /// <returns>A 3d point representing a unit vector in the relative direction of the bone.</returns>
        private Orientation GetOrientation(JointType terminus)
        {
            
            /**
             * NOTE: The bones are indexed by their terminal joint.
             * http://msdn.microsoft.com/en-us/library/hh973073.aspx
             * The Z axis extends directly in front of the player.
             * The elbow is restricted to a postive yawing motion.
             * The shoulder can move in all directions.   
             */

            //Debug.WriteLine(_playerOne.BoneOrientations[JointType.WristRight].HierarchicalRotation.Matrix.M11.ToString() + "\t\t" + _playerOne.BoneOrientations[JointType.WristRight].HierarchicalRotation.Matrix.M21.ToString() + "\t\t" + _playerOne.BoneOrientations[JointType.WristRight].HierarchicalRotation.Matrix.M31.ToString());
            //Debug.WriteLine(_playerOne.BoneOrientations[JointType.WristRight].HierarchicalRotation.Matrix.M12.ToString() + "\t\t" + _playerOne.BoneOrientations[JointType.WristRight].HierarchicalRotation.Matrix.M22.ToString() + "\t\t" + _playerOne.BoneOrientations[JointType.WristRight].HierarchicalRotation.Matrix.M32.ToString());
            //Debug.WriteLine(_playerOne.BoneOrientations[JointType.WristRight].HierarchicalRotation.Matrix.M13.ToString() + "\t\t" + _playerOne.BoneOrientations[JointType.WristRight].HierarchicalRotation.Matrix.M23.ToString() + "\t\t" + _playerOne.BoneOrientations[JointType.WristRight].HierarchicalRotation.Matrix.M33.ToString());
            //Debug.WriteLine("");

            Matrix4 r = _playerOne.BoneOrientations[terminus].HierarchicalRotation.Matrix;

            float roll = (float)Math.Asin(r.M13);
            float inv_cos_roll = 1 / (float)Math.Cos(roll);
            float pitch = (float)Math.Asin(r.M23 * inv_cos_roll);
            float yaw = (float)Math.Asin(r.M12 * inv_cos_roll);

            //Debug.WriteLine("Roll: " + roll * 180 / Math.PI);
            //Debug.WriteLine("Pitch: " + pitch * 180 / Math.PI);
            //Debug.WriteLine("Yaw: " + yaw * 180 / Math.PI);
            //Debug.WriteLine("");

            return new Orientation(roll, pitch, yaw);
        }

        #endregion Private Methods

        #region Member Classes
        /**********************************************************************/
        /* Member Classes */
        /**********************************************************************/

        #endregion Member Classes
    }
}
