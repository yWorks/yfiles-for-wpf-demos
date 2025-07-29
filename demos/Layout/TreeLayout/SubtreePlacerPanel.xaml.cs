/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.0.
 ** Copyright (c) 2000-2025 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Demo.yFiles.Layout.Tree.Configuration;
using Demo.yFiles.Option.DataBinding;
using Demo.yFiles.Option.Handler;
using Demo.yFiles.Toolkit;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout.Tree;

namespace Demo.yFiles.Layout.Tree
{
  /// <summary>
  ///   Interaction logic for SubtreePlacerPanel.xaml
  /// </summary>
  public partial class SubtreePlacerPanel
  {
    private readonly IDictionary<string, SubtreePlacerDescriptor> levelConfigurations =
        new Dictionary<string, SubtreePlacerDescriptor>();

    private List<ISubtreePlacerConfiguration> descriptorCollection;

    private TreeLayout previewLayout;
    private TreeLayoutData<INode, IEdge, ILabel, ILabel> previewLayoutData;
    private DefaultSelectionProvider<ISubtreePlacerConfiguration> selectionProvider;

    public SubtreePlacerPanel() {
      InitializeComponent();
      subtreePlacerTypeComboBox.Items.Add(SubtreePlacerConfigurations.SingleLayerSubtreePlacer.Name);
      subtreePlacerTypeComboBox.Items.Add(SubtreePlacerConfigurations.BusPlacer.Name);
      subtreePlacerTypeComboBox.Items.Add(SubtreePlacerConfigurations.DoubleLayerPlacer.Name);
      subtreePlacerTypeComboBox.Items.Add(SubtreePlacerConfigurations.LeftRightPlacer.Name);
      subtreePlacerTypeComboBox.Items.Add(SubtreePlacerConfigurations.ArSubtreePlacer.Name);
      subtreePlacerTypeComboBox.Items.Add(SubtreePlacerConfigurations.AssistantPlacer.Name);
      subtreePlacerTypeComboBox.Items.Add(SubtreePlacerConfigurations.None.Name);

      SetupSubtreePlacerOptions();
      SetupPreview();
    }

    public IList<SubtreePlacerDescriptor> SubtreePlacers { get; } = new List<SubtreePlacerDescriptor>();

    #region UI event handlers that connect to the outside world

    /// <summary>
    ///   Raises the <see cref="ReloadConfiguration"/> event.
    /// </summary>
    protected virtual void OnReloadConfiguration(EventArgs args) {
      if (ReloadConfiguration != null) {
        ReloadConfiguration(this, args);
      }
    }

    /// <summary>
    ///   Occurs when a request to reload the configuration is issued.
    /// </summary>
    public event EventHandler ReloadConfiguration;

    /// <summary>
    ///   Raises the <see cref="ApplyConfiguration"/> event.
    /// </summary>
    protected virtual void OnApplyConfiguration(EventArgs args) {
      if (ApplyConfiguration != null) {
        ApplyConfiguration(this, args);
      }
    }

    /// <summary>
    ///   Occurs when a request to Apply the configuration is issued.
    /// </summary>
    public event EventHandler ApplyConfiguration;

    private void OnReloadButtonClicked(object sender, RoutedEventArgs e) {
      OnReloadConfiguration(e);
      levelConfigurations.Clear();
      // Force update of current property
      SetLevel(Level);
    }

    private void OnApplyButtonClicked(object sender, RoutedEventArgs e) {
      OnApplyConfiguration(e);
    }

    #endregion

    #region Level dependency property

    public static readonly DependencyProperty LevelProperty = DependencyProperty.Register("Level", typeof(int),
        typeof(SubtreePlacerPanel), new FrameworkPropertyMetadata(0, OnLevelChanged, CoerceLevel));

    public int Level {
      get => (int) GetValue(LevelProperty);
      set => SetValue(LevelProperty, value);
    }

    private static object CoerceLevel(DependencyObject d, object basevalue) {
      var level = (int) basevalue;
      var npp = (SubtreePlacerPanel) d;
      if (level < 0) {
        return 0;
      }
      if (level >= npp.SubtreePlacers.Count) {
        return npp.SubtreePlacers.Count - 1;
      }
      return level;
    }

    private static void OnLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      var npp = (SubtreePlacerPanel) d;
      npp.SetLevel((int) e.NewValue);
    }

    internal void SetLevel(int level) {
      levelConfigurations.Clear();
      Level = level;
      CurrentDescriptor = SubtreePlacers[level];
      layerVisualizationBorder.Background = CurrentBrush;
      subtreePlacerTypeComboBox.SelectedItem = CurrentDescriptor.Name;
    }

    #endregion

    #region CurrentDescriptor dependency property

