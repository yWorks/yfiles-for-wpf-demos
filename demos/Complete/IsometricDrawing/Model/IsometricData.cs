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

using System.Windows.Media;

namespace Demo.yFiles.Complete.IsometricDrawing.Model
{
  /// <summary>
  /// Data sets of <see cref="NodeData"/> for group and normal nodes and of <see cref="EdgeData"/> for the edges.
  /// </summary>
  public class IsometricData
  {
    /// <summary>
    /// A data set for normal nodes.
    /// </summary>
    public static readonly NodeData[] NodesData = {
        new NodeData {
            Id = "tablet1",
            Color = Color.FromArgb(255, 255, 153, 0),
            Geometry = new Geometry { Width = 23, Height = 5, Depth = 30 },
            Label = "Tablet",
            Group = "development"
        },
        new NodeData {
            Id = "tablet2",
            Color = Color.FromArgb(255, 255, 153, 0),
            Geometry = new Geometry { Width = 23, Height = 5, Depth = 30 },
            Label = "Tablet",
            Group = "sales"
        },
        new NodeData {
            Id = "tablet3",
            Color = Color.FromArgb(255, 255, 153, 0),
            Geometry = new Geometry { Width = 23, Height = 5, Depth = 30 },
            Label = "Tablet",
            Group = "sales"
        },
        new NodeData {
            Id = "tablet4",
            Color = Color.FromArgb(255, 255, 153, 0),
            Geometry = new Geometry { Width = 23, Height = 5, Depth = 30 },
            Label = "Tablet",
            Group = "it"
        },
        new NodeData {
            Id = "server1",
            Color = Color.FromArgb(255, 153, 51, 255),
            Geometry = new Geometry { Width = 29, Height = 30, Depth = 47 },
            Label = "Server",
            Group = "development"
        },
        new NodeData {
            Id = "server2",
            Color = Color.FromArgb(255, 153, 51, 255),
            Geometry = new Geometry { Width = 29, Height = 30, Depth = 47 },
            Label = "Server",
            Group = "it"
        },
        new NodeData {
            Id = "pc1",
            Color = Color.FromArgb(255, 153, 204, 0),
            Geometry = new Geometry { Width = 15, Height = 30, Depth = 47 },
            Label = "PC",
            Group = "development"
        },
        new NodeData {
            Id = "pc2",
            Color = Color.FromArgb(255, 153, 204, 0),
            Geometry = new Geometry { Width = 15, Height = 30, Depth = 47 },
            Label = "PC",
            Group = "development"
        },
        new NodeData {
            Id = "pc3",
            Color = Color.FromArgb(255, 153, 204, 0),
            Geometry = new Geometry { Width = 15, Height = 30, Depth = 47 },
            Label = "PC",
            Group = "development"
        },
        new NodeData {
            Id = "pc4",
            Color = Color.FromArgb(255, 153, 204, 0),
            Geometry = new Geometry { Width = 15, Height = 30, Depth = 47 },
            Label = "PC",
            Group = "development"
        },
        new NodeData {
            Id = "pc5",
            Color = Color.FromArgb(255, 153, 204, 0),
            Geometry = new Geometry { Width = 15, Height = 30, Depth = 47 },
            Label = "PC",
            Group = "management"
        },
        new NodeData {
            Id = "pc6",
            Color = Color.FromArgb(255, 153, 204, 0),
            Geometry = new Geometry { Width = 15, Height = 30, Depth = 47 },
            Label = "PC",
            Group = "management"
        },
        new NodeData {
            Id = "pc7",
            Color = Color.FromArgb(255, 153, 204, 0),
            Geometry = new Geometry { Width = 15, Height = 30, Depth = 47 },
            Label = "PC",
            Group = "management"
        },
        new NodeData {
            Id = "pc8",
            Color = Color.FromArgb(255, 153, 204, 0),
            Geometry = new Geometry { Width = 15, Height = 30, Depth = 47 },
            Label = "PC",
            Group = "production"
        },
        new NodeData {
            Id = "pc9",
            Color = Color.FromArgb(255, 153, 204, 0),
            Geometry = new Geometry { Width = 15, Height = 30, Depth = 47 },
            Label = "PC",
            Group = "production"
        },
        new NodeData {
            Id = "pc10",
            Color = Color.FromArgb(255, 153, 204, 0),
            Geometry = new Geometry { Width = 15, Height = 30, Depth = 47 },
            Label = "PC",
            Group = "it"
        },
        new NodeData {
            Id = "pc11",
            Color = Color.FromArgb(255, 153, 204, 0),
            Geometry = new Geometry { Width = 15, Height = 30, Depth = 47 },
            Label = "PC",
            Group = "it"
        },
        new NodeData {
            Id = "laptop1",
            Color = Color.FromArgb(255, 0, 204, 255),
            Geometry = new Geometry { Width = 43, Height = 10, Depth = 24 },
            Label = "Laptop",
            Group = "development"
        },
        new NodeData {
            Id = "laptop2",
            Color = Color.FromArgb(255, 0, 204, 255),
            Geometry = new Geometry { Width = 43, Height = 10, Depth = 24 },
            Label = "Laptop",
            Group = "development"
        },
        new NodeData {
            Id = "laptop3",
            Color = Color.FromArgb(255, 0, 204, 255),
            Geometry = new Geometry { Width = 43, Height = 10, Depth = 24 },
            Label = "Laptop",
            Group = "sales"
        },
        new NodeData {
            Id = "laptop4",
            Color = Color.FromArgb(255, 0, 204, 255),
            Geometry = new Geometry { Width = 43, Height = 10, Depth = 24 },
            Label = "Laptop",
            Group = "sales"
        },
        new NodeData {
            Id = "laptop5",
            Color = Color.FromArgb(255, 0, 204, 255),
            Geometry = new Geometry { Width = 43, Height = 10, Depth = 24 },
            Label = "Laptop",
            Group = "it"
        },
        new NodeData {
            Id = "db",
            Color = Color.FromArgb(255, 153, 51, 255),
            Geometry = new Geometry { Width = 20, Height = 30, Depth = 20 },
            Label = "DB",
            Group = "it"
        },
        new NodeData {
            Id = "hub1",
            Color = Color.FromArgb(255, 192, 192, 192),
            Geometry = new Geometry { Width = 38, Height = 7, Depth = 24 },
            Label = "Hub",
            Group = "development"
        },
        new NodeData {
            Id = "hub2",
            Color = Color.FromArgb(255, 192, 192, 192),
            Geometry = new Geometry { Width = 38, Height = 7, Depth = 24 },
            Label = "Hub",
            Group = "management"
        },
        new NodeData {
            Id = "hub3",
            Color = Color.FromArgb(255, 192, 192, 192),
            Geometry = new Geometry { Width = 38, Height = 7, Depth = 24 },
            Label = "Hub",
            Group = "production"
        },
        new NodeData {
            Id = "hub4",
            Color = Color.FromArgb(255, 192, 192, 192),
            Geometry = new Geometry { Width = 38, Height = 7, Depth = 24 },
            Label = "Hub",
            Group = "sales"
        },
        new NodeData {
            Id = "hub5",
            Color = Color.FromArgb(255, 192, 192, 192),
            Geometry = new Geometry { Width = 38, Height = 7, Depth = 24 },
            Label = "Hub",
            Group = "it"
        },
        new NodeData {
            Id = "switch", 
            Color = Color.FromArgb(255, 255, 102, 0), 
            Geometry = new Geometry { Width = 63, Height = 15, Depth = 30 }, 
            Label = "Switch"
        }, 
        new NodeData {
            Id = "gateway", 
            Color = Color.FromArgb(255, 153, 51, 255), 
            Geometry = new Geometry { Width = 29, Height = 30, Depth = 47 }, 
            Label = "Gateway"
        }, 
        new NodeData {
            Id = "firewall", 
            Color = Color.FromArgb(255, 255, 0, 0), 
            Geometry = new Geometry { Width = 57, Height = 30, Depth = 10 }, 
            Label = "Firewall"
        }
    };

