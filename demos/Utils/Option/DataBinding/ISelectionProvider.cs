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

using System;
using System.Collections.Generic;
using Demo.yFiles.Option.Handler;
using yWorks.Graph;

namespace Demo.yFiles.Option.DataBinding
{
  /// <summary>
  /// The interface that is used by <see cref="OptionHandler.BuildFromSelection{T}"/>
  /// to create and populate a set of properties for a given selection.
  /// </summary>
  /// <typeparam name="T">The type of the items in the selection.</typeparam>
  /// <seealso cref="DefaultSelectionProvider{T}"/>
  public interface ISelectionProvider<T>: ILookup
  {
    /// <summary>
    /// The current selection.
    /// </summary>
    ICollection<IPropertyItemDescriptor<T>> Selection {
      get;
    }

    /// <summary>
    /// This method is called to update properties views that display the properties
    /// of the current selection.
    /// </summary>
    void UpdatePropertyViews();

    /// <summary>
    /// This method is called to update the currently selected items using the
    /// values in the properties views.
    /// </summary>
    void UpdateSelectedItems();

    /// <summary>
    /// This event gets fired whenever the content or properties
    /// of the items in the selection have been changed.
    /// </summary>
    event EventHandler SelectedItemsChanged;

    /// <summary>
    /// This event gets fired whenever the properties
    /// of the items in the properties views have been changed and should be committed to the 
    /// corresponding items in the selection.
    /// </summary>
    event EventHandler PropertyItemsChanged;

    /// <summary>
    /// Bracketing event call that indicates a compound update.
    /// </summary>
    void BeginValueUpdate();
    /// <summary>
    /// Bracketing event call that indicates the end of a compound update.
    /// </summary>
    void EndValueUpdate();
  }
}
