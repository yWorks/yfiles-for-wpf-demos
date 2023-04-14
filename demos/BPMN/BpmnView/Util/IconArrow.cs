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

using System.Windows;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.Bpmn.Util
{
  /// <summary>
  /// An <see cref="IArrow"/> implementation using an <see cref="IIcon"/> for the visualization.
  /// </summary>
  internal class IconArrow : IArrow, IVisualCreator, IBoundsProvider
  {
    // these variables hold the state for the flyweight pattern
    // they are populated in GetPaintable and used in the implementations of the IVisualCreator interface.
    private PointD anchor;
    private PointD direction;
    private readonly IIcon icon;

    #region Constructor

    public IconArrow(IIcon icon) {
      this.icon = icon;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Returns the length of the arrow, i.e. the distance from the arrow's tip to
    /// the position where the visual representation of the edge's path should begin.
    /// </summary>
    public double Length { get; set; }

    /// <summary>
    /// Gets the cropping length associated with this instance.
    /// </summary>
    /// <remarks>
    /// This value is used by <see cref="IEdgeStyle"/>s to let the
    /// edge appear to end shortly before its actual target.
    /// </remarks>
    public double CropLength { get; set; }

    /// <summary>
    /// Gets or sets the bounds of the arrow icon.
    /// </summary>
    public SizeD Bounds { get; set; }

    #endregion

    #region IArrow Members

    /// <summary>
    /// Gets an <see cref="IVisualCreator"/> implementation that will create
    /// the <see cref="FrameworkElement"/> for this arrow
    /// at the given location using the given direction for the given edge.
    /// </summary>
    /// <param name="edge">the edge this arrow belongs to</param>
    /// <param name="atSource">whether this will be the source arrow</param>
    /// <param name="anchor">the anchor point for the tip of the arrow</param>
    /// <param name="direction">the direction the arrow is pointing in</param>
    /// <returns>
    /// Itself as a flyweight.
    /// </returns>
    public IVisualCreator GetVisualCreator(IEdge edge, bool atSource, PointD anchor, PointD direction) {
      this.anchor = anchor;
      this.direction = direction;
      return this;
    }

    /// <summary>
    /// Gets an <see cref="IBoundsProvider"/> implementation that can yield
    /// this arrow's bounds if painted at the given location using the
    /// given direction for the given edge.
    /// </summary>
    /// <param name="edge">the edge this arrow belongs to</param>
    /// <param name="atSource">whether this will be the source arrow</param>
    /// <param name="anchor">the anchor point for the tip of the arrow</param>
    /// <param name="directionVector">the direction the arrow is pointing in</param>
    /// <returns>
    /// an implementation of the <see cref="IBoundsProvider"/> interface that can
    /// subsequently be used to query the bounds. Clients will always call
    /// this method before using the implementation and may not cache the instance returned.
    /// This allows for applying the flyweight design pattern to implementations.
    /// </returns>
    public IBoundsProvider GetBoundsProvider(IEdge edge, bool atSource, PointD anchor, PointD directionVector) {
      this.anchor = anchor;
      this.direction = directionVector;
      return this;
    }

    #endregion

    #region Rendering

    /// <summary>
    /// This method is called by the framework to create a <see cref="FrameworkElement"/>
    /// that will be included into the <see cref="IRenderContext"/>.
    /// </summary>
    /// <param name="context">The context that describes where the visual will be used.</param>
    /// <returns>
    /// The arrow visual to include in the canvas object visual tree./>.
    /// </returns>
    /// <seealso cref="UpdateVisual"/>
    Visual IVisualCreator.CreateVisual(IRenderContext context) {
      icon.SetBounds(new RectD(-Bounds.Width, -Bounds.Height/2, Bounds.Width, Bounds.Height));
      var visualGroup = new VisualGroup();
      visualGroup.Add(icon.CreateVisual(context));

      // Rotate arrow and move it to correct position
      visualGroup.Transform = new MatrixTransform
      {
        Matrix = (Matrix) new Matrix2D(direction.X, -direction.Y, direction.Y, direction.X, anchor.X, anchor.Y)
      };

      return visualGroup;
    }


    /// <summary>
    /// This method updates or replaces a previously created <see cref="FrameworkElement"/> for inclusion
    /// in the <see cref="IRenderContext"/>.
    /// </summary>
    /// <param name="context">The context that describes where the visual will be used in.</param>
    /// <param name="oldVisual">The visual instance that had been returned the last time the <see cref="IVisualCreator.CreateVisual"/>
    /// method was called on this instance.</param>
    /// <returns>
    /// 	<paramref name="oldVisual"/>, if this instance modified the visual, or a new visual that should replace the
    /// existing one in the canvas object visual tree.
    /// </returns>
    /// <remarks>
    /// The <see cref="CanvasControl"/> uses this method to give implementations a chance to
    /// update an existing Visual that has previously been created by the same instance during a call
    /// to <see cref="IVisualCreator.CreateVisual"/>. Implementation may update the <paramref name="oldVisual"/>
    /// and return that same reference, or create a new visual and return the new instance or <see langword="null"/>.
    /// </remarks>
    /// <seealso cref="IVisualCreator.CreateVisual"/>
    /// <seealso cref="ICanvasObjectDescriptor"/>
    /// <seealso cref="CanvasControl"/>
    public Visual UpdateVisual(IRenderContext context, Visual oldVisual) {
      VisualGroup p = oldVisual as VisualGroup;
      if (p != null) {
        ((MatrixTransform) p.Transform).Matrix = (Matrix) new Matrix2D(direction.X, -direction.Y, +direction.Y, direction.X,
          anchor.X, anchor.Y);
        return p;
      }
      return ((IVisualCreator) this).CreateVisual(context);
    }

    #endregion

    #region Rendering Helper Methods

    /// <summary>
    /// Returns the bounds of the arrow for the current flyweight configuration.
    /// </summary>
    RectD IBoundsProvider.GetBounds(ICanvasContext context) {
      return new RectD(anchor.X - Bounds.Width, anchor.Y - Bounds.Height/2, Bounds.Width, Bounds.Height);
    }

    #endregion
  }
}
