using Microsoft.Kinect;
using System.Collections.Generic;
using System.Linq;

namespace HandtrackLibraryUtilities
{
    /// <summary>
    /// Represents a hand.
    /// </summary>
    public class Hand
    {
        /// <summary>
        /// The tracking ID of the body the current hand belongs to.
        /// </summary>
        public ulong TrackingId { get; protected set; }

        /// <summary>
        /// A list of the detected fingers.
        /// </summary>
        public IList<Finger> Fingers { get; protected set; }

        /// <summary>
        /// A list of the contour points in the 3D Camera space.
        /// </summary>
        public IList<CameraSpacePoint> ContourCamera { get; protected set; }

        /// <summary>
        /// A list of the contour points in the 2D Depth space.
        /// </summary>
        public IList<DepthSpacePoint> ContourDepth { get; protected set; }

        /// <summary>
        /// A list of the countour points in the 2D Color space.
        /// </summary>
        public IList<ColorSpacePoint> ContourColor { get; protected set; }

        public DepthSpacePoint HandPointDepth { get; protected set; }

        public IList<DepthSpacePoint> Range { get; protected set; }
        public float Radius { get; protected set; }

        /// <summary>
        /// Deprecated no handstate needed
        /// </summary>
        /// <param name="trackingID"></param>
        /// <param name="state"></param>
        /// <param name="contour"></param>
        /// <param name="fingers"></param>
        /// <param name="coordinateMapper"></param>
        /// <param name="handPosition"></param>
        /// <param name="range"></param>
        internal Hand(ulong trackingID, HandState state, IList<DepthPointEx> contour, IList<DepthPointEx> fingers, CoordinateMapper coordinateMapper, DepthPointEx handPosition, IList<DepthPointEx> range)
        {
            TrackingId = trackingID;

            Fingers = fingers.Select(f => new Finger(f, coordinateMapper)).ToList();

            ushort[] depths = contour.Select(d => (ushort)d.Z).ToArray();

            ContourDepth = contour.Select(p => new DepthSpacePoint { X = p.X, Y = p.Y }).ToArray();

            ContourCamera = new CameraSpacePoint[ContourDepth.Count];
            coordinateMapper.MapDepthPointsToCameraSpace((DepthSpacePoint[])ContourDepth, depths, (CameraSpacePoint[])ContourCamera);

            ContourColor = new ColorSpacePoint[ContourDepth.Count];
            coordinateMapper.MapDepthPointsToColorSpace((DepthSpacePoint[])ContourDepth, depths, (ColorSpacePoint[])ContourColor);

            HandPointDepth = new DepthSpacePoint { X = handPosition.X, Y = handPosition.Y };

            Range = range.Select(p => new DepthSpacePoint { X = p.X, Y = p.Y }).ToArray();
        }

        internal Hand(ulong trackingID, IList<DepthPointEx> contour, IList<DepthPointEx> fingers, CoordinateMapper coordinateMapper, DepthPointEx handPosition, IList<DepthPointEx> range)
        {
            TrackingId = trackingID;

            Fingers = fingers.Select(f => new Finger(f, coordinateMapper)).ToList();

            ushort[] depths = contour.Select(d => (ushort)d.Z).ToArray();

            ContourDepth = contour.Select(p => new DepthSpacePoint { X = p.X, Y = p.Y }).ToArray();

            ContourCamera = new CameraSpacePoint[ContourDepth.Count];
            coordinateMapper.MapDepthPointsToCameraSpace((DepthSpacePoint[])ContourDepth, depths, (CameraSpacePoint[])ContourCamera);

            ContourColor = new ColorSpacePoint[ContourDepth.Count];
            coordinateMapper.MapDepthPointsToColorSpace((DepthSpacePoint[])ContourDepth, depths, (ColorSpacePoint[])ContourColor);

            HandPointDepth = new DepthSpacePoint { X = handPosition.X, Y = handPosition.Y };

            Range = range.Select(p => new DepthSpacePoint { X = p.X, Y = p.Y }).ToArray();
        }
    }
}