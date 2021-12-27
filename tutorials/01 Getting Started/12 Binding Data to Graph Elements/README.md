# 12 Binding Data to Graph Elements

This demo shows how to bind data to graph elements.
  

The usual method to bind arbitrary data to graph elements 
  is with the help of instances of the `IMapper{K,V}`
  interface. This demo uses class `DictionaryMapper{K,V}` 
  to store the creation time of each node. Also shown is
  how to display tool tips for these data items.
    

Note: Since we are using folding, there are actually two graph 
  instances where we could bind data to. This demo binds data to the items 
  in the model graph, since we want the data to be associated to each logical
  entity in the graph, independent of its presence in the current managed view. 
  However, since we want to access these values through instances of the managed view,
  we can't just use a simple map, but have to query the data through a symbolic name.

Note: An alternative approach would be to use the `ITagOwner` 
  interface of the `IModelItem`s to bind a single datum to 
  graph elements.

Note: Custom data persistence is shown later in this tutorial.
