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

namespace Demo.yFiles.Option.DataBinding
{
  /// <summary>
  /// The interface that is used by <see cref="IPropertyItem"/> to set a value in a given
  /// context.
  /// </summary>
  public interface IValueSetter
  {
    /// <summary>
    /// Sets the new value in the context.
    /// </summary>
    /// <param name="value">The new value to set.</param>
    /// <seealso cref="IPropertyBuildContext{TSubject}.CurrentInstance"/>
    /// <seealso cref="IPropertyBuildContext{TSubject}.SetNewInstance"/>
    void SetValue(object value);
    /// <summary>
    /// Determines whether this instance can perform a <see cref="SetValue"/>.
    /// </summary>
    /// <seealso cref="IPropertyBuildContext{TSubject}.CurrentInstance"/>
    /// <seealso cref="IPropertyBuildContext{TSubject}.SetNewInstance"/>
    /// <returns>Whether a call to <see cref="SetValue"/> would succeed.</returns>
    bool CanSet();
  }

  /// <summary>
  /// An adapter class that adapts <see cref="ValueSetterDelegate{TValue}"/>
  /// and <see cref="ValueSetterValidityPredicate"/> to the <see cref="IValueSetter"/>
  /// interface.
  /// </summary>
  /// <typeparam name="T">The type of the items to set.</typeparam>
  public class DelegateSetter<T> : IValueSetter 
  {
    private readonly ValueSetterDelegate<T> setter;
    private readonly ValueSetterValidityPredicate predicate;


    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="setter">The setter to use.</param>
    /// <param name="predicate">The predicate to use</param>
    public DelegateSetter(ValueSetterDelegate<T> setter, ValueSetterValidityPredicate predicate) {
      this.setter = setter;
      this.predicate = predicate;
    }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="setter">The setter to use.</param>
    public DelegateSetter(ValueSetterDelegate<T> setter) {
      this.setter = setter;
    }

    #region IValueSetter Members
    /// <inheritdoc/>
    public virtual void SetValue(object value) {
      if (value is T || value == null) {
        setter((T)value);
      }
    }

    /// <inheritdoc/>
    public virtual bool CanSet() {
      return predicate==null? setter != null : setter != null && predicate();
    }

    #endregion
  }

  /// <summary>
  /// A delegate version of the <see cref="IValueSetter"/> interface.
  /// </summary>
  /// <typeparam name="TValue">The type of value to set.</typeparam>
  /// <param name="value">The value to set on the context.</param>
  /// <seealso cref="IPropertyBuildContext{TSubject}.CurrentInstance"/>
  /// <seealso cref="IValueSetter"/>
  public delegate void ValueSetterDelegate<TValue>(TValue value);

  /// <summary>
  /// A delegate version of the <see cref="IValueSetter"/> interface.
  /// </summary>
  /// <returns>Whether setting would succeed.</returns>
  /// <seealso cref="IPropertyBuildContext{TSubject}.CurrentInstance"/>
  /// <seealso cref="IValueSetter"/>
  public delegate bool ValueSetterValidityPredicate();
}
