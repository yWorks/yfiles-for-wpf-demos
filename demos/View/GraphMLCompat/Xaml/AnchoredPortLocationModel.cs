/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.GraphML;
using yWorks.Annotations;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;

namespace Demo.yFiles.IO.GraphML.Compat.Xaml
{
  /// <summary>
  /// A simple implementation of the <see cref="IPortLocationModel"/>
  /// that uses simple <see cref="PointD"/> and <see cref="IPoint"/> implementations 
  /// to anchor ports in the world coordinate system.
  /// </summary>
  /// <remarks>This implementation provides compatibility with yFiles WPF 2.5 and earlier.</remarks>
  [SingletonSerialization]
  public sealed class AnchoredPortLocationModel : IPortLocationModel
  {
    /// <summary>
    /// A static immutable global instance of this class.
    /// </summary>
    [GraphML(Name = "Instance")]
    public static readonly AnchoredPortLocationModel Instance = new AnchoredPortLocationModel();

    /// <summary>
    /// This implementation has nothing in its lookup and will always yield <see langword="null"/>
    /// </summary>
    public object Lookup(Type type) {
      return null;
    }

    /// <inheritdoc/>
    public PointD GetLocation(IPort port, IPortLocationModelParameter locationParameter) {
      var anchorParameter = locationParameter as AnchorParameter;
      if (anchorParameter != null) {
        return anchorParameter.anchor.ToPointD();
      } else {
        var pointAnchorParameter = locationParameter as PointAnchorParameter;
        return pointAnchorParameter != null ? pointAnchorParameter.anchor : PointD.Origin;
      }
    }

    private sealed class AnchorParameter : IPortLocationModelParameter, IMarkupExtensionConverter
    {
      private readonly IPortLocationModel model;
      internal readonly IPoint anchor;

      public AnchorParameter(IPortLocationModel model, IPoint anchor) {
        this.model = model;
        this.anchor = anchor;
      }

      public object Clone() {
        return this;
      }

      public IPortLocationModel Model {
        get { return model; }
      }

      public bool Supports(IPortOwner owner) {
        return true;
      }

      #region IMarkupExtensionConverter implementation

      bool IMarkupExtensionConverter.CanConvert(IWriteContext context, object value) {
        return true;
      }

      MarkupExtension IMarkupExtensionConverter.Convert(IWriteContext context, object value) {
        return new DynamicAnchoredParameterExtension{ Anchor = anchor };
      }

      #endregion
    }

    private sealed class PointAnchorParameter : IPortLocationModelParameter, IMarkupExtensionConverter
    {
      private readonly IPortLocationModel model;
      internal readonly PointD anchor;

      public PointAnchorParameter(IPortLocationModel model, PointD anchor) {
        this.model = model;
        this.anchor = anchor;
      }

      public object Clone() {
        return this;
      }

      public IPortLocationModel Model {
        get { return model; }
      }

      public bool Supports(IPortOwner owner) {
        return true;
      }

      #region IMarkupExtensionConverter implementation

      bool IMarkupExtensionConverter.CanConvert(IWriteContext context, object value) {
        return true;
      }

      MarkupExtension IMarkupExtensionConverter.Convert(IWriteContext context, object value) {
        return new AnchoredParameterExtension{ Anchor = anchor };
      }

      #endregion
    }

    /// <inheritdoc/>
    public IPortLocationModelParameter CreateParameter(IPortOwner owner, PointD location) {
      return CreateParameter(location);
    }

    /// <summary>
    /// Creates a parameter that fixes the port location at the given coordinates.
    /// </summary>
    /// <param name="location">The location of the port.</param>
    /// <returns>A parameter that exactly matches the provided coordinates.</returns>
    public IPortLocationModelParameter CreateParameter(PointD location) {
      return new PointAnchorParameter(this, location);
    }

    /// <summary>
    /// Creates a dynamic parameter that fixes the port location at the given coordinates.
    /// </summary>
    /// <param name="location">The location of the port.</param>
    /// <returns>A parameter that exactly matches the provided coordinates.</returns>
    [NotNull] 
    public IPortLocationModelParameter CreateDynamicParameter([NotNull] IPoint location) {
      return new AnchorParameter(this, location);
    }

    /// <inheritdoc/>
    public ILookup GetContext(IPort port, IPortLocationModelParameter locationParameter) {
      return Lookups.Empty;
    }
  }

  [GraphML(Name="AnchoredParameter")]
  public class AnchoredParameterExtension : MarkupExtension
  {
    [DefaultValue(typeof(PointD), "0,0")]
    [GraphML(Name = "Anchor")]
    public PointD Anchor { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return AnchoredPortLocationModel.Instance.CreateParameter(Anchor);
    }
  }

  [GraphML(Name="DynamicAnchoredParameter")]
  public class DynamicAnchoredParameterExtension : MarkupExtension
  {
    public DynamicAnchoredParameterExtension() {
      Anchor = PointD.Origin;
    }

    [GraphML(Name = "Anchor")]
    [DefaultValue(typeof(PointD), "0,0")]
    public IPoint Anchor { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return AnchoredPortLocationModel.Instance.CreateDynamicParameter(Anchor);
    }
  }
}
