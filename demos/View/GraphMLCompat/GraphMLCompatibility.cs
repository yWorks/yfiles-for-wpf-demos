/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Windows.Markup;
using System.Xml.Linq;
using Demo.yFiles.IO.GraphML.Compat;
using Demo.yFiles.IO.GraphML.Compat.Xaml;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;
using yWorks.GraphML;
using yWorks.Markup.Common;
using yWorks.Markup.Platform;

// Assembly attributes to allow the framework XAML deserializer to find our types.
[assembly: XmlnsDefinition(GraphMLCompatibility.YfilesCommonNS20, "Demo.yFiles.IO.GraphML.Compat.Common")]
[assembly: XmlnsDefinition(GraphMLCompatibility.YfilesCommonMarkupNS20, "Demo.yFiles.IO.GraphML.Compat.CommonMarkup")]
[assembly: XmlnsDefinition(GraphMLCompatibility.YfilesWpfXamlNS20, "Demo.yFiles.IO.GraphML.Compat.Xaml")]

namespace Demo.yFiles.IO.GraphML.Compat
{
  /// <summary>
  /// Helper class to enable parsing GraphML files from previous versions of yFiles WPF.
  /// </summary>
  /// <remarks>
  /// For usage in your own applications, you can reference the demo and simply call
  /// the <see cref="ConfigureGraphMLCompatibility"/> method with your <see cref="GraphMLIOHandler"/>
  /// (or use the provided extension method on <see cref="GraphMLIOHandler"/>).
  /// </remarks>
  public static class GraphMLCompatibility
  {
    /// <summary>
    /// Configures a <see cref="GraphMLIOHandler"/> instance to read GraphML files written by
    /// previous versions of yFiles WPF.
    /// </summary>
    public static void ConfigureGraphMLCompatibility(GraphMLIOHandler handler) {
      // Register our three special namespaces containing MarkupExtensions that can then just be picked up by namespace and name
      // and don't have to be mapped manually (see below).
      // These mappings are akin to the assembly attributes at the top of this file.
      handler.AddXamlNamespaceMapping(YfilesCommonMarkupNS20, typeof(CommonMarkup.StaticExtension));
      handler.AddXamlNamespaceMapping(YfilesWpfXamlNS20, typeof(NodeScaledPortLocationModelExtension));
      handler.AddXamlNamespaceMapping(YfilesCommonNS20, typeof(Common.LabelExtension));

      // Provides a mapping between XML element names in specific yFiles WPF 2.5 namespaces
      // to either their equivalent in the current library version, or to specially-written
      // MarkupExtensions that surface the old API and map it to the correct instances in the new API.
      var mappings = new Dictionary<XName, Type> {
        // General graph stuff
        { XName.Get("Bend", YfilesCommonNS20), typeof(BendExtension) },
        { XName.Get("NodeViewState", YfilesCommonNS20), typeof(FolderNodeStateExtension) },
        { XName.Get("EdgeViewState", YfilesCommonNS20), typeof(FoldingEdgeStateExtension) },
        { XName.Get("GraphSettings", YfilesCommonNS20), typeof(GraphSettings) },
        { XName.Get("NodeDefaults", YfilesCommonNS20), typeof(NodeDefaults) },
        { XName.Get("EdgeDefaults", YfilesCommonNS20), typeof(EdgeDefaults) },
        { XName.Get("GraphMLReference", YfilesCommonNS20), typeof(GraphMLReferenceExtension) },

        // Void styles
        { XName.Get("VoidPortStyle", YfilesCommonNS20), typeof(VoidPortStyle) },
        { XName.Get("VoidNodeStyle", YfilesCommonNS20), typeof(VoidNodeStyle) },
        { XName.Get("VoidEdgeStyle", YfilesCommonNS20), typeof(VoidEdgeStyle) },
        { XName.Get("VoidLabelStyle", YfilesCommonNS20), typeof(VoidLabelStyle) },

        // Node styles
        { XName.Get("ShapeNodeStyle", YfilesWpfXamlNS20), typeof(ShapeNodeStyle) },
        { XName.Get("BevelNodeStyle", YfilesWpfXamlNS20), typeof(BevelNodeStyle) },
        { XName.Get("ImageNodeStyle", YfilesWpfXamlNS20), typeof(ImageNodeStyle) },
        { XName.Get("MemoryImageNodeStyle", YfilesWpfXamlNS20), typeof(MemoryImageNodeStyle) },
        { XName.Get("MemoryImage", YfilesWpfXamlNS20), typeof(MemoryImageExtension) },
        { XName.Get("ImageSource", YfilesWpfXamlNS20), typeof(ImageSourceExtension) },
        { XName.Get("GeneralPathNodeStyle", YfilesWpfXamlNS20), typeof(GeneralPathNodeStyle) },
        { XName.Get("GeneralPathMarkup", YfilesWpfXamlNS20), typeof(GeneralPathExtension) },
        { XName.Get("MoveTo", YfilesWpfXamlNS20), typeof(MoveTo) },
        { XName.Get("LineTo", YfilesWpfXamlNS20), typeof(LineTo) },
        { XName.Get("QuadTo", YfilesWpfXamlNS20), typeof(QuadTo) },
        { XName.Get("CubicTo", YfilesWpfXamlNS20), typeof(CubicTo) },
        { XName.Get("Close", YfilesWpfXamlNS20), typeof(Close) },
        { XName.Get("PanelNodeStyle", YfilesWpfXamlNS20), typeof(PanelNodeStyle) },
        { XName.Get("ShinyPlateNodeStyle", YfilesWpfXamlNS20), typeof(ShinyPlateNodeStyle) },
        { XName.Get("ShadowNodeStyleDecorator", YfilesWpfXamlNS20), typeof(ShadowNodeStyleDecorator) },
        { XName.Get("CollapsibleNodeStyleDecorator", YfilesWpfXamlNS20), typeof(CollapsibleNodeStyleDecoratorExtension) },
        { XName.Get("TableNodeStyle", YfilesWpfXamlNS20), typeof(TableNodeStyle) },

        // Port styles
        { XName.Get("NodeStylePortStyleAdapter", YfilesWpfXamlNS20), typeof(NodeStylePortStyleAdapter) },

        // Edge styles
        { XName.Get("PolylineEdgeStyle", YfilesWpfXamlNS20), typeof(PolylineEdgeStyleExtension) },
        { XName.Get("ArcEdgeStyle", YfilesWpfXamlNS20), typeof(ArcEdgeStyle) },

        // Label styles
        { XName.Get("NodeStyleLabelStyleAdapter", YfilesWpfXamlNS20), typeof(NodeStyleLabelStyleAdapter) },

        // Auxiliary style classes
        { XName.Get("Typeface", YfilesWpfXamlNS20), typeof(TypefaceExtension) },
        { XName.Get("Arrow", YfilesWpfXamlNS20), typeof(Arrow) },
        { XName.Get("DefaultArrow", YfilesWpfXamlNS20), typeof(Arrows) },
        { XName.Get("Table", YfilesCommonNS20), typeof(TableExtension) },
        { XName.Get("Row", YfilesCommonNS20), typeof(Common.RowExtension) },
        { XName.Get("Column", YfilesCommonNS20), typeof(Common.ColumnExtension) },

        // Label models and their parameters
        { XName.Get("FreeLabelModel", YfilesWpfXamlNS20), typeof(FreeLabelModel) },
        { XName.Get("FixedLabelModelParameter", YfilesWpfXamlNS20), typeof(FixedLabelModelParameterExtension) },
        { XName.Get("AnchoredLabelModelParameter", YfilesWpfXamlNS20), typeof(AnchoredLabelModelParameterExtension) },
        { XName.Get("CompositeLabelModel", YfilesWpfXamlNS20), typeof(CompositeLabelModel) },
        { XName.Get("CompositeLabelModelParameter", YfilesWpfXamlNS20), typeof(CompositeLabelModelParameterExtension) },
        { XName.Get("GenericModel", YfilesWpfXamlNS20), typeof(GenericLabelModelExtension) },
        { XName.Get("GenericLabelModelParameter", YfilesWpfXamlNS20), typeof(GenericLabelModelParameterExtension) },
        { XName.Get("DescriptorWrapperLabelModel", YfilesWpfXamlNS20), typeof(DescriptorWrapperLabelModel) },
        { XName.Get("DescriptorWrapperLabelModelParameter", YfilesWpfXamlNS20), typeof(DescriptorWrapperLabelModelParameterExtension) },

        { XName.Get("FreeNodeLabelModel", YfilesWpfXamlNS20), typeof(FreeNodeLabelModel) },
        { XName.Get("RatioAnchoredLabelModelParameter", YfilesWpfXamlNS20), typeof(RatioAnchoredLabelModelParameterExtension) },
        { XName.Get("SandwichLabelModel", YfilesWpfXamlNS20), typeof(SandwichLabelModel) },
        { XName.Get("SandwichParameter", YfilesWpfXamlNS20), typeof(SandwichParameterExtension) },
        { XName.Get("ExteriorLabelModel", YfilesWpfXamlNS20), typeof(ExteriorLabelModel) },
        { XName.Get("ExteriorLabelModelParameter", YfilesWpfXamlNS20), typeof(ExteriorLabelModelParameterExtension) },
        { XName.Get("InteriorLabelModel", YfilesWpfXamlNS20), typeof(InteriorLabelModel) },
        { XName.Get("InteriorLabelModelParameter", YfilesWpfXamlNS20), typeof(InteriorLabelModelParameterExtension) },
        { XName.Get("InteriorStretchLabelModel", YfilesWpfXamlNS20), typeof(InteriorStretchLabelModel) },
        { XName.Get("InteriorStretchLabelModelParameter", YfilesWpfXamlNS20), typeof(InteriorStretchLabelModelParameterExtension) },

        { XName.Get("NinePositionsEdgeLabelModel", YfilesWpfXamlNS20), typeof(NinePositionsEdgeLabelModel) },
        { XName.Get("NinePositionsEdgeLabelParameter", YfilesWpfXamlNS20), typeof(Xaml.NinePositionsEdgeLabelModelParameterExtension) },
        { XName.Get("FreeEdgeLabelModel", YfilesWpfXamlNS20), typeof(FreeEdgeLabelModel) },
        { XName.Get("FreeEdgeLabelModelParameter", YfilesWpfXamlNS20), typeof(FreeEdgeLabelModelParameterExtension) },
        { XName.Get("SmartEdgeLabelModel", YfilesWpfXamlNS20), typeof(SmartEdgeLabelModel) },
        { XName.Get("SmartEdgeLabelModelParameter", YfilesWpfXamlNS20), typeof(SmartEdgeLabelModelParameterExtension) },

        { XName.Get("BendAnchoredPortLocationModel", YfilesWpfXamlNS20), typeof(BendAnchoredPortLocationModel) },
        { XName.Get("BendAnchoredParameter", YfilesWpfXamlNS20), typeof(BendAnchoredParameterExtension) },
        { XName.Get("GenericPortLocationModel", YfilesWpfXamlNS20), typeof(Xaml.GenericPortLocationModelExtension) },
        { XName.Get("GenericPortLocationParameter", YfilesWpfXamlNS20), typeof(GenericPortLocationParameterExtension) },
        { XName.Get("SegmentRatioPortLocationModel", YfilesWpfXamlNS20), typeof(SegmentRatioPortLocationModel) },
        { XName.Get("SegmentRatioParameterParameter", YfilesWpfXamlNS20), typeof(SegmentRatioParameterExtension) },

        { XName.Get("StripeLabelModel", YfilesWpfXamlNS20), typeof(StripeLabelModel) },
        { XName.Get("StripeLabelModelParameter", YfilesWpfXamlNS20), typeof(StripeLabelModelParameterExtension) },
        { XName.Get("StretchStripeLabelModel", YfilesWpfXamlNS20), typeof(StretchStripeLabelModel) },
        { XName.Get("StretchStripeLabelModelParameter", YfilesWpfXamlNS20), typeof(StretchStripeLabelModelParameterExtension) }
      };
      handler.QueryType += (sender, args) => {
        Type result;
        if (mappings.TryGetValue(args.XmlName, out result)) {
          args.Result = result;
        }
      };
    }

    /// <summary>The namespace URI that is used by the yFiles XAML extensions.</summary>
    public const string YfilesWpfXamlNS20 = "http://www.yworks.com/xml/yfiles-wpf/2.0/xaml";
    /// <summary>The namespace URI for common yFiles extensions to GraphML.</summary>
    public const string YfilesCommonNS20 = "http://www.yworks.com/xml/yfiles-common/2.0";
    /// <summary>The namespace URI for common yFiles extensions to GraphML.</summary>
    public const string YfilesCommonMarkupNS20 = "http://www.yworks.com/xml/yfiles-common/markup/2.0";
  }

}

namespace yWorks.GraphML
{
  /// <summary>
  /// Contains an extension method on <see cref="GraphMLIOHandler"/> to allow parsing GraphML files from earlier versions of yFiles WPF.
  /// </summary>
  public static class GraphMLCompatibilityExtensions
  {
    /// <summary>
    /// Configures a <see cref="GraphMLIOHandler"/> instance to read GraphML files written by
    /// previous versions of yFiles WPF.
    /// </summary>
    public static void ConfigureGraphMLCompatibility(this GraphMLIOHandler handler) {
      GraphMLCompatibility.ConfigureGraphMLCompatibility(handler);
    }
  }
}
