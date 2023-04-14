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
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Demo.yFiles.GraphEditor;
using Demo.yFiles.GraphEditor.Styles;
using System.Linq;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.GraphML;

namespace Demo.yFiles.Graph.Editor
{
  public partial class StylePalette
  {
    public StylePalette() {
      InitializeComponent();
    }

    public string DefaultsGraph { get; set; }

    protected virtual void OnLoaded(object source, EventArgs e) {
      this.Loaded -= OnLoaded;
      if (null != DefaultsGraph) {
        LoadDefaults(DefaultsGraph);
      }
    }

    private void LoadDefaults(string uri) {
      Style accordionStyle = (Style) Application.Current.Resources["AccordionItemStyle"];
      IMapper<string, HeaderedContentControl> accordionItems = new DictionaryMapper<string, HeaderedContentControl>();
      {
        var graph = new DefaultGraph();
        try {
          new GraphMLIOHandler().Read(graph, uri);
        } catch (Exception) {
          return;
        }
        List<StyleChooser> nodeChoosers = new List<StyleChooser>();
        foreach (var node in graph.Nodes) {
          string tag = (string) (node.Tag ?? "Other");

          if ("Dummy" == tag) {
            continue;
          }

          HeaderedContentControl accordionItem = accordionItems[tag];
          if (null == accordionItem) {
            accordionItem = new Expander(){Style = accordionStyle};
            accordionItem.Header = tag;
            accordionItems[tag] = accordionItem;
            StyleChooserAccordion.Items.Add(accordionItem);
            var styleChooser = new StyleChooser();
            styleChooser.ItemType = typeof (INodeStyle);
            styleChooser.ItemsSource = new ObservableCollection<INode>();
            styleChooser.SelectionChanged += delegate(object sender, SelectionChangedEventArgs args) {
                                               if (args.AddedItems.Count > 0) {
                                                 foreach (var n in nodeChoosers) {
                                                   if (n != sender) {
                                                     n.Deselect();
                                                   }
                                                 }
                                               }
                                             };
            styleChooser.SelectionChanged += OnSelectionChanged;

            nodeChoosers.Add(styleChooser);
            accordionItem.Content = styleChooser;
            styleChooser.HorizontalAlignment = HorizontalAlignment.Stretch;
          }

          StyleChooser chooser = (StyleChooser) accordionItem.Content;
          ObservableCollection<INode> itemSource = (ObservableCollection<INode>) chooser.ItemsSource;
          itemSource.Add(node);
        }
        var nodeChooser = nodeChoosers.FirstOrDefault();
        if (nodeChooser != null) {
          nodeChooser.SelectFirst();
        }

        HeaderedContentControl edgeTypesAccordionItem = new Expander() { Style = accordionStyle };
        edgeTypesAccordionItem.Header = "Edge Types";
        StyleChooserAccordion.Items.Add(edgeTypesAccordionItem);
        var edgeStyleChooser = new StyleChooser();
        edgeStyleChooser.ItemType = typeof (IEdgeStyle);
        var edgeStyles = new ObservableCollection<IEdgeStyle>();
        edgeStyleChooser.ItemsSource = edgeStyles;

        edgeStyleChooser.SelectionChanged += OnSelectionChanged;
        

        edgeTypesAccordionItem.Content = edgeStyleChooser;

        foreach (var edge in graph.Edges) {
          edgeStyles.Add(edge.Style);
        }

        edgeStyleChooser.SelectFirst();

        var labelTypesAccordionItem = new Expander() { Style = accordionStyle };
        labelTypesAccordionItem.Header = "Label Types";
        StyleChooserAccordion.Items.Add(labelTypesAccordionItem);
        var labelStyleChooser = new StyleChooser();
        labelStyleChooser.ItemType = typeof (ILabelStyle);
        var labelStyles = new ObservableCollection<ILabelStyle>();
        labelStyleChooser.ItemsSource = labelStyles;
        labelTypesAccordionItem.Content = labelStyleChooser;

        labelStyleChooser.SelectionChanged += OnSelectionChanged;

        foreach (var label in graph.Labels) {
          if (label.Owner is INode && Equals("Dummy", label.Owner.Tag)) {
            var style = label.Style;
            if (style is NodeStyleLabelStyleAdapter) {
              // unwrap from NodeStyleLabelStyleAdapter
              style = ((NodeStyleLabelStyleAdapter) style).LabelStyle;
            }

            // adjust the default flow direction of SimpleLabelStyles to the flowdirection of the current application.
            if (style is DefaultLabelStyle) {
              ((DefaultLabelStyle)style).FlowDirection = this.FlowDirection;
            }
            if (style is LabelStyle) {
              ((LabelStyle)style).FlowDirection = this.FlowDirection;
            }
            labelStyles.Add(label.Style);
          }
        }
        labelStyleChooser.SelectFirst();

        var portTypesAccordionItem = new Expander() { Style = accordionStyle };
        portTypesAccordionItem.Header = "Port Types";
        StyleChooserAccordion.Items.Add(portTypesAccordionItem);
        var portStyleChooser = new StyleChooser();
        portStyleChooser.ItemType = typeof(IPortStyle);
        var portStyles = new ObservableCollection<IPortStyle>();
        portStyleChooser.ItemsSource = portStyles;
        portTypesAccordionItem.Content = portStyleChooser;

        portStyleChooser.SelectionChanged += OnSelectionChanged;

        foreach (var port in graph.Ports) {
          if (Equals("Dummy", port.Owner.Tag) && port.Tag != null) {
            portStyles.Add(port.Style);
          }
        }
        // clear tags except for group nodes - no one needs them....
        foreach (var node in graph.Nodes) {
          if (!GraphEditorWindow.IsGroupNode(node)) {
            node.Tag = null;
          }
        }
        foreach (var edge in graph.Edges) {
          edge.Tag = null;
        }
      }

    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs) {
      StyleChooser source = (StyleChooser) sender;
      GraphEditorWindow.SetStyleDefaultCommand.Execute(source.SelectedItem, this);
      if (SelectionChanged != null) {
        SelectionChanged(this, selectionChangedEventArgs);
      }
    }

    public event SelectionChangedEventHandler SelectionChanged;
  }
}
