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
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using yWorks.Controls.Input;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.LabelModels;

[assembly: XmlnsDefinition("http://www.yworks.com/yfiles-wpf/2.1/demos/GraphClipboard", "Demo.yFiles.Graph.Clipboard")]
[assembly: XmlnsPrefix("http://www.yworks.com/yfiles-wpf/2.1/demos/GraphClipboard", "demo")]


namespace Demo.yFiles.Graph.Clipboard
{
  /// <summary>
  /// This demo shows how to use the <see cref="GraphClipboard"/> for Copy/Paste functionality.
  /// </summary>
  /// <remarks>
  /// This demo also supports grouped graphs, i.e., selected nodes can be grouped 
  /// in so-called group nodes using CTRL-G, and again be ungrouped using CTRL-U. 
  /// To move sets of nodes into and out of group nodes using the mouse, hold down 
  /// the SHIFT key while dragging.
  /// </remarks>
  public partial class GraphClipboardWindow
  {
    /// <summary>
    /// A custom command that uses a different past strategy.
    /// </summary>
    public static readonly RoutedCommand PasteSpecialCommand = new RoutedCommand("PasteSpecial", typeof(GraphClipboardWindow));

    /// <summary>
    /// Automatically generated by Visual Studio.
    /// Wires up the UI components and adds a 
    /// <see cref="GraphControl"/> to the form.
    /// </summary>
    public GraphClipboardWindow() {
      InitializeComponent();

      // set focusedGraphControl to currently focused graphcontrol
      tabControl.SelectionChanged += delegate {
                                           var focusedGraphControl = tabControl.SelectedIndex == 0
                                                                   ? graphControl
                                                                   : graphControl2;
                                           focusedGraphControl.Focus();
                                         };
    }

    #region Initialization

    /// <summary>
    /// Called upon loading of the form.
    /// This method initializes the graph and the input mode.
    /// </summary>
    /// <seealso cref="InitializeInputModes"/>
    /// <seealso cref="InitializeGraph"/>
    protected virtual void OnLoaded(object source, RoutedEventArgs e) {
      // initialize the graph
      InitializeGraph();

      // initialize the input mode
      InitializeInputModes();
    }

    /// <summary>
    /// Initializes the graph instance setting default styles
    /// and creating a small sample graph.
    /// </summary>
    protected virtual void InitializeGraph() {
      IGraph graph = graphControl.Graph;
      
      /// Enable undo/redo.
      graph.SetUndoEngineEnabled(true);

      graph.NodeDefaults.Size = new SizeD(120, 60);

      var style = new NodeControlNodeStyle("ClipboardStyle");
      // set the style as the default for all new nodes
      graph.NodeDefaults.Style = style;

      graph.NodeDefaults.Labels.LayoutParameter = ExteriorLabelModel.North;
      graph.EdgeDefaults.Labels.LayoutParameter = NinePositionsEdgeLabelModel.CenterAbove;

      // add customized copy support for this demo
      graph.GetDecorator().NodeDecorator.ClipboardHelperDecorator.SetImplementation(new TaggedNodeClipboardHelper());

      // Create nodes and an edge.
      var sharedBusinessObject = CreateBusinessObject();

      INode node1 = graph.CreateNode(new PointD(100, 100));
      node1.Tag = sharedBusinessObject;
      INode node2 = graph.CreateNode(new PointD(350, 100)); 
      node2.Tag = sharedBusinessObject;
      INode node3 = graph.CreateNode(new PointD(100, 200)); 
      node3.Tag= CreateBusinessObject();
      graph.AddLabel(node1, "Label 1");
      graph.AddLabel(node2, "Label 2");
      graph.AddLabel(graph.CreateEdge(node1, node2), "Shared Object");

      // reset the Undo queue so the initial graph creation cannot be undone
      graph.GetUndoEngine().Clear();

      // register specialized copiers that can deal with our business objects
      graphControl.Clipboard.FromClipboardCopier.NodeCopied += NodeCopiedOnPaste;
      graphControl.Clipboard.ToClipboardCopier.NodeCopied += NodeCopiedOnCopy;

      // now initialize the second graph
      var graph2 = graphControl2.Graph;
      graph2.SetUndoEngineEnabled(true);
      graph2.NodeDefaults = graph.NodeDefaults;
      graph2.GetDecorator().NodeDecorator.ClipboardHelperDecorator.SetImplementation(new TaggedNodeClipboardHelper());
      graphControl2.Clipboard = graphControl.Clipboard;
    }

