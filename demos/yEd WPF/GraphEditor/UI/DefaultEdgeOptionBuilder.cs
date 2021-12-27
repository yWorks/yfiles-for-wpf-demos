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
using Demo.yFiles.Option.DataBinding;
using Demo.yFiles.Option.Handler;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.GraphEditor.Option
{
  /// <summary>
  /// Default implementation of <see cref="IOptionBuilder"/> for <see cref="IEdge"/> objects
  /// that recursively creates <see cref="IOptionItem"/>s for a <see cref="IPropertyMap"/>
  /// that contains properties of <see cref="IEdge"/> instances. This implementation contains
  /// the first label and the <see cref="IEdge.Style"/> of the edge.
  /// </summary>
  /// <remarks>
  /// Use this implementation together with <see cref="DefaultEdgePropertyMapBuilder"/>
  /// in a scenario where <see cref="ISelectionProvider{T}"/> is used to create an
  /// <see cref="OptionHandler"/> to show the properties of an <see cref="IEdge"/>.
  /// </remarks>
  public class DefaultEdgeOptionBuilder : IOptionBuilder
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


    #region IOptionBuilder Members

    /// <inheritdoc/>
    public virtual void AddItems(IOptionBuilderContext context, Type subjectType, object subject) {
      //layout group
      IEdge edge = (IEdge) subject;
      BuildLabelOptions(context, edge);
      BuildStyleOptions(context, edge);
    }

    #endregion

    /// <summary>
    /// Builds the options for the style of the edge instance.
    /// </summary>
    /// <param name="context">The context to use for building.</param>
    /// <param name="edge">The current edge instance.</param>
    protected virtual void BuildStyleOptions(IOptionBuilderContext context, IEdge edge) {
      context = context.CreateChildContext(StylePropertyName);
      //style group...
      //retrieve current style...
      IEdgeStyle style = edge.Style;
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
    protected virtual IOptionBuilder GetStyleOptionBuilder(IOptionBuilderContext context, IEdgeStyle style) {
      return context.GetOptionBuilder(style);
    }

    /// <summary>
    /// Builds the options for the first label of the edge instance.
    /// </summary>
    /// <param name="context">The context to use for building.</param>
    /// <param name="subject">The current edge instance.</param>
    protected virtual void BuildLabelOptions(IOptionBuilderContext context, ILabelOwner subject) {
      if (subject.Labels.Count > 0) {
        context = context.CreateChildContext(LabelPropertyName);
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
  }

  /// <summary>
  /// A simple default implementation of the <see cref="IPropertyMapBuilder"/>
  /// that recursively creates a property map for an <see cref="IEdge"/> which contains
  /// the first label and the <see cref="IEdge.Style"/> of the edge.
  /// </summary>
  /// <remarks>
  /// Use this implementation together with <see cref="DefaultEdgeOptionBuilder"/>
  /// in a scenario where <see cref="ISelectionProvider{T}"/> is used to create an
  /// <see cref="OptionHandler"/> to show the properties of an <see cref="IEdge"/>.
  /// <br/>
  /// The <see cref="StyleAssignmentPolicy"/> can be used to determine whether 
  /// the implementation should try to modify an existing style instance or
  /// create a new instance every time the style is changed.
  /// This implementation depends on the fact that an implementation of <see cref="IGraph"/>
  /// can be found in the <see cref="ILookup.Lookup"/> of <see cref="IPropertyBuildContext{TSubject}"/>
  /// to <see cref="IGraph.SetStyle(IEdge,IEdgeStyle)">assign a new style to an edge</see> when necessary.
  /// </remarks>
  public class DefaultEdgePropertyMapBuilder : PropertyMapBuilderBase<IEdge> {
    private AssignmentPolicy styleAssignmentPolicy = AssignmentPolicy.ModifyInstance;

    /// <summary>
    /// Creates a new instance of a builder for <see cref="IEdge"/> instances.
    /// </summary>
    public DefaultEdgePropertyMapBuilder() : base(false) { }

    /// <summary>
    /// Determines how changes to the <see cref="IEdge.Style"/> instance
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
    /// Called by the base class to actually build the properties for the <see cref="IEdge"/>.
    /// </summary>
    /// <remarks>
    /// This implementation simply delegates its work to <see cref="BuildLabelProperties{T}"/>
    /// and <see cref="BuildStyleProperties"/>.
    /// </remarks>
    /// <param name="context">The context to use as the builder.</param>
    protected override void BuildPropertyMapImpl(IPropertyBuildContext<IEdge> context) {
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
      IPropertyBuildContext<ILabel> childContext = context.CreateChildContext<ILabel>(DefaultEdgeOptionBuilder.LabelPropertyName, 
                                                                                      delegate() {
                                                                                        T item = context.CurrentInstance;
                                                                                        return item.Labels.Count > 0 ? item.Labels[0] : null;
                                                                                      },
                                                                                      delegate(ILabel newInstance) {
                                                                                      }, AssignmentPolicy.ModifyInstance);

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
    /// Method that populates the <see cref="IPropertyMap"/> for edge styles in a given context.
    /// </summary>
    /// <remarks>
    /// This implementation uses <see cref="GetStylePropertyMapBuilder"/> to retrieve the builder for the style.
    /// </remarks>
    /// <param name="context">The context to use for queries.</param>
    protected virtual void BuildStyleProperties(IPropertyBuildContext<IEdge> context) {
      IEdge edge = context.CurrentInstance;
      if (edge != null) {
        IEdgeStyle style = edge.Style;
        if (style != null) {
          IPropertyMapBuilder propertyBuilder = GetStylePropertyMapBuilder(context, style);
          if (propertyBuilder != null) {
            GetInstanceDelegate<IEdgeStyle> styleGetter = delegate { return context.CurrentInstance.Style; };
            SetInstanceDelegate<IEdgeStyle> styleSetter = delegate(IEdgeStyle newInstance) {
              IGraph graph = context.Lookup(typeof(IGraph)) as IGraph;
              IEdge currentEdge = context.CurrentInstance;
              if (graph != null) {
                graph.SetStyle(currentEdge, newInstance);
              }
            };
            propertyBuilder.BuildPropertyMap(context.CreateChildContext(DefaultEdgeOptionBuilder.StylePropertyName, styleGetter, styleSetter, styleAssignmentPolicy));
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
    /// <param name="style">The style instance currently associated with the edge.</param>
    /// <returns>A builder or <see langword="null"/></returns>
    protected virtual IPropertyMapBuilder GetStylePropertyMapBuilder(IPropertyBuildContext<IEdge> context, IEdgeStyle style) {
      return context.GetPropertyMapBuilder(style);
    }
  }
}