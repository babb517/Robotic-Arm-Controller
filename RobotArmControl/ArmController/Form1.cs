using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Touchless.Vision.Camera;
using System.Diagnostics;
using System.Net;
using TCPCam;
using TCPCamActivex;

using ArmController.Kinect_Module;
using ArmController.Integration;
using ArmController.Robot_Arm_Module;
using ArmController.CyberGloveLibrary;
using ArmController.IMU_Module;
using System.Windows.Threading;
using System.Windows.Media;

namespace ArmController // Capstone_GUI
{
    public partial class Form1 : Form
    {
        /**************************************************************************/
        /* Private Members */
        /**************************************************************************/
        /// <summary>
        /// TODO
        /// </summary>
        private CameraFrameSource _frameSource;

        /// <summary>
        /// TODO
        /// </summary>
        private static Bitmap _latestFrame;

        /// <summary>
        /// Check to see whether or not the modules have been initialized
        /// </summary>
        private bool isStarted = false;

        /// <summary>
        /// The modules which are being managed by the application.
        /// </summary>
        List<Module> _modules;

        /// <summary>
        /// The bus used to communicate between modules.
        /// </summary>
        VirtualBus _bus;

        /// <summary>
        /// The drawing for the kinect to use.
        /// </summary>
        DrawingGroup _kinectOutput;


        /// <summary>
        /// TODO
        /// </summary>
        TCPCam.Host Host;


        /**************************************************************************/
        /* Constructors */
        /**************************************************************************/
        /// <summary>
        /// TODO
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }


        /**************************************************************************/
        /* Lifecycle Management */
        /**************************************************************************/

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
              
                webCamComboBx.Items.Clear();
                foreach (Camera1 cam in CameraService.AvailableCameras)
                    webCamComboBx.Items.Add(cam);


