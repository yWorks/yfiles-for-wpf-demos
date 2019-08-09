/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Algorithms;
using yWorks.Algorithms.Geometry;
using yWorks.Layout;
using yWorks.Layout.Multipage;
using yWorks.Graph.LabelModels;
using yWorks.Graph.PortLocationModels;

namespace Demo.yFiles.Layout.MultiPage
{
  public class MultiPageIGraphBuilder
  {
    /// <summary>
    /// The node data mapper key.
    /// </summary>
    /// <remarks>
    /// Use this key to get the <see cref="NodeData" /> from the <see cref="MapperRegistry"/> of one of the view graphs.
    /// </remarks>
    public const String MapperKeyNodeData = "Demo.yFiles.Layout.MultiPage.MultiPageIGraphBuilder#MapperKeyNodeData";

    private readonly MultiPageLayoutResult result;

    private readonly DictionaryMapper<Node, INode> layoutToViewNode = new DictionaryMapper<Node, INode>();
    private readonly DictionaryMapper<IPort, IPort> modelToViewPort = new DictionaryMapper<IPort, IPort>();
    private readonly DictionaryMapper<INode, Node> viewToLayoutNode = new DictionaryMapper<INode, Node>();

    /// <summary>
    ///  Creates a new instance for the given model graph and the given <see cref="MultiPageLayout"/>. 
    /// </summary>
    /// <param name="result">The <see cref="MultiPageLayout"/>: a holder for the pages created by the <see cref="MultiPageLayout"/>.</param>
    public MultiPageIGraphBuilder(MultiPageLayoutResult result) {
      // initialize the graph item defaults with the null styles
      NormalEdgeDefaults = new EdgeDefaults { Style = NullEdgeStyle, Labels = { Style = NullLabelStyle, LayoutParameter = NullLabelModelParameter } };
      ConnectorEdgeDefaults = new EdgeDefaults { Style = NullEdgeStyle, Labels = { Style = NullLabelStyle, LayoutParameter = NullLabelModelParameter } };
      ProxyEdgeDefaults = new EdgeDefaults { Style = NullEdgeStyle, Labels = { Style = NullLabelStyle, LayoutParameter = NullLabelModelParameter } };
      ProxyReferenceEdgeDefaults = new EdgeDefaults { Style = NullEdgeStyle, Labels = { Style = NullLabelStyle, LayoutParameter = NullLabelModelParameter } };
      NormalNodeDefaults = new NodeDefaults { Style = NullNodeStyle, Labels = { Style = NullLabelStyle, LayoutParameter = NullLabelModelParameter } };
      GroupNodeDefaults = new NodeDefaults { Style = NullNodeStyle, Labels = { Style = NullLabelStyle, LayoutParameter = NullLabelModelParameter } };
      ConnectorNodeDefaults = new NodeDefaults { Style = NullNodeStyle, Labels = { Style = NullLabelStyle, LayoutParameter = NullLabelModelParameter } };
      ProxyNodeDefaults = new NodeDefaults { Style = NullNodeStyle, Labels = { Style = NullLabelStyle, LayoutParameter = NullLabelModelParameter } };
      ProxyReferenceNodeDefaults = new NodeDefaults { Style = NullNodeStyle, Labels = { Style = NullLabelStyle, LayoutParameter = NullLabelModelParameter } };
      this.result = result;
    }

    #region Style properties

    /// <summary>
    /// The default node style used for node defaults.
    /// </summary>
    /// <remarks>
    /// This style instance is only a marker that tells the graph builder to clone the style
    /// of the corresponding node in the original graph.
    /// </remarks>
    public static readonly INodeStyle NullNodeStyle = new ShapeNodeStyle();

    /// <summary>
    /// The default edge style used for edge defaults.
    /// </summary>
    /// <remarks>
    /// This style instance is only a marker that tells the graph builder to clone the style
    /// of the corresponding edge in the original graph.
    /// </remarks>
    public static readonly IEdgeStyle NullEdgeStyle = new PolylineEdgeStyle();

    /// <summary>
    /// The default label style used for label defaults.
    /// </summary>
    /// <remarks>
    /// This style instance is only a marker that tells the graph builder to clone the style
    /// of the corresponding label in the original graph.
    /// </remarks>
    public static readonly ILabelStyle NullLabelStyle = new DefaultLabelStyle();

    /// <summary>
    /// The default label model parameter used for label defaults.
    /// </summary>
    /// <remarks>
    /// This label model parameter instance is only a marker that tells the graph builder to clone the 
    /// label model parameter of the corresponding label in the original graph.
    /// </remarks>
    public static readonly ILabelModelParameter NullLabelModelParameter = FreeLabelModel.Instance.CreateDefaultParameter();

    /// <summary>
    /// Gets the edge defaults for normal edges
    /// </summary>
    public IEdgeDefaults NormalEdgeDefaults { get; private set; }

