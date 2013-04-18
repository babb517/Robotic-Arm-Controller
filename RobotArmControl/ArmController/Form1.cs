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

namespace ArmController // Capstone_GUI
{
    public partial class Form1 : Form
    {
        private CameraFrameSource _frameSource;
        private static Bitmap _latestFrame;

        TCPCam.Host Host;

        public Form1()
        {
            InitializeComponent();
        }

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
        }


      
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
        }
        private void hide_servoPanel()
        {
            hand_groupBx.Visible = false;
            gripGpBox.Visible = false;
            forarmGpBox.Visible = false;
            wrist_gpBox.Visible = false;
            shoulderGP.Visible = false;
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
            enable_homePanel();
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
            if (_frameSource != null && _frameSource.Camera == comboBox1.SelectedItem)
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
          
        
        
    }
}
