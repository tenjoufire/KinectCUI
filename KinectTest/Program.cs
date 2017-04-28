using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace KinectTest
{
    class Program
    {
        static Body[] bodies;
        static KinectSensor kinectSensor;
        static BodyFrameReader bodyFrameReader;

        static void Main(string[] args)
        {

            //Kinectの起動
            kinectSensor = KinectSensor.GetDefault();
            kinectSensor.Open();

            CancellationTokenSource cts = new CancellationTokenSource();

            Task.WhenAll(
                Task.Run(() => ReadKeyInput(cts)),
                KinectBodyFrameReader(cts.Token)
                ).Wait();

        }

        static void ReadKeyInput(CancellationTokenSource cts)
        {
            Console.ReadLine();
            cts.Cancel();
        }

        static async Task KinectBodyFrameReader(CancellationToken ct)
        {

            bodies = new Body[kinectSensor.BodyFrameSource.BodyCount];

            Console.WriteLine("meu");
            //骨格データの取得のためのリーダを起動
            bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
            bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;

            await Task.Delay(1000);


        }
        static async void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame == null)
                {
                    Console.WriteLine("No body detected");
                    return;
                }

                // ボディデータを取得する
                bodyFrame.GetAndRefreshBodyData(bodies);

                //認識しているBodyに対して
                foreach (var body in bodies.Where(b => b.IsTracked))
                {
                    //左手のY座標を取得
                    float leftHandPosY = body.Joints[JointType.HandLeft].Position.Y;
                    //右手のY座標を取得
                    float rightHandPosY = body.Joints[JointType.HandRight].Position.Y;
                    //胸と頭のY座標を取得
                    float chestPosY = body.Joints[JointType.SpineMid].Position.Y;
                    float headPosY = body.Joints[JointType.Head].Position.Y;

                    if (leftHandPosY > chestPosY && leftHandPosY < headPosY)
                    {
                        Console.WriteLine("左手が胸から顔のあたりにある");
                    }
                    else if (leftHandPosY > headPosY)
                    {
                        Console.WriteLine("左手を挙げている");
                    }

                    if (rightHandPosY > chestPosY && rightHandPosY < headPosY)
                    {
                        Console.WriteLine("右手が胸から顔のあたりにある");
                    }
                    else if (rightHandPosY > headPosY)
                    {
                        Console.WriteLine("右手を挙げている");
                    }

                    Console.WriteLine("left:" + leftHandPosY + "right:" + rightHandPosY);

                    await Task.Delay(1000);
                    //bodyFrame.Dispose();

                }
            }
        }
    }
}