    /// <summary>
    /// Gets the edge defaults for connector edges
    /// </summary>
    public IEdgeDefaults ConnectorEdgeDefaults { get; private set; }

    /// <summary>
    /// Gets the edge defaults for proxy edges
    /// </summary>
    public IEdgeDefaults ProxyEdgeDefaults { get; private set; }

    /// <summary>
    /// Gets the edge defaults for proxy reference edges
    /// </summary>
    public IEdgeDefaults ProxyReferenceEdgeDefaults { get; private set; }

    /// <summary>
    /// Gets the node defaults for normal nodes
    /// </summary>
    public INodeDefaults NormalNodeDefaults { get; private set; }

    /// <summary>
    /// Gets the node defaults for group nodes
    /// </summary>
    public INodeDefaults GroupNodeDefaults { get; private set; }

    /// <summary>
    /// Gets the node defaults for connector nodes
    /// </summary>
    public INodeDefaults ConnectorNodeDefaults { get; private set; }

    /// <summary>
    /// Gets the node defaults for proxy nodes
    /// </summary>
    public INodeDefaults ProxyNodeDefaults { get; private set; }

    /// <summary>
    /// Gets the node defaults for proxy reference nodes
    /// </summary>
    public INodeDefaults ProxyReferenceNodeDefaults { get; private set; }

    #endregion

    /// <summary>
    /// Creates a list of page graphs.
    /// </summary>
    /// <remarks>
    /// For each page of the multi-page layout, a graph is created that contains
    /// the page's nodes.
    /// A mapper is added to the returned graphs with the key <see cref="MapperKeyNodeData"/>
    /// to store the multi-page metadata for each node. The data stored can be used to retrieve 
    /// the referenced node and the page number.
    /// </remarks>
    /// <returns>The array of created graphs.</returns>
    public IGraph[] CreateViewGraphs() {
      var mapper = new WeakDictionaryMapper<INode, NodeData>();
      viewToLayoutNode.Clear();
      IGraph[] viewGraphs = new IGraph[result.PageCount()];

      for (int i = 0; i < viewGraphs.Length; i++) {
        viewGraphs[i] = CreatePageView(i, mapper);
      }

      foreach (var graph in viewGraphs) {
        foreach(INode node in graph.Nodes) {
          SetReferencingNodeId(graph, node);
        }
      }

      return viewGraphs;
    }

    /// <summary>
    /// Copies a single page into the given view graph. 
    /// </summary>
    /// <param name="pageNo">The page number.</param>
    /// <param name="mapper">The mapper that is used to store the <see cref="NodeData"/></param>
    /// <returns>The view graph where the page was created in.</returns>
    public IGraph CreatePageView(int pageNo, IMapper<INode, NodeData> mapper) {
      LayoutGraph pageLayoutGraph = result.GetPage(pageNo);
      IGraph pageView = new DefaultGraph();
      pageView.MapperRegistry.AddMapper(MapperKeyNodeData, mapper);
      CopyPage(pageLayoutGraph, pageView, pageNo, mapper);
      return pageView;
    }

    /// <summary>
    /// Copies the contents of the given <see cref="LayoutGraph"/> into the given view graph. 
    /// </summary>
    /// <param name="pageLayoutGraph">The layout graph to use as source.</param>
    /// <param name="pageView">The view graph to use as target.</param>
    /// <param name="pageNo">The page number of the page to copy.</param>
    /// <param name="mapper">The mapper that is used to store the <see cref="NodeData"/></param>
    private void CopyPage(LayoutGraph pageLayoutGraph, IGraph pageView, int pageNo, IMapper<INode, NodeData> mapper) {
      // copy all nodes
      foreach(var layoutNode in pageLayoutGraph.Nodes) {
        INode copiedNode = CopyNode(pageLayoutGraph, layoutNode, pageView);
        // add a mapping from layout node to view node
        layoutToViewNode[layoutNode] = copiedNode;
        // store the page number
        mapper[copiedNode].PageNumber = pageNo;
      }

      // copy all edges
      foreach(var layoutEdge in pageLayoutGraph.Edges) {
        CopyEdge(pageLayoutGraph, layoutEdge, pageView);
      }
    }

    /// <summary>
    /// Copy all labels of the given edge.
    /// </summary>
    private void CopyEdgeLabels(LayoutGraph pageLayoutGraph, IGraph pageView, Edge layoutEdge, IEdge copiedEdge, IEdge modelEdge, ILabelDefaults labelDefaults) {
      IEdgeLabelLayout[] edgeLabels = pageLayoutGraph.GetLabelLayout(layoutEdge);
      for (int i = 0; i < edgeLabels.Length; i++) {
        // get the label layout from the layout graph
        IEdgeLabelLayout edgeLabelLayout = edgeLabels[i];
        // get the original label from the model graph
        ILabel edgeModelLabel = modelEdge.Labels[i];
        CopyEdgeLabel(pageView, edgeLabelLayout, edgeModelLabel, copiedEdge, labelDefaults);
      }
    }

