using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Kinect;


namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
           

        }
        byte[] globalvideo = new byte[307200];
        byte[] globalvideo2 = new byte[307200];
        byte[] globalvideo3 = new byte[307200];
        KinectSensor _sensor;
        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                _sensor = KinectSensor.KinectSensors[0];

                if (_sensor.Status == KinectStatus.Connected)
                {
                    _sensor.ColorStream.Enable();
                    _sensor.DepthStream.Enable();
                    _sensor.SkeletonStream.Enable();
                    _sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(_sensor_AllFramesReady);
                    _sensor.Start();
                    

                }
            }

        }

        private byte[] GenerateColoredBytes(DepthImageFrame depthFrame)
        {
            short[] rawDepthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawDepthData);

            Byte[] pixels = new byte[depthFrame.Height * depthFrame.Width * 4];

            const int BlueIndex = 0;
            const int GreenIndex = 1;
            const int RedIndex = 2;

            for (int depthIndex = 0, colorIndex = 0;
                depthIndex < rawDepthData.Length && colorIndex < pixels.Length;
                depthIndex++, colorIndex += 4)
            {
                int player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;
                int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                if (depth <= 900)
                {
                    pixels[colorIndex + BlueIndex] = globalvideo2[colorIndex + BlueIndex];
                    pixels[colorIndex + GreenIndex] = globalvideo2[colorIndex + GreenIndex];
                    pixels[colorIndex + RedIndex] = globalvideo2[colorIndex + RedIndex];

                }
                else if (depth < 2000)
                {
                    pixels[colorIndex + BlueIndex] = globalvideo[colorIndex + BlueIndex];
                    pixels[colorIndex + GreenIndex] = globalvideo[colorIndex + GreenIndex];
                    pixels[colorIndex + RedIndex] = globalvideo[colorIndex + RedIndex];

                }
                else
                {
                    pixels[colorIndex + BlueIndex] = globalvideo3[colorIndex + BlueIndex];
                    pixels[colorIndex + GreenIndex] = globalvideo3[colorIndex + GreenIndex];
                    pixels[colorIndex + RedIndex] = globalvideo3[colorIndex + RedIndex];

                }


            }

            return pixels;

        }

        private byte[] MessWithColor(byte[] pixels)
        {
            byte[] frame = new byte[pixels.Length];
            const int BlueIndex = 0;
            const int GreenIndex = 1;
            const int RedIndex = 2;

            //make it shades of red (remove b and g)
            for (int idx = 0;
                idx < frame.Length;
                idx += 4)
            {
                frame[idx + BlueIndex] = 0;
                frame[idx + GreenIndex] = 0;
                frame[idx + RedIndex] = pixels[idx + RedIndex];
            }
            
            return frame;
        }
        private byte[] MessWithColor2(byte[] pixels)
        {
            byte[] frame = new byte[pixels.Length];
            const int BlueIndex = 0;
            const int GreenIndex = 1;
            const int RedIndex = 2;

            //make it shades of red (remove b and g)
            for (int idx = 0;
                idx < frame.Length;
                idx += 4)
            {
                frame[idx + BlueIndex] =0;
                frame[idx + GreenIndex] = pixels[idx + GreenIndex];
                frame[idx + RedIndex] =0;
            }

            return frame;
        }
        private byte[] MessWithColor3(byte[] pixels)
        {
            byte[] frame = new byte[pixels.Length];
            const int BlueIndex = 0;
            const int GreenIndex = 1;
            const int RedIndex = 2;

            //make it shades of red (remove b and g)
            for (int idx = 0;
                idx < frame.Length;
                idx += 4)
            {
                frame[idx + BlueIndex] = pixels[idx + BlueIndex];
                frame[idx + GreenIndex] = 0;
                frame[idx + RedIndex] = 0;
            }

            return frame;
        }

        void _sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {


            //throw new NotImplementedException();
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null)
                {
                    return;
                }

                byte[] rawColor = new byte[colorFrame.PixelDataLength];
                byte[] rawColor2 = new byte[colorFrame.PixelDataLength];
                byte[] rawColor3 = new byte[colorFrame.PixelDataLength];

                //copy data out into our byte array
                colorFrame.CopyPixelDataTo(rawColor2);
                colorFrame.CopyPixelDataTo(rawColor);
                colorFrame.CopyPixelDataTo(rawColor3);

                //create pixels2 array that contain tinted colordata
                byte[] pixels2 = MessWithColor(rawColor);
                byte[] pixels3 = MessWithColor2(rawColor2);
                byte[] pixels4 = MessWithColor3(rawColor3);

                globalvideo = pixels2;
                globalvideo2 = pixels3;
                globalvideo3 = pixels4;
                int stride = colorFrame.Width * 4;

            }
            using (DepthImageFrame howdeep = e.OpenDepthImageFrame())
            {
                if (howdeep == null)
                {
                    return;
                }
                byte[] finalpicture = GenerateColoredBytes(howdeep);

                int stride = howdeep.Width * 4;


                videodisplay.Source = BitmapSource.Create(howdeep.Width, howdeep.Height,
                    96, 96, PixelFormats.Bgr32, null, finalpicture, stride);

            }




        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

      


    }
}
