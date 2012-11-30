using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;


using Microsoft.Kinect;

namespace ArmController.Kinect_Module
{
    /// <summary>
    /// This class provides us with a means to render a skeleton on a screen. 
    /// This is based on the example provided by the Kinect SDK.
    /// </summary>
    public class SkeletonRenderer
    {
        #region Constants
        /**********************************************************************/
        /* Constants */
        /**********************************************************************/
        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 480.0f;

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

        #endregion Constants

        #region Private Members
        /**********************************************************************/
        /* Private Members */
        /**********************************************************************/
        /// <summary>
        /// The canvas to render the skeleton on.
        /// </summary>
        public DrawingGroup Canvas { get; set; }

        /// <summary>
        /// Virtual member used to determine if the renderer is ready to draw something.
        /// </summary>
        public bool IsReady
        {
            get
            {
                return Canvas != null;
            }
        }

        /// <summary>
        /// The delegate used to resolve the 2d position of skeletal points.
        /// </summary>
        public PointResolverDelegate PointResolver { get; set; }

        #endregion Private Members

        #region Public Interfaces
        /**********************************************************************/
        /* Public Interfaces */
        /**********************************************************************/
        /// <summary>
        /// A delegate used to map the 3d position of a skeletal point onto a 2d canvas.
        /// </summary>
        /// <param name="point">The skeletal point to map.</param>
        /// <returns>The 2d position of that point on a canvas.</returns>
        public delegate Point PointResolverDelegate(SkeletonPoint point);

        #endregion Public Interfaces

        #region Constructors
        /**********************************************************************/
        /* Constructors */
        /**********************************************************************/
        /// <summary>
        /// Initializes the rendering helper.
        /// </summary>
        /// <param name="pointResolver">The delegate used to resolve the 2d position of skeletal points.</param>
        /// <param name="canvas">The canvas to draw on, or null.</param>
        public SkeletonRenderer(PointResolverDelegate pointResolver, DrawingGroup canvas = null)
        {
            PointResolver = pointResolver;
            Canvas = canvas;
        }

        #endregion Constructors

        #region Public Methods
        /**********************************************************************/
        /* Public Methods */
        /**********************************************************************/

        /// <summary>
        /// Draws a set of skeletons on the canvas.
        /// </summary>
        /// <param name="skeletons">The skeletons to draw.</param>
        public void DrawSkeletons(Skeleton[] skeletons)
        {
            //Debug.WriteLine("Drawing " + skeletons.Length + " skeletons...");
            
            using (DrawingContext context = Canvas.Open())
            {

                // Draw a transparent background to set the render size
                context.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));


                foreach (Skeleton skeleton in skeletons)
                {
                    DrawSkeleton(context, skeleton);
                }

                
            }

            // prevent drawing outside of our render area
            Canvas.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
        }

        #endregion Public Methods

        #region Private Methods
        /**********************************************************************/
        /* Private Methods */
        /**********************************************************************/

        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="context">The context for the canvas to draw on.</param>
        /// <param name="skeleton">skeleton to draw</param>
        private void DrawSkeleton(DrawingContext context, Skeleton skeleton)
        {
            // Render Torso
            DrawBone(context, skeleton.Joints[JointType.Head], skeleton.Joints[JointType.ShoulderCenter]);
            DrawBone(context, skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderLeft]);
            DrawBone(context, skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderRight]);
            DrawBone(context, skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.Spine]);
            DrawBone(context, skeleton.Joints[JointType.Spine], skeleton.Joints[JointType.HipCenter]);
            DrawBone(context, skeleton.Joints[JointType.HipCenter], skeleton.Joints[JointType.HipLeft]);
            DrawBone(context, skeleton.Joints[JointType.HipCenter], skeleton.Joints[JointType.HipRight]);

            // Left Arm
            DrawBone(context, skeleton.Joints[JointType.ShoulderLeft], skeleton.Joints[JointType.ElbowLeft]);
            DrawBone(context, skeleton.Joints[JointType.ElbowLeft], skeleton.Joints[JointType.WristLeft]);
            DrawBone(context, skeleton.Joints[JointType.WristLeft], skeleton.Joints[JointType.HandLeft]);

            // Right Arm
            DrawBone(context, skeleton.Joints[JointType.ShoulderRight], skeleton.Joints[JointType.ElbowRight]);
            DrawBone(context, skeleton.Joints[JointType.ElbowRight], skeleton.Joints[JointType.WristRight]);
            DrawBone(context, skeleton.Joints[JointType.WristRight], skeleton.Joints[JointType.HandRight]);

            // Left Leg
            DrawBone(context, skeleton.Joints[JointType.HipLeft], skeleton.Joints[JointType.KneeLeft]);
            DrawBone(context, skeleton.Joints[JointType.KneeLeft], skeleton.Joints[JointType.AnkleLeft]);
            DrawBone(context, skeleton.Joints[JointType.AnkleLeft], skeleton.Joints[JointType.FootLeft]);

            // Right Leg
            DrawBone(context, skeleton.Joints[JointType.HipRight], skeleton.Joints[JointType.KneeRight]);
            DrawBone(context, skeleton.Joints[JointType.KneeRight], skeleton.Joints[JointType.AnkleRight]);
            DrawBone(context, skeleton.Joints[JointType.AnkleRight], skeleton.Joints[JointType.FootRight]);

            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = trackedJointBrush;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    context.DrawEllipse(drawBrush, null, PointResolver(joint.Position), JointThickness, JointThickness);
                }
            }
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="context">The context for the canvas to draw on.</param>
        /// <param name="joint0">joint to start drawing from</param>
        /// <param name="joint1">joint to end drawing at</param>
        private void DrawBone(DrawingContext context, Joint joint0, Joint joint1)
        {
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

            context.DrawLine(drawPen, this.PointResolver(joint0.Position), this.PointResolver(joint1.Position));
        }

        /// <summary>
        /// Draws indicators to show which edges are clipping skeleton data
        /// </summary>
        /// <param name="context">The context for the canvas to draw on.</param>
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        private void RenderClippedEdges(DrawingContext context, Skeleton skeleton)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                context.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                context.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                context.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                context.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        #endregion Private Methods

    }
}