    /// <summary>
    /// Copy all labels of the given node.
    /// </summary>
    private void CopyNodeLabels(LayoutGraph pageLayoutGraph, IGraph pageView, Node layoutNode, INode copiedNode, INode modelNode) {
      INodeLabelLayout[] nodeLabels = pageLayoutGraph.GetLabelLayout(layoutNode);
      // for each label
      for (int i = 0; i < nodeLabels.Length; i++) {
        // get the layout from the layout graph
        INodeLabelLayout nodeLabelLayout = nodeLabels[i];
        // get the original label from the model graph
        ILabel nodeModelLabel = modelNode.Labels[i];
        CopyNodeLabel(pageView, nodeLabelLayout, nodeModelLabel, layoutNode, copiedNode);
      }
    }

    /// <summary>
    /// Copy one edge label. 
    /// </summary>
    /// <param name="pageView">The view (i.e. target) graph.</param>
    /// <param name="edgeLabelLayout">The layout of the label.</param>
    /// <param name="modelLabel">The original label.</param>
    /// <param name="viewEdge">The copied edge (from the view graph).</param>
    /// <param name="labelDefaults"></param>
    /// <returns>The copied label.</returns>
    private ILabel CopyEdgeLabel(IGraph pageView, IEdgeLabelLayout edgeLabelLayout, ILabel modelLabel, IEdge viewEdge, ILabelDefaults labelDefaults) {
      // get the style from edgeLabelStyle property. If none is set get it from the original (model) label.
      ILabelStyle style = (ILabelStyle)(labelDefaults.Style != NullLabelStyle ?
        labelDefaults.GetStyleInstance(viewEdge) : 
        modelLabel.Style.Clone());
      ILabelModelParameter parameter = labelDefaults.LayoutParameter != NullLabelModelParameter
                            ? labelDefaults.GetLayoutParameterInstance(viewEdge)
                            : (ILabelModelParameter)edgeLabelLayout.ModelParameter;
      // create a new label in the view graph using the style, 
      // the text from the original label and the layout from the layout graph
      ILabel viewLabel = pageView.AddLabel(viewEdge, modelLabel.Text, parameter, style);
      viewLabel.Tag = modelLabel.Tag;
      return viewLabel;
    }

    /// <summary>
    /// Copy one node label. 
    /// </summary>
    /// <param name="pageView">The view (i.e. target) graph.</param>
    /// <param name="nodeLabelLayout">The layout of the label.</param>
    /// <param name="modelLabel">The model (i.e. original) label.</param>
    /// <param name="layoutNode">The node in the layout graph. </param>
    /// <param name="viewNode">The node in the view graph.</param>
    /// <returns>The copied label.</returns>
    private ILabel CopyNodeLabel(IGraph pageView, INodeLabelLayout nodeLabelLayout, ILabel modelLabel, Node layoutNode, INode viewNode) {
      INodeInfo nodeInfo = result.GetNodeInfo(layoutNode);
      ILabelDefaults labelDefaults;
      // determine the style for the label
      switch (nodeInfo.Type) {
        case NodeType.Group:
          labelDefaults = GroupNodeDefaults.Labels;
          break;
        case NodeType.Connector:
          labelDefaults = ConnectorNodeDefaults.Labels;
          break;
        case NodeType.Proxy:
          labelDefaults = ProxyNodeDefaults.Labels;
          break;
        case NodeType.ProxyReference:
          labelDefaults = ProxyReferenceNodeDefaults.Labels;
          break;
        default:
          labelDefaults = NormalNodeDefaults.Labels;
          break;
      }
      ILabelModelParameter parameter = labelDefaults.LayoutParameter != NullLabelModelParameter
                                         ? labelDefaults.GetLayoutParameterInstance(viewNode)
                                         : (ILabelModelParameter)nodeLabelLayout.ModelParameter;
      ILabelStyle style = labelDefaults.Style != NullLabelStyle
                            ? labelDefaults.GetStyleInstance(viewNode)
                            : (ILabelStyle) (modelLabel != null ? modelLabel.Style.Clone() : pageView.NodeDefaults.Labels.Style);
      string text = modelLabel != null ? modelLabel.Text : null;
      object tag = modelLabel != null ? modelLabel.Tag : null;
      
      // create a new label in the view graph using the style, 
      // the text and tag from the original label and the layout from the layout graph
      ILabel viewLabel = pageView.AddLabel(viewNode, text, parameter, style);
      viewLabel.Tag = tag;
      return viewLabel;
    }

    /// <summary>
    /// Returns a node of the output <see cref="IGraph"/> that corresponds
    /// to the provided node of the <see cref="LayoutGraph"/> returned by the
    /// multi-page layout.
    /// </summary>
    protected INode GetViewNode(Node layoutNode) {
      return layoutToViewNode[layoutNode];
    }

