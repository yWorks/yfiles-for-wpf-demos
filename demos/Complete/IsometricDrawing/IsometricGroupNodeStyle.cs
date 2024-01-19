/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Complete.IsometricDrawing
{
  /// <summary>
  /// A node style that visualizes group nodes in an isometric fashion.
  /// </summary>
  public class IsometricGroupNodeStyle : NodeStyleBase<VisualGroup>
  {
    private static readonly SolidColorBrush HeaderBrush = new SolidColorBrush(Color.FromArgb(255, 153, 204, 255)); 
    
    public static readonly Pen BorderPen = new Pen(new SolidColorBrush(Color.FromArgb(255, 153, 204, 255)), 1);

    /// <summary>
    /// The insets between the group node bounds and its children.
    /// </summary>
    private const double Insets = 20;
    
    /// <summary>
    /// The height of a group node header.
    /// </summary>
    public const double HeaderHeight = 18;

    private readonly IsometricNodeStyle wrapped = new IsometricNodeStyle();

    protected override VisualGroup CreateVisual(IRenderContext context, INode node) {
      var container = new VisualGroup();
      // delegate the main rendering to the wrapped IsometricNodeStyle
      container.Add(this.wrapped.Renderer.GetVisualCreator(node, wrapped).CreateVisual(context));
      // add a header
      container.Add(RenderHeader(node, new Rectangle { Fill = HeaderBrush }));
      return container;
    }

    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup container, INode node) {
      if (container.Children.Count == 2) {
        var wrappedVisual = container.Children[0];
        var headerRect = container.Children[1] as Rectangle;
        if (headerRect != null) {
          this.wrapped.Renderer.GetVisualCreator(node, wrapped).UpdateVisual(context, wrappedVisual);
          RenderHeader(node, headerRect);
          return container;
        }
      }
      return this.CreateVisual(context, node);
    }

    /// <summary>
    /// Updates a rectangle that fills the header in which a label can be placed. 
    /// </summary>
    /// <param name="node">The group node.</param>
    /// <param name="header">The visual to update.</param>
    /// <returns>The updated header rectangle.</returns>
    private static Rectangle RenderHeader(INode node, Rectangle header) {
      // the lower corner is the anchor of the label.
      var anchorX = node.Layout.X;
      var anchorY = node.Layout.GetMaxY();

      // Calculate the box of the label. It uses the whole width of the node.
      var width = node.Layout.Width;
      var headerHeight = HeaderHeight;
      var firstLabel = node.Labels.FirstOrDefault();
      if (firstLabel != null) {
        headerHeight = Math.Max(headerHeight, firstLabel.GetLayout().Height);
      }

      var oldCache = header.GetRenderDataCache<HeaderRenderData>();
      var newCache = new HeaderRenderData {
          AnchorX = anchorX, AnchorY = anchorY, Width = width, HeaderHeight = headerHeight
      };
      
      if (!newCache.Equals(oldCache)) {
        header.SetRenderDataCache(newCache);
        header.SetCanvasArrangeRect(new Rect(anchorX, anchorY - headerHeight, width, headerHeight));
      }
      return header;
    }

    /// <summary>
    /// The render data class for the group node header.
    /// </summary>
    private sealed class HeaderRenderData
    {
      public double AnchorX;
      public double AnchorY;

      public double HeaderHeight;
      public double Width;

      private bool Equals(HeaderRenderData other) {
        return other.AnchorX == AnchorX && other.AnchorY == AnchorY && other.Width == Width && other.HeaderHeight == HeaderHeight;
      }

      public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) {
          return false;
        }
        if (obj.GetType() != typeof(HeaderRenderData)) {
          return false;
        }
        return Equals((HeaderRenderData)obj);
      }
    }

    #region Delegates to the wrapped IsometricNodeStyle

    protected override RectD GetBounds(ICanvasContext context, INode node) {
      return wrapped.Renderer.GetBoundsProvider(node, wrapped).GetBounds(context);
    }

    protected override bool IsVisible(ICanvasContext context, RectD rectangle, INode node) {
      return wrapped.Renderer.GetVisibilityTestable(node, wrapped).IsVisible(context, rectangle);
    }

    protected override bool IsHit(IInputModeContext context, PointD location, INode node) {
      return wrapped.Renderer.GetHitTestable(node, wrapped).IsHit(context, location);
    }

    protected override PointD? GetIntersection(INode node, PointD inner, PointD outer) {
      return wrapped.Renderer.GetShapeGeometry(node, wrapped).GetIntersection(inner, outer);
    }

    protected override bool IsInside(INode node, PointD location) {
      return wrapped.Renderer.GetShapeGeometry(node, wrapped).IsInside(location);
    }

    protected override GeneralPath GetOutline(INode node) {
      return wrapped.Renderer.GetShapeGeometry(node, wrapped).GetOutline();
    }
    
    #endregion

    protected override object Lookup(INode node, Type type) {
      if (type == typeof(INodeInsetsProvider)) {
        // use a group node insets provider that considers the header and the insets
        return GroupNodeInsetsProvider.Instance;
      }
      return base.Lookup(node, type);
    }

    /// <summary>
    /// A group node insets provider that considers the header and the insets.
    /// </summary>
    sealed class GroupNodeInsetsProvider : INodeInsetsProvider
    {
      internal static readonly GroupNodeInsetsProvider Instance = new GroupNodeInsetsProvider();
      
      public InsetsD GetInsets(INode node) {
        var headerHeight = HeaderHeight;
        if (node.Labels.Count > 0) {
          headerHeight = Math.Max(headerHeight, node.Labels[0].GetLayout().Height);
        }
        return new InsetsD(Insets, Insets, Insets, Insets + headerHeight);
      }
    }
  }
}
