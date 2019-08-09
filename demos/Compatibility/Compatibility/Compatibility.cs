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
using System.Collections.Generic;
using System.Linq;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Graph
{
    /// <summary>
    /// Provides extension methods which replace the most prominent methods which were removed, renamed, 
    /// or whose signature has been changed between yFiles WPF 2.5 and yFiles WPF 3.0.
    /// </summary>
    /// <remarks>
    /// We recommend to inline these methods once your projects have been migrated to yFiles WPF 3.0.
    /// </remarks>
    public static class GraphCompat
    {
      public static IBend AppendBend(this IGraph graph, IEdge edge, PointD location) {
        return graph.AddBend(edge, location);
      }

      public static void SetLabelModelParameter(this IGraph graph, ILabel label, ILabelModelParameter parameter) {
        graph.SetLabelLayoutParameter(label, parameter);
      }

      public static IBend AddBend(this IGraph graph, IEdge edge, int index, PointD location) {
        return graph.AddBend(edge, location, index);
      }

      public static INode CreateNode(this IGraph graph, PointD location, object tag) {
        return graph.CreateNode(location, null, null);
      }

      public static void SetSize(this IGraph graph, ILabel label, SizeD size) {
        graph.SetLabelPreferredSize(label, size);
      }

      public static T Get<T>(this ILookup lookup) where T : class {
        return lookup.Lookup<T>();
      }

      public static T SafeGet<T>(this ILookup lookup) where T : class {
        return lookup.SafeLookup<T>();
      }

      public static IGraph GetGroupedGraph(this IGraph graph) {
        return graph;
      }

      public static void SetValue<K, V>(this IMapper<K, V> mapper, K key, V value) {
        mapper[key] = value;
      }

      public static V GetValue<K, V>(this IMapper<K, V> mapper, K key) {
        return mapper[key];
      }
      
      public static V RemoveValue<K, V>(this IMapper<K, V> mapper, K key) {
        var dm = mapper as DictionaryMapper<K, V>;
        if (dm != null) {
          var value = dm[key];
          dm.RemoveValue(key);
          return value;
        }
        var wm = mapper as WeakDictionaryMapper<K, V>;
        if (wm != null) {
          var value = wm[key];
          wm.RemoveValue(key);
          return value;
        }
        throw new NotSupportedException("Unknown mapper type");
      }

      public static void SetPorts(this IGraph graph, IEdge edge, IPort sourcePort, IPort targetPort) {
        graph.SetEdgePorts(edge, sourcePort, targetPort);
      }

      public static IMapper<K, V> AddMapper<K, V>(this IMapperRegistry registry, object key) {
        return registry.CreateMapper<K, V>(key);
      }
      
      public static IMapper<K, V> AddDictionaryMapper<K, V>(this IMapperRegistry registry, object key) {
        return registry.CreateMapper<K, V>(key);
      }

      public static IMapper<K, V> AddMapper<K, V>(this IMapperRegistry registry, object key, MapperDelegate<K, V> mapperDelegate) {
        return registry.CreateDelegateMapper(key, mapperDelegate);
      }

      public static void SetCenter(this IGraph graph, INode node, PointD center) {
        graph.SetNodeCenter(node, center);
      }

      public static void SetBounds(this IGraph graph, INode node, RectD rect) {
        graph.SetNodeLayout(node, rect);
      }

      public static T GetMaster<T>(this IFoldingView fv, T item) where T: class, IModelItem {
        return fv.GetMasterItem(item);
      }

      public static IFoldingView CreateManagedView(this FoldingManager manager) {
        return manager.CreateFoldingView();
      }

      public static ILabel AddLabel(this IGraph graph, ILabelOwner owner, ILabelModelParameter parameter, ILabelStyle style, string text, object tag = null) {
        return graph.AddLabel(owner, text, parameter, style, null, tag);
      }
      
      public static ILabel AddLabel(this IGraph graph, ILabelOwner owner, ILabelModelParameter parameter, ILabelStyle style, string text, SizeD preferredSize, object tag = null) {
        return graph.AddLabel(owner, text, parameter, style, preferredSize, tag);
      }

      public static ILabel AddLabel(this IGraph graph, ILabelOwner owner, ILabelModelParameter parameter, string text, object tag = null) {
        return graph.AddLabel(owner, text, parameter, null, null, tag);
      }

      public static RectD GetDefaultNodeBounds(this IGraph graph) {
        return new RectD(PointD.Origin, graph.NodeDefaults.Size);
      }

      public static IPortStyle CreatePortStyle(this IGraph graph, IPortOwner owner) {
        return owner is INode ? graph.NodeDefaults.Ports.GetStyleInstance(owner) : graph.EdgeDefaults.Ports.GetStyleInstance(owner);
      }

      public static ILabelStyle CreateLabelStyle(this IGraph graph, ILabelOwner owner) {
        if (owner is INode) {
          return graph.NodeDefaults.Labels.GetStyleInstance(owner);
        }
        if (owner is IEdge) {
          return graph.EdgeDefaults.Labels.GetStyleInstance(owner);
        }
        var port = owner as IPort;
        if (port != null) {
          if (port.Owner is INode) {
            return graph.NodeDefaults.Ports.Labels.GetStyleInstance(owner);
          }
          if (port.Owner is IEdge) {
            return graph.EdgeDefaults.Ports.Labels.GetStyleInstance(owner);
          }
        }
        return null;
      }

      public static IEdgeStyle CreateEdgeStyle(this IGraph graph) {
        return graph.EdgeDefaults.GetStyleInstance();
      }

      public static INodeStyle CreateNodeStyle(this IGraph graph) {
        return graph.NodeDefaults.GetStyleInstance();
      }

      public static IColumn CreateColumn(this ITable table, double size, IStripeStyle style) {
        return table.CreateColumn(table.RootColumn, size, null, null, style);
      }
      public static IRow CreateRow(this ITable table, double size, IStripeStyle style) {
        return table.CreateRow(table.RootRow, size, null, null, style);
      }

      public static void SetInsets(this ITable table, IStripe stripe, InsetsD insets) {
        table.SetStripeInsets(stripe, insets);
      }

      public static IStripe GetParent(this IStripe stripe) {
        return stripe.GetParentStripe();
      }

      public static IEnumerable<IStripe> GetChildren(this IStripe stripe) {
        return stripe.GetChildStripes();
      }

      public static void SetRelativeLocation(this IGraph graph, IPort port, PointD location) {
        graph.SetRelativePortLocation(port, location);
      }

      public static void AdjustGroupNodeBounds(this IGraph graph, INode node) {
        graph.AdjustGroupNodeLayout(node);
      }

      public static bool IsLeaf(this IGraph graph, INode node) {
        return !graph.IsGroupNode(node);
      }

      public static void SetLeaf(this IGraph graph, INode node, bool leaf) {
        graph.SetIsGroupNode(node, !leaf);
      }
    }
}

