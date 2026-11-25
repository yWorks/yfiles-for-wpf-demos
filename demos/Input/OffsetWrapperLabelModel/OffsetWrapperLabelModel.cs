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
using System.Collections.Generic;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Utils;

namespace yWorks.Demo.OffsetWrapperLabelModel
{
  /// <summary>
  ///   Custom implementation of <see cref="ILabelModel"/> that provides label positions with an adjustable
  ///   offset relative to that of the wrapped label model.
  /// </summary>
  public class OffsetWrapperLabelModel : ILabelModel
  {
    // The distance the label will be offset from its original position.

    internal double WorldOffset { get; }

    /// <summary>
    ///   Constructs an OffsetWrapperLabelModel with an optional offset value.
    /// </summary>
    /// <param name="worldOffset">The offset distance to apply to the label position. Default is 0.</param>
    public OffsetWrapperLabelModel(double worldOffset = 0) {
      this.WorldOffset = worldOffset;
    }

    /// <summary>
    ///   Calculates the geometry of the label by applying an offset to the position
    ///   determined by the wrapped model parameter.
    /// </summary>
    public IOrientedRectangle GetGeometry(ILabel label, ILabelModelParameter layoutParameter) {
      if (layoutParameter is not OffsetWrapperLabelModelParameter parameter) {
        return OrientedRectangle.Empty;
      }

      // Use the wrapped model to get the original label position.
      var innerModel = parameter.WrappedModelParameter.Model;
      var innerParameterLayout = innerModel.GetGeometry(label, parameter.WrappedModelParameter);

      // Convert the resulting layout to a mutable rectangle for applying the offset.
      var mutableLayout = innerParameterLayout.ToOrientedRectangle();

      // Adjust the label's position by moving it in the direction of its up vector by the offset value.
      var upVector = mutableLayout.GetUpVector();
      mutableLayout.AnchorX += upVector.X * WorldOffset;
      mutableLayout.AnchorY += upVector.Y * WorldOffset;
      return mutableLayout;
    }

    /// <summary>
    ///   Provides a lookup context for the given combination of label and parameter.
    /// </summary>
    public ILookup GetContext(ILabel label) {
      // Provide a lookup that delegates parameter-related services to the wrapped model.
      if (label.LayoutParameter is OffsetWrapperLabelModelParameter parameter) {
        var innerModel = parameter.WrappedModelParameter.Model;
        var innerLookup = innerModel.GetContext(label);
        return new OffsetWrapperLabelModelLookup(this, innerLookup);
      }

      // Fallback if we cannot detect our wrapper parameter.
      return Lookups.Empty;
    }

    /// <summary>
    ///   Creates a parameter that wraps an existing label model parameter with offset capabilities.
    /// </summary>
    public ILabelModelParameter CreateParameter(ILabelModelParameter innerParameter) =>
        new OffsetWrapperLabelModelParameter(this, innerParameter);
  }

  /// <summary>
  ///   Custom implementation of <see cref="ILabelModelParameter"/> that wraps around another
  ///   ILabelModelParameter and applies an offset to the calculated label position from that model.
  /// </summary>
  internal sealed class OffsetWrapperLabelModelParameter : ILabelModelParameter
  {
    /// <summary>
    ///   Constructs a new instance of <see cref="OffsetWrapperLabelModelParameter"/>.
    /// </summary>
    /// <param name="model">The model for this parameter.</param>
    /// <param name="wrappedModelParameter">The parameter of the label model being augmented.</param>
    public OffsetWrapperLabelModelParameter(OffsetWrapperLabelModel model, ILabelModelParameter wrappedModelParameter) {
      Model = model;
      WrappedModelParameter = wrappedModelParameter;
    }

    public OffsetWrapperLabelModel Model { get; }

    ILabelModel ILabelModelParameter.Model => Model;

    public ILabelModelParameter WrappedModelParameter { get; set; }

    /// <summary>
    ///   Creates a clone of this <see cref="OffsetWrapperLabelModelParameter"/> object.
    /// </summary>
    public object Clone() {
      var inner = (ILabelModelParameter) WrappedModelParameter.Clone();
      return ReferenceEquals(inner, WrappedModelParameter) ? this : new OffsetWrapperLabelModelParameter(Model, inner);
    }
  }

  internal sealed class OffsetWrapperLabelModelLookup : ILookup, ILabelModelParameterProvider,
      ILabelModelParameterFinder
  {
    private readonly OffsetWrapperLabelModel model;
    private readonly ILookup innerLookup;

    public OffsetWrapperLabelModelLookup(OffsetWrapperLabelModel model, ILookup innerLookup) {
      this.model = model;
      this.innerLookup = innerLookup;
    }

    public object? Lookup(Type type) {
      if (type == typeof(ILabelModelParameterProvider) && innerLookup.Lookup<ILabelModelParameterProvider>() != null) {
        return this;
      }
      // Delegate finder: invert our offset before asking the inner finder and wrap the result.
      if (type == typeof(ILabelModelParameterFinder) && innerLookup.Lookup<ILabelModelParameterFinder>() != null) {
        return this;
      }

      // Fallback: forward other lookups to the inner model.
      return innerLookup.Lookup(type);
    }

    /// <summary>
    ///   Returns an enumerator over possible parameters for this label and model.
    /// </summary>
    public IEnumerable<ILabelModelParameter> GetParameters() {
      var innerProvider = innerLookup.Lookup<ILabelModelParameterProvider>();
      if (innerProvider == null) {
        yield break;
      }

      // Delegate provider: wrap all returned parameters so they work with this model.
      foreach (var parameter in innerProvider.GetParameters()) {
        yield return new OffsetWrapperLabelModelParameter(model, parameter);
      }
    }

    /// <summary>
    ///   Finds the parameter that best matches the given layout.
    /// </summary>
    public ILabelModelParameter FindBestParameter(IOrientedRectangle layout) {
      var innerFinder = innerLookup.Lookup<ILabelModelParameterFinder>();
      if (innerFinder == null) {
        return null!;
      }

      // Reverse the offset that this model applies in GetGeometry.
      var tmp = layout.ToOrientedRectangle();
      var upVector = tmp.GetUpVector();
      tmp.AnchorX -= upVector.X * model.WorldOffset;
      tmp.AnchorY -= upVector.Y * model.WorldOffset;
      var innerParameter = innerFinder.FindBestParameter(tmp);
      return new OffsetWrapperLabelModelParameter(model, innerParameter);
    }
  }
}
