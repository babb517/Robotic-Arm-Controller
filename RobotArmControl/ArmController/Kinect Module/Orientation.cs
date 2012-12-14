using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmController.Kinect_Module
{
    /// <summary>
    /// A simple structure used to describe the orientation of an object in 3d space.
    /// </summary>
    [Serializable]
    class Orientation
    {
        /// <summary>
        /// The roll of the orientation.
        /// </summary>
        public float Roll { get; set; }

        /// <summary>
        /// The pitch of the orientation.
        /// </summary>
        public float Pitch { get; set; }

        /// <summary>
        /// The yaw of the orientation.
        /// </summary>
        public float Yaw { get; set; }

        /// <summary>
        /// Construct a new orientation descriptor.
        /// </summary>
        /// <param name="roll">The roll of the orientation.</param>
        /// <param name="pitch">The pitch of the orientation.</param>
        /// <param name="yaw">The yaw of the orientation.</param>
        public Orientation(float roll, float pitch, float yaw)
        {
            Roll = roll;
            Pitch = pitch;
            Yaw = yaw;
        }
    }
}
