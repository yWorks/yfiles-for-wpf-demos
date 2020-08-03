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
using System.Windows.Media;
using System.Windows.Shapes;
using yWorks.Controls.Input;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Layout;

namespace Demo.yFiles.Layout.IncrementalHierarchicLayout
{
  /// <summary>
  /// Helper class that provides a handle for the first and last bend of an edge
  /// that interactively determines the port constraint.
  /// </summary>
  public class PortConstraintBendHandle : ConstrainedHandle, IVisualCreator
  {
    // The minimum distance to require for a port constraint
    private const int MinDistance = 12;
    private readonly bool sourceEnd;
    private readonly IBend bend;
    private readonly WeakDictionaryMapper<IEdge, PortConstraint> portConstraints;
    private ICanvasObject canvasObject;

    public PortConstraintBendHandle(bool sourceEnd, IBend bend, IHandle originalImplementation, WeakDictionaryMapper<IEdge, PortConstraint> portConstraints) : base(originalImplementation) {
      this.sourceEnd = sourceEnd;
      this.bend = bend;
      this.portConstraints = portConstraints;
    }

    protected override void OnInitialized(IInputModeContext context, PointD originalLocation) {
      base.OnInitialized(context, originalLocation);
      // render the indicator
      canvasObject = context.CanvasControl.RootGroup.AddChild(this);
    }

    protected override void OnCanceled(IInputModeContext context, PointD originalLocation) {
      base.OnCanceled(context, originalLocation);
      // remove the indicator
      canvasObject.Remove();
    }

    protected override void OnFinished(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      base.OnFinished(context, originalLocation, newLocation);
      // remove the indicator
      canvasObject.Remove();

      // calculate the direction
      IPort port = sourceEnd ? bend.Owner.SourcePort : bend.Owner.TargetPort;
      var nodeLayout = ((INode)port.Owner).Layout.ToRectD();
      PointD portLocation = nodeLayout.Center;
      PointD bendLocation = bend.Location.ToPointD();
      PointD delta = bendLocation - portLocation;
      PortConstraint pc = null;
      if (delta.VectorLength > MinDistance && !nodeLayout.Contains(bendLocation)) {
        PointD direction = delta.Normalized;
        if (direction.IsHorizontalVector) {
          if (direction.X > 0) {
            pc = PortConstraint.Create(PortSide.East);
          } else {
            pc = PortConstraint.Create(PortSide.West);
          }
        } else {
          if (direction.Y > 0) {
            pc = PortConstraint.Create(PortSide.South);
          } else {
            pc = PortConstraint.Create(PortSide.North);
          }
        }
      }

      // and set the port constraint
      if (pc == null) {
        portConstraints.RemoveValue(bend.Owner);
      } else {
        portConstraints[bend.Owner] = pc;
      }
    }

    protected override PointD ConstrainNewLocation(IInputModeContext context, PointD originalLocation, PointD newLocation) {
      return newLocation;
    }

    /// <summary>
    /// Creates a visual representation of the constraint indicator.
    /// </summary>
    public GeneralPath CreateConstraintIndicator() {
      IPort port = sourceEnd ? bend.Owner.SourcePort : bend.Owner.TargetPort;
      var nodeLayout = ((INode) port.Owner).Layout.ToRectD();

      PointD portLocation = nodeLayout.Center;
      double plX = portLocation.X, plY = portLocation.Y;
      PointD bendLocation = bend.Location.ToPointD();
      PointD delta = bendLocation - portLocation;
      if (delta.VectorLength > MinDistance && !nodeLayout.Contains(bendLocation)) {
        PointD direction = delta.Normalized;

        GeneralPath path = new GeneralPath(20);

        path.MoveTo(-15, 0);
        path.LineTo(-5, 10);
        path.LineTo(-2, 7);
        path.LineTo(-5, 4);
        path.LineTo(8, 4);
        path.LineTo(8, -4);
        path.LineTo(-5, -4);
        path.LineTo(-2, -7);
        path.LineTo(-5, -10);
        path.Close();

        // mirror at target end
        if (!sourceEnd) {
          path.Transform(new Matrix2D(-1, 0, 0, 1, 0, 0));
        }

        // rotate and translate arrow 
        const int ArrowOffset = 11;
        if (direction.IsHorizontalVector) {
          plY = nodeLayout.CenterY;
          if (direction.X > 0) {
            plX = nodeLayout.MaxX + ArrowOffset;
            path.Transform(new Matrix2D(-1, 0, 0, 1, plX, plY));
          } else {
            plX = nodeLayout.X - ArrowOffset;
            path.Transform(new Matrix2D(1, 0, 0, 1, plX, plY));
          }
        } else {
          plX = nodeLayout.CenterX;
          if (direction.Y < 0) {
            plY = nodeLayout.Y - ArrowOffset;
            path.Transform(new Matrix2D(0, 1, 1, 0, plX, plY));
          } else {
            plY = nodeLayout.MaxY + ArrowOffset;
            path.Transform(new Matrix2D(0, 1, -1, 0, plX, plY));
          }
        }
        return path;
      }
      return null;
    }

    public Visual CreateVisual(IRenderContext context) {
      var indicator = CreateConstraintIndicator();
      if (indicator != null) {
        var path = indicator.CreatePath(Brushes.Green, Pens.Black, new Matrix2D(), FillMode.FillClosedFigures);
        path.SetRenderDataCache(indicator.Clone());
        return path;
      } else {
        return null;
      }
    }

    public Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
      var indicator = CreateConstraintIndicator();
      var oldIndicator = oldVisual is Path ? oldVisual.GetRenderDataCache<GeneralPath>() : null;
      // use an already created constraint indicator
      if (indicator != null && oldIndicator != null && indicator.IsEquivalentTo(oldIndicator)) {
        return oldVisual;
      }
      return CreateVisual(context);
    }
  }
}
