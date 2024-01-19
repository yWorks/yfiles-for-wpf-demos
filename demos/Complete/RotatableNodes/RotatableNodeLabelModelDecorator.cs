/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Markup;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.GraphML;

namespace Demo.yFiles.Complete.RotatableNodes
{
  /// <summary>
  /// An <see cref="ILabelModel"/> decorator for node labels that wraps another label model and considers 
  /// the <see cref="RotatableNodeStyleDecorator.Angle">rotation angle</see>
  /// of the label owner when a <see cref="RotatableNodeStyleDecorator"/> is used.
  /// </summary>
  /// <remarks>
  /// This will make the node labels rotate with the node's rotation.
  /// </remarks>
  public class RotatableNodeLabelModelDecorator : ILabelModel, IMarkupExtensionConverter
  {
    /// <summary>
    /// Gets or sets if the <see cref="RotatableNodeStyleDecorator.Angle">rotation</see> of the label owner should be considered.
    /// </summary>
    public bool UseNodeRotation { get; set; }

    /// <summary>
    /// Gets or sets the wrapped label model.
    /// </summary>
    public ILabelModel Wrapped { get; set; }

    /// <summary>
    /// Creates a new instance using <see cref="FreeNodeLabelModel.Instance"/> as <see cref="Wrapped"/> label model.
    /// </summary>
    public RotatableNodeLabelModelDecorator() : this(FreeNodeLabelModel.Instance) {}

    /// <summary>
    /// Creates a new instance for the passed model.
    /// </summary>
    /// <param name="wrapped">The label model to wrap.</param>
    public RotatableNodeLabelModelDecorator(ILabelModel wrapped) {
      this.Wrapped = wrapped;
      UseNodeRotation = true;
    }

    /// <summary>
    /// Provides custom implementations of <see cref="ILabelModelParameterProvider"/> and 
    /// <see cref="ILabelModelParameterFinder"/> that consider the nodes rotation.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public object Lookup(Type type) {
      if (type == typeof (ILabelModelParameterProvider)) {
        var provider = Wrapped.Lookup<ILabelModelParameterProvider>();
        if (provider != null) {
          return new RotatedNodeLabelModelParameterProvider(provider);
        }
      }
      if (type == typeof (ILabelModelParameterFinder)) {
        var finder = Wrapped.Lookup<ILabelModelParameterFinder>();
        if (finder != null) {
          return new RotatedNodeLabelModelParameterFinder(finder);
        }
      }
      return null;
    }

    /// <summary>
    /// Returns the current geometry of the given label.
    /// </summary>
    public IOrientedRectangle GetGeometry(ILabel label, ILabelModelParameter parameter) {
      var styleWrapper = GetNodeStyleWrapper(label);
      var wrappedParameter = GetWrappedParameter(parameter);
      var orientedRectangle = wrappedParameter.Model.GetGeometry(label, wrappedParameter);
      var node = label.Owner as INode;
      if (!UseNodeRotation || node == null || styleWrapper == null || styleWrapper.Angle == 0) {
        return orientedRectangle;
      }

      var rotatedCenter = styleWrapper.GetRotatedPoint(orientedRectangle.GetCenter(), node, true);
      var rotatedLayout = styleWrapper.GetRotatedLayout(node);

      var rectangle = new OrientedRectangle(orientedRectangle);
      rectangle.Angle += rotatedLayout.GetRadians();
      rectangle.SetCenter(rotatedCenter);
      return rectangle;
    }

    /// <summary>
    /// Creates a wrapped instance of the wrapped label model's default parameter.
    /// </summary>
    public ILabelModelParameter CreateDefaultParameter() {
      return new RotatableNodeLabelModelDecoratorParameter(Wrapped.CreateDefaultParameter(), this);
    }

    /// <summary>
    /// Creates a new parameter wrapping <paramref name="wrapped"/>.
    /// </summary>
    /// <remarks>The label model of <paramref name="wrapped"/> should be the same as <see cref="Wrapped"/>.</remarks>
    /// <param name="wrapped">The parameter to wrap.</param>
    /// <returns>A parameter wrapping <paramref name="wrapped"/>.</returns>
    public ILabelModelParameter CreateWrappingParameter(ILabelModelParameter wrapped) {
      return new RotatableNodeLabelModelDecoratorParameter(wrapped, this);
    }

