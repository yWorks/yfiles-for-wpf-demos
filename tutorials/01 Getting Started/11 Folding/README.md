# 11 Folding

This demo shows how to enable collapse/expand functionality for grouped graphs.
  This support is provided through class `FoldingManager`
  and its support classes.
  

`GraphEditorInputMode` already provides the following
  default gestures for collapse/expand:
  
- Press CTRL++ to open (expand) a closed group node.
- Press CTRL+- to close (collapse) an open group node.
- Press CTRL+Return to enter (navigate into) a group node.
- Press CTRL+Backspace to exit (navigate out of a group node.



Conceptually, folding is implemented by separating a graph model that
  always contains the full (unfolded) information from one or more views where
  the folding state is used to dynamically hide/unhide parts of the graph, create
  representatives for merged edges etc. Usually, the graph from such a "managed view"
  can be used transparently in place of the model graph, however, some care must be
  taken when working with graph elements that don't exist explicitly in the model
  graph, such as representative edges for multiple edges that connect into a folded
  group node.
