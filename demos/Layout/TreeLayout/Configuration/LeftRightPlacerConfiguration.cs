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

using yWorks.Layout.Tree;

namespace Demo.yFiles.Layout.Tree.Configuration
{
  /// <summary>
  /// Configuration handler for <see cref="LeftRightNodePlacer"/>
  /// </summary>
  public sealed class LeftRightPlacerConfiguration : RotatableNodePlacerConfigurationBase<LeftRightNodePlacer>
  {
    public LeftRightPlacerConfiguration() {
      AdoptSettings(new LeftRightNodePlacer());
    }

    public bool LastOnBottom { get; set; }

    #region INodePlacerConfiguration Members

    protected override void ConfigurePlacer(LeftRightNodePlacer placer) {
      base.ConfigurePlacer(placer);
      placer.PlaceLastOnBottom = LastOnBottom;
    }

    public override void AdoptSettings(INodePlacer nodePlacer) {
      base.AdoptSettings(nodePlacer);
      var dnp = (LeftRightNodePlacer) nodePlacer;
      LastOnBottom = dnp.PlaceLastOnBottom;
    }

    protected override LeftRightNodePlacer CreatePlacerCore() {
      return new LeftRightNodePlacer(ModificationMatrix);
    }

    #endregion
  }
}