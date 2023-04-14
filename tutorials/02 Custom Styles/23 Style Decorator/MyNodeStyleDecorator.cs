/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Annotations;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.PortLocationModels;
using yWorks.Utils;

namespace Tutorial.CustomStyles
{

  ////////////////////////////////////////////////////////////////
  /////////////// This class is new in this sample ///////////////
  ////////////////////////////////////////////////////////////////

  /// <summary>
  /// A simple node style wrapper that takes a given node style and adds label edge rendering 
  /// as a visual decorator on top of the wrapped visualization.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This node style wrapper implementation adds the label edge rendering that was formerly part
  /// of <see cref="MySimpleNodeStyle"/> to the wrapped style. For this purpose of this tutorial step, 
  /// label edge rendering was removed from <see cref="MySimpleNodeStyle"/>.
  /// </para>
  /// <para>
  /// Similar to this implementation, wrapping styles for other graph items can be created by implementing
  /// <see cref="EdgeStyleBase{TVisual}"/>, <see cref="LabelStyleBase{TVisual}"/> and 
  /// <see cref="PortStyleBase{TVisual}"/>.
  /// </para>
  /// </remarks>
  class MyNodeStyleDecorator : NodeStyleBase<VisualGroup> {

    // the wrapped style
    private readonly INodeStyle wrapped;

    /// <summary>
    /// Creates a new instance of this style using the given wrapped style.
    /// </summary>
    /// <param name="wrappedStyle">The style that is decorated by this instance.</param>
    public MyNodeStyleDecorator([NotNull]INodeStyle wrappedStyle) {
      this.wrapped = wrappedStyle;
    }

    #region Rendering

    protected override VisualGroup CreateVisual(IRenderContext context, INode node) {
      // create the outer container
      VisualGroup container = new VisualGroup();
      // create the cache for updating the visual
      var renderDataCache = CreateRenderDataCache(node);
      // create the wrapped style's visual
      var wrappedVisual = wrapped.Renderer.GetVisualCreator(node, wrapped).CreateVisual(context);

      // create label edges as decorators for wrapped style
      var labelEdgesContainer = new VisualGroup();
      RenderLabelEdges(context, node, labelEdgesContainer, renderDataCache);
      labelEdgesContainer.SetCanvasArrangeRect(node.Layout.ToRectD());

      // add both visuals to outer container
      container.Add(wrappedVisual);
      container.Add(labelEdgesContainer);

      // store the cache with the container
      container.SetRenderDataCache(renderDataCache);
      return container;
    }

    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, INode node) {
      // get the container's children
      var wrappedVisual = oldVisual.Children[0];
      var labelEdgesContainer = (VisualGroup) oldVisual.Children[1];

      // update the wrapped visual
      var updateVisual = wrapped.Renderer.GetVisualCreator(node, wrapped).UpdateVisual(context, wrappedVisual);
      if(oldVisual.Children[0] != updateVisual) {
        oldVisual.Children[0] = updateVisual;
      }

      // check if we need to re-render the label edges
      var oldCache = oldVisual.GetRenderDataCache<RenderDataCache>();
      var newCache = CreateRenderDataCache(node);
      if (!newCache.Equals(oldCache)) {
        labelEdgesContainer.Children.Clear();
        RenderLabelEdges(context, node, labelEdgesContainer, newCache);
        oldVisual.SetRenderDataCache(newCache);
      }
      labelEdgesContainer.SetCanvasArrangeRect(node.Layout.ToRectD());
      return oldVisual;
    }

    /// <summary>
    /// Draws the edge-like connectors from a node to its labels
    /// </summary>
    private void RenderLabelEdges(IRenderContext context, INode node, VisualGroup container, RenderDataCache cache) {
      if (node.Labels.Count > 0) {
        // Create a SimpleEdge which will be used as a dummy for the rendering
        SimpleEdge simpleEdge = new SimpleEdge(null, null);
        // Assign the style
        simpleEdge.Style = new MySimpleEdgeStyle { PathThickness = 2 };

        // Create a SimpleNode which provides the source port for the edge but won't be drawn itself
        SimpleNode sourceDummyNode = new SimpleNode{ Layout = new RectD(0, 0, node.Layout.Width, node.Layout.Height), Style = node.Style };


        // Set source port to the port of the node using a dummy node that is located at the origin.
        simpleEdge.SourcePort = new SimplePort(sourceDummyNode, FreeNodePortLocationModel.NodeCenterAnchored);

        // Create a SimpleNode which provides the target port for the edge but won't be drawn itself
        SimpleNode targetDummyNode = new SimpleNode();

        // Create port on targetDummynode for the label target
        targetDummyNode.Ports =
            new ListEnumerable<IPort>(new[] { new SimplePort(targetDummyNode, FreeNodePortLocationModel.NodeCenterAnchored) });
        simpleEdge.TargetPort = new SimplePort(targetDummyNode, FreeNodePortLocationModel.NodeCenterAnchored);

        // Render one edge for each label
        foreach (PointD labelLocation in cache.LabelLocations) {
          // move the dummy node to the location of the label
          targetDummyNode.Layout = new MutableRectangle(labelLocation, SizeD.Zero); ;

          // now create the visual using the style interface:
          IEdgeStyleRenderer renderer = simpleEdge.Style.Renderer;
          IVisualCreator creator = renderer.GetVisualCreator(simpleEdge, simpleEdge.Style);
          Visual element = creator.CreateVisual(context);
          if (element != null) {
            container.Add(element);
          }
        }
      }
    }

