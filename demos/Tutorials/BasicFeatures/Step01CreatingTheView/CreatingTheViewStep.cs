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

using System.Windows.Controls;
using System.Windows.Threading;
using yWorks.Controls;

namespace yWorks.Demo.Tutorial.BasicFeatures.Step01CreatingTheView
{
  /// <summary>
  ///   Tutorial step 01: Creating the View.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     This step introduces <see cref="GraphControl"/>, the central UI element for working with
  ///     graphs.  It shows the minimal code needed to display a <see cref="GraphControl"/> and
  ///     populate it with a single labeled node.
  ///   </para>
  ///   <para>
  ///     Unlike other steps, this one overrides <see cref="TutorialStepBase.Start"/> directly
  ///     to make the <see cref="GraphControl"/> creation and placement explicit, demonstrating
  ///     exactly what the tutorial step is teaching.
  ///   </para>
  /// </remarks>
  public class CreatingTheViewStep : TutorialStepBase
  {
    public override string Title => "01 Creating the View";

    // The embedded resource logical name is <RootNamespace>.Step01CreatingTheView.description.html.
    // MSBuild replaces folder separators with dots, so we match by the dot-separated suffix.
    public override string DescriptionSource => "Step01CreatingTheView.description.html";

    /// <summary>
    ///   Overrides <see cref="TutorialStepBase.Start"/> to make the <see cref="GraphControl"/>
    ///   creation and placement explicit — this is what the step is teaching.
    /// </summary>
    public override void Start(ContentControl container, Description description) {
      // create a new GraphControl
      var graphControl = new GraphControl();

      // place it in the container
      container.Content = graphControl;

      // register it with the base class so CleanUp can release it correctly
      SetGraphControl(graphControl);

      var graph = graphControl.Graph;

      // create a single node as an example
      var node = graph.CreateNode();
      // and add a label
      graph.AddLabel(node, "y");
      // then center the graph in the component.
      // FitGraphBounds is deferred via the Dispatcher (equivalent to Java's EventQueue.invokeLater)
      // so that the GraphControl has been laid out and has a non-zero size before fitting.
      graphControl.Dispatcher.InvokeAsync(() => graphControl.FitGraphBounds(), DispatcherPriority.Loaded);
    }
  }
}
