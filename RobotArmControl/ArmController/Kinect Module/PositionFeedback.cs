using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Threading.Tasks;

using ArmController.Integration;

namespace ArmController.Kinect_Module
{
    /// <summary>
    /// This module is used for debugging purposes. It listens for the orientations of the user's right arm on the virtual bus and displays them on screen.
    /// </summary>
    class PositionFeedback : Module
    {
        #region Private Members
        /**********************************************************************/
        /* Private Members */
        /**********************************************************************/

        /// <summary>
        /// A reference to the text box used to display the upper arm orientation.
        /// </summary>
        TextBox _upperArmDisplay;

        /// <summary>
        /// A reference to the text box used to display lower arm orientation.
        /// </summary>
        TextBox _lowerArmDisplay;

        /// <summary>
        /// A reference to the text box used to display hand orientation.
        /// </summary>
        TextBox _handDisplay;

        #endregion Private Members


        #region Constructors
        /**********************************************************************/
        /* Constructors */
        /**********************************************************************/
        /// <summary>
        /// Initializes the position feedback module to output the provided text boxes.
        /// </summary>
        /// <param name="upperArmDisplay">The text box to display upper arm orientation.</param>
        /// <param name="lowerArmDisplay">The text box to display lower arm orientation.</param>
        /// <param name="handDisplay">The text box to display hand orientation.</param>
        public PositionFeedback(TextBox upperArmDisplay, TextBox lowerArmDisplay, TextBox handDisplay)
        {
            _upperArmDisplay = upperArmDisplay;
            _lowerArmDisplay = lowerArmDisplay;
            _handDisplay = handDisplay;
        }

        #endregion Constructors

        #region Event Handling

        /**********************************************************************/
        /* Event Handling */
        /**********************************************************************/

        protected override void OnInitialize()
        {
            _upperArmDisplay.Text = "No Data";
            _lowerArmDisplay.Text = "No Data";
            _handDisplay.Text = "No Data";

            Bus.Subscribe(BusNode.ORIENTATION_RIGHT_UPPER_ARM, OnValuePublished);
            Bus.Subscribe(BusNode.ORIENTATION_RIGHT_LOWER_ARM, OnValuePublished);
            Bus.Subscribe(BusNode.ORIENTATION_RIGHT_HAND, OnValuePublished);
        }

        protected override void OnFinalize()
        {
            
        }

        /// <summary>
        /// Called when a value is published to the virtual bus.
        /// </summary>
        /// <param name="node">The node that has been published.</param>
        /// <param name="value">The new value of the node.</param>
        private void OnValuePublished(BusNode node, object value)
        {
            Orientation or = (Orientation)value;

            if (node == BusNode.ORIENTATION_RIGHT_UPPER_ARM)
            {
                _upperArmDisplay.Text = OrientationToString(or);
            }
            else if (node == BusNode.ORIENTATION_RIGHT_LOWER_ARM)
            {
                _lowerArmDisplay.Text = OrientationToString(or);
            }
            else if (node == BusNode.ORIENTATION_RIGHT_HAND)
            {
                //_handDisplay.Text = OrientationToString(or);
                _handDisplay.Text = OrientationToString(Bus.Get<Orientation>(BusNode.ORIENTATION_RIGHT_HAND));
            }
        }

        /// <summary>
        /// Constructs a string describing the provided orientation.
        /// </summary>
        /// <param name="or">The orientation to construct the string from.</param>
        /// <returns>A string of the form 'ROLL / PITCH / YAW'.</returns>
        private string OrientationToString(Orientation or)
        {
            return (or.Roll * (180 / Math.PI)).ToString("F2") + " / " + (or.Pitch * (180 / Math.PI)).ToString("F2") + " / " + (or.Yaw * (180 / Math.PI)).ToString("F2");
        }

        #endregion Event Handling
    }
}
