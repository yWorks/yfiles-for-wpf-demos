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
  /// An <see cref="IPortStyle"/> implementation representing an Event attached to an Activity boundary according to the BPMN.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class EventPortStyle : IPortStyle {
    private readonly IPortStyleRenderer renderer;

    #region Properties

    /// <summary>
    /// Gets or sets the event type for this style.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(EventType.Compensation)]
    public EventType Type {
      get { return EventNodeStyle.Type; }
      set { EventNodeStyle.Type = value; }
    }

    /// <summary>
    /// Gets or sets the event characteristic for this style.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(EventCharacteristic.BoundaryInterrupting)]
    public EventCharacteristic Characteristic {
      get { return EventNodeStyle.Characteristic; }
      set { EventNodeStyle.Characteristic = value; }
    }

    /// <summary>
    /// Gets or sets the size the port style is rendered with.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(SizeD), "20,20")]
    public SizeD RenderSize {
      get { return adapter.RenderSize; }
      set { adapter.RenderSize = value; }
    }

    /// <summary>
    /// Gets or sets the background color of the event.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "DefaultEventBackground")]
    public Brush Background {
      get { return EventNodeStyle.Background; }
      set { EventNodeStyle.Background = value; }
    }

    /// <summary>
    /// Gets or sets the outline color of the event.
    /// </summary>
    /// <remarks>
    /// If this is set to <see langword="null"/>, the outline color is automatic, based on the <see cref="Characteristic"/>.
    /// </remarks>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "DefaultEventOutline")]
    public Brush Outline {
      get { return EventNodeStyle.Outline; }
      set { EventNodeStyle.Outline = value; }
    }

    /// <summary>
    /// Gets or sets the primary color for icons and markers.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "DefaultIconColor")]
    public Brush IconColor {
      get { return EventNodeStyle.IconColor; }
      set { EventNodeStyle.IconColor = value; }
    }

    #endregion

    private readonly NodeStylePortStyleAdapter adapter;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public EventPortStyle() {
      adapter = new NodeStylePortStyleAdapter(new EventNodeStyle
      {
          Characteristic = EventCharacteristic.BoundaryInterrupting,
          Type = EventType.Compensation
      }) { RenderSize = BpmnConstants.EventPortSize };
      renderer = EventPortStyleRenderer.Instance;
    }

    internal EventNodeStyle EventNodeStyle {
      get {
        return (EventNodeStyle) adapter.NodeStyle;
      }
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public object Clone() {
      return MemberwiseClone();
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public IPortStyleRenderer Renderer {
      get { return renderer; }
    }


    /// <summary>
    /// Renderer used by <see cref="EventPortStyle"/>.
    /// </summary>
    private class EventPortStyleRenderer : IPortStyleRenderer, ILookup {
      public static readonly EventPortStyleRenderer Instance = new EventPortStyleRenderer();

      private ILookup fallbackLookup;

      /// <inheritdoc/>
      public IVisualCreator GetVisualCreator(IPort item, IPortStyle style) {
        var adapter = ((EventPortStyle)style).adapter;
        return adapter.Renderer.GetVisualCreator(item, adapter);
      }

      /// <inheritdoc/>
      public IBoundsProvider GetBoundsProvider(IPort item, IPortStyle style) {
        var adapter = ((EventPortStyle)style).adapter;
        return adapter.Renderer.GetBoundsProvider(item, adapter);
      }

      /// <inheritdoc/>
      public IVisibilityTestable GetVisibilityTestable(IPort item, IPortStyle style) {
        var adapter = ((EventPortStyle)style).adapter;
        return adapter.Renderer.GetVisibilityTestable(item, adapter);
      }

      /// <inheritdoc/>
      public IHitTestable GetHitTestable(IPort item, IPortStyle style) {
        var adapter = ((EventPortStyle)style).adapter;
        return adapter.Renderer.GetHitTestable(item, adapter);
      }

      /// <inheritdoc/>
      public IMarqueeTestable GetMarqueeTestable(IPort item, IPortStyle style) {
        var adapter = ((EventPortStyle)style).adapter;
        return adapter.Renderer.GetMarqueeTestable(item, adapter);
      }

      /// <inheritdoc/>
      public ILookup GetContext(IPort item, IPortStyle style) {
        var adapter = ((EventPortStyle)style).adapter;
        fallbackLookup = adapter.Renderer.GetContext(item, adapter);
        return this;
      }

      /// <inheritdoc/>
      public object Lookup(Type type) {
        if (type == typeof(IEdgePathCropper)) {
          return EventPortEdgePathCropper.CalculatorInstance;
        }
        return fallbackLookup.Lookup(type);
      }
    }

    /// <summary>
    /// IEdgePathCropper instance that crops the edge at the circular port bounds.
    /// </summary>
    private sealed class EventPortEdgePathCropper : DefaultEdgePathCropper {
      public static readonly EventPortEdgePathCropper CalculatorInstance = new EventPortEdgePathCropper();

      private EventPortEdgePathCropper() {
        CropAtPort = true;
      }

      protected override IShapeGeometry GetPortGeometry(IPort port) {
        return port.Style.Renderer.GetContext(port, port.Style).Lookup<IShapeGeometry>();
      }
    }
  }
}
