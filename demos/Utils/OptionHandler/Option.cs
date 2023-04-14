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
using System.Collections.Generic;
using System.Windows;

namespace Demo.yFiles.Toolkit.OptionHandler {

  /// <summary>
  /// A data holder for a single configuration option
  /// </summary>
  /// <remarks>
  /// The <see cref="ConfigConverter"/> scanns a configuration object in its <see cref="ConfigConverter.Convert"/> method
  /// and creates <see cref="Option"/> items for the public fields and properties of this object.
  /// </remarks>
  public class Option {

    /// <summary>
    /// The internally used name of this option.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The displayed label of this option.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// The default value this option has.
    /// </summary>
    public object DefaultValue { get; set; }

    /// <summary>
    /// The current value of the field or property in the scanned configuration object.
    /// </summary>
    /// <remarks>
    /// Note that this property is read from and written to the configuration object this option was created for.
    /// </remarks>
    public object Value {
      get {
        if (Getter != null) {
          return Getter();
        }
        return DefaultValue;
      }
      set {
        if (Setter != null) {
          Setter(value);
        }    
      }
    }

    /// <summary>
    /// The <see cref="Type"/> of this option.
    /// </summary>
    public Type ValueType { get; set; }

    /// <summary>
    /// The type of the ui component that shall be used to represent this option.
    /// </summary>
    public ComponentTypes ComponentType { get; set; }

    /// <summary>
    /// A list of the available enum values for this option.
    /// </summary>
    public IList<EnumValue> EnumValues { get; set; }

    /// <summary>
    /// The <see cref="MinMaxAttribute"/> containing the minimum and maximum value for this option.
    /// </summary>
    public MinMaxAttribute MinMax { get; set; }

    /// <summary>
    /// A utility method that returns whether this option should currently be disabled.
    /// </summary>
    public Func<bool> CheckDisabled { get; set; }

    public bool IsEnabled {
      get { return CheckDisabled == null || !CheckDisabled(); }
    }

    /// <summary>
    /// A utility method that returns whether this option should currently be hidden.
    /// </summary>
    public Func<bool> CheckHidden { get; set; }

    public Visibility Visibility {
      get {
        return CheckHidden == null || !CheckHidden() ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    #region internal properties

    internal Func<object> Getter { get; set; }
    
    internal Action<object> Setter { get; set; }

    #endregion

    /// <summary>
    /// Resets the <see cref="Value"/> to the <see cref="DefaultValue"/>
    /// </summary>
    public virtual void Reset() {
      Value = DefaultValue;
    }

  }
}
