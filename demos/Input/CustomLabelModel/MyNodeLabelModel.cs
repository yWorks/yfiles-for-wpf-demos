/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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

// specify xml namespaces and prefixes to use in GraphML for the classes in this namespace
[assembly: XmlnsDefinition("http://www.yworks.com/yfiles-wpf/2.1/demos/CustomLabelModel", "Demo.yFiles.Graph.CustomLabelModel")]
[assembly: XmlnsPrefix("http://www.yworks.com/yfiles-wpf/2.1/demos/CustomLabelModel", "demo")]

namespace Demo.yFiles.Graph.CustomLabelModel
{
  /// <summary>
  /// Custom implementation of <see cref="ILabelModel"/> that provides either continuous or discrete label positions directly outside the node border.
  /// </summary>
  /// <remarks>In addition to the label model itself, two important support interfaces (<see cref="ILabelModelParameterFinder"/> and <see cref="ILabelModelParameterProvider"/>
  /// are also implemented.</remarks>
  public class MyNodeLabelModel : ILabelModel, ILabelModelParameterProvider, ILabelModelParameterFinder
  {
    private int candidateCount = 8;
    private double offset;

    /// <summary>
    /// Returns the number of discrete label positions around the border.
    /// </summary>
    /// <remarks>A value of 0 signifies that continuous label positions are used.</remarks>
    [DefaultValue(8)]
    public int CandidateCount {
      get { return candidateCount; }
      set { candidateCount = value; }
    }

    /// <summary>
    /// The offset of the label location, i.e. the distance to the node layout borders.
    /// </summary>
    [DefaultValue(0.0d)]
    public double Offset {
      get { return offset; }
      set { offset = value; }
    }

    /// <summary>
    /// Return instances of the support interfaces (which are actually the model instance itself)
    /// </summary>
    public object Lookup(Type type) {
      if (type == typeof (ILabelModelParameterProvider) && CandidateCount > 0) {
        //If we request a ILabelModelParameterProvider AND we use discrete label candidates, we return ourselfs
        //otherwise, null is returned, which means that continuous label positions are supported.
        return this;
      }
      if (type == typeof(ILabelModelParameterFinder)) {
        //If we request a ILabelModelParameterProvider, we return ourselfs, so we can always retrieve a matching parameter for a given actual position.
        return this;
      }
      return null;
    }

    /// <summary>
    /// For the given parameter, calculate the actual geometry of the specified label in absolute world coordinates.
    /// </summary>
    /// <remarks>The actual position is calculated from the <see cref="MyNodeLabelModelParameter.Ratio"/> specified in the parameter as
    /// the counterclock-wise angle on the label owner's circumference. Note that we also rotate the label layout itself accordingly.</remarks>
    public IOrientedRectangle GetGeometry(ILabel label, ILabelModelParameter parameter) {
      var modelParameter = parameter as MyNodeLabelModelParameter;
      var ownerNode = label.Owner as INode;
      if (modelParameter != null && ownerNode != null) {
        //If we have a matching parameter and a node as owner, calculate the angle for the label position and the matchin rotation of the label layout box itself.
        var center = ownerNode.Layout.GetCenter();
        var radius = Math.Max(ownerNode.Layout.Width, ownerNode.Layout.Height)*0.5d;
        var ratio = modelParameter.Ratio;
        double angle = ratio*Math.PI*2;
        double x = Math.Sin(angle);
        double y = Math.Cos(angle);
        PointD up = new PointD(-y,x);
        OrientedRectangle result = new OrientedRectangle();
        result.SetUpVector(up);
        result.Resize(label.PreferredSize);
        result.SetCenter(center + (offset + radius + label.PreferredSize.Height * 0.5d) * up);
        return result;
      } else {
        return OrientedRectangle.Empty;
      }
    }

    /// <summary>
    /// Create the default parameter for this model. Here it is located at 1/4 around the node's circumference.
    /// </summary>
    public ILabelModelParameter CreateDefaultParameter() {
      return CreateParameter(0.25d);
    }

    /// <summary>
    /// Factory method to create a parameter for a given rotation angle.
    /// </summary>
    public ILabelModelParameter CreateParameter(double ratio) {
      return new MyNodeLabelModelParameter(this, ratio);
    }

    public ILookup GetContext(ILabel label, ILabelModelParameter parameter) {
      return Lookups.Empty;
    }

