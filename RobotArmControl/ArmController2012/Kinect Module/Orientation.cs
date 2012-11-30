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
        public float Roll { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }

        public Orientation(float roll, float pitch, float yaw)
        {
            Roll = roll;
            Pitch = pitch;
            Yaw = yaw;
        }
    }
}