    private int counter = 0;

    private ClipboardBusinessObject CreateBusinessObject() {
      return new ClipboardBusinessObject { Name = "Name " + (++counter) };
    }

    /// <summary>
    /// Calls <see cref="CreateEditorMode"/> and registers
    /// the result as the <see cref="CanvasControl.InputMode"/>.
    /// </summary>
    protected virtual void InitializeInputModes() {
      graphControl.InputMode = CreateEditorMode();
      graphControl2.InputMode = CreateEditorMode();
    }

    /// <summary>
    /// Creates the default input mode for the GraphControl,
    /// a <see cref="GraphEditorInputMode"/>.
    /// </summary>
    /// <returns>a new GraphEditorInputMode instance</returns>
    protected virtual IInputMode CreateEditorMode() {
      GraphEditorInputMode mode = new GraphEditorInputMode{AllowGroupingOperations = true};
      mode.NodeCreator = CreateNodeCallback;

      // also, we enable clipboard operations (basically the key bindings/commands)
      mode.AllowClipboardOperations = true;
      return mode;
    }

    private INode CreateNodeCallback(IInputModeContext context, IGraph graph, PointD location, INode parent) {
      // We create a node label and business object automatically for new nodes
      INode node = graph.CreateNode(location);
      node.Tag = CreateBusinessObject();
      graph.AddLabel(node, "Label " + graph.Nodes.Count);
      return node;
    }

    #endregion

    private void NodeCopiedOnCopy(object sender, ItemCopiedEventArgs<INode> e) {
      e.Copy.Tag = graphControl.Clipboard.ToClipboardCopier.GetOrCreateCopy(e.Original.Tag, value => new TagCopyItem(value));
    }

    private void NodeCopiedOnPaste(object sender, ItemCopiedEventArgs<INode> e) {
      e.Copy.Tag = graphControl.Clipboard.FromClipboardCopier.GetOrCreateCopy(e.Original.Tag, CreateBusinessObjectFromTagCopyItem);
    }

    private static object CreateBusinessObjectFromTagCopyItem(object tag) {
      TagCopyItem copyItem = (TagCopyItem) tag;
      copyItem.incPasteCount();
      ClipboardBusinessObject origObject = (ClipboardBusinessObject) copyItem.Tag;
      string name;
      if (copyItem.PasteCount < 2) {
        name = "Copy of " + origObject.Name;
      } else {
        name = "Copy (" + copyItem.PasteCount + ") of " + origObject.Name;
      }
      return new ClipboardBusinessObject {Name = name, Value = origObject.Value};
    }

    private class TagCopyItem {

      private object tag;
      private int pasteCount = 0;

      public TagCopyItem(object tag) {
        this.tag = tag;
      }

      public int PasteCount {
        get { return pasteCount; }
      }

      public void incPasteCount() {
        pasteCount++;
      }

      public object Tag {
        get {
          return tag;
        }
      }
    }

    #region Custom Labels for Pasted Nodes

    /// <summary>
    /// This class is used to assign custom labels to pasted nodes.
    /// </summary>
    /// <remarks>
    /// <see cref="IClipboardHelper"/> implementations can be used to associate custom actions
    /// with graph elements when the element is cut/copied/pasted. Moreover, a clipboard helper
    /// allows to add user state to clipboard operations. This implementation uses these possibilities
    /// to retrieve the label of the original node for a Paste operation and the number of copies
    /// so far in order to set a customised label for the pasted node.
    /// </remarks>
    private class TaggedNodeClipboardHelper : IClipboardHelper
    {
      /// <summary>
      /// We can be copied unconditionally
      /// </summary>
      public bool ShouldCopy(IGraphClipboardContext context, IModelItem item) {
        return true;
      }

