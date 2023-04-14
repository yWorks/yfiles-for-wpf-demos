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

using System.ComponentModel;
using System.Windows.Controls;
using Demo.yFiles.Option.Handler;
using Demo.yFiles.Option.View;

namespace Demo.yFiles.Option.Editor
{
  /// <summary>
  /// Create a dialog representation of an <see cref="OptionHandler"/> instance.
  /// </summary>
  /// <remarks>Each <see cref="IOptionItem"/> in the handler is normally
  /// represented as a child control in the dialog. Nested <see cref="OptionGroup"/>s are shown
  /// as tab pages on the first level and nested <see cref="GroupBox"/>es on deeper levels.  
  /// This control can use the <see cref="TypeConverter"/> mechanism to display and edit values
  /// as strings, however controls can be completely specified at runtime with
  ///  <see cref="OptionItem.CustomTypeConverterAttribute"/>
  /// and <see cref="OptionItem.CustomDialogitemEditor"/> attributes on the item level.
  /// If <see cref="OptionItem.SupportNullValueAttribute"/> is set to <see langword="true"/>,
  /// an automatic wrapper is created for the item editor and/or the TypeConverter that allows
  /// to explicitly state that "No value" should be set for this item (may differ from setting a <see langword="null"/>
  /// value for the item, which is usually not supported by the core converters and editors.)
  /// If <see cref="OptionItem.SupportUndefinedValueAttribute"/> is set to <see langword="true"/>,
  /// there is also automatic support for displaying so-called <c>undefined</c> values,
  /// which may result from conflicting source values if the editor tries to show
  /// values from multiple instances at once. Undefined values can't be entered in the editor, and
  /// editing an item will clear the <c>undefined</c> state for the item, however
  /// canceling the edit will leave the <c>undefined</c> state intact.</remarks>
  public class DefaultEditorFactory 
  {

    /// <summary>
    /// Attribute key for display-related settings that can be used on an 
    /// <see cref="IOptionItem"/> as
    /// attribute key for <see cref="IOptionItem.Attributes"/>.
    /// </summary>
    /// <remarks>This key is used to change the appearance of a dialog view created with this factory.
    /// The framework expects to find bitwise combinations from values in <see cref="Demo.yFiles.Option.Editor.DefaultEditorFactory.RenderingHints"/>
    /// there, other values are silently ignored.</remarks>
    public const string RenderingHintsAttribute = "DefaultEditorFactory.RenderingHintsAttribute";
   
    /// <summary>
    /// Enum that provides item rendering hints.
    /// </summary>
    public enum RenderingHints
    {
      /// <summary>
      /// If set on an <see cref="IOptionGroup"/>, the item will not be shown as a group,
      /// but its children will be visible as children of the item's owner. This can be used
      /// to enforce logical structuring of a handler without actually displaying a lot of nested groups.
      /// </summary>
      Invisible = 1
    }
  }
}
