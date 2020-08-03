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
using Demo.yFiles.Option.DataBinding;
using Demo.yFiles.Option.Handler;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.GraphEditor.Option
{
  /// <summary>
  /// Default implementation of <see cref="IOptionBuilder"/> for <see cref="INode"/> objects
  /// that recursively creates <see cref="IOptionItem"/>s for a <see cref="IPropertyMap"/>
  /// that contains properties of <see cref="INode"/> instances. This implementation contains
  /// the first label and the <see cref="INode.Style"/> of the node, as well as properties
  /// for the node's <see cref="INode.Layout"/>.
  /// </summary>
  /// <remarks>
  /// Use this implementation together with <see cref="DefaultNodePropertyMapBuilder"/>
  /// in a scenario where <see cref="ISelectionProvider{T}"/> is used to create an
  /// <see cref="OptionHandler"/> to show the properties of an <see cref="INode"/>.
  /// </remarks>
  public class DefaultNodeOptionBuilder : IOptionBuilder
  {
    /// <inheritdoc/>
    public virtual void AddItems(IOptionBuilderContext context, Type subjectType, object subject) {
      //layout group
      INode node = (INode) subject;
      BuildLabelOptions(context, node);
      BuildStyleOptions(context, node);
      BuildLayoutOptions(context);
    }

    /// <summary>
    /// Builds the options for the style of the node instance.
    /// </summary>
    /// <param name="context">The context to use for building.</param>
    /// <param name="node">The current node instance.</param>
    protected virtual void BuildStyleOptions(IOptionBuilderContext context, INode node) {
      context = context.CreateChildContext(DefaultNodePropertyMapBuilder.StylePropertyName);
      //style group...
      //retrieve current style...
      INodeStyle style = node.Style;
      if (style != null) {
        //retrieve OptionBuilder from style
        IOptionBuilder builder = GetStyleOptionBuilder(context, style);
        if (builder != null) {
          builder.AddItems(context, style.GetType(), style);
        }
      }
    }

    /// <summary>
    /// Method that retrieves an <see cref="IOptionBuilder"/> instance for the given style and context.
    /// </summary>
    /// <remarks>
    /// This implementation simply delegates to <see cref="IOptionBuilderContext.GetOptionBuilder(object)"/>.
    /// </remarks>
    /// <param name="context">The context to use.</param>
    /// <param name="style">The current style instance.</param>
    /// <returns>The builder to use or <see langword="null"/>.</returns>
    protected virtual IOptionBuilder GetStyleOptionBuilder(IOptionBuilderContext context, INodeStyle style) {
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

    /// <summary>
    /// This method <see cref="IOptionBuilderContext.BindItem(IOptionItem,string)"/> binds the
    /// <see cref="INode.Layout"/> properties to corresponding option items.
    /// </summary>
    /// <param name="context">The context to use.</param>
    protected virtual void BuildLayoutOptions(IOptionBuilderContext context) {
      context = context.CreateChildContext(DefaultNodePropertyMapBuilder.LayoutPropertyName);
      context.BindItem(new OptionItem(DefaultNodePropertyMapBuilder.LayoutXName) { Value = 0.0d, Type = typeof(double) }, DefaultNodePropertyMapBuilder.LayoutXName);
      context.BindItem(new OptionItem(DefaultNodePropertyMapBuilder.LayoutYName) { Value = 0.0d, Type = typeof(double) }, DefaultNodePropertyMapBuilder.LayoutYName);
      context.BindItem(new OptionItem(DefaultNodePropertyMapBuilder.LayoutWidthName) { Value = 0.0d, Type = typeof(double) }, DefaultNodePropertyMapBuilder.LayoutWidthName);
      context.BindItem(new OptionItem(DefaultNodePropertyMapBuilder.LayoutHeightName) { Value = 0.0d, Type = typeof(double) }, DefaultNodePropertyMapBuilder.LayoutHeightName);
    }
  }

  /// <summary>
  /// A simple default implementation of the <see cref="IPropertyMapBuilder"/>
  /// that recursively creates a property map for an <see cref="INode"/> which contains
  /// the first label and the <see cref="INode.Style"/> of the node.
  /// </summary>
  /// <remarks>
  /// Use this implementation together with <see cref="DefaultNodeOptionBuilder"/>
  /// in a scenario where <see cref="ISelectionProvider{T}"/> is used to create an
  /// <see cref="OptionHandler"/> to show the properties of an <see cref="INode"/>.
  /// <br/>
  /// The <see cref="StyleAssignmentPolicy"/> can be used to determine whether 
  /// the implementation should try to modify an existing style instance or
  /// create a new instance every time the style is changed.
  /// This implementation depends on the fact that an implementation of <see cref="IGraph"/>
  /// can be found in the <see cref="ILookup.Lookup"/> of <see cref="IPropertyBuildContext{TSubject}"/>
  /// to <see cref="IGraph.SetStyle(INode,INodeStyle)">assign a new style to a node</see> when necessary.
  /// </remarks>
  public class DefaultNodePropertyMapBuilder : PropertyMapBuilderBase<INode>
  {
    /// <summary>
    /// The <see cref="IOptionBuilderContext.CreateChildContext">child context prefix</see>
    /// used for the style of the item.
    /// </summary>
    public const string StylePropertyName = "Style";
    /// <summary>
    /// The <see cref="IOptionBuilderContext.CreateChildContext">child context prefix</see>
    /// used for the label of the item.
    /// </summary>
    public const string LabelPropertyName = "Label";
    /// <summary>
    /// The <see cref="IOptionBuilderContext.CreateChildContext">child context prefix</see>
    /// used for the layout of the node.
    /// </summary>
    public const string LayoutPropertyName = "Layout";
    /// <summary>
    /// The <see cref="IOptionBuilderContext.BindItem(IOptionItem,string)">entry name</see>
    /// used for the x property of the layout.
    /// </summary>
    public const string LayoutXName = "X";
    /// <summary>
    /// The <see cref="IOptionBuilderContext.BindItem(IOptionItem,string)">entry name</see>
    /// used for the y property of the layout.
    /// </summary>
    public const string LayoutYName = "Y";
    /// <summary>
    /// The <see cref="IOptionBuilderContext.BindItem(IOptionItem,string)">entry name</see>
    /// used for the height property of the layout.
    /// </summary>
    public const string LayoutHeightName = "Height";
    /// <summary>
    /// The <see cref="IOptionBuilderContext.BindItem(IOptionItem,string)">entry name</see>
    /// used for the width property of the layout.
    /// </summary>
    public const string LayoutWidthName = "Width";

    private AssignmentPolicy styleAssignmentPolicy = AssignmentPolicy.ModifyInstance;

    /// <summary>
    /// Creates a new instance of a builder for <see cref="INode"/> instances.
    /// </summary>
    public DefaultNodePropertyMapBuilder() : base(false) { }

    /// <summary>
    /// Determines how changes to the <see cref="INode.Style"/> instance
    /// that are triggered by modifying the properties this instance creates 
    /// are treated.
    /// </summary>
    /// <remarks>
    /// A value of <see cref="AssignmentPolicy.CreateNewInstance"/>
    /// will result in the creation of new style instances every time a property of the
    /// style is modified, whereas <see cref="AssignmentPolicy.ModifyInstance"/>
    /// will result in a direct style modification.<br/>
    /// The default is <see cref="AssignmentPolicy.CreateNewInstance"/>.
    /// </remarks>
    public AssignmentPolicy StyleAssignmentPolicy {
      get { return styleAssignmentPolicy; }
      set { styleAssignmentPolicy = value; }
    }

    /// <summary>
    /// Called by the base class to actually build the properties for the <see cref="INode"/>.
    /// </summary>
    /// <remarks>
    /// This implementation simply delegates its work to 
    /// <see cref="BuildLayoutProperties"/>,
    /// <see cref="BuildLabelProperties{T}"/>,
    /// and <see cref="BuildStyleProperties"/>.
    /// </remarks>
    /// <param name="context">The context to use as the builder.</param>
    protected override void BuildPropertyMapImpl(IPropertyBuildContext<INode> context) {
      BuildLayoutProperties(context);
      BuildLabelProperties(context);
      BuildStyleProperties(context);
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
    /// Method that <see cref="IPropertyBuildContext{TSubject}.AddEntry(string,IValueGetter,Demo.yFiles.Option.DataBinding.IValueSetter)">adds entries</see>
    /// for the <see cref="INode.Layout"/> of a node.
    /// </summary>
    /// <remarks>
    /// This implementation create a <see cref="IPropertyBuildContext{TSubject}.CreateChildContext{TChild}">child context</see>
    /// and adds properties for x,y,width, and height.
    /// </remarks>
    /// <param name="context">The context to use.</param>
    protected virtual void BuildLayoutProperties(IPropertyBuildContext<INode> context) {

      IPropertyBuildContext<IMutableRectangle> childContext = context.CreateChildContext<IMutableRectangle>(LayoutPropertyName,
                                                                                      delegate {
                                                                                        return (IMutableRectangle) context.CurrentInstance.Lookup(typeof(IMutableRectangle));
                                                                                      },
                                                                                      delegate {
                                                                                        //we work directly on the mutable rectangle
                                                                                      }, AssignmentPolicy.ModifyInstance);

      childContext.AddEntry<double>(LayoutXName, delegate { return childContext.CurrentInstance.X; },
                                    delegate(double value) {
                                      childContext.CurrentInstance.X = value;
                                    });
      childContext.AddEntry<double>(LayoutYName, delegate { return childContext.CurrentInstance.Y; },
                                    delegate(double value) {
                                      childContext.CurrentInstance.Y = value;
                                    });
      childContext.AddEntry<double>(LayoutWidthName, delegate { return childContext.CurrentInstance.Width; },
                                    delegate(double value) {
                                      childContext.CurrentInstance.Width = value;
                                    });
      childContext.AddEntry<double>(LayoutHeightName, delegate { return childContext.CurrentInstance.Height; },
                                    delegate(double value) {
                                      childContext.CurrentInstance.Height = value;
                                    });
    }


    /// <summary>
    /// Method that populates the <see cref="IPropertyMap"/> for edge styles in a given context.
    /// </summary>
    /// <remarks>
    /// This implementation uses <see cref="GetStylePropertyMapBuilder"/> to retrieve the builder for the style.
    /// </remarks>
    /// <param name="context">The context to use for queries.</param>
    protected virtual void BuildStyleProperties(IPropertyBuildContext<INode> context) {
      INode node = context.CurrentInstance;
      if (node != null) {
        INodeStyle style = node.Style;
        if (style != null) {
          GetInstanceDelegate<INodeStyle> styleGetter = delegate { return context.CurrentInstance.Style; };
          SetInstanceDelegate<INodeStyle> styleSetter = delegate(INodeStyle newValue) {
            IGraph graph = context.Lookup(typeof(IGraph)) as IGraph;
            INode currentNode = context.CurrentInstance;
            if (graph != null) {
              graph.SetStyle(currentNode, newValue);
              var foldingView = graph.Lookup(typeof(IFoldingView)) as IFoldingView;
              if (foldingView != null) {
                var masterNode = foldingView.GetMasterItem(currentNode);
                if (foldingView.IsInFoldingState(currentNode)) {
                  // update non-dummy node
                  foldingView.Manager.MasterGraph.SetStyle(masterNode, newValue);
                } else if (foldingView.IsExpanded(currentNode)) {
                  // update dummy node
                  if (foldingView.Manager.HasFolderNodeState(masterNode)) {
                    foldingView.Manager.GetFolderNodeState(masterNode).Style = newValue;
                  }
                }
              }
            }
          };
          IPropertyBuildContext<INodeStyle> styleContext =
            context.CreateChildContext(StylePropertyName, styleGetter, styleSetter, styleAssignmentPolicy);

          IPropertyMapBuilder propertyBuilder = GetStylePropertyMapBuilder(context, style);
          if (propertyBuilder != null) {
            GetInstanceDelegate<INodeStyle> innerStyleGetter = delegate { return styleContext.CurrentInstance; };
            SetInstanceDelegate<INodeStyle> innerStyleSetter = delegate(INodeStyle newValue) { styleContext.SetNewInstance(newValue); };
            propertyBuilder.BuildPropertyMap(styleContext.CreateChildContext(string.Empty, innerStyleGetter, innerStyleSetter, styleAssignmentPolicy));
          }
        }
      }
    }

    /// <summary>
    /// Method that retrieves the <see cref="IPropertyMapBuilder"/> for the provided style instance.
    /// </summary>
    /// <remarks>
    /// This implementation simply delegates to <see cref="IPropertyBuildContext{TSubject}.GetPropertyMapBuilder(object)"/>.
    /// </remarks>
    /// <param name="context">The context to use for queries.</param>
    /// <param name="style">The style instance currently associated with the node.</param>
    /// <returns>A builder or <see langword="null"/></returns>
    protected virtual IPropertyMapBuilder GetStylePropertyMapBuilder(IPropertyBuildContext<INode> context, INodeStyle style) {
      return context.GetPropertyMapBuilder(style);
    }
  }  
}