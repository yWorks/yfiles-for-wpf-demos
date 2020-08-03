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
using System.Windows.Markup;
using System.Windows.Media;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;
using yWorks.GraphML;

namespace Demo.yFiles.Complete.RotatableNodes
{
  /// <summary>
  /// Port location model decorator that automatically provides the location in the rotated coordinates of the owner
  /// </summary>
  public class RotatablePortLocationModelDecorator : IPortLocationModel, IMarkupExtensionConverter
  {
    private const double Eps = 0.001d;

    /// <summary>
    /// Default instance
    /// </summary>
    public static readonly RotatablePortLocationModelDecorator Instance = new RotatablePortLocationModelDecorator();

    private IPortLocationModel wrapped = new FreeNodePortLocationModel();

    /// <summary>
    /// The wrapped location model.
    /// </summary>
    /// <remarks>
    /// This is only used when new parameters are created via <see cref="CreateParameter(yWorks.Graph.IPortOwner,yWorks.Geometry.PointD)"/>. 
    /// Default is <see cref="FreeNodePortLocationModel.Instance"/>
    /// </remarks>
    public IPortLocationModel Wrapped {
      get { return wrapped; }
      set { wrapped = value; }
    }

    /// <summary>
    /// Delegates to the wrapped location model's lookup.
    /// </summary>
    public object Lookup(Type type) {
      return Wrapped.Lookup(type);
    }

    /// <summary>
    /// Recalculates the coordinates provided by <paramref name="parameter"/>.
    /// </summary>
    /// <remarks>
    /// This has only an effect when <paramref name="parameter"/> is created by this model and the owner of
    /// <paramref name="port"/> has a <see cref="RotatableNodeStyleDecorator"/>.
    /// </remarks>
    public PointD GetLocation(IPort port, IPortLocationModelParameter parameter) {
      var param = ((RotatablePortLocationModelDecoratorParameter)parameter).Wrapped;
      var coreLocation = Wrapped.GetLocation(port, param);
      var ownerNode = port.Owner as INode;
      if (ownerNode == null) {
        return coreLocation;
      }
      var angle = GetAngle(ownerNode);

      if (Math.Abs(angle) < Eps) {
        return coreLocation;
      }
      var center = ownerNode.Layout.GetCenter();
      var rotation = new RotateTransform(-angle, center.X, center.Y);
      var result = rotation.Transform(coreLocation.ToPoint());
      return new PointD(result);
    }

    /// <summary>
    /// Creates a parameter that matches <paramref name="location"/>
    /// </summary>
    /// <remarks>
    /// This implementation undoes the rotation by the <paramref name="portOwner"/>, creates a parameter for this location in
    /// <see cref="Wrapped"/> and wraps this into a model specific parameter.
    /// </remarks>
    /// <param name="portOwner"></param>
    /// <param name="location">The actual coordinates</param>
    /// <returns></returns>
    public IPortLocationModelParameter CreateParameter(IPortOwner portOwner, PointD location) {
      double angle = 0;
      var ownerNode = portOwner as INode;
      if (ownerNode != null) {
        angle = GetAngle(ownerNode);
      }
      if (Math.Abs(angle) >= Eps) {
        //Undo the rotation by the ownerNode so that we can create a core parameter for the unrotated layout.
        var center = ownerNode.Layout.GetCenter();
        var rotation = new RotateTransform(angle, center.X, center.Y);
        var result = rotation.Transform(location.ToPoint());
        location = new PointD(result);
      }
      return new RotatablePortLocationModelDecoratorParameter(Wrapped.CreateParameter(portOwner, location), this);
    }

    /// <summary>
    /// Wrapes a given parameter so it can be automatically rotated
    /// </summary>
    /// <remarks>
    /// The <paramref name="coreParameter"/> is assumed to provide coordinates for an unrotated owner.
    /// </remarks>
    public IPortLocationModelParameter CreateWrappingParameter(IPortLocationModelParameter coreParameter) {
      return new RotatablePortLocationModelDecoratorParameter(coreParameter, this);
    }

    /// <summary>
    /// Returns the lookup of the wrapped location model.
    /// </summary>
    public ILookup GetContext(IPort port, IPortLocationModelParameter parameter) {
      return Wrapped.GetContext(port, parameter);
    }