    /// <summary>
    /// A data set for edges.
    /// </summary>
    public static readonly EdgeData[] EdgesData = {
        new EdgeData { From = "server1", To = "hub1" }, 
        new EdgeData { From = "pc1", To = "hub1" }, 
        new EdgeData { From = "pc2", To = "hub1" }, 
        new EdgeData { From = "hub1", To = "pc3" }, 
        new EdgeData { From = "hub1", To = "pc4" }, 
        new EdgeData { From = "laptop1", To = "hub1" },
        new EdgeData { From = "laptop2", To = "hub1" }, 
        new EdgeData { From = "tablet1", To = "hub1" }, 
        new EdgeData { From = "hub1", To = "switch", Label = "10 GBytes/s" }, 
        new EdgeData { From = "pc5", To = "hub2" }, 
        new EdgeData { From = "pc6", To = "hub2" }, 
        new EdgeData { From = "pc7", To = "hub2" }, 
        new EdgeData { From = "hub2", To = "switch", Label = "1 GByte/s" }, 
        new EdgeData { From = "pc8", To = "hub3" }, 
        new EdgeData { From = "pc9", To = "hub3" }, 
        new EdgeData { From = "hub3", To = "switch", Label = "1 GByte/s" }, 
        new EdgeData { From = "tablet2", To = "hub4" }, 
        new EdgeData { From = "tablet3", To = "hub4" }, 
        new EdgeData { From = "laptop3", To = "hub4" }, 
        new EdgeData { From = "laptop4", To = "hub4" },
        new EdgeData { From = "hub4", To = "switch", Label = "1 GByte/s" }, 
        new EdgeData { From = "tablet4", To = "hub5" }, 
        new EdgeData { From = "laptop5", To = "hub5" }, 
        new EdgeData { From = "pc10", To = "hub5" }, 
        new EdgeData { From = "hub5", To = "pc11" }, 
        new EdgeData { From = "hub5", To = "switch", Label = "1 GByte/s" }, 
        new EdgeData { From = "server2", To = "switch", Label = "10 GByte/s" }, 
        new EdgeData { From = "db", To = "switch", Label = "10 GByte/s" }, 
        new EdgeData { From = "switch", To = "gateway", Label = "100 MByte/s" }, 
        new EdgeData { From = "gateway", To = "firewall" }
    };

