/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using System.Collections;
using System.Collections.Generic;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.LargeGraphs.Styles.LevelOfDetail
{
  /// <summary>
  ///   Immutable container for maintaining a list of zoom level / style pairs for use in level-of-detail styles.
  /// </summary>
  /// <typeparam name="T">
  ///   Either <see cref="INodeStyle" />, <see cref="IEdgeStyle" />, or <see cref="ILabelStyle" />. This
  ///   type parameter is not constrained as there is no suitable base interface for the aforementioned style interfaces.
  /// </typeparam>
  /// <remarks>
  ///   Styles need to be added to this container in ascending order of their zoom levels. This container cannot be
  ///   empty when it's used; <see cref="GetStyle" /> and <see cref="HasSameStyle" /> require at least one style to work
  ///   correctly.
  /// </remarks>
  /// <example>
  ///   <para>
  ///     This class implements <see cref="IEnumerable{T}" /> and offers an <see cref="Add" /> method, thus it can be
  ///     used with collection initializers:
  ///   </para>
  ///   <code>
  ///     new LevelOfDetailStyleContainer&lt;INodeStyle> {
  ///       { 0, VoidNodeStyle.Instance },
  ///       { 0.2, new ShapeNodeStyle(ShapeNodeShape.Rectangle, null, Brushes.CornflowerBlue) },
  ///       { 1.5, new ShinyPlateNodeStyle(Brushes.CornflowerBlue) }
  ///     }
  ///   </code>
  /// </example>
  public class LevelOfDetailStyleContainer<T> : IEnumerable<T>
  {
    /// <summary>The list of styles.</summary>
    private readonly List<T> styles = new List<T>();

    /// <summary>The list of zoom levels for the styles.</summary>
    private readonly List<double> zoomLevels = new List<double>();

    /// <summary>
    ///   Adds the given zoom level / style pair.
    /// </summary>
    /// <param name="zoomLevel">The zoom level.</param>
    /// <param name="style">The style.</param>
    /// <remarks>Styles need to be added in ascending order of their zoom levels.</remarks>
    public void Add(double zoomLevel, T style) {
      zoomLevels.Add(zoomLevel);
      styles.Add(style);
    }

    /// <summary>
    ///   Gets the style for the given zoom level.
    /// </summary>
    /// <param name="zoomLevel">The zoom level.</param>
    /// <returns>The style for the given zoom level.</returns>
    public T GetStyle(double zoomLevel) {
      return styles[GetIndex(zoomLevel)];
    }

    /// <summary>
    ///   Determines whether two zoom levels would correspond to the same style.
    /// </summary>
    /// <param name="z1">The first zoom level.</param>
    /// <param name="z2">The second zoom level.</param>
    /// <returns>
    ///   <see langword="true" /> if both zoom levels would fall into the same style »bucket«, <see langword="false" />
    ///   otherwise.
    /// </returns>
    public bool HasSameStyle(double z1, double z2) {
      return GetIndex(z1) == GetIndex(z2);
    }

    /// <summary>
    ///   Helper method to get the index in the list of styles or zoom levels corresponding to the given zoom level.
    /// </summary>
    /// <param name="zoomLevel">The zoom level to look up a list index for.</param>
    /// <returns>The list index for the given zoom level.</returns>
    private int GetIndex(double zoomLevel) {
      if (zoomLevels[0] > zoomLevel) {
        return 0;
      }

      for (int i = 1; i < zoomLevels.Count; i++) {
        if (zoomLevels[i] > zoomLevel) {
          return i - 1;
        }
      }

      return styles.Count - 1;
    }

    #region IEnumerable

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() {
      return styles.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() {
      return styles.GetEnumerator();
    }

    #endregion
  }
}