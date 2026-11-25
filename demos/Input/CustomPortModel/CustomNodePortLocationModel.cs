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
using System.Globalization;
using System.Xml.Linq;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;
using yWorks.GraphML;
using yWorks.Utils;

namespace yWorks.Demo.CustomPortModel
{
  /// <summary>
  ///   Symbolic constants for the five supported port locations.
  /// </summary>
  public enum PortLocation
  {
    Center,
    Top,
    Bottom,
    Right,
    Left
  }

  /// <summary>
  ///   Custom implementation of <see cref="IPortLocationModel"/> that provides
  ///   five discrete port locations, one at the node center and one at each side.
  /// </summary>
  public sealed class CustomNodePortLocationModel : IPortLocationModel
  {
    /// <summary>
    ///   Creates a new instance of <see cref="CustomNodePortLocationModel"/>.
    /// </summary>
    /// <param name="inset">The inset for port locations Top, Left, Bottom, and Right.</param>
    public CustomNodePortLocationModel(double inset = 0) {
      Inset = inset;
    }

    /// <summary>The inset for port locations on the sides of the node.</summary>
    public double Inset { get; }

    /// <summary>
    ///   Determines the actual absolute world location of the given port for the given parameter.
    /// </summary>
    /// <param name="port">The port to determine the location for.</param>
    /// <param name="locationParameter">The parameter to use.</param>
    public PointD GetLocation(IPort port, IPortLocationModelParameter locationParameter) {
      if (locationParameter is CustomNodePortLocationModelParameter modelParameter && port.Owner is INode ownerNode) {
        // If we have an actual owner node and the parameter can be really used by this model,
        // we just calculate the correct location, based on the node's layout.
        var layout = ownerNode.Layout;
        return modelParameter.Location switch {
            PortLocation.Center => layout.GetCenter(),
            PortLocation.Top => new PointD(layout.GetCenterX(), layout.Y + Inset),
            PortLocation.Bottom => new PointD(layout.GetCenterX(), layout.GetMaxY() - Inset),
            PortLocation.Right => new PointD(layout.GetMaxX() - Inset, layout.GetCenterY()),
            PortLocation.Left => new PointD(layout.X + Inset, layout.GetCenterY()),
            _ => throw new ArgumentOutOfRangeException(nameof(locationParameter), "Unknown PortLocation")
        };
      }

      // No owner node (e.g., an edge port), or parameter mismatch - return (0,0).
      return PointD.Origin;
    }

    /// <summary>
    ///   Factory method that creates a parameter for the given port that tries to match the provided location in absolute
    ///   world coordinates.
    ///   While you are free to return arbitrary implementations of <see cref="IPortLocationModelParameter"/>, you
    ///   usually want to use a specialized implementation that corresponds to your model. Here we return
    ///   <see cref="CustomNodePortLocationModelParameter"/> instances. Note that for discrete port models, you'll want to use
    ///   some discretization of the coordinate space. This means that retrieving the actual location with
    ///   <see cref="GetLocation"/> for the returned parameter does not necessarily have to provide the original coordinates
    ///   location; still, the actual location should probably be included in the coordinate subset that is mapped to the
    ///   return value (otherwise behavior will be very confusing).
    /// </summary>
    /// <param name="owner">The port owner that will own the port for which the parameter shall be created.</param>
    /// <param name="location">The location in the world coordinate system that should be matched as best as possible.</param>
    /// <returns>A new instance that can be used to describe the location of an <see cref="IPort"/> at the given owner.</returns>
    public IPortLocationModelParameter CreateParameter(IPortOwner owner, PointD location) {
      if (owner is INode ownerNode) {
        var layout = ownerNode.Layout;
        // Determine the distance of the specified location to the node layout center.
        var delta = location - layout.GetCenter();
        if (delta.VectorLength < 0.25 * Math.Min(layout.Width, layout.Height)) {
          // Nearer to the center than to the border => map to center.
          return CreateCustomParameter(PortLocation.Center);
        }

        // Map to a location on the side.
        if (Math.Abs(delta.X) > Math.Abs(delta.Y)) {
          return CreateCustomParameter(delta.X > 0 ? PortLocation.Right : PortLocation.Left);
        }
        return CreateCustomParameter(delta.Y > 0 ? PortLocation.Bottom : PortLocation.Top);
      }

      // Just return a fallback - GetLocation will ignore this if the owner is not a node.
      return CreateCustomParameter(PortLocation.Center);
    }

