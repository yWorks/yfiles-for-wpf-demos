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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace yWorks.Demo
{
  public class Menu
  {
    private readonly Func<ItemsControl> containerFactory;
    private ItemsControl? container;

    internal Menu(Func<ItemsControl> containerFactory) {
      this.containerFactory = containerFactory;
    }

    public ItemsControl Container => container ??= containerFactory();

    public Button AddButton(string title, Action action) {
      var button = new Button { Content = title };
      button.Click += (_, _) => action();
      Container.Items.Add(button);
      return button;
    }

    public ToggleButton AddToggleButton(string title, Action<bool> action, bool initialState = false) {
      var button = new ToggleButton {
          Content = title,
          IsChecked = initialState
      };
      button.Checked += (_, _) => action(true);
      button.Unchecked += (_, _) => action(false);
      Container.Items.Add(button);
      return button;
    }

    public ComboBox AddComboBox<T>(
        string title,
        IEnumerable<(T value, string label)> options,
        Action<T?> callback,
        T initialValue) {
      var panel = new StackPanel { Orientation = Orientation.Horizontal };

      panel.Children.Add(new Label {
          Content = title,
          VerticalAlignment = VerticalAlignment.Center
      });

      var combobox = new ComboBox {
          DisplayMemberPath = "Label",
          SelectedValuePath = "Value",
          VerticalAlignment = VerticalAlignment.Center,
          MinWidth = 120
      };

      foreach (var (value, label) in options) {
        combobox.Items.Add(new ComboBoxOption<T>(value, label));
      }

      combobox.SelectedValue = initialValue;
      combobox.SelectionChanged += (_, _) => callback((T?) combobox.SelectedValue);

      panel.Children.Add(combobox);
      Container.Items.Add(panel);
      return combobox;
    }

    private sealed record ComboBoxOption<T>(T Value, string Label);

    public Slider AddSlider(
        string title,
        double min,
        double max,
        Action<double> callback,
        double initialValue = 0,
        double step = 1,
        bool showValue = true,
        double sliderWidth = 120) {
      var panel = new StackPanel { Orientation = Orientation.Horizontal };

      panel.Children.Add(new Label {
          Content = title,
          VerticalAlignment = VerticalAlignment.Center
      });

      var slider = new Slider {
          Minimum = min,
          Maximum = max,
          SmallChange = step,
          Value = initialValue,
          Width = sliderWidth,
          VerticalAlignment = VerticalAlignment.Center
      };

      Label? value = null;
      if (showValue) {
        var minText = min.ToString("0.##");
        var maxText = max.ToString("0.##");
        var initialText = initialValue.ToString("0.##");

        value = new Label {
            Content = initialText,
            MinWidth = 40,
            VerticalAlignment = VerticalAlignment.Center
        };
      }

      slider.ValueChanged += (_, e) => {
        value?.SetCurrentValue(ContentControl.ContentProperty, e.NewValue.ToString("0.##"));
        callback(e.NewValue);
      };

      panel.Children.Add(slider);

      if (value != null) {
        panel.Children.Add(value);
      }

      Container.Items.Add(panel);
      return slider;
    }

    public Separator AddSeparator() {
      var separator = new Separator();
      Container.Items.Add(separator);
      return separator;
    }
  }
}
