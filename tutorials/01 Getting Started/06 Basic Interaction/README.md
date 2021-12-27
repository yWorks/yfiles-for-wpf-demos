# 06 Basic Interaction

This demo shows the default interaction possibilities that are provided by class
  `GraphEditorInputMode`.
  

Interaction is handled by so called InputModes. `GraphEditorInputMode`
  is the main InputMode that already provides a large number of graph interaction possibilities, 
  such as moving, deleting, creating, resizing graph elements. 
  
- To select a single element, just click it with the mouse. Press SHIFT to step 
  through the different possible hits (e.g. to select a node label inside its owner). 
  To select multiple elements, either extend an existing selection by pressing CTRL while 
  clicking, or drag a selection rectangle over all graph elements that you want in your 
  selection. CTRL-A selects all elements.
- Resizing nodes is done through the handles that appear on selected nodes.
- To move a node or bend, just drag it when it is selected.
- To create an edge bend, click and drag the edge at the desired bend location.
- To create an edge, start dragging anywhere on the unselected source node and stop 
  dragging on the target node.
- Nodes may specify multiple port locations (by default, only a single port at the 
  node center exists). 
  You can either create an edge directly between these port, or later move the source or 
  target to a different port (just select the edge and drag the edge's source or target 
  handle). Note that custom port locations are not part of this tutorial step, but are 
  introduced later.
- To create or edit a label, just press F2 when the owner is selected.
- To move a label, just drag it to the desired location. Note that the valid 
  positions are determined by the label model for this label and show up as empty 
  rectangles when you start dragging the label. You can only move a label to one of 
  these positions.
