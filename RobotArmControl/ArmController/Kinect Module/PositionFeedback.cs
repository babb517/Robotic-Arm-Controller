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
        #region Constants
        /**********************************************************************/
        /* Constants */
        /**********************************************************************/
        /// <summary>
        /// Milliseconds to wait between position updates.
        /// </summary>
        private const long MIN_UPDATE_INTERVAL = 100;

        #endregion

        #region Private Members
        /**********************************************************************/
        /* Private Members */
        /**********************************************************************/

        /// <summary>
        /// The last time that a command was sent to the GUI arm.
        /// </summary>
        long _lastUpdateTime;

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

            Bus.Subscribe(BusNode.POSITION_TICK, OnValuePublished);
            _lastUpdateTime = 0;

        }

        protected override void OnFinalize()
        {
            
        }

        private void OnValuePublished(BusNode node, object value)
        {
            Integration.Orientation or;

            if (System.DateTime.Now.Ticks < _lastUpdateTime + MIN_UPDATE_INTERVAL * System.TimeSpan.TicksPerMillisecond) return;
            _lastUpdateTime = System.DateTime.Now.Ticks;

            or = Bus.Get<Integration.Orientation>(BusNode.ORIENTATION_RIGHT_UPPER_ARM);
            if (or != null)
            {
                ((MainWindow)(Application.Current.MainWindow)).txt_l_upper_pitch.Text = format(or.Pitch);
                ((MainWindow)(Application.Current.MainWindow)).txt_l_upper_roll.Text = format(or.Roll);
                ((MainWindow)(Application.Current.MainWindow)).txt_l_upper_yaw.Text = format(or.Yaw);
            }
            or = Bus.Get<Integration.Orientation>(BusNode.ABSOLUTE_ORIENTATION_RIGHT_UPPER_ARM);
            if (or != null)
            {
                ((MainWindow)(Application.Current.MainWindow)).txt_g_upper_pitch.Text = format(or.Pitch);
                ((MainWindow)(Application.Current.MainWindow)).txt_g_upper_roll.Text = format(or.Roll);
                ((MainWindow)(Application.Current.MainWindow)).txt_g_upper_yaw.Text = format(or.Yaw);
            }
            or = Bus.Get<Integration.Orientation>(BusNode.ORIENTATION_RIGHT_LOWER_ARM);
            if (or != null)
            {
                ((MainWindow)(Application.Current.MainWindow)).txt_l_lower_pitch.Text = format(or.Pitch);
                ((MainWindow)(Application.Current.MainWindow)).txt_l_lower_roll.Text = format(or.Roll);
                ((MainWindow)(Application.Current.MainWindow)).txt_l_lower_yaw.Text = format(or.Yaw);
            }
            or = Bus.Get<Integration.Orientation>(BusNode.ABSOLUTE_ORIENTATION_RIGHT_LOWER_ARM);
            if (or != null)
            {
                ((MainWindow)(Application.Current.MainWindow)).txt_g_lower_pitch.Text = format(or.Pitch);
                ((MainWindow)(Application.Current.MainWindow)).txt_g_lower_roll.Text = format(or.Roll);
                ((MainWindow)(Application.Current.MainWindow)).txt_g_lower_yaw.Text = format(or.Yaw);
            }
            or = Bus.Get<Integration.Orientation>(BusNode.ORIENTATION_RIGHT_HAND);
            if (or != null)
            {
                //_handDisplay.Text = OrientationToString(or);
                ((MainWindow)(Application.Current.MainWindow)).txt_l_hand_pitch.Text = format(or.Pitch);
                ((MainWindow)(Application.Current.MainWindow)).txt_l_hand_roll.Text = format(or.Roll);
                ((MainWindow)(Application.Current.MainWindow)).txt_l_hand_yaw.Text = format(or.Yaw);
            }
            or = Bus.Get<Integration.Orientation>(BusNode.ABSOLUTE_ORIENTATION_RIGHT_HAND);
            if (or != null)
            {
                //_handDisplay.Text = OrientationToString(or);
                ((MainWindow)(Application.Current.MainWindow)).txt_g_hand_pitch.Text = format(or.Pitch);
                ((MainWindow)(Application.Current.MainWindow)).txt_g_hand_roll.Text = format(or.Roll);
                ((MainWindow)(Application.Current.MainWindow)).txt_g_hand_yaw.Text = format(or.Yaw);
            }
            or = Bus.Get<Integration.Orientation>(BusNode.ORIENTATION_RIGHT_SHOULDER_RAW);
            if (or != null)
            {
                //_handDisplay.Text = OrientationToString(or);
                ((MainWindow)(Application.Current.MainWindow)).txt_raw_shoulder_pitch.Text = format(or.Pitch);
                ((MainWindow)(Application.Current.MainWindow)).txt_raw_shoulder_roll.Text = format(or.Roll);
                ((MainWindow)(Application.Current.MainWindow)).txt_raw_shoulder_yaw.Text = format(or.Yaw);
            }
            or = Bus.Get<Integration.Orientation>(BusNode.ORIENTATION_RIGHT_UPPER_ARM_RAW);
            if (or != null)
            {
                //_handDisplay.Text = OrientationToString(or);
                ((MainWindow)(Application.Current.MainWindow)).txt_raw_bicep_pitch.Text = format(or.Pitch);
                ((MainWindow)(Application.Current.MainWindow)).txt_raw_bicep_roll.Text = format(or.Roll);
                ((MainWindow)(Application.Current.MainWindow)).txt_raw_bicep_yaw.Text = format(or.Yaw);
            }
            or = Bus.Get<Integration.Orientation>(BusNode.ORIENTATION_RIGHT_LOWER_ARM_RAW);
            if (or != null)
            {
                //_handDisplay.Text = OrientationToString(or);
                ((MainWindow)(Application.Current.MainWindow)).txt_raw_forearm_pitch.Text = format(or.Pitch);
                ((MainWindow)(Application.Current.MainWindow)).txt_raw_forearm_roll.Text = format(or.Roll);
                ((MainWindow)(Application.Current.MainWindow)).txt_raw_forearm_yaw.Text = format(or.Yaw);
            }
        }


        private string format(float or)
        {
            return (or * (180 / Math.PI)).ToString("F2");
        }

        #endregion Event Handling
    }
}