    /// <summary>
    /// A data set for group nodes.
    /// </summary>
    public static readonly NodeData[] GroupsData = {
        new NodeData {
            Id = "development", 
            Label = "Development", 
            Geometry = new Geometry { Width = 10, Height = 0, Depth = 10 }, 
            Color = Color.FromArgb(128, 202, 236, 255), 
            Pen = new Pen(new SolidColorBrush(Color.FromArgb(255, 153, 204, 255)), 1)
        }, 
        new NodeData {
            Id = "management", 
            Label = "Management", 
            Geometry = new Geometry { Width = 10, Height = 0, Depth = 10 }, 
            Color = Color.FromArgb(128, 202, 236, 255), 
            Pen = new Pen(new SolidColorBrush(Color.FromArgb(255, 153, 204, 255)), 1)
        }, 
        new NodeData {
            Id = "production", 
            Label = "Production", 
            Geometry = new Geometry { Width = 10, Height = 0, Depth = 10 }, 
            Color = Color.FromArgb(128, 202, 236, 255), 
            Pen = new Pen(new SolidColorBrush(Color.FromArgb(255, 153, 204, 255)), 1)
        }, 
        new NodeData {
            Id = "sales", 
            Label = "Sales", 
            Geometry = new Geometry { Width = 10, Height = 0, Depth = 10 }, 
            Color = Color.FromArgb(128, 202, 236, 255), 
            Pen = new Pen(new SolidColorBrush(Color.FromArgb(255, 153, 204, 255)), 1)
        }, 
        new NodeData {
            Id = "it", 
            Label = "IT", 
            Geometry = new Geometry { Width = 10, Height = 0, Depth = 10 }, 
            Color = Color.FromArgb(128, 202, 236, 255), 
            Pen = new Pen(new SolidColorBrush(Color.FromArgb(255, 153, 204, 255)), 1)
        }
    };
  }
}
