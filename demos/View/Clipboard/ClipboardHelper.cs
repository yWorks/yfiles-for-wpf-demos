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

using System.Linq;
using yWorks.Graph;

namespace yWorks.Demo.Clipboard
{
  /// <summary>
  ///   This class is used to assign custom labels to pasted nodes.
  ///   <see cref="IClipboardHelper"/> implementations can be used to associate custom actions
  ///   with graph elements when the element is cut/copied/pasted. Moreover, a clipboard helper
  ///   allows adding user state to clipboard operations. This implementation uses these possibilities
  ///   to retrieve the label of the original node for a Paste operation and the number of copies
  ///   so far to set a customized label for the pasted node.
  /// </summary>
  internal sealed class TaggedNodeClipboardHelper : IClipboardHelper
  {
    private CopyItem? userData;

    /// <summary>
    ///   If the copied node has at least one label, we store a variant of the label text
    ///   (see <see cref="CopyItem"/> implementation).
    /// </summary>
    public void OnCopied(IGraphClipboardContext context, IModelItem item) {
      if (item is INode node && GetExampleLabel(node) is { } label) {
        userData = new CopyItem(label.Text);
      }
    }

    /// <summary>
    ///   If the cut node has at least one label, we store a variant of the label text
    ///   (see <see cref="CopyItem"/> implementation).
    /// </summary>
    public void OnCut(IGraphClipboardContext context, IModelItem item) {
      // do the same for Cut, since it's essentially just a Copy and Delete in succession.
      OnCopied(context, item);
    }

    /// <summary>
    ///   If the pasted node has at least one label, we change the text using the one that is provided
    ///   by <c>userData</c>.
    /// </summary>
    public void OnPasted(IGraphClipboardContext context, IModelItem item) {
      // Note: This is the copied item.
      if (item is INode node && GetExampleLabel(node) is { } label && userData != null) {
        context.TargetGraph.SetLabelText(label, userData.ToString());
      }
    }

    /// <summary>
    ///   If the original node has at least one label, we change the text of the duplicate node with
    ///   the one that is provided by the original node.
    /// </summary>
    public void OnDuplicated(IGraphClipboardContext context, IModelItem original, IModelItem duplicate) {
      if (original is INode originalNode && GetExampleLabel(originalNode) is { } originalLabel &&
          duplicate is INode duplicateNode && GetExampleLabel(duplicateNode) is { } duplicateLabel) {
        var data = new CopyItem(originalLabel.Text);
        context.TargetGraph.SetLabelText(duplicateLabel, data.ToString());
      }
    }

    public bool ShouldCopy(IGraphClipboardContext context, IModelItem item) => true;

    public bool ShouldCut(IGraphClipboardContext context, IModelItem item) => true;

    public bool ShouldPaste(IGraphClipboardContext context, IModelItem item) => true;

    public bool ShouldDuplicate(IGraphClipboardContext context, IModelItem item) => true;

    private static ILabel? GetExampleLabel(INode node) => node.Labels.Count > 1 ? node.Labels.Last() : null;
  }

  /// <summary>
  ///   Instances of this class are used to store information about the clipboard operations as label text and number
  ///   of pasted elements.
  /// </summary>
  internal sealed class CopyItem
  {
    private readonly string text;

    /// <summary>
    ///   Creates a new instance of <see cref="CopyItem"/>.
    /// </summary>
    public CopyItem(string text) {
      this.text = text;
    }

    public int PasteCount { get; private set; }

    /// <summary>
    ///   Returns the label text of the copied item and the number of pasted elements as string.
    /// </summary>
    public override string ToString() {
      // We count how often we have been pasted and change the string
      // accordingly.
      // If we start from a new copy, the counter is thus reset (try it!)
      PasteCount++;
      return PasteCount < 2 ? $"Copy of {text}" : $"Copy ({PasteCount}) of {text}";
    }
  }
}