    /// <summary>
    /// Returns the layout node that corresponds
    /// to the provided node of the output graph.
    /// </summary>
    protected Node GetLayoutNode(INode viewNode) {
      return viewToLayoutNode[viewNode];
    }

    /// <summary>
    /// Returns a node of the original input graph that corresponds
    /// to the provided node of the <see cref="LayoutGraph"/> returned by the
    ///  multi-page layout.
    /// </summary>
    /// <remarks>
    /// As the multi-page layout introduces auxiliary nodes, this method
    /// might return <see langword="null"/>.
    /// </remarks>
    protected INode GetModelNode(Node layoutNode) {
      INodeInfo nodeInfo = result.GetNodeInfo(layoutNode);
      return nodeInfo.Id as INode;
    }

    /// <summary>
    /// Returns an edge of the original input graph that corresponds
    /// the provided edge of the <see cref="LayoutGraph"/> returned by the
    /// multi-page layout.
    /// </summary>
    /// <remarks>
    /// As the multi-page layout introduces auxiliary nodes, this method
    ///  might return <see langword="null"/>
    /// </remarks>
    protected IEdge GetModelEdge(Edge layoutEdge) {
      IEdgeInfo edgeInfo = result.GetEdgeInfo(layoutEdge);
      return edgeInfo.Id as IEdge;
    }

    /// <summary>
    /// If the provided <see cref="LayoutGraph"/> node has a represented node,
    /// returns the node of the original input graph that corresponds to this node.
    /// </summary>
    protected INode GetRepresentedNode(Node layoutNode) {
      INodeInfo nodeInfo = result.GetNodeInfo(layoutNode);
      // represented node is the Node in the
      // CopiedLayoutIGraph which is the LayoutGraph representation
      // of the model graph
      Node representedNode = nodeInfo.RepresentedNode;
      if (null != representedNode) {
        CopiedLayoutGraph copiedLayoutGraph = representedNode.Graph as CopiedLayoutGraph;
        if (null != copiedLayoutGraph) {
          // translate it into the corresponding INode from the model graph.
          return (INode)copiedLayoutGraph.GetOriginalNode(representedNode);
        }
      }
      return null;
    }

    /// <summary>
    /// If the provided <see cref="LayoutGraph"/> edge has a represented edge,
    ///  returns the edge of the original input graph that corresponds to this edge.
    /// </summary>
    protected IEdge GetRepresentedEdge(Edge layoutEdge) {
      IEdgeInfo edgeInfo = result.GetEdgeInfo(layoutEdge);
      Edge representedEdge = edgeInfo.RepresentedEdge;
      if (null != representedEdge) {
        CopiedLayoutGraph copiedLayoutGraph = representedEdge.Graph as CopiedLayoutGraph;
        if (null != copiedLayoutGraph) {
          return (IEdge)copiedLayoutGraph.GetOriginalEdge(representedEdge);
        }
      }
      return null;
    }

    /// <summary>
    /// Returns a port of the resulting graph view that corresponds to the
    ///  provided port of the original input graph.
    /// </summary>
    protected IPort GetViewPort(IPort layoutPort) {
      return modelToViewPort[layoutPort];
    }

    /// <summary>
    /// Called by the various edge creation callbacks to create an edge in the resulting graph view
    /// that corresponds to the provided <paramref name="layoutEdge"/>.
    /// </summary>
    /// <remarks>
    /// If a model edge is provided, the edge will be created between the copies of the corresponding
    /// source/target ports.
    /// </remarks>
    ///<param name="pageLayoutGraph">The layout graph representing the current page.</param>
    ///<param name="pageView">The <see cref="IGraph"/> that is built to show the multi-page layout in a graph canvas.</param>
    ///<param name="layoutEdge">The edge of the layout graph that should be copied.</param>
    ///<param name="modelEdge">The edge of the original input graph that corresponds to the <paramref name="layoutEdge"/> (may be <see langword="null"/>).</param>
    ///<param name="edgeDefaults"></param>
    ///<returns>The created edge</returns>
    /// <seealso cref="CreateConnectorEdge"/>
    /// <seealso cref="CreateNormalEdge"/>
    /// <seealso cref="CreateProxyEdge"/>
    /// <seealso cref="CreateProxyReferenceEdge"/>
    protected IEdge CreateEdgeCore(LayoutGraph pageLayoutGraph, IGraph pageView, Edge layoutEdge, IEdge modelEdge, IEdgeDefaults edgeDefaults) {
      IEdge viewEdge;
      if (modelEdge != null) {
        // if the edge has a model edge: create the copied edge between
        // the copies of its source and target ports
        IPort modelSourcePort = modelEdge.SourcePort;
        IPort modelTargetPort = modelEdge.TargetPort;
        IPort viewSourcePort = GetViewPort(modelSourcePort);
        IPort viewTargetPort = GetViewPort(modelTargetPort);
        IEdgeStyle style = (IEdgeStyle) (edgeDefaults.Style != NullEdgeStyle ? 
                                edgeDefaults.GetStyleInstance() : 
                                modelEdge.Style.Clone());
        viewEdge = pageView.CreateEdge(viewSourcePort, viewTargetPort, style, modelEdge.Tag);
      } else {
        // otherwise create it between the copies of its source and target nodes
        INode viewSource = GetViewNode(layoutEdge.Source);
        INode viewTarget = GetViewNode(layoutEdge.Target);
        viewEdge = pageView.CreateEdge(viewSource, viewTarget);
      }

      // adjust the port location
      YPoint newSourcePortLocation = pageLayoutGraph.GetSourcePointAbs(layoutEdge);
      YPoint newTargetPortLocation = pageLayoutGraph.GetTargetPointAbs(layoutEdge);
      pageView.SetPortLocation(viewEdge.SourcePort, newSourcePortLocation.ToPointD());
      pageView.SetPortLocation(viewEdge.TargetPort, newTargetPortLocation.ToPointD());

      // and copy the bends
      IEdgeLayout edgeLayout = pageLayoutGraph.GetLayout(layoutEdge);
      for (int i = 0; i < edgeLayout.PointCount(); i++) {
        YPoint bendLocation = edgeLayout.GetPoint(i);
        pageView.AddBend(viewEdge, new PointD(bendLocation.X, bendLocation.Y), i);
      }

      return viewEdge;
    }

