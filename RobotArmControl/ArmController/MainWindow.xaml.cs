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
using System.Diagnostics;

using ArmController.Kinect_Module;
using ArmController.Integration;
using ArmController.Robot_Arm_Module;
using ArmController.CyberGloveLibrary;

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
        /// The modules which are being managed by the application.
        /// </summary>
        List<Module> _modules;

        /// <summary>
        /// The bus used to communicate between modules.
        /// </summary>
        VirtualBus _bus;


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
            // setup the virtual bus.
            _bus = new VirtualBus(Dispatcher);
            _bus.Subscribe(BusNode.STOP_REQUESTED, OnBusValueChanged);


            // setup the image
            DrawingGroup drawingGroup = new DrawingGroup();
            KinectRenderFeedback.Source = new DrawingImage(drawingGroup);

            // setup each of the modules.
            _modules = new List<Module>();

            // TODO: Add each module to the list here.
           // _modules.Add(new PositionalTracker(drawingGroup));
            _modules.Add(new PositionFeedback(this.UpperArmOrientation, this.LowerArmOrientaiton, this.HandOrientation));
            _modules.Add(new RobotArmModule());
            _modules.Add(new GloveModule());
            
            // start everything!
            InitializeModules();
        }

        /// <summary>
        /// Finalizes the states of all modules in preparation for the program exiting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // stop the kinect
            FinalizeModules();
        }

        /// <summary>
        /// Handles bus
        /// </summary>
        /// <param name="node"></param>
        /// <param name="value"></param>
        private void OnBusValueChanged(BusNode node, Object value)
        {
            if (node == BusNode.STOP_REQUESTED)
            {
                Debug.WriteLine("Caught a program stop request");
                this.Close();
            }
        }

        /**************************************************************************/
        /* Private Members */
        /**************************************************************************/

        /// <summary>
        /// Initializes the states of all the modules.
        /// </summary>
        private void InitializeModules()
        {
            foreach (Module module in _modules)
            {
                module.InitializeModule(_bus);
            }
        }

        /// <summary>
        /// Requests that all modules finalize their states in preparation to close.
        /// </summary>
        private void FinalizeModules()
        {
            foreach (Module module in _modules)
            {
                module.FinalizeModule();
            }
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }
    }
}
