/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.1.
 ** Copyright (c) 2000-2026 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;
using yWorks.Utils;

namespace yWorks.Demo.PortCandidateProvider
{
  /// <summary>
  ///   Custom implementation of <see cref="IPortLocationModel"/> that provides
  ///   port locations on the shape boundary.
  /// </summary>
  public sealed class ShapeBoundaryPortLocationModel : IPortLocationModel
  {
    /// <summary>
    ///   Determines the actual absolute world location of the given port for the given parameter.
    /// </summary>
    /// <param name="port">The port to determine the location for.</param>
    /// <param name="locationParameter">The parameter to use.</param>
    public PointD GetLocation(IPort port, IPortLocationModelParameter locationParameter) {
      if (locationParameter is ShapeBoundaryPortLocationModelParameter modelParameter &&
          port.Owner is INode ownerNode) {
        var layout = ownerNode.Layout;
        return new PointD(modelParameter.Location.X * layout.Width + layout.X,
            modelParameter.Location.Y * layout.Height + layout.Y);
      }

      // no owner node (e.g., an edge port), or parameter mismatch - return (0,0)
      return PointD.Origin;
    }

    /// <summary>
    ///   Factory method that creates a parameter for the given port that tries to match the provided
    ///   location in absolute world coordinates.
    ///   While you are free to return arbitrary implementations of <see cref="IPortLocationModelParameter"/>, you
    ///   usually want to use a specialized implementation that corresponds to your model. Here we return
    ///   <see cref="ShapeBoundaryPortLocationModelParameter"/> instances.
    /// </summary>
    /// <param name="owner">The port owner that will own the port for which the parameter shall be created.</param>
    /// <param name="location">
    ///   The location in the world coordinate system that should be matched as
    ///   best as possible.
    /// </param>
    /// <returns>
    ///   A new instance that can be used to describe the location of
    ///   an <see cref="IPort"/> at the given owner.
    /// </returns>
    public IPortLocationModelParameter CreateParameter(IPortOwner owner, PointD location) {
      if (owner is INode ownerNode) {
        // We calculate the intersection of the ray starting from the center of the node and
        // going through the given location with the style's outline geometry.
        var layout = ownerNode.Layout;
        var center = layout.GetCenter();
        var style = ownerNode.Style;
        var outline = style.Renderer.GetShapeGeometry(ownerNode, style).GetOutline();

        if (outline != null) {
          var ray = location - center;
          // Move intersection slightly into the outline shape to ensure edge cropping always works
          var intersection = outline.FindRayIntersection(center, ray) * 0.9999;
          if (intersection != double.PositiveInfinity) {
            var intersectionLocation = new PointD(center.X + ray.X * intersection, center.Y + ray.Y * intersection);
            var relativeToNode = new PointD(intersectionLocation.X - layout.X, intersectionLocation.Y - layout.Y);
            return CreateCustomParameter(new PointD(relativeToNode.X / layout.Width, relativeToNode.Y / layout.Height));
          }
        }

        return CreateCustomParameter(new PointD(0.5, 0.5));
      }

      // Just return a fallback - GetLocation will ignore this anyway if the owner is null or not a node.
      return CreateCustomParameter(PointD.Origin);
    }

    /// <summary>
    ///   Factory method that creates a custom port location parameter.
    /// </summary>
    /// <param name="location">The normalized location of the port within the node bounds.</param>
    public ShapeBoundaryPortLocationModelParameter CreateCustomParameter(PointD location) => new(this, location);

    /// <summary>
    ///   Provides a lookup context for the given combination of port and parameter.
    /// </summary>
    /// <param name="port">The port to use in the context.</param>
    public ILookup GetContext(IPort port) => Lookups.Empty;
  }

  /// <summary>
  ///   Custom implementation of <see cref="IPortLocationModelParameter"/> that is
  ///   tailored to match <see cref="ShapeBoundaryPortLocationModel"/> instances.
  ///   This implementation stores the normalized location within the node bounds.
  /// </summary>
  public sealed class ShapeBoundaryPortLocationModelParameter : IPortLocationModelParameter, ICloneable
  {
    private readonly ShapeBoundaryPortLocationModel owner;

    /// <summary>
    ///   Creates an instance of <see cref="ShapeBoundaryPortLocationModelParameter"/>.
    /// </summary>
    public ShapeBoundaryPortLocationModelParameter(ShapeBoundaryPortLocationModel owner, PointD location) {
      this.owner = owner;
      Location = location;
    }

    /// <summary>The normalized location within the node bounds.</summary>
    public PointD Location { get; }

    /// <summary>
    ///   Returns a model instance to which this parameter belongs.
    ///   This is usually a reference to the model instance that has created this parameter.
    /// </summary>
    public IPortLocationModel Model => owner;

    /// <summary>
    ///   Creates a clone of this <see cref="ShapeBoundaryPortLocationModelParameter"/> object.
    /// </summary>
    public object Clone() =>
        // we have no mutable state, so return this.
        this;
  }
}