    /// <summary>
    /// Copies the given edge. Delegate to the type specific implementations.
    /// </summary>
    /// <param name="pageLayoutGraph">The layout graph.</param>
    /// <param name="layoutEdge">The edge in the layout graph.</param>
    /// <param name="pageView"> The view graph to create the copy in.</param>
    /// <returns>The copied edge.</returns>
    private IEdge CopyEdge(LayoutGraph pageLayoutGraph, Edge layoutEdge, IGraph pageView) {
      IEdgeInfo edgeInfo = result.GetEdgeInfo(layoutEdge);
      switch (edgeInfo.Type) {
        case EdgeType.Normal:
          return CreateNormalEdge(pageLayoutGraph, layoutEdge, pageView);
        case EdgeType.Connector:
          return CreateConnectorEdge(pageLayoutGraph, layoutEdge, pageView);
        case EdgeType.Proxy:
          return CreateProxyEdge(pageLayoutGraph, layoutEdge, pageView);
        case EdgeType.ProxyReference:
          return CreateProxyReferenceEdge(pageLayoutGraph, layoutEdge, pageView);
      }
      throw new ArgumentException("unknown edge type");
    }

    /// <summary>
    /// Create a normal edge, i.e., an edge that directly corresponds to an edge of the original input graph.
    /// </summary>
    /// <param name="pageLayoutGraph">The layout graph representing the current page</param>
    /// <param name="layoutEdge">The edge of the layout graph that should be copied</param>
    /// <param name="pageView">The <see cref="IGraph"/> that is built to show the multi-page layout in a graph canvas</param>
    /// <returns>The created edge</returns>
    /// <seealso cref="CreateEdgeCore"/>
    protected IEdge CreateNormalEdge(LayoutGraph pageLayoutGraph, Edge layoutEdge, IGraph pageView) {
      IEdge modelEdge = GetModelEdge(layoutEdge);
      IEdge edge = CreateEdgeCore(pageLayoutGraph, pageView, layoutEdge, modelEdge, NormalEdgeDefaults);
      CopyEdgeLabels(pageLayoutGraph, pageView, layoutEdge, edge, modelEdge, NormalEdgeDefaults.Labels);
      return edge;
    }

    /// <summary>
    /// Create a connector edge.
    /// </summary>
    /// <remarks>A connector edge is an edge connected to a connector node, i.e., it represents an edge
    /// of the input graph whose endpoints lie on different pages.
    /// </remarks>
    /// <param name="pageLayoutGraph">The layout graph representing the current page</param>
    /// <param name="layoutEdge">The edge of the layout graph that should be copied</param>
    /// <param name="pageView">The <see cref="IGraph"/> that is built to show the multi-page layout in a graph canvas</param>
    /// <returns>The created edge</returns>
    /// <seealso cref="CreateEdgeCore"/>
    protected IEdge CreateConnectorEdge(LayoutGraph pageLayoutGraph, Edge layoutEdge, IGraph pageView) {
      IEdge representedEdge = GetRepresentedEdge(layoutEdge);
      IEdge viewEdge = CreateEdgeCore(pageLayoutGraph, pageView, layoutEdge, representedEdge, ConnectorEdgeDefaults);
      CopyEdgeLabels(pageLayoutGraph, pageView, layoutEdge, viewEdge, representedEdge, ConnectorEdgeDefaults.Labels);
      return viewEdge;
    }