namespace yWorks.Geometry
{
  /// <summary>
  /// Provides extension methods which replace the most prominent methods which were removed, renamed, 
  /// or whose signature has been changed between yFiles WPF 2.5 and yFiles WPF 3.0.
  /// </summary>
  /// <remarks>
  /// We recommend to inline these methods once your projects have been migrated to yFiles WPF 3.0.
  /// </remarks>
  public static class GeometryCompat
  {
    public static SizeD ToSize(this IRectangle rect) {
      return rect.ToSizeD();
    }

    public static SizeD ToSize(this IOrientedRectangle rect) {
      return rect.ToSizeD();
    }

    public static IRectangle ToRectangle(this RectD rectd) {
      return rectd;
    }

    public static PointD GetRounded(this PointD point) {
      return new PointD(Math.Round(point.X), Math.Round(point.Y));
    }

    public static void Set(this OrientedRectangle mutable, IOrientedRectangle r) {
      mutable.AnchorX = r.AnchorX;
      mutable.AnchorY = r.AnchorY;
      mutable.Width = r.Width;
      mutable.Height = r.Height;
      mutable.UpX = r.UpX;
      mutable.UpY = r.UpY;
    }
    
    public static void Set(this MutableRectangle mutable, IRectangle r) {
      mutable.X = r.X;
      mutable.Y = r.Y;
      mutable.Width = r.Width;
      mutable.Height = r.Height;
    }

    public static PointD ToPoint(this IPoint p) {
      return p.ToPointD();
    }

    public static void SetSize(this OrientedRectangle rect, SizeD size) {
      rect.Width = size.Width;
      rect.Height = size.Height;
    }
    
  }
}

namespace yWorks.Controls.Input
{
  /// <summary>
  /// Provides extension methods which replace the most prominent methods which were removed, renamed, 
  /// or whose signature has been changed between yFiles WPF 2.5 and yFiles WPF 3.0.
  /// </summary>
  /// <remarks>
  /// We recommend to inline these methods once your projects have been migrated to yFiles WPF 3.0.
  /// </remarks>
  public static class InputCompat
  {
    public static void AddConcurrent(this MultiplexingInputMode mux, IInputMode mode) {
      mux.Add(mode);
    }

    public static IModelItem FindItem(this GraphEditorInputMode mode, PointD location,
      GraphItemTypes[] tests, Predicate<IModelItem> filter, IInputModeContext context) {
      return mode.FindItems(context, location, tests, filter).FirstOrDefault();
    }

  }
}

namespace yWorks.Controls
{
  /// <summary>
  /// Provides extension methods which replace the most prominent methods which were removed, renamed, 
  /// or whose signature has been changed between yFiles WPF 2.5 and yFiles WPF 3.0.
  /// </summary>
  /// <remarks>
  /// We recommend to inline these methods once your projects have been migrated to yFiles WPF 3.0.
  /// </remarks>
  public static class ControlsCompat
  {
    public static ICanvasObject Add(this CanvasControl canvas, object userObject, ICanvasObjectDescriptor descriptor, ICanvasObjectGroup group) {
      return group.AddChild(userObject, descriptor);
    }
  }
}