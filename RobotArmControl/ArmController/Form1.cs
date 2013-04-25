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
            LoadGUISettings();

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
                _bus.Publish(BusNode.CYBERGLOVE_BAUD_RATE, Convert.ToInt32(cyberGloveBaudRate.SelectedValue));
                _bus.Publish(BusNode.CYBERGLOVE_COM_PORT, cyberGloveCOMPort.SelectedText);
                _bus.Publish(BusNode.CYBERGLOVE_ENABLE, cyberGloveEnable.Checked);
                _bus.Publish(BusNode.CYBERGLOVE_COM_PORT, "COM1");
                _bus.Publish(BusNode.CYBERGLOVE_BAUD_RATE, 115200);

                //IMU settings
                _bus.Publish(BusNode.IMU_BAUD_RATE, Convert.ToInt32(IMUBaudRate.SelectedValue));
                _bus.Publish(BusNode.IMU_COM_PORT, IMUCOMPort.SelectedText);
                _bus.Publish(BusNode.IMU_ENABLE, IMUEnable.Checked);
                _bus.Publish(BusNode.IMU_COM_PORT, "COM5");
                _bus.Publish(BusNode.IMU_BAUD_RATE, 19200);

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

            if (_bus.Get<bool>(BusNode.IMU_ENABLE))
                _modules.Add(new IMUModule());

            _modules.Add(new PositionFeedback());
            _modules.Add(new RobotArmModule());

            if (_bus.Get<bool>(BusNode.CYBERGLOVE_ENABLE))
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

        /// <summary>
        /// Loads all of the GUI textfields, checkboxes, and comboboxes with saved settings
        /// </summary>
        private void LoadGUISettings()
        {
            armServoEnable.Checked = ArmController.Properties.Settings.Default.armEnable;
            armServoMinRange.Text = Convert.ToString(ArmController.Properties.Settings.Default.armMinRange);
            armServoMaxRange.Text = Convert.ToString(ArmController.Properties.Settings.Default.armMaxRange);
            armServoSpeed.Text = Convert.ToString(ArmController.Properties.Settings.Default.armSpeed);

            cyberGloveBaudRate.SelectedIndex = ArmController.Properties.Settings.Default.cyberGloveBaudRateIndex;
            cyberGloveCOMPort.SelectedIndex = ArmController.Properties.Settings.Default.cyberGloveCOMPortIndex;
            cyberGloveDefaultPosition.Text = Convert.ToString(ArmController.Properties.Settings.Default.cyberGloveDefaultPosition);
            cyberGloveEnable.Checked = ArmController.Properties.Settings.Default.cyberGloveEnable;

            forearmServoEnable.Checked = ArmController.Properties.Settings.Default.forearmEnable;
            forearmServoMinRange.Text = Convert.ToString(ArmController.Properties.Settings.Default.forearmMinRange);
            forearmServoMaxRange.Text = Convert.ToString(ArmController.Properties.Settings.Default.forearmMaxRange);
            forearmServoSpeed.Text = Convert.ToString(ArmController.Properties.Settings.Default.forearmSpeed);

            handServoEnable.Checked = ArmController.Properties.Settings.Default.handEnable;
            handServoMinRange.Text = Convert.ToString(ArmController.Properties.Settings.Default.handMinRange);
            handServoMaxRange.Text = Convert.ToString(ArmController.Properties.Settings.Default.handMaxRange);
            handServoSpeed.Text = Convert.ToString(ArmController.Properties.Settings.Default.handSpeed);

            IMUBaudRate.SelectedIndex = ArmController.Properties.Settings.Default.IMUBaudRateIndex;
            IMUCOMPort.SelectedIndex = ArmController.Properties.Settings.Default.IMUCOMPortIndex;
            IMUEnable.Checked = ArmController.Properties.Settings.Default.IMUEnable;

            kinectEnable.Checked = ArmController.Properties.Settings.Default.kinectEnable;

            shoulderServoEnable.Checked = ArmController.Properties.Settings.Default.shoulderEnable;
            shoulderServoMinRange.Text = Convert.ToString(ArmController.Properties.Settings.Default.shoulderMinRange);
            shoulderServoMaxRange.Text = Convert.ToString(ArmController.Properties.Settings.Default.shoulderMaxRange);
            shoulderServoSpeed.Text = Convert.ToString(ArmController.Properties.Settings.Default.shoulderSpeed);

            wristServoEnable.Checked = ArmController.Properties.Settings.Default.wristEnable;
            wristServoMinRange.Text = Convert.ToString(ArmController.Properties.Settings.Default.wristMinRange);
            wristServoMaxRange.Text = Convert.ToString(ArmController.Properties.Settings.Default.wristMaxRange);
            wristServoSpeed.Text = Convert.ToString(ArmController.Properties.Settings.Default.wristSpeed);

            wristRotateServoEnable.Checked = ArmController.Properties.Settings.Default.wristRotateEnable;
            wristRotateServoMinRange.Text = Convert.ToString(ArmController.Properties.Settings.Default.wristRotateMinRange);
            wristRotateServoMaxRange.Text = Convert.ToString(ArmController.Properties.Settings.Default.wristRotateMaxRange);
            wristRotateServoSpeed.Text = Convert.ToString(ArmController.Properties.Settings.Default.wristRotateSpeed);
        }

        private void IMUBaudRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.IMUBaudRateIndex = IMUBaudRate.SelectedIndex;
            ArmController.Properties.Settings.Default.Save();
        }

        private void IMUCOMPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.IMUCOMPortIndex = IMUCOMPort.SelectedIndex;
            ArmController.Properties.Settings.Default.Save();
        } 

        private void IMUEnable_CheckedChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.IMUEnable = IMUEnable.Checked;
            ArmController.Properties.Settings.Default.Save();
        }

        private void cyberGloveEnable_CheckedChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.cyberGloveEnable = cyberGloveEnable.Checked;
            ArmController.Properties.Settings.Default.Save();
        }

        private void cyberGloveDefaultPosition_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.cyberGloveDefaultPosition = Convert.ToInt32(cyberGloveDefaultPosition.Text);
            ArmController.Properties.Settings.Default.Save();
        }

        private void cyberGloveBaudRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.cyberGloveBaudRateIndex = cyberGloveBaudRate.SelectedIndex;
            ArmController.Properties.Settings.Default.Save();
        }

        private void cyberGloveCOMPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.cyberGloveCOMPortIndex = cyberGloveCOMPort.SelectedIndex;
            ArmController.Properties.Settings.Default.Save();
        }

        private void kinectEnable_CheckedChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.kinectEnable = kinectEnable.Checked;
            ArmController.Properties.Settings.Default.Save();
        }

        private void handServoMinRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.handMinRange = Convert.ToInt32(handServoMinRange.Text);
            ArmController.Properties.Settings.Default.Save();
        }

        private void handServoMaxRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.handMaxRange = Convert.ToInt32(handServoMaxRange.Text);
            ArmController.Properties.Settings.Default.Save();
        }

        private void handServoSpeed_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.handSpeed = Convert.ToInt32(handServoSpeed.Text);
            ArmController.Properties.Settings.Default.Save();
        }

        private void handServoEnable_CheckedChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.handEnable = handServoEnable.Checked;
            ArmController.Properties.Settings.Default.Save();
        }

        private void wristServoMinRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.wristMinRange = Convert.ToInt32(wristServoMinRange.Text);
            ArmController.Properties.Settings.Default.Save();
        }

        private void wristServoMaxRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.wristMaxRange = Convert.ToInt32(wristServoMaxRange.Text);
            ArmController.Properties.Settings.Default.Save();
        }

        private void wristServoSpeed_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.wristSpeed = Convert.ToInt32(wristServoSpeed.Text);
            ArmController.Properties.Settings.Default.Save();
        }

        private void wristServoEnable_CheckedChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.wristEnable = wristServoEnable.Checked;
            ArmController.Properties.Settings.Default.Save();
        }

        private void wristRotateServoMinRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.wristRotateMinRange = Convert.ToInt32(wristRotateServoMinRange.Text);
            ArmController.Properties.Settings.Default.Save();
        }

        private void wristRotateServoMaxRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.wristRotateMaxRange = Convert.ToInt32(wristRotateServoMaxRange.Text);
            ArmController.Properties.Settings.Default.Save();
        }

        private void wristRotateServoSpeed_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.wristRotateSpeed = Convert.ToInt32(wristRotateServoSpeed.Text);
            ArmController.Properties.Settings.Default.Save();
        }

        private void wristRotateServoEnable_CheckedChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.wristRotateEnable = wristRotateServoEnable.Checked;
            ArmController.Properties.Settings.Default.Save();
        }

        private void forearmServoMinRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.forearmMinRange = Convert.ToInt32(forearmServoMinRange.Text);
            ArmController.Properties.Settings.Default.Save();
        }

        private void forearmServoMaxRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.forearmMaxRange = Convert.ToInt32(forearmServoMaxRange.Text);
            ArmController.Properties.Settings.Default.Save();
        }

        private void forearmServoSpeed_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.forearmSpeed = Convert.ToInt32(forearmServoSpeed.Text);
            ArmController.Properties.Settings.Default.Save();
        }

        private void forearmServoEnable_CheckedChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.forearmEnable = forearmServoEnable.Checked;
            ArmController.Properties.Settings.Default.Save();
        }

        private void armServoMinRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.armMinRange = Convert.ToInt32(armServoMinRange.Text);
            ArmController.Properties.Settings.Default.Save();
        }

        private void armServoMaxRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.armMaxRange = Convert.ToInt32(armServoMaxRange.Text);
            ArmController.Properties.Settings.Default.Save();
        }

        private void armServoSpeed_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.armSpeed = Convert.ToInt32(armServoSpeed.Text);
            ArmController.Properties.Settings.Default.Save();
        }

        private void armServoEnable_CheckedChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.armEnable = armServoEnable.Checked;
            ArmController.Properties.Settings.Default.Save();
        }

        private void shoulderServoMinRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.shoulderMinRange = Convert.ToInt32(shoulderServoMinRange.Text);
            ArmController.Properties.Settings.Default.Save();
        }

        private void shoulderServoMaxRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.shoulderMaxRange = Convert.ToInt32(shoulderServoMaxRange.Text);
            ArmController.Properties.Settings.Default.Save();
        }

        private void shoulderServoSpeed_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.shoulderSpeed = Convert.ToInt32(shoulderServoSpeed.Text);
            ArmController.Properties.Settings.Default.Save();
        }

        private void shoulderServoEnable_CheckedChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.shoulderEnable = shoulderServoEnable.Checked;
            ArmController.Properties.Settings.Default.Save();
        }
    }
}