    /// <summary>
    /// Returns that this port location model can be converted.
    /// </summary>
    public bool CanConvert(IWriteContext context, object value) {
      return true;
    }

    /// <summary>
    /// Converts this port location model using <see cref="RotatablePortLocationModelDecoratorExtension"/>.
    /// </summary>
    public MarkupExtension Convert(IWriteContext context, object value) {
      return new RotatablePortLocationModelDecoratorExtension {Wrapped = wrapped};
    }

    /// <summary>
    /// An <see cref="IPortLocationModelParameter"/> decorator for ports using <see cref="RotatablePortLocationModelDecorator"/> 
    /// to adjust the port location to the node rotation.
    /// </summary>
    private sealed class RotatablePortLocationModelDecoratorParameter : IPortLocationModelParameter, IMarkupExtensionConverter
    {
      private readonly IPortLocationModelParameter wrapped;
      private readonly RotatablePortLocationModelDecorator model;

      /// <summary>
      /// Creates a new instance wrapping the given location model parameter.
      /// </summary>
      public RotatablePortLocationModelDecoratorParameter(IPortLocationModelParameter wrapped, RotatablePortLocationModelDecorator model) {
        this.wrapped = wrapped;
        this.model = model;
      }

      #region Implementation of ICloneable

      /// <summary>
      /// Creates a copy of this location model parameter.
      /// </summary>
      public object Clone() {
        return new RotatablePortLocationModelDecoratorParameter((IPortLocationModelParameter)wrapped.Clone(), model);
      }

      #endregion

      #region Implementation of IPortLocationModelParameter

      /// <summary>
      /// The model.
      /// </summary>
      public IPortLocationModel Model {
        get { return model; }
      }

      /// <summary>
      /// The wrapped parameter.
      /// </summary>
      public IPortLocationModelParameter Wrapped {
        get { return wrapped; }
      }

      /// <summary>
      /// Accepts all port owners that are supported by the wrapped parameter.
      /// </summary>
      public bool Supports(IPortOwner portOwner) {
        return wrapped.Supports(portOwner);
      }

      #endregion

      #region Implementation of IMarkupExtensionConverter

      /// <summary>
      /// Returns that this port location model parameter can be converted.
      /// </summary>
      bool IMarkupExtensionConverter.CanConvert(IWriteContext context, object value) {
        return true;
      }

      /// <summary>
      /// Converts this port location model parameter using <see cref="RotatablePortLocationModelDecoratorParameterExtension"/>.
      /// </summary>
      MarkupExtension IMarkupExtensionConverter.Convert(IWriteContext context, object value) {
        return new RotatablePortLocationModelDecoratorParameterExtension {
          Model = model == Instance ? null : model,
          Wrapped = wrapped
        };
      }

      #endregion
    }

    /// <summary>
    /// Returns the current angle of the given rotated node.
    /// </summary>
    private static double GetAngle(INode ownerNode) {
      var ra = ownerNode.Style as RotatableNodeStyleDecorator;
      return ra != null ? ra.Angle : 0;
    }
  }


  /// <summary>
  /// Markup extension that helps (de-)serializing a <see cref="RotatablePortLocationModelDecorator"/>.
  /// </summary>
  [ContentProperty("Wrapped")]
  public class RotatablePortLocationModelDecoratorExtension : MarkupExtension
  {
    public IPortLocationModel Wrapped { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new RotatablePortLocationModelDecorator { Wrapped = Wrapped };
    }
  }

  /// <summary>
  /// Markup extension that helps (de-)serializing a <see cref="RotatablePortLocationModelDecorator.RotatablePortLocationModelDecoratorParameter"/>.
  /// </summary>
  [ContentProperty("Wrapped")]
  public sealed class RotatablePortLocationModelDecoratorParameterExtension : MarkupExtension
  {
    [DefaultValue(null)]
    public IPortLocationModel Model { get; set; }

    [DefaultValue(null)]
    public IPortLocationModelParameter Wrapped { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      RotatablePortLocationModelDecorator model = Model as RotatablePortLocationModelDecorator ?? RotatablePortLocationModelDecorator.Instance;
      return model.CreateWrappingParameter(Wrapped);
    }
  }
}
