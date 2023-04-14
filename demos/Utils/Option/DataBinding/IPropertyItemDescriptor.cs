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

namespace Demo.yFiles.Option.DataBinding
{
  /// <summary>
  /// A descriptor that is used by the <see cref="ISelectionProvider{T}"/> interface.
  /// </summary>
  /// <typeparam name="T">The item of the items being described.</typeparam>
  public interface IPropertyItemDescriptor<T>
  {
    /// <summary>
    /// Returns the item whose virtual properties can be accessed via the <see cref="Properties"/>
    /// map.
    /// </summary>
    T Item { get; }
    /// <summary>
    /// Retrieves the map of the properties for the <see cref="Item"/>.
    /// </summary>
    IPropertyMap Properties { get; }
  }

  internal class DefaultPropertyItemDescriptor<T>: IPropertyItemDescriptor<T> {
    private T context;
    private IPropertyMap properties;
    public DefaultPropertyItemDescriptor(T item, IPropertyMap properties) {
      this.context = item;
      this.properties = properties;
    }

    public T Item {
      get { return context; }
    }

    public IPropertyMap Properties {
      get { return properties; }
    }
  }
}