    public static readonly DependencyProperty CurrentDescriptorProperty = DependencyProperty.Register(
        "CurrentDescriptor", typeof(SubtreePlacerDescriptor), typeof(SubtreePlacerPanel),
        new FrameworkPropertyMetadata(OnDescriptorChanged));

    public SubtreePlacerDescriptor CurrentDescriptor {
      get => (SubtreePlacerDescriptor) GetValue(CurrentDescriptorProperty);
      set => SetValue(CurrentDescriptorProperty, value);
    }

    private static void OnDescriptorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      var npp = d as SubtreePlacerPanel;
      if (npp != null) {
        npp.SetDescriptor((SubtreePlacerDescriptor) e.NewValue);
      }
    }

    private void SetDescriptor(SubtreePlacerDescriptor npd) {
      SubtreePlacers[Level] = npd;
      if (descriptorCollection.Count == 0) {
        descriptorCollection.Add(npd.Configuration);
      } else {
        descriptorCollection[0] = npd.Configuration;
      }
      rotationGrid.IsEnabled = npd.Rotatable;
      if (npd.Configuration != null) {
        selectionProvider.UpdatePropertyViewsNow();
        editorControl.Visibility = Visibility.Visible;
      } else {
        editorControl.Visibility = Visibility.Hidden;
      }
      UpdatePreview();
    }

    #endregion

    #region Option handler setup

    /// <summary>
    ///   Configure option handler related stuff
    /// </summary>
    private void SetupSubtreePlacerOptions() {
      descriptorCollection = new List<ISubtreePlacerConfiguration>();
      selectionProvider =
          new DefaultSelectionProvider<ISubtreePlacerConfiguration>(descriptorCollection, delegate { return true; }) {
              ContextLookupDelegate = (subject, type) =>
                  type == typeof(IPropertyMapBuilder) && subject is ISubtreePlacerConfiguration
                      ? new AttributeBasedPropertyMapBuilderAttribute().CreateBuilder(subject.GetType())
                      : null
          };
      // when the selection content changes, trigger this action (usually rebuild associated option handlers)
      selectionProvider.SelectedItemsChanged += SelectionProviderSelectionChanged;

      editorControl.OptionHandler = new OptionHandler("SubtreePlacerOptions");
      editorControl.IsAutoAdopt = true;
      editorControl.IsAutoCommit = true;
      selectionProvider.PropertyItemsChanged += delegate {
        UpdatePreview();
        SelectionProviderSelectionChanged(this, null);
      };
    }


    /// <summary>
    ///   Update the properties when the current subtree placer descriptor has changed.
    /// </summary>
    private void SelectionProviderSelectionChanged(object sender, EventArgs e) {
      // We just rebuild the option handler from scratch
      editorControl.OptionHandler.BuildFromSelection(selectionProvider,
          (subject, type) => type == typeof(IOptionBuilder) && subject is ISubtreePlacerConfiguration
              ? new SortableOptionBuilder()
              : null);
    }

    #endregion

    #region Preview configuration and update

    /// <summary>
    ///   Creates and configures the graph that is used for the placer configuration preview.
    /// </summary>
    private void SetupPreview() {
      var graph = previewControl.Graph;
      previewLayoutData = new TreeLayoutData<INode, IEdge, ILabel, ILabel>();
      graph.NodeDefaults.Size = new SizeD(40, 30);
      DemoStyles.InitDemoStyles(graph, Themes.Palette58);
      var rootStyle = DemoStyles.CreateDemoNodeStyle(Themes.PaletteRed);
      var assistantStyle = NewAssistantStyle(Themes.Palette58);
      var root = graph.CreateNode();
      graph.SetStyle(root, rootStyle);
      var n1 = graph.CreateNode();
      graph.SetStyle(n1, assistantStyle);
      previewLayoutData.AssistantNodes.Item = n1;
      var n2 = graph.CreateNode();
      var n3 = graph.CreateNode();
      var n4 = graph.CreateNode();
      var n5 = graph.CreateNode(new RectD(0, 0, 60, 30));
      graph.CreateEdge(root, n1);
      graph.CreateEdge(root, n2);
      graph.CreateEdge(root, n3);
      graph.CreateEdge(root, n4);
      graph.CreateEdge(root, n5);
      previewLayout = new TreeLayout();
    }

    internal static INodeStyle NewAssistantStyle(Palette palette) {
      var style = DemoStyles.CreateDemoNodeStyle(palette);
      var pen = style.Pen.Clone();
      pen.DashStyle = DashStyles.Dash;
      style.Pen = (Pen) pen.GetAsFrozen();
      return style;
    }

    /// <summary>
    ///   Update the preview canvas: Apply a new layout with the current placer.
    /// </summary>
    public void UpdatePreview() {
      var placer = CurrentDescriptor.Configuration != null
          ? CurrentDescriptor.Configuration.CreateSubtreePlacer()
          : null;
      previewLayout.DefaultSubtreePlacer = placer ?? new SingleLayerSubtreePlacer();
      try {
        previewControl.Graph.ApplyLayout(previewLayout, previewLayoutData);
        previewControl.FitGraphBounds();
      } catch (Exception) { }
    }

    #endregion

    /// <summary>
    ///   Switch the options whenever a new subtree placer type is selected
    /// </summary>
    /// <remarks>If we didn't change the level, try to retrieve a previously set configuration, otherwise, start from scratch</remarks>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SubtreePlacerComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e) {
      var name = subtreePlacerTypeComboBox.SelectedItem as string;
      if (name != null) {
        if (CurrentDescriptor.Name != (string) subtreePlacerTypeComboBox.SelectedItem) {
          // Check if we already have a configuration for this level...
          SubtreePlacerDescriptor desc;
          if (levelConfigurations.TryGetValue(name, out desc)) {
            CurrentDescriptor = desc;
          } else {
            // Load from descriptor configurations... we use reflection
            foreach (var propertyInfo in typeof(SubtreePlacerConfigurations).GetProperties(BindingFlags.Static |
                BindingFlags.Public)) {
              var d = propertyInfo.GetValue(null, null) as SubtreePlacerDescriptor;
              if (d != null && d.Name == name) {
                CurrentDescriptor = d;
                levelConfigurations[name] = d;
                break;
              }
            }
          }
        }
      }
    }

    private Brush CurrentBrush => LayerPalettes[Level % LayerPalettes.Length].Fill;


    public static readonly Palette[] LayerPalettes = {
        Themes.PaletteRed, Themes.PaletteOrange, Themes.Palette22, Themes.PaletteGreen, Themes.Palette21,
        Themes.PaletteLightblue
    };

    #region Rotation Buttons

    private void LeftRotateButtonClicked(object sender, RoutedEventArgs e) {
      var rotablePlacer = CurrentDescriptor.Configuration as IRotatableSubtreePlacerConfiguration;
      if (rotablePlacer != null) {
        // Rotate the placer 90 degrees counter clockwise - we modify the modification matrix of the placer configuration
        rotablePlacer.SetTransform(rotablePlacer.Transform.Multiply(SubtreeTransform.RotateRight));
        UpdatePreview();
      }
    }

    private void RightRotateButtonClicked(object sender, RoutedEventArgs e) {
      var rotablePlacer = CurrentDescriptor.Configuration as IRotatableSubtreePlacerConfiguration;
      if (rotablePlacer != null) {
        // Rotate the placer 90 degrees clockwise - we modify the modification matrix of the placer configuration
        rotablePlacer.SetTransform(rotablePlacer.Transform.Multiply(SubtreeTransform.RotateLeft));
        UpdatePreview();
      }
    }

    private void MirrorHorizButtonClicked(object sender, RoutedEventArgs e) {
      var rotablePlacer = CurrentDescriptor.Configuration as IRotatableSubtreePlacerConfiguration;
      if (rotablePlacer != null) {
        // Mirror the placer horizontally - we modify the modification matrix of the placer configuration
        rotablePlacer.SetTransform(rotablePlacer.Transform.Multiply(SubtreeTransform.FlipY));
        UpdatePreview();
      }
    }

    private void MirrorVertButtonClicked(object sender, RoutedEventArgs e) {
      var rotablePlacer = CurrentDescriptor.Configuration as IRotatableSubtreePlacerConfiguration;
      if (rotablePlacer != null) {
        // Mirror the placer vertically - we modify the modification matrix of the placer configuration
        rotablePlacer.SetTransform(rotablePlacer.Transform.Multiply(SubtreeTransform.FlipX));
        UpdatePreview();
      }
    }

    #endregion
  }

  /// <summary>
  ///   Convert a <see langword="null"/> value into the hidden visibility of an <see cref="UIElement"/>.
  /// </summary>
  /// <remarks>
  ///   This is used to completely hide the configuration panels for the <see cref="SubtreePlacerConfigurations.None"/>
  ///   configuration.
  /// </remarks>
  internal class DescriptorVisibilityConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return value == null || value is NonePlacerConfiguration ? Visibility.Hidden : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }

    #endregion
  }

  /// <summary>
  ///   Converts the internal 0-based index to/from a 1-based
  /// </summary>
  internal class LevelConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var i = (int) value;
      return i + 1;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      var s = (string) value;
      var i = int.Parse(s);
      return i - 1;
    }
  }

  /// <summary>
  ///   Custom option builder that allows to reorder the displayed properties based on an additional sort criterion (i.e. not
  ///   alphabetically)
  /// </summary>
  internal class SortableOptionBuilder : AttributeBasedOptionBuilder
  {
    protected override PropertyInfo[] SortProperties(IOptionBuilderContext context, PropertyInfo[] properties) {
      Array.Sort(properties, new MyPropertyInfoComparer());
      return properties;
    }

    #region Nested type: MyPropertyInfoComparer

    private class MyPropertyInfoComparer : IComparer<PropertyInfo>
    {
      #region IComparer<PropertyInfo> Members

      public int Compare(PropertyInfo x1, PropertyInfo x2) {
        var oia1 = GetAttributes<OptionItemAttributeAttribute>(x1).FirstOrDefault(attr =>
            ((OptionItemAttributeAttribute) attr).Name == "OptionItem.Index") as OptionItemAttributeAttribute;
        var oia2 = GetAttributes<OptionItemAttributeAttribute>(x2).FirstOrDefault(attr =>
            ((OptionItemAttributeAttribute) attr).Name == "OptionItem.Index") as OptionItemAttributeAttribute;

        if (oia1 != null && oia2 != null) {
          return ((int) oia1.Value).CompareTo((int) oia2.Value);
        }
        var s1 = x1.Name;
        var s2 = x2.Name;
        return s1.CompareTo(s2);
      }

      #endregion

      private static IEnumerable<Attribute> GetAttributes<T>(PropertyInfo info) where T : Attribute {
        return Attribute.GetCustomAttributes(info, typeof(T));
      }
    }

    #endregion
  }
}

