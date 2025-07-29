# 04 Customizing Styles

This demo shows how to configure the visual appearance of graph elements (using so called styles).



  The visual appearance for each type of graph elements (apart from edge bends) can be
  customized through implementations of `IVisualStyle&lt;TModelItem&gt;`s. You can either
  set a default style through the `IGraph.NodeDefaults` and `IGraph.EdgeDefaults`
  properties on `IGraph`, 
  which takes effect for all **newly created** elements, or set a specific style for a graph 
  element, either at creation time with the various `Create/AddXXX` methods and extension methods, or by calling 
  one of the `IGraph.SetStyle` methods.
  

yFiles WPF already comes with a number of useful styles for most graph element types. 
  Creating your own custom style is not part of this tutorial, please refer to the various 
  additional sample demos and the yFiles WPF Developer's Guide.
