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
using System.ComponentModel;
using Demo.yFiles.Option.DataBinding;
using Demo.yFiles.Option.Handler;
using yWorks.Layout.Tree;

namespace Demo.yFiles.Layout.Tree.Configuration
{
  /// <summary>
  /// Configuration handler for <see cref="AssistantNodePlacer"/>
  /// </summary>
  /// <remarks>This implementation provides the possibility to change and configure the child handler, too</remarks>
  public sealed class AssistantPlacerConfiguration : RotatableNodePlacerConfigurationBase<AssistantNodePlacer>
  {
    /// <summary>
    /// Symbolic enumeration for the valid child placer types 
    /// </summary>
    /// <remarks>This is a subset of all valid placer configurations (especially, <see cref="AssistantPlacerConfiguration"/> is excluded.</remarks>
    public enum ChildNodePlacerTypes
    {
      DefaultNodePlacer,
      SimpleNodePlacer,
      BusPlacer,
      DoubleLinePlacer,
      LeftRightPlacer,
      ARNodePlacer
    }

    public AssistantPlacerConfiguration() {
      AdoptSettings(new AssistantNodePlacer());
    }

    [OptionItemAttribute(Name = "OptionItem.Index", Value = 0)]
    public override double Spacing { get; set; }

    private INodePlacerConfiguration childNodePlacerCfg;

    [OptionItemAttribute(Name = OptionItem.CustomDialogitemEditor, Value = "NestedChildNodePlacer.OptionItemPresenter")]
    [OptionItemAttribute(Name = "OptionItem.Index", Value = 2)]
    [Description("Child Placer")]
    public INodePlacerConfiguration ChildNodePlacer {
      get { return childNodePlacerCfg; }
      set { childNodePlacerCfg = value; }
    }

    private ChildNodePlacerTypes type;

    [OptionItemAttribute(Name = "OptionItem.Index", Value = 1)]
    public ChildNodePlacerTypes Type {
      get { return type; }
      set {
        if (type != value) {
          type = value;
          childNodePlacerCfg = UpdateChildPlacer();
        }
      }
    }

    /// <summary>
    /// Update the child placer configuration when the child placer type has been changed
    /// </summary>
    /// <returns></returns>
    private INodePlacerConfiguration UpdateChildPlacer() {
      switch (Type) {
        case ChildNodePlacerTypes.DefaultNodePlacer:
          return NodePlacerConfigurations.DefaultNodePlacer.Configuration;
        case ChildNodePlacerTypes.SimpleNodePlacer:
          return NodePlacerConfigurations.SimpleNodePlacer.Configuration;
        case ChildNodePlacerTypes.BusPlacer:
          return NodePlacerConfigurations.BusPlacer.Configuration;
        case ChildNodePlacerTypes.DoubleLinePlacer:
          return NodePlacerConfigurations.DoubleLinePlacer.Configuration;
        case ChildNodePlacerTypes.LeftRightPlacer:
          return NodePlacerConfigurations.LeftRightPlacer.Configuration;
        case ChildNodePlacerTypes.ARNodePlacer:
          return NodePlacerConfigurations.ARNodePlacer.Configuration;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    #region INodePlacerConfiguration Members

    protected override void ConfigurePlacer(AssistantNodePlacer placer) {
      base.ConfigurePlacer(placer);
      placer.ChildNodePlacer = ChildNodePlacer.CreateNodePlacer();
    }

    protected override AssistantNodePlacer CreatePlacerCore() {
      return new AssistantNodePlacer(ModificationMatrix);
    }

    public override void AdoptSettings(INodePlacer nodePlacer) {
      base.AdoptSettings(nodePlacer);
      var dnp = (AssistantNodePlacer) nodePlacer;
      //Configure the child placer configuration handler
      //Create a default configuration
      INodePlacer childNodePlacer = dnp.ChildNodePlacer;
      if (childNodePlacer is DefaultNodePlacer) {
        childNodePlacerCfg = new DefaultNodePlacerConfiguration();
        Type = ChildNodePlacerTypes.DefaultNodePlacer;
      }
      if (childNodePlacer is SimpleNodePlacer) {
        childNodePlacerCfg = new SimpleNodePlacerConfiguration();
        Type = ChildNodePlacerTypes.SimpleNodePlacer;
      }
      if (childNodePlacer is BusNodePlacer) {
        childNodePlacerCfg = new BusPlacerConfiguration();
        Type = ChildNodePlacerTypes.BusPlacer;
      }
      if (childNodePlacer is DoubleLineNodePlacer) {
        childNodePlacerCfg = new DoubleLinePlacerConfiguration();
        Type = ChildNodePlacerTypes.DoubleLinePlacer;
      }
      if (childNodePlacer is AspectRatioNodePlacer) {
        childNodePlacerCfg = new ARNodePlacerConfiguration();
        Type = ChildNodePlacerTypes.ARNodePlacer;
      }
      if (childNodePlacer is LeftRightNodePlacer) {
        childNodePlacerCfg = new LeftRightPlacerConfiguration();
        Type = ChildNodePlacerTypes.LeftRightPlacer;
      }
      //And adopt the current values
      childNodePlacerCfg.AdoptSettings(childNodePlacer);
    }

    #endregion
  }
}