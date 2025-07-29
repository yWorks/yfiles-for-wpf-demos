
# yFiles WPF Programming Samples Overview
The [yFiles WPF](https://www.yworks.com/products/yfileswpf) programming samples are extensive Visual Studio projects with **full C# source code** that present the functionality of the yFiles WPF diagramming library. All programming samples can be found in subdirectories of the current directory. Most samples are self-contained, i.e., they consist of a single Visual Studio project. 


## Working with the samples
All demos can be accessed by opening the solution file (yFiles Demos.sln) in this folder. 




### Upgrade instructions
If you currently have an active yFiles WPF subscription and license, and want to try out new (or old) demos, you have to place your existing license file in the `lib/net6.0` folder where it will be picked up when building demos in this package. 
### Required SDKs
The demos in this folder are targeted to the highest .NET version which is available on your current machine. You can change the framework in the project’s properties or the csproj file itself. The minimum required framework version is .NET 6.0. 

In contrast to the Visual Studio installer, JetBrains Rider may not install the required .NET SDK versions. To run the demos seamlessly in Rider, an installation of .NET Core / .NET may be needed. 


### Troubleshooting

#### To run this application, you must install .NET / Target framework not installed
The demos target a .NET SDK which is not installed on your development machine. 



Either install an SDK which supports the required target framework or update the target framework to a version which is already installed on your machine. 
#### NETSDK1005: Asset file is missing target
.NET 5.0 and higher require MSBuild 16.8 or higher. Most likely you are working with an older version of JetBrains Rider which uses its own MSBuild. 

Set the MSBuild version (Build,Execution,Deployment > Toolset and Build) to 16.8 or higher. An appropriate MSBuild.dll should be provided in the .NET SDK and automatically be detected by Rider. 


#### Security Warning when Opening a Demo or Tutorial Project in Visual Studio
Visual Studio might show a Security Warning when opening the demo or tutorial solution. This happens because the solution and project files were contained in a zip archive which was downloaded from the web. 

It is safe to open the projects, anyway. 


#### Untrusted Solution yFiles Demos in JetBrains Rider
In JetBrains Rider, each solution you open is untrusted by default. Therefore, you see this dialog which lets you choose to trust the solution or not. You can find more details [in the JetBrains Rider help](https://www.jetbrains.com/help/rider/Creating_and_Opening_Projects_and_Solutions.html#trusted-and-untrusted-solutions) 

The yFiles WPF demo project files contain a `PropertyGroup` that attempts to automatically detect the installed .NET SDK. This poses no security risk. 

Click the **Trust and Open** button to open the solution. 


### Running a Single Sample
To open a specific programming sample, the corresponding Visual Studio project file (usually named `[demoname].csproj`) can be found in the demo's directory, which also uses the `[demoname]` naming scheme. <br /> Note that samples that demonstrate related aspects are grouped together in solution folders. 

<a id="samples"></a> 
### Programming Samples Categories

| Category | Folder | Description |
|:---|:---|:---|
|[BPMN Demos](BPMN) |**BPMN** | Source code samples that demonstrate how to implement BPMN layout and editing features. |
|[Complete Demos](Complete) |**Complete** | Source code samples that demonstrate the interaction between the viewer component and the layout algorithms. |
|[Data Binding Demos](DataBinding) |**DataBinding** | Source code samples that demonstrate binding graph elements to business data. |
|[InputMode Demos](Input) |**Input** | Source code samples that demonstrate how to configure specific aspects of the InputMode functionality. |
|[Layout Demos](Layout) |**Layout** | Source code samples demonstrate the basic usage of the yFiles WPF layout algorithms. |
|[Layout Only Demos](LayoutOnly) |**LayoutOnly** | Source code samples that demonstrate the basic usage of the yFiles WPF layout algorithms **without** the viewer component. |
|[Style Demos](Style) |**Style** | Source code samples that demonstrate how to customize the visual representation of yFiles WPF graph elements. |
|[Tutorial - Getting Started](Tutorial%20-%20Getting%20Started) |**Tutorial - Getting Started** | This tutorial shows you both basic concepts and also how to build a diagramming application that supports custom styles, full user interaction, Undo/Redo, clipboard, I/O, grouping and folding. |
|[Tutorial - Styles](Tutorial%20-%20Styles) |**Tutorial - Styles** | This tutorial is a step-by-step introduction to customizing the visual representation of graph elements. It is intended for for advanced users that want to learn how to create custom styles from scratch. |
|[Utility Projects for Demos](Utils) |**Utils** | Utility projects for common functionality in demos. |
|[Graph Viewer Demos](View) |**View** | Source code samples demonstrate basic concepts related to the `GraphControl` component and the underlying graph model. |




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


