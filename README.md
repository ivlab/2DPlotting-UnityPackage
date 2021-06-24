# 2DPlotting-UnityPackage

A Unity package that provides 2D plotting functionality, along with brushing, linking, and filtering between multiple plots.

## Getting Started

Once installed following the installation instructions below, navigate to the `IVLab 2DPlotting\Runtime` folder now located in your project's `Packages` directory and locate the "LayerSetup" asset.

It's important that your Unity project's Tags/Sorting Layers/Layers match those used by this package, so once you've located the "LayerSetup" select it and you should now see the Sorting Layers/Layers used by this package in your inspector window. In order to apply these to your own project, click the icon with the two horizontal sliders on it in the top right of the inspector window, just to the left of the icon with the three vertical dots. From here, simply select the "LayerSetup" preset and you should be good to go.

To get setup with an example of what can be done with this package, proceed to the example scene in `Runtime\Scenes.`

To actually use some 2D plots in your own project, navigate to `Runtime\Prefabs\Grab and Go` and drag the "Complete Plotting Setup" prefab into an empty scene. 

## Installation in a Unity Project

### Non-development (read-only) Package use
1. In Unity, open Window -> Package Manager. 
2. Click the ```+``` button
3. Select ```Add package from git URL```
4. Paste ```git@github.umn.edu:ivlab-cs/2DPlotting-UnityPackage.git``` for the latest package

### Development use in a git-managed project
1. Navigate your terminal or Git tool into your version-controlled Unity project's main folder. 
2. Add this repository as a submodule: ```cd Packages; git submodule add git@github.umn.edu:ivlab-cs/2DPlotting-UnityPackage.git; git submodule update --init --recursive```
3. See https://git-scm.com/book/en/v2/Git-Tools-Submodules for more details on working with Submodules. 

### Development use in a non git-managed project
1. Navigate your terminal or Git tool into your non version-controlled Unity project's main folder. 
2. Clone this repository into the Assets folder: ```cd Packages; git clone git@github.umn.edu:ivlab-cs/2DPlotting-UnityPackage.git```