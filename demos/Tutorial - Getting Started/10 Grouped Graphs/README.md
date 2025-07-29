# 10 Grouped Graphs

This demo shows how to enable support for grouped (or hierarchically organized) 
  graphs and  presents the default grouping interaction capabilities available
  in yFiles WPF. Note that collapse/expand functionality is introduced later in this tutorial.
  

`GraphEditorInputMode` already provides the following 
  default gestures for grouping/ungrouping:
  
- Press CTRL+G to group the currently selected nodes.
- Press CTRL+U to ungroup the currently selected nodes. Note that this 
  does not automatically shrink the group node or remove it if it would be empty.
- Press SHIFT+CTRL+G to shrink a group node to its minimum size.
- Press SHIFT when dragging nodes into or out of groups to change the graph 
  hierarchy.