    /// <summary>
    ///   Factory method that creates a custom port location parameter.
    /// </summary>
    /// <param name="location">The location of the port.</param>
    public CustomNodePortLocationModelParameter CreateCustomParameter(PortLocation location) => new(this, location);

    /// <summary>
    ///   Provides a lookup context for the given combination of port and parameter.
    /// </summary>
    /// <param name="port">The port to use in the context.</param>
    public ILookup GetContext(IPort port) => Lookups.Empty;
  }

  /// <summary>
  ///   Custom implementation of <see cref="IPortLocationModelParameter"/> that is
  ///   tailored to match <see cref="CustomNodePortLocationModel"/> instances.
  ///   This implementation just stores one of the symbolic <see cref="PortLocation"/> instances.
  /// </summary>
  public sealed class CustomNodePortLocationModelParameter : IPortLocationModelParameter, ICloneable
  {
    private const string XmlNamespace =
        "http://www.yworks.com/yFiles-wpf/demos/CustomNodePortLocationModelParameter/1.0";

    /// <summary>
    ///   Creates an instance of <see cref="CustomNodePortLocationModelParameter"/>.
    /// </summary>
    public CustomNodePortLocationModelParameter(CustomNodePortLocationModel owner, PortLocation location) {
      Model = owner;
      Location = location;
    }

    /// <summary>The location of the port.</summary>
    public PortLocation Location { get; }

    /// <summary>The model instance to which this parameter belongs.</summary>
    public IPortLocationModel Model { get; }

    /// <summary>Creates a clone of this parameter object.</summary>
    public object Clone() =>
        // We have no mutable state, so return this.
        this;

    /// <summary>
    ///   Handles GraphML serialization for this parameter instance.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="args">An object that contains the event data.</param>
    public static void SerializationHandler(object? sender, HandleSerializationEventArgs args) {
      // Only serialize items that are of the specific type.
      if (args.Item is CustomNodePortLocationModelParameter modelParameter) {
        var writer = args.Writer;
        var model = (CustomNodePortLocationModel) modelParameter.Model;
        writer.WriteStartElement("CustomNodePortLocationModelParameter", XmlNamespace);
        writer.WriteAttribute(string.Empty, "Inset", string.Empty, model.Inset.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttribute(string.Empty, "PortLocation", string.Empty,
            ((int) modelParameter.Location).ToString(CultureInfo.InvariantCulture));
        writer.WriteEndElement();
        // Signal that this item is serialized.
        args.Handled = true;
      }
    }

    /// <summary>
    ///   Handles GraphML deserialization for this parameter instance.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="args">An object that contains the event data.</param>
    public static void DeserializationHandler(object? sender, HandleDeserializationEventArgs args) {
      if (args.XmlNode is XElement element && element.Name.LocalName == "CustomNodePortLocationModelParameter" &&
          element.Name.NamespaceName == XmlNamespace) {
        // Setting the result marks the event arguments as handled.
        var inset = double.Parse(element.Attribute("Inset")!.Value, CultureInfo.InvariantCulture);
        var location = (PortLocation) int.Parse(element.Attribute("PortLocation")!.Value, CultureInfo.InvariantCulture);
        args.Result = new CustomNodePortLocationModelParameter(new CustomNodePortLocationModel(inset), location);
        args.Handled = true;
      }
    }
  }
}
