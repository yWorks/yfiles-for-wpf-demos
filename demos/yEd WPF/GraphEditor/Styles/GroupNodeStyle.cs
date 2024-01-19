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
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.LabelModels;

namespace Demo.yFiles.GraphEditor.Styles
{
  public enum ButtonLocation { TopLeft, TopRight, BottomLeft, BottomRight }

  [ContentProperty("Wrapped")]
  public class GroupNodeStyle : INodeStyle
  {
    private static readonly GroupNodeStyleRenderer renderer = new GroupNodeStyleRenderer();
    private readonly CollapsibleNodeStyleDecorator delegateStyle;
    private ButtonLocation buttonLocation;
    private double inset;

    public GroupNodeStyle() {
      delegateStyle = new CollapsibleNodeStyleDecorator {ButtonStyle = null};
      ButtonLocation = ButtonLocation.TopLeft;
      Inset = 5;
#pragma warning disable CS0618
      //We keep this for compatibility reasons with older versions/files
      Wrapped = new PanelNodeStyle();
#pragma warning restore CS0618
    }

    public object Clone() {
      return new GroupNodeStyle() {Wrapped = (INodeStyle) this.Wrapped.Clone(), ButtonLocation = ButtonLocation, Inset = this.Inset};
    }

    public INodeStyleRenderer Renderer {
      get {
        return renderer;
      }
    }

    [DisplayName("Inner Style")]
    public INodeStyle Wrapped {
      get { return delegateStyle.Wrapped; }
      set { delegateStyle.Wrapped = value; }
    }

    public ILabelModelParameter ButtonPlacement {
      get { switch (ButtonLocation) {
        case ButtonLocation.TopLeft:
          return InteriorLabelModel.NorthWest;
        case ButtonLocation.TopRight:
          return InteriorLabelModel.NorthEast;
        case ButtonLocation.BottomLeft:
          return InteriorLabelModel.SouthWest;
        case ButtonLocation.BottomRight:
          return InteriorLabelModel.SouthEast;
        default:
          throw new ArgumentOutOfRangeException();
      }
      }
    }

    [DisplayName("Toggle Button Location")]
    [DefaultValue(ButtonLocation.TopLeft)]
    public ButtonLocation ButtonLocation {
      get { return buttonLocation; }
      set {
        buttonLocation = value;
        if (delegateStyle != null) {
          delegateStyle.ButtonPlacement = ButtonPlacement;
        }
      }
    }

    public InsetsD Insets { get { return new InsetsD(Inset); } }

    [DefaultValue(5.0d)]
    public double Inset {
      get { return inset; }
      set {
        inset = value;
        if (delegateStyle != null) {
          delegateStyle.Insets = Insets;
        }
      }
    }

    public Style ButtonStyle {
      get { return null; }
    }

    private class GroupNodeStyleRenderer : INodeStyleRenderer {
      private static readonly CollapsibleNodeStyleDecoratorRenderer renderer = new CollapsibleNodeStyleDecoratorRenderer();


      public IVisualCreator GetVisualCreator(INode node, INodeStyle style) {
        return renderer.GetVisualCreator(node, ((GroupNodeStyle) style).delegateStyle);
      }

      public IBoundsProvider GetBoundsProvider(INode node, INodeStyle style) {
        return renderer.GetBoundsProvider(node, ((GroupNodeStyle)style).delegateStyle);
      }

      public IHitTestable GetHitTestable(INode node, INodeStyle style) {
        return renderer.GetHitTestable(node, ((GroupNodeStyle)style).delegateStyle);
      }

      public IMarqueeTestable GetMarqueeTestable(INode node, INodeStyle style) {
        return renderer.GetMarqueeTestable(node, ((GroupNodeStyle)style).delegateStyle);
      }

      public IVisibilityTestable GetVisibilityTestable(INode node, INodeStyle style) {
        return renderer.GetVisibilityTestable(node, ((GroupNodeStyle)style).delegateStyle);
      }

      public ILookup GetContext(INode node, INodeStyle style) {
        return renderer.GetContext(node, ((GroupNodeStyle)style).delegateStyle);
      }

      public IShapeGeometry GetShapeGeometry(INode node, INodeStyle style) {
        return renderer.GetShapeGeometry(node, ((GroupNodeStyle)style).delegateStyle);
      }
    }
  }
}
