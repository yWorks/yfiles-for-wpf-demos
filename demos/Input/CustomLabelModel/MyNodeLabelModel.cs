/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Globalization;
using System.Windows.Markup;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Utils;

// specify xml namespaces and prefixes to use in GraphML for the classes in this namespace
[assembly :
    XmlnsDefinition("http://www.yworks.com/yfiles-wpf/2.1/demos/CustomLabelModel",
        "Demo.yFiles.Graph.CustomLabelModel")]
[assembly : XmlnsPrefix("http://www.yworks.com/yfiles-wpf/2.1/demos/CustomLabelModel", "demo")]

namespace Demo.yFiles.Graph.CustomLabelModel
{
  /// <summary>
  ///   Custom implementation of <see cref="ILabelModel"/> that provides either continuous or discrete label positions
  ///   directly outside the node border.
  /// </summary>
  public class MyNodeLabelModel : ILabelModel
  {
    /// <summary>
    ///   Returns the number of discrete label positions around the border.
    /// </summary>
    /// <remarks>A value of 0 signifies that continuous label positions are used.</remarks>
    [DefaultValue(8)]
    public int CandidateCount { get; set; } = 8;

    /// <summary>
    ///   The offset of the label location, i.e. the distance to the node layout borders.
    /// </summary>
    [DefaultValue(2.0d)]
    public double Offset { get; set; } = 2;

    /// <summary>
    ///   For the given parameter, calculate the actual geometry of the specified label in absolute world coordinates.
    /// </summary>
    /// <remarks>
    ///   The actual position is calculated from the <see cref="MyNodeLabelModelParameter.Ratio"/> specified in the parameter
    ///   as the counterclockwise angle on the label owner's circumference. Note that we also rotate the label layout itself
    ///   accordingly.
    /// </remarks>
    public IOrientedRectangle GetGeometry(ILabel label, ILabelModelParameter parameter) {
      if (parameter is not MyNodeLabelModelParameter modelParameter) {
        throw new ArgumentException("The parameter is not supported by this label model.", nameof(parameter));
      }
      if (!DefaultGraph.HasOwner(label)) {
        throw new ArgumentException("The label has no valid owner.", nameof(label));
      }
      if (label.Owner is not INode node) {
        throw new ArgumentException("This model only supports node labels.", nameof(label));
      }

      // If we have a matching parameter and a node as owner, calculate the angle for the label position and the matchin rotation of the label layout box itself.
      var center = node.Layout.GetCenter();
      var radius = Math.Max(node.Layout.Width, node.Layout.Height) * 0.5d;
      var ratio = modelParameter.Ratio;
      var angle = ratio * Math.PI * 2;
      var x = Math.Sin(angle);
      var y = Math.Cos(angle);
      var up = new PointD(-y, x);
      var result = new OrientedRectangle();
      result.SetUpVector(up);
      result.Size = label.PreferredSize;
      result.SetCenter(center + (Offset + radius + label.PreferredSize.Height * 0.5d) * up);
      return result;
    }

    /// <summary>
    ///   Factory method to create a parameter for a given rotation angle.
    /// </summary>
    public MyNodeLabelModelParameter CreateParameter(double ratio) {
      return new MyNodeLabelModelParameter(this, ratio);
    }

    /// <summary>
    ///   Gets a lookup that provides two important support interfaces (<see cref="ILabelModelParameterFinder"/> and
    ///   <see cref="ILabelModelParameterProvider"/>)
    /// </summary>
    public ILookup GetContext(ILabel label) {
      return new MyNodeLabelModelLookup(label, this);
    }

    /// <summary>
    ///   Custom implementation of <see cref="ILabelModelParameter"/> that is tailored to match <see cref="MyNodeLabelModel"/>
    ///   instances.
    /// </summary>
    [TypeConverter(typeof(MyNodeLabelModelParameterConverter))]
    public class MyNodeLabelModelParameter : ILabelModelParameter
    {
      private readonly MyNodeLabelModel owner;

      public MyNodeLabelModelParameter(MyNodeLabelModel owner, double ratio) {
        this.owner = owner;
        Ratio = ratio;
      }

      public double Ratio { get; }

      public object Clone() {
        // we have no mutable state, so return this.
        return this;
      }

      /// <summary>
      ///   Return a model instance where this parameter belongs to.
      /// </summary>
      /// <remarks>This is usually a reference to the model instance that has created this parameter.</remarks>
      public ILabelModel Model => owner;
    }