    /// <summary>
    /// Create a proxy edge.
    /// </summary>
    /// <remarks>
    /// A proxy edge is an edge connected to a proxy node, i.e., a node that is a proxy
    /// for an original node located on a different page.
    /// </remarks>
    /// <param name="pageLayoutGraph">The layout graph representing the current page</param>
    /// <param name="layoutEdge">The edge of the layout graph that should be copied</param>
    /// <param name="pageView">The <see cref="IGraph"/> that is built to show the multi-page layout in a graph canvas</param>
    /// <returns>The created edge</returns>
    /// <seealso cref="CreateEdgeCore"/>
    protected IEdge CreateProxyEdge(LayoutGraph pageLayoutGraph, Edge layoutEdge, IGraph pageView) {
      IEdge representedEdge = GetRepresentedEdge(layoutEdge);
      IEdge viewEdge = CreateEdgeCore(pageLayoutGraph, pageView, layoutEdge, representedEdge, ProxyEdgeDefaults);
      CopyEdgeLabels(pageLayoutGraph, pageView, layoutEdge, viewEdge, representedEdge, ProxyEdgeDefaults.Labels);
      return viewEdge;
    }

    /// <summary>
    ///  Create a proxy reference edge.
    /// </summary>
    /// <remarks>
    ///  A proxy reference edge is an edge connected to a proxy reference node, i.e., a node that
    /// refers to a proxy of an original node located on a different page.
    /// </remarks>
    /// <param name="pageLayoutGraph">The layout graph representing the current page</param>
    /// <param name="layoutEdge">The edge of the layout graph that should be copied</param>
    /// <param name="pageView">The <see cref="IGraph"/> that is built to show the multi-page layout in a graph canvas</param>
    /// <returns>The created edge</returns>
    /// <seealso cref="CreateEdgeCore"/>
    protected IEdge CreateProxyReferenceEdge(LayoutGraph pageLayoutGraph, Edge layoutEdge, IGraph pageView) {
      IEdge representedEdge = GetRepresentedEdge(layoutEdge);
      IEdge viewEdge = CreateEdgeCore(pageLayoutGraph, pageView, layoutEdge, representedEdge, ProxyReferenceEdgeDefaults);
      return viewEdge;
    }

    /// <summary>
    /// Called by the various node creation callbacks to create a node in the resulting graph view
    /// that corresponds to the provided <paramref name="layoutNode"/>.
    /// </summary>
    /// <remarks>
    /// If a model node is provided, the ports of the original node will be copied to the created view node.
    /// Also, a clone of the original node style will be used as the style of the created node.
    /// </remarks>
    /// <param name="pageLayoutGraph">The layout graph representing the current page</param>
    /// <param name="pageView">The <see cref="IGraph"/> that is built to show the multi-page layout in a graph canvas</param>
    /// <param name="layoutNode">The node of the layout graph that should be copied</param>
    /// <param name="modelNode">The node of the original input graph that corresponds to the <paramref name="layoutNode"/> (may be <see langword="null"/>)</param>
    /// <param name="isReferenceNode"></param>
    /// <param name="nodeDefaults"></param>
    /// <returns>the created node</returns>
    /// <seealso cref="CreateConnectorNode"/>
    /// <seealso cref="CreateNormalNode"/>
    /// <seealso cref="CreateGroupNode"/>
    /// <seealso cref="CreateProxyNode"/>
    /// <seealso cref="CreateProxyReferenceNode"/>
    protected INode CreateNodeCore(LayoutGraph pageLayoutGraph, IGraph pageView, Node layoutNode, INode modelNode, bool isReferenceNode, INodeDefaults nodeDefaults) {

      // get the layout from the layout graph
      INodeLayout nodeLayout = pageLayoutGraph.GetLayout(layoutNode);
      // get the style from the node defaults or the model node (or the default style if none is provided)
      INodeStyle style = (INodeStyle) (nodeDefaults.Style != NullNodeStyle
                                         ? nodeDefaults.GetStyleInstance()
                                         : (modelNode != null
                                              ? modelNode.Style.Clone()
                                              : pageView.NodeDefaults.Style.Clone()));
      var tag = modelNode != null ? modelNode.Tag : null;
      // create the copied node
      INode viewNode = pageView.CreateNode(new RectD(nodeLayout.X, nodeLayout.Y, nodeLayout.Width, nodeLayout.Height), style, tag);
      // copy the ports of the model node
      if (modelNode != null) {
        CopyPorts(pageView, layoutNode, viewNode, modelNode);
      }

      viewToLayoutNode[viewNode] = layoutNode;
      IMapper<INode, NodeData> referencingMapper = pageView.MapperRegistry.GetMapper<INode, NodeData>(MapperKeyNodeData);
      NodeData data = new NodeData { IsReferenceNode = isReferenceNode };
      referencingMapper[viewNode] = data;

      return viewNode;
    }

