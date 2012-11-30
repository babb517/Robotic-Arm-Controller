using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Threading.Tasks;

using ArmController.Integration;

namespace ArmController.Kinect_Module
{
    class PositionFeedback : Module
    {
        #region Private Members
        /**********************************************************************/
        /* Private Members */
        /**********************************************************************/

        TextBox _upperArmDisplay;
        TextBox _lowerArmDisplay;
        TextBox _handDisplay;

        #endregion Private Members


        #region Constructors
        /**********************************************************************/
        /* Constructors */
        /**********************************************************************/
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
                _handDisplay.Text = OrientationToString(or);
            }
        }


        private string OrientationToString(Orientation or)
        {
            return (or.Roll * (180 / Math.PI)).ToString("F2") + " / " + (or.Pitch * (180 / Math.PI)).ToString("F2") + " / " + (or.Yaw * (180 / Math.PI)).ToString("F2");
        }

        #endregion Event Handling
    }
}
