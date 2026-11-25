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
using System.Threading.Tasks;
using yWorks.Controls;
using yWorks.Graph;
using yWorks.Layout;
using yWorks.Layout.Circular;

namespace yWorks.Demo.CircularSubstructures
{
  /// <summary>
  ///   The settings related to substructures for the layout algorithm.
  /// </summary>
  public static class ConfigureLayout
  {
    /// <summary>
    ///   Calculates a new graph layout and optionally applies the new layout in an animated fashion.
    ///   This method creates and configures a new circular layout algorithm for this purpose.
    /// </summary>
    public static async Task RunLayoutCoreAsync(GraphControl graphControl, Settings settings, bool animate) {
      var layout = new CircularLayout();

      var starStyle = settings.StarSubstructureStyle;
      if (starStyle != StarSubstructureStyle.None) {
        // For a more compact layout, do not place children on a common radius for radial star styles.
        layout.PlaceChildrenOnCommonRadius = false;
      }
      // Configure the star substructure style.
      layout.StarSubstructureStyle = starStyle;

      // Configure type separation for star substructures.
      layout.StarSubstructureTypeSeparation = settings.StarSubstructureTypeSeparation;

      // Layout data is necessary to support data-driven features like node types.
      var layoutData = layout.CreateLayoutData(graphControl.Graph);

      if (settings.ConsiderNodeTypes) {
        // If node types should be considered, define a delegate on the respective layout data property
        // that queries the type from the node's tag.
        layoutData.NodeTypes.MapperFunction = node => TypesPopup.GetNodeType(node);
      }

      if (animate) {
        // Run the layout algorithm and apply the result with a morph animation.
        await graphControl.ApplyLayoutAnimated(layout, TimeSpan.FromMilliseconds(500), layoutData);
      } else {
        // Run the layout algorithm and apply the result without an animation.
        graphControl.Graph.ApplyLayout(layout, layoutData);
        await graphControl.FitGraphBounds();
      }
    }
  }
}
