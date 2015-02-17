﻿//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace SpotifyKinectInterface
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.ComponentModel;
    using Microsoft.Kinect;
    using Microsoft.Speech.AudioFormat;
    using Microsoft.Speech.Recognition;


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Debug counter
        /// </summary>
        private int debugCounter = 0;

        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double BodyCenterThickness = 10;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly Brush centerPointBrush = Brushes.Blue;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Speech recognition engine using audio data from Kinect.
        /// </summary>
        private SpeechRecognitionEngine speechEngine;

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Draws indicators to show which edges are clipping skeleton data
        /// </summary>
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        /// <summary>
        /// Gets the metadata for the speech recognizer (acoustic model) most suitable to
        /// process audio from Kinect device.
        /// </summary>
        /// <returns>
        /// RecognizerInfo if found, <code>null</code> otherwise.
        /// </returns>
        private static RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            Image.Source = this.imageSource;

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }

            RecognizerInfo ri = GetKinectRecognizer();

            if (null != ri)
            {

                this.speechEngine = new SpeechRecognitionEngine(ri.Id);

                //Use this code to create grammar programmatically rather than from
                //a grammar file.

                var commands = new Choices();
                commands.Add(new SemanticResultValue("play", "PLAY"));
                commands.Add(new SemanticResultValue("pause", "PAUSE"));
                commands.Add(new SemanticResultValue("next", "NEXT"));
                commands.Add(new SemanticResultValue("previous", "PREVIOUS"));
                commands.Add(new SemanticResultValue("mute", "MUTE"));
                commands.Add(new SemanticResultValue("volume up", "VOLUME UP"));
                commands.Add(new SemanticResultValue("volume down", "VOLUME DOWN"));

                var gb = new GrammarBuilder { Culture = ri.Culture };
                gb.Append(commands);

                var g = new Grammar(gb);

                speechEngine.LoadGrammar(g);

                speechEngine.SpeechRecognized += SpeechRecognized;
                speechEngine.SpeechRecognitionRejected += SpeechRejected;

                // For long recognition sessions (a few hours or more), it may be beneficial to turn off adaptation of the acoustic model. 
                // This will prevent recognition accuracy from degrading over time.
                ////speechEngine.UpdateRecognizerSetting("AdaptationOn", 0);

                speechEngine.SetInputToAudioStream(
                    sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                speechEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
            else
            {
                throw new Exception();
            }

        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.AudioSource.Stop();

                this.sensor.Stop();
                this.sensor = null;
            }

            if (null != this.speechEngine)
            {
                this.speechEngine.SpeechRecognized -= SpeechRecognized;
                this.speechEngine.SpeechRecognitionRejected -= SpeechRejected;
                this.speechEngine.RecognizeAsyncStop();
            }
        }

        /// <summary>
        /// Handler for recognized speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Speech utterance confidence below which we treat speech as if it hadn't been heard
            const double ConfidenceThreshold = 0.3;

            if (e.Result.Confidence >= ConfidenceThreshold)
            {
                switch (e.Result.Semantics.Value.ToString())
                {
                    case "PLAY":
                        Console.WriteLine("Play");
                        break;
                    case "PAUSE":
                        Console.WriteLine("Pause");
                        break;
                    case "NEXT":
                        Console.WriteLine("Next");
                        break;
                    case "PREVIOUS":
                        Console.WriteLine("Previous");
                        break;
                    case "MUTE":
                        Console.WriteLine("Mute");
                        break;
                    case "VOLUME UP":
                        Console.WriteLine("Volume up");
                        break;
                    case "VOLUME DOWN":
                        Console.WriteLine("Volume down");
                        break;
                }
            }
        }

        /// <summary>
        /// Handler for rejected speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            Console.WriteLine("Rejected");
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }

        /// <summary>
        /// Dumps skeleton joint co-ordinates to debug console
        /// </summary>
        /// <param name="skeleton">skeleton to dump joint data for</param>
        private void DumpJointData(Skeleton skeleton)
        {
            System.Diagnostics.Debug.WriteLine("Head: ");
            System.Diagnostics.Debug.Write("Mapped to drawing area co-ords: ");
            System.Diagnostics.Debug.WriteLine(this.SkeletonPointToScreen(skeleton.Joints[JointType.Head].Position));
            System.Diagnostics.Debug.Write("X, Y, Z co-ords in skeleton space: ");
            System.Diagnostics.Debug.WriteLine(skeleton.Joints[JointType.Head].Position.X + " ; " + skeleton.Joints[JointType.Head].Position.X + " ; " + skeleton.Joints[JointType.Head].Position.X);
            System.Diagnostics.Debug.WriteLine("Shoulder centre: ");
            System.Diagnostics.Debug.Write("Mapped to drawing area co-ords: ");
            System.Diagnostics.Debug.WriteLine(this.SkeletonPointToScreen(skeleton.Joints[JointType.ShoulderCenter].Position));
            System.Diagnostics.Debug.Write("X, Y, Z co-ords in skeleton space: ");
            System.Diagnostics.Debug.WriteLine(skeleton.Joints[JointType.ShoulderCenter].Position.X + " ; " + skeleton.Joints[JointType.ShoulderCenter].Position.X + " ; " + skeleton.Joints[JointType.ShoulderCenter].Position.X);
            System.Diagnostics.Debug.WriteLine("Shoulder left: ");
            System.Diagnostics.Debug.Write("Mapped to drawing area co-ords: ");
            System.Diagnostics.Debug.WriteLine(this.SkeletonPointToScreen(skeleton.Joints[JointType.ShoulderLeft].Position));
            System.Diagnostics.Debug.Write("X, Y, Z co-ords in skeleton space: ");
            System.Diagnostics.Debug.WriteLine(skeleton.Joints[JointType.ShoulderLeft].Position.X + " ; " + skeleton.Joints[JointType.ShoulderLeft].Position.X + " ; " + skeleton.Joints[JointType.ShoulderLeft].Position.X);
            System.Diagnostics.Debug.WriteLine("Shoulder right: ");
            System.Diagnostics.Debug.Write("Mapped to drawing area co-ords: ");
            System.Diagnostics.Debug.WriteLine(this.SkeletonPointToScreen(skeleton.Joints[JointType.ShoulderRight].Position));
            System.Diagnostics.Debug.Write("X, Y, Z co-ords in skeleton space: ");
            System.Diagnostics.Debug.WriteLine(skeleton.Joints[JointType.ShoulderRight].Position.X + " ; " + skeleton.Joints[JointType.ShoulderRight].Position.X + " ; " + skeleton.Joints[JointType.ShoulderRight].Position.X);
            System.Diagnostics.Debug.WriteLine("Spine: ");
            System.Diagnostics.Debug.Write("Mapped to drawing area co-ords: ");
            System.Diagnostics.Debug.WriteLine(this.SkeletonPointToScreen(skeleton.Joints[JointType.Spine].Position));
            System.Diagnostics.Debug.Write("X, Y, Z co-ords in skeleton space: ");
            System.Diagnostics.Debug.WriteLine(skeleton.Joints[JointType.Spine].Position.X + " ; " + skeleton.Joints[JointType.Spine].Position.X + " ; " + skeleton.Joints[JointType.Spine].Position.X);
            System.Diagnostics.Debug.WriteLine("Hip centre: ");
            System.Diagnostics.Debug.Write("Mapped to drawing area co-ords: ");
            System.Diagnostics.Debug.WriteLine(this.SkeletonPointToScreen(skeleton.Joints[JointType.HipCenter].Position));
            System.Diagnostics.Debug.Write("X, Y, Z co-ords in skeleton space: ");
            System.Diagnostics.Debug.WriteLine(skeleton.Joints[JointType.HipCenter].Position.X + " ; " + skeleton.Joints[JointType.HipCenter].Position.X + " ; " + skeleton.Joints[JointType.HipCenter].Position.X);
            System.Diagnostics.Debug.WriteLine("Hip left: ");
            System.Diagnostics.Debug.Write("Mapped to drawing area co-ords: ");
            System.Diagnostics.Debug.WriteLine(this.SkeletonPointToScreen(skeleton.Joints[JointType.HipLeft].Position));
            System.Diagnostics.Debug.Write("X, Y, Z co-ords in skeleton space: ");
            System.Diagnostics.Debug.WriteLine(skeleton.Joints[JointType.HipLeft].Position.X + " ; " + skeleton.Joints[JointType.HipLeft].Position.X + " ; " + skeleton.Joints[JointType.HipLeft].Position.X);
            System.Diagnostics.Debug.WriteLine("Hip right: ");
            System.Diagnostics.Debug.Write("Mapped to drawing area co-ords: ");
            System.Diagnostics.Debug.WriteLine(this.SkeletonPointToScreen(skeleton.Joints[JointType.HipRight].Position));
            System.Diagnostics.Debug.Write("X, Y, Z co-ords in skeleton space: ");
            System.Diagnostics.Debug.WriteLine(skeleton.Joints[JointType.HipRight].Position.X + " ; " + skeleton.Joints[JointType.HipRight].Position.X + " ; " + skeleton.Joints[JointType.HipRight].Position.X);
            System.Diagnostics.Debug.WriteLine("Elbow left: ");
            System.Diagnostics.Debug.Write("Mapped to drawing area co-ords: ");
            System.Diagnostics.Debug.WriteLine(this.SkeletonPointToScreen(skeleton.Joints[JointType.ElbowLeft].Position));
            System.Diagnostics.Debug.Write("X, Y, Z co-ords in skeleton space: ");
            System.Diagnostics.Debug.WriteLine(skeleton.Joints[JointType.ElbowLeft].Position.X + " ; " + skeleton.Joints[JointType.ElbowLeft].Position.X + " ; " + skeleton.Joints[JointType.ElbowLeft].Position.X);
            System.Diagnostics.Debug.WriteLine("Elbow right: ");
            System.Diagnostics.Debug.Write("Mapped to drawing area co-ords: ");
            System.Diagnostics.Debug.WriteLine(this.SkeletonPointToScreen(skeleton.Joints[JointType.ElbowRight].Position));
            System.Diagnostics.Debug.Write("X, Y, Z co-ords in skeleton space: ");
            System.Diagnostics.Debug.WriteLine(skeleton.Joints[JointType.ElbowRight].Position.X + " ; " + skeleton.Joints[JointType.ElbowRight].Position.X + " ; " + skeleton.Joints[JointType.ElbowRight].Position.X);
            System.Diagnostics.Debug.WriteLine("Wrist left: ");
            System.Diagnostics.Debug.Write("Mapped to drawing area co-ords: ");
            System.Diagnostics.Debug.WriteLine(this.SkeletonPointToScreen(skeleton.Joints[JointType.WristLeft].Position));
            System.Diagnostics.Debug.Write("X, Y, Z co-ords in skeleton space: ");
            System.Diagnostics.Debug.WriteLine(skeleton.Joints[JointType.WristLeft].Position.X + " ; " + skeleton.Joints[JointType.WristLeft].Position.X + " ; " + skeleton.Joints[JointType.WristLeft].Position.X);
            System.Diagnostics.Debug.WriteLine("Wrist right: ");
            System.Diagnostics.Debug.Write("Mapped to drawing area co-ords: ");
            System.Diagnostics.Debug.WriteLine(this.SkeletonPointToScreen(skeleton.Joints[JointType.WristRight].Position));
            System.Diagnostics.Debug.Write("X, Y, Z co-ords in skeleton space: ");
            System.Diagnostics.Debug.WriteLine(skeleton.Joints[JointType.WristRight].Position.X + " ; " + skeleton.Joints[JointType.WristRight].Position.X + " ; " + skeleton.Joints[JointType.WristRight].Position.X);
            System.Diagnostics.Debug.WriteLine("Hand left: ");
            System.Diagnostics.Debug.Write("Mapped to drawing area co-ords: ");
            System.Diagnostics.Debug.WriteLine(this.SkeletonPointToScreen(skeleton.Joints[JointType.HandLeft].Position));
            System.Diagnostics.Debug.Write("X, Y, Z co-ords in skeleton space: ");
            System.Diagnostics.Debug.WriteLine(skeleton.Joints[JointType.HandLeft].Position.X + " ; " + skeleton.Joints[JointType.HandLeft].Position.X + " ; " + skeleton.Joints[JointType.HandLeft].Position.X);
            System.Diagnostics.Debug.WriteLine("Hand right: ");
            System.Diagnostics.Debug.Write("Mapped to drawing area co-ords: ");
            System.Diagnostics.Debug.WriteLine(this.SkeletonPointToScreen(skeleton.Joints[JointType.HandRight].Position));
            System.Diagnostics.Debug.Write("X, Y, Z co-ords in skeleton space: ");
            System.Diagnostics.Debug.WriteLine(skeleton.Joints[JointType.HandRight].Position.X + " ; " + skeleton.Joints[JointType.HandRight].Position.X + " ; " + skeleton.Joints[JointType.HandRight].Position.X);
            System.Diagnostics.Debug.WriteLine("Knee left: ");
            System.Diagnostics.Debug.Write("Mapped to drawing area co-ords: ");
            System.Diagnostics.Debug.WriteLine(this.SkeletonPointToScreen(skeleton.Joints[JointType.KneeLeft].Position));
            System.Diagnostics.Debug.Write("X, Y, Z co-ords in skeleton space: ");
            System.Diagnostics.Debug.WriteLine(skeleton.Joints[JointType.KneeLeft].Position.X + " ; " + skeleton.Joints[JointType.KneeLeft].Position.X + " ; " + skeleton.Joints[JointType.KneeLeft].Position.X);
            System.Diagnostics.Debug.WriteLine("Knee right: ");
            System.Diagnostics.Debug.Write("Mapped to drawing area co-ords: ");
            System.Diagnostics.Debug.WriteLine(this.SkeletonPointToScreen(skeleton.Joints[JointType.KneeRight].Position));
            System.Diagnostics.Debug.Write("X, Y, Z co-ords in skeleton space: ");
            System.Diagnostics.Debug.WriteLine(skeleton.Joints[JointType.KneeRight].Position.X + " ; " + skeleton.Joints[JointType.KneeRight].Position.X + " ; " + skeleton.Joints[JointType.KneeRight].Position.X);
            System.Diagnostics.Debug.WriteLine("Ankle left: ");
            System.Diagnostics.Debug.Write("Mapped to drawing area co-ords: ");
            System.Diagnostics.Debug.WriteLine(this.SkeletonPointToScreen(skeleton.Joints[JointType.AnkleLeft].Position));
            System.Diagnostics.Debug.Write("X, Y, Z co-ords in skeleton space: ");
            System.Diagnostics.Debug.WriteLine(skeleton.Joints[JointType.AnkleLeft].Position.X + " ; " + skeleton.Joints[JointType.AnkleLeft].Position.X + " ; " + skeleton.Joints[JointType.AnkleLeft].Position.X);
            System.Diagnostics.Debug.WriteLine("Ankle right: ");
            System.Diagnostics.Debug.Write("Mapped to drawing area co-ords: ");
            System.Diagnostics.Debug.WriteLine(this.SkeletonPointToScreen(skeleton.Joints[JointType.AnkleRight].Position));
            System.Diagnostics.Debug.Write("X, Y, Z co-ords in skeleton space: ");
            System.Diagnostics.Debug.WriteLine(skeleton.Joints[JointType.AnkleRight].Position.X + " ; " + skeleton.Joints[JointType.AnkleRight].Position.X + " ; " + skeleton.Joints[JointType.AnkleRight].Position.X);
            System.Diagnostics.Debug.WriteLine("***************************************************************************");
        }

        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);

            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;                    
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;                    
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
            if (debugCounter % 100 == 0)
            {
                this.DumpJointData(skeleton);
            }
            debugCounter++;
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }

        /// <summary>
        /// Handles the checking or unchecking of the seated mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxSeatedModeChanged(object sender, RoutedEventArgs e)
        {
            if (null != this.sensor)
            {
                if (this.checkBoxSeatedMode.IsChecked.GetValueOrDefault())
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                }
                else
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                }
            }
        }
    }
}