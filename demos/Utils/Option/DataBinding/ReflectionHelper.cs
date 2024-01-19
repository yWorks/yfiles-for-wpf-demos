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
using System.Diagnostics;

namespace Demo.yFiles.Option.DataBinding
{
  /// <summary>
  /// Static helper class that retrieves <see cref="IPropertyMapBuilder"/>
  /// and <see cref="IOptionBuilder"/> instances for a given type via annotations.
  /// </summary>
  /// <seealso cref="PropertyMapBuilderAttribute"/>
  /// <seealso cref="OptionBuilderAttribute"/>
  /// <seealso cref="AttributeBasedPropertyMapBuilderAttribute"/>
  public static class ReflectionHelper
  {
    /// <summary>
    /// Tries to retrieve an <see cref="IPropertyMapBuilder"/> via attributes for a given
    /// type.
    /// </summary>
    /// <remarks>
    /// This method constructs a builder using either the <see cref="PropertyMapBuilderAttribute"/>
    /// or an automatically created instance via the <see cref="AttributeBasedPropertyMapBuilderAttribute"/>.
    /// </remarks>
    /// <param name="type">The type to introspect.</param>
    /// <returns>An instance or <see langword="null"/>.</returns>
    public static IPropertyMapBuilder GetPropertyBuilderFromAttribute(Type type) {
      object[] builderAttributes = type.GetCustomAttributes(typeof (PropertyMapBuilderAttribute), true);
      if (builderAttributes.Length > 0) {
        PropertyMapBuilderAttribute mapBuilderAttr = builderAttributes[0] as PropertyMapBuilderAttribute;
        if (mapBuilderAttr != null) {
          try {
            return Activator.CreateInstance(mapBuilderAttr.PropertyMapBuilderType) as IPropertyMapBuilder;
          } catch (Exception exc) {
            Trace.WriteLine("Unable to create OptionBuilder for type " + type + ": " + exc.Message);
          }
        }
      }
      builderAttributes = type.GetCustomAttributes(typeof (AttributeBasedPropertyMapBuilderAttribute), true);
      if (builderAttributes.Length > 0) {
        AttributeBasedPropertyMapBuilderAttribute mapBuilderAttr = builderAttributes[0] as AttributeBasedPropertyMapBuilderAttribute;
        if (mapBuilderAttr != null) {
          try {
            return mapBuilderAttr.CreateBuilder(type);
          } catch (Exception exc) {
            Trace.WriteLine("Unable to create OptionBuilder for type " + type + ": " + exc.Message);
          }
        }
      }
      return null;
    }

    /// <summary>
    /// Tries to retrieve an <see cref="IOptionBuilder"/> via attributes for a given
    /// type.
    /// </summary>
    /// <remarks>
    /// This method constructs a builder using the <see cref="OptionBuilderAttribute"/>.
    /// </remarks>
    /// <param name="type">The type to introspect.</param>
    /// <returns>An instance or <see langword="null"/>.</returns>
    public static IOptionBuilder GetOptionBuilderFromAttribute(Type type) {
      object[] builderAttributes = type.GetCustomAttributes(typeof (OptionBuilderAttribute), true);
      if (builderAttributes.Length > 0) {
        OptionBuilderAttribute mapBuilderAttr = builderAttributes[0] as OptionBuilderAttribute;
        if (mapBuilderAttr != null) {
          try {
            return Activator.CreateInstance(mapBuilderAttr.OptionBuilderType) as IOptionBuilder;
          } catch (Exception exc) {
            Trace.WriteLine("Unable to create OptionBuilder for type " + type + ": " + exc.Message);
          }
        }
      }
      return null;
    }
  }
}