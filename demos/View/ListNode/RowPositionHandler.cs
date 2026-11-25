/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.1.
 ** Copyright (c) 2000-2026 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Linq;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;

namespace yWorks.Demo.ListNode;

/// <summary>
///   An <see cref="IPositionHandler"/> which lets the user change the order of rows by dragging on them.
///   It uses the port's <see cref="IHandle"/>s to keep the adjacent edges orthogonal.
/// </summary>
public sealed class RowPositionHandler : IPositionHandler
{
  private readonly int index;
  private readonly INode node;
  private int currentIndex = -1;
  private RowMoveInfo[] originalState = [];
  private RowInfo[] originalRows = [];
  private readonly Dictionary<IPort, RowMoveInfo> portHandles = [];

  public RowPositionHandler(INode node, int index) {
    this.node = node;
    this.index = index;
  }

  /// <summary>
  ///   The drag has been canceled: restore the original state and clean up.
  /// </summary>
  public void CancelDrag(IInputModeContext context, PointD originalLocation) {
    var nodeInfo = (NodeInfo) node.Tag!;
    foreach (var moveInfo in originalState) {
      moveInfo.Handle.CancelDrag(context, moveInfo.OriginalHandleLocation);
    }
    for (var i = 0; i < originalRows.Length; i++) {
      nodeInfo.Rows[i] = originalRows[i];
    }
    nodeInfo.DraggingIndex = null;
  }

  /// <summary>
  ///   The drag has finished. Update the ports and clean up.
  /// </summary>
  public void DragFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
    var nodeInfo = (NodeInfo) node.Tag!;
    HandleMove(context, originalLocation, newLocation);

    var nodeLocationY = node.Layout.Y;
    foreach (var moveInfo in originalState) {
      var ports = GetPortsForRow(node, moveInfo.Row);
      foreach (var port in ports) {
        var portLocation = port.GetLocation();
        moveInfo.Handle.DragFinished(context, moveInfo.OriginalHandleLocation, portLocation);
        // Moving ports through their handles can leave them anchored somewhere other than the top
        // left or top right corner of their owner node. Restore those anchors so resizing the node
        // does not move the ports unexpectedly.
        var portInfo = (RowPortInfo) port.Tag!;
        if (portInfo.Incoming) {
          context.GetGraph()!.SetPortLocationParameter(port,
              FreeNodePortLocationModel.Instance.CreateParameter(new PointD(0, 0),
                  new PointD(0, portLocation.Y - nodeLocationY)));
        } else {
          context.GetGraph()!.SetPortLocationParameter(port,
              FreeNodePortLocationModel.Instance.CreateParameter(new PointD(1, 0),
                  new PointD(0, portLocation.Y - nodeLocationY)));
        }
      }
    }
    nodeInfo.DraggingIndex = null;
  }

  /// <summary>
  ///   Called during a drag to update the row and port locations.
  /// </summary>
  public void HandleMove(IInputModeContext context, PointD originalLocation, PointD newLocation) {
    var style = (ListNodeStyle) node.Style;
    var newIndex = style.GetRowIndex(node, new PointD(node.Layout.X + node.Layout.Width * 0.5, newLocation.Y));
    var nodeInfo = (NodeInfo) node.Tag!;
    if (newIndex < 0 || newIndex >= nodeInfo.Rows.Count || newIndex == currentIndex) {
      return;
    }

    var row = nodeInfo.Rows[currentIndex];
    var otherRow = nodeInfo.Rows[newIndex];
    var rowPorts = GetPortsForRow(node, row);
    var otherPorts = GetPortsForRow(node, otherRow);
    nodeInfo.Rows[currentIndex] = otherRow;
    nodeInfo.Rows[newIndex] = row;

    foreach (var port in rowPorts) {
      var moveInfo = portHandles[port];
      moveInfo.Handle.HandleMove(context, moveInfo.OriginalHandleLocation,
          new PointD(moveInfo.OriginalHandleLocation.X, node.Layout.Y + style.GetRowCenterY(newIndex)));
    }
    foreach (var port in otherPorts) {
      var moveInfo = portHandles[port];
      moveInfo.Handle.HandleMove(context, moveInfo.OriginalHandleLocation,
          new PointD(moveInfo.OriginalHandleLocation.X, node.Layout.Y + style.GetRowCenterY(currentIndex)));
    }
    currentIndex = newIndex;
    nodeInfo.DraggingIndex = newIndex;
  }

  /// <summary>
  ///   Called when the drag starts. Store current row and port state and initialize the port handles.
  /// </summary>
  public void InitializeDrag(IInputModeContext context) {
    var nodeInfo = (NodeInfo) node.Tag!;
    currentIndex = index;
    // Store the current index in the node tag so the node style can highlight the row.
    nodeInfo.DraggingIndex = currentIndex;
    originalRows = [.. nodeInfo.Rows];
    var states = new List<RowMoveInfo>();
    portHandles.Clear();
    // Initialize every port because the row being moved may have more than one port.
    foreach (var row in nodeInfo.Rows) {
      foreach (var port in GetPortsForRow(node, row)) {
        var handle = (IHandle) port.Lookup(typeof(IHandle))!;
        var info = new RowMoveInfo(row, port.LocationParameter, handle, port.GetLocation());
        states.Add(info);
        handle.InitializeDrag(context);
        portHandles.Add(port, info);
      }
    }
    originalState = [.. states];
  }

  public IPoint Location => PointD.Origin;

  /// <summary>
  ///   Returns the port for the row identified by the given row information.
  /// </summary>
  /// <param name="node">The node whose is queried for a port.</param>
  /// <param name="row">The row information that identifies the row whose port is returned.</param>
  private static IPort[] GetPortsForRow(INode node, RowInfo row) =>
      [.. node.Ports.Where(port => ReferenceEquals(((RowPortInfo) port.Tag!).Row, row))];

  private sealed record RowMoveInfo(
      RowInfo Row,
      IPortLocationModelParameter Parameter,
      IHandle Handle,
      PointD OriginalHandleLocation);
}
