
# yFiles WPF Tutorials Overview
The [yFiles WPF](https://www.yworks.com/products/yfileswpf) tutorials are extensive Visual Studio projects with **full C# source code** that present the functionality of the yFiles WPF diagramming library. All tutorials can be found in subdirectories of the current directory. All tutorial steps are self-contained, i.e., they consist of a single Visual Studio project. 


## Working with the samples
All demos can be accessed by opening the solution file (yFiles Demos.sln) in this folder. 




### Required SDKs
The demos and tutorials in this folder are targeted to .NET Framework 4.8. You can change the framework in the project’s properties or the csproj file itself. The minimum required framework version is .NET Framework 4.5 (the former .NET 4.5). 

In contrast to the Visual Studio installer, JetBrains Rider does not install various .NET SDK versions. To run the demos seamlessly in Rider, an installation of .NET Framework 4.8 is needed. If this is not desired, the target in the demo projects needs to be adjusted to the available version. 


### Troubleshooting

#### To run this application, you must install .NET / Target framework not installed
The demos target a .NET framework which is not installed on your development machine. 

Either install an SDK which supports the required .NET framework or update the target framework to a version which is already installed on your machine. 


#### Untrusted Solution yFiles Demos in JetBrains Rider
In JetBrains Rider, each solution you open is untrusted by default. Therefore, you see this dialog which lets you choose to trust the solution or not. You can find more details [in the JetBrains Rider help](https://www.jetbrains.com/help/rider/Creating_and_Opening_Projects_and_Solutions.html#trusted-and-untrusted-solutions) 

The yFiles WPF demo project files contain a custom build target that automatically unblocks the yFiles WPF assemblies. This either uses PowerShell or cmd to unblock the files, but poses no known security risk. 

Click the **Trust and Open** button to open the solution. 


### Running a Single Sample
To open a specific programming sample, the corresponding Visual Studio project file (usually named `[demoname].csproj`) can be found in the demo's directory, which also uses the `[demoname]` naming scheme. <br /> Note that samples that demonstrate related aspects are grouped together in solution folders. 

<h3 id="samples">Available Tutorials

| Category | Folder | Description 
|:---|:---|:---
|[01 Getting Started](01%20Getting%20Started) |**01 Getting Started** | This tutorial shows you both basic concepts and also how to build a diagramming application that supports custom styles, full user interaction, Undo/Redo, clipboard, I/O, grouping and folding. 
|[02 Custom Styles](02%20Custom%20Styles) |**02 Custom Styles** | This tutorial is a step-by-step introduction to customizing the visual representation of graph elements. It is intended for for advanced users that want to learn how to create custom styles from scratch. 




#### See also
[Product Page](https://www.yworks.com/products/yfileswpf)  
[API Documentation](https://docs.yworks.com/yfileswpf)    
[Help and Support](https://www.yworks.com/products/yfiles/support)


#### Contact
yWorks GmbH  
Vor dem Kreuzberg 28  
72070 Tuebingen  
Germany  
Phone: +49 7071 979050
Email: contact@yworks.com

COPYRIGHT &#x00A9; 2021 yWorks   


