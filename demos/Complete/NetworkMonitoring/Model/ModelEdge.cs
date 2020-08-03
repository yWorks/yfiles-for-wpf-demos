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

using System.ComponentModel;
using System.Diagnostics;
using Demo.yFiles.Graph.NetworkMonitoring.Simulator;

namespace Demo.yFiles.Graph.NetworkMonitoring.Model
{
  /// <summary>
  /// Class representing an edge in the network model.
  /// </summary>
  [DebuggerDisplay("{Source.Name} ? {Target.Name}")]
  public class ModelEdge : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    private double load;
    private ModelNode source;
    private ModelNode target;
    private bool failed;
    private bool hasForwardPacket;
    private bool hasBackwardPacket;

    /// <summary>
    /// Handles property change events from the attached nodes to update the <see cref="Enabled"/> property
    /// accordingly, which depends on the end nodes being enabled and not broken.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void NodePropertyChangedHandler(object sender, PropertyChangedEventArgs args) {
      var node = sender as ModelNode;
      if (node != null && (args.PropertyName == "Enabled" || args.PropertyName == "Failed")) {
        OnPropertyChanged("Enabled");
      }
    }

    /// <summary>
    /// Gets or sets the source node of this edge.
    /// </summary>
    public ModelNode Source {
      get { return source; }
      set {
        if (source != null) {
          source.PropertyChanged -= NodePropertyChangedHandler;
        }
        source = value;
        source.PropertyChanged += NodePropertyChangedHandler;
      }
    }

    /// <summary>
    /// Gets or sets the target node of this edge.
    /// </summary>
    public ModelNode Target {
      get { return target; }
      set {
        if (target != null) {
          target.PropertyChanged -= NodePropertyChangedHandler;
        }
        target = value;
        target.PropertyChanged += NodePropertyChangedHandler;
      }
    }

    /// <summary>
    /// Gets or sets the load of this edge.
    /// </summary>
    /// <remarks>
    /// Load is a value between 0 and 1 that indicates how utilized the edge is (with 0 being not at all and 1
    /// being fully). Load also factors into the failure probability of edges in the
    /// <see cref="NetworkSimulator"/>.
    /// </remarks>
    public double Load {
      get { return load; }
      set {
        load = value;
        OnPropertyChanged("Load");
      }
    }

    /// <summary>
    /// Gets a value indicating whether this edge is enabled.
    /// </summary>
    /// <remarks>
    /// An edge is enabled if and only if its attached nodes are enabled and have not failed.
    /// </remarks>
    public bool Enabled {
      get { return Source.Enabled && Target.Enabled && !Source.Failed && !Target.Failed; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this edge has failed.
    /// </summary>
    public bool Failed {
      get { return failed; }
      set {
        failed = value;
        OnPropertyChanged("Failed");
      }
    }

    public bool HasForwardPacket {
      get { return hasForwardPacket; }
      set {
        hasForwardPacket = value;
        OnPropertyChanged("HasForwardPacket");
      }
    }

    public bool HasBackwardPacket {
      get { return hasBackwardPacket; }
      set {
        hasBackwardPacket = value;
        OnPropertyChanged("HasBackwardPacket");
      }
    }

    protected virtual void OnPropertyChanged(string propertyName) {
      var handler = PropertyChanged;
      if (handler != null) {
        handler(this, new PropertyChangedEventArgs(propertyName));
      }
    }
  }
}