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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Demo.yFiles.Graph.NetworkMonitoring.Model;

namespace Demo.yFiles.Graph.NetworkMonitoring.Simulator
{
  /// <summary>
  /// A simple simulator that sends packets through the network model.
  /// </summary>
  public class NetworkSimulator : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    public event EventHandler SomethingBroke;

    /// <summary>The number of new packets that should be created per tick.</summary>
    private const int NewPacketsPerTick = 5;

    /// <summary>The probability that a node or edge fails.</summary>
    private const double FailureProbability = 0.001;

    /// <summary>The number of past ticks to consider when calculating the load of nodes and edges.</summary>
    private const int HistorySize = 23;

    #region Backing fields
    private NetworkModel model;
    private bool failuresEnabled;
    #endregion

    private readonly Random random = new Random();

    /// <summary>List of packets in the past that are no longer active, but still need to be retained for calculating the load of nodes and edges.</summary>
    private readonly List<Packet> historicalPackets = new List<Packet>();

    /// <summary>List of active packets that are currently moving around the network.</summary>
    private readonly List<Packet> activePackets = new List<Packet>();

    /// <summary>Current timestamp of the simulation.</summary>
    private int time;

    /// <summary>
    /// Gets or sets the network model to simulate.
    /// </summary>
    public NetworkModel Model {
      get { return model; }
      set {
        model = value;
        OnPropertyChanged("Model");
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether random failures of nodes and edges should happen.
    /// </summary>
    public bool FailuresEnabled {
      get { return failuresEnabled; }
      set {
        failuresEnabled = value;
        OnPropertyChanged("FailuresEnabled");
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Demo.yFiles.Graph.NetworkMonitoring.Simulator.NetworkSimulator"/> class to operate on the given <see cref="NetworkModel"/>.
    /// </summary>
    /// <param name="model">The network model to simulate.</param>
    public NetworkSimulator(NetworkModel model) {
      Model = model;
    }

    /// <summary>
    /// Performs one step in the simulation.
    /// </summary>
    /// <remarks>
    /// Packets move one node per tick. Every tick a number of new packets are created.
    /// </remarks>
    public void Tick() {
      if (FailuresEnabled) {
        BreakThings();
      }

      // Reset packet-related properties on the edges
      foreach (var packet in activePackets) {
        packet.Edge.HasForwardPacket = false;
        packet.Edge.HasBackwardPacket = false;
      }

      PruneOldPackets();
      MovePackets();
      UpdateLoads();

      CreatePackets();

      foreach (var packet in activePackets) {
        packet.Edge.HasForwardPacket |= packet.Start == packet.Edge.Source;
        packet.Edge.HasBackwardPacket |= packet.Start == packet.Edge.Target;
      }

      time++;
    }

    /// <summary>
    /// Determines for every edge and node whether it should fail and does so, if necessary.
    /// </summary>
    private void BreakThings() {
      var thingsThatCanBreak = Model.Nodes.Concat<dynamic>(Model.Edges).Where(x => !x.Failed);
      var thingsThatShouldBreak =
          thingsThatCanBreak.Where(x => random.NextDouble() < FailureProbability*(x.Load + 0.1)).Take(2);
      foreach (var thing in thingsThatShouldBreak) {
        thing.Failed = true;
        OnSomethingFailed(thing);
      }
    }

    /// <summary>
    /// Creates new packets.
    /// </summary>
    /// <remarks>
    /// Packets are only sent from laptops, workstations, smartphones and tablets.
    /// </remarks>
    /// <seealso cref="SimulatorExtensions.CanSendPackets"/>
    private void CreatePackets() {
      // Find all edges that are still enabled and unbroken. Edges are automatically disabled if either endpoint is disabled or broken.
      var enabledEdges = Model.Edges.Where(e => e.Enabled && !e.Failed);

      // Restrict them to those edges that are adjacent to a node that can send packets.
      var eligibleEdges = enabledEdges.Where(e => e.Source.CanSendPackets() || e.Target.CanSendPackets());

      // Pick a number of those edges at random
      var selectedEdges = eligibleEdges.Shuffle(random).Take(NewPacketsPerTick);

      var packets =
          from edge in selectedEdges
          let startNode = edge.Source.CanSendPackets() ? edge.Source : edge.Target
          let endNode = edge.Source.CanSendPackets() ? edge.Target : edge.Source
          select CreatePacket(startNode, endNode, edge);

      activePackets.AddRange(packets);
    }

    /// <summary>
    /// Moves the active packets around the network according to certain rules.
    /// </summary>
    /// <remarks>
    /// Packets move freely and randomly within the network until they arrive at a non-switch, non-WiFi node.
    /// Servers and databases always bounce back a new packet when they receive one, while “client” nodes
    /// simply receive packets and maybe spawn new ones in <see cref="CreatePackets"/>.
    /// </remarks>
    /// <seealso cref="CreatePackets"/>
    /// <seealso cref="SimulatorExtensions.CanConnectTo"/>
    private void MovePackets() {
      // Find packets that need to be considered for moving.
      // This excludes packets that end in a disabled or broken node or that travel along a now-broken edge.
      // We don't care whether the source is alive or not by now.
      var packetsToMove =
          (from p in activePackets
            where p.Edge.Enabled && !p.Edge.Failed
            where p.End.Enabled && !p.End.Failed
            select p).ToList();

      // Packets that arrive at servers or databases. They result in a reply packet.
      var replyPackets =
          (from p in packetsToMove
            where p.End.Kind == NodeKind.Server || p.End.Kind == NodeKind.Database
            // There won't be a reply if the source is dead by now
            where p.Start.Enabled && !p.Start.Failed
            select p).ToList();

      // All other packets that just move on to their next destination.
      var movingPackets =
          (from p in packetsToMove
            where !p.End.CanReceivePackets()
            select p).ToList();

      // All packets have to be moved to the history list. We create new ones appropriately.
      historicalPackets.AddRange(activePackets);
      activePackets.Clear();

      foreach (var packet in movingPackets) {
        var origin = packet.Start;
        var currentEdge = packet.Edge;

        // We start from the old target of the packet
        var startNode = packet.End;

        // Try finding a random edge to follow ...
        var edge = (from e in Model.GetAdjacentEdges(startNode)
          // ... that does not lead back to where we came from
          where e != currentEdge
          // ... and doesn't connect the same kind of node, unless it's a switch
          let edgeTarget = e.Source == startNode ? e.Target : e.Source
          where origin.CanConnectTo(edgeTarget)
          where e.Enabled && !e.Failed
          select e).Shuffle(random).FirstOrDefault();

        // If we don't find one, the packet vanishes
        if (edge == null) {
          continue;
        }

        var endNode = edge.Source == startNode ? edge.Target : edge.Source;

        var newPacket = CreatePacket(startNode, endNode, edge);
        activePackets.Add(newPacket);
      }

      foreach (var packet in replyPackets) {
        // We just bounce a new packet on the same edge, but in reverse direction.
        activePackets.Add(CreatePacket(packet.End, packet.Start, packet.Edge));
      }
    }

    /// <summary>
    /// Removes packets from the history that are no longer considered for edge or node load.
    /// </summary>
    /// <seealso cref="HistorySize"/>
    private void PruneOldPackets() {
      historicalPackets.RemoveAll(p => p.Time < time - HistorySize);
    }

    /// <summary>
    /// Updates load of nodes and edges based on traffic in the network.
    /// </summary>
    /// <remarks>
    /// The criteria are perhaps a bit arbitrary here. Edge load is defined as the number of timestamps in the
    /// history that this edge transmitted a packet. Node load is the number of packets involving this node
    /// adjusted by the number of adjacent edges.
    /// </remarks>
    private void UpdateLoads() {
      var history = activePackets.Concat(historicalPackets).ToList();

      var edgeLoads =
          from edge in Model.Edges
          let numberOfHistoryPackets =
              (from packet in history
                where packet.Edge == edge
                select packet.Time).Distinct().Count()
          select new
          {
            Edge = edge,
            Load = Math.Min(1, (double) numberOfHistoryPackets/HistorySize),
          };

      foreach (var x in edgeLoads) {
        x.Edge.Load = x.Load;
      }

      var nodeLoads =
          from node in Model.Nodes
          let numberOfHistoryPackets =
              (from packet in history
                where packet.Start == node || packet.End == node
                select packet).Count()
          select new
          {
            Node = node,
            Load = Math.Min(1, (double) numberOfHistoryPackets/HistorySize/Model.GetAdjacentEdges(node).Count()),
          };

      foreach (var x in nodeLoads) {
        x.Node.Load = x.Load;
      }
    }

    /// <summary>
    /// Convenience method to create a single packet with the appropriate timestamp.
    /// </summary>
    /// <param name="startNode">The start node of the packet.</param>
    /// <param name="endNode">The end node of the packet.</param>
    /// <param name="edge">The edge on which the packet travels.</param>
    /// <returns>The newly-created packet.</returns>
    private Packet CreatePacket(ModelNode startNode, ModelNode endNode, ModelEdge edge) {
      return new Packet(startNode, endNode, edge, time);
    }

    protected virtual void OnPropertyChanged(string propertyName) {
      var handler = PropertyChanged;
      if (handler != null) {
        handler(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    protected virtual void OnSomethingFailed(object sender) {
      var handler = SomethingBroke;
      if (handler != null) {
        handler(sender, new EventArgs());
      }
    }
  }

  /// <summary>
  /// Simple data structure to model a packet moving through the network.
  /// </summary>
  public struct Packet
  {
    public long Time { get; set; }
    public ModelNode Start { get; set; }
    public ModelNode End { get; set; }
    public ModelEdge Edge { get; set; }

    public Packet(ModelNode start, ModelNode end, ModelEdge edge, long time) : this() {
      Time = time;
      Start = start;
      End = end;
      Edge = edge;
    }
  }

  /// <summary>
  /// Helper class containing extension methods specific to the simulator.
  /// </summary>
  public static class SimulatorExtensions
  {
    /// <summary>
    /// Determines whether the given node can send packets.
    /// </summary>
    /// <remarks>
    /// By definition in our model, neither switches nor WiFi access points can send packets; they just relay them. Servers and databases won't send packets without receiving one first.
    /// </remarks>
    /// <param name="node">The <see cref="ModelNode"/> to check.</param>
    /// <returns><see langword="true"/> if the node is not a switch or access point, <see langword="false"/> otherwise.</returns>
    public static bool CanSendPackets(this ModelNode node) {
      switch (node.Kind) {
        case NodeKind.Switch:
        case NodeKind.Wlan:
        case NodeKind.Server:
        case NodeKind.Database:
          return false;
      }

      return true;
    }

    /// <summary>
    /// Determines whether the given node can receive packets.
    /// </summary>
    /// <remarks>
    /// By definition in our model, switches and WiFi access points only relay packets. Everything else can receive them.
    /// </remarks>
    /// <param name="node">The <see cref="ModelNode"/> to check.</param>
    /// <returns><see langword="true"/> if the node is not a switch or access point, <see langword="false"/> otherwise.</returns>
    public static bool CanReceivePackets(this ModelNode node) {
      switch (node.Kind) {
        case NodeKind.Workstation:
        case NodeKind.Laptop:
        case NodeKind.Smartphone:
        case NodeKind.Server:
        case NodeKind.Database:
          return true;
      }

      return false;
    }

    /// <summary>
    /// Determines whether a packet may take a certain connection to a given <see cref="NodeKind"/>, coming
    /// from a certain <see cref="NodeKind"/>.
    /// </summary>
    /// <remarks>
    /// <para>To make the simulation a bit nicer to watch, we establish a few arbitrary rules here. Packets are not
    /// allowed to visit the same sort of node directly after moving through a switch. For this purpose all
    /// “client” <see cref="NodeKind"/>s are considered equal (laptop, workstation, smartphone). Traffic in
    /// between “relay” nodes, i.e. switch and WiFi access points is always permitted.</para>
    /// 
    /// <para>This means that the following exemplary packet paths are never considered:</para>
    /// 
    /// <list type="bullet">
    ///   <item><description>Server ? Switch ? Server</description></item>
    ///   <item><description>Laptop ? WiFi ? Workstation</description></item>
    ///   <item><description>Workstation ? Switch ? Smartphone</description></item>
    /// </list>
    /// </remarks>
    /// <param name="sourceNode">The source node's kind.</param>
    /// <param name="targetNode">The candidate target node's kind.</param>
    /// <returns>
    /// <see langword="true"/> if the packet could travel to the target node according to the described rules,
    /// <see langword="false"/> otherwise.
    /// </returns>
    public static bool CanConnectTo(this ModelNode sourceNode, ModelNode targetNode) {
      if (sourceNode.Kind == NodeKind.Switch || targetNode.Kind == NodeKind.Switch) {
        return true;
      }

      var clientTypes = new HashSet<NodeKind> { NodeKind.Laptop, NodeKind.Smartphone, NodeKind.Workstation };

      if (clientTypes.Contains(sourceNode.Kind)) {
        return !clientTypes.Contains(targetNode.Kind);
      }

      return sourceNode != targetNode;
    }
  }
}