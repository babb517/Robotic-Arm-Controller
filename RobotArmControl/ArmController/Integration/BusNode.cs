using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Kinect;

namespace ArmController.Integration
{
        /// <summary>
    /// <summary>
    /// This enumeration stores known data points which may be queried
    /// using the virtual bus.
    /// </summary>
    public class BusNode
    {
        #region Private Members
        /************************************************************/
        /* Private Members */
        /************************************************************/

        /// <summary>
        /// The type of the node.
        /// </summary>
        public Type NodeType { get; private set; }

        #endregion Private Members

        #region Nodes
        /************************************************************/
        /* Nodes */
        /************************************************************/

        #region Program Control
        // Program Control
        public static readonly BusNode STOP_REQUESTED = new BusNode(typeof(Boolean));                       ///< (Boolean) A flag used to request that the program controller ends program execution.
        #endregion Program Control

        #region Position Module Output
        // Output from the Position Detection Module (Kinect)
        public static readonly BusNode POSITION_TICK = new BusNode(typeof(Object));                         ///< (Object) A value that's published once every output from the position detection module has been published. Subscribe to this to avoid getting notified when each value has been changed.

        #region Right Arm
        public static readonly BusNode DIR_RIGHT_UPPER_ARM = new BusNode(typeof(SkeletonPoint));        ///< (SkeletonPoint) A unit vector providing the direction the right upper arm is pointing relative to the right collar bone.
        public static readonly BusNode ORIENTATION_RIGHT_UPPER_ARM = new BusNode(typeof(Orientation));   ///< (Orientation) The orientation of the right upper-arm relative to the right collar bone.
        public static readonly BusNode ABSOLUTE_ORIENTATION_RIGHT_UPPER_ARM = new BusNode(typeof(Orientation));   ///< (Orientation) The absolute orientation of the right upper-arm.

        public static readonly BusNode DIR_RIGHT_LOWER_ARM = new BusNode(typeof(SkeletonPoint));        ///< (SkeletonPoint) A unit vector providing the direction the right lower arm is pointing relative to the right upper arm.
        public static readonly BusNode ORIENTATION_RIGHT_LOWER_ARM = new BusNode(typeof(Orientation));      ///< (Orientation) The orientation of the right lower-arm relative to the right upper-arm.
        public static readonly BusNode ABSOLUTE_ORIENTATION_RIGHT_LOWER_ARM = new BusNode(typeof(Orientation));      ///< (Orientation) The absolute orientation of the right lower arm.

        public static readonly BusNode DIR_RIGHT_HAND = new BusNode(typeof(SkeletonPoint));             ///< (SkeletonPoint) A unit vector providing the direction the right hand is pointing relative to the right lower arm.
        public static readonly BusNode ORIENTATION_RIGHT_HAND = new BusNode(typeof(Orientation));      ///< (Orientation) The orientation of the right hand relative to the right lower-arm.
        public static readonly BusNode ABSOLUTE_ORIENTATION_RIGHT_HAND = new BusNode(typeof(Orientation));      ///< (Orientation) The absolute orientation of the right hand.

        
        #endregion Right Arms

        #endregion Position Module Output

        #region CyberGlove Nodes

        public static readonly BusNode CLAW_OPEN_PERCENT = new BusNode(typeof(int));
        public static readonly BusNode WRIST_PERCENT = new BusNode(typeof(int));
        public static readonly BusNode ROBOT_ACTIVE = new BusNode(typeof(bool));
        public static readonly BusNode ROBOT_RESET = new BusNode(typeof(object));

        #endregion CyberGlove Nodes

        #region GUI Nodes

        #region CyberGlove Configuration
        public static readonly BusNode CYBERGLOVE_COM_PORT = new BusNode(typeof(String));
        public static readonly BusNode CYBERGLOVE_BAUD_RATE = new BusNode(typeof(int));
        public static readonly BusNode CYBERGLOVE_ENABLE = new BusNode(typeof(bool));
        #endregion CyberGlove Configuration

        #region IMU Configuration

        public static readonly BusNode IMU_COM_PORT = new BusNode(typeof(String));
        public static readonly BusNode IMU_BAUD_RATE = new BusNode(typeof(int));
        public static readonly BusNode IMU_ENABLE = new BusNode(typeof(bool));

