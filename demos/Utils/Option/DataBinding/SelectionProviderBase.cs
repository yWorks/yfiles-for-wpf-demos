/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using yWorks.Graph;
using yWorks.Utils;

namespace Demo.yFiles.Option.DataBinding
{
  /// <summary>
  ///   A simple abstract implementation of <see cref="ISelectionProvider{T}"/>
  ///   that provides a couple of convenience methods and default implementations where applicable.
  /// </summary>
  /// <typeparam name="T">The type of the items in the provider.</typeparam>
  public abstract class SelectionProviderBase<T> : ISelectionProvider<T>
  {
    /// <summary>
    ///   Whether there is currently an update in progress.
    /// </summary>
    /// <see cref="BeginValueUpdate"/>
    /// <see cref="EndValueUpdate"/>
    public bool InUpdate => inUpdateCounter > 0;

    private int inUpdateCounter;
    private ICompoundEdit currentEdit;

    /// <inheritdoc/>
    public abstract ICollection<IPropertyItemDescriptor<T>> Selection { get; }

    /// <summary>
    ///   Gets or sets the <see cref="InnerContextLookup"/> implementation that is used
    ///   to resolve calls to <see cref="Lookup"/>.
    /// </summary>
    public IContextLookup InnerContextLookup { get; set; } = Lookups.EmptyContextLookup;

    /// <inheritdoc/>
    public abstract void UpdatePropertyViews();

    /// <summary>
    ///   Needs to be implemented by subclasses in order to update the property maps.
    /// </summary>
    protected abstract void UpdateProperties();

    /// <inheritdoc/>
    public void UpdateSelectedItems() {
      OnSelectedItemsUpdated();
    }

    /// <inheritdoc/>
    public event EventHandler SelectedItemsChanged;

    /// <inheritdoc/>
    public event EventHandler PropertyItemsChanged;

    /// <summary>
    ///   Can be called to trigger <see cref="SelectedItemsChanged"/>
    ///   but will not do so if <see cref="InUpdate"/>.
    /// </summary>
    protected virtual void OnSelectionContentUpdated() {
      if (inUpdateCounter == 0) {
        if (SelectedItemsChanged != null) {
          SelectedItemsChanged(this, EventArgs.Empty);
        }
      }
    }

    /// <summary>
    ///   Modifies <see cref="InUpdate"/> if applicable.
    /// </summary>
    public virtual void BeginValueUpdate() {
      inUpdateCounter++;
      var graph = InnerContextLookup.Lookup<IGraph>(this);
      if (graph != null && currentEdit == null) {
        currentEdit = graph.BeginEdit("Property Changed", "PropertyChanged");
      }
    }

    /// <summary>
    ///   Modifies <see cref="InUpdate"/> if applicable.
    /// </summary>
    public void EndValueUpdate() {
      if (currentEdit != null) {
        currentEdit.Commit();
        currentEdit = null;
      }
      inUpdateCounter--;
    }

    /// <summary>
    ///   Called by <see cref="UpdateSelectedItems"/>
    ///   triggers the <see cref="PropertyItemsChanged"/> event
    ///   and calls <see cref="UpdateProperties"/>.
    /// </summary>
    protected virtual void OnSelectedItemsUpdated() {
      inUpdateCounter++;
      try {
        UpdateProperties();
        if (PropertyItemsChanged != null) {
          PropertyItemsChanged(this, EventArgs.Empty);
        }
        // make sure our properties are up to date
      } finally {
        inUpdateCounter--;
      }
    }

    #region ILookup Members

    #endregion

    /// <summary>
    ///   Uses <see cref="InnerContextLookup"/> to satisfy requests.
    /// </summary>
    public virtual object Lookup(Type type) {
      return InnerContextLookup.Lookup(this, type);
    }
  }
}
