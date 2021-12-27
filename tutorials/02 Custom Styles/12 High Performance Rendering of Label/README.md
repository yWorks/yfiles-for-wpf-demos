# 12 High Performance Rendering of Label

Shows how to implement high-performance rendering for labels.


To do this, you need to implement UpdateVisual() which is called when
the Container decides to update the visual representation of a label.
In contrast to CreateVisual(), we try to re-use the old visual instead
of creating a new one.



The RenderDataCache class saves the relevant data for creating a visual.
UpdateVisual() checks whether this data has changed. If it hasn't changed,
the old visual can be returned, otherwise the whole or part of the
visual has to be re-rendered.
