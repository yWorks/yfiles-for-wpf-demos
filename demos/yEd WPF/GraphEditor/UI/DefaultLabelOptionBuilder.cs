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
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.LabelModels;

namespace Demo.yFiles.GraphEditor.Option
{

  /// <summary>
  /// Default implementation of <see cref="IOptionBuilder"/> for <see cref="ILabel"/> objects
  /// that recursively creates <see cref="IOptionItem"/>s for a <see cref="IPropertyMap"/>
  /// that contains properties of <see cref="ILabel"/> instances. This implementation contains
  /// the model parameter, preferred size, and the <see cref="ILabel.Style"/> of the label.
  /// </summary>
  /// <remarks>
  /// Use this implementation together with <see cref="DefaultLabelPropertyMapBuilder"/>
  /// in a scenario where <see cref="ISelectionProvider{T}"/> is used to create an
  /// <see cref="OptionHandler"/> to show the properties of an <see cref="ILabel"/>.
  /// </remarks>
  public class DefaultLabelOptionBuilder : IOptionBuilder
  {

    #region IOptionBuilder Members

    private readonly IList<Type> validNodeLabelModels = new List<Type>(7);
    private readonly IList<Type> validEdgeLabelModels = new List<Type>(10);
    private readonly IList<Type> validPortLabelModels = new List<Type>(10);
    private readonly IList<Type> validLabelStyles = new List<Type>(3); 

    /// <summary>
    /// Creates a new instance of a builder for labels.
    /// </summary>
    public DefaultLabelOptionBuilder() {
      validNodeLabelModels.Add(typeof(InteriorLabelModel));
      validNodeLabelModels.Add(typeof(ExteriorLabelModel));
      validNodeLabelModels.Add(typeof(FreeNodeLabelModel));
      validNodeLabelModels.Add(typeof(SandwichLabelModel));
      validNodeLabelModels.Add(typeof(InteriorStretchLabelModel));
      validNodeLabelModels.Add(typeof(FreeLabelModel));

      validEdgeLabelModels.Add(typeof(EdgeSegmentLabelModel));
      validEdgeLabelModels.Add(typeof(NinePositionsEdgeLabelModel));
      validEdgeLabelModels.Add(typeof(SmartEdgeLabelModel));
      validEdgeLabelModels.Add(typeof(FreeEdgeLabelModel));
      validEdgeLabelModels.Add(typeof(FreeLabelModel));

      validPortLabelModels.Add(typeof(FreePortLabelModel));
      validPortLabelModels.Add(typeof(InsideOutsidePortLabelModel));

      validLabelStyles.Add(typeof(LabelStyle));
      validLabelStyles.Add(typeof(DefaultLabelStyle));
      validLabelStyles.Add(typeof(NodeStyleLabelStyleAdapter));
    }

    /// <summary>
    /// Gets a modifiable list of types of <see cref="ILabelModel"/> that are valid
    /// for <see cref="INode"/>s.
    /// </summary>
    public IList<Type> ValidNodeLabelModels {
      get { return validNodeLabelModels; }
    }

    /// <summary>
    /// Gets a modifiable list of types of <see cref="ILabelModel"/> that are valid
    /// for <see cref="IEdge"/>s.
    /// </summary>
    public IList<Type> ValidEdgeLabelModels {
      get { return validEdgeLabelModels; }
    }

    /// <summary>
    /// Gets a modifiable list of types of <see cref="ILabelModel"/> that are valid
    /// for <see cref="IPort"/>s.
    /// </summary>
    public IList<Type> ValidPortLabelModels {
      get { return validPortLabelModels; }
    }


    /// <summary>
    /// Gets a modifiable list of types of <see cref="ILabelStyle"/>.
    /// </summary>
    public IList<Type> ValidLabelStyles {
      get { return validLabelStyles; }
    }

    /// <inheritdoc/>
    public virtual void AddItems(IOptionBuilderContext context, Type subjectType, object subject) {
      AddLabelTextItem(context);
      AddPreferredSizeItems(context);
      AddValidModelsItems(context);
      ILabel label = subject as ILabel;
      if (label == null) {
        return;
      }

      AddLabelLayoutParameterItems(context, label);
      AddValidStyleItems(context);
      AddStyleItems(context, label);
    }

