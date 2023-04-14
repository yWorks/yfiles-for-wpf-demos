
# yFiles WPF Programming Samples Overview
The [yFiles WPF](https://www.yworks.com/products/yfileswpf) programming samples are extensive Visual Studio projects with **full C# source code** that present the functionality of the yFiles WPF diagramming library. All programming samples can be found in subdirectories of the current directory. Most samples are self-contained, i.e., they consist of a single Visual Studio project. 


## Working with the samples
All demos can be accessed by opening the solution file (yFiles Demos.sln) in this folder. 




### Upgrade instructions
If you currently have an active yFiles WPF subscription and license, and want to try out new (or old) demos, you have to place your existing license file in the `lib/net40` folder where it will be picked up when building demos in this package. 
### Required SDKs
The demos and tutorials in this folder are targeted to .NET Framework 4.8. You can change the framework in the project’s properties or the csproj file itself. The minimum required framework version is .NET Framework 4.5. 

In contrast to the Visual Studio installer, JetBrains Rider may not install the required .NET SDK versions. To run the demos seamlessly in Rider, an installation of .NET Framework 4.8 may be needed. If this is not desired, the target framework in the demo projects needs to be adjusted to the available version. 


### Troubleshooting

#### To run this application, you must install .NET / Target framework not installed
The demos target a .NET Framework which is not installed on your development machine. 

Either install an SDK which supports the required .NET Framework or update the target framework to a version which is already installed on your machine. 


#### Security Warning when Opening a Demo or Tutorial Project in Visual Studio
Visual Studio might show a Security Warning when opening the demo or tutorial solution. This happens because the solution and project files were contained in a zip archive which was downloaded from the web. 

It is safe to open the projects, anyway. 


#### Untrusted Solution yFiles Demos in JetBrains Rider
In JetBrains Rider, each solution you open is untrusted by default. Therefore, you see this dialog which lets you choose to trust the solution or not. You can find more details [in the JetBrains Rider help](https://www.jetbrains.com/help/rider/Creating_and_Opening_Projects_and_Solutions.html#trusted-and-untrusted-solutions) 

The yFiles WPF demo project files contain a custom build target that automatically unblocks the yFiles WPF assemblies. This either uses PowerShell or cmd to unblock the files, but poses no known security risk. 

Click the **Trust and Open** button to open the solution. 


### Running a Single Sample
To open a specific programming sample, the corresponding Visual Studio project file (usually named `[demoname].csproj`) can be found in the demo's directory, which also uses the `[demoname]` naming scheme. <br /> Note that samples that demonstrate related aspects are grouped together in solution folders. 

<a id="samples"></a> 
### Programming Samples Categories

| Category | Folder | Description |
|:---|:---|:---|
|[BPMN Demos](BPMN) |**BPMN** | Source code samples that demonstrate how to implement BPMN layout and editing features. |
|[Compatibility Classes](Compatibility) |**Compatibility** | Support project to facilitate migration from yFiles WPF 2.5 to yFiles WPF 3.0. |
|[Complete Demos](Complete) |**Complete** | Source code samples that demonstrate the interaction between the viewer component and the layout algorithms. |
|[Data Binding Demos](DataBinding) |**DataBinding** | Source code samples that demonstrate binding graph elements to business data. |
|[InputMode Demos](Input) |**Input** | Source code samples that demonstrate how to configure specific aspects of the InputMode functionality. |
|[Layout Demos](Layout) |**Layout** | Source code samples demonstrate the basic usage of the yFiles WPF layout algorithms. |
|[Layout Only Demos](LayoutOnly) |**LayoutOnly** | Source code samples that demonstrate the basic usage of the yFiles WPF layout algorithms **without** the viewer component. |
|[Style Demos](Style) |**Style** | Source code samples that demonstrate how to customize the visual representation of yFiles WPF graph elements. |
|[Utility Projects for Demos](Utils) |**Utils** | Utility projects for common functionality in demos. |
|[Graph Viewer Demos](View) |**View** | Source code samples demonstrate basic concepts related to the `GraphControl` component and the underlying graph model. |
|[yEd WPF Graph Editor Demo](yEd%20WPF) |**yEd WPF** | Complex demo application that showcases most features that are present in yFiles WPF. yEd WPF will help you to interactively explore many capabilities of both the yFiles WPF Layout and the yFiles WPF Viewer functionality. |




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


