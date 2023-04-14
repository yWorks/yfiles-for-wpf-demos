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

using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using yWorks.Graph;
using INeo4jNode = Neo4j.Driver.INode;


namespace Neo4JIntegration
{
  public partial class ShortestPathDialog
  {
    public List<INode> Nodes { get; set; }
    public IEnumerable<INeo4jNode> NodeItems => Nodes.Select(n => (INeo4jNode) n.Tag);
    public INode From { get; set; }
    public int ToIndex { get; set; }
    public INode To => Nodes[ToIndex];
    public INeo4jNode FromItem => (INeo4jNode) From.Tag;
    public INeo4jNode ToItem => (INeo4jNode) To.Tag;

    public ShortestPathDialog(INode from, List<INode> nodes) {
      From = from;
      Nodes = nodes;

      InitializeComponent();
    }

    private void Button_Click(object sender, System.Windows.RoutedEventArgs e) {
      DialogResult = true;
    }
  }
}
