using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Kinect;

using ArmController.Kinect_Module;

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

        public static readonly BusNode PLAYER_ONE_ID = new BusNode(typeof(int));                            ///< (Integer) ID for player one according to the Kinect

        #region Left Arm
        public static readonly BusNode POS_LEFT_SHOULDER = new BusNode(typeof(SkeletonPoint));          ///< (SkeletonPoint) the 3d point of the left shoulder in space.
        public static readonly BusNode POS_LEFT_ELBOW = new BusNode(typeof(SkeletonPoint));             ///< (SkeletonPoint) the 3d point of the left elbow in space.
        public static readonly BusNode POS_LEFT_WRIST = new BusNode(typeof(SkeletonPoint));             ///< (SkeletonPoint) the 3d point of the left wrist in space.
        public static readonly BusNode POS_LEFT_HAND = new BusNode(typeof(SkeletonPoint));              ///< (SkeletonPoint) the 3d point of the left hand in space.

        public static readonly BusNode DIR_LEFT_UPPER_ARM = new BusNode(typeof(SkeletonPoint));         ///< (SkeletonPoint) A unit vector providing the direction the left upper arm is pointing relative to the left collar bone.
        public static readonly BusNode ORIENTATION_LEFT_UPPER_ARM = new BusNode(typeof(Orientation));    ///< (Orientation) The orientation of the left upper-arm relative to the left collar bone.

        public static readonly BusNode DIR_LEFT_LOWER_ARM = new BusNode(typeof(SkeletonPoint));         ///< (SkeletonPoint) A unit vector providing the direction the left lower arm is pointing relative to the left upper arm.
        public static readonly BusNode ORIENTATION_LEFT_LOWER_ARM = new BusNode(typeof(Orientation));       ///< (Orientation) The orientation of the left lower-arm relative to the left upper-arm.

        public static readonly BusNode DIR_LEFT_HAND = new BusNode(typeof(SkeletonPoint));              ///< (SkeletonPoint) A unit vector providing the direction the left hand is pointing relative to the left lower arm.
        public static readonly BusNode ORIENTATION_LEFT_HAND = new BusNode(typeof(Orientation));       ///< (Orientation) The orientation of the left hand relative to the left lower-arm.

        #endregion Left Arm

        #region Right Arm
        public static readonly BusNode POS_RIGHT_SHOULDER = new BusNode(typeof(SkeletonPoint));         ///< (SkeletonPoint) giving the 3d point of the right shoulder in space.
        public static readonly BusNode POS_RIGHT_ELBOW = new BusNode(typeof(SkeletonPoint));            ///< (SkeletonPoint) giving the 3d point of the right elbow in space.
        public static readonly BusNode POS_RIGHT_WRIST = new BusNode(typeof(SkeletonPoint));            ///< (SkeletonPoint) giving the 3d point of the right wrist in space.
        public static readonly BusNode POS_RIGHT_HAND = new BusNode(typeof(SkeletonPoint));             ///< (SkeletonPoint) giving the 3d point of the right hand in space.

        public static readonly BusNode DIR_RIGHT_UPPER_ARM = new BusNode(typeof(SkeletonPoint));        ///< (SkeletonPoint) A unit vector providing the direction the right upper arm is pointing relative to the right collar bone.
        public static readonly BusNode ORIENTATION_RIGHT_UPPER_ARM = new BusNode(typeof(Orientation));   ///< (Orientation) The orientation of the right upper-arm relative to the right collar bone.

        public static readonly BusNode DIR_RIGHT_LOWER_ARM = new BusNode(typeof(SkeletonPoint));        ///< (SkeletonPoint) A unit vector providing the direction the right lower arm is pointing relative to the right upper arm.
        public static readonly BusNode ORIENTATION_RIGHT_LOWER_ARM = new BusNode(typeof(Orientation));      ///< (Orientation) The orientation of the right lower-arm relative to the right upper-arm.

        public static readonly BusNode DIR_RIGHT_HAND = new BusNode(typeof(SkeletonPoint));             ///< (SkeletonPoint) A unit vector providing the direction the right hand is pointing relative to the right lower arm.
        public static readonly BusNode ORIENTATION_RIGHT_HAND = new BusNode(typeof(Orientation));      ///< (Orientation) The orientation of the right hand relative to the right lower-arm.
        #endregion Right Arms

        #endregion Position Module Output

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
