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

namespace Demo.yFiles.Option.DataBinding
{
  /// <summary>
  /// Interface for classes that check whether an option item should be <see cref="OptionItemValidities.ReadWrite"/>, <see cref="OptionItemValidities.ReadOnly"/> or
  /// <see cref="OptionItemValidities.Invalid"/>, based on the capabilities of the items in a given <see cref="ISelectionProvider{T}"/>.
  /// </summary>
  public interface IOptionItemFilter<T>
  {
    /// <summary>
    /// Check whether an option item should be <see cref="OptionItemValidities.ReadWrite"/>, <see cref="OptionItemValidities.ReadOnly"/> or
    /// <see cref="OptionItemValidities.Invalid"/>, based on the capabilities of the items in <paramref name="selection"/>.
    /// </summary>
    /// <param name="selection">The selectiion</param>
    /// <returns>An enumeration value from <see cref="OptionItemValidities"/>, depending on the items in <paramref name="selection"/></returns>
    OptionItemValidities CheckValidity(ISelectionProvider<T> selection);
  }

  /// <summary>
  /// Default implementation that determines the state by the presence of valid getters and setters for a given virtual property
  /// </summary>
  internal class DefaultOptionItemFilter<T> : IOptionItemFilter<T>
  {    
    private readonly string virtualPropertyName;

    /// <summary>
    /// Create a new filter that filters based on the capabilities/existence of the specified virtual property for each selection item.
    /// </summary>
    /// <param name="virtualPropertyName">The name of the virtual property</param>
    public DefaultOptionItemFilter(string virtualPropertyName) {
      this.virtualPropertyName = virtualPropertyName;
    }

    #region IOptionItemFilter Members

    ///<inheritdoc/>
    public virtual OptionItemValidities CheckValidity(ISelectionProvider<T> selection) {
      if (selection.Selection.Count == 0) {
        return OptionItemValidities.Invalid;
      }
      OptionItemValidities retval = OptionItemValidities.ReadWrite;
      foreach (IPropertyItemDescriptor<T> descriptor in selection.Selection) {

        IPropertyMap map = descriptor.Properties;
        if(map == null) {
          return OptionItemValidities.Invalid;
        }
        IPropertyItem item = map.GetEntry(virtualPropertyName);
        if (item != null) {
          IValueGetter getter = item.Getter;
          IValueSetter setter = item.Setter;

          if (getter != null && getter.CanGet()) {
            if (setter == null || !setter.CanSet()) {
              //readonly item...
              retval = OptionItemValidities.ReadOnly;
            }
          } else {
            //we can't even get the values :-(
            return OptionItemValidities.Invalid;
          }
        } else {
          return OptionItemValidities.Invalid;
        }
      }
      return retval;
    }

    #endregion
  }

  /// <summary>
  /// Enumeration that describes the capabilities that an option item should have.
  /// </summary>
  [Flags]
  public enum OptionItemValidities : short
  {
    /// <summary>
    /// The item is invalid for the selection
    /// </summary>
    Invalid = 0,
    /// <summary>
    /// The item should be set for the selection
    /// </summary>
    ReadOnly = 1,
    /// <summary>
    /// The item should be set read-write for the selection
    /// </summary>
    ReadWrite = 2
  }
}