    /// <summary>
    /// Copy the given node. Delegate to the type specific implementations. 
    /// </summary>
    /// <param name="pageLayoutGraph">The layout graph for the page.</param>
    /// <param name="layoutNode">The node in the layout graph.</param>
    /// <param name="pageView">The view graph to copy the node to.</param>
    /// <returns>The copied node.</returns>
    private INode CopyNode(LayoutGraph pageLayoutGraph, Node layoutNode, IGraph pageView) {
      INodeInfo nodeInfo = result.GetNodeInfo(layoutNode);
      INode viewNode;
      switch (nodeInfo.Type) {
        case NodeType.Normal:
          viewNode = CreateNormalNode(pageLayoutGraph, layoutNode, pageView);
          break;
        case NodeType.Group:
          viewNode = CreateGroupNode(pageLayoutGraph, layoutNode, pageView);
          break;
        case NodeType.Connector:
          viewNode = CreateConnectorNode(pageLayoutGraph, layoutNode, pageView);
          break;
        case NodeType.Proxy:
          viewNode = CreateProxyNode(pageLayoutGraph, layoutNode, pageView);
          break;
        case NodeType.ProxyReference:
          viewNode = CreateProxyReferenceNode(pageLayoutGraph, layoutNode, pageView);
          break;
        default:
          throw new ArgumentException("unknown node type");
      }
      return viewNode;
    }

    /// <summary>
    /// Copy the ports from a provided node of the original input graph to a node of the resulting multi-page graph view.
    /// </summary>
    protected void CopyPorts(IGraph pageView, Node layoutNode, INode viewNode, INode modelNode) {

      foreach (IPort port in modelNode.Ports) {
        IPort viewPort = pageView.AddPort(viewNode, (IPortLocationModelParameter)port.LocationParameter.Clone());
        if (port.Style != null) {
          pageView.SetStyle(viewPort, (IPortStyle)port.Style.Clone());
        }
        modelToViewPort[port] = viewPort;
      }

    }

    /// <summary>
    /// Create a normal node, i.e., a node that directly corresponds to a node of the original input graph.
    /// </summary>
    /// <remarks>
    /// This implementation copies the labels of the corresponding node in the original input graph.
    /// Also the style of the original node is used for the returned node, unless the <c>NormalNodeStyle</c> is set.
    /// </remarks>
    /// <param name="pageLayoutGraph">The layout graph representing the current page</param>
    /// <param name="layoutNode">The node of the layout graph that should be copied</param>
    /// <param name="pageView">The <see cref="IGraph"/> that is built to show the multi-page layout in a graph canvas</param>
    /// <returns>The created node</returns>
    /// <seealso cref="CreateNodeCore"/>
    protected INode CreateNormalNode(LayoutGraph pageLayoutGraph, Node layoutNode, IGraph pageView) {
      INode modelNode = GetModelNode(layoutNode);
      INode viewNode = CreateNodeCore(pageLayoutGraph, pageView, layoutNode, modelNode, false, NormalNodeDefaults);
      CopyNodeLabels(pageLayoutGraph, pageView, layoutNode, viewNode, modelNode);
      return viewNode;
    }

    /// <summary>
    /// Create a group node, i.e., a node that directly corresponds to a group node of the original input graph.
    /// </summary>
    /// <remarks>
    ///  This implementation copies the labels of the corresponding node in the original input graph.
    /// Also the style of the original node is used for the returned node, unless the <c>GroupNodeStyle</c> is set.
    /// </remarks>
    /// <param name="pageLayoutGraph">The layout graph representing the current page</param>
    /// <param name="layoutNode">The node of the layout graph that should be copied</param>
    /// <param name="pageView">The <see cref="IGraph"/> that is built to show the multi-page layout in a graph canvas</param>
    /// <returns>The created node</returns>
    /// <seealso cref="CreateNodeCore"/>
    protected INode CreateGroupNode(LayoutGraph pageLayoutGraph, Node layoutNode, IGraph pageView) {
      INode modelNode = GetModelNode(layoutNode);
      INode viewNode = CreateNodeCore(pageLayoutGraph, pageView, layoutNode, modelNode, false, GroupNodeDefaults);
      CopyNodeLabels(pageLayoutGraph, pageView, layoutNode, viewNode, modelNode);
      return viewNode;
    }

