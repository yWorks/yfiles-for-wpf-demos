# 08 Input and Output

This demo shows how to use GraphML I/O functionality. 
  

GraphML is the standard file format for yFiles WPF. It is an XML format 
  that allows for great flexibility when storing custom data. However, note that 
  these attributes (such as styles or even node locations) are not standardized,
  so you probably won't be able to exchange all of them between different graph 
  libraries, for example.

Class `GraphControl` already comes with several convenience 
  methods for reading and writing GraphML (see the various `ImportFromGraphML` 
  and `ExportToGraphML` overloads. If you want to customize your I/O process 
  (e.g. to write additional custom data), please see class 
  `GraphMLIOHandler` 
  (custom data IO is shown later in this tutorial).


  The necessary CommandBindings are already available (but disabled by default),
  so typically all that needs to be done is to set
  `GraphControl.FileOperationsEnabled` to true.