    /// <summary>
    /// Adds an <see cref="IOptionItem"/> that is bound to the label's <see cref="ILabel.Text"/>
    /// property to the builder.
    /// </summary>
    /// <param name="context">The context to use for building.</param>
    protected virtual void AddLabelTextItem(IOptionBuilderContext context) {
      context.BindItem(new OptionItem(DefaultLabelPropertyMapBuilder.TextProperty){Value= string.Empty, Type=typeof(string)}, DefaultLabelPropertyMapBuilder.TextProperty);
    }

    /// <summary>
    /// Adds the currently valid <see cref="ILabelStyle"/> items to the context.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="ValidLabelStyles"/>.
    /// </remarks>
    /// <param name="context">The context to use.</param>
    protected virtual void AddValidStyleItems(IOptionBuilderContext context) {
      CollectionOptionItem<Type> labelStyleItems = new CollectionOptionItem<Type>(DefaultLabelPropertyMapBuilder.LabelStyleProperty, validLabelStyles);
      labelStyleItems.Attributes[CollectionOptionItem<Type>.USE_ONLY_DOMAIN_ATTRIBUTE] = false;
      labelStyleItems.Attributes[OptionItem.ItemTemplateAttribute] = Application.Current.FindResource("LabelStyleTypeTemplate") as DataTemplate;
      context.BindItem(labelStyleItems, DefaultLabelPropertyMapBuilder.LabelStyleProperty);
    }
    
