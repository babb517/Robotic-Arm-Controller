﻿using System;
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
using System.IO.Ports;
using System.Threading;
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


        //manual Control
        SerialPort _serialPort;
        int shoulderPos = 1600;
        int armPos = 1600;
        int forearmPos = 1600;
        int wristRotatePos = 1600;
        int wristPos = 1600;
        int handPos = 1600;
        int servo;
        private delegate void SetTextDeleg(string text);


        /**************************************************************************/
        /* Constructors */
        /**************************************************************************/
        /// <summary>
        /// TODO
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            LoadGUISettings();
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
            startBtn.Visible = false;
            _serialPort = new SerialPort("COM3", 115200, Parity.None, 8, StopBits.One);

            _serialPort.Handshake = Handshake.None;
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            try
            {
                _serialPort.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening COM Port for manual robot arm control.");
            }


            
            shBtnLeft.Visible = true;
            shBtnRight.Visible = true;
            armBtnLeft.Visible = true;
            armBtnRight.Visible = true;
            forArmBtnLeft.Visible = true;
            foreArmBtnRight.Visible = true;
            wristLeft.Visible = true;
            wristRight.Visible = true;
            WristRotateLeft.Visible = true;
            wristRotateRight.Visible = true;
            handLeft.Visible = true;
            handRight.Visible = true;

            shoulderLabel.Visible = true;
            armLabel.Visible = true;
            forearmLabel.Visible = true;
            wristLabel.Visible = true;
            wristRotateLabel.Visible = true;
            handLabel.Visible = true;
            hide_firstPanel1();
        }

        void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(500);
            string data = _serialPort.ReadLine();
            this.BeginInvoke(new SetTextDeleg(si_DataReceived), new object[] { data });
        }
        private void si_DataReceived(string data)
        {
            //textBox1.Text = data.Trim();

        }


        public void send_manualControl(string command)
        {
            string tempcommand = command + "\r\n";

            try
            {
                if (!_serialPort.IsOpen)
                    _serialPort.Open();

                _serialPort.Write(tempcommand);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening/writing to serial port :: " + ex.Message, "Error!");
            }

        }
        private void hide_firstPanel1()
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
           // startBtn.Visible = true;

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
            robotArm_Configuration.Visible = true;
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
            hide_controlPanel();

        }

        private void hide_controlPanel()
        {
            shBtnLeft.Visible = false;
            shBtnRight.Visible = false;
            armBtnLeft.Visible = false;
            armBtnRight.Visible = false;
            forArmBtnLeft.Visible = false;
            foreArmBtnRight.Visible = false;
            wristLeft.Visible = false;
            wristRight.Visible = false;
            WristRotateLeft.Visible = false;
            wristRotateRight.Visible = false;
            handLeft.Visible = false;
            handRight.Visible = false;



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
            shoulderLabel.Visible = false;
            armLabel.Visible = false;
            forearmLabel.Visible = false;
            wristLabel.Visible = false;
            wristRotateLabel.Visible = false;
            handLabel.Visible = false;
            if (_serialPort != null && _serialPort.IsOpen == true)
            {
                _serialPort.Close();
            }
        }

        private void hide_Settings()
        {

            IMU_Configuration.Visible =false;
            glove_configuration.Visible = false;
            webcam_configuration.Visible = false;
            kinect_configuration.Visible = false;
            robotArm_Configuration.Visible = false;
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
                _bus.Publish(BusNode.CYBERGLOVE_BAUD_RATE, Convert.ToInt32(cyberGloveBaudRate.Text));
                _bus.Publish(BusNode.CYBERGLOVE_COM_PORT, cyberGloveCOMPort.Text);
                _bus.Publish(BusNode.CYBERGLOVE_ENABLE, cyberGloveEnable.Checked);
                //_bus.Publish(BusNode.CYBERGLOVE_COM_PORT, "COM1");
                //_bus.Publish(BusNode.CYBERGLOVE_BAUD_RATE, 115200);

                //IMU settings
                _bus.Publish(BusNode.IMU_BAUD_RATE, Convert.ToInt32(IMUBaudRate.Text));
                _bus.Publish(BusNode.IMU_COM_PORT, IMUCOMPort.Text);
                _bus.Publish(BusNode.IMU_ENABLE, IMUEnable.Checked);
                //_bus.Publish(BusNode.IMU_COM_PORT, "COM5");
                //_bus.Publish(BusNode.IMU_BAUD_RATE, 19200);

                //Robot Arm Settings
                _bus.Publish(BusNode.ROBOT_ARM_BAUD_RATE, Convert.ToInt32(robotArmBaudRate.Text));
                _bus.Publish(BusNode.ROBOT_ARM_COM_PORT, cyberGloveCOMPort.Text);
                _bus.Publish(BusNode.ROBOT_ARM_ENABLE, cyberGloveEnable.Checked);

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
            armServoMinRange.Text = ArmController.Properties.Settings.Default.armMinRange;
            armServoMaxRange.Text = ArmController.Properties.Settings.Default.armMaxRange;
            armServoSpeed.Text = ArmController.Properties.Settings.Default.armSpeed;

            cyberGloveBaudRate.SelectedIndex = ArmController.Properties.Settings.Default.cyberGloveBaudRateIndex;
            cyberGloveCOMPort.SelectedIndex = ArmController.Properties.Settings.Default.cyberGloveCOMPortIndex;
            cyberGloveEnable.Checked = ArmController.Properties.Settings.Default.cyberGloveEnable;

            forearmServoEnable.Checked = ArmController.Properties.Settings.Default.forearmEnable;
            forearmServoMinRange.Text = ArmController.Properties.Settings.Default.forearmMinRange;
            forearmServoMaxRange.Text = ArmController.Properties.Settings.Default.forearmMaxRange;
            forearmServoSpeed.Text = ArmController.Properties.Settings.Default.forearmSpeed;

            handServoEnable.Checked = ArmController.Properties.Settings.Default.handEnable;
            handServoMinRange.Text = ArmController.Properties.Settings.Default.handMinRange;
            handServoMaxRange.Text = ArmController.Properties.Settings.Default.handMaxRange;
            handServoSpeed.Text = ArmController.Properties.Settings.Default.handSpeed;

            IMUBaudRate.SelectedIndex = ArmController.Properties.Settings.Default.IMUBaudRateIndex;
            IMUCOMPort.SelectedIndex = ArmController.Properties.Settings.Default.IMUCOMPortIndex;
            IMUEnable.Checked = ArmController.Properties.Settings.Default.IMUEnable;

            kinectEnable.Checked = ArmController.Properties.Settings.Default.kinectEnable;

            robotArmBaudRate.SelectedIndex = ArmController.Properties.Settings.Default.robotArmBaudRateIndex;
            robotArmCOMPort.SelectedIndex = ArmController.Properties.Settings.Default.robotArmCOMPortIndex;
            robotArmEnable.Checked = ArmController.Properties.Settings.Default.robotArmEnable;

            shoulderServoEnable.Checked = ArmController.Properties.Settings.Default.shoulderEnable;
            shoulderServoMinRange.Text = ArmController.Properties.Settings.Default.shoulderMinRange;
            shoulderServoMaxRange.Text = ArmController.Properties.Settings.Default.shoulderMaxRange;
            shoulderServoSpeed.Text = ArmController.Properties.Settings.Default.shoulderSpeed;

            wristServoEnable.Checked = ArmController.Properties.Settings.Default.wristEnable;
            wristServoMinRange.Text = ArmController.Properties.Settings.Default.wristMinRange;
            wristServoMaxRange.Text = ArmController.Properties.Settings.Default.wristMaxRange;
            wristServoSpeed.Text = ArmController.Properties.Settings.Default.wristSpeed;

            wristRotateServoEnable.Checked = ArmController.Properties.Settings.Default.wristRotateEnable;
            wristRotateServoMinRange.Text = ArmController.Properties.Settings.Default.wristRotateMinRange;
            wristRotateServoMaxRange.Text = ArmController.Properties.Settings.Default.wristRotateMaxRange;
            wristRotateServoSpeed.Text = ArmController.Properties.Settings.Default.wristRotateSpeed;
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
            ArmController.Properties.Settings.Default.handMinRange = handServoMinRange.Text;
            ArmController.Properties.Settings.Default.Save();
        }

        private void handServoMaxRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.handMaxRange = handServoMaxRange.Text;
            ArmController.Properties.Settings.Default.Save();
        }

        private void handServoSpeed_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.handSpeed = handServoSpeed.Text;
            ArmController.Properties.Settings.Default.Save();
        }

        private void handServoEnable_CheckedChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.handEnable = handServoEnable.Checked;
            ArmController.Properties.Settings.Default.Save();
        }

        private void wristServoMinRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.wristMinRange = wristServoMinRange.Text;
            ArmController.Properties.Settings.Default.Save();
        }

        private void wristServoMaxRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.wristMaxRange = wristServoMaxRange.Text;
            ArmController.Properties.Settings.Default.Save();
        }

        private void wristServoSpeed_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.wristSpeed = wristServoSpeed.Text;
            ArmController.Properties.Settings.Default.Save();
        }

        private void wristServoEnable_CheckedChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.wristEnable = wristServoEnable.Checked;
            ArmController.Properties.Settings.Default.Save();
        }

        private void wristRotateServoMinRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.wristRotateMinRange = wristRotateServoMinRange.Text;
            ArmController.Properties.Settings.Default.Save();
        }

        private void wristRotateServoMaxRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.wristRotateMaxRange = wristRotateServoMaxRange.Text;
            ArmController.Properties.Settings.Default.Save();
        }

        private void wristRotateServoSpeed_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.wristRotateSpeed = wristRotateServoSpeed.Text;
            ArmController.Properties.Settings.Default.Save();
        }

        private void wristRotateServoEnable_CheckedChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.wristRotateEnable = wristRotateServoEnable.Checked;
            ArmController.Properties.Settings.Default.Save();
        }

        private void forearmServoMinRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.forearmMinRange = forearmServoMinRange.Text;
            ArmController.Properties.Settings.Default.Save();
        }

        private void forearmServoMaxRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.forearmMaxRange = forearmServoMaxRange.Text;
            ArmController.Properties.Settings.Default.Save();
        }

        private void forearmServoSpeed_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.forearmSpeed = forearmServoSpeed.Text;
            ArmController.Properties.Settings.Default.Save();
        }

        private void forearmServoEnable_CheckedChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.forearmEnable = forearmServoEnable.Checked;
            ArmController.Properties.Settings.Default.Save();
        }

        private void armServoMinRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.armMinRange = armServoMinRange.Text;
            ArmController.Properties.Settings.Default.Save();
        }

        private void armServoMaxRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.armMaxRange = armServoMaxRange.Text;
            ArmController.Properties.Settings.Default.Save();
        }

        private void armServoSpeed_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.armSpeed = armServoSpeed.Text;
            ArmController.Properties.Settings.Default.Save();
        }

        private void armServoEnable_CheckedChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.armEnable = armServoEnable.Checked;
            ArmController.Properties.Settings.Default.Save();
        }

        private void shoulderServoMinRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.shoulderMinRange = shoulderServoMinRange.Text;
            ArmController.Properties.Settings.Default.Save();
        }

        private void shoulderServoMaxRange_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.shoulderMaxRange = shoulderServoMaxRange.Text;
            ArmController.Properties.Settings.Default.Save();
        }

        private void shoulderServoSpeed_TextChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.shoulderSpeed = shoulderServoSpeed.Text;
            ArmController.Properties.Settings.Default.Save();
        }

        private void shoulderServoEnable_CheckedChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.shoulderEnable = shoulderServoEnable.Checked;
            ArmController.Properties.Settings.Default.Save();
        }

        private void robotArmBaudRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.robotArmBaudRateIndex = robotArmBaudRate.SelectedIndex;
            ArmController.Properties.Settings.Default.Save();
        }

        private void robotArmCOMPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.robotArmCOMPortIndex = robotArmCOMPort.SelectedIndex;
            ArmController.Properties.Settings.Default.Save();
        }

        private void robotArmEnable_CheckedChanged(object sender, EventArgs e)
        {
            ArmController.Properties.Settings.Default.robotArmEnable = robotArmEnable.Checked;
            ArmController.Properties.Settings.Default.Save();
        }

        private void shBtnLeft_Click(object sender, EventArgs e)
        {
            servo = 0;
            shoulderPos = shoulderPos + 100;
            string tempServo = servo.ToString();
            string tempPos = shoulderPos.ToString();
            string command = "#" + tempServo + "P" + tempPos + "S" + "500" + "\r\n";

            //send(command);

            send_manualControl(command);
        }

        private void shBtnRight_Click(object sender, EventArgs e)
        {
            servo = 0;
            shoulderPos = shoulderPos - 100;
            string tempServo = servo.ToString();
            string tempPos = shoulderPos.ToString();
            string command = "#" + tempServo + "P" + tempPos + "S" + "500" + "\r\n";

            //send(command);

            send_manualControl(command);
        }

        private void armBtnLeft_Click(object sender, EventArgs e)
        {
            servo = 1;
            armPos = armPos + 100;

            Thread.Sleep(100);
            string tempServo = servo.ToString();
            string tempPos = armPos.ToString();
            string command = "#" + tempServo + "P" + tempPos + "S" + "500" + "\r\n";

            //send(command);

            send_manualControl(command);
        }

        private void armBtnRight_Click(object sender, EventArgs e)
        {
            servo = 1;
            armPos = armPos - 100;
            string tempServo = servo.ToString();
            string tempPos = armPos.ToString();
            string command = "#" + tempServo + "P" + tempPos + "S" + "500" + "\r\n";

            //send(command);

            send_manualControl(command);
        }

        private void forArmBtnLeft_Click(object sender, EventArgs e)
        {
            servo = 2;
            forearmPos = forearmPos + 100;
            string tempServo = servo.ToString();
            string tempPos = forearmPos.ToString();
            string command = "#" + tempServo + "P" + tempPos + "S" + "500" + "\r\n";

            //send(command);

            send_manualControl(command);
        }

        private void foreArmBtnRight_Click(object sender, EventArgs e)
        {
            servo = 2;
            forearmPos = forearmPos - 100;
            string tempServo = servo.ToString();
            string tempPos = forearmPos.ToString();
            string command = "#" + tempServo + "P" + tempPos + "S" + "500" + "\r\n";

            //send(command);

            send_manualControl(command);
        }

        private void wristLeft_Click(object sender, EventArgs e)
        {
            servo = 3;
            wristPos = wristPos + 100;
            string tempServo = servo.ToString();
            string tempPos = wristPos.ToString();
            string command = "#" + tempServo + "P" + tempPos + "S" + "500" + "\r\n";

            //send(command);

            send_manualControl(command);
        }

        private void wristRight_Click(object sender, EventArgs e)
        {
            servo = 3;
            wristPos = wristPos - 100;
            string tempServo = servo.ToString();
            string tempPos = wristPos.ToString();
            string command = "#" + tempServo + "P" + tempPos + "S" + "500" + "\r\n";

            //send(command);

            send_manualControl(command);
        }

        private void WristRotateLeft_Click(object sender, EventArgs e)
        {
            servo = 4;
            wristRotatePos = wristRotatePos + 100;
            string tempServo = servo.ToString();
            string tempPos = wristRotatePos.ToString();
            string command = "#" + tempServo + "P" + tempPos + "S" + "500" + "\r\n";

            //send(command);

            send_manualControl(command);
        }

        private void wristRotateRight_Click(object sender, EventArgs e)
        {
            servo = 4;
            wristRotatePos = wristRotatePos - 100;
            string tempServo = servo.ToString();
            string tempPos = wristRotatePos.ToString();
            string command = "#" + tempServo + "P" + tempPos + "S" + "500" + "\r\n";

            //send(command);

            send_manualControl(command);
        }

        private void handLeft_Click(object sender, EventArgs e)
        {
            servo = 5;
            handPos = handPos + 100;
            string tempServo = servo.ToString();
            string tempPos = handPos.ToString();
            string command = "#" + tempServo + "P" + tempPos + "S" + "500" + "\r\n";

            //send(command);

            send_manualControl(command);
        }

        private void handRight_Click(object sender, EventArgs e)
        {
            servo = 5;
            handPos = handPos - 100;
            string tempServo = servo.ToString();
            string tempPos = handPos.ToString();
            string command = "#" + tempServo + "P" + tempPos + "S" + "500" + "\r\n";

            //send(command);

            send_manualControl(command);
        }
    }
}
