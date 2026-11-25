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

using System.Collections.Generic;
using System.Linq;
using yWorks.Graph;

namespace yWorks.Demo.Clipboard
{
  /// <summary>
  ///   Holds the business objects associated with the nodes in this demo.
  /// </summary>
  public sealed class ClipboardBusinessObject
  {
    public string Name { get; set; }

    public ClipboardBusinessObject(string name) {
      Name = name;
    }
  }

  internal static class BusinessDataHandling
  {
    private static int counter;

    /// <summary>
    ///   Creates an instance of ClipboardBusinessObject with a new name.
    /// </summary>
    public static ClipboardBusinessObject CreateNodeBusinessData() => new($"Name {++counter}");

    /// <summary>
    ///   Returns the common name of the selected nodes if such a common name
    ///   exists, or the empty string otherwise.
    /// </summary>
    public static string GetCommonName(IEnumerable<INode> selectedNodes) {
      var nodes = selectedNodes.ToList();
      if (nodes.Count == 0) {
        return string.Empty;
      }

      var name = (nodes[0].Tag as ClipboardBusinessObject)?.Name ?? string.Empty;
      return nodes.Any(node => (node.Tag as ClipboardBusinessObject)?.Name != name) ? string.Empty : name;
    }
  }
}
