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

using System.ComponentModel;
using System.Diagnostics;
using Demo.yFiles.Graph.NetworkMonitoring.Simulator;

namespace Demo.yFiles.Graph.NetworkMonitoring.Model
{
  /// <summary>
  /// Kind of a node.
  /// </summary>
  public enum NodeKind
  {
    Workstation = 1,
    Laptop,
    Smartphone,
    Switch,
    Wlan,
    Server,
    Database
  }

  /// <summary>
  /// Class representing a node in the network model.
  /// </summary>
  [DebuggerDisplay("{Name}")]
  public class ModelNode : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    private bool enabled;
    private bool failed;
    private double load;
    private bool labelVisible;

    /// <summary>
    /// Gets or sets a value indicating whether this node is enabled. Disabled nodes are turned off and
    /// cannot send or receive packets.
    /// </summary>
    public bool Enabled {
      get { return enabled; }
      set {
        enabled = value;
        OnPropertyChanged("Enabled");
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this node failed. A failed node has to be repaired before it
    /// can send or receive packets again.
    /// </summary>
    /// <remarks>
    /// The actual result for <see cref="Enabled"/> and <see cref="Failed"/> is essentially the same, just the
    /// interaction and graphical appearance in the demo changes.
    /// </remarks>
    public bool Failed {
      get { return failed; }
      set {
        failed = value;
        OnPropertyChanged("Failed");
      }
    }

    /// <summary>
    /// Gets or sets the name of this node.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the IP address of this node.
    /// </summary>
    public string Ip { get; set; }

    /// <summary>
    /// Gets or sets the load of this node.
    /// </summary>
    /// <remarks>
    /// Load is a value between 0 and 1 that indicates how utilized the node is (with 0 being not at all and 1
    /// being fully). Load also factors into the failure probability of nodes in the
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
    /// Gets or sets the kind of the node.
    /// </summary>
    public NodeKind Kind { get; set; }

    // The following properties are strictly view model properties. They are just in here to simplify the demo.
    // In a real application they wouldn't be in the model.

    /// <summary>
    /// Gets or sets a value indicating whether the node's label is currently visible.
    /// </summary>
    public bool LabelVisible {
      get { return labelVisible; }
      set {
        labelVisible = value;
        OnPropertyChanged("LabelVisible");
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