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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;
using yWorks.Utils;

namespace yWorks.Demo.MoveUnselectedNodes
{
  /// <summary>
  ///   Demonstrates how moving unselected nodes can be constrained, for example with keys or custom hit testing.
  /// </summary>
  [Demo("move-unselected-nodes", "input", "Move Unselected Nodes",
      Summary = "Shows a special input mode that allows you to move nodes without selecting them first.",
      Tags = ["interaction", "selection"], Keywords = ["input mode", "move"])]
  public partial class MoveUnselectedNodesDemo
  {
    private readonly Settings settings = new();
    private MoveInputMode moveUnselectedItemsInputMode = null!;
    private MoveInputMode moveSelectedItemsInputMode = null!;

    public MoveUnselectedNodesDemo() {
      InitializeComponent();
      DataContext = settings;
      settings.PropertyChanged += OnSettingsPropertyChanged;
    }

    protected override void Initialize() {
      InitializeGraph();
      InitializeInputModes();

      // Pre-select the 'Drag at Top' mode.
      UpdateMoveMode();
      UpdateEdgeCreationMode();
    }

    /// <summary>
    ///   Initializes the graph instance, setting default styles and creating a small sample graph.
    /// </summary>
    private void InitializeGraph() {
      var graph = graphControl.Graph;

      // Set the default node style.
      graph.NodeDefaults.Style = new GroupNodeStyle {
          TabBrush = Themes.PaletteOrange.Fill,
          ContentAreaPadding = InsetsD.Empty
      };
      graph.NodeDefaults.Size = new SizeD(60, 80);
      graph.NodeDefaults.Labels.LayoutParameter = InteriorNodeLabelModel.Top;
      graph.NodeDefaults.Labels.Style = new LabelStyle { TextBrush = Brushes.White };

      graph.EdgeDefaults.Style = DemoStyles.CreateDemoEdgeStyle();

      // Create a sample node.
      graph.AddLabel(graph.CreateNode(), "Node");

      graphControl.FitGraphBounds();
    }

    /// <summary>
    ///   Creates and registers the input modes.
    /// </summary>
    private void InitializeInputModes() {
      var graphEditorInputMode = new GraphEditorInputMode();

      // Always add a label to the newly created nodes.
      graphEditorInputMode.NodeCreated += (_, args) => graphControl.Graph.AddLabel(args.Item, "Node");

      // Enable the MoveUnselectedInputMode.
      moveUnselectedItemsInputMode = graphEditorInputMode.MoveUnselectedItemsInputMode;
      moveUnselectedItemsInputMode.Enabled = true;
      moveSelectedItemsInputMode = graphEditorInputMode.MoveSelectedItemsInputMode;

      // The recognizers should behave differently, depending on what mode is selected in the demo.
      var originalBeginRecognizer = moveUnselectedItemsInputMode.BeginRecognizer;
      moveUnselectedItemsInputMode.BeginRecognizer = (source, evt) =>
          originalBeginRecognizer(source, evt) && IsRecognized(evt, source);
      var originalHoverRecognizer = moveUnselectedItemsInputMode.HoverRecognizer;
      moveUnselectedItemsInputMode.HoverRecognizer = (source, evt) =>
          originalHoverRecognizer(source, evt) && IsRecognized(evt, source);

      graphControl.InputMode = graphEditorInputMode;
    }

    /// <summary>
    ///   Called when the move mode changes. If necessary, it changes the hit testable for the move input mode.
    /// </summary>
    private void UpdateMoveMode() {
      var geim = (GraphEditorInputMode) graphControl.InputMode!;
      if (settings.UnselectedMoveMode == UnselectedMoveMode.DragAtTop) {
        // Mode Drag at Top (only top region): set a custom hit-testable which detects hits only at the top of the nodes.
        if (moveUnselectedItemsInputMode.HitTestable is not TopPaddingHitTestable) {
          moveUnselectedItemsInputMode.HitTestable =
              new TopPaddingHitTestable(moveUnselectedItemsInputMode.HitTestable);
        }
      } else if (moveUnselectedItemsInputMode.HitTestable is TopPaddingHitTestable topPaddingHitTestable) {
        // For all other modes, if a TopPaddingHitTestable is the current hit-testable, restore the original hit testable.
        moveUnselectedItemsInputMode.HitTestable = topPaddingHitTestable.Original;
      }

      // Mode Shift Not Pressed: disable directional constraints (which are bound to Shift too).
      geim.CreateEdgeInputMode.DirectionalConstraintRecognizer =
          settings.UnselectedMoveMode == UnselectedMoveMode.ShiftNotPressed
              ? EventRecognizers.Never
              : EventRecognizers.ShiftIsDown;

      UpdateEdgeCreationMode();
    }

