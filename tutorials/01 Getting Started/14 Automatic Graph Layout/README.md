# 14 Automatic Graph Layout

This demo shows how to use the layout algorithms in yFiles WPF to automatically
  place the graph elements.
  

To use the layout algorithms you have to reference the following
  assemblies in addition to `@assembly.Viewer@`:
  
- `@assembly.Algorithms@`: This assembly contains the actual layout
  and analysis algorithms.
- `@assembly.Adapter@`: This class is necessary to use the layout and
  analysis algorithms with the object model in the
  yFiles WPF viewer part.


  Note that this sample loads a sample graph from a file instead of creating it
  programmatically, since the graphs from the previous examples
  are not really suited for automatic layout.
