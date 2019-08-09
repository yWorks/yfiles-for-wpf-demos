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
using System.ComponentModel;
using System.Reflection;
using System.Windows.Media.Imaging;
using yWorks.Controls.Input;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Utils;

namespace Demo.yFiles.Graph.StyleDecorators {
  /// <summary>
  /// A simple node style decorator that adds an image in the upper right corner
  /// of a given node style.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <see cref="ImageNodeStyle"/> is used to render the image decorator.
  /// </para>
  /// <para>
  /// This style overrides <see cref="IVisibilityTestable.IsVisible"/> with a custom implementation 
  /// that also checks the visibility of the image decorator in addition to calling the 
  /// implementation of the decorated style.
  /// Other checks like <see cref="IHitTestable.IsHit" /> and <see cref="IMarqueeTestable.IsInBox" />
  /// are simply delegated to the wrapped style in order to not make the node selectable 
  /// by clicking or marquee selecting the image decorator part of the visualization.
  /// If desired, this feature can be implemented like demonstrated in <see cref="NodeStyleDecorator.IsVisible"/>.
  /// </para>
  /// </remarks>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class NodeStyleDecorator : NodeStyleBase<VisualGroup>
  {
    // the wrapped style
    private INodeStyle wrapped;
    // the url of the decorator image
    private string imageUrl;
    // the size of the decorator image
    private SizeD decoratorSize = new SizeD(32, 32);
    // create style to render decorator
    private readonly ImageNodeStyle imageStyle = new ImageNodeStyle();

    /// <summary>
    /// Initializes a default instance of this style.
    /// </summary>
    public NodeStyleDecorator(): this(new ShapeNodeStyle(), null) {}

    /// <summary>
    /// Initializes an instance of this style.
    /// </summary>
    /// <param name="wrapped">the wrapped style</param>
    /// <param name="imageUrl">the url of the decorator image</param>
    public NodeStyleDecorator(INodeStyle wrapped, string imageUrl) {
      this.wrapped = wrapped;
      this.imageUrl = imageUrl;
    }

    /// <summary>
    /// Gets the wrapped node style.
    /// </summary>
    public INodeStyle Wrapped {
      get { return wrapped; }
      set { wrapped = value; }
    }

    /// <summary>
    /// Gets or sets the decorator image url.
    /// </summary>
    /// <remarks>
    /// Changes to this property will only apply after the next call of 
    /// <see cref="UpdateVisual"/>.</remarks>
    public string ImageUrl {
      get { return imageUrl; }
      set { imageUrl = value; }
    }

    /// <summary>
    /// Gets or sets the size of the decorator image.
    /// </summary>
    [DefaultValue(typeof(SizeD), "32,32")]
    public SizeD DecoratorSize {
      get { return decoratorSize; }
      set { decoratorSize = value; }
    }

    protected override VisualGroup CreateVisual(IRenderContext context, INode node) {
      var layout = node.Layout;
      VisualGroup container = new VisualGroup();
      // create wrapped visual
      var wrappedVisual = wrapped.Renderer.GetVisualCreator(node, wrapped).CreateVisual(context);
      // set ImageSource
      if (imageUrl != null) {
        imageStyle.Image = new BitmapImage(new Uri(imageUrl, UriKind.Relative));
      }
      // create dummy node to render
      SimpleNode dummyNode = new SimpleNode
      {
        Layout = new RectD(layout.X + layout.Width - decoratorSize.Width * 0.5, layout.Y - decoratorSize.Height * 0.5, decoratorSize.Width, decoratorSize.Height)
      };
      var image = imageStyle.Renderer.GetVisualCreator(dummyNode, imageStyle).CreateVisual(context);
      // add both to the decorator
      container.Add(wrappedVisual);
      container.Add(image);
      // save image url with the visual for later update
      container.SetRenderDataCache<string>(imageUrl);
      return container;
    }

    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, INode node) {
      var layout = node.Layout;
      if (oldVisual.Children.Count != 2) {
        // something's wrong - re-create visual
        return CreateVisual(context, node);
      }
      // get the child visuals from the container
      var wrappedVisual = oldVisual.Children[0];
      var image = oldVisual.Children.Count > 1?oldVisual.Children[1]:null;

      // update wrapped visual - delegate to renderer of wrapped style
      wrappedVisual = wrapped.Renderer.GetVisualCreator(node, wrapped).UpdateVisual(context, wrappedVisual);
      if (wrappedVisual != oldVisual.Children[0]) {
        oldVisual.Children[0] = wrappedVisual;
      }
      var oldImageUrl = oldVisual.GetRenderDataCache<string>();
      if (imageUrl != oldImageUrl) {
        // update image
        imageStyle.Image = new BitmapImage(new Uri(imageUrl, UriKind.Relative));
      }
      SimpleNode dummyNode = new SimpleNode
      {
        Layout = new RectD(layout.X + layout.Width - (decoratorSize.Width * 0.5), layout.Y - decoratorSize.Height * 0.5, decoratorSize.Width, decoratorSize.Height)
      };
      image = imageStyle.Renderer.GetVisualCreator(dummyNode, imageStyle).UpdateVisual(context, image);
      if (oldVisual.Children.Count > 1) {
        if(image == null) {
          oldVisual.Children.RemoveAt(1);
        }
        else if (oldVisual.Children[1] != image) {
          oldVisual.Children[1] = image;
        }
      }
      else if(image != null) {
        oldVisual.Add(image);
      }
      return oldVisual;
    }

    protected override bool IsVisible(ICanvasContext context, RectD rectangle, INode node) {
      var layout = node.Layout;
      // check if wrapped is visible
      var isWrappedVisible = wrapped.Renderer.GetVisibilityTestable(node, wrapped).IsVisible(context, rectangle);
      // check if the decorator is visible
      var isDecoratorVisible =
        rectangle.Intersects(new RectD(layout.X + layout.Width - (decoratorSize.Width*0.5),
                                  layout.Y + layout.Height - (decoratorSize.Height*0.5), decoratorSize.Width, decoratorSize.Height));
      return isWrappedVisible || isDecoratorVisible;
    }

    protected override bool IsHit(IInputModeContext context, PointD location, INode node) {
      // return only hit test of wrapped - we don't want the decorator to be hit testable
      return wrapped.Renderer.GetHitTestable(node, wrapped).IsHit(context, location);
    }

    protected override bool IsInBox(IInputModeContext context, RectD rectangle, INode node) {
      // return only box containment test of wrapped - we don't want the decorator to be marquee selectable
      return wrapped.Renderer.GetMarqueeTestable(node, wrapped).IsInBox(context, rectangle);
    }

    protected override PointD? GetIntersection(INode node, PointD inner, PointD outer) {
      // return only intersection with wrapped
      return wrapped.Renderer.GetShapeGeometry(node, wrapped).GetIntersection(inner, outer);
    }

    protected override bool IsInside(INode node, PointD location) {
      // return only inside test of wrapped
      return wrapped.Renderer.GetShapeGeometry(node, wrapped).IsInside(location);
    }
  }
}
