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

using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;

namespace yWorks.Demo.Tutorial.StyleImplementationNode.Step04ConfigurableStyle
{
  /// <summary>
  ///   Tutorial step 04: Configurable style.
  /// </summary>
  public class ConfigurableStyleStep : TutorialStepBase
  {
    public override string Title => "04 Configurable Style";

    public override string DescriptionSource => "Step04ConfigurableStyle.description.html";

    protected override void OnStart(GraphControl graphControl, Description description) {
      var graph = graphControl.Graph;

      // the fill brush can be specified in the constructor
      var styleWithRedFill = new CustomNodeStyle(new SolidColorBrush(Color.FromRgb(185, 28, 59)));

      // the fill brush can also be changed later using the property on the style class
      var styleWithPurpleFill = new CustomNodeStyle(Brushes.Gray);
      styleWithPurpleFill.FillBrush = new SolidColorBrush(Color.FromRgb(158, 124, 181));

      // not specifying the fill brush will use the default brush
      var styleWithDefaultFill = new CustomNodeStyle();

      graph.CreateNode(new RectD(0, 0, 100, 70), styleWithRedFill);
      graph.CreateNode(new RectD(140, 0, 100, 70), styleWithPurpleFill);
      graph.CreateNode(new RectD(70, 150, 100, 70), styleWithDefaultFill);

      CustomNodeStyleTutorial.EnableGraphEditing(graphControl);
      CustomNodeStyleTutorial.FitGraphBounds(graphControl);
    }
  }
}
