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
using System.ComponentModel;
using System.Reflection;
using System.Windows.Media;
using Demo.yFiles.Graph.Bpmn.Util;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.Bpmn.Styles {

  /// <summary>
  /// An <see cref="INodeStyle"/> implementation representing an Group Node according to the BPMN.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class GroupNodeStyle : INodeStyle {
    private readonly GroupNodeStyleRenderer renderer = new GroupNodeStyleRenderer();

    private InsetsD insets = new InsetsD(15);

    /// <summary>
    /// Gets or sets the insets for the node.
    /// </summary>
    /// <remarks>
    /// These insets are returned via an <see cref="INodeInsetsProvider"/> if such an instance is queried through the
    /// <see cref="INodeStyleRenderer.GetContext">context lookup</see>.
    /// </remarks>
    /// <seealso cref="INodeInsetsProvider"/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(InsetsD), "15")]
    public InsetsD Insets {
      get { return insets; }
      set { insets = value; }
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public object Clone() {
      return new GroupNodeStyle {
          Insets = Insets,
          Background = Background,
          Outline = Outline,
      };
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public INodeStyleRenderer Renderer {
      get { return renderer; }
    }

    /// <summary>
    /// Gets or sets the background color of the group.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "GroupDefaultBackground")]
    public Brush Background {
      get { return renderer.shapeNodeStyle.Brush; }
      set {
        if (renderer.shapeNodeStyle.Brush != value) {
          renderer.shapeNodeStyle.Brush = value;
        }
      }
    }

    /// <summary>
    /// Gets or sets the outline color of the group.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "GroupDefaultOutline")]
    public Brush Outline {
      get { return renderer.shapeNodeStyle.Pen.Brush; }
      set {
        if (renderer.shapeNodeStyle.Pen.Brush != value) {
          renderer.shapeNodeStyle.Pen = GetPen(value);
        }
      }
    }

    private static Pen GetPen(Brush outline) {
      return (Pen) new Pen { DashStyle = DashStyles.DashDot, DashCap = PenLineCap.Round, Brush = outline }.GetAsFrozen();
    }

    /// <summary>
    /// An <see cref="INodeStyleRenderer"/> implementation used by <see cref="GroupNodeStyle"/>.
    /// </summary>
    internal class GroupNodeStyleRenderer : INodeStyleRenderer, ILookup
    {
      internal readonly ShapeNodeStyle shapeNodeStyle =
          new ShapeNodeStyle(new ShapeNodeStyleRenderer { RoundRectArcRadius = BpmnConstants.GroupNodeCornerRadius }) {
              Shape = ShapeNodeShape.RoundRectangle,
              Brush = BpmnConstants.GroupDefaultBackground,
              Pen = GetPen(BpmnConstants.GroupDefaultOutline)
          };

      private INode lastNode;
      private GroupNodeStyle lastStyle;

      /// <inheritdoc/>
      public IVisualCreator GetVisualCreator(INode item, INodeStyle style) {
        return shapeNodeStyle.Renderer.GetVisualCreator(item, shapeNodeStyle);
      }

      /// <inheritdoc/>
      public IBoundsProvider GetBoundsProvider(INode item, INodeStyle style) {
        return shapeNodeStyle.Renderer.GetBoundsProvider(item, shapeNodeStyle);
      }

      /// <inheritdoc/>
      public IVisibilityTestable GetVisibilityTestable(INode item, INodeStyle style) {
        return shapeNodeStyle.Renderer.GetVisibilityTestable(item, shapeNodeStyle);
      }

      /// <inheritdoc/>
      public IHitTestable GetHitTestable(INode item, INodeStyle style) {
        var geometry = shapeNodeStyle.Renderer.GetShapeGeometry(item, shapeNodeStyle);
        var outline = geometry.GetOutline();
        return HitTestables.Create((context, location) => outline.PathContains(location, context.HitTestRadius));
      }

      /// <inheritdoc/>
      public IMarqueeTestable GetMarqueeTestable(INode item, INodeStyle style) {
        return shapeNodeStyle.Renderer.GetMarqueeTestable(item, shapeNodeStyle);
      }

      /// <inheritdoc/>
      public ILookup GetContext(INode item, INodeStyle style) {
        lastNode = item;
        lastStyle = style as GroupNodeStyle;
        return this;
      }

      /// <inheritdoc/>
      public IShapeGeometry GetShapeGeometry(INode node, INodeStyle style) {
        return shapeNodeStyle.Renderer.GetShapeGeometry(node, shapeNodeStyle);
      }

      /// <inheritdoc/>
      [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
      public object Lookup(Type type) {
        if (type == typeof(INodeInsetsProvider) && lastStyle != null) {
          return new GroupInsetsProvider(lastStyle);
        }
        var lookup = shapeNodeStyle.Renderer.GetContext(lastNode, shapeNodeStyle);
        return lookup != null ? lookup.Lookup(type) : null;
      }


      /// <summary>
      /// Uses the style insets extended by the size of the participant bands.
      /// </summary>
      private sealed class GroupInsetsProvider : INodeInsetsProvider {
        private readonly GroupNodeStyle style;

        internal GroupInsetsProvider(GroupNodeStyle style) {
          this.style = style;
        }

        public InsetsD GetInsets(INode node) {
          return style.Insets;
        }
      }
    }
  }
}
