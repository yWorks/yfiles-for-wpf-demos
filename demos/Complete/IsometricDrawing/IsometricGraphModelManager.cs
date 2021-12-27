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
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Complete.IsometricDrawing
{
  /// <summary>
  /// A <see cref="GraphModelManager"/> that uses a sensible render order for its nodes in an isometric view.
  /// </summary>
  /// <remarks>
  /// The manager ensures that the 3D-nodes overlap other nodes that are farther away.
  /// </remarks>
  public class IsometricGraphModelManager : GraphModelManager
  {
    private readonly IsometricComparer comparer;

    public IsometricGraphModelManager(CanvasControl canvasControl) : base(canvasControl, canvasControl.ContentGroup) {
      comparer = new IsometricComparer(canvasControl);
      // The comparer needs the user object (=node) to be set on the main canvas object
      ProvideUserObjectOnMainCanvasObject = true;
    }

    protected override ItemModelManager<INode> CreateNodeModelManager(ICanvasObjectDescriptor descriptor, Func<INode, ICanvasObjectGroup> callback) {
      var nodeModelManager = base.CreateNodeModelManager(descriptor, callback);
      nodeModelManager.Comparer = comparer;
      return nodeModelManager;
    }

    /// <summary>
    /// Called when the projection changes and the node overlaps can change as a result.
    /// </summary>
    public void Update() {
      comparer.Update();
      foreach (var node in Graph.Nodes) {
        Update(node);
      }
    }
    
    /// <summary>
    /// An <see cref="IComparer{T}"/> for nodes that determines their render order in an isometric view.
    /// </summary>
    /// <remarks>
    /// This heuristic assumes that all nodes have a cubical form and no two nodes have overlapping <see cref="INode.Layout"/>. 
    /// </remarks>
    sealed class IsometricComparer : IComparer<INode>
    {
      private readonly CanvasControl control;
      private Transform projection;

      private bool leftFaceVisible;
      private bool backFaceVisible;

      public IsometricComparer(CanvasControl control) {
        this.control = control;
        Update();
      }

      /// <summary>
      /// Updates which faces are visible and therefore which corners should be used for the z-order comparison.
      /// </summary>
      /// <remarks>
      /// This method has to be called when the <see cref="CanvasControl.Projection"/> has changed.
      /// </remarks>
      internal void Update() {
        if (control.Projection != projection) {
          projection = control.Projection;
          var upVector = IsometricNodeStyle.CalculateHeightVector(projection);
          leftFaceVisible = upVector.X > 0; 
          backFaceVisible = upVector.Y > 0; 
        }
      }

      public int Compare(INode x, INode y) {
        var projection = control.Projection;
        var xViewCenter = PointD.Origin;
        var yViewCenter = PointD.Origin;
        var xViewRight = PointD.Origin;
        var yViewRight = PointD.Origin;
        var xViewLeft = PointD.Origin;
        var yViewLeft = PointD.Origin;
        if (leftFaceVisible && backFaceVisible) {
          xViewCenter = x.Layout.GetTopLeft();
          yViewCenter = y.Layout.GetTopLeft();
          xViewRight = x.Layout.GetBottomLeft();
          yViewRight = y.Layout.GetBottomLeft();
          xViewLeft = x.Layout.GetTopRight();
          yViewLeft = y.Layout.GetTopRight();
        } else if (!leftFaceVisible && backFaceVisible) {
          xViewCenter = x.Layout.GetTopRight();
          yViewCenter = y.Layout.GetTopRight();
          xViewRight = x.Layout.GetTopLeft();
          yViewRight = y.Layout.GetTopLeft();
          xViewLeft = x.Layout.GetBottomRight();
          yViewLeft = y.Layout.GetBottomRight();
        } else if (!leftFaceVisible && !backFaceVisible) {
          xViewCenter = x.Layout.GetBottomRight();
          yViewCenter = y.Layout.GetBottomRight();
          xViewRight = x.Layout.GetTopRight();
          yViewRight = y.Layout.GetTopRight();
          xViewLeft = x.Layout.GetBottomLeft();
          yViewLeft = y.Layout.GetBottomLeft();
        } else if (leftFaceVisible && !backFaceVisible) {
          xViewCenter = x.Layout.GetBottomLeft();
          yViewCenter = y.Layout.GetBottomLeft();
          xViewRight = x.Layout.GetBottomRight();
          yViewRight = y.Layout.GetBottomRight();
          xViewLeft = x.Layout.GetTopLeft();
          yViewLeft = y.Layout.GetTopLeft();
        }

        var sgnX = leftFaceVisible ? -1 : 1;
        var sgnY = backFaceVisible ? -1 : 1;

        var dViewCenter = ((PointD)projection.Transform(yViewCenter)) - ((PointD)projection.Transform(xViewCenter));
        // determine order in two steps:
        // 1) compare view coordinates of ViewCenter values to determine which node corners to compare in step 2
        // 2) compare the world coordinates of the corners found in step 1 considering which faces are visible
        if (dViewCenter.X < 0 && dViewCenter.Y < 0) {
          var vector = yViewRight - xViewLeft;
          if (vector.X * sgnX > 0 && vector.Y * sgnY > 0) {
            return -1;
          } else {
            return 1;
          }
        } else if (dViewCenter.X > 0 && dViewCenter.Y > 0) {
          var vector = yViewLeft - xViewRight;
          if (vector.X * sgnX < 0 && vector.Y * sgnY < 0) {
            return 1;
          } else {
            return -1;
          }
        } else if (dViewCenter.X > 0) {
          var vector = yViewCenter - xViewRight;
          if (vector.X * sgnX > 0 && vector.Y * sgnY > 0) {
            return -1;
          } else {
            return 1;
          }
        } else {
          var vector = yViewRight - xViewCenter;
          if (vector.X * sgnX < 0 && vector.Y * sgnY < 0) {
            return 1;
          } else {
            return -1;
          }
        }
      }
    }
  }
}
