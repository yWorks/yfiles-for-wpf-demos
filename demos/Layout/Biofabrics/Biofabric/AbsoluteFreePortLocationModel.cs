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
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.PortLocationModels;
using yWorks.Utils;

namespace yWorks.Demo.Biofabrics
{
  /// <summary>
  ///   A custom port location model that allows ports to be placed at absolute positions.
  ///   Unlike relative models, this places ports at fixed coordinates regardless of node transformations.
  /// </summary>
  public sealed class AbsoluteFreePortLocationModel : IPortLocationModel
  {
    /// <summary>
    ///   Creates a parameter for this port location model at the specified location.
    /// </summary>
    /// <param name="owner">The port owner, typically a node.</param>
    /// <param name="location">The absolute position for the port.</param>
    /// <returns>A parameter describing the port's location.</returns>
    public IPortLocationModelParameter CreateParameter(IPortOwner owner, PointD location) =>
        new AbsoluteFreePortLocationModelParameter(this, location);

    /// <summary>Creates a default parameter at the origin.</summary>
    /// <returns>A parameter whose absolute port location is (0, 0).</returns>
    public IPortLocationModelParameter CreateDefaultParameter() =>
        new AbsoluteFreePortLocationModelParameter(this, PointD.Origin);

    /// <summary>
    ///   Returns the lookup context for this port location model.
    /// </summary>
    /// <param name="port">The port to get the context for.</param>
    /// <returns>An empty lookup context.</returns>
    public ILookup GetContext(IPort port) => Lookups.Empty;

    /// <summary>
    ///   Gets the absolute location for the given port based on its parameter.
    /// </summary>
    /// <param name="port">The port to get the location for.</param>
    /// <param name="locationParameter">The parameter describing the port's location.</param>
    /// <returns>The absolute point where the port should be located.</returns>
    /// <exception cref="ArgumentException">The parameter is not of the expected type.</exception>
    public PointD GetLocation(IPort port, IPortLocationModelParameter locationParameter) =>
        locationParameter is AbsoluteFreePortLocationModelParameter parameter
            ? parameter.Point
            : throw new ArgumentException("Not Supported", nameof(locationParameter));

    /// <summary>
    ///   Parameter class storing the absolute position for a port.
    /// </summary>
    private sealed class AbsoluteFreePortLocationModelParameter : IPortLocationModelParameter, ICloneable
    {
      /// <summary>Creates a location parameter for the specified model and point.</summary>
      /// <param name="model">The port location model that created this parameter.</param>
      /// <param name="point">The absolute graph-coordinate location.</param>
      public AbsoluteFreePortLocationModelParameter(AbsoluteFreePortLocationModel model, PointD point) {
        Model = model;
        Point = point;
      }

      /// <summary>Gets the port location model associated with this parameter.</summary>
      public IPortLocationModel Model { get; }

      /// <summary>Gets the absolute graph-coordinate location represented by this parameter.</summary>
      public PointD Point { get; }

      /// <summary>
      ///   Returns this parameter, as this is immutable.
      /// </summary>
      public object Clone() => this;
    }
  }
}
