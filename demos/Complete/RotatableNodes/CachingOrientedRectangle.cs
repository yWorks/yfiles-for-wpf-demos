/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
 ** 72070 Tuebingen, Germany. All rights reserved.
 ** 
 ** yFiles demo files exhibit yFiles WPF functionalities. Any redistribution
 ** of demo files in source code or binary form, with or without
 ** modification, is not permitted.
 ** 
 ** Owners of a valid software license for a yFiles WPF version that this
 ** demo is shipped with are allowed to use the demo source code as basis
 ** for their own yFiles WPF powered applications. Use of such programs is
 ** governed by the rights and conditions as set out in the yFiles WPF
 ** license agreement.
 ** 
 ** THIS SOFTWARE IS PROVIDED ''AS IS'' AND ANY EXPRESS OR IMPLIED
 ** WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 ** MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 ** NO EVENT SHALL yWorks BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 ** SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
 ** TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 ** PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 ** LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 ** NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 ** SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ** 
 ***************************************************************************/

using System;
using System.ComponentModel;
using yWorks.Geometry;

namespace Demo.yFiles.Complete.RotatableNodes
{
  /// <summary>
  /// An oriented rectangle that specifies the location, size and rotation angle of a rotated node.
  /// </summary>
  /// <remarks>
  /// This class is used mainly for performance reasons. It provides cached values. In principle, it would be enough to 
  /// store just the rotation angle but then, we would have to recalculate all the properties of this class very often.
  /// </remarks>
  public class CachingOrientedRectangle : IOrientedRectangle
  {
    private readonly OrientedRectangle cachedOrientedRect;
    private RectD cachedLayout;
    private double angle;
    private PointD upVector;

    /// <summary>
    /// Creates a new instance with an empty layout.
    /// </summary>
    public CachingOrientedRectangle() : this(RectD.Empty) {}

    /// <summary>
    /// Creates a new instance with the given layout.
    /// </summary>
    public CachingOrientedRectangle(RectD layout) {
      upVector = new PointD(0, -1);
      angle = 0.0;
      cachedLayout = layout;
      cachedOrientedRect = new OrientedRectangle(cachedLayout);
    }

    /// <summary>
    /// Gets or sets the rotation angle.
    /// </summary>
    [DefaultValue(0d)]
    public double Angle {
      get { return angle; }
      set {
        angle = NormalizeAngle(value);
        cachedOrientedRect.Angle = ToRadians(value);
        cachedOrientedRect.SetCenter(cachedLayout.Center);
        upVector = cachedOrientedRect.GetUp();
      }
    }

    /// <summary>
    /// The width of the rectangle.
    /// </summary>
    public double Width { get { return cachedLayout.Width; } }

    /// <summary>
    /// The height of the rectangle.
    /// </summary>
    public double Height { get { return cachedLayout.Height; } }

    /// <summary>
    /// Returns the x-coordinate of the rectangle's anchor point.
    /// </summary>
    public double AnchorX { get { return cachedOrientedRect.AnchorX; } }

    /// <summary>
    /// Returns the y-coordinate of the rectangle's anchor point.
    /// </summary>
    public double AnchorY { get { return cachedOrientedRect.AnchorY; } }

    /// <summary>
    /// Returns the x-coordinate of the rectangle's up vector.
    /// </summary>
    public double UpX { get { return cachedOrientedRect.UpX; } }

    /// <summary>
    /// Returns the y-coordinate of the rectangle's up vector.
    /// </summary>
    public double UpY { get { return cachedOrientedRect.UpY; } }

    /// <summary>
    /// Returns the rectangle's up vector.
    /// </summary>
    public PointD UpVector {
      get { return upVector; }
      set {
        upVector = value;
        cachedOrientedRect.SetUpVector(value.X, value.Y);
        cachedOrientedRect.SetCenter(cachedLayout.Center);
        angle = ToDegrees(cachedOrientedRect.Angle);
      }
    }

    /// <summary>
    /// Returns the angle in radians.
    /// </summary>
    public double GetRadians() {
      return ToRadians(angle);
    }

    /// <summary>
    /// Updates the layout in the cache.
    /// </summary>
    public void UpdateCache(RectD layout) {
      if (layout.Equals(cachedLayout) && upVector.Equals(cachedOrientedRect.GetUp())) {
        return;
      }
      cachedLayout = layout;
      cachedOrientedRect.SetUpVector(upVector.X, upVector.Y);
      cachedOrientedRect.Width = Width;
      cachedOrientedRect.Height = Height;
      cachedOrientedRect.SetCenter(cachedLayout.Center);
    }

    /// <summary>
    /// Normalizes the angle to 0–360°.
    /// </summary>
    internal static double NormalizeAngle(double angle) {
      angle %= 360;
      if (angle < 0) angle += 360;
      return angle;
    }
    
    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    internal static double ToRadians(double degrees) {
      return degrees / 180 * Math.PI;
    }

    /// <summary>
    /// Converts radians to degrees.
    /// </summary>
    internal static double ToDegrees(double radians) {
      return radians * 180 / Math.PI;
    }
  }
}
