/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Utils;

namespace Demo.yFiles.Graph.Bpmn.Styles
{
  /// <summary>
  ///   An <see cref="INodeStyle"/> implementation representing a Group Node according to the BPMN.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public sealed class GroupNodeStyle : INodeStyle
  {
    private readonly GroupNodeStyleRenderer renderer = new();

    /// <summary>
    ///   Gets or sets the padding for the node.
    /// </summary>
    /// <remarks>
    ///   This padding is returned via an <see cref="IGroupPaddingProvider"/> if such an instance is queried through the
    ///   <see cref="INodeStyleRenderer.GetContext">context lookup</see>.
    /// </remarks>
    /// <seealso cref="IGroupPaddingProvider"/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(InsetsD), "15")]
    public InsetsD Padding { get; set; } = new(15);

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public object Clone() {
      return new GroupNodeStyle { Padding = Padding, Background = Background, Outline = Outline };
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public INodeStyleRenderer Renderer => renderer;

    /// <summary>
    ///   Gets or sets the background color of the group.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "GroupDefaultBackground")]
    public Brush Background {
      get => renderer.rectangleNodeStyle.Brush;
      set {
        if (renderer.rectangleNodeStyle.Brush != value) {
          renderer.rectangleNodeStyle.Brush = value;
        }
      }
    }

    /// <summary>
    ///   Gets or sets the outline color of the group.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "GroupDefaultOutline")]
    public Brush Outline {
      get => renderer.rectangleNodeStyle.Pen.Brush;
      set {
        if (renderer.rectangleNodeStyle.Pen.Brush != value) {
          renderer.rectangleNodeStyle.Pen = GetPen(value);
        }
      }
    }

    private static Pen GetPen(Brush outline) {
      return (Pen) new Pen { DashStyle = DashStyles.DashDot, DashCap = PenLineCap.Round, Brush = outline }
          .GetAsFrozen();
    }

    /// <summary>
    ///   An <see cref="INodeStyleRenderer"/> implementation used by <see cref="GroupNodeStyle"/>.
    /// </summary>
    private sealed class GroupNodeStyleRenderer : INodeStyleRenderer, ILookup
    {
      internal readonly RectangleNodeStyle rectangleNodeStyle = new() {
          CornerSize = BpmnConstants.GroupNodeCornerRadius,
          Brush = BpmnConstants.GroupDefaultBackground,
          Pen = GetPen(BpmnConstants.GroupDefaultOutline)
      };

      private INode lastNode;
      private GroupNodeStyle lastStyle;

      /// <inheritdoc/>
      public IVisualCreator GetVisualCreator(INode item, INodeStyle style) {
        return rectangleNodeStyle.Renderer.GetVisualCreator(item, rectangleNodeStyle);
      }

      /// <inheritdoc/>
      public IBoundsProvider GetBoundsProvider(INode item, INodeStyle style) {
        return rectangleNodeStyle.Renderer.GetBoundsProvider(item, rectangleNodeStyle);
      }

      /// <inheritdoc/>
      public IVisibilityTestable GetVisibilityTestable(INode item, INodeStyle style) {
        return rectangleNodeStyle.Renderer.GetVisibilityTestable(item, rectangleNodeStyle);
      }

      /// <inheritdoc/>
      public IHitTestable GetHitTestable(INode item, INodeStyle style) {
        var geometry = rectangleNodeStyle.Renderer.GetShapeGeometry(item, rectangleNodeStyle);
        var outline = geometry.GetOutline();
        return HitTestables.Create((context, location) => outline.PathContains(location, context.HitTestRadius));
      }

      /// <inheritdoc/>
      public IMarqueeTestable GetMarqueeTestable(INode item, INodeStyle style) {
        return rectangleNodeStyle.Renderer.GetMarqueeTestable(item, rectangleNodeStyle);
      }

      public ILassoTestable GetLassoTestable(INode node, INodeStyle style) {
        return rectangleNodeStyle.Renderer.GetLassoTestable(node, rectangleNodeStyle);
      }

      /// <inheritdoc/>
      public ILookup GetContext(INode item, INodeStyle style) {
        lastNode = item;
        lastStyle = style as GroupNodeStyle;
        return this;
      }

      /// <inheritdoc/>
      public IShapeGeometry GetShapeGeometry(INode node, INodeStyle style) {
        return rectangleNodeStyle.Renderer.GetShapeGeometry(node, rectangleNodeStyle);
      }

      /// <inheritdoc/>
      [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
      public object Lookup(Type type) {
        if (type == typeof(IGroupPaddingProvider) && lastStyle != null) {
          return new GroupPaddingProvider(lastStyle);
        }
        var lookup = rectangleNodeStyle.Renderer.GetContext(lastNode, rectangleNodeStyle);
        return lookup != null ? lookup.Lookup(type) : null;
      }


      /// <summary>
      ///   Uses the style padding extended by the size of the participant bands.
      /// </summary>
      private sealed class GroupPaddingProvider : IGroupPaddingProvider
      {
        private readonly GroupNodeStyle style;

        internal GroupPaddingProvider(GroupNodeStyle style) {
          this.style = style;
        }

        public InsetsD GetPadding() {
          return style.Padding;
        }
      }
    }
  }
}