        #endregion IMU Configuration

        #region Kinect Configuration

        public static readonly BusNode KINECT_ENABLE = new BusNode(typeof(bool));

        #endregion Kinect Configuration

        #region Robot Arm Configuration

        public static readonly BusNode ROBOT_ARM_COM_PORT = new BusNode(typeof(String));
        public static readonly BusNode ROBOT_ARM_BAUD_RATE = new BusNode(typeof(int));
        public static readonly BusNode ROBOT_ARM_ENABLE = new BusNode(typeof(bool));

        #endregion Robot Arm Configuration

        #region Servos

        public static readonly BusNode SHOULDER_SERVO_MIN_RANGE = new BusNode(typeof(int));
        public static readonly BusNode SHOULDER_SERVO_MAX_RANGE = new BusNode(typeof(int));
        public static readonly BusNode SHOULDER_SERVO_SPEED = new BusNode(typeof(int));
        public static readonly BusNode SHOULDER_SERVO_ENABLE = new BusNode(typeof(bool));

        public static readonly BusNode ARM_SERVO_MIN_RANGE = new BusNode(typeof(int));
        public static readonly BusNode ARM_SERVO_MAX_RANGE = new BusNode(typeof(int));
        public static readonly BusNode ARM_SERVO_SPEED = new BusNode(typeof(int));
        public static readonly BusNode ARM_SERVO_ENABLE = new BusNode(typeof(bool));

        public static readonly BusNode FOREARM_SERVO_MIN_RANGE = new BusNode(typeof(int));
        public static readonly BusNode FOREARM_SERVO_MAX_RANGE = new BusNode(typeof(int));
        public static readonly BusNode FOREARM_SERVO_SPEED = new BusNode(typeof(int));
        public static readonly BusNode FOREARM_SERVO_ENABLE = new BusNode(typeof(bool));

        public static readonly BusNode WRIST_ROTATE_SERVO_MIN_RANGE = new BusNode(typeof(int));
        public static readonly BusNode WRIST_ROTATE_SERVO_MAX_RANGE = new BusNode(typeof(int));
        public static readonly BusNode WRIST_ROTATE_SERVO_SPEED = new BusNode(typeof(int));
        public static readonly BusNode WRIST_ROTATE_SERVO_ENABLE = new BusNode(typeof(bool));

        public static readonly BusNode WRIST_SERVO_MIN_RANGE = new BusNode(typeof(int));
        public static readonly BusNode WRIST_SERVO_MAX_RANGE = new BusNode(typeof(int));
        public static readonly BusNode WRIST_SERVO_SPEED = new BusNode(typeof(int));
        public static readonly BusNode WRIST_SERVO_ENABLE = new BusNode(typeof(bool));

        public static readonly BusNode HAND_SERVO_MIN_RANGE = new BusNode(typeof(int));
        public static readonly BusNode HAND_SERVO_MAX_RANGE = new BusNode(typeof(int));
        public static readonly BusNode HAND_SERVO_SPEED = new BusNode(typeof(int));
        public static readonly BusNode HAND_SERVO_ENABLE = new BusNode(typeof(bool));

        #endregion Servos

        #endregion GUI Nodes

        #region Kinect Nodes

        public static readonly BusNode KINECT_DEBUG_OUTPUT = new BusNode(typeof(String));
        
        #endregion Kinect Nodes

        // TODO: Populate the nodes with appropriate values.


        #endregion Nodes

        #region Constructors
        /************************************************************/
        /* Constructors */
        /************************************************************/

        /// <summary>
        /// Instantiates of a descriptor for a bus node.
        /// </summary>
        /// <param name="nodeType">The type of the node.</param>
        public BusNode(Type nodeType) {
            NodeType = nodeType;
        }

        #endregion Constructors

        #region Public Members
        /************************************************************/
        /* Public Members */
        /************************************************************/

        public static IEnumerable<BusNode> Values
        {
            get
            {
                Debug.Write("Getting Bus Nodes...\n");
                FieldInfo[] members = typeof(BusNode).GetFields();

                foreach (FieldInfo member in members)
                {
                    if (member.GetValue(null).GetType() == typeof(BusNode))
                    {
                        yield return (BusNode)member.GetValue(null);
                    }
                }
            }
        }

        #endregion Public Members
    }
}
