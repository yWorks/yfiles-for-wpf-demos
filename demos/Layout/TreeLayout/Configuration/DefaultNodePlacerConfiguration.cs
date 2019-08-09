/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using Demo.yFiles.Option.DataBinding;
using yWorks.Layout.Tree;

namespace Demo.yFiles.Layout.Tree.Configuration
{
  /// <summary>
  /// Configuration handler for <see cref="SimpleNodePlacer"/>
  /// </summary>
  /// <remarks>This implementation maps directly to <see cref="DefaultNodePlacer"/> properties.</remarks>
  public sealed class DefaultNodePlacerConfiguration : INodePlacerConfiguration
  {
    public DefaultNodePlacerConfiguration() {
      AdoptSettings(new DefaultNodePlacer());
    }

    [OptionItemAttribute(Name = "OptionItem.Index", Value = 4)]
    public RootAlignment RootAlignment { get; set; }

    [OptionItemAttribute(Name = "OptionItem.Index", Value = 3)]
    public double VerticalDistance { get; set; }

    [OptionItemAttribute(Name = "OptionItem.Index", Value = 2)]
    public double HorizontalDistance { get; set; }

    [OptionItemAttribute(Name = "OptionItem.Index", Value = 1)]
    public RoutingStyle RoutingStyle { get; set; }

    [OptionItemAttribute(Name = "OptionItem.Index", Value = 0)]
    public ChildPlacement ChildPlacement { get; set; }

    #region INodePlacerConfiguration Members

    public INodePlacer CreateNodePlacer() {
      return new DefaultNodePlacer
               {
                 ChildPlacement = ChildPlacement,
                 RoutingStyle = RoutingStyle,
                 HorizontalDistance = HorizontalDistance,
                 VerticalDistance = VerticalDistance,
                 RootAlignment = RootAlignment,
               };
    }

    public void AdoptSettings(INodePlacer nodePlacer) {
      var dnp = (DefaultNodePlacer) nodePlacer;
      ChildPlacement = dnp.ChildPlacement;
      RoutingStyle = dnp.RoutingStyle;
      HorizontalDistance = dnp.HorizontalDistance;
      VerticalDistance = dnp.VerticalDistance;
      RootAlignment = dnp.RootAlignment;
    }

    #endregion
  }
}