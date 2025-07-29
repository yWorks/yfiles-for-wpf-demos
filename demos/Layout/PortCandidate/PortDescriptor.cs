/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using System.Collections.Generic;
using System.ComponentModel;
using yWorks.Layout;

namespace Demo.yFiles.Layout.PortCandidateDemo
{
  /// <summary>
  ///   A PortDescriptor describes a port, i.e. its location relative to its node, its capacity, its cost, and its direction.
  /// </summary>
  public class PortDescriptor
  {
    /// <summary>
    ///   Relative x-coordinate of the port
    /// </summary>
    [DefaultValue(int.MaxValue)]
    public int X { get; set; } = int.MaxValue;

    /// <summary>
    ///   Relative y-coordinate of the port
    /// </summary>
    [DefaultValue(int.MaxValue)]
    public int Y { get; set; } = int.MaxValue;

    /// <summary>
    ///   Specifies how many edges this port can hold.
    /// </summary>
    [DefaultValue(int.MaxValue)]
    public int Capacity { get; set; } = int.MaxValue;

    /// <summary>
    ///   Specifies the cost for this candidate
    /// </summary>
    [DefaultValue(0)]
    public int Cost { get; set; }

    /// <summary>
    ///   Specifies at which side the port should be located
    /// </summary>
    [DefaultValue(PortSides.Any)]
    public PortSides Side { get; set; } = PortSides.Any;

    /// <summary>
    ///   Creates an enumerable of all edges belonging to the type of the node as specified by its
    ///   <see cref="FlowChartType"/> set as its tag or an empty list if there is no
    ///   <see cref="FlowChartType"/>.
    /// </summary>
    /// <param name="flowChartType"></param>
    public static IEnumerable<PortDescriptor> CreatePortDescriptors(FlowChartType flowChartType) {
      switch (flowChartType) {
        default:
        case FlowChartType.Start:
          return new List<PortDescriptor> { new() { X = 15, Y = 15, Side = PortSides.Any } };
        case FlowChartType.Operation:
          return new List<PortDescriptor> { new() { Side = PortSides.Top }, new() { Side = PortSides.Bottom } };
        case FlowChartType.Branch:
          return new List<PortDescriptor> {
              new() {
                  X = 30,
                  Y = 0,
                  Side = PortSides.Top,
                  Capacity = 1,
                  Cost = 0
              },
              new() {
                  X = 30,
                  Y = 30,
                  Side = PortSides.Bottom,
                  Capacity = 1,
                  Cost = 0
              },
              new() {
                  X = 60,
                  Y = 15,
                  Side = PortSides.Right,
                  Capacity = 1,
                  Cost = 0
              },
              new() {
                  X = 0,
                  Y = 15,
                  Side = PortSides.Left,
                  Capacity = 1,
                  Cost = 0
              },
              new() { X = 30, Y = 0, Side = PortSides.Top, Cost = 1 },
              new() { X = 30, Y = 30, Side = PortSides.Bottom, Cost = 1 }
          };
        case FlowChartType.End:
          return new List<PortDescriptor> {
              new() {
                  X = 15,
                  Y = 0,
                  Side = PortSides.Top,
                  Capacity = 1,
                  Cost = 0
              },
              new() {
                  X = 30,
                  Y = 15,
                  Side = PortSides.Right,
                  Capacity = 1,
                  Cost = 1
              },
              new() {
                  X = 0,
                  Y = 15,
                  Side = PortSides.Left,
                  Capacity = 1,
                  Cost = 1
              },
              new() { X = 30, Y = 15, Side = PortSides.Right, Cost = 2 },
              new() { X = 0, Y = 15, Side = PortSides.Left, Cost = 2 }
          };
      }
    }
  }
}
