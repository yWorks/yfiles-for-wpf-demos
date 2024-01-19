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
using System.Reflection;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.StyleDecorators {

  /// <summary>
  /// An edge style that shows how to decorator an existing edge style and delegate rendering, 
  /// as well as all rendering helper methods, to the wrapped style as well as how to use 
  /// a port style to render the edge's bends.
  /// </summary>
  /// <remarks>
  /// This implementation wraps <see cref="PolylineEdgeStyle"/>.
  /// The <see cref="PolylineEdgeStyle.Pen"/> of the wrapped style is modified based on the 
  /// <see cref="TrafficLoad"/> value stored in the edge's tag.
  /// In order to render the edge's bend, and arbitrary <see cref="IPortStyle">port style</see>, 
  /// that can be set in the constructor, is used.
  /// </remarks>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public class EdgeStyleDecorator : EdgeStyleBase<VisualGroup>
  {
    private readonly PolylineEdgeStyle wrapped = new PolylineEdgeStyle {SmoothingLength = 5.0};

    private IPortStyle bendStyle;

    public EdgeStyleDecorator() {}

    public EdgeStyleDecorator(IPortStyle bendStyle) {
      this.bendStyle = bendStyle;
    }

    /// <summary>
    /// Gets the <see cref="IPortStyle">style</see> that is used for visualizing the edge's bends
    /// </summary>
    public IPortStyle BendStyle {
      get { return bendStyle; }
      set { bendStyle = value; }
    }

    protected override VisualGroup CreateVisual(IRenderContext context, IEdge edge) {
      // create container
      var container = new VisualGroup();
      // set pen
      wrapped.Pen = GetPen(edge);
      // delegate rendering
      var wrappedVisual =  wrapped.Renderer.GetVisualCreator(edge, wrapped).CreateVisual(context);
      container.Add(wrappedVisual);

      if (bendStyle != null) {
        var bendContainer = new VisualGroup();
        container.Add(bendContainer);
        RenderBends(context, edge, bendContainer);
      }
      return container;
    }

    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, IEdge edge) {
      // set pen
      wrapped.Pen = GetPen(edge);
      var wrappedVisual = oldVisual.Children[0];
      // delegate update
      wrappedVisual = wrapped.Renderer.GetVisualCreator(edge, wrapped).UpdateVisual(context, wrappedVisual);
      if (oldVisual.Children[0] != wrappedVisual) {
        oldVisual.Children[0] = wrappedVisual;
      }

      if (bendStyle != null) {
        var bendContainer = (VisualGroup) oldVisual.Children[1];
        RenderBends(context, edge, bendContainer);
      }
      return oldVisual;
    }

    #region bend rendering

    /// <summary>
    /// Renders the edge's bends, using <see cref="BendStyle" /> and dummy ports.
    /// </summary>
    private void RenderBends(IRenderContext context, IEdge edge, VisualGroup container) {
      var bends = edge.Bends;
      // remove surplus visuals
      while (container.Children.Count > bends.Count) {
        // remove last child
        container.Remove(container.Children[container.Children.Count-1]);
      }
      // update existing bend visuals
      for(int i=0; i<container.Children.Count; i++) {
        var bend = bends[i];
        // create a dummy port at the bend's location to render
        SimplePort dummyPort = new SimplePort(null, PointPortLocationModel.Instance.CreateParameter(bend.Location.ToPointD()))
        {
          LookupImplementation = Lookups.Empty
        };
        // update the dummy port visual
        var visual = bendStyle.Renderer.GetVisualCreator(dummyPort, bendStyle).UpdateVisual(context, container.Children[i]);
        // switch instances if necessary
        if (container.Children[i] != visual) {
          container.Children[i] = visual;
        }
      }
      // add missing visuals
      for(int i=container.Children.Count; i<bends.Count; i++) {
        var bend = bends[i];
        // create a dummy port at the bend's location to render
        SimplePort dummyPort = new SimplePort(null, PointPortLocationModel.Instance.CreateParameter(bend.Location.ToPointD()))
        {
          LookupImplementation = Lookups.Empty
        };
        // render the dummy port visual
        var bendVisual = bendStyle.Renderer.GetVisualCreator(dummyPort, bendStyle).CreateVisual(context);
        container.Children.Add(bendVisual);
      }
    }

    #endregion

    #region overriding methods

    protected override RectD GetBounds(ICanvasContext context, IEdge edge) {
      return wrapped.Renderer.GetBoundsProvider(edge, wrapped).GetBounds(context);
    }

    protected override bool IsVisible(ICanvasContext context, RectD rectangle, IEdge edge) {
      return wrapped.Renderer.GetVisibilityTestable(edge, wrapped).IsVisible(context, rectangle);
    }

    protected override bool IsHit(IInputModeContext context, PointD location, IEdge edge) {
      return wrapped.Renderer.GetHitTestable(edge, wrapped).IsHit(context, location);
    }

    protected override bool IsInBox(IInputModeContext context, RectD rectangle, IEdge edge) {
      return wrapped.Renderer.GetMarqueeTestable(edge, wrapped).IsInBox(context, rectangle);
    }

    protected override object Lookup(IEdge edge, Type type) {
      return wrapped.Renderer.GetContext(edge, wrapped).Lookup(type);
    }

    #endregion

    /// <summary>
    /// Returns a pen based on the priority stored in the edge's tag
    /// </summary>
    /// <param name="edge"></param>
    /// <returns></returns>
    private static Pen GetPen(IEdge edge) {
      if (edge.Tag is TrafficLoad) {
        switch((TrafficLoad)edge.Tag) {
          case TrafficLoad.VeryHigh:
            return new Pen(Brushes.Red, 3.0);
          case TrafficLoad.High:
            return new Pen(Brushes.Orange, 2.0);
          case TrafficLoad.Normal:
            return new Pen(Brushes.Black, 1.0);
          case TrafficLoad.Low:
            return new Pen(Brushes.LightGray, 1.0);
          default:
            return new Pen(Brushes.Black, 1.0);
        }
      }
      return new Pen(Brushes.Black, 1.0);
    }
  }

  public class PointPortLocationModel : IPortLocationModel {

    public static PointPortLocationModel Instance = new PointPortLocationModel();

    public object Lookup(Type type) {
      return null;
    }

    public PointD GetLocation(IPort port, IPortLocationModelParameter locationParameter) {
      var modelParameter = locationParameter as PointPortLocationModelParameter;
      if (modelParameter == null) {
        throw new ArgumentException("parameter is not supported", "locationParameter");
      }
      return modelParameter.Location;
    }

    public IPortLocationModelParameter CreateParameter(PointD location) {
      return new PointPortLocationModelParameter { Location = location, Model = this};
    }

    public IPortLocationModelParameter CreateParameter(IPortOwner owner, PointD location) {
      return new PointPortLocationModelParameter { Location = location, Model = this};
    }

    public ILookup GetContext(IPort port, IPortLocationModelParameter locationParameter) {
      return this;
    }

    private class PointPortLocationModelParameter : IPortLocationModelParameter {

      public PointD Location { get; set; }

      public object Clone() {
        return MemberwiseClone();
      }

      public IPortLocationModel Model { get; set; }
      
      public bool Supports(IPortOwner owner) {
        return true;
      }
    }
  }
}
