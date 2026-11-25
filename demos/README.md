<!-- populated by readme-style.js --> Programming Samples Overview


# yFiles WPF Programming Samples Overview
The [yFiles WPF](https://www.yworks.com/products/yfileswpf) programming samples are extensive .NET projects with **full C# source code** that present the functionality of the yFiles WPF diagramming library. All programming samples can be found in subdirectories of the current directory. Most samples are self-contained, i.e., they consist of a single .NET project. 


## Working with the samples
All demos can be accessed by opening the solution file (yFiles Demos.sln) in this folder. 

The Demo Browser Application in the DemoBrowser project makes it easy to browse the demo applications. It includes a search feature and brief descriptions of each demo. You can also run each demo as a standalone project. 




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
To open a specific programming sample, the corresponding project file (usually named `[demoname].csproj`) can be found in the demo's directory, which also uses the `[demoname]` naming scheme. <br /> Note that samples that demonstrate related aspects are grouped together in solution folders. 

<a id="samples"></a> 
### Programming Samples Categories

| Category | Folder | Description |
|:---|:---|:---|
|[Application Feature Demos](ApplicationFeatures) |**ApplicationFeatures** | Source code samples demonstrate how to use and configure single yFiles WPF features. |
|[Data Binding Demos](DataBinding) |**DataBinding** | Source code samples that demonstrate binding graph elements to business data. |
|[The Demo Browser](DemoBrowser) |**DemoBrowser** | A Demo Browser for easy finding and running demo applications. |
|[Input Demos](Input) |**Input** | Source code samples that demonstrate how to configure specific aspects of the interaction functionality. |
|[Layout Demos](Layout) |**Layout** | Source code samples demonstrate the basic usage of the yFiles WPF layout algorithms. |
|[Layout Feature Demos](LayoutFeatures) |**LayoutFeatures** | Source code samples demonstrate specific features of the yFiles WPF layout algorithms. |
|[Showcase Demos](Showcase) |**Showcase** | Complex samples that show application fields coming with complex visualization and interaction. |
|[Style Demos](Style) |**Style** | Source code samples that demonstrate how to customize the visual representation of yFiles WPF graph elements. |
|[Tutorials](Tutorials) |**Tutorials** | These tutorials show you the basic features of yFiles WPF, how to bind graph elements to business data, and how to create custom styles for nodes, edges, and labels. |
|[Utility Projects for Demos](Utils) |**Utils** | Utility projects for common functionality in demos. |
|[Graph Viewer Demos](View) |**View** | Source code samples demonstrate basic concepts related to the `GraphControl` component and the underlying graph model. |


<!-- populated by readme-style.js --> 



<script src="../doc/resources/readme-style.js"></script> 
