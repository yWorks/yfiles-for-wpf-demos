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
using Demo.yFiles.Option.Handler;

namespace Demo.yFiles.Option.DataBinding
{

  /// <summary>
  /// Interface for classes that can provide an <see cref="OptionHandler"/> presentation of a given set of properties.
  /// </summary>
  /// <remarks>Usually, instances of this class can be retrieved through an object's lookup or, alternatively, by an 
  /// <see cref="OptionBuilderAttribute"/> that is applied to the type in question.
  /// </remarks>
  public interface IOptionBuilder
  {
    /// <summary>
    /// Adds new option items to the given <paramref name="context"/>.
    /// </summary>
    /// <remarks>The actual items should be added with the help of the <see cref="IOptionBuilderContext.BindItem(IOptionItem,string)"/>
    /// method that creates all necessary binding between the item and the underlying selection.</remarks>
    void AddItems(IOptionBuilderContext context, Type subjectType, object subject);
  }

  /// <summary>
  /// When this attribute is set to a type or property, the framework will use the given type to create a new <see cref="IOptionBuilder"/> instance.
  /// </summary>
  /// <remarks>Specifying a builder in an object's lookup or related places will override this attribute.</remarks>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property, Inherited = true)]
  public class OptionBuilderAttribute: Attribute
  {
    private readonly Type optionBuilderType;

    /// <summary>
    /// Creates a new attribute that specifies that the given builder type should be used by default for the target type.
    /// </summary>
    /// <param name="optionBuilderType">The type of the builder to use.</param>
    public OptionBuilderAttribute(Type optionBuilderType) {
      this.optionBuilderType = optionBuilderType;
    }

    /// <summary>
    /// Gets the type of builder to use for the attribute target.
    /// </summary>
    public Type OptionBuilderType {
      get { return optionBuilderType; }
    }
  }
}