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

namespace yWorks.Demo.BorderAlignedLabelModel
{
  /// <summary>
  ///   Custom implementation of <see cref="ILabelModel"/> that provides either continuous or discrete label
  ///   positions directly around the node border.
  ///   In addition to the label model itself, two important support interfaces
  ///   <see cref="ILabelModelParameterFinder"/> and <see cref="ILabelModelParameterProvider"/> are also
  ///   implemented.
  /// </summary>
  public class BorderAlignedLabelModel : ILabelModel
  {
    /// <summary>
    ///   The number of discrete label positions around the border.
    ///   A value of 0 signifies that continuous label positions are used.
    /// </summary>
    public int CandidateCount { get; set; } = 8;

    /// <summary>
    ///   The offset of the label location, i.e., the distance to the node layout borders.
    /// </summary>
    public double Offset { get; set; }

    /// <summary>
    ///   Calculates for the given parameter the actual geometry of the specified label in absolute world coordinates.
    ///   The actual position is calculated from the ratio specified in the parameter as the counterclockwise angle on
    ///   the label owner's circumference. Note that we also rotate the label layout itself accordingly.
    /// </summary>
    public IOrientedRectangle GetGeometry(ILabel label, ILabelModelParameter layoutParameter) {
      var ownerNode = label.Owner as INode;
      if (ownerNode != null && layoutParameter is BorderAlignedLabelModelParameter parameter) {
        // If we have a matching parameter and a node as owner, calculate the angle for the label position
        // and the matching rotation of the label layout box itself.
        var center = ownerNode.Layout.GetCenter();
        var radius = Math.Max(ownerNode.Layout.Width, ownerNode.Layout.Height) * 0.5;
        var angle = parameter.Ratio * Math.PI * 2;
        var x = Math.Sin(angle);
        var y = Math.Cos(angle);
        var up = new PointD(-y, x);
        var result = new OrientedRectangle();
        result.SetUpVector(up);
        result.Width = label.PreferredSize.Width;
        result.Height = label.PreferredSize.Height;
        result.SetCenter(center + up * (Offset + radius + label.PreferredSize.Height * 0.5));
        return result;
      }
      return OrientedRectangle.Empty;
    }

    /// <summary>Factory method that creates a parameter for a given rotation angle.</summary>
    public BorderAlignedLabelModelParameter CreateParameter(double ratio) => new(this, ratio);

    /// <summary>Provides a lookup context for the given combination of label and parameter.</summary>
    public ILookup GetContext(ILabel label) => new BorderAlignedLabelModelLookup(label, this);
  }

  /// <summary>
  ///   Custom implementation of <see cref="ILabelModelParameter"/> that is tailored to match
  ///   <see cref="BorderAlignedLabelModel"/> instances.
  /// </summary>
  public sealed class BorderAlignedLabelModelParameter : ILabelModelParameter
  {
    /// <summary>Creates a new parameter instance.</summary>
    /// <param name="model">The model for this parameter</param>
    /// <param name="ratio">The ratio for the given label model parameter</param>
    public BorderAlignedLabelModelParameter(BorderAlignedLabelModel model, double ratio) {
      Model = model;
      Ratio = ratio;
    }

    public double Ratio { get; }

    public BorderAlignedLabelModel Model { get; }

    ILabelModel ILabelModelParameter.Model => Model;

    /// <summary>Creates a clone of this parameter object.</summary>
    public object Clone() =>
        // we have no mutable state, so return this.
        this;
  }

  internal sealed class BorderAlignedLabelModelLookup : ILookup, ILabelModelParameterProvider,
      ILabelModelParameterFinder
  {
    private readonly BorderAlignedLabelModel model;
    private readonly ILabel label;

    public BorderAlignedLabelModelLookup(ILabel label, BorderAlignedLabelModel model) {
      this.label = label;
      this.model = model;
      if (label.Owner == null) {
        throw new InvalidOperationException("The label has no valid owner.");
      }
      if (!(label.Owner is INode)) {
        throw new InvalidOperationException("This model supports only node labels.");
      }
    }

    /// <summary>Returns instances of the support interfaces, which are actually this lookup object.</summary>
    public object? Lookup(Type type) {
      if (type == typeof(ILabelModelParameterProvider) && model.CandidateCount > 0) {
        // If we request a parameter provider and use discrete label candidates, return this lookup;
        // otherwise, null means that continuous label positions are supported.
        return this;
      }
      if (type == typeof(ILabelModelParameterFinder)) {
        // Return this lookup so that a matching parameter can always be retrieved.
        return this;
      }
      return null;
    }

    /// <summary>
    ///   Returns an enumerator over possible parameters for this label and model.
    ///   This method is called for discrete candidates.
    /// </summary>
    public IEnumerable<ILabelModelParameter> GetParameters() {
      var parameters = new List<ILabelModelParameter>();
      for (var i = 0; i < model.CandidateCount; ++i) {
        parameters.Add(model.CreateParameter((double) i / model.CandidateCount));
      }
      return parameters;
    }

    /// <summary>
    ///   Finds the parameter that best matches the given layout. For continuous positions, this creates a parameter
    ///   for the angle at the center of the layout.
    /// </summary>
    public ILabelModelParameter FindBestParameter(IOrientedRectangle layout) {
      var direction = (layout.GetCenter() - ((INode) label.Owner).Layout.GetCenter()).Normalized;
      var ratio = Math.Atan2(direction.Y, -direction.X) / (Math.PI * 2);
      return model.CreateParameter(ratio);
    }

    public BorderAlignedLabelModelLookup Clone() =>
        // we have no mutable state, so return this.
        this;
  }
}
