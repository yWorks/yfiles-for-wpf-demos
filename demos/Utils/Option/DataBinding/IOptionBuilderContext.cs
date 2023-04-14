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
using Demo.yFiles.Option.Handler;
using yWorks.Graph;

namespace Demo.yFiles.Option.DataBinding
{
  /// <summary>
  /// The interface for the context object used in <see cref="IOptionBuilder.AddItems"/>.
  /// </summary>
  /// <remarks>
  /// This interface is used as a builder for the underlying <see cref="OptionHandler"/>
  /// and is used primarily during <see cref="OptionHandler.BuildFromSelection{T}"/>.
  /// Additional information can be passed to clients via the <see cref="ILookup.Lookup"/>.
  /// </remarks>
  /// <seealso cref="DefaultSelectionProvider{T}.CreateDescriptor(T)"/>
  public interface IOptionBuilderContext : ILookup
  {
    /// <summary>
    /// Tries to bind an <see cref="IOptionItem"/> that has not been added to an <see cref="OptionHandler"/>
    /// to a <see cref="IPropertyItem"/> that is referenced by an id in the current context.
    /// </summary>
    /// <remarks>
    /// If successful, this method will add the item to the <see cref="IOptionGroup"/> that is
    /// currently selected by this context.
    /// </remarks>
    /// <param name="item">The item to add to the optionhandler and bind to the corresponding property.</param>
    /// <param name="localId">The local id of the item in the current context.</param>
    /// <returns>Whether the item was successfully bound and added.</returns>
    /// <seealso cref="BindItem(IOptionItem,bool,string,bool)"/>
    /// <seealso cref="CreateChildContext"/>
    bool BindItem(IOptionItem item, string localId);
    /// <summary>
    /// Tries to bind an <see cref="IOptionItem"/> that has not been added to an <see cref="OptionHandler"/>
    /// to a <see cref="IPropertyItem"/> that is referenced by an id in the current context.
    /// </summary>
    /// <remarks>
    /// Using <paramref name="addToOptionHandler"/> it can be specified whether the item should be added to 
    /// the current <see cref="IOptionGroup"/> that is specific for this context. If set to <see langword="false"/>
    /// the caller has to add the item to the option handler.
    /// <br/>
    /// Using <paramref name="fullyQualifiedId"/> it can be specified whether the <paramref name="id"/> property should
    /// be interpreted as a fully qualified id that is not bound to the current context. 
    /// </remarks>
    /// <param name="item">The item to add to the optionhandler and bind to the corresponding property.</param>
    /// <param name="fullyQualifiedId">Whether to interpret <paramref name="id"/> as a fully qualified id or not.</param>
    /// <param name="addToOptionHandler">Whether to add the item at its default location to 
    /// the context if it is bound successfully or not.</param>
    /// <param name="id">The id of the item in the current context, this can be a fully qualified id if <paramref name="fullyQualifiedId"/> is
    /// set to <see langword="true"/>.</param>
    /// <returns>Whether the item was successfully bound.</returns>
    /// <seealso cref="BindItem(IOptionItem,string)"/>
    bool BindItem(IOptionItem item, bool fullyQualifiedId, string id, bool addToOptionHandler);
    /// <summary>
    /// Tries to find a suitable <see cref="IOptionBuilder"/> instance for the specified subject.
    /// </summary>
    /// <param name="subject">The non-<see langword="null"/> subject to retrieve a builder instance for.</param>
    /// <returns>A builder instance or <see langword="null"/></returns>
    /// <seealso cref="GetOptionBuilder(Type,object)"/>
    IOptionBuilder GetOptionBuilder(object subject);
    /// <summary>
    /// Tries to find a suitable <see cref="IOptionBuilder"/> instance for the specified subject based
    /// on the type information.
    /// </summary>
    /// <param name="type">The type of the subject for which a builder should be returned.</param>
    /// <param name="subject">The possibly <see langword="null"/> subject to retrieve a builder instance for.</param>
    /// <returns>A builder instance or <see langword="null"/></returns>
    IOptionBuilder GetOptionBuilder(Type type, object subject);
    /// <summary>
    /// Creates a child builder context of this context using a provided prefix for the properties ids
    /// and creating a new <see cref="IOptionGroup"/> in the optionhandler using <paramref name="prefix"/>
    /// as the symbolic name.
    /// </summary>
    /// <param name="prefix">The name to use for the <see cref="IOptionGroup"/> and to use for prefixing the
    /// property item ids. This prefix will be appended to the local prefix.</param>
    /// <returns>A nested child context.</returns>
    IOptionBuilderContext CreateChildContext(string prefix);
  }
}