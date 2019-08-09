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

using System.Collections.Generic;
using yWorks.Geometry;
using yWorks.Graph.LabelModels;

namespace Demo.yFiles.Layout.LogicGate
{
  /// <summary>
  /// Helper class that describes properties that are necessary to create port candidates in this demo. 
  /// </summary>
  public class PortDescriptor
  {
    /// <summary>
    /// Relative x coordinate of the port
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Relative y coordinate of the port
    /// </summary>
    public double Y { get; set; }

    public string LabelText { get; set; }

    public ILabelModelParameter LabelPlacementWithEdge { get; set; }

    /// <summary>
    /// Direction of the port
    /// </summary>
    public EdgeDirection EdgeDirection { get; set; }

    /// <summary>
    /// Creates an enumerable of all edges belonging to the type of the node as specified by its
    /// <see cref="LogicGateType"/> set as its tag or an empty list if there is no
    /// <see cref="LogicGateType"/>.
    /// </summary>
    /// <param name="logicGateType"></param>
    public static IEnumerable<PortDescriptor> CreatePortDescriptors(LogicGateType logicGateType) {
      var inside = new InsideOutsidePortLabelModel().CreateInsideParameter();
      var aboveEdgeLeft = FreePortLabelModel.Instance.CreateParameter(new PointD(-5, -3), new PointD(1, 1), PointD.Origin, 0);
      var aboveEdgeRight = FreePortLabelModel.Instance.CreateParameter(new PointD(5, -3), new PointD(0, 1), PointD.Origin, 0);
      var belowEdgeLeft = FreePortLabelModel.Instance.CreateParameter(new PointD(-5, 3), new PointD(1, 0), PointD.Origin, 0);
      switch (logicGateType) {
        default:
        case LogicGateType.And:
          return new List<PortDescriptor> {
            new PortDescriptor {
              X = 0, Y = 5, EdgeDirection = EdgeDirection.In, LabelText = "in1",
              LabelPlacementWithEdge = aboveEdgeLeft
            },
            new PortDescriptor {
              X = 0, Y = 25, EdgeDirection = EdgeDirection.In, LabelText = "in2",
              LabelPlacementWithEdge = belowEdgeLeft
            },
            new PortDescriptor {
              X = 50, Y = 15, EdgeDirection = EdgeDirection.Out, LabelText = "out",
              LabelPlacementWithEdge = aboveEdgeRight
            }
          };
        case LogicGateType.Nand:
          return new List<PortDescriptor> {
            new PortDescriptor { X = 0, Y = 5, EdgeDirection = EdgeDirection.In, LabelText = "in1",
              LabelPlacementWithEdge = aboveEdgeLeft
            },
            new PortDescriptor { X = 0, Y = 25, EdgeDirection = EdgeDirection.In, LabelText = "in2",
              LabelPlacementWithEdge = belowEdgeLeft
            },
            new PortDescriptor { X = 50, Y = 15, EdgeDirection = EdgeDirection.Out, LabelText = "out",
              LabelPlacementWithEdge = aboveEdgeRight
            }
          };
        case LogicGateType.Or:
          return new List<PortDescriptor> {
            new PortDescriptor { X = 0, Y = 5, EdgeDirection = EdgeDirection.In, LabelText = "in1",
              LabelPlacementWithEdge = aboveEdgeLeft
            },
            new PortDescriptor { X = 0, Y = 25, EdgeDirection = EdgeDirection.In, LabelText = "in2",
              LabelPlacementWithEdge = belowEdgeLeft
            },
            new PortDescriptor { X = 50, Y = 15, EdgeDirection = EdgeDirection.Out, LabelText = "out",
              LabelPlacementWithEdge = aboveEdgeRight
            }
          };
        case LogicGateType.Nor:
          return new List<PortDescriptor> {
            new PortDescriptor { X = 0, Y = 5, EdgeDirection = EdgeDirection.In, LabelText = "in1",
              LabelPlacementWithEdge = aboveEdgeLeft
            },
            new PortDescriptor { X = 0, Y = 25, EdgeDirection = EdgeDirection.In, LabelText = "in2",
              LabelPlacementWithEdge = belowEdgeLeft
            },
            new PortDescriptor { X = 50, Y = 15, EdgeDirection = EdgeDirection.Out, LabelText = "out",
              LabelPlacementWithEdge = aboveEdgeRight
            }
          };
        case LogicGateType.Not:
          return new List<PortDescriptor> {
            new PortDescriptor { X = 0, Y = 15, EdgeDirection = EdgeDirection.In, LabelText = "in",
              LabelPlacementWithEdge = aboveEdgeLeft
            },
            new PortDescriptor { X = 50, Y = 15, EdgeDirection = EdgeDirection.Out, LabelText = "out",
              LabelPlacementWithEdge = aboveEdgeRight
            }
          };
        case LogicGateType.Timer:
          return new[] {
            new PortDescriptor { X = 0, Y = 20, EdgeDirection = EdgeDirection.In, LabelText = "gnd",
              LabelPlacementWithEdge = inside
            },
            new PortDescriptor { X = 0, Y = 40, EdgeDirection = EdgeDirection.In, LabelText = "trig",
              LabelPlacementWithEdge = inside
            },
            new PortDescriptor { X = 0, Y = 80, EdgeDirection = EdgeDirection.Out, LabelText = "out",
              LabelPlacementWithEdge = inside
            },
            new PortDescriptor { X = 0, Y = 100, EdgeDirection = EdgeDirection.In, LabelText = "rst",
              LabelPlacementWithEdge = inside
            },
            new PortDescriptor { X = 70, Y = 20, EdgeDirection = EdgeDirection.In, LabelText = "Vcc",
              LabelPlacementWithEdge = inside
            },
            new PortDescriptor { X = 70, Y = 40, EdgeDirection = EdgeDirection.Out, LabelText = "dis",
              LabelPlacementWithEdge = inside
            },
            new PortDescriptor { X = 70, Y = 80, EdgeDirection = EdgeDirection.In, LabelText = "thr",
              LabelPlacementWithEdge = inside
            },
            new PortDescriptor { X = 70, Y = 100, EdgeDirection = EdgeDirection.In, LabelText = "ctrl",
              LabelPlacementWithEdge = inside
            },
          };
        case LogicGateType.ADConverter:
          return new[] {
            new PortDescriptor { X = 0, Y = 20, EdgeDirection = EdgeDirection.In, LabelText = "Vin",
              LabelPlacementWithEdge = inside
            },
            new PortDescriptor { X = 0, Y = 40, EdgeDirection = EdgeDirection.In, LabelText = "gnd",
              LabelPlacementWithEdge = inside
            },
            new PortDescriptor { X = 0, Y = 80, EdgeDirection = EdgeDirection.In, LabelText = "Vref",
              LabelPlacementWithEdge = inside
            },
            new PortDescriptor { X = 0, Y = 100, EdgeDirection = EdgeDirection.In, LabelText = "clk",
              LabelPlacementWithEdge = inside
            },
            new PortDescriptor { X = 70, Y = 20, EdgeDirection = EdgeDirection.Out, LabelText = "d1",
              LabelPlacementWithEdge = inside
            },
            new PortDescriptor { X = 70, Y = 40, EdgeDirection = EdgeDirection.Out, LabelText = "d2",
              LabelPlacementWithEdge = inside
            },
            new PortDescriptor { X = 70, Y = 100, EdgeDirection = EdgeDirection.Out, LabelText = "sign",
              LabelPlacementWithEdge = inside
            },
          };
      }
    }
  }

  /// <summary>
  /// Describes the direction of a port (to allow incoming and outgoing edges)
  /// </summary>
  public enum EdgeDirection
  {
    In,
    Out
  }
}