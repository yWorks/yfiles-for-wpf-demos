# 13 GraphML IO for Custom Data

This demo shows how to read and write data that is bound to graph elements 
  to/from a graphml file.
  

In GraphML, data that is associated with graph elements is stored in
  `data` tags. Class `GraphMLIOHandler` provides several convenience 
  methods to create these tags from a given `IMapper{K,V}` instance, or to 
  read these data into a mapper instance.
