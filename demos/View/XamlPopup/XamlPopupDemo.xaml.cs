/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.1.
 ** Copyright (c) 2000-2026 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace yWorks.Demo.XamlPopup
{
  [Demo("xaml-popup", "view", "XAML Popups",
      Summary = "Shows popovers with additional information about a clicked node or edge.",
      Tags = ["interaction", "overlay"],
      Keywords = ["XAML popups", "popovers", "data panel", "tool tips", "structures", "details"])]
  public sealed partial class XamlPopupDemo
  {
    private readonly Dictionary<string, ImageSource> portraitCache = [];
    private GraphViewerInputMode inputMode = null!;

    /// <summary>
    ///   Initializes the demo.
    /// </summary>
    protected override void Initialize() {
      InitializeInputMode();
      InitializePopups();
      BuildGraph(graphControl.Graph);
      graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Opens a node/edge popup on click.
    /// </summary>
    /// <remarks>
    ///   The <see cref="PopoverManager"/> in combination with popovers
    ///   configured with <see cref="PopoverBehavior.Auto"/> ensures that there is only one popup opened at a time.
    ///   This demo watches the <see cref="GraphControl.CurrentItem"/> so that clicks on items but also
    ///   keyboard navigation opens the popup for the currently focused item.
    /// </remarks>
    private void InitializePopups() {
      // The pop-up is shown for the current item, thus nodes and edges should be focusable
      inputMode.FocusableItems = GraphItemTypes.Node | GraphItemTypes.Edge;

      // Register a listener that shows the pop-up for the current item
      graphControl.CurrentItemChanged += (_, _) => {
        switch (graphControl.CurrentItem) {
          case INode node when node.Tag is Employee employee:
            OpenNodePopup(node, employee);
            break;
          case IEdge edge:
            OpenEdgePopup(edge);
            break;
        }
      };
    }

    /// <summary>
    ///   Opens a popup that shows the business data of the given node.
    /// </summary>
    /// <param name="node">The node for which to create the popup.</param>
    /// <param name="employee">The business data to show.</param>
    private void OpenNodePopup(INode node, Employee employee) {
      var layout = node.Layout;
      _ = inputMode.PopoverManager.Open(new PopoverDescriptor {
          Behavior = PopoverBehavior.Auto,
          // display the popup above the node
          Ratios = new PointD(0.5, 1),
          Offset = new PointD(0, -20),
          Anchor = new PointD(layout.GetCenterX(), layout.Y),
          Content = new ContentControl {
              Content = employee,
              ContentTemplate = (DataTemplate) FindResource("EmployeePopupTemplate")
          }
      });
    }

    /// <summary>
    ///   Opens a popup that shows the business data of the given edge.
    /// </summary>
    /// <param name="edge">The edge for which to create the popup.</param>
    private void OpenEdgePopup(IEdge edge) {
      // get business data from node tags
      if (edge.SourcePort.Owner.Tag is not Employee source || edge.TargetPort.Owner.Tag is not Employee target) {
        return;
      }

      var bounds = edge.Style.Renderer.GetBoundsProvider(edge, edge.Style).GetBounds(graphControl.CanvasContext);
      _ = inputMode.PopoverManager.Open(new PopoverDescriptor {
          Behavior = PopoverBehavior.Auto,
          // display the popup in the center of the edge's bounds
          Ratios = new PointD(0.5, 0.5),
          Anchor = bounds.GetCenter(),
          Content = new ContentControl {
              Content = new EmployeeConnection(source.Name, target.Name),
              ContentTemplate = (DataTemplate) FindResource("EmployeeConnectionPopupTemplate")
          }
      });
    }

    /// <summary>
    ///   Iterates through the given data set and creates nodes and edges according to the given data.
    /// </summary>
    private void BuildGraph(IGraph graph) {
      using var stream = LoadAssemblyResource("graph-data.json");
      using var json = JsonDocument.Parse(stream);
      BuildGraphHelper.BuildGraph(graph, json, typeof(Employee));

      foreach (var node in graph.Nodes) {
        if (node.Tag is Employee employee) {
          var icon = GetPortrait(employee.Icon);
          employee.Picture = icon;
          graph.SetStyle(node, new ImageNodeStyle(icon) {
              KeepAspectRatio = true,
              OutlineShape = new Ellipse { Stretch = Stretch.Fill }
          });
        }
      }
    }

    private ImageSource GetPortrait(string? icon) {
      if (string.IsNullOrWhiteSpace(icon)) {
        throw new InvalidDataException("An employee portrait is missing from the graph data.");
      }

      if (!portraitCache.TryGetValue(icon, out var image)) {
        image = TryFindResource(icon) as ImageSource ??
            throw new InvalidDataException($"The employee portrait '{icon}' is missing from the icon resources.");
        portraitCache.Add(icon, image);
      }

      return image;
    }

    /// <summary>
    ///   Creates a viewer input mode for the graphControl of this demo.
    /// </summary>
    private void InitializeInputMode() {
      inputMode = new GraphViewerInputMode {
          ToolTipItems = GraphItemTypes.Node,
          SelectableItems = GraphItemTypes.None,
          MarqueeSelectableItems = GraphItemTypes.None
      };

      // As the selection is deactivated, the focused item is highlighted instead
      graphControl.FocusIndicatorManager.ShowFocusPolicy = ShowFocusPolicy.WhenFocused;

      inputMode.ToolTipInputMode.ToolTipLocationOffset = new PointD(10, 10);
      inputMode.QueryItemToolTip += (_, args) => {
        if (args.Item is INode { Tag: Employee { Name: { Length: > 0 } name } } && !args.Handled) {
          args.ToolTip = name;
          args.Handled = true;
        }
      };

      graphControl.InputMode = inputMode;
    }

    [STAThread]
    public static void Main() {
      Run<XamlPopupDemo>();
    }

    public XamlPopupDemo() {
      InitializeComponent();
    }
  }

  /// <summary>
  ///   A type that describes an Employee.
  /// </summary>
  public sealed class Employee
  {
    public string? Position { get; init; }

    public string? Name { get; init; }

    public string? Email { get; init; }

    public string? Phone { get; init; }

    public string? Icon { get; init; }

    [JsonIgnore]
    public ImageSource? Picture { get; set; }
  }

  public sealed record EmployeeConnection(string? SourceName, string? TargetName);
}
