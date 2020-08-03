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
using System.Globalization;
using System.Windows.Markup;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;

// specify xml namespaces and prefixes to use in GraphML for the classes in this namespace
[assembly: XmlnsDefinition("http://www.yworks.com/yfiles-for-silverlight/2.0/demos/CustomPortModel", "Demo.yFiles.Graph.CustomPortModel")]
[assembly: XmlnsPrefix("http://www.yworks.com/yfiles-for-silverlight/2.0/demos/CustomPortModel", "demo")]

namespace Demo.yFiles.Graph.CustomPortModel
{
  /// <summary>
  /// Symbolic constants for the five supported port locations.
  /// </summary>
  public enum PortLocation
  {
    Center, North, South, East, West
  }

  /// <summary>
  /// Custom implementation of <see cref="IPortLocationModel"/> that provides five discrete port locations, one at the node center and one at each side.
  /// </summary>
  public class MyNodePortLocationModel : IPortLocationModel
  {
    private double inset;

    /// <summary>
    /// The inset of the port location, i.e. the distance to the node layout borders.
    /// </summary>
    /// <remarks>This is ignored for the <see cref="PortLocation.Center"/> position.</remarks>
    [DefaultValue(0.0d)]
    public double Inset {
      get { return inset; }
      set { inset = value; }
    }

    public object Lookup(Type type) {
      return null;
    }

    /// <summary>
    /// Determines the actual absolute world location of the port for the given parameter.
    /// </summary>
    /// <param name="port">The port to determine the location for.</param>
    /// <param name="locationParameter">The parameter to use.</param>
    /// <returns>The calculated location of the port.</returns>
    public PointD GetLocation(IPort port, IPortLocationModelParameter locationParameter) {
      var modelParameter = locationParameter as MyNodePortLocationModelParameter;
      var ownerNode = port.Owner as INode;
      if (modelParameter != null && ownerNode != null) {
        //If we have an actual owner node and the parameter can be really used by this model,
        //we just calculate the correct location, based on the node's layout.
        var layout = ownerNode.Layout;
        switch(modelParameter.Location) {
          case PortLocation.Center:
            return layout.GetCenter();
          case PortLocation.North:
            return (layout.GetTopLeft() + layout.GetTopRight())*0.5d + new PointD(0, Inset);
          case PortLocation.South:
            return (layout.GetBottomLeft() + layout.GetBottomRight()) * 0.5d + new PointD(0, -Inset);
          case PortLocation.East:
            return (layout.GetTopRight() + layout.GetBottomRight()) * 0.5d + new PointD(-Inset, 0);
          case PortLocation.West:
            return (layout.GetTopLeft() + layout.GetBottomLeft()) * 0.5d + new PointD(Inset, 0);
          default:
            throw new ArgumentOutOfRangeException();
        }
      } else {
        //No owner node (e.g. an edge port), or parameter mismatch - return (0,0)
        return PointD.Origin;
      }
    }

    /// <summary>
    /// Factory method that creates a parameter for the given port that tries to match the provided location
    /// in absolute world coordinates.
    /// </summary>
    /// <remarks>While you are free to return arbitrary implementations of <see cref="IPortLocationModelParameter"/>, you usually want to 
    /// use a specialized implementation that corresponds to your model, here we return <see cref="MyNodePortLocationModelParameter"/> instances. Note that
    /// for discrete port models, you'll want to use some discretization of the coordinate space. This means that retrieving the actual location with
    /// <see cref="GetLocation"/> with the returned value does not necessarily have to provide the original coordinates <paramref name="location"/>
    /// still, the actual location should probably
    /// be included in the coordinate subset that is mapped to the return value (otherwise behaviour will be very confusing)</remarks>
    /// <param name="owner">The port owner that will own the port for which the parameter shall be created.</param>
    /// <param name="location">The location in the world coordinate system that should be matched as best as possible.</param>
    /// <returns>A new instance that can be used to describe the location of an <see cref="IPort"/> at the given
    /// <paramref name="owner"/>.</returns>
    public IPortLocationModelParameter CreateParameter(IPortOwner owner, PointD location) {
      var ownerNode = owner as INode;
      if (ownerNode != null) {
        //determine the distance of the specified location to the node layout center
        var delta = location - ownerNode.Layout.GetCenter();
        if (delta.VectorLength < 0.25d * Math.Min(ownerNode.Layout.Width, ownerNode.Layout.Height)) {
          //nearer to the center than to the border => map to center
          return CreateParameter(PortLocation.Center);
        } else {
          //map to a location on the side
          if (Math.Abs(delta.X) > Math.Abs(delta.Y)) {
            return CreateParameter(delta.X > 0 ? PortLocation.East : PortLocation.West);
          } else {
            return CreateParameter(delta.Y > 0 ? PortLocation.South : PortLocation.North);
          }
        }
      } else {
        //Just return  a fallback  - GetLocation will ignore this anyway if the owner is null or not a node.
        return CreateParameter(PortLocation.Center);
      }
    }

