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

using System.Xml;
using System.Xml.Linq;
using yWorks.Graph;
using yWorks.GraphML;

namespace Demo.yFiles.Graph.ZOrder
{
  /// <summary>
  /// A <see cref="GraphMLIOHandler"/> that supports writing and parsing z-orders for nodes.
  /// </summary>
  public class ZOrderGraphMLIOHandler : GraphMLIOHandler
  {
    // flag indicating if the z-order key definition was included in a parsed GraphML file
    private bool ZOrderKeyDefinitionFound;

    public ZOrderGraphMLIOHandler() {
      this.Parsing += (sender, args) => {
        var zOrderSupport = args.Context.Graph.Lookup<ZOrderSupport>();
        if (zOrderSupport != null) {
          // clear old z-orders of old graph
          zOrderSupport.Clear();
          // disable automatic z-order creation for new nodes
          zOrderSupport.AddZOrderForNewNodes = false;
          ZOrderKeyDefinitionFound = false;
        }
      };
      this.Parsed += (sender, args) => {
        var zOrderSupport = args.Context.Graph.Lookup<ZOrderSupport>();
        if (zOrderSupport != null) {
          // enable automatic z-order creation for new nodes again
          zOrderSupport.AddZOrderForNewNodes = true;
          if (!ZOrderKeyDefinitionFound) {
            // no z-orders were stored in the GraphML so initialize the nodes in the view
            zOrderSupport.SetTempNormalizedZOrders();
            zOrderSupport.ApplyTempZOrders();
          }
        }
      };
    }

    protected override void ConfigureOutputHandlers(IGraph graph, GraphMLWriter writer) {
      base.ConfigureOutputHandlers(graph, writer);
      writer.QueryOutputHandlers += RegisterZOrderOutputHandler;
    }

    /// <summary>
    /// Predefined output handler that writes z-orders.
    /// </summary>
    /// <remarks>This handler is by default registered for the <see cref="GraphMLWriter.QueryOutputHandlers"/> event</remarks>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    internal virtual void RegisterZOrderOutputHandler(object sender, QueryOutputHandlersEventArgs e) {
      if (e.Scope == KeyScope.Node) {
        e.AddOutputHandler(new ZOrderOutputHandler());
      }
    }

    protected override void ConfigureInputHandlers(GraphMLParser parser) {
      base.ConfigureInputHandlers(parser);
      parser.QueryInputHandlers += RegisterZOrderInputHandler;
    }

    /// <summary>
    /// Predefined input handler that reads z-orders. 
    /// </summary>
    /// <remarks>This handler is by default registered for the <see cref="GraphMLParser.QueryInputHandlers"/> event</remarks>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    internal virtual void RegisterZOrderInputHandler(object sender, QueryInputHandlersEventArgs e) {
      if (!e.Handled && MatchesScope(e.KeyDefinition, KeyScope.Node) && MatchesName(e.KeyDefinition, ZOrderOutputHandler.ZOrderKeyName)) {
        ZOrderKeyDefinitionFound = true;
        e.AddInputHandler(new ZOrderInputHandler());
        e.Handled = true;
      }
    }
  }

  /// <summary>
  /// An <see cref="IOutputHandler"/> that writes the z-order of nodes, edges and ports.
  /// </summary>
  class ZOrderOutputHandler : OutputHandlerBase<INode, int>
  {
    public const string ZOrderKeyName = "zOrder";
    
    /// <summary>
    /// The namespace URI for z-order extensions to GraphML.
    /// </summary>
    /// <remarks>This field has the constant value <c>http://www.yworks.com/xml/yfiles-z-order/1.0</c></remarks>
    public const string ZOrderNS = "http://www.yworks.com/xml/yfiles-z-order/1.0";
    
    public ZOrderOutputHandler() : base(KeyScope.Node, ZOrderKeyName, KeyType.Int) {
      DefaultValue = 0;
      WriteKeyDefault = false;
      Precedence = WritePrecedence.BeforeChildren;
      SetKeyDefinitionUri(ZOrderNS + "/" + ZOrderKeyName);
    }

    protected override void WriteValueCore(IWriteContext context, int data) {
      context.Writer.WriteString(XmlConvert.ToString(data));
    }

    protected override int GetValue(IWriteContext context, INode key) {
      var zOrderSupport = context.Graph.Lookup<ZOrderSupport>();
      if (zOrderSupport != null) {
        return zOrderSupport.GetZOrder(key);
      }
      return 0;
    }
  }

  /// <summary>
  /// An <see cref="IInputHandler"/> that reads the z-order of nodes, edges and ports.
  /// </summary>
  class ZOrderInputHandler : InputHandlerBase<INode, int>
  {
    protected override int ParseDataCore(IParseContext context, XObject node) {
      var zOrder = XmlConvert.ToInt32(((XElement) node).Value);
      return zOrder;
    }

    protected override void SetValue(IParseContext context, INode key, int data) {
      var zOrderSupport = context.Graph.Lookup<ZOrderSupport>();
      if (zOrderSupport != null) {
        zOrderSupport.SetZOrder(key, data);
        zOrderSupport.Update(key);
      }
    }
  }
}
