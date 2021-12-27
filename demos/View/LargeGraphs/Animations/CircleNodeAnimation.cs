/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Linq;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Graph.LargeGraphs.Animations
{
  /// <summary>
  ///   An animation that moves nodes in a circular motion.
  /// </summary>
  public class CircleNodeAnimation : IAnimation
  {
    /// <summary>The graph the nodes belong to.</summary>
    private readonly IGraph graph;

    /// <summary>The list of nodes to move.</summary>
    private readonly List<INode> nodes;

    /// <summary>The radius of the movement circle.</summary>
    private readonly double radius;

    /// <summary>The number of revolutions around the circle.</summary>
    private readonly double revolutions;

    /// <summary>A list of the nodes' start locations.</summary>
    private List<RectD> startBounds;

    /// <summary>
    ///   Initializes a new instance of the <see cref="CircleNodeAnimation" /> class with the given graph, nodes, radius,
    ///   number of revolutions and preferred duration.
    /// </summary>
    /// <param name="g">The graph the nodes belong to.</param>
    /// <param name="nodes">The nodes.</param>
    /// <param name="radius">The radius of the movement circle.</param>
    /// <param name="revolutions">The number of revolutions around the circle.</param>
    /// <param name="preferredDuration">Preferred duration of the animation.</param>
    public CircleNodeAnimation(IGraph g, IEnumerable<INode> nodes, double radius, double revolutions,
                               TimeSpan preferredDuration) {
      graph = g;
      this.radius = radius;
      this.nodes = nodes.ToList();
      this.revolutions = revolutions;
      PreferredDuration = preferredDuration;
    }

    /// <inheritdoc />
    public void Initialize() {
      startBounds = nodes.Select(n => n.Layout.ToRectD()).ToList();
    }

    /// <inheritdoc />
    public void Animate(double time) {
      var totalAngle = 2 * Math.PI * revolutions;
      var currentAngle = totalAngle * time;
      var offset = new PointD(Math.Cos(currentAngle) * radius, Math.Sin(currentAngle) * radius);
      for (int i = 0; i < nodes.Count; i++) {
        var n = nodes[i];
        var newPosition = startBounds[i].TopLeft - new PointD(radius, 0) + offset;
        graph.SetNodeLayout(n, new RectD(newPosition, startBounds[i].Size));
      }
    }

    /// <inheritdoc />
    public void CleanUp() {}

    /// <inheritdoc />
    public TimeSpan PreferredDuration { get; private set; }
  }
}