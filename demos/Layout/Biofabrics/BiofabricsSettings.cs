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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace yWorks.Demo.Biofabrics
{
  public enum DataSetName
  {
    CompanyNetwork,
    LogisticsNetwork,
    TradingGraph
  }

  public sealed record SettingOption<T>(T Value, string Label) where T : struct, Enum;

  public sealed class BiofabricsSettings : INotifyPropertyChanged
  {
    private bool suppressChangeNotification;
    private DataSetName selectedDataset = DataSetName.CompanyNetwork;
    private EdgeColorMode edgeColorMode = EdgeColorMode.NodeGroups;
    private NodeWidthMode nodeWidthMode = NodeWidthMode.Full;
    private EdgeOrderingKey edgeOrdering = EdgeOrderingKey.EdgeLength;
    private NodeOrderingKey nodeOrdering = NodeOrderingKey.Degree;
    private bool edgeGrouping = true;
    private EdgeGroupOrderingKey edgeGroupOrdering = EdgeGroupOrderingKey.CardinalityDescending;
    private bool nodeGrouping = true;
    private NodeGroupOrderingKey nodeGroupOrdering = NodeGroupOrderingKey.CardinalityDescending;
    private bool isEnabled = true;
    private string datasetTitle = string.Empty;
    private string datasetDescription = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler<SettingChangedEventArgs>? SettingChanged;

    public IReadOnlyList<SettingOption<DataSetName>> DatasetOptions { get; } = [
        new(DataSetName.CompanyNetwork, "The Company Network"),
        new(DataSetName.LogisticsNetwork, "The Logistics Network"),
        new(DataSetName.TradingGraph, "The Trading Network")
    ];

    public IReadOnlyList<SettingOption<EdgeColorMode>> EdgeColorModeOptions { get; } = [
        new(EdgeColorMode.NodeGroupsInverted, "By Node Groups (source-target inverted)"),
        new(EdgeColorMode.NodeGroups, "By Node Groups"),
        new(EdgeColorMode.EdgeGroups, "By Edge Groups"),
        new(EdgeColorMode.None, "None")
    ];

    public IReadOnlyList<SettingOption<NodeWidthMode>> NodeWidthModeOptions { get; } = [
        new(NodeWidthMode.Full, "Full-width"),
        new(NodeWidthMode.Stairs, "To Last Port"),
        new(NodeWidthMode.Compact, "Compact")
    ];

    public IReadOnlyList<SettingOption<EdgeOrderingKey>> EdgeOrderingOptions { get; } = [
        new(EdgeOrderingKey.EdgeLength, "Edge Length"),
        new(EdgeOrderingKey.LexicographicalDescending, "Lexicographical Descending"),
        new(EdgeOrderingKey.LexicographicalAscending, "Lexicographical Ascending")
    ];

    public IReadOnlyList<SettingOption<NodeOrderingKey>> NodeOrderingOptions { get; } = [
        new(NodeOrderingKey.Degree, "Degree"),
        new(NodeOrderingKey.LexicographicalDescending, "Lexicographical Descending"),
        new(NodeOrderingKey.LexicographicalAscending, "Lexicographical Ascending")
    ];

    public IReadOnlyList<SettingOption<EdgeGroupOrderingKey>> EdgeGroupOrderingOptions { get; } = [
        new(EdgeGroupOrderingKey.CardinalityDescending, "Cardinality Descending"),
        new(EdgeGroupOrderingKey.CardinalityAscending, "Cardinality Ascending"),
        new(EdgeGroupOrderingKey.LexicographicalDescending, "Lexicographical Descending"),
        new(EdgeGroupOrderingKey.LexicographicalAscending, "Lexicographical Ascending")
    ];

    public IReadOnlyList<SettingOption<NodeGroupOrderingKey>> NodeGroupOrderingOptions { get; } = [
        new(NodeGroupOrderingKey.CardinalityDescending, "Cardinality Descending"),
        new(NodeGroupOrderingKey.CardinalityAscending, "Cardinality Ascending"),
        new(NodeGroupOrderingKey.LexicographicalDescending, "Lexicographical Descending"),
        new(NodeGroupOrderingKey.LexicographicalAscending, "Lexicographical Ascending")
    ];

    public DataSetName SelectedDataset {
      get => selectedDataset;
      set => SetField(ref selectedDataset, value);
    }

    public EdgeColorMode EdgeColorMode {
      get => edgeColorMode;
      set => SetField(ref edgeColorMode, value);
    }

    public NodeWidthMode NodeWidthMode {
      get => nodeWidthMode;
      set => SetField(ref nodeWidthMode, value);
    }

    public EdgeOrderingKey EdgeOrdering {
      get => edgeOrdering;
      set => SetField(ref edgeOrdering, value);
    }

    public NodeOrderingKey NodeOrdering {
      get => nodeOrdering;
      set => SetField(ref nodeOrdering, value);
    }

    public bool EdgeGrouping {
      get => edgeGrouping;
      set {
        if (SetField(ref edgeGrouping, value, notifySettings: true)) {
          OnPropertyChanged(nameof(IsEdgeGroupOrderingEnabled));
        }
      }
    }

    public EdgeGroupOrderingKey EdgeGroupOrdering {
      get => edgeGroupOrdering;
      set => SetField(ref edgeGroupOrdering, value);
    }

    public bool NodeGrouping {
      get => nodeGrouping;
      set {
        if (SetField(ref nodeGrouping, value, notifySettings: true)) {
          OnPropertyChanged(nameof(IsNodeGroupOrderingEnabled));
        }
      }
    }

    public NodeGroupOrderingKey NodeGroupOrdering {
      get => nodeGroupOrdering;
      set => SetField(ref nodeGroupOrdering, value);
    }

    public bool IsEnabled {
      get => isEnabled;
      set {
        if (SetField(ref isEnabled, value, notifySettings: false)) {
          OnPropertyChanged(nameof(IsEdgeGroupOrderingEnabled));
          OnPropertyChanged(nameof(IsNodeGroupOrderingEnabled));
        }
      }
    }

    public bool IsEdgeGroupOrderingEnabled => IsEnabled && EdgeGrouping;

    public bool IsNodeGroupOrderingEnabled => IsEnabled && NodeGrouping;

    public string DatasetTitle {
      get => datasetTitle;
      private set => SetField(ref datasetTitle, value, notifySettings: false);
    }

    public string DatasetDescription {
      get => datasetDescription;
      private set => SetField(ref datasetDescription, value, notifySettings: false);
    }

    /// <summary>
    ///   Applies the dataset's string enum values while keeping the interaction panel bound to enum instances.
    /// </summary>
    public void ApplyDefaults(DataSetName datasetName, DatasetDefaults defaults) {
      suppressChangeNotification = true;
      try {
        SelectedDataset = datasetName;
        EdgeColorMode = Enum.Parse<EdgeColorMode>(defaults.EdgeColorModePicker);
        NodeWidthMode = Enum.Parse<NodeWidthMode>(defaults.NodeWidthPicker);
        NodeOrdering = Enum.Parse<NodeOrderingKey>(defaults.NodeSortingPicker);
        EdgeOrdering = Enum.Parse<EdgeOrderingKey>(defaults.EdgeSortingPicker);
        EdgeGrouping = defaults.EdgeGroupingPicker;
        EdgeGroupOrdering = Enum.Parse<EdgeGroupOrderingKey>(defaults.EdgeGroupSortingPicker);
        NodeGrouping = defaults.NodeGroupingPicker;
        NodeGroupOrdering = Enum.Parse<NodeGroupOrderingKey>(defaults.NodeGroupSortingPicker);
      } finally {
        suppressChangeNotification = false;
      }
    }

    public void SetDatasetDescription(string title, string description) {
      DatasetTitle = title;
      DatasetDescription = description;
    }

    private bool SetField<T>(
        ref T field,
        T value,
        [CallerMemberName] string propertyName = "",
        bool notifySettings = true) {
      if (EqualityComparer<T>.Default.Equals(field, value)) {
        return false;
      }
      field = value;
      OnPropertyChanged(propertyName);
      if (notifySettings && !suppressChangeNotification) {
        SettingChanged?.Invoke(this, new SettingChangedEventArgs(propertyName));
      }
      return true;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }

  public sealed class SettingChangedEventArgs(string propertyName) : EventArgs
  {
    public string PropertyName { get; } = propertyName;
  }
}
