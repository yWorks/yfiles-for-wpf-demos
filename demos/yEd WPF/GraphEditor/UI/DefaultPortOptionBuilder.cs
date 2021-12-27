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
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Demo.yFiles.GraphEditor.Styles;
using Demo.yFiles.Option.DataBinding;
using Demo.yFiles.Option.Handler;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.GraphEditor.Option {

  /// <summary>
  /// Default implementation of <see cref="IOptionBuilder"/> for <see cref="IPort"/> objects
  /// that recursively creates <see cref="IOptionItem"/>s for a <see cref="IPropertyMap"/>
  /// that contains properties of <see cref="IPort"/> instances. This implementation contains
  /// the <see cref="IPort.Style"/> of the port.
  /// </summary>
  /// <remarks>
  /// Use this implementation together with <see cref="DefaultPortPropertyMapBuilder"/>
  /// in a scenario where <see cref="ISelectionProvider{T}"/> is used to create an
  /// <see cref="OptionHandler"/> to show the properties of an <see cref="IPort"/>.
  /// </remarks>
  public class DefaultPortOptionBuilder  : IOptionBuilder
  {

    #region IOptionBuilder Members

    private readonly IList<Type> validPortStyles = new List<Type>(2); 

    /// <summary>
    /// Creates a new instance of a builder for labels.
    /// </summary>
    public DefaultPortOptionBuilder() {
      validPortStyles.Add(typeof(CirclePortStyle));
      validPortStyles.Add(typeof(NodeStylePortStyleAdapter));
    }

    /// <summary>
    /// Gets a modifiable list of types of <see cref="IPortStyle"/>.
    /// </summary>
    public IList<Type> ValidPortStyles {
      get { return validPortStyles; }
    }

    /// <inheritdoc/>
    public virtual void AddItems(IOptionBuilderContext context, Type subjectType, object subject) {
      IPort port = subject as IPort;
      if (port == null) {
        return;
      }

      BuildLabelOptions(context, port);
//      BuildStyleOptions(context, port);

      AddValidStyleItems(context);
      AddStyleItems(context, port);
    }

    /// <summary>
    /// Adds the currently valid <see cref="IPortStyle"/> items to the context.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="ValidPortStyles"/>.
    /// </remarks>
    /// <param name="context">The context to use.</param>
    protected virtual void AddValidStyleItems(IOptionBuilderContext context) {
      CollectionOptionItem<Type> portStyleItems = new CollectionOptionItem<Type>(DefaultPortPropertyMapBuilder.PortStyleProperty, validPortStyles);
      portStyleItems.Attributes[CollectionOptionItem<Type>.USE_ONLY_DOMAIN_ATTRIBUTE] = false;
      portStyleItems.Attributes[OptionItem.ItemTemplateAttribute] = Application.Current.FindResource("PortStyleTypeTemplate") as DataTemplate;
      context.BindItem(portStyleItems, DefaultPortPropertyMapBuilder.PortStyleProperty);
    }

    /// <summary>
    /// Adds a <see cref="IOptionItem"/>s that are bound to the port's <see cref="IPort.Style"/>
    /// to the builder.
    /// </summary>
    /// <param name="context">The context to use for building.</param>
    /// <param name="port"></param>
    protected virtual void AddStyleItems(IOptionBuilderContext context, IPort port) {
      //style group...
      //retrieve current style...
      IPortStyle style = port.Style;
      if (style != null) {
        //retrieve OptionBuilder from style
        IOptionBuilder styleBuilder = GetStyleOptionBuilder(context, style);
        if (styleBuilder != null) {
          styleBuilder.AddItems(context.CreateChildContext(DefaultPortPropertyMapBuilder.StyleProperty), style.GetType(), style);
        }
      }
    }

    /// <summary>
    /// Retrieves the <see cref="IOptionBuilder"/> to use for the given style.
    /// </summary>
    /// <remarks>
    /// This implementation simply delegates to <see cref="IOptionBuilderContext.GetOptionBuilder(object)"/>.
    /// </remarks>
    /// <param name="context">The context to use.</param>
    /// <param name="style">The current style instance.</param>
    /// <returns>The builder or <see langword="null"/>.</returns>
    protected virtual IOptionBuilder GetStyleOptionBuilder(IOptionBuilderContext context, IPortStyle style) {
      return context.GetOptionBuilder(style);
    }

    /// <summary>
    /// Builds the options for the first label of the node instance.
    /// </summary>
    /// <param name="context">The context to use for building.</param>
    /// <param name="subject">The current node instance.</param>
    protected virtual void BuildLabelOptions(IOptionBuilderContext context, ILabelOwner subject) {
      if (subject.Labels.Count > 0) {
        context = context.CreateChildContext(DefaultNodePropertyMapBuilder.LabelPropertyName);
        ILabel label = subject.Labels[0];
        if (label != null) {
          IOptionBuilder builder = GetLabelOptionBuilder(context, label);
          if (builder != null) {
            builder.AddItems(context, typeof(ILabel), label);
          }
        }
      }
    }

    /// <summary>
    /// Method that retrieves an <see cref="IOptionBuilder"/> instance for the given label and context.
    /// </summary>
    /// <remarks>
    /// This implementation simply delegates to <see cref="IOptionBuilderContext.GetOptionBuilder(object)"/>.
    /// </remarks>
    /// <param name="context">The context to use.</param>
    /// <param name="label">The label instance.</param>
    /// <returns>The builder to use or <see langword="null"/>.</returns>
    protected virtual IOptionBuilder GetLabelOptionBuilder(IOptionBuilderContext context, ILabel label) {
      return context.GetOptionBuilder(label);
    }

    #endregion

  }

  public class PortTypeNameConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var splittedName = value.ToString().Split('.');
      return splittedName[splittedName.Length-1];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }


  /// <summary>
  /// A simple default implementation of the <see cref="IPropertyMapBuilder"/>
  /// that recursively creates a property map for an <see cref="IPort"/> which contains
  /// the <see cref="ILabel.Style"/> of the label.
  /// </summary>
  /// <remarks>
  /// Use this implementation together with <see cref="DefaultPortOptionBuilder"/>
  /// in a scenario where <see cref="ISelectionProvider{T}"/> is used to create an
  /// <see cref="OptionHandler"/> to show the properties of an <see cref="IPort"/>.
  /// <br/>
  /// The <see cref="StyleAssignmentPolicy"/> can be used to determine whether 
  /// the implementation should try to modify an existing style instance or
  /// create a new instance every time the style is changed.
  /// This implementation depends on the fact that an implementation of <see cref="IGraph"/>
  /// can be found in the <see cref="ILookup.Lookup"/> of <see cref="IPropertyBuildContext{TSubject}"/>
  /// to <see cref="IGraph.SetStyle(IPort,IPortStyle)">assign a new style to a port</see> when necessary.
  /// </remarks>
  public class DefaultPortPropertyMapBuilder : PropertyMapBuilderBase<IPort>
  {
    private AssignmentPolicy styleAssignmentPolicy = AssignmentPolicy.ModifyInstance;
    /// <summary>
    /// The name of the property to use for the port styles.
    /// </summary>
    public const string PortStyleProperty = "PortStyle";
    /// <summary>
    /// The <see cref="IOptionBuilderContext.CreateChildContext">child context prefix</see>
    /// used for the style of the item.
    /// </summary>
    public const string StyleProperty = "Style";
    /// <summary>
    /// The <see cref="IOptionBuilderContext.CreateChildContext">child context prefix</see>
    /// used for the label of the item.
    /// </summary>
    public const string LabelPropertyName = "Label";


    /// <summary>
    /// Determines how changes to the <see cref="IPort.Style"/> instance
    /// that are triggered by modifying the properties this instance creates 
    /// are treated.
    /// </summary>
    /// <remarks>
    /// A value of <see cref="AssignmentPolicy.CreateNewInstance"/>
    /// will result in the creation of new style instances every time a property of the
    /// style is modified, whereas <see cref="AssignmentPolicy.ModifyInstance"/>
    /// will result in a direct style modification.<br/>
    /// The default is <see cref="AssignmentPolicy.ModifyInstance"/>.
    /// </remarks>
    public AssignmentPolicy StyleAssignmentPolicy {
      get { return styleAssignmentPolicy; }
      set { styleAssignmentPolicy = value; }
    }

    /// <summary>
    /// Creates a new instance for building port properties.
    /// </summary>
    public DefaultPortPropertyMapBuilder() : base(false){}

    /// <inheritdoc/>
    protected override void BuildPropertyMapImpl(IPropertyBuildContext<IPort> context) {
      IPort currentPort = context.CurrentInstance;
      if (currentPort == null) {
        return;
      }
      BuildLabelProperties(context);
      BuildPortStylesProperty(context);
      BuildStyleProperties(context, currentPort.Style);
    }

    /// <summary>
    /// Method that populates the <see cref="IPropertyMap"/> for labels in a given context.
    /// </summary>
    /// <remarks>
    /// This implementation uses <see cref="GetLabelPropertyMapBuilder{T}"/> to retrieve the builder for the label.
    /// </remarks>
    /// <param name="context">The context to use for queries.</param>
    protected virtual void BuildLabelProperties<T>(IPropertyBuildContext<T> context) where T : class, ILabelOwner {
      IPropertyBuildContext<ILabel> childContext = context.CreateChildContext<ILabel>(LabelPropertyName,
        delegate() {
          T item = context.CurrentInstance;
          return item.Labels.Count > 0 ? item.Labels[0] : null;
        },
        delegate(ILabel newInstance) { }, AssignmentPolicy.ModifyInstance);

      if (context.CurrentInstance.Labels.Count > 0) {
        IPropertyMapBuilder builder = GetLabelPropertyMapBuilder(context, context.CurrentInstance.Labels[0]);
        if (builder != null) {
          builder.BuildPropertyMap(childContext);
        }
      }
    }

    /// <summary>
    /// Method that retrieves the <see cref="IPropertyMapBuilder"/> for the provided label instance.
    /// </summary>
    /// <remarks>
    /// This implementation simply delegates to <see cref="IPropertyBuildContext{TSubject}.GetPropertyMapBuilder(object)"/>.
    /// </remarks>
    /// <param name="context">The context to use for queries.</param>
    /// <param name="label">The label instance currently associated with the <see cref="ILabelOwner"/>.</param>
    /// <returns>A builder or <see langword="null"/></returns>
    protected virtual IPropertyMapBuilder GetLabelPropertyMapBuilder<T>(IPropertyBuildContext<T> context, ILabel label) where T : class, ILabelOwner {
      return context.GetPropertyMapBuilder(label);
    }

    /// <summary>
    /// Builds the properties for the port's <see cref="IPort.Style"/>
    /// </summary>
    protected virtual void BuildStyleProperties(IPropertyBuildContext<IPort> context, IPortStyle style) {
      //style group...
      //retrieve current style...
      if (style != null) {
        IPropertyMapBuilder propertyBuilder = GetStyleBuilder(context, style);
        if (propertyBuilder != null) {
          propertyBuilder.BuildPropertyMap(context.CreateChildContext<IPortStyle>(StyleProperty,
                                                                                   delegate { return context.CurrentInstance.Style;},
                                                                                   delegate(IPortStyle newInstance) {
                                                                                     IGraph graph = context.Lookup(typeof(IGraph)) as IGraph;
                                                                                     if (graph != null) {
                                                                                       graph.SetStyle(context.CurrentInstance, newInstance);
                                                                                     }
                                                                                   }, styleAssignmentPolicy));
        }
      }
    }

    /// <summary>
    /// Retrieves the builder for the given style.
    /// </summary>
    protected virtual IPropertyMapBuilder GetStyleBuilder(IPropertyBuildContext<IPort> context, IPortStyle style) {
      return context.GetPropertyMapBuilder(style);
    }

    /// <summary>
    /// Builds the property for the port's <see cref="IPort.Style"/>.
    /// </summary>
    protected virtual void BuildPortStylesProperty(IPropertyBuildContext<IPort> context) {
      context.AddEntry(PortStyleProperty, 
                        new ValueGetterDelegate<Type>(delegate {
                                                        var type = context.CurrentInstance.Style.GetType();
                                                        while (!type.IsPublic) {
                                                          type = type.BaseType;
                                                        }
                                                        return type;
                                                      })
                       , new ValueSetterDelegate<Type>(delegate(Type value) {
                                                           IGraph graph = context.Lookup(typeof(IGraph)) as IGraph;
                                                           IPortStyle style = Activator.CreateInstance(value) as IPortStyle;
                                                           if (graph != null && style != null) {
                                                             graph.SetStyle(context.CurrentInstance, style);
                                                           }
                                                         }), null);
    }
  }
}