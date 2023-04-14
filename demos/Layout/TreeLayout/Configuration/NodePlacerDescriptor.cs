/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.5.
 ** Copyright (c) 2000-2022 by yWorks GmbH, Vor dem Kreuzberg 28,
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
  /// Support class that provides the default configurations for the various node placers
  /// </summary>
  public static class NodePlacerConfigurations
  {
    public static NodePlacerDescriptor DefaultNodePlacer {
      get {
        return new NodePlacerDescriptor
                 {
                   Name = "DefaultNodePlacer",
                   Description =
                     "The DefaultNodePlacer places the child nodes horizontally aligned below their root node. It offers options to change the orientation of the subtree, the edge routing style, and the alignment of the root node.",
                   Rotatable = false,
                   Configuration = new DefaultNodePlacerConfiguration()
                 };
      }
    }

    public static NodePlacerDescriptor SimpleNodePlacer {
      get {
        return new NodePlacerDescriptor
                 {
                   Name = "SimpleNodePlacer",
                   Description =
                     "The SimpleNodePlacer places the child nodes horizontally aligned below their root node. It supports rotated subtrees and offers options to change the alignment of the root node.",
                   Rotatable = true,
                   Configuration = new SimpleNodePlacerConfiguration()
                 };
      }
    }

    public static NodePlacerDescriptor BusPlacer {
      get {
        return new NodePlacerDescriptor
                 {
                   Name = "BusPlacer",
                   Description =
                     "The BusPlacer places the child nodes evenly distributed in two lines to the left and right of the root node. It supports rotated subtrees.",
                   Rotatable = true,
                   Configuration = new BusPlacerConfiguration()
                 };
      }
    }

    public static NodePlacerDescriptor DoubleLinePlacer {
      get {
        return new NodePlacerDescriptor
                 {
                   Name = "DoubleLinePlacer",
                   Description =
                     "The DoubleLinePlacer places the child nodes staggered in two lines below their root node. It supports rotated subtrees and offers options to change the alignment of the root node.",
                   Rotatable = true,
                   Configuration = new DoubleLinePlacerConfiguration()
                 };
      }
    }

    public static NodePlacerDescriptor LeftRightPlacer {
      get {
        return new NodePlacerDescriptor
                 {
                   Name = "LeftRightPlacer",
                   Description =
                     "The LeftRightPlacer places the child nodes below their root node, left and right of the downward extending bus-like routing. It supports rotated subtrees",
                   Rotatable = true,
                   Configuration = new LeftRightPlacerConfiguration()
                 };
      }
    }

    public static NodePlacerDescriptor ARNodePlacer {
      get {
        return new NodePlacerDescriptor
                 {
                   Name = "ARNodePlacer",
                   Description =
                     "The ARNodePlacer places the child nodes such that a given aspect ratio is obeyed.",
                   Rotatable = false,
                   Configuration = new ARNodePlacerConfiguration()
                 };
      }
    }

    public static NodePlacerDescriptor AssistantPlacer {
      get {
        return new NodePlacerDescriptor
                 {
                   Name = "AssistantPlacer",
                   Description =
                     "The AssistantPlacer delegates to two different node placers to place the child nodes: Nodes which are marked as \"Assistants\" are placed using a LeftRightPlacer . The other children are placed below the assistant nodes using the child node placer.",
                   Rotatable = true,
                   Configuration = new AssistantPlacerConfiguration()
                 };
      }
    }

    public static NodePlacerDescriptor None {
      get {
        return new NodePlacerDescriptor
                 {
                   Name = "None",
                   Description =
                     "No placer is set for the selected nodes. The default node placer implementation is used.",
                   Rotatable = false,
                   Configuration = new NonePlacerConfiguration()
                 };
      }
    }

    /// <summary>
    /// Factory method to create a new suitably configured <see cref="NodePlacerDescriptor"/> for <paramref name="placer"/>
    /// </summary>
    public static NodePlacerDescriptor GetDescriptor(INodePlacer placer) {
      NodePlacerDescriptor desc = None;
      if (placer is DefaultNodePlacer) {
        desc = DefaultNodePlacer;
      }
      if (placer is SimpleNodePlacer) {
        desc = SimpleNodePlacer;
      }
      if (placer is BusNodePlacer) {
        desc = BusPlacer;
      }
      if (placer is DoubleLineNodePlacer) {
        desc = DoubleLinePlacer;
      }
      if (placer is LeftRightNodePlacer) {
        desc = LeftRightPlacer;
      }
      if (placer is AspectRatioNodePlacer) {
        desc = ARNodePlacer;
      }
      if (placer is AssistantNodePlacer) {
        desc = AssistantPlacer;
      }
      desc.Configuration.AdoptSettings(placer);
      return desc;
    }
  }

  public sealed class NonePlacerConfiguration : INodePlacerConfiguration
  {
    public INodePlacer CreateNodePlacer() {
      return null;
    }

    public void AdoptSettings(INodePlacer nodePlacer) {}
  }

  /// <summary>
  /// Contract for the actual configuration of a node placer.
  /// </summary>
  public interface INodePlacerConfiguration
  {
    /// <summary>
    /// Create a new node placer instance from the configuration's values
    /// </summary>
    /// <returns></returns>
    INodePlacer CreateNodePlacer();

    /// <summary>
    /// Read the <paramref name="nodePlacer"/>'s values into the configuration. 
    /// </summary>
    /// <param name="nodePlacer"></param>
    void AdoptSettings(INodePlacer nodePlacer);
  }

  /// <summary>
  /// Support class that bundles various placer configuration related information.
  /// </summary>
  public class NodePlacerDescriptor
  {
    public INodePlacerConfiguration Configuration { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Rotatable { get; set; }
  }
}