    public IPortLocationModelParameter CreateParameter(PortLocation location) {
      return new MyNodePortLocationModelParameter(this, location);
    }

    public ILookup GetContext(IPort label, IPortLocationModelParameter locationParameter) {
      return Lookups.Empty;
    }

    /// <summary>
    /// Custom implementation of <see cref="IPortLocationModelParameter"/> that is tailored to match <see cref="MyNodePortLocationModel"/> instances.
    /// </summary>
    /// <remarks>This implementation just stores one of the symbolic <see cref="PortLocation"/> instances.</remarks>
    [TypeConverter(typeof(MyNodePortLocationModelParameterConverter))]
    internal class MyNodePortLocationModelParameter : IPortLocationModelParameter
    {
      private readonly MyNodePortLocationModel owner;
      private readonly PortLocation location;

      public MyNodePortLocationModelParameter(MyNodePortLocationModel owner, PortLocation location) {
        this.owner = owner;
        this.location = location;
      }

      internal PortLocation Location {
        get { return location; }
      }

      public object Clone() {
        // we have no mutable state, so return this.
        return this;
      }

      /// <summary>
      /// Return a model instance where this parameter belongs to.
      /// </summary>
      /// <remarks>This is usually a reference to the model instance that has created this parameter.</remarks>
      public IPortLocationModel Model {
        get { return owner; }
      }

      /// <summary>
      /// Predicate that checks if this parameter instance may be used to describe ports for <paramref name="owner"/>
      /// </summary>
      /// <remarks>Our model/parameter implementation only makes sense when used for <see cref="INode"/>s.</remarks>
      public bool Supports(IPortOwner owner) {
        return owner is INode;
      }
    }
  }

  /// <summary>
  /// This helper class needs to be public in order to be accessable by GraphML's reflection code.
  /// </summary>
  public sealed class MyNodePortLocationModelParameterConverter : TypeConverter
  {
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
      return destinationType == typeof(MarkupExtension) || base.CanConvertTo(context, destinationType);
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
      if (destinationType == typeof(MarkupExtension) && value is MyNodePortLocationModel.MyNodePortLocationModelParameter) {
        var parameter = (MyNodePortLocationModel.MyNodePortLocationModelParameter)value;
        return new MyNodePortLocationModelParameterExtension { Model = (MyNodePortLocationModel)parameter.Model, Location = parameter.Location };
      } else {
        return base.ConvertTo(context, culture, value, destinationType);
      }
    }
  }


  /// <summary>
  /// This helper class needs to be public in order to be accessable by GraphML's reflection code
  /// </summary>
  /// <remarks>Since <see cref="MyNodePortLocationModel.MyNodePortLocationModelParameter"/> is internal, we need to provice a public
  /// <see cref="MarkupExtension"/> support class for serialization.</remarks>
  [MarkupExtensionReturnType(typeof(IPortLocationModelParameter))]
  public sealed class MyNodePortLocationModelParameterExtension : MarkupExtension {
    
    [DefaultValue(null)]
    public MyNodePortLocationModel Model { get; set; }
    
    [DefaultValue(PortLocation.Center)]
    public PortLocation Location { get; set; }
    
    public override object ProvideValue(IServiceProvider serviceProvider) {
      return (Model ?? new MyNodePortLocationModel()).CreateParameter(Location);
    }
  }
}