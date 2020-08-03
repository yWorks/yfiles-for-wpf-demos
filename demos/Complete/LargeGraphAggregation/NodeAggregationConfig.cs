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
using Demo.yFiles.Layout.Configurations;
using Demo.yFiles.Toolkit.OptionHandler;
using yWorks.Analysis;
using yWorks.Controls;
using yWorks.Layout;

namespace Demo.yFiles.Complete.LargeGraphAggregation {
  
  /// <summary>
  /// A configuration that uses the <see cref="ConfigurationEditor"/> to display a properties panel.
  /// </summary>
  public class NodeAggregationConfig : LayoutConfiguration
  {
    
    /// <summary>
    /// Setup default values for various configuration parameters.
    /// </summary>
    public NodeAggregationConfig() {
      var nodeAggregation = new NodeAggregation();
      
      AggregationPolicy = nodeAggregation.Aggregation;
      MaximumDuration = nodeAggregation.MaximumDuration.TotalSeconds;
      MinimumClusterSize = nodeAggregation.MinimumClusterSize;
      MaximumClusterSize = nodeAggregation.MaximumClusterSize;
    }

    public NodeAggregation CreateConfiguredAggregation() {
      return new NodeAggregation {
          Aggregation = AggregationPolicy,
          MaximumDuration = TimeSpan.FromSeconds(MaximumDuration),
          MinimumClusterSize = MinimumClusterSize,
          MaximumClusterSize = MaximumClusterSize
      };
    }

    /// <inheritdoc />
    protected override ILayoutAlgorithm CreateConfiguredLayout(GraphControl graphControl) {
      // cannot use this method, CreateConfiguredAggregation is used instead
      return null;
    }
    
    [Label("Aggregation Mode")]
    [OptionGroup("RootGroup", 10)]
    [DefaultValue(NodeAggregation.AggregationPolicy.Structural)]
    [EnumValue("Structural", NodeAggregation.AggregationPolicy.Structural)]
    [EnumValue("Geometric", NodeAggregation.AggregationPolicy.Geometric)]
    public NodeAggregation.AggregationPolicy AggregationPolicy { get; set; }

    [Label("Maximum Duration")]
    [OptionGroup("RootGroup", 20)]
    [DefaultValue(0)]
    [MinMax(Min = 0.0d, Max = 120.0d)]
    [ComponentType(ComponentTypes.Slider)]
    public double MaximumDuration { get; set; }

    [Label("Minimum Cluster Size")]
    [OptionGroup("RootGroup", 30)]
    [DefaultValue(5)]
    [MinMax(Min = 1, Max = 50)]
    [ComponentType(ComponentTypes.Slider)]
    public int MinimumClusterSize { get; set; }

    [Label("Maximum Cluster Size")]
    [OptionGroup("RootGroup", 40)]
    [DefaultValue(10)]
    [MinMax(Min = 2, Max = 100)]
    [ComponentType(ComponentTypes.Slider)]
    public int MaximumClusterSize { get; set; }

  }
}
