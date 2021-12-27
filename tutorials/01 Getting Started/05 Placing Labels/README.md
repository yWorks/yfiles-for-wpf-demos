# 05 Placing Labels

This demo shows how to control label placement with the help of so called label model parameters.

Usually, label positions are not specified through explicit (absolute or relative) 
  coordinates. Instead, so called `ILabelModelParameter`s are used instead, that 
  encode a specific symbolic position in a specific `ILabelModel`.
  So for example, `InteriorLabelModel.NorthWest` encodes a label position in the 
  upper left corner inside the `INode` that owns the label, without having to explicitly 
  determine the coordinates yourself.
  


  Label models are also used for interactive placement of labels (you can only drag to 
  valid positions in the given label model) as well as for the various automatic labeling algorithms.
