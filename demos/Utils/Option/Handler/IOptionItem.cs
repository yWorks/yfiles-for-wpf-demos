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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Xml;
using yWorks.Graph;

namespace Demo.yFiles.Option.Handler
{
  /// <summary>
  /// This implements the basic interface for an atomic item in an <see cref="OptionHandler"/>
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public interface IOptionItem : INotifyPropertyChanged, ILookup
  {
    /// <summary>
    /// Get or set whether this item is enabled or not.
    /// </summary>
    /// <remarks>This only affects the status in GUI editors, directly setting
    /// the item's value with <see cref="Value"/> works regardless of this property.</remarks>
    bool Enabled { get; set; }

    /// <summary>
    /// Get the canonical name for this item, which will be used for i18n lookups and
    /// item identification
    /// </summary>
    string Name { get; }

    /// <summary>
    /// This event should be fired whenever an attribute value has been changed.
    /// </summary>
//    event AttributeChangedHandler AttributeChanged;

    /// <summary>
    /// The actual type that this item can contain.
    /// </summary>
    /// <remarks>This is needed since <see cref="Value"/> can only handle  <see cref="object"/>
    /// values in its interface. It is used to determine the GUI editors and converters.</remarks>
    Type Type { get; }

    /// <summary>
    /// The actual value that is currently stored in this item.
    /// </summary>
    object Value { get; set; }

    /// <summary>
    /// Get or set the owning group of an item
    /// </summary>
    IOptionGroup Owner {
      get; set;
    }

    /// <summary>
    /// Gets the attribute map of this item
    /// </summary>
    /// <remarks>Attributes control various aspects of an item's behaviour, such as custom
    /// UI editors, converters, whether null or undefined values are allowed etc.
    /// </remarks>
    /// <returns>The map for storing and retrieving attributes</returns>
    IMapper<string, object> Attributes { get; }
  }
}
