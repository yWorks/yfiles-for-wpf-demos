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
using System.IO;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.GraphML;

namespace Demo.yFiles.IO.GraphML.Compat
{
  /// <summary>
  /// This demo shows how to display a graph with the GraphViewer component.
  /// </summary>
  public partial class GraphMLCompatibilityWindow
  {

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphMLCompatibilityWindow"/> class.
    /// </summary>
    public GraphMLCompatibilityWindow() {
      InitializeComponent();
    }

    public void OnLoaded(object source, EventArgs args) {
      EnableFolding();

      graphControl.InputMode = new GraphViewerInputMode {
        ToolTipItems = GraphItemTypes.LabelOwner,
        ClickableItems = GraphItemTypes.Node,
        FocusableItems = GraphItemTypes.Node,
        SelectableItems = GraphItemTypes.None,
        MarqueeSelectableItems = GraphItemTypes.None,
        NavigationInputMode = {
          AllowCollapseGroup = true,
          AllowExpandGroup = true,
          UseCurrentItemForCommands = true,
          FitContentAfterGroupActions = false
        }
      };

      // Set up compatibility to load older GraphML files
      // Take a look at the GraphMLCompatibility class for more detail on how this is accomplished
      graphControl.GraphMLIOHandler.ConfigureGraphMLCompatibility();

      graphChooserBox.ItemsSource = new[] { "styles", "table", "nesting", "uml-diagram" };
      graphChooserBox.SelectedIndex = 0;

      graphControl.FitGraphBounds();
    }

    /// <summary>
    /// Enable folding - change the GraphControl's graph to a managed view
    /// that provides the actual collapse/expand state.
    /// </summary>
    private void EnableFolding() {
      // create the manager
      var manager = new FoldingManager();
      // replace the displayed graph with a managed view
      graphControl.Graph = manager.CreateFoldingView().Graph;
    }

    private void ReadSampleGraph() {
      string fileName = Path.Combine("Resources", string.Format("{0}.graphml", graphChooserBox.SelectedItem));
      graphControl.ImportFromGraphML(fileName);
      graphControl.FitGraphBounds();
    }

    private void UpdateButtons() {
      nextButton.IsEnabled = graphChooserBox.SelectedIndex < graphChooserBox.Items.Count - 1;
      previousButton.IsEnabled = graphChooserBox.SelectedIndex > 0;
    }

    private void previousButton_Click(object sender, EventArgs e) {
      graphChooserBox.SelectedIndex--;
      UpdateButtons();
    }

    private void nextButton_Click(object sender, EventArgs e) {
      graphChooserBox.SelectedIndex++;
      UpdateButtons();
    }

    private void graphChooserBox_SelectedIndexChanged(object sender, EventArgs e) {
      ReadSampleGraph();
      UpdateButtons();
    }
  }
}
