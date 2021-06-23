# IVLab-Template-UnityPackage

Template repository for an IVLab Unity package, including structure and documentation.

Package structure should follow the [Lab Guidelines on Unity Packages](https://docs.google.com/document/d/1BWo-OIJx3uG72XyvIiO-t1jVDnXKFhoxj-o5VYO5Gq0/edit?usp=sharing).

Before getting started, think of a name for your package. IVLab Unity package names should follow the convention "<YourPackage>-UnityPackage" - for example, "OBJImport-UnityPackage".

## Getting Started

This is a template Unity package. Click the "Use this template" button in this GitHub repository, clone your new repository, then follow this guide and the [Making Unity Packages](https://docs.google.com/document/d/1BWo-OIJx3uG72XyvIiO-t1jVDnXKFhoxj-o5VYO5Gq0/edit?usp=sharing) document to get started.

1. In `package.json`:
    - Replace the template name with your package name (don't need to retain "IVLab" in package name)
    - Replace the template identifier with your package identifier (retain the `edu.umn.cs.ivlab` portion and add identifiers as necessary [e.g. `edu.umn.cs.ivlab.utilities`])
2. Rename `Runtime/IVLab.Template.Runtime.asmdef` to match your package name (only change the `Template` part, and add namespaces as appropriate)
3. In the new `Runtime/IVLab.<YourPackage>.Runtime.asmdef`, edit the assembly `name` to match the asmdef file name.
4. Create your package, whilst following lab guidelines on Unity packages.
5. Make sure to document your code as you write it (using [C# XML comments](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/recommended-tags-for-documentation-comments)), as shown in `Runtime/Scripts/TemplateExample.cs`.
6. Generate the documentation for your package using the instructions found in [Documentation](./Documentation).


## Installation in a Unity Project

### Non-development (read-only) Package use
1. In Unity, open Window -> Package Manager. 
2. Click the ```+``` button
3. Select ```Add package from git URL```
4. Paste ```git@github.umn.edu:ivlab-cs/IVLab-Template-UnityPackage.git``` for the latest package

### Development use in a git-managed project
1. Navigate your terminal or Git tool into your version-controlled Unity project's main folder. 
2. Add this repository as a submodule: ```cd Packages; git submodule add git@github.umn.edu:ivlab-cs/IVLab-Template-UnityPackage.git; git submodule update --init --recursive```
3. See https://git-scm.com/book/en/v2/Git-Tools-Submodules for more details on working with Submodules. 

### Development use in a non git-managed project
1. Navigate your terminal or Git tool into your non version-controlled Unity project's main folder. 
2. Clone this repository into the Assets folder: ```cd Packages; git clone git@github.umn.edu:ivlab-cs/IVLab-Template-UnityPackage.git```