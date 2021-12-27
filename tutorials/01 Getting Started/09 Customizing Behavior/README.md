# 09 Customizing Behavior

This demo presents the `ILookup` interface. Here, it is used
  to customize the port handling, i.e. it is used to return a different set
  of ports that are available for interactive edge creation.
  

To change an edge to a different port (on the same node), just
  select the edge and drag its source or target handle to the new port. The
  valid port locations are highlighted when you start dragging. Note that this
  demo only allows you to reassign the edge to ports at the same node.
  

`ILookup` is a central concept in yFiles WPF which is used
  to customize many aspects of interaction and appearance. Please refer to the
  additional bundled sample demos as well as the yFiles WPF Developer's Guide
  for many more examples of how to use the `ILookup` pattern.
