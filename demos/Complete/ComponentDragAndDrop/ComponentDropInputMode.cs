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

using System.Collections.Generic;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Graph.ComponentDragAndDrop
{
  /// <summary>
  ///  An <see cref="ItemDropInputMode{T}"/> specialized to drag and drop <see cref="IGraph">components</see>.
  /// </summary>
  /// <remarks>
  /// Each component is stored and handled as an <see cref="IGraph"/> instance
  /// where all nodes and edges belong solely to that component.
  /// </remarks>
  internal class ComponentDropInputMode : ItemDropInputMode<IGraph>
  {
    /// <summary>
    /// The center of the preview graph. 
    /// </summary>
    private PointD center;

    /// <summary>
    /// Constructs a new instance of class <see cref="ComponentDropInputMode"/>. 
    /// </summary>
    public ComponentDropInputMode() {
      ItemCreator = CreateComponent;
    }

    /// <summary>
    /// Creates the component in the graph after it has been dropped.
    /// </summary>
    /// <remarks>This method is called by the <see cref="ItemDropInputMode{T}.ItemCreator"/> that
    /// is set as default on this class.</remarks>
    /// <param name="context">The context for which the component should be created.</param>
    /// <param name="graph">The <see cref="IGraph">Graph</see> in which to create the component.</param>
    /// <param name="draggedGraph">The component that was dragged and should therefore be created.
    /// The nodes and edges of the component will be copied into the <paramref name="graph"/>.</param>
    /// <param name="dropTarget">The <see cref="IModelItem"/> on which the component is dropped. This is ignored here.</param>
    /// <param name="dropLocation">The location where the component has been dropped.</param>
    /// <returns>A newly created component.</returns>
    private IGraph CreateComponent(IInputModeContext context, IGraph graph, IGraph draggedGraph, IModelItem dropTarget, PointD dropLocation) {
      // move the component to the drop location
      var delta = dropLocation - GetCenter(draggedGraph);
      Move(draggedGraph, delta);

      // create the component and collect the dropped nodes
      var droppedNodes = new HashSet<INode>();
      new GraphCopier().Copy(draggedGraph, item => true, graph, PointD.Origin, (original, copy) => {
        var node = copy as INode;
        if (node != null) {
          droppedNodes.Add(node);
        }
      });

      // return the dropped component
      return new FilteredGraphWrapper(graph, droppedNodes.Contains);
    }

    /// <summary>
    /// Fills the specified graph that is used to preview the dragged component.
    /// </summary>
    /// <param name="previewGraph">The preview graph to fill.</param>
    protected override void PopulatePreviewGraph(IGraph previewGraph) {
      var draggedGraph = DraggedItem;
      if (draggedGraph == null) {
        return;
      }
      // copy the component into the preview graph
      new GraphCopier().Copy(draggedGraph, previewGraph);
      center = GetCenter(previewGraph);
    }

    /// <summary>
    /// Updates the <see paramref="previewGraph">preview graph</see> so the dragged component is
    /// displayed at the specified <paramref name="dragLocation"/>.
    /// </summary>
    /// <param name="previewGraph">The preview graph to update.</param>
    /// <param name="dragLocation">The current drag location.</param>
    protected override void UpdatePreview(IGraph previewGraph, PointD dragLocation) {
      // move the preview graph to the drag location
      var delta = dragLocation - center;
      Move(previewGraph, delta);
      center = dragLocation;

      // trigger a repaint
      var canvas = InputModeContext.CanvasControl;
      if (canvas != null) {
        canvas.Invalidate();
      }
    }

    /// <summary>
    /// Returns the center of the <paramref name="graph"/>.
    /// </summary>
    private PointD GetCenter(IGraph graph) {
      var bounds = RectD.Empty;
      foreach (var node in graph.Nodes) {
        bounds += node.Layout.ToRectD();
      }
      return bounds.Center;
    }

    /// <summary>
    /// Moves the <paramref name="graph"/> by <paramref name="delta"/>.
    /// </summary>
    private void Move(IGraph graph, PointD delta) {
      foreach (var node in graph.Nodes) {
        graph.SetNodeLayout(node, node.Layout.ToRectD().GetTranslated(delta));
      }
    }
  }
}