    /// <summary>
    /// Creates an object containing all necessary data to create a visual for the node
    /// </summary>
    private RenderDataCache CreateRenderDataCache(INode node) {
      List<PointD> labelLocations = new List<PointD>();
      // Remember center points of labels to draw label edges, relative the node's top left corner
      foreach (ILabel label in node.Labels) {
        PointD labelCenter = label.GetLayout().GetCenter();
        labelLocations.Add(labelCenter - node.Layout.GetTopLeft());
      }
      return new RenderDataCache(labelLocations);
    }

    #endregion

    #region Rendering Helper Methods

    protected override RectD GetBounds(ICanvasContext context, INode node) {
      // delegate this to the wrapped style
      return wrapped.Renderer.GetBoundsProvider(node, wrapped).GetBounds(context);
    }

    protected override bool IsVisible(ICanvasContext context, RectD rectangle, INode node) {
      // first check if the wrapped style is visible
      if (wrapped.Renderer.GetVisibilityTestable(node, wrapped).IsVisible(context, rectangle)) {
        return true;
      }
      // if not, check for labels connection lines 
      rectangle = rectangle.GetEnlarged(10);
      foreach (var label in node.Labels) {
        if (rectangle.IntersectsLine(node.Layout.GetCenter(), label.GetLayout().GetCenter())) {
          return true;
        }
      }
      return false;
    }

    protected override bool IsHit(IInputModeContext context, PointD location, INode node) {
      // delegate this to the wrapped style since we don't want the visual decorator to be hit testable
      return wrapped.Renderer.GetHitTestable(node, wrapped).IsHit(context, location);
    }

    protected override bool IsInBox(IInputModeContext context, RectD rectangle, INode node) {
      // delegate this to the wrapped style
      return wrapped.Renderer.GetMarqueeTestable(node, wrapped).IsInBox(context, rectangle);
    }

    protected override object Lookup(INode node, Type type) {
      // delegate this to the wrapped style
      return wrapped.Renderer.GetContext(node, wrapped).Lookup(type);
    }

    protected override PointD? GetIntersection(INode node, PointD inner, PointD outer) {
      // delegate this to the wrapped style
      return wrapped.Renderer.GetShapeGeometry(node, wrapped).GetIntersection(inner, outer);
    }

    protected override bool IsInside(INode node, PointD location) {
      // delegate this to the wrapped style
      return wrapped.Renderer.GetShapeGeometry(node, wrapped).IsInside(location);
    }

    protected override GeneralPath GetOutline(INode node) {
      // delegate this to the wrapped style
      return wrapped.Renderer.GetShapeGeometry(node, wrapped).GetOutline();
    }

    #endregion

    /// <summary>
    /// Saves the data which is necessary for the creation of a node
    /// </summary>
    private sealed class RenderDataCache
    {
      public List<PointD> LabelLocations { get; private set; }

      public RenderDataCache(List<PointD> labelLocations) {
        LabelLocations = labelLocations;
      }

      public bool Equals(RenderDataCache other) {
        return ListsAreEqual(LabelLocations, other.LabelLocations);
      }

      /// <summary>
      /// Helper method to decide if two lists are equals
      /// </summary>
      private static bool ListsAreEqual<T>(List<T> list1, List<T> list2) {
        if (list1.Count != list2.Count) {
          return false;
        }
        for (int i = 0; i < list1.Count; i++) {
          if (!Equals(list1[i], list2[i])) {
            return false;
          }
        }
        return true;
      }

      public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) {
          return false;
        }
        if (obj.GetType() != typeof(RenderDataCache)) {
          return false;
        }
        return Equals((RenderDataCache)obj);
      }
    }

  }
}
