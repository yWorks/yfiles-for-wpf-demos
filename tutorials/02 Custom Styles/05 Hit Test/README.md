# 05 Hit Test

Shows how to override IsHit() and IsInBox() in SimpleAbstractNodeStyle<TVisual>.


IsHit() is used for mouse click detection. It should return true if the tested point is inside
the node. IsHit() should take into account the imprecision radius specified in the CanvasContext
(HitTestRadius).



IsInBox() is used for marquee detection. It should return true if the node intersects with the box
to test or lies completely inside. Also it should be true if the tested box lies completely inside the node.