    /// <summary>
    /// Provides a lookup context for the given combination of label and parameter.
    /// </summary>
    public ILookup GetContext(ILabel label, ILabelModelParameter parameter) {
      var wrappedParameter = GetWrappedParameter(parameter);
      return wrappedParameter.Model.GetContext(label, wrappedParameter);
    }

    /// <summary>
    /// Returns the wrapped label model parameter.
    /// </summary>
    private ILabelModelParameter GetWrappedParameter(ILabelModelParameter parameter) {
      return ((RotatableNodeLabelModelDecoratorParameter) parameter).Wrapped;
    }

    /// <summary>
    /// Returns the wrapping style for nodes when <see cref="RotatableNodeStyleDecorator"/> is used, 
    /// <see langword="null"/> otherwise.
    /// </summary>
    private RotatableNodeStyleDecorator GetNodeStyleWrapper(ILabel label) {
      var node = label.Owner as INode;
      return node != null ? node.Style as RotatableNodeStyleDecorator : null;
    }

    /// <summary>
    /// Returns that this label model can be converted.
    /// </summary>
    public bool CanConvert(IWriteContext context, object value) {
      return true;
    }

    /// <summary>
    /// Converts this label model using <see cref="RotatableNodeLabelModelDecoratorExtension"/>.
    /// </summary>
    public MarkupExtension Convert(IWriteContext context, object value) {
      return new RotatableNodeLabelModelDecoratorExtension {Wrapped = Wrapped, UseNodeRotation = UseNodeRotation};
    }

    /// <summary>
    /// A <see cref="ILabelModelParameter"/> decorator for node labels using 
    /// <see cref="RotatableNodeLabelModelDecorator"/> to adjust the label rotation to the node rotation.
    /// </summary>
    private sealed class RotatableNodeLabelModelDecoratorParameter : ILabelModelParameter, IMarkupExtensionConverter
    {
      /// <summary>
      /// Creates a new instance wrapping the given parameter.
      /// </summary>
      public RotatableNodeLabelModelDecoratorParameter(ILabelModelParameter wrapped, ILabelModel model) {
        Wrapped = wrapped;
        Model = model;
      }

      /// <summary>
      /// Returns a copy of this label model parameter.
      /// </summary>
      public object Clone() {
        return new RotatableNodeLabelModelDecoratorParameter(Wrapped, Model);
      }

      /// <summary>
      /// Returns the label model.
      /// </summary>
      public ILabelModel Model { get; private set; }

      /// <summary>
      /// Returns the wrapped label model parameter.
      /// </summary>
      public ILabelModelParameter Wrapped { get; private set; }

      /// <summary>
      /// Accepts node labels that are supported by the wrapped label model parameter.
      /// </summary>
      public bool Supports(ILabel label) {
        return label.Owner is INode && Wrapped.Supports(label);
      }

      #region Implementation of IMarkupExtensionConverter

      /// <summary>
      /// Returns that this label model parameter can be converted.
      /// </summary>
      public bool CanConvert(IWriteContext context, object value) {
        return true;
      }

      /// <summary>
      /// Converts this label model parameter using <see cref="RotatableNodeLabelModelDecoratorParameterExtension"/>.
      /// </summary>
      public MarkupExtension Convert(IWriteContext context, object value) {
        return new RotatableNodeLabelModelDecoratorParameterExtension {Model = Model, Wrapped = Wrapped};
      }

      #endregion
    }

    /// <summary>
    /// Provides candidate parameters for rotated label models.
    /// </summary>
    private sealed class RotatedNodeLabelModelParameterProvider : ILabelModelParameterProvider
    {

      private readonly ILabelModelParameterProvider wrappedProvider;

      /// <summary>
      /// Creates a new instance using the given parameter provider.
      /// </summary>
      public RotatedNodeLabelModelParameterProvider(ILabelModelParameterProvider wrappedProvider) {
        this.wrappedProvider = wrappedProvider;
      }