    private sealed class MyNodeLabelModelLookup : ILookup, ILabelModelParameterProvider, ILabelModelParameterFinder
    {
      private readonly ILabel label;
      private readonly MyNodeLabelModel model;

      public MyNodeLabelModelLookup(ILabel label, MyNodeLabelModel model) {
        if (!DefaultGraph.HasOwner(label)) {
          throw new ArgumentException("The label has no valid owner.", nameof(label));
        }
        if (label.Owner is not INode) {
          throw new ArgumentException("This model only supports node labels.", nameof(label));
        }
        this.label = label;
        this.model = model;
      }

      /// <summary>
      ///   Return instances of the support interfaces (which are actually the lookup instance itself)
      /// </summary>
      public object Lookup(Type type) {
        if (type == typeof(ILabelModelParameterProvider) && model.CandidateCount > 0) {
          // If we request a ILabelModelParameterProvider AND we use discrete label candidates, we return ourselves
          // otherwise, null is returned, which means that continuous label positions are supported.
          return this;
        }
        if (type == typeof(ILabelModelParameterFinder)) {
          // If we request a ILabelModelParameterFinder, we return ourselves, so we can always retrieve a matching parameter for a given actual position.
          return this;
        }
        return null;
      }

      /// <summary>
      ///   Returns an enumerator over a set of possible <see cref="ILabelModelParameter"/>
      ///   instances that can be used for the given label and model.
      /// </summary>
      /// <remarks>
      ///   Since in <see cref="Lookup"/>, we return an instance of this class only for positive <see cref="CandidateCount"/>s,
      ///   this method is only called for <b>discrete</b> candidates.
      /// </remarks>
      IEnumerable<ILabelModelParameter> ILabelModelParameterProvider.GetParameters() {
        var parameters = new ILabelModelParameter[model.CandidateCount];
        for (var i = 0; i < parameters.Length; i++) {
          parameters[i] = model.CreateParameter((double) i / parameters.Length);
        }
        return parameters;
      }

      /// <summary>
      ///   Tries to find a parameter that best matches the given layout for the
      ///   provided label instance.
      /// </summary>
      /// <remarks>
      ///   By default, this method is only called when <b>no discrete</b> candidates are specified (i.e. here for
      ///   <see cref="CandidateCount"/> = 0.
      ///   This implementation just calculates the rotation angle for the center of <paramref name="layout"/>
      ///   and creates a parameter for exactly this angle which <see cref="CreateParameter"/>.
      /// </remarks>
      public ILabelModelParameter FindBestParameter(IOrientedRectangle layout) {
        var direction = (layout.GetCenter() - ((INode) label.Owner).Layout.GetCenter()).Normalized;
        var ratio = Math.Atan2(direction.Y, -direction.X) / (Math.PI * 2.0d);
        return model.CreateParameter(ratio);
      }
    }
  }

  /// <summary>
  ///   This helper class needs to be public in order to be accessible by GraphML's reflection code.
  /// </summary>
  public sealed class MyNodeLabelModelParameterConverter : TypeConverter
  {
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
      return destinationType == typeof(MarkupExtension) || base.CanConvertTo(context, destinationType);
    }

    public override object ConvertTo(
        ITypeDescriptorContext context,
        CultureInfo culture,
        object value,
        Type destinationType) {
      if (destinationType == typeof(MarkupExtension) && value is MyNodeLabelModel.MyNodeLabelModelParameter) {
        var parameter = (MyNodeLabelModel.MyNodeLabelModelParameter) value;
        return new MyNodeLabelModelParameterExtension {
            Model = (MyNodeLabelModel) parameter.Model, Ratio = parameter.Ratio
        };
      }
      return base.ConvertTo(context, culture, value, destinationType);
    }
  }


  /// <summary>
  ///   This helper class needs to be public in order to be accessible by GraphML's reflection code
  /// </summary>
  /// <remarks>
  ///   Since <see cref="MyNodeLabelModel.MyNodeLabelModelParameter"/> is internal, we need to provide a public
  ///   <see cref="MarkupExtension"/> support class for serialization.
  /// </remarks>
  [MarkupExtensionReturnType(typeof(ILabelModelParameter))]
  public sealed class MyNodeLabelModelParameterExtension : MarkupExtension
  {
    [DefaultValue(0.0d)]
    public double Ratio { get; set; }

    public MyNodeLabelModel Model { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      var labelModel = Model ?? new MyNodeLabelModel();
      return labelModel.CreateParameter(Ratio);
    }
  }
}