                if (webCamComboBx.Items.Count > 0)
                    webCamComboBx.SelectedIndex = 0;
            }

            ip_label.Text = GetLocalIP();

            // Startup (from mainwindow).

            // setup the virtual bus.
            _bus = new VirtualBus(Dispatcher.CurrentDispatcher);
            _bus.Subscribe(BusNode.STOP_REQUESTED, OnBusValueChanged);

            // setup kinect output
            _kinectOutput = new DrawingGroup();
           // kinect_console.Image = new DrawingImage(_kinectOutput);

            _modules = new List<Module>();

        }

        /// <summary>
        /// Finalizes the states of all modules in preparation for the program exiting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Close(object sender, FormClosingEventArgs e)
        {
            // stop the kinect
            FinalizeModules();

        }

        /**************************************************************************/
        /* Event Management */
        /**************************************************************************/
      
        private void btn_robbotCtrl_Click(object sender, EventArgs e)
        {

            hide_firstPanel();
          
        }

        private void hide_firstPanel()
        {
            btn_debug.Visible = false;
            btn_robbotCtrl.Visible = false;
            btn_videoFeed.Visible = false;
            btn_ports.Visible = false;
            btn_servo.Visible = false;
            asu_logo.Visible = false;
            setPictureBox.Visible = false;
            robot_armPicture.Visible = false;
            designPictureBx.Visible = false;
            home_btn.Visible = true;
            startBtn.Visible = true;
            
        }

        private void btn_videoFeed_Click(object sender, EventArgs e)
        {
            hide_firstPanel();
            enable_webCamPanel();
            
        }

        private void enable_webCamPanel()
        {
            camStartBtn.Visible = true;
            webcam_pictureBx.Visible = true;
            webCamGroupBx.Visible = true;
            webcamGroupbx2.Visible = true;
            canStopBtn.Visible = true;
          
        }

        private void btn_servo_Click(object sender, EventArgs e)
        {
            hide_firstPanel();
            enable_servoPanel();

        }
        private void enable_servoPanel()
        {
            hand_groupBx.Visible = true;
            gripGpBox.Visible = true;
            forarmGpBox.Visible = true;
            wrist_gpBox.Visible = true;
            shoulderGP.Visible = true;
            elbowGP.Visible = true;
        }
        private void hide_servoPanel()
        {
            hand_groupBx.Visible = false;
            gripGpBox.Visible = false;
            forarmGpBox.Visible = false;
            wrist_gpBox.Visible = false;
            shoulderGP.Visible = false;
            elbowGP.Visible = false;
        }

        private void btn_ports_Click(object sender, EventArgs e)
        {
            hide_firstPanel();
            enable_kinect();
        }

        private void enable_kinect()
        {
            kinect_console.Visible = true;
            kinect_Gp.Visible = true;

        }

        private void hide_kinect()
        {
            kinect_console.Visible = false;
            kinect_Gp.Visible = false;

        }

        private void btn_debug_Click(object sender, EventArgs e)
        {
            hide_firstPanel();
            enable_debugPanel();
        }


        private void enable_debugPanel()
        {
            debugGp.Visible = true;
            System.Windows.Application.Current.MainWindow.Show();
        }
   
        private void setPictureBox_Click(object sender, EventArgs e)
        {
            hide_firstPanel();
            enable_Settings();
        }

        private void enable_Settings()
        {
            IMU_Configuration.Visible = true;
            glove_configuration.Visible = true;
            webcam_configuration.Visible = true;
            kinect_configuration.Visible = true;
            setting_label.Visible = true;
            setting_pictureBx.Visible = true;
            startBtn.Visible = true;
            home_btn.Visible = true;
        }

        private void home_btn_Click(object sender, EventArgs e)
        {
            hide_Settings();
            hide_webcamPanel();
            hide_servoPanel();
            hide_kinect();
            debugGp.Visible = false;
            System.Windows.Application.Current.MainWindow.Hide();
            enable_homePanel();
            FinalizeModules();
        }
        private void hide_webcamPanel()
        {
            camStartBtn.Visible = false;
            webcam_pictureBx.Visible = false;
            webCamGroupBx.Visible = false;
            canStopBtn.Visible = false;
            webcamGroupbx2.Visible = false;
        }

        private void enable_homePanel()
        {
            btn_debug.Visible = true;
            btn_robbotCtrl.Visible = true;
            btn_videoFeed.Visible = true;
            btn_ports.Visible = true;
            btn_servo.Visible = true;
            asu_logo.Visible = true;
            setPictureBox.Visible = true;
            robot_armPicture.Visible = true;
            designPictureBx.Visible = true;

        }

        private void hide_Settings()
        {

            IMU_Configuration.Visible =false;
            glove_configuration.Visible = false;
            webcam_configuration.Visible = false;
            kinect_configuration.Visible = false;
            setting_label.Visible = false;
            setting_pictureBx.Visible = false;
            startBtn.Visible = false;
            home_btn.Visible = false;
            webcam_pictureBx.Visible = false;
            camStartBtn.Visible = false;
        }

        private void startCapturing()
        {
           

            try
            {
                Camera1 c = (Camera1)webCamComboBx.SelectedItem;
                setFrameSource(new CameraFrameSource(c));
                _frameSource.Camera.CaptureWidth = 20;
                _frameSource.Camera.CaptureHeight = 40;
                _frameSource.Camera.Fps = 20;
                _frameSource.NewFrame += OnImageCaptured;

                webcam_pictureBx.Paint += new PaintEventHandler(drawLatestImage);
                _frameSource.StartFrameCapture();



            }
            catch (Exception ex)
            {
                webCamComboBx.Text = "Select A Camera";
                MessageBox.Show(ex.Message);
            }
        }

        private void drawLatestImage(object sender, PaintEventArgs e)
        {
            if (_latestFrame != null)
            {
                // Draw the latest image from the active camera
                e.Graphics.DrawImage(_latestFrame, 0, 0, _latestFrame.Width, _latestFrame.Height);
            }
        }

        public void OnImageCaptured(Touchless.Vision.Contracts.IFrameSource frameSource, Touchless.Vision.Contracts.Frame frame, double fps)
        {
            _latestFrame = frame.Image;
            webcam_pictureBx.Invalidate();
        }

        private void setFrameSource(CameraFrameSource cameraFrameSource)
        {
            if (_frameSource == cameraFrameSource)
                return;

            _frameSource = cameraFrameSource;
        }

        //

        private void thrashOldCamera()
        {
            // Trash the old camera
            if (_frameSource != null)
            {
                _frameSource.NewFrame -= OnImageCaptured;
                _frameSource.Camera.Dispose();
                setFrameSource(null);
                webcam_pictureBx.Paint -= new PaintEventHandler(drawLatestImage);
            }
        }

        private void camStartBtn_Click(object sender, EventArgs e)
        {
           // screenSize.Maximize(this);
            if (_frameSource != null && _frameSource.Camera == IMUCOMPort.SelectedItem)
                return;

            thrashOldCamera();
            startCapturing();
        }

        private void canStopBtn_Click(object sender, EventArgs e)
        {
            thrashOldCamera();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            var url = "http://192.168.43.121:8080";

            using (var process = new Process())
            {
                process.StartInfo.FileName = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
                process.StartInfo.Arguments = url + " --incognito";

                process.Start();
            }
        }


        public string GetLocalIP()
        {
            string _IP = null;

           
            System.Net.IPHostEntry _IPHostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());

          
            foreach (System.Net.IPAddress _IPAddress in _IPHostEntry.AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    _IP = _IPAddress.ToString();
                }
            }
            return _IP;
        }

        private void remoteStartBtn_Click(object sender, EventArgs e)
        {
            Host = new TCPCam.Host(webcam_pictureBx, 8080);
            Host.NoDelay = true;
            Host.StartConnection();
        }

        private void remoteStopBtn_Click(object sender, EventArgs e)
        {
            Host.CloseConnection();
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            // TODO: Add each module to the list here.

            if (isStarted == false)
            {
                //Don't allow modules to be initialized more than once
                isStarted = true;

                //Wrist servo settings
                _bus.Publish(BusNode.WRIST_SERVO_MIN_RANGE, Convert.ToInt32(wristServoMinRange.Text));
                _bus.Publish(BusNode.WRIST_SERVO_MAX_RANGE, Convert.ToInt32(wristServoMaxRange.Text));
                _bus.Publish(BusNode.WRIST_SERVO_SPEED, Convert.ToInt32(wristServoSpeed.Text));
                _bus.Publish(BusNode.WRIST_SERVO_ENABLE, wristServoEnable.Checked);

                //Hand servo settings
                _bus.Publish(BusNode.HAND_SERVO_MIN_RANGE, Convert.ToInt32(handServoMinRange.Text));
                _bus.Publish(BusNode.HAND_SERVO_MAX_RANGE, Convert.ToInt32(handServoMaxRange.Text));
                _bus.Publish(BusNode.HAND_SERVO_SPEED, Convert.ToInt32(handServoSpeed.Text));
                _bus.Publish(BusNode.HAND_SERVO_ENABLE, handServoEnable.Checked);

                //Wrist rotate servo settings
                _bus.Publish(BusNode.WRIST_ROTATE_SERVO_MIN_RANGE, Convert.ToInt32(wristRotateServoMinRange.Text));
                _bus.Publish(BusNode.WRIST_ROTATE_SERVO_MAX_RANGE, Convert.ToInt32(wristRotateServoMaxRange.Text));
                _bus.Publish(BusNode.WRIST_ROTATE_SERVO_SPEED, Convert.ToInt32(wristRotateServoSpeed.Text));
                _bus.Publish(BusNode.WRIST_ROTATE_SERVO_ENABLE, wristRotateServoEnable.Checked);

                //Arm servo settings
                _bus.Publish(BusNode.ARM_SERVO_MIN_RANGE, Convert.ToInt32(armServoMinRange.Text));
                _bus.Publish(BusNode.ARM_SERVO_MAX_RANGE, Convert.ToInt32(armServoMaxRange.Text));
                _bus.Publish(BusNode.ARM_SERVO_SPEED, Convert.ToInt32(armServoSpeed.Text));
                _bus.Publish(BusNode.ARM_SERVO_ENABLE, armServoEnable.Checked);

                //Forearm servo settings
                _bus.Publish(BusNode.FOREARM_SERVO_MIN_RANGE, Convert.ToInt32(forearmServoMinRange.Text));
                _bus.Publish(BusNode.FOREARM_SERVO_MAX_RANGE, Convert.ToInt32(forearmServoMaxRange.Text));
                _bus.Publish(BusNode.FOREARM_SERVO_SPEED, Convert.ToInt32(forearmServoSpeed.Text));
                _bus.Publish(BusNode.FOREARM_SERVO_ENABLE, forearmServoEnable.Checked);

                //Shoulder servo settings
                _bus.Publish(BusNode.SHOULDER_SERVO_MIN_RANGE, Convert.ToInt32(shoulderServoMinRange.Text));
                _bus.Publish(BusNode.SHOULDER_SERVO_MAX_RANGE, Convert.ToInt32(shoulderServoMaxRange.Text));
                _bus.Publish(BusNode.SHOULDER_SERVO_SPEED, Convert.ToInt32(shoulderServoSpeed.Text));
                _bus.Publish(BusNode.SHOULDER_SERVO_ENABLE, shoulderServoEnable.Checked);

                //CyberGlove settings
                _bus.Publish(BusNode.CYBERGLOVE_BAUD_RATE, Convert.ToInt32(cyberGloveBaudRate));
                _bus.Publish(BusNode.CYBERGLOVE_COM_PORT, cyberGloveCOMPort);
                _bus.Publish(BusNode.CYBERGLOVE_ENABLE, cyberGloveEnable);

                //IMU settings
                _bus.Publish(BusNode.IMU_BAUD_RATE, Convert.ToInt32(IMUBaudRate));
                _bus.Publish(BusNode.IMU_COM_PORT, IMUCOMPort);
                _bus.Publish(BusNode.IMU_ENABLE, Convert.ToInt32(IMUEnable));

                //Kinect settings
                _bus.Publish(BusNode.KINECT_ENABLE, kinectEnable.Checked);

                InitializeModules();
            }
        }


        /**************************************************************************/
        /* Private Methods */
        /**************************************************************************/

        /// <summary>
        /// Initializes the states of all the modules.
        /// </summary>
        private void InitializeModules()
        {
            if (_bus.Get<bool>(BusNode.KINECT_ENABLE))
                _modules.Add(new PositionalTracker(_kinectOutput)); // Kinect

           // if (_bus.Get<bool>(BusNode.IMU_ENABLE))
                _modules.Add(new IMUModule());

            _modules.Add(new PositionFeedback());
            _modules.Add(new RobotArmModule());

           // if (_bus.Get<bool>(BusNode.CYBERGLOVE_ENABLE))
                _modules.Add(new GloveModule());


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

            _modules.Clear();
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


        
    }
}
