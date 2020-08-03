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
using System.Reflection;
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.StyleDecorators {

  /// <summary>
  /// A simple label style that shows how to decorate an arbitrary existing label style and 
  /// delegate rendering, as well as all rendering helper methods, to the wrapped 
  /// style. Additionally, two lines are added to the wrapped visualization.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class LabelStyleDecorator : LabelStyleBase<VisualGroup>
  {

    private ILabelStyle wrapped;

    public ILabelStyle Wrapped {
      get { return wrapped; }
      set { wrapped = value; }
    }

    public LabelStyleDecorator(): this(new DefaultLabelStyle()) {}

    public LabelStyleDecorator(ILabelStyle wrappedStyle) {
      this.wrapped = wrappedStyle;
    }

    protected override VisualGroup CreateVisual(IRenderContext context, ILabel label) {
      var labelLayout = label.GetLayout();
      // create container
      var container = new VisualGroup();
      // create wrapped visual
      var wrappedVisual = wrapped.Renderer.GetVisualCreator(label, wrapped).CreateVisual(context);
      // create visual decorators
      VisualGroup innerContainer = new VisualGroup();
      var decorator1 = new Line {X1 = 0, Y1 = 0, X2 = labelLayout.Width, Y2 = 0, Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0x24, 0x9A, 0xE7)), StrokeThickness = 2.0};
      var decorator2 = new Line {X1 = 0, Y1 = labelLayout.Height, X2 = labelLayout.Width, Y2 = labelLayout.Height, Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0x24, 0x9A, 0xE7)), StrokeThickness = 2.0};
      innerContainer.Add(decorator1);
      innerContainer.Add(decorator2);
      // arrange inner container - this is a ready-to-use utility method to apply the correct tranformation
      ArrangeByLayout(context, innerContainer, labelLayout, false);
      // add visuals to container
      container.Add(wrappedVisual);
      container.Add(innerContainer);

      return container;
    }

    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, ILabel label) {
      var labelLayout = label.GetLayout();
      var container = oldVisual;
      // update wrapped visual
      var wrappedVisual = wrapped.Renderer.GetVisualCreator(label, wrapped).UpdateVisual(context, container.Children[0]);
      // set new child only if wrapped visual instance has been switched
      if (container.Children[0] != wrappedVisual) {
        container.Children[0] = wrappedVisual;
      }
      VisualGroup innerContainer = (VisualGroup) container.Children[1];
      // update visual decorators
      Line decorator1 = (Line) innerContainer.Children[0];
      Line decorator2 = (Line) innerContainer.Children[1];
      decorator1.X2 = labelLayout.Width;
      decorator2.Y1 = labelLayout.Height;
      decorator2.X2 = labelLayout.Width;
      decorator2.Y2 = labelLayout.Height;
      // arrange inner container
      ArrangeByLayout(context, innerContainer, labelLayout, false);

      return container;
    }

    #region overriding methods

    protected override SizeD GetPreferredSize(ILabel label) {
      // delegate preferred size calculation to wrapped renderer
      return wrapped.Renderer.GetPreferredSize(label, wrapped);
    }

    protected override RectD GetBounds(ICanvasContext context, ILabel label) {
      return wrapped.Renderer.GetBoundsProvider(label, wrapped).GetBounds(context);
    }

    protected override bool IsVisible(ICanvasContext context, RectD rectangle, ILabel label) {
      return wrapped.Renderer.GetVisibilityTestable(label, wrapped).IsVisible(context, rectangle);
    }

    protected override bool IsHit(IInputModeContext context, PointD location, ILabel label) {
      return wrapped.Renderer.GetHitTestable(label, wrapped).IsHit(context, location);
    }

    protected override bool IsInBox(IInputModeContext context, RectD rectangle, ILabel label) {
      return wrapped.Renderer.GetMarqueeTestable(label, wrapped).IsInBox(context, rectangle);
    }

    protected override object Lookup(ILabel label, Type type) {
      return wrapped.Renderer.GetContext(label, wrapped).Lookup(type);
    }

    #endregion

  }
}
