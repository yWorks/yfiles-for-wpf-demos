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
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

namespace yWorks.Demo.OffsetWrapperLabelModel
{
  [Demo("offset-wrapper-label-model", "input", "Offset Wrapper Label Model",
      Summary = "Shows how to implement a custom label model that adds an adjustable offset to labels.",
      Tags = ["interaction", "label"], Keywords = ["placements", "ILabelModel", "playground"])]
  public class OffsetWrapperLabelModelDemo : SimpleDemo
  {
    protected override void Initialize() {
      // Create the example graph
      var node1 = Graph.CreateNode(new RectD(0, 0, 100, 100));
      var node2 = Graph.CreateNode(new RectD(500, 0, 100, 100));
      var edge = Graph.CreateEdge(node1, node2, new ArcEdgeStyle {
          Height = 50,
          Pen = new Pen(new SolidColorBrush(Color.FromRgb(173, 173, 173)), 1.5)
      });
      GraphControl.FitGraphBounds();

      // Add reference labels with default parameter
      var innerParameter = new EdgePathLabelModel { AutoRotation = true }.CreateRatioParameter();
      Graph.AddLabel(edge, "Reference", innerParameter);

      // Add offset labels with different offsets
      Graph.AddLabel(edge, "Offset: 25", new OffsetWrapperLabelModel(25).CreateParameter(innerParameter));
      Graph.AddLabel(edge, "Offset:-25", new OffsetWrapperLabelModel(-25).CreateParameter(innerParameter));

      // Enable graph editing
      GraphControl.InputMode = new GraphEditorInputMode();
    }

    [STAThread]
    private static void Main() => Run<OffsetWrapperLabelModelDemo>();
  }
}
