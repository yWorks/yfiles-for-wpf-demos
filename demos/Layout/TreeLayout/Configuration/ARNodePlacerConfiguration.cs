/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using yWorks.Layout.Tree;

namespace Demo.yFiles.Layout.Tree.Configuration
{
  /// <summary>
  /// Configuration handler for <see cref="AspectRatioNodePlacer"/>
  /// </summary>
  /// <remarks>This implementation maps directly to the placer's property</remarks>
  public sealed class ARNodePlacerConfiguration : INodePlacerConfiguration
  {
    public ARNodePlacerConfiguration() {
      AdoptSettings(new AspectRatioNodePlacer());
    }

    public bool Horizontal { get; set; }

    public FillStyle FillStyle { get; set; }

    public double VerticalDistance { get; set; }

    public double HorizontalDistance { get; set; }

    public double AspectRatio { get; set; }

    #region INodePlacerConfiguration Members

    public INodePlacer CreateNodePlacer() {
      return new AspectRatioNodePlacer
               {
                 AspectRatio = AspectRatio,
                 HorizontalDistance = HorizontalDistance,
                 VerticalDistance = VerticalDistance,
                 FillStyle = FillStyle,
                 Horizontal = Horizontal,
               };
    }

    public void AdoptSettings(INodePlacer nodePlacer) {
      var dnp = (AspectRatioNodePlacer) nodePlacer;
      AspectRatio = dnp.AspectRatio;
      HorizontalDistance = dnp.HorizontalDistance;
      VerticalDistance = dnp.VerticalDistance;
      FillStyle = dnp.FillStyle;
      Horizontal = dnp.Horizontal;
    }

    #endregion
  }
}