    /// <summary>
    /// Create a connector node, i.e., a node that represents a "jump mark" to another connector node on a different page.
    /// </summary>
    /// <remarks>
    /// This implementation copies the labels of the represented node and applies the <c>ConnectorNodeStyle</c>.
    /// </remarks>
    /// <param name="pageLayoutGraph">The layout graph representing the current page</param>
    /// <param name="layoutNode">The node of the layout graph that should be copied</param>
    /// <param name="pageView">The <see cref="IGraph"/> that is built to show the multi-page layout in a graph canvas</param>
    /// <returns>The created node</returns>
    /// <seealso cref="CreateNodeCore"/>
    protected INode CreateConnectorNode(LayoutGraph pageLayoutGraph, Node layoutNode, IGraph pageView) {
      INode representedNode = GetRepresentedNode(layoutNode);
      INode viewNode = CreateNodeCore(pageLayoutGraph, pageView, layoutNode, representedNode, true, ConnectorNodeDefaults);
      CopyNodeLabels(pageLayoutGraph, pageView, layoutNode, viewNode, representedNode);
      return viewNode;
    }

    private void SetReferencingNodeId(IGraph pageView, INode viewNode) {
      INodeInfo nodeInfo = result.GetNodeInfo(GetLayoutNode(viewNode));
      IMapper<INode, NodeData> referencingMapper = pageView.MapperRegistry.GetMapper<INode, NodeData>(MapperKeyNodeData);
      NodeData data = referencingMapper[viewNode];
      if (data.IsReferenceNode) {
        Node referencingNode = nodeInfo.ReferencingNode;
        data.ReferencedNode = GetViewNode(referencingNode);
      }
    }

    /// <summary>
    /// Create a proxy node, i.e., a node that "partially" represents a node of the input graph.
    /// </summary>
    /// <remarks>
    /// This implementation copies the labels of the represented node and applies the <c>ProxyNodeStyle</c>.
    /// </remarks>
    /// <param name="pageLayoutGraph">The layout graph representing the current page</param>
    /// <param name="layoutNode">The node of the layout graph that should be copied</param>
    /// <param name="pageView">The <see cref="IGraph"/> that is built to show the multi-page layout in a graph canvas</param>
    /// <returns>The created node</returns>
    /// <seealso cref="CreateNodeCore"/>
    protected INode CreateProxyNode(LayoutGraph pageLayoutGraph, Node layoutNode, IGraph pageView) {
      INode representedNode = GetRepresentedNode(layoutNode);
      INode viewNode = CreateNodeCore(pageLayoutGraph, pageView, layoutNode, representedNode, true, ProxyNodeDefaults);
      CopyNodeLabels(pageLayoutGraph, pageView, layoutNode, viewNode, representedNode);
      return viewNode;
    }

    /// <summary>
    /// Create a proxy reference node, i.e., a node referencing a proxy node.
    /// </summary>
    /// <remarks>
    /// This implementation copies the labels of the represented node and applies the <c>ProxyReferenceNodeStyle</c>.
    /// </remarks>
    /// <param name="pageLayoutGraph">The layout graph representing the current page</param>
    /// <param name="layoutNode">The node of the layout graph that should be copied</param>
    /// <param name="pageView">The <see cref="IGraph"/> that is built to show the multi-page layout in a graph canvas</param>
    /// <returns>The created node</returns>
    /// <seealso cref="CreateNodeCore"/>
    protected INode CreateProxyReferenceNode(LayoutGraph pageLayoutGraph, Node layoutNode, IGraph pageView) {
      INode representedNode = GetRepresentedNode(layoutNode);
      INode viewNode = CreateNodeCore(pageLayoutGraph, pageView, layoutNode, representedNode, true, ProxyReferenceNodeDefaults);
      INodeInfo nodeInfo = result.GetNodeInfo(layoutNode);
      Node referencingNode = nodeInfo.ReferencingNode;
      int targetPage = result.GetNodeInfo(referencingNode).PageNo;
      ILabelStyle style = ProxyNodeDefaults.Labels.Style != NullLabelStyle
                            ? ProxyNodeDefaults.Labels.GetStyleInstance(viewNode)
                            : pageView.NodeDefaults.Labels.GetStyleInstance(viewNode);
      ILabelModelParameter parameter = ProxyNodeDefaults.Labels.LayoutParameter != NullLabelModelParameter
                            ? ProxyNodeDefaults.Labels.GetLayoutParameterInstance(viewNode)
                            : pageView.NodeDefaults.Labels.GetLayoutParameterInstance(viewNode);
      pageView.AddLabel(viewNode, "p" + targetPage, parameter, style);
      return viewNode;
    }

    /// <summary>
    /// This class is used to store reference information about a node.
    /// </summary>
    /// <remarks>
    /// Connector, proxy, or proxy reference nodes hold references to another node.</remarks>
    public class NodeData
    {
      public NodeData() {
        IsReferenceNode = false;
        ReferencedNode = null;
        PageNumber = -1;
      }

      /// <summary>
      /// Gets or sets if the node represents a reference to another node
      /// </summary>
      /// <remarks>This is the case if the node is a connector, proxy, or proxy reference node.</remarks>
      public bool IsReferenceNode { get; internal set; }

      /// <summary>
      /// Gets or sets the referenced node.
      /// </summary>
      public INode ReferencedNode { get; internal set; }

      public int PageNumber { get; internal set; }
    }
  }
}