      /// <summary>
      /// Returns a set of possible wrapped <see cref="ILabelModelParameter"/> instances.
      /// </summary>
      public IEnumerable<ILabelModelParameter> GetParameters(ILabel label, ILabelModel model) {
        var wrapperModel = model as RotatableNodeLabelModelDecorator;
        var parameters = wrappedProvider.GetParameters(label, wrapperModel.Wrapped);
        var result = new List<ILabelModelParameter>();
        foreach (var parameter in parameters) {
          result.Add(wrapperModel.CreateWrappingParameter(parameter));
        }
        return result;
      }
    }

    /// <summary>
    /// Finds the best <see cref="ILabelModelParameter"/> to approximate a specific rotated layout.
    /// </summary>
    private sealed class RotatedNodeLabelModelParameterFinder : ILabelModelParameterFinder
    {
      private readonly ILabelModelParameterFinder wrappedFinder;

      /// <summary>
      /// Creates a new instance using the given parameter finder.
      /// </summary>
      public RotatedNodeLabelModelParameterFinder(ILabelModelParameterFinder wrappedFinder) {
        this.wrappedFinder = wrappedFinder;
      }

      /// <summary>
      /// Finds the label model parameter that describes the given label layout best.
      /// </summary>
      /// <remarks>
      /// Sometimes the layout cannot be met exactly, then the nearest location is used.
      /// </remarks>
      public ILabelModelParameter FindBestParameter(ILabel label, ILabelModel model, IOrientedRectangle labelLayout) {
        var wrapperModel = model as RotatableNodeLabelModelDecorator;
        var styleWrapper = wrapperModel.GetNodeStyleWrapper(label);
        if (!wrapperModel.UseNodeRotation || styleWrapper == null || styleWrapper.Angle == 0) {
          return
              wrapperModel.CreateWrappingParameter(wrappedFinder.FindBestParameter(label, wrapperModel.Wrapped,
                  labelLayout));
        }

        var node = label.Owner as INode;
        var rotatedCenter = styleWrapper.GetRotatedPoint(labelLayout.GetCenter(), node, false);
        var rotatedLayout = styleWrapper.GetRotatedLayout(node);

        var rectangle = new OrientedRectangle(labelLayout);
        rectangle.Angle -= rotatedLayout.GetRadians();
        rectangle.SetCenter(rotatedCenter);

        return
            wrapperModel.CreateWrappingParameter(wrappedFinder.FindBestParameter(label, wrapperModel.Wrapped, rectangle));
      }
    }
  }

  /// <summary>
  /// Markup extension that helps (de-)serializing a <see cref="RotatableNodeLabelModelDecorator"/>.
  /// </summary>
  [ContentProperty("Wrapped")]
  public class RotatableNodeLabelModelDecoratorExtension : MarkupExtension
  {
    /// <summary>
    /// Gets or sets if the <see cref="RotatableNodeStyleDecorator.Angle">rotation</see> of the label owner should be considered.
    /// </summary>
    [DefaultValue(true)]
    public bool UseNodeRotation { get; set; }

    /// <summary>
    /// Gets or sets the wrapped label model.
    /// </summary>
    public ILabelModel Wrapped { get; set; }

    public RotatableNodeLabelModelDecoratorExtension() {
      UseNodeRotation = true;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return new RotatableNodeLabelModelDecorator(Wrapped) {UseNodeRotation = UseNodeRotation};
    }
  }

  /// <summary>
  /// Markup extension that helps (de-)serializing parameters created by a <see cref="RotatableNodeLabelModelDecorator"/>.
  /// </summary>
  [ContentProperty("Wrapped")]
  public class RotatableNodeLabelModelDecoratorParameterExtension : MarkupExtension
  {
    /// <summary>
    /// The label model.
    /// </summary>
    public ILabelModel Model { get; set; }

    /// <summary>
    /// The wrapped label model parameter.
    /// </summary>
    public ILabelModelParameter Wrapped { get; set; }

    #region Overrides of MarkupExtension

    public override object ProvideValue(IServiceProvider serviceProvider) {
      var rotatingModel = Model as RotatableNodeLabelModelDecorator;
      return rotatingModel != null ? rotatingModel.CreateWrappingParameter(Wrapped) : Wrapped;
    }

    #endregion
  }
}