    /// <summary>
    /// Returns an enumerator over a set of possible <see cref="ILabelModelParameter"/>
    /// instances that can be used for the given label and model.
    /// </summary>
    /// <remarks>Since in <see cref="Lookup"/>, we return an instance of this class only for positive <see cref="CandidateCount"/>s,
    /// this method is only called for <b>discrete</b> candidates.</remarks>
    IEnumerable<ILabelModelParameter> ILabelModelParameterProvider.GetParameters(ILabel label, ILabelModel model) {
      var parameters = new ILabelModelParameter[candidateCount];
      for (int i = 0; i < parameters.Length; i++) {
        parameters[i] = new MyNodeLabelModelParameter(this, (double)i/(parameters.Length));
      }
      return parameters;
    }

    /// <summary>
    /// Tries to find a parameter that best matches the given layout for the
    /// provided label instance.
    /// </summary>
    /// <remarks>By default, this method is only called when <b>no discrete</b> candidates are specified (i.e. here for <see cref="CandidateCount"/> = 0. 
    /// This implementation just calculates the rotation angle for the center of <paramref name="layout"/> 
    /// and creates a parameter for exactly this angle which <see cref="CreateParameter"/>.</remarks>
    ILabelModelParameter ILabelModelParameterFinder.FindBestParameter(ILabel label, ILabelModel model, IOrientedRectangle layout) {
      var labelModel = model as MyNodeLabelModel;
      var node = label.Owner as INode;
      if (labelModel != null && node != null) {
        var direction = (layout.GetCenter() - node.Layout.GetCenter()).Normalized;
        double ratio = Math.Atan2(direction.Y, -direction.X)/(Math.PI*2.0d);
        return labelModel.CreateParameter(ratio);
      } else {
        return DefaultLabelModelParameterFinder.Instance.FindBestParameter(label, model, layout);
      }
    }

    /// <summary>
    /// Custom implementation of <see cref="ILabelModelParameter"/> that is tailored to match <see cref="MyNodeLabelModel"/> instances.
    /// </summary>
    [TypeConverter(typeof(MyNodeLabelModelParameterConverter))]
    internal class MyNodeLabelModelParameter : ILabelModelParameter
    {

      private readonly MyNodeLabelModel owner;
      private readonly double ratio;

      public MyNodeLabelModelParameter(MyNodeLabelModel owner, double ratio) {
        this.owner = owner;
        this.ratio = ratio;
      }

      internal double Ratio {
        get { return ratio; }
      }

      public object Clone() {
        // we have no mutable state, so return this.
        return this;
      }

      /// <summary>
      /// Return a model instance where this parameter belongs to.
      /// </summary>
      /// <remarks>This is usually a reference to the model instance that has created this parameter.</remarks>
      public ILabelModel Model {
        get { return owner; }
      }

      /// <summary>
      /// Predicate that checks if this parameter instance may be used with the given <paramref name="label"/>.
      /// </summary>
      /// <remarks>Our model/parameter implementation only makes sense when used for <see cref="INode"/>s.</remarks>
      public bool Supports(ILabel label) {
        return label.Owner is INode;
      }
    }
  }

  /// <summary>
  /// This helper class needs to be public in order to be accessable by GraphML's reflection code.
  /// </summary>
  public sealed class MyNodeLabelModelParameterConverter : TypeConverter
  {
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
      return destinationType == typeof(MarkupExtension) || base.CanConvertTo(context, destinationType);
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
      if (destinationType == typeof(MarkupExtension) && value is MyNodeLabelModel.MyNodeLabelModelParameter) {
        var parameter = (MyNodeLabelModel.MyNodeLabelModelParameter)value;
        return new MyNodeLabelModelParameterExtension { Model = (MyNodeLabelModel)parameter.Model, Ratio = parameter.Ratio };
      } else {
        return base.ConvertTo(context, culture, value, destinationType);
      }
    }
  }


  /// <summary>
  /// This helper class needs to be public in order to be accessable by GraphML's reflection code
  /// </summary>
  /// <remarks>Since <see cref="MyNodeLabelModel.MyNodeLabelModelParameter"/> is internal, we need to provice a public
  /// <see cref="MarkupExtension"/> support class for serialization.</remarks>
  [MarkupExtensionReturnType(typeof(ILabelModelParameter))]
  public sealed class MyNodeLabelModelParameterExtension : MarkupExtension
  {
    private double ratio;
    private MyNodeLabelModel model;
    public MyNodeLabelModelParameterExtension() {}

    [DefaultValue(0.0d)]
    public double Ratio {
      get { return ratio; }
      set { ratio = value; }
    }

    public MyNodeLabelModel Model {
      get { return model; }
      set { model = value; }
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      var labelModel = model ?? new MyNodeLabelModel();
      return labelModel.CreateParameter(ratio);
    }
  }
}