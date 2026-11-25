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

using System;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace yWorks.Demo.CustomSnapping
{
  /// <summary>
  ///   Wraps a given <see cref="ISnapReferenceProvider"/> and adds additional <see cref="OrthogonalSnapLine"/>s
  ///   for orthogonal labels of an <see cref="IModelItem"/>. For each orthogonal label there are
  ///   <see cref="OrthogonalSnapLine"/>s added for its top, bottom, left, and right side.
  /// </summary>
  internal sealed class OrthogonalLabelSnapLineProviderWrapper : ISnapReferenceProvider
  {
    private readonly ISnapReferenceProvider wrapped;
    private readonly IModelItem item;

    /// <summary>
    ///   Creates a new instance that wraps the given snap line provider.
    /// </summary>
    /// <param name="item">The owner of the label</param>
    /// <param name="wrapped">The snap line provider that will be wrapped.</param>
    public OrthogonalLabelSnapLineProviderWrapper(IModelItem item, ISnapReferenceProvider wrapped) {
      this.item = item;
      this.wrapped = wrapped;
    }

    /// <summary>
    ///   Calls <see cref="ISnapReferenceProvider.AddSnapReferences"/> of the wrapped provider and adds custom
    ///   <see cref="OrthogonalSnapLine"/>s for the given item.
    /// </summary>
    /// <param name="context">The context which holds the settings for the snap lines.</param>
    /// <param name="args">The argument to use for adding snap lines.</param>
    public void AddSnapReferences(GraphSnapContext context, CollectSnapReferencesEventArgs args) {
      wrapped.AddSnapReferences(context, args);

      if (item is ILabelOwner labelOwner) {
        AddCustomSnapLines(args, labelOwner);
      }
    }

    /// <summary>
    ///   Adds custom snap lines for orthogonal labels
    /// </summary>
    /// <param name="args">The argument to use for adding snap lines.</param>
    /// <param name="labeledItem">The item to add snap lines for.</param>
    private static void AddCustomSnapLines(CollectSnapReferencesEventArgs args, ILabelOwner labeledItem) {
      foreach (var label in labeledItem.Labels) {
        var labelLayout = label.GetLayout();
        // round UpX to its first 6 digits
        var upX = Math.Round(labelLayout.UpX * Math.Pow(10, 6)) / Math.Pow(10, 6);
        // check if it's orthogonal
        if (upX == 0 || upX == 1 || upX == -1) {
          // label is orthogonal
          var bounds = labelLayout.GetBounds();

          // add snap lines to the top, bottom, left and right border of the label
          //
          // snap line for the label's top border
          var topCenter = bounds.GetTopLeft() + new PointD(labelLayout.Width / 2, 0);
          var snapLine = new OrthogonalSnapLine(SnapLineOrientation.Horizontal, SnapLineSnapTypes.Bottom,
              SnapReferenceVisualizationType.FixedLine, topCenter, bounds.X - 10, bounds.GetMaxX() + 10, true, 100);
          args.AddSnapReference(snapLine);

          // snap line for the label's bottom border
          var bottomCenter = bounds.GetBottomLeft() + new PointD(labelLayout.Width / 2, 0);
          snapLine = new OrthogonalSnapLine(SnapLineOrientation.Horizontal, SnapLineSnapTypes.Top,
              SnapReferenceVisualizationType.FixedLine, bottomCenter, bounds.X - 10, bounds.GetMaxX() + 10, true, 100);
          args.AddSnapReference(snapLine);

          // snap line for the label's left border
          var leftCenter = bounds.GetTopLeft() + new PointD(0, labelLayout.Height / 2);
          snapLine = new OrthogonalSnapLine(SnapLineOrientation.Vertical, SnapLineSnapTypes.Right,
              SnapReferenceVisualizationType.FixedLine, leftCenter, bounds.Y - 10, bounds.GetMaxY() + 10, true, 100);
          args.AddSnapReference(snapLine);

          // snap line for the label's right border
          var rightCenter = bounds.GetTopRight() + new PointD(0, labelLayout.Height / 2);
          snapLine = new OrthogonalSnapLine(SnapLineOrientation.Vertical, SnapLineSnapTypes.Left,
              SnapReferenceVisualizationType.FixedLine, rightCenter, bounds.Y - 10, bounds.GetMaxY() + 10, true, 100);
          args.AddSnapReference(snapLine);
        }
      }
    }
  }
}