internal static class TransformationHelper
{
  internal static SubtreeTransform Multiply(this SubtreeTransform a, SubtreeTransform b) {
    switch (a) {
      case SubtreeTransform.None:
        return b;
      case SubtreeTransform.RotateLeft:
        switch (b) {
          case SubtreeTransform.RotateLeft:
            return SubtreeTransform.Rotate180;
          case SubtreeTransform.RotateRight:
            return SubtreeTransform.None;
          case SubtreeTransform.FlipY:
            return SubtreeTransform.RotateLeftFlipY;
          case SubtreeTransform.FlipX:
            return SubtreeTransform.RotateRightFlipY;
        }
        break;
      case SubtreeTransform.RotateRight:
        switch (b) {
          case SubtreeTransform.RotateLeft:
            return SubtreeTransform.None;
          case SubtreeTransform.RotateRight:
            return SubtreeTransform.Rotate180;
          case SubtreeTransform.FlipY:
            return SubtreeTransform.RotateRightFlipY;
          case SubtreeTransform.FlipX:
            return SubtreeTransform.RotateLeftFlipY;
        }
        break;
      case SubtreeTransform.Rotate180:
        switch (b) {
          case SubtreeTransform.RotateLeft:
            return SubtreeTransform.RotateRight;
          case SubtreeTransform.RotateRight:
            return SubtreeTransform.RotateLeft;
          case SubtreeTransform.FlipY:
            return SubtreeTransform.FlipX;
          case SubtreeTransform.FlipX:
            return SubtreeTransform.FlipY;
        }
        break;
      case SubtreeTransform.FlipY:
        switch (b) {
          case SubtreeTransform.RotateLeft:
            return SubtreeTransform.RotateRightFlipY;
          case SubtreeTransform.RotateRight:
            return SubtreeTransform.RotateLeftFlipY;
          case SubtreeTransform.FlipY:
            return SubtreeTransform.None;
          case SubtreeTransform.FlipX:
            return SubtreeTransform.Rotate180;
        }
        break;
      case SubtreeTransform.FlipX:
        switch (b) {
          case SubtreeTransform.RotateLeft:
            return SubtreeTransform.RotateLeftFlipY;
          case SubtreeTransform.RotateRight:
            return SubtreeTransform.RotateRightFlipY;
          case SubtreeTransform.FlipY:
            return SubtreeTransform.Rotate180;
          case SubtreeTransform.FlipX:
            return SubtreeTransform.None;
        }
        break;
      case SubtreeTransform.RotateLeftFlipY:
        switch (b) {
          case SubtreeTransform.RotateLeft:
            return SubtreeTransform.FlipY;
          case SubtreeTransform.RotateRight:
            return SubtreeTransform.FlipX;
          case SubtreeTransform.FlipY:
            return SubtreeTransform.RotateLeft;
          case SubtreeTransform.FlipX:
            return SubtreeTransform.RotateRight;
        }
        break;
      case SubtreeTransform.RotateRightFlipY:
        switch (b) {
          case SubtreeTransform.RotateLeft:
            return SubtreeTransform.FlipX;
          case SubtreeTransform.RotateRight:
            return SubtreeTransform.FlipY;
          case SubtreeTransform.FlipY:
            return SubtreeTransform.RotateRight;
          case SubtreeTransform.FlipX:
            return SubtreeTransform.RotateLeft;
        }
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(a), a, null);
    }

    throw new ArgumentOutOfRangeException(nameof(b), b, null);
  }
}
