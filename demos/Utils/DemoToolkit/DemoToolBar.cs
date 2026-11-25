/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.1.
 ** Copyright (c) 2000-2026 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using yWorks.Controls.Input;

namespace yWorks.Demo
{
  /// <summary>
  ///   The button presets available through <see cref="DemoToolBar.ButtonPresets"/>.
  ///   Presets can be combined with the bitwise OR operator in C# or comma-separated names in XAML.
  /// </summary>
  [Flags]
  public enum DemoToolBarButtonSet
  {
    /// <summary>No preset buttons.</summary>
    None = 0,

    /// <summary>Decrease zoom, increase zoom, and fit graph bounds.</summary>
    Zoom = 1,

    /// <summary>Cut, copy, paste, undo, redo, group, and ungroup.</summary>
    Editing = 2,

    /// <summary>Open and save as. The command target must provide the corresponding command bindings.</summary>
    File = 4
  }

  public class DemoToolBar : ToolBar
  {
    public static readonly DependencyProperty ButtonPresetsProperty = DependencyProperty.Register(nameof(ButtonPresets),
        typeof(DemoToolBarButtonSet), typeof(DemoToolBar),
        new PropertyMetadata(DemoToolBarButtonSet.None, OnButtonSetChanged));

    public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(nameof(CommandTarget),
        typeof(IInputElement), typeof(DemoToolBar), new PropertyMetadata(null));

    private readonly List<Control> presetItems = [];
    private Separator? customItemsSeparator;

    static DemoToolBar() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(DemoToolBar), new FrameworkPropertyMetadata(typeof(DemoToolBar)));
    }

    /// <summary>
    ///   Gets or sets the combination of button presets placed before the demo-specific items.
    ///   Presets appear in the order File, Zoom, Editing, with separators between groups.
    ///   A separator is shown after the preset buttons when demo-specific items are present.
    ///   Defaults to <see cref="DemoToolBarButtonSet.None"/>.
    /// </summary>
    /// <example>
    ///   In XAML, use <c>ButtonPreset="Zoom, Editing"</c> to combine zoom and editing buttons.
    /// </example>
    public DemoToolBarButtonSet ButtonPresets {
      get => (DemoToolBarButtonSet) GetValue(ButtonPresetsProperty);
      set => SetValue(ButtonPresetsProperty, value);
    }

    /// <summary>
    ///   Gets or sets the command target for the buttons created by <see cref="ButtonPresets"/>,
    ///   usually the demo's graph control.
    ///   Demo-specific buttons can specify their own command targets.
    /// </summary>
    public IInputElement? CommandTarget {
      get => (IInputElement?) GetValue(CommandTargetProperty);
      set => SetValue(CommandTargetProperty, value);
    }

    private static void OnButtonSetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      ((DemoToolBar) d).UpdatePresetItems();
    }

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e) {
      base.OnItemsChanged(e);
      if (customItemsSeparator != null) {
        customItemsSeparator.Visibility = Items.Count > presetItems.Count ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    private void UpdatePresetItems() {
      customItemsSeparator = null;
      foreach (var item in presetItems) {
        Items.Remove(item);
      }
      presetItems.Clear();

      if (ButtonPresets == DemoToolBarButtonSet.None) {
        return;
      }

      if ((ButtonPresets & DemoToolBarButtonSet.Zoom) != 0) {
        AddPresetButtons(NavigationCommands.DecreaseZoom, NavigationCommands.IncreaseZoom,
            GraphCommands.FitGraphBounds);
      }

      if ((ButtonPresets & DemoToolBarButtonSet.File) != 0) {
        AddPresetButtons(ApplicationCommands.Open, ApplicationCommands.SaveAs);
      }

      if ((ButtonPresets & DemoToolBarButtonSet.Editing) != 0) {
        AddPresetButtons(ApplicationCommands.Cut, ApplicationCommands.Copy, ApplicationCommands.Paste);
        AddPresetButtons(ApplicationCommands.Undo, ApplicationCommands.Redo);
        AddPresetButtons(GraphCommands.GroupSelection, GraphCommands.UngroupSelection);
      }

      if (presetItems.Count == 0) {
        return;
      }

      customItemsSeparator = new Separator { Visibility = Visibility.Collapsed };
      presetItems.Add(customItemsSeparator);

      for (var i = 0; i < presetItems.Count; i++) {
        Items.Insert(i, presetItems[i]);
      }
    }

    private void AddPresetButtons(params ICommand[] commands) {
      if (presetItems.Count > 0) {
        presetItems.Add(new Separator());
      }
      foreach (var command in commands) {
        AddPresetButton(command);
      }
    }

    private void AddPresetButton(ICommand command) {
      var button = new Button { Command = command };
      button.SetBinding(ButtonBase.CommandTargetProperty,
          new Binding(nameof(CommandTarget)) {
              RelativeSource = new RelativeSource { AncestorType = typeof(DemoToolBar) }
          });
      presetItems.Add(button);
    }
  }
}
