/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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

namespace Demo.yFiles.Option.DataBinding
{
  /// <summary>
  /// The interface that is used by <see cref="IPropertyItem"/> to retrieve a value from a given
  /// context.
  /// </summary>
  public interface IValueGetter
  {
    /// <summary>
    /// Retrieves the value from it's context.
    /// </summary>
    /// <returns>The current value.</returns>
    /// <seealso cref="IPropertyBuildContext{TSubject}.CurrentInstance"/>
    object GetValue();
    /// <summary>
    /// Determines whether this instance can get the value from it's context.
    /// </summary>
    /// <returns>Whether a call to <see cref="GetValue"/> can be made.</returns>
    /// <seealso cref="IPropertyBuildContext{TSubject}.CurrentInstance"/>
    bool CanGet();
  }

  /// <summary>
  /// A typed delegate version of <see cref="IValueGetter"/> used primarily by
  /// <see cref="IPropertyBuildContext{TSubject}.AddEntry{TValue}(string,ValueGetterDelegate{TValue},ValueSetterDelegate{TValue})"/>.
  /// </summary>
  /// <typeparam name="TValue">The type of the value to get.</typeparam>
  /// <returns>The value.</returns>
  /// <seealso cref="IPropertyBuildContext{TSubject}.CurrentInstance"/>
  public delegate TValue ValueGetterDelegate<TValue>();

  /// <summary>
  /// A typed delegate version of <see cref="IValueGetter"/> used by
  /// <see cref="DelegateGetter{T}"/>.
  /// </summary>
  /// <returns>Whether a value can be "get".</returns>
  /// <seealso cref="IPropertyBuildContext{TSubject}.CurrentInstance"/>
  /// <seealso cref="DelegateGetter{T}"/>
  public delegate bool ValueGetterValidityPredicate();

  /// <summary>
  /// An adapter class that adapts <see cref="ValueGetterDelegate{TValue}"/>
  /// and <see cref="ValueGetterValidityPredicate"/> to the <see cref="IValueGetter"/>
  /// interface.
  /// </summary>
  /// <typeparam name="T">The type of the items.</typeparam>
  public class DelegateGetter<T> : IValueGetter 
  {

    private readonly ValueGetterDelegate<T> getter;
    private readonly ValueGetterValidityPredicate predicate;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="getter">The getter to use.</param>
    /// <param name="predicate">The predicate to use.</param>
    public DelegateGetter(ValueGetterDelegate<T> getter, ValueGetterValidityPredicate predicate) {
      this.getter = getter;
      this.predicate = predicate;
    }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="getter">The getter to use.</param>
    public DelegateGetter(ValueGetterDelegate<T> getter) {
      this.getter = getter;
    }

    #region IValueGetter Members

    /// <inheritdoc/>
    public virtual object GetValue() {
      return getter();
    }

    /// <inheritdoc/>
    public virtual bool CanGet() {
      return predicate == null? getter != null : getter != null && predicate();
    }

    #endregion
  }
}