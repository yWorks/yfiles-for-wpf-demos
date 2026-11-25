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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Utils;

namespace yWorks.Demo
{
  public static class PresentationSupport
  {
    /// <summary>
    ///   Highlights the given <paramref name="items"/> for the given <paramref name="duration"/>.
    /// </summary>
    internal static async Task HighlightItems(GraphControl gc, IEnumerable<IModelItem> items, TimeSpan duration) {
      var manager = gc.HighlightIndicatorManager;
      var highlightItems = manager.Items;
      var prevSet = new HashSet<IModelItem>(highlightItems);
      var newlyAdded = new List<IModelItem>();
      var modelItems = items as IModelItem[] ?? items.ToArray();

      if (GetItemsBounds(gc, modelItems) is { } bounds) {
        await gc.EnsureVisible(bounds.GetEnlarged(20));
      }

      foreach (var item in modelItems) {
        if (!prevSet.Contains(item)) {
          highlightItems.Add(item);
          newlyAdded.Add(item);
        }
      }

      await Task.Delay(duration);

      foreach (var item in newlyAdded) {
        try {
          highlightItems.Remove(item);
        } catch {
          // ignored
        }
      }
    }

    private static RectD? GetItemsBounds(GraphControl gc, IEnumerable<IModelItem> items) {
      var context = gc.CanvasContext;
      RectD? union = null;
      foreach (var item in items) {
        if (item.Lookup<IBoundsProvider>() is not { } bp) {
          continue;
        }
        var r = bp.GetBounds(context);
        if (r.IsEmpty()) {
          continue;
        }
        union = union is { } u ? RectD.Add(u, r) : r;
      }
      return union;
    }
  }
}
