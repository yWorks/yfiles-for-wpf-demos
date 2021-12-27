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
using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.CustomPortModel
{
  public partial class CustomPortModelWindow
  {
    public CustomPortModelWindow() {
      InitializeComponent();
    }

    private void OnLoaded(object sender, EventArgs e) {
      // initialize the graph
      InitializeGraph();
      // initialize the input mode
      InitializeInputModes();
    }

    /// <summary>
    /// Calls <see cref="CreateEditorMode"/> and registers
    /// the result as the <see cref="CanvasControl.InputMode"/>.
    /// </summary>
    protected virtual void InitializeInputModes() {
      graphControl.InputMode = CreateEditorMode();
    }

    /// <summary>
    /// Creates the default input mode for the GraphControl,
    /// a <see cref="GraphEditorInputMode"/>.
    /// </summary>
    /// <returns>a new GraphEditorInputMode instance</returns>
    protected virtual IInputMode CreateEditorMode() {

      // for selected nodes show the handles
      graphControl.Graph.GetDecorator().NodeDecorator.HandleProviderDecorator.SetFactory(
        node => new PortsHandleProvider(node));

      // for nodes add a custom port candidate provider implementation which uses our model
      graphControl.Graph.GetDecorator().NodeDecorator.PortCandidateProviderDecorator.SetFactory(GetPortCandidateProvider);

      return new GraphEditorInputMode();
    }

    /// <summary>
    ///  Callback used by the decorator in <see cref="CreateEditorMode"/>
    /// </summary>
    private IPortCandidateProvider GetPortCandidateProvider(INode forNode) {
      var model = new MyNodePortLocationModel {Inset = 10};
      return PortCandidateProviders.FromCandidates(
          new DefaultPortCandidate(forNode, model.CreateParameter(PortLocation.Center)),
          new DefaultPortCandidate(forNode, model.CreateParameter(PortLocation.North)),
          new DefaultPortCandidate(forNode, model.CreateParameter(PortLocation.East)),
          new DefaultPortCandidate(forNode, model.CreateParameter(PortLocation.South)),
          new DefaultPortCandidate(forNode, model.CreateParameter(PortLocation.West)));
    }

    /// <summary>
    /// Sets a custom node port model parameter instance for newly created
    /// node ports in the graph, creates a example nodes with a ports using
    /// the our model and an edge to connect the ports.
    /// </summary>
    protected void InitializeGraph() {
      graphControl.Graph.NodeDefaults.Ports.LocationParameter = new MyNodePortLocationModel().CreateParameter(PortLocation.Center);
      graphControl.Graph.NodeDefaults.Ports.Style = new NodeStylePortStyleAdapter(new ShapeNodeStyle() {Shape = ShapeNodeShape.Ellipse, Brush = Brushes.Red, Pen = null}) {RenderSize = new SizeD(3, 3)};
      graphControl.Graph.NodeDefaults.Size = new SizeD(50, 50);
      graphControl.Graph.NodeDefaults.Style = new ShinyPlateNodeStyle { Brush = Brushes.Orange };

      var source = graphControl.Graph.CreateNode(new RectD(90, 90, 100, 100));
      var target = graphControl.Graph.CreateNode(new RectD(250, 90, 100, 100));

      // creates a port using the default declared above
      var sourcePort = graphControl.Graph.AddPort(source);
      // creates a port using the custom model instance
      var targetPort = graphControl.Graph.AddPort(target, new MyNodePortLocationModel { Inset = 10 }.CreateParameter(PortLocation.North));

      // create an edge
      graphControl.Graph.CreateEdge(sourcePort, targetPort);
    }

    private void OnExitClicked(object sender, RoutedEventArgs e) {
      Application.Current.Shutdown();
    }
  }
}
