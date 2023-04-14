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

using Demo.yFiles.Option.Handler;
using yWorks.Graph;

namespace Demo.yFiles.Option.View
{
  /// <summary>
  /// Interface for classes that present a specialized view on an OptionHandler.
  /// </summary>
  /// <remarks>Each view is associatiated with one OptionHandler, whereas a single OptionHandler
  /// can have many views on it.</remarks>
  public interface IModelView : ILookup
  {
    /// <summary>
    /// Adopt values from the underlying OptionHandler.
    /// </summary>
    void AdoptValues();

    /// <summary>
    /// Propagate all values to the underlying OptionHandler
    /// </summary>
    void CommitValues();

    /// <summary>
    /// Enable or disable automatic value adoption from the underlying OptionHandler
    /// </summary>
    bool IsAutoAdopt { get; set; }

    /// <summary>
    /// Enable or disable automatic value propagation to the underlying OptionHandler
    /// </summary>
    bool IsAutoCommit { get; set; }

    /// <summary>
    /// Return the <see cref="OptionHandler"/> instance where this view is currently registered.
    /// </summary>
    OptionHandler Handler {
      get;
    }

    /// <summary>
    /// Get the structural item that corresponds to <paramref name="item"/> in the original
    /// option handler.
    /// </summary>
    /// <param name="item">The item in the original option handler.</param>
    /// <returns>the structural item that corresponds to <paramref name="item"/></returns>
    IOptionItem GetViewItem(IOptionItem item);

    /// <summary>
    /// This method resets all values of a view to a backup value.
    /// </summary>
    /// <remarks>
    /// Which value will be used as backup value depends on the specific view implementation.
    /// </remarks>
    void ResetValues();


    /// <summary>
    /// This method resets the value of the provided item to a backup value.
    /// </summary>
    /// <remarks>
    /// Which value will be used as backup value depends on the specific view implementation.
    /// </remarks>
    void ResetValue(IOptionItem item);
  }
}