    /// <summary>
    ///   Called when the edge creation mode changes and adjusts the edge creation behavior.
    /// </summary>
    private void UpdateEdgeCreationMode() {
      var geim = (GraphEditorInputMode) graphControl.InputMode!;
      if (settings.EdgeCreationMode == EdgeCreationType.Default) {
        moveUnselectedItemsInputMode.Priority = 41;
        moveSelectedItemsInputMode.Priority = 40;
        geim.CreateEdgeInputMode.StartOverCandidateOnly = false;
      } else {
        moveUnselectedItemsInputMode.Priority = 47;
        moveSelectedItemsInputMode.Priority = 46;
        geim.CreateEdgeInputMode.StartOverCandidateOnly = true;
      }

      geim.CreateEdgeInputMode.ShowPortCandidates =
          settings.EdgeCreationMode == EdgeCreationType.Default ||
          settings.UnselectedMoveMode == UnselectedMoveMode.Always
              ? ShowPortCandidates.End
              : ShowPortCandidates.All;
    }

    /// <summary>
    ///   A custom EventRecognizer to be used as modifier recognizer.
    ///   Has to return true if the move input mode is allowed to move a node.
    /// </summary>
    private bool IsRecognized(EventArgs evt, object source) {
      // Return the value according to the mode combo box.
      return settings.UnselectedMoveMode switch {
          UnselectedMoveMode.Always => true,
          UnselectedMoveMode.DragAtTop => true, // This is handled by the custom IHitTestable.
          UnselectedMoveMode.ShiftNotPressed => !EventRecognizers.ShiftIsDown(source, evt),
          UnselectedMoveMode.IfMoveEnabled => settings.MoveEnabled,
          _ => false
      };
    }

    /// <summary>Updates the input modes when a toolbar setting changes.</summary>
    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs args) {
      if (args.PropertyName == nameof(Settings.UnselectedMoveMode)) {
        UpdateMoveMode();
      } else if (args.PropertyName == nameof(Settings.EdgeCreationMode)) {
        UpdateEdgeCreationMode();
      } else if (args.PropertyName == nameof(Settings.MoveSelectedNodes)) {
        moveSelectedItemsInputMode.Enabled = settings.MoveSelectedNodes;
      }
    }

    /// <summary>
    ///   An IHitTestable implementation which detects hits only on top padding of a node.
    ///   This instance keeps a reference to the original hit testable, so its behavior can be restored conveniently.
    /// </summary>
    private sealed class TopPaddingHitTestable : IHitTestable
    {
      /// <summary>Creates a new instance of <see cref="TopPaddingHitTestable"/>.</summary>
      public TopPaddingHitTestable(IHitTestable original) {
        Original = original;
      }

      /// <summary>Gets the original hit testable.</summary>
      public IHitTestable Original { get; }

      /// <summary>
      ///   Test whether the given location is a valid hit.
      ///   The hit is considered valid if the location lies inside a node's top padding.
      /// </summary>
      /// <param name="context">The current input mode context.</param>
      /// <param name="location">The location to test.</param>
      public bool IsHit(IInputModeContext context, PointD location) {
        // Get an enumerator over all elements at the given location.
        var hitTester = context.Lookup<IHitTester>();

        if (hitTester != null) {
          return hitTester.EnumerateHits(context, location, GraphItemTypes.Node).OfType<INode>().Any(node => {
            // Determine whether the given location lies inside the top padding.
            var paddingProvider = node.Lookup<IGroupPaddingProvider>();
            return paddingProvider != null &&
                new RectD(node.Layout.X, node.Layout.Y, node.Layout.Width, paddingProvider.GetPadding().Top)
                    .Contains(location);
          });
        }

        // No hits found: return false.
        return false;
      }
    }

    [STAThread]
    public static void Main() => Run<MoveUnselectedNodesDemo>();
  }

  /// <summary>The available ways to move unselected nodes.</summary>
  public enum UnselectedMoveMode
  {
    Always,
    ShiftNotPressed,
    DragAtTop,
    IfMoveEnabled,
    Never
  }

  /// <summary>The available edge creation behaviors.</summary>
  public enum EdgeCreationType
  {
    Default,
    AtPortCandidates
  }

  /// <summary>Settings bound to the demo toolbar.</summary>
  public sealed class Settings : INotifyPropertyChanged
  {
    private UnselectedMoveMode unselectedMoveMode = UnselectedMoveMode.DragAtTop;
    private bool moveEnabled;
    private bool moveSelectedNodes = true;
    private EdgeCreationType edgeCreationMode;

    public event PropertyChangedEventHandler? PropertyChanged;

    public UnselectedMoveMode UnselectedMoveMode {
      get => unselectedMoveMode;
      set {
        if (SetField(ref unselectedMoveMode, value)) {
          OnPropertyChanged(nameof(ShowMoveEnabled));
        }
      }
    }

    public bool MoveEnabled {
      get => moveEnabled;
      set => SetField(ref moveEnabled, value);
    }

    public bool MoveSelectedNodes {
      get => moveSelectedNodes;
      set => SetField(ref moveSelectedNodes, value);
    }

    public EdgeCreationType EdgeCreationMode {
      get => edgeCreationMode;
      set => SetField(ref edgeCreationMode, value);
    }

    public bool ShowMoveEnabled => UnselectedMoveMode == UnselectedMoveMode.IfMoveEnabled;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
      if (Equals(field, value)) {
        return false;
      }
      field = value;
      OnPropertyChanged(propertyName);
      return true;
    }

    private void OnPropertyChanged(string? propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}