    /// <summary>
    /// Adds a <see cref="IOptionItem"/>s that are bound to the label's <see cref="ILabel.Style"/>
    /// to the builder.
    /// </summary>
    /// <param name="context">The context to use for building.</param>
    /// <param name="label">The current label instance.</param>
    protected virtual void AddStyleItems(IOptionBuilderContext context, ILabel label) {
      //style group...
      //retrieve current style...
      ILabelStyle style = label.Style;
      if (style != null) {
        //retrieve OptionBuilder from style
        IOptionBuilder styleBuilder = GetStyleOptionBuilder(context, style);
        if (styleBuilder != null) {
          styleBuilder.AddItems(context.CreateChildContext(DefaultLabelPropertyMapBuilder.StyleProperty), style.GetType(), style);
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
    protected virtual IOptionBuilder GetStyleOptionBuilder(IOptionBuilderContext context, ILabelStyle style) {
      return context.GetOptionBuilder(style);
    }

    /// <summary>
    /// Adds <see cref="IOptionItem"/>s that are bound to the label's <see cref="ILabel.LayoutParameter"/>
    /// and the associated <see cref="ILabelModelParameter.Model"/>.
    /// property to the builder.
    /// </summary>
    /// <param name="context">The context to use for building.</param>
    /// <param name="label">The current label instance.</param>
    protected virtual void AddLabelLayoutParameterItems(IOptionBuilderContext context, ILabel label) {
      if (label.LayoutParameter != null) {
        IOptionBuilder modelBuilder = GetLabelModelOptionBuilder(context, label.LayoutParameter.Model);
        if (modelBuilder != null) {
          modelBuilder.AddItems(context.CreateChildContext("Model"), label.LayoutParameter.Model.GetType(), label.LayoutParameter.Model);
        }
      }
    }

    /// <summary>
    /// Retrieves the <see cref="IOptionBuilder"/> to use for the label model of the provided label.
    /// </summary>
    /// <remarks>
    /// This implementation simply delegates to <see cref="IOptionBuilderContext.GetOptionBuilder(object)"/>.
    /// </remarks>
    /// <param name="context">The context to use.</param>
    /// <param name="model">The current model instance.</param>
    /// <returns>The builder or <see langword="null"/>.</returns>
    protected virtual IOptionBuilder GetLabelModelOptionBuilder(IOptionBuilderContext context, ILabelModel model) {
      return context.GetOptionBuilder(model);
    }

    /// <summary>
    /// Adds the currently valid <see cref="ILabelModel"/> items to the context.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="ValidEdgeLabelModels"/>, <see cref="ValidNodeLabelModels"/>, and <see cref="ValidPortLabelModels"/>
    /// respectively.
    /// </remarks>
    /// <param name="context">The context to use.</param>
    protected virtual void AddValidModelsItems(IOptionBuilderContext context) {
      CollectionOptionItem<Type> edgeLabelModelItem =
        new CollectionOptionItem<Type>(DefaultLabelPropertyMapBuilder.EdgeLabelModelProperty, validEdgeLabelModels);
      edgeLabelModelItem.Attributes[CollectionOptionItem<Type>.USE_ONLY_DOMAIN_ATTRIBUTE] = false;
      edgeLabelModelItem.Attributes[OptionItem.ItemTemplateAttribute] = Application.Current.FindResource("LabelModelTypeTemplate") as DataTemplate;
      context.BindItem(edgeLabelModelItem, DefaultLabelPropertyMapBuilder.EdgeLabelModelProperty);
      CollectionOptionItem<Type> nodeLabelModelItem =
        new CollectionOptionItem<Type>(DefaultLabelPropertyMapBuilder.NodeLabelModelProperty, validNodeLabelModels);
      nodeLabelModelItem.Attributes[CollectionOptionItem<Type>.USE_ONLY_DOMAIN_ATTRIBUTE] = false;
      nodeLabelModelItem.Attributes[OptionItem.ItemTemplateAttribute] = Application.Current.FindResource("LabelModelTypeTemplate") as DataTemplate;
      context.BindItem(nodeLabelModelItem, DefaultLabelPropertyMapBuilder.NodeLabelModelProperty);
      CollectionOptionItem<Type> portLabelModelItem =
        new CollectionOptionItem<Type>(DefaultLabelPropertyMapBuilder.PortLabelModelProperty, validPortLabelModels);
      portLabelModelItem.Attributes[CollectionOptionItem<Type>.USE_ONLY_DOMAIN_ATTRIBUTE] = false;
      portLabelModelItem.Attributes[OptionItem.ItemTemplateAttribute] = Application.Current.FindResource("LabelModelTypeTemplate") as DataTemplate;
      context.BindItem(portLabelModelItem, DefaultLabelPropertyMapBuilder.PortLabelModelProperty);
    }

    /// <summary>
    /// Adds <see cref="IOptionItem"/>s that are bound to the label's <see cref="ILabel.PreferredSize"/>
    /// property to the builder.
    /// </summary>
    /// <param name="context">The context to use for building.</param>
    protected virtual void AddPreferredSizeItems(IOptionBuilderContext context) {
      var preferredSizeItem = new OptionItem(DefaultLabelPropertyMapBuilder.PreferredSizeProperty){Type = typeof(SizeD)};
      context.BindItem(preferredSizeItem, DefaultLabelPropertyMapBuilder.PreferredSizeProperty);
    }
    #endregion

  }

  public class TypeNameConverter : IValueConverter
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
  /// that recursively creates a property map for an <see cref="ILabel"/> which contains
  /// the model parameter, preferred size, and the <see cref="ILabel.Style"/> of the label.
  /// </summary>
  /// <remarks>
  /// Use this implementation together with <see cref="DefaultLabelOptionBuilder"/>
  /// in a scenario where <see cref="ISelectionProvider{T}"/> is used to create an
  /// <see cref="OptionHandler"/> to show the properties of an <see cref="ILabel"/>.
  /// <br/>
  /// The <see cref="StyleAssignmentPolicy"/> can be used to determine whether 
  /// the implementation should try to modify an existing style instance or
  /// create a new instance every time the style is changed.
  /// This implementation depends on the fact that an implementation of <see cref="IGraph"/>
  /// can be found in the <see cref="ILookup.Lookup"/> of <see cref="IPropertyBuildContext{TSubject}"/>
  /// to <see cref="IGraph.SetStyle(ILabel,ILabelStyle)">assign a new style to a label</see> when necessary.
  /// </remarks>
  public class DefaultLabelPropertyMapBuilder : PropertyMapBuilderBase<ILabel>
  {
    private AssignmentPolicy styleAssignmentPolicy = AssignmentPolicy.ModifyInstance;
    /// <summary>
    /// The name of the property to use for the text.
    /// </summary>
    public const string TextProperty = "Text";
    /// <summary>
    /// The name of the property to use for the preferred size.
    /// </summary>
    public const string PreferredSizeProperty = "PreferredSize";
    /// <summary>
    /// The name of the property to use for the label models for edges.
    /// </summary>
    public const string EdgeLabelModelProperty = "EdgeLabelModel";
    /// <summary>
    /// The name of the property to use for the label models for nodes.
    /// </summary>
    public const string NodeLabelModelProperty = "NodeLabelModel";
    /// <summary>
    /// The name of the property to use for the label models for nodes.
    /// </summary>
    public const string PortLabelModelProperty = "PortLabelModel";
    /// <summary>
    /// The name of the property to use for the label styles.
    /// </summary>
    public const string LabelStyleProperty = "LabelStyle";
    /// <summary>
    /// The <see cref="IOptionBuilderContext.CreateChildContext">child context prefix</see>
    /// used for the style of the item.
    /// </summary>
    public const string StyleProperty = "Style";


    /// <summary>
    /// Determines how changes to the <see cref="ILabel.Style"/> instance
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
    /// Creates a new instance for building label properties.
    /// </summary>
    public DefaultLabelPropertyMapBuilder() : base(false){}

    /// <inheritdoc/>
    protected override void BuildPropertyMapImpl(IPropertyBuildContext<ILabel> context) {
      BuildLabelTextProperty(context);
      BuildPreferredSizeProperties(context);
      BuildModelProperties(context);
      ILabel currentLabel;
      currentLabel = context.CurrentInstance;
      if (currentLabel == null) {
        return;
      }
      BuildLabelModelParameterProperties(context, currentLabel.LayoutParameter);
      BuildLabelStylesProperty(context);
      BuildStyleProperties(context, currentLabel.Style);
    }

    /// <summary>
    /// Builds the properties for the labels's <see cref="ILabelModel"/> type.
    /// </summary>
    protected virtual void BuildModelProperties(IPropertyBuildContext<ILabel> context) {
      ValueGetterDelegate<Type> labelModelGetter = new ValueGetterDelegate<Type>(
        delegate {
          var type = context.CurrentInstance.LayoutParameter.Model.GetType();
          while (!type.IsPublic) {
            type = type.BaseType;
          }
          return type;
        });
      ValueSetterDelegate<Type> labelModelSetter = new ValueSetterDelegate<Type>(
        delegate(Type value) {
          IGraph graph = context.Lookup(typeof(IGraph)) as IGraph;
          if (graph != null) {
            ILabelModel model = Activator.CreateInstance(value) as ILabelModel;
            if (model != null) {
              ILabelModelParameterFinder finder =
                model.Lookup(typeof (ILabelModelParameterFinder)) as ILabelModelParameterFinder;
              ILabelModelParameter parameter;
              ILabel subject = context.CurrentInstance;
              if (finder != null) {
                parameter = finder.FindBestParameter(subject, model, subject.GetLayout());
              } else {
                parameter = model.CreateDefaultParameter();
              }
              graph.SetLabelLayoutParameter(subject, parameter);
            }
          }
        });
      ILabel currentLabel;
      currentLabel = context.CurrentInstance;
      if (currentLabel == null) {
        return;
      }
      if (currentLabel.Owner is IEdge ) {
        context.AddEntry(EdgeLabelModelProperty,
                         labelModelGetter,
                         labelModelSetter, null);
      }
      
      if (currentLabel.Owner is INode) {
        context.AddEntry(NodeLabelModelProperty, labelModelGetter, labelModelSetter, null);
      }

      if (currentLabel.Owner is IPort) {
        context.AddEntry(PortLabelModelProperty, labelModelGetter, labelModelSetter, null);
      }
    }

    /// <summary>
    /// Builds the properties for the labels's <see cref="ILabel.LayoutParameter"/>
    /// and <see cref="ILabelModel"/>.
    /// </summary>
    protected virtual void BuildLabelModelParameterProperties(IPropertyBuildContext<ILabel> context, ILabelModelParameter layoutParameter) {
      IPropertyMapBuilder modelBuilder = GetLabelModelPropertyMapBuilder(context, layoutParameter.Model);
      if (modelBuilder != null) {
        modelBuilder.BuildPropertyMap(context.CreateChildContext<ILabelModel>("Model", 
                                                                              delegate {
                                                                                return context.CurrentInstance.LayoutParameter.Model;
                                                                              }, delegate(ILabelModel newInstance) {
                                                                                   return;
                                                                                 }, AssignmentPolicy.ModifyInstance ));
      }
    }

    /// <summary>
    /// Retrieves the builder for the given label model.
    /// </summary>
    protected virtual IPropertyMapBuilder GetLabelModelPropertyMapBuilder(IPropertyBuildContext<ILabel> context, ILabelModel model) {
      return context.GetPropertyMapBuilder(model);
    }

    /// <summary>
    /// Builds the properties for the labels's <see cref="ILabel.Style"/>
    /// </summary>
    protected virtual void BuildStyleProperties(IPropertyBuildContext<ILabel> context, ILabelStyle style) {
      //style group...
      //retrieve current style...
      if (style != null) {
        IPropertyMapBuilder propertyBuilder = GetStyleBuilder(context, style);
        if (propertyBuilder != null) {
          propertyBuilder.BuildPropertyMap(context.CreateChildContext<ILabelStyle>(StyleProperty,
                                                                                   delegate { return context.CurrentInstance.Style;},
                                                                                   delegate(ILabelStyle newInstance) {
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
    protected virtual IPropertyMapBuilder GetStyleBuilder(IPropertyBuildContext<ILabel> context, ILabelStyle style) {
      return context.GetPropertyMapBuilder(style);
    }

    /// <summary>
    /// Builds the properties for the labels's <see cref="ILabel.PreferredSize"/>.
    /// </summary>
    protected virtual void BuildPreferredSizeProperties(IPropertyBuildContext<ILabel> context) {
      context.AddEntry(PreferredSizeProperty,
                       new ValueGetterDelegate<SizeD>(delegate() {
                                                        return context.CurrentInstance.PreferredSize;
                                                      }),
                       new ValueSetterDelegate<SizeD>(delegate(SizeD value) {
                                                        IGraph graph = context.Lookup(typeof(IGraph)) as IGraph;
                                                        if (graph != null) {
                                                          graph.SetLabelPreferredSize(context.CurrentInstance, value);
                                                        }
                                                      }),null);
    }

    /// <summary>
    /// Builds the property for the labels's <see cref="ILabel.Style"/>.
    /// </summary>
    protected virtual void BuildLabelStylesProperty(IPropertyBuildContext<ILabel> context) {
      context.AddEntry(LabelStyleProperty,
                        new ValueGetterDelegate<Type>(delegate {
        var type = context.CurrentInstance.Style.GetType();
        while (!type.IsPublic) {
          type = type.BaseType;
        }
        return type;
      })
                       , new ValueSetterDelegate<Type>(delegate(Type value) {
        IGraph graph = context.Lookup(typeof(IGraph)) as IGraph;
        ILabelStyle style = Activator.CreateInstance(value) as ILabelStyle;
        if (graph != null && style != null) {
          graph.SetStyle(context.CurrentInstance, style);
        }
      }), null);
    }

    /// <summary>
    /// Builds the property for the labels's <see cref="ILabel.Text"/>.
    /// </summary>
    protected virtual void BuildLabelTextProperty(IPropertyBuildContext<ILabel> context) {
      context.AddEntry(TextProperty
                       , new ValueGetterDelegate<string>(delegate { return context.CurrentInstance.Text; })
                       , new ValueSetterDelegate<string>(delegate(string value) {
                                                           IGraph graph = context.Lookup(typeof(IGraph)) as IGraph;
                                                           if (graph != null) {
                                                             graph.SetLabelText(context.CurrentInstance, value);
                                                           }
                                                         }), null);
    }
  }
}