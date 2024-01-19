/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Collections;
using yWorks.Graph;

namespace Demo.yFiles.Option.DataBinding
{
  /// <summary>
  /// The interface that is used by implementations that are passed to
  /// calls to <see cref="IPropertyMapBuilder.BuildPropertyMap{T}"/>.
  /// </summary>
  /// <typeparam name="TSubject">The type of the subject the context is working on.</typeparam>
  public interface IPropertyBuildContext<TSubject> : ILookup where TSubject : class
  {
    /// <summary>
    /// Create and add a new virtual property with the given parameters
    /// </summary>
    /// <param name="virtualPropertyName">The name of the property</param>
    /// <param name="getter">The getter that is used to retrieve the value from the property</param>
    /// <param name="setter">The setter taht is used to set the value on the property</param>
    /// <returns>A new virtual property</returns>
    /// <param name="comparer">An optional comparer that is used instead of object equality for forming the composites and write back</param>
    IPropertyItem AddEntry(string virtualPropertyName, IValueGetter getter, IValueSetter setter,
                           IEqualityComparer comparer);

    /// <summary>
    /// Create and add a new virtual property with the given parameters
    /// </summary>
    /// <param name="virtualPropertyName">The name of the property</param>
    /// <param name="getter">The getter that is used to retrieve the value from the property</param>
    /// <param name="setter">The setter taht is used to set the value on the property</param>
    /// <returns>A new virtual property</returns>
    /// <param name="comparer">An optional comparer that is used instead of object equality for forming the composites and write back</param>
    IPropertyItem AddEntry<TValue>(string virtualPropertyName, ValueGetterDelegate<TValue> getter,
                                   ValueSetterDelegate<TValue> setter,
                                   IEqualityComparer comparer);

    /// <summary>
    /// Create and add a new virtual property with the given parameters
    /// </summary>
    /// <param name="virtualPropertyName">The name of the property</param>
    /// <param name="getter">The getter that is used to retrieve the value from the property</param>
    /// <param name="setter">The setter taht is used to set the value on the property</param>
    /// <returns>A new virtual property</returns>
    IPropertyItem AddEntry<TValue>(string virtualPropertyName, ValueGetterDelegate<TValue> getter, ValueSetterDelegate<TValue> setter);

    /// <summary>
    /// Create and add a new virtual property with the given parameters
    /// </summary>
    /// <param name="virtualPropertyName">The name of the property</param>
    /// <param name="getter">The getter that is used to retrieve the value from the property</param>
    /// <param name="setter">The setter that is used to set the value on the property</param>
    /// <returns>A new virtual property</returns>
    IPropertyItem AddEntry(string virtualPropertyName, IValueGetter getter, IValueSetter setter);

    /// <summary>
    /// Callback that is used to set a new instance for this context.
    /// </summary>
    /// <param name="newInstance">The new instance to set for this context.</param>
    /// <seealso cref="CurrentInstance"/>
    void SetNewInstance(TSubject newInstance);

    /// <summary>
    /// Retrieves the current instance the getter and setter implementations need to perform their query.
    /// Note that this is a dynamic instance and has be requeried each time it is needed.
    /// </summary>
    TSubject CurrentInstance { get; }

    /// <summary>
    /// Gets the policy the client requests for the building of the property map.
    /// </summary>
    AssignmentPolicy Policy {
      get;
    }

    /// <summary>
    /// Tries to retrieve a builder instance for the given subject.
    /// </summary>
    /// <param name="subject">The subject to retrieve a builder for.</param>
    /// <returns>A builder instance that can be used for building the properties of the provided subject or <see langword="null"/>.</returns>
    /// <seealso cref="GetPropertyMapBuilder(Type,object)"/>
    IPropertyMapBuilder GetPropertyMapBuilder(object subject);
    /// <summary>
    /// Tries to retrieve a builder instance for the given subject or type, e.g. if subject itself is <see langword="null"/>.
    /// </summary>
    /// <param name="type">The type of the subject to retrieve a builder for.</param>
    /// <param name="subject">The subject to retrieve a builder for.</param>
    /// <returns>A builder instance that can be used for building the properties of the provided subject or <see langword="null"/>.</returns>
    /// <seealso cref="GetPropertyMapBuilder(object)"/>
    IPropertyMapBuilder GetPropertyMapBuilder(Type type, object subject);

    /// <summary>
    /// Creates a build context that can be used to recursively build the properties of a compound type.
    /// </summary>
    /// <typeparam name="TChild">The type of the property.</typeparam>
    /// <param name="name">The name of the child context.</param>
    /// <param name="getHandler">The handler that retrieves the value of the property. It must use <see cref="CurrentInstance"/>
    /// to retrieve the value if it depends on that context.</param>
    /// <param name="setHandler">The handler that can actually set a new instance of the property to this context.</param>
    /// <param name="policy">The policy the child context should use.</param>
    /// <returns>A context to use for adding properties of a child of the current context.</returns>
    IPropertyBuildContext<TChild> CreateChildContext<TChild>(string name, GetInstanceDelegate<TChild> getHandler, SetInstanceDelegate<TChild> setHandler, AssignmentPolicy policy) where TChild:class;
  }
}
