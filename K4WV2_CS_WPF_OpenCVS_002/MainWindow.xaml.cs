using System;
using System.Windows;
using System.Drawing;
using Microsoft.Kinect;
using OpenCvSharp;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using OpenCvSharp.Extensions;

namespace K4WV2_CS_WPF_OpenCVS_002
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        //Kinect SDK
        KinectSensor kinect;

        ColorFrameReader colorFrameReader;
        FrameDescription colorFrameDec;

        ColorImageFormat colorFormat = ColorImageFormat.Bgra;

        //WPF
        WriteableBitmap colorBitmap;
        byte[] colorBuffer;
        int colorStride;
        Int32Rect colorRect;

        //OpenCV#

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //Kinectを開く
                kinect = KinectSensor.GetDefault();
                kinect.Open();

                //カラー画像の情報を作成する
                colorFrameDec = kinect.ColorFrameSource.CreateFrameDescription(colorFormat);

                //カラーリーダーを開く
                colorFrameReader = kinect.ColorFrameSource.OpenReader();
                colorFrameReader.FrameArrived += colorFrameReader_FrameArrived;

                //カラー用のビットマップを作成する
                colorBitmap = new WriteableBitmap(colorFrameDec.Width, colorFrameDec.Height, 96, 96, PixelFormats.Bgra32, null);
                colorStride = colorFrameDec.Width * (int)colorFrameDec.BytesPerPixel;
                colorRect = new Int32Rect(0, 0, colorFrameDec.Width, colorFrameDec.Height);
                colorBuffer = new byte[colorStride * colorFrameDec.Height];
                ImageColor.Source = colorBitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }
        }

        private void colorFrameReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            UpdateColorFrame(e);
            DrawColorFrame();
        }

        private void DrawColorFrame()
        {
            //ビットマップにする
            colorBitmap.WritePixels(colorRect, colorBuffer, colorStride, 0);
        }

        private void UpdateColorFrame(ColorFrameArrivedEventArgs e)
        {
            //カラーフレームを取得する
            using (var colorFrame=e.FrameReference.AcquireFrame())
            {
                if (colorFrame==null)
                {
                    return;
                }

                //BGRAデータを取得する
                colorBuffer = new byte[colorFrameDec.LengthInPixels * colorFrameDec.BytesPerPixel];
                colorFrame.CopyConvertedFrameDataToArray(colorBuffer, ColorImageFormat.Bgra);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (colorFrameReader!=null)
            {
                colorFrameReader.Dispose();
                colorFrameReader = null;
            }
            if (kinect!=null)
            {
                kinect.Close();
                kinect = null;
            }
        }
    }
}
