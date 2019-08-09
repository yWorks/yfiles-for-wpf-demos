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

using System;
using Demo.yFiles.GraphEditor.Modules;
using yWorks.Controls;
using yWorks.Graph;

namespace Demo.yFiles.GraphEditor.UI
{
  internal abstract class IGraphModule : YModule {
    /// <summary>
    /// Create a new instance.
    /// </summary>
    /// <param name="moduleName">The name of the module</param>
    protected IGraphModule(string moduleName) : base(moduleName) {}

    protected override Type[] GetRequiredTypes() {
      return new Type[] {typeof (IGraph), typeof (GraphControl)};
    }

    protected IGraph CurrentIGraph {
      get { return Context.Lookup<IGraph>(); }
    }

    protected GraphControl GraphControl {
      get { return Context.Lookup<GraphControl>(); }
    }

    public virtual bool IsSelected(ILookup context, IModelItem item) {
      ISelectionModel<IModelItem> selectionModel = context.Lookup<ISelectionModel<IModelItem>>();      
      if (selectionModel != null) {
        return selectionModel.IsSelected(item);
      }
      return false;
    }
  }
}