      /// <summary>
      /// We can be cut unconditionally
      /// </summary>
      public bool ShouldCut(IGraphClipboardContext context, IModelItem item) {
        return true;
      }

      /// <summary>
      /// We can be pasted unconditionally
      /// </summary>
      public bool ShouldPaste(IGraphClipboardContext context, IModelItem item, object userData) {
        return true;
      }

      public object Copy(IGraphClipboardContext context, IModelItem item) {
        INode node = (INode) item;
        // If we are a Node with at least one label, we
        // store a variant of the label text (see CopyItem
        // implementation)
        if (node.Labels.Count > 0) {
          return new CopyItem(node.Labels[0].Text);
        } else {
          return null;
        }
      }

      public object Cut(IGraphClipboardContext context, IModelItem item) {
        // We do the same for Cut, since it's essentially just a Copy and Delete in succession.
        return Copy(context, item);
      }

      public void Paste(IGraphClipboardContext context, IModelItem item, object userData) {
        //Note: This is the _copied_ item
        INode node = (INode) item;
        if (node.Labels.Count > 0) {
          if (userData is CopyItem && context != null) {
            // If we are a Node with at least one label, we
            // change our text to the one that is provided by userData.
            CopyItem cs = (CopyItem) userData;
            context.TargetGraph.SetLabelText(node.Labels[0], cs.ToString());
          }
        }
      }

      /// <summary>
      /// Instances of this class are used for the user state for the clipboard helper.
      /// </summary>
      private class CopyItem
      {
        private int pasteCount;
        private readonly string text;

        public CopyItem(string text) {
          this.text = text;
          pasteCount = 0;
        }

        public override string ToString() {
          // We count how often we have been pasted and change the string
          // accordingly.
          // If we start from a new copy, the counter is thus reset (try it!)
          pasteCount++;
          if (pasteCount < 2) {
            return "Copy of " + text;
          } else {
            return "Copy (" + pasteCount + ") of " + text;
          }
        }
      }
    }

    #endregion
   
    #region Special Paste Action

    /// <summary>
    /// Paste implementation that uses a filter delegate to copy only nodes and labels as well as a different paste offset.
    /// </summary>
    /// <remarks>The customized paste operations (i.e. label text change) from <see cref="TaggedNodeClipboardHelper"/> 
    /// will still be used.</remarks>
    private void OnPasteSpecialCommandExecuted(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs) {
      var control = (GraphControl)sender;

      GraphClipboard clipboard = control.Clipboard;
      // Clear the old selection.
      control.Selection.Clear();

      // This is the filter delegate for the Paste call.
      Predicate<IModelItem> filter = item => item is INode || (item is ILabel && ((ILabel)item).Owner is INode);
      // This delegate is executed for every pasted element. We use it to select the pasted nodes.
      ElementCopiedCallback pasted =
        delegate(IModelItem originalItem, IModelItem pastedItem) {
          if (pastedItem is INode) {
            control.Selection.SetSelected(pastedItem, true);
          }
        };
      clipboard.Paste(control.Graph, filter, pasted);
      // The next paste location should be shifted a little (just like the normal paste)
      clipboard.PasteDelta += ((GraphEditorInputMode) graphControl.InputMode).PasteDelta;
    }

    #endregion


    /// <summary>
    /// Exits the demo.
    /// </summary>
    private void ExitMenuItem_Click(object sender, EventArgs e) {
      Application.Current.Shutdown();
    }

  }

  public class ClipboardBusinessObject : INotifyPropertyChanged
  {
    private string name;
    private double value = 0.5d;

    public string Name {
      get { return name; }
      set {
        if (name != value) {
          name = value;
          if (PropertyChanged != null) {
            PropertyChanged(this, new PropertyChangedEventArgs("Name"));
          }
        }
      }
    }
      
    public double Value {
      get { return value; }
      set {
        if (value < 0) {
          this.value = 0;
        } else if (value > 1) {
          this.value = 1;
        } else {
          this.value = value;
        }
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
