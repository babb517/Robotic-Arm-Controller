using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows;

using ArmController.Integration;

namespace ArmController.Kinect_Module
{
    class PositionFeedback : Module
    {
        #region Private Members
        /**********************************************************************/
        /* Private Members */
        /**********************************************************************/

        #endregion Private Members


        #region Constructors
        /**********************************************************************/
        /* Constructors */
        /**********************************************************************/
        public PositionFeedback()
        {

        }

        #endregion Constructors

        #region Event Handling

        /**********************************************************************/
        /* Event Handling */
        /**********************************************************************/

        protected override void OnInitialize()
        {

            Bus.Subscribe(BusNode.ORIENTATION_RIGHT_UPPER_ARM, OnValuePublished);
            Bus.Subscribe(BusNode.ABSOLUTE_ORIENTATION_RIGHT_UPPER_ARM, OnValuePublished);
            Bus.Subscribe(BusNode.ORIENTATION_RIGHT_LOWER_ARM, OnValuePublished);
            Bus.Subscribe(BusNode.ABSOLUTE_ORIENTATION_RIGHT_LOWER_ARM, OnValuePublished);
            Bus.Subscribe(BusNode.ORIENTATION_RIGHT_HAND, OnValuePublished);
            Bus.Subscribe(BusNode.ABSOLUTE_ORIENTATION_RIGHT_HAND, OnValuePublished);

        }

        protected override void OnFinalize()
        {
            
        }

        private void OnValuePublished(BusNode node, object value)
        {
            Integration.Orientation or = (Integration.Orientation)value;

            if (node == BusNode.ORIENTATION_RIGHT_UPPER_ARM)
            {
                ((MainWindow)(Application.Current.MainWindow)).txt_l_upper_pitch.Text = format(or.Pitch);
                ((MainWindow)(Application.Current.MainWindow)).txt_l_upper_roll.Text = format(or.Roll);
                ((MainWindow)(Application.Current.MainWindow)).txt_l_upper_yaw.Text = format(or.Yaw);
            }
            else if (node == BusNode.ABSOLUTE_ORIENTATION_RIGHT_UPPER_ARM)
            {
                ((MainWindow)(Application.Current.MainWindow)).txt_g_upper_pitch.Text = format(or.Pitch);
                ((MainWindow)(Application.Current.MainWindow)).txt_g_upper_roll.Text = format(or.Roll);
                ((MainWindow)(Application.Current.MainWindow)).txt_g_upper_yaw.Text = format(or.Yaw);
            }
            else if (node == BusNode.ORIENTATION_RIGHT_LOWER_ARM)
            {
                ((MainWindow)(Application.Current.MainWindow)).txt_l_lower_pitch.Text = format(or.Pitch);
                ((MainWindow)(Application.Current.MainWindow)).txt_l_lower_roll.Text = format(or.Roll);
                ((MainWindow)(Application.Current.MainWindow)).txt_l_lower_yaw.Text = format(or.Yaw);
            }
            else if (node == BusNode.ABSOLUTE_ORIENTATION_RIGHT_LOWER_ARM)
            {
                ((MainWindow)(Application.Current.MainWindow)).txt_g_lower_pitch.Text = format(or.Pitch);
                ((MainWindow)(Application.Current.MainWindow)).txt_g_lower_roll.Text = format(or.Roll);
                ((MainWindow)(Application.Current.MainWindow)).txt_g_lower_yaw.Text = format(or.Yaw);
            }
            else if (node == BusNode.ORIENTATION_RIGHT_HAND)
            {
                //_handDisplay.Text = OrientationToString(or);
                ((MainWindow)(Application.Current.MainWindow)).txt_l_hand_pitch.Text = format(or.Pitch);
                ((MainWindow)(Application.Current.MainWindow)).txt_l_hand_roll.Text = format(or.Roll);
                ((MainWindow)(Application.Current.MainWindow)).txt_l_hand_yaw.Text = format(or.Yaw);
            }
            else if (node == BusNode.ABSOLUTE_ORIENTATION_RIGHT_HAND)
            {
                //_handDisplay.Text = OrientationToString(or);
                ((MainWindow)(Application.Current.MainWindow)).txt_g_hand_pitch.Text = format(or.Pitch);
                ((MainWindow)(Application.Current.MainWindow)).txt_g_hand_roll.Text = format(or.Roll);
                ((MainWindow)(Application.Current.MainWindow)).txt_g_hand_yaw.Text = format(or.Yaw);
            }
        }


        private string format(float or)
        {
            return (or * (180 / Math.PI)).ToString("F2");
        }

        #endregion Event Handling
    }
}
