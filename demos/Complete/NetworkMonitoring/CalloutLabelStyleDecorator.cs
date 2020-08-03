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

using System.Windows;
using System.Windows.Media;
using Demo.yFiles.Graph.NetworkMonitoring.Model;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.NetworkMonitoring
{
  /// <summary>
  /// Wraps a label style and renders a speech bubble-like appearance originating from the node.
  /// </summary>
  public class CalloutLabelStyleDecorator : LabelStyleBase<VisualGroup>
  {
    /// <summary>
    /// Gets or sets the wrapped style instance.
    /// </summary>
    public ILabelStyle WrappedStyle { get; set; }

    protected override VisualGroup CreateVisual(IRenderContext context, ILabel label) {
      var modelNode = (ModelNode) label.Tag;

      if (!modelNode.LabelVisible) {
        return null;
      }

      var container = new VisualGroup();
      var wrappedVisual = WrappedStyle.Renderer.GetVisualCreator(label, WrappedStyle).CreateVisual(context);

      container.Add(UpdateDecorationVisual(label, null));
      container.Add(wrappedVisual);
      return container;
    }

    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup oldVisual, ILabel label) {
      var modelNode = (ModelNode) label.Tag;

      if (!modelNode.LabelVisible) {
        return null;
      }

      // Nothing changed in visibility, but visuals still need to be updated.
      // Maybe because something changed that caused a re-render.

      // Update the wrapped visual
      var wrappedVisual = oldVisual.Children[1];
      var updatedVisual = WrappedStyle.Renderer.GetVisualCreator(label, WrappedStyle)
                                      .UpdateVisual(context, wrappedVisual);

      if (!ReferenceEquals(wrappedVisual, updatedVisual)) {
        oldVisual.Children[1] = updatedVisual;
      }

      var oldPath = oldVisual.Children[0];
      var updatedPath = UpdateDecorationVisual(label, oldPath);

      if (!ReferenceEquals(oldPath, updatedPath)) {
        oldVisual.Children[0] = updatedPath;
      }

      return oldVisual;
    }

    protected override SizeD GetPreferredSize(ILabel label) {
      return WrappedStyle.Renderer.GetPreferredSize(label, WrappedStyle);
    }

    /// <summary>
    /// Generates a path for the callout decoration, which is an isosceles triangle from the label's center pointing
    /// to the node.
    /// </summary>
    /// <param name="label">The label for which to create the path.</param>
    /// <returns>A path for the callout triangle.</returns>
    private static GeneralPath GenerateTrianglePath(ILabel label) {
      var node = (INode) label.Owner;
      var layout = label.GetLayout();

      // Find the point where the triangle attaches to the outline shape
      var outlineShape = node.Style.Renderer.GetShapeGeometry(node, node.Style).GetOutline();
      var nodeCenter = node.Layout.GetCenter();
      var labelCenter = layout.GetCenter();
      if (outlineShape == null) {
        outlineShape = new GeneralPath(4);
        outlineShape.AppendOrientedRectangle(layout, false);
      }
      var intersection = outlineShape.FindLineIntersection(nodeCenter, labelCenter);
      var intersectionPoint = nodeCenter + intersection * (labelCenter - nodeCenter);

      // Find the vector that is orthogonal to the line connecting the label and node centers
      var triangleVector = labelCenter - intersectionPoint;
      var orthoVector = OrthoNormal(triangleVector);

      // Construct the other points of the triangle
      var point2 = labelCenter + orthoVector * 20;
      var point3 = labelCenter - orthoVector * 20;

      // Create the path
      var p = new GeneralPath();
      p.MoveTo(intersectionPoint);
      p.LineTo(point2);
      p.LineTo(point3);
      p.Close();

      return p;
    }

    /// <summary>
    /// Returns the orthonormal vector of the given vector.
    /// </summary>
    private static PointD OrthoNormal(PointD vector) {
      var length = vector.VectorLength;
      return new PointD(-vector.Y / length, vector.X / length);
    }

    /// <summary>
    /// Generates or updates the visual for the callout decoration.
    /// </summary>
    /// <param name="label">The label for the callout decoration.</param>
    /// <param name="oldPath">The old decoration visual, or <see langword="null"/> if there is none.</param>
    /// <returns>Either the old visual if no changes are necessary or a new one.</returns>
    private static Visual UpdateDecorationVisual(ILabel label, Visual oldPath) {
      var newGeneralPath = GenerateTrianglePath(label);

      if (oldPath != null) {
        var oldGeneralPath = oldPath.GetRenderDataCache<GeneralPath>();
        if (oldGeneralPath == newGeneralPath) {
          return oldPath;
        }
      }

      var background = (Brush) Application.Current.Resources["LabelBackground"];

      var newPath = newGeneralPath.CreatePath(background, null, null, FillMode.FillClosedFigures);
      newPath.SetRenderDataCache(newGeneralPath);
      return newPath;
    }
  }
}