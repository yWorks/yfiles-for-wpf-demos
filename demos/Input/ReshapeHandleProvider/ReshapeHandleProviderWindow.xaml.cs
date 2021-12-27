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

using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.Input.ReshapeHandleProvider
{
  /// <summary>
  /// Shows how to implement a custom <code>IReshapeHandleProvider</code> for <code>IPort</code>s using a
  /// <code>NodeStylePortStyleAdapter</code>.
  /// </summary>
  public partial class ReshapeHandleProviderWindow {

    /// <summary>
    /// Registers a callback function as decorator that provides a customized <see cref="IReshapeHandleProvider"/> for
    /// each port with a <see cref="NodeStylePortStyleAdapter"/>.
    /// </summary>
    /// <remarks>
    /// This callback function is called whenever a port in the graph is queried
    /// for its <c>IReshapeHandleProvider</c>. In this case, the 'port'
    /// parameter will be set to that port.
    /// </remarks>
    public void RegisterReshapeHandleProvider() {
      graphControl.Graph.GetDecorator().PortDecorator.GetDecoratorFor<IReshapeHandleProvider>().SetFactory(
          port => port.Style is NodeStylePortStyleAdapter,
          port => port.Style is NodeStylePortStyleAdapter adapter 
              ? new PortReshapeHandlerProvider(port, adapter) { MinimumSize = new SizeD(5, 5) } 
              : null);
    }

    #region Initialization

    public ReshapeHandleProviderWindow() {
      InitializeComponent();
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e) {
      IGraph graph = graphControl.Graph;

      // initialize graph defaults
      var adaptedStyle = new ShapeNodeStyle { Brush = Brushes.Green, Pen = Pens.Transparent };
      graph.NodeDefaults.Ports.Style = new NodeStylePortStyleAdapter(adaptedStyle) {RenderSize = new SizeD(7, 7)};
      // each port needs its own style instance to have its own render size
      graph.NodeDefaults.Ports.ShareStyleInstance = false;
      // disable removing ports when all attached edges have been removed
      graph.NodeDefaults.Ports.AutoCleanUp = false;

      graph.EdgeDefaults.Style = new PolylineEdgeStyle { Pen = new Pen(Brushes.Black, 3) };
      
      // create a default editor input mode
      GraphEditorInputMode geim = new GraphEditorInputMode();

      // ports are preferred for clicks
      geim.ClickHitTestOrder = new []{
          GraphItemTypes.Port,
          GraphItemTypes.PortLabel,
          GraphItemTypes.Bend,
          GraphItemTypes.EdgeLabel,
          GraphItemTypes.Edge,
          GraphItemTypes.Node,
          GraphItemTypes.NodeLabel,
      };
      // enable orthogonal edge editing
      geim.OrthogonalEdgeEditingContext = new OrthogonalEdgeEditingContext();

      // PortReshapeHandlerProvider considers pressed Ctrl keys. Whenever Ctrl is pressed or released, 
      // we force GraphEditorInputMode to requery the handles of selected items
      graphControl.KeyDown += UpdateHandles;
      graphControl.KeyUp += UpdateHandles;
      
      // finally, set the input mode to the graph control.
      graphControl.InputMode = geim;

      // register the reshape handle provider for ports
      RegisterReshapeHandleProvider();

      // read initial graph from embedded resource
      graphControl.ImportFromGraphML("Resources\\defaultGraph.graphml");
    }

    private void UpdateHandles(object sender, KeyEventArgs e) {
      if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) {
        ((GraphEditorInputMode) graphControl.InputMode).RequeryHandles();
      }
    }

    #endregion

  }
}
