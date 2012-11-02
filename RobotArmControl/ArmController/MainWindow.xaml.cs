using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using RobotArmControl.Kinect_Module;

namespace ArmController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /**************************************************************************/
        /* Private Members */
        /**************************************************************************/

        /// <summary>
        /// A drawing group used to render the kinect feedback image.
        /// </summary>
        DrawingGroup _kinectDrawingGroup;

        PositionalTracker _kinectModule;

        /**************************************************************************/
        /* Constructors */
        /**************************************************************************/
        public MainWindow()
        {
            InitializeComponent();
        }

        /**************************************************************************/
        /* Event Handling */
        /**************************************************************************/

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // setup the kinect feedback
            _kinectDrawingGroup = new DrawingGroup();
            KinectRenderFeedback.Source = new DrawingImage(_kinectDrawingGroup);

            // setup kinect module
            _kinectModule = new PositionalTracker(_kinectDrawingGroup);
            _kinectModule.Start();

        }


        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // stop the kinect
            _kinectModule.Stop();
        }
    }

    
}
