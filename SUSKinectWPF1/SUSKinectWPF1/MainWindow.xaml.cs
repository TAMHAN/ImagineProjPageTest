using Microsoft.Kinect;
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

namespace SUSKinectWPF1
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor mySensor;
        MultiSourceFrameReader myReader;
        byte[] myPixels=null;
        Body[] myBodies = null;
        WriteableBitmap myBitmap;
        public MainWindow()
        {
            InitializeComponent();
            mySensor = KinectSensor.GetDefault();
            mySensor.Open();
            myReader = mySensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body);
            myReader.MultiSourceFrameArrived += myReader_MultiSourceFrameArrived;

            myBitmap = new WriteableBitmap(1920, 1080, 96.0, 96.0, PixelFormats.Pbgra32, null);
            image1.Source = myBitmap;
        }

        void drawABone(Joint _a, Joint _b)
        {
            ColorSpacePoint pointA = mySensor.CoordinateMapper.MapCameraPointToColorSpace(_a.Position);
            ColorSpacePoint pointB = mySensor.CoordinateMapper.MapCameraPointToColorSpace(_b.Position);
            myBitmap.DrawLine((int)pointA.X - 1, (int)pointA.Y - 1, (int)pointB.X - 1, (int)pointB.Y - 1, Color.FromRgb(200, 0, 200));
            myBitmap.DrawLine((int)pointA.X, (int)pointA.Y, (int)pointB.X, (int)pointB.Y, Color.FromRgb(200, 0, 200));
            myBitmap.DrawLine((int)pointA.X+1, (int)pointA.Y+1, (int)pointB.X+1, (int)pointB.Y+1, Color.FromRgb(200, 0, 200));
        }

        void myReader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            MultiSourceFrameReference multiRef = e.FrameReference;
            MultiSourceFrame multiFrame = multiRef.AcquireFrame();
            if (multiFrame == null) return;

            ColorFrameReference colorRef = multiFrame.ColorFrameReference;
            BodyFrameReference bodyRef = multiFrame.BodyFrameReference;

            using (ColorFrame colorFrame = colorRef.AcquireFrame())
            {
                using (BodyFrame bodyFrame = bodyRef.AcquireFrame())
                {
                    if (colorFrame == null || bodyFrame == null) return;
                    //Farbdaten konvertieren
                    if (myPixels == null)
                    {
                        myPixels = new byte[colorFrame.FrameDescription.Width * colorFrame.FrameDescription.Height * ((PixelFormats.Bgr32.BitsPerPixel + 7) / 8)];
                    }
                    if (colorFrame.RawColorImageFormat == ColorImageFormat.Bgra)
                    {
                        colorFrame.CopyRawFrameDataToArray(myPixels);
                    }
                    else
                    {
                        colorFrame.CopyConvertedFrameDataToArray(myPixels, ColorImageFormat.Bgra);
                    }
                    myBitmap.WritePixels(
                        new Int32Rect(0, 0, myBitmap.PixelWidth, myBitmap.PixelHeight),
                        myPixels,
                        myBitmap.PixelWidth * sizeof(int),
                        0);
                    //Handle Skeletal data
                    if (myBodies == null)
                    {
                        myBodies = new Body[bodyFrame.BodyCount];
                    }
                    bodyFrame.GetAndRefreshBodyData(myBodies);
                    foreach (Body body in myBodies)
                    {
                        if (body.IsTracked)
                        {
                            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;
                            drawABone(joints[JointType.Head], joints[JointType.SpineBase]);
                            drawABone(joints[JointType.FootLeft], joints[JointType.SpineBase]);
                            drawABone(joints[JointType.FootRight], joints[JointType.SpineBase]);
                            drawABone(joints[JointType.HandLeft], joints[JointType.SpineShoulder]);
                            drawABone(joints[JointType.HandRight], joints[JointType.SpineShoulder]);
                        }
                    }
                }
            }
        }
    }
}
