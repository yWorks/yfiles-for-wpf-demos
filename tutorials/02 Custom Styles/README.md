
# 02 Custom Styles
This tutorial is a step-by-step introduction to customizing the visual representation of graph elements. It is intended for advanced users that need to create custom styles from scratch. yFiles WPF already provides a number of styles that can be used for complex visualizations, using standard WPF techniques. <br /> The tutorial shows you how to create custom styles for nodes, edges, labels, and ports. It also presents how to create a custom arrowhead rendering, how to customize edge selection, and how the visual representation of graph elements can be changed depending on application state. Furthermore, the tutorial discusses several optimization strategies. 

If you are new to styles, respectively their customization, it is recommended to start by going through the projects in this tutorial one by one. To make full use of the tutorial, it is also recommended to review and possibly modify the source code for each sample project. 



You will find the following programming samples in this package: 


| Name | Description |
|:---|:---|
|**01 Custom Node Style** | Shows how to create a custom node style. |
|**02 Node Color** | Shows how to create properties so that the style can be changed dynamically. |
|**03 UpdateVisual and RenderDataCache** | Shows how to implement high-performance rendering for nodes. |
|**04 IsInside** | Shows how to override IsInside() and GetIntersection() in SimpleAbstractNodeStyle&lt;TVisual&gt;. |
|**05 Hit Test** | Shows how to override IsHit() and IsInBox() in SimpleAbstractNodeStyle&lt;TVisual&gt;. |
|**06 GetBounds** | Shows how to override the SimpleAbstractNodeStyle&lt;TVisual&gt;.GetBounds() method. |
|**07 Dropshadow Performance** | Shows how to pre-render the drop shadow of nodes in order to improve the rendering performance in comparison to the built-in effect. |
|**08 Edge from Node to Label** | Shows how to visually connect a node to its label(s) by means of edges. |
|**09 IsVisible** | Shows how to override the IsVisible() method of SimpleAbstractNodeStyle&lt;TVisual&gt;. |
|**10 Custom Label Style** | Shows how to create a custom label style. |
|**11 Label Preferred Size** | Shows how to override the GetPreferredSize() method to  set the size of the label dependent on the size of its text. |
|**12 High Performance Rendering of Label** | Shows how to implement high-performance rendering for labels. |
|**13 Label Edit Button** | Shows how to implement a button within a label to open the label editor. |
|**14 Button Visibility** | Shows how to hide the button dependent on the zoom level. |
|**15 Using Data in Label Tag** | Shows how to use data from a business object, which is stored in the label's tag, for rendering. |
|**16 Custom Edge Style** | Shows how to create a custom edge style which allows to specify the edge thickness by setting a property on the style. |
|**17 Edge HitTest** | Shows how to take the thickness of the edge into account when checking if the edge was clicked. |
|**18 Edge Cropping** | Shows how to crop an edge at the node bounds. |
|**19 Animated Edge Selection** | Shows how to change the style of an edge if the edge is selected. |
|**20 Custom Arrow** | Shows how to create a custom arrow. |
|**21 Arrow Thickness** | Shows how to render the arrow dependent on a property of the edge it belongs to. |
|**22 Custom Ports** | Shows how to implement a custom port style. |
|**23 Style Decorator** | Shows how to create a style that decorates an existing style. |
|**24 Zoom Invariant Label Rendering** | Shows how to implement a zoom invariant label style. |
|**25 Bridge Support** | Shows how to enable bridges for a custom edge style. |

## Running the Demos

### With Visual Studio

* To load all samples into Visual Studio you can simply open the solution file yFiles Tutorials.sln. 
* To load a single sample into Visual Studio you can open the project file (.csproj) in the sample's directory. 




#### See also
[Product Page](https://www.yworks.com/products/yfileswpf)  
[API Documentation](https://docs.yworks.com/yfileswpf)    
[Help and Support](https://www.yworks.com/products/yfiles/support)


#### Contact
yWorks GmbH  
Vor dem Kreuzberg 28  
72070 Tuebingen  
Germany  
Phone: +49 7071 979050
Email: contact@yworks.com

COPYRIGHT &#x00A9; 2021 yWorks   


