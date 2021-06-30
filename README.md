# 2DPlotting-UnityPackage

A Unity package that provides 2D plotting functionality, along with brushing, linking, and filtering between multiple plots and 3D visualizations.

## Getting Started

**Note:** This packages relies on TextMeshPro to display text. If you have not yet imported TextMeshPro, please navigate to `Window > TextMeshPro > Import TMP Essential Resources` in your Unity project and select `Import` before using this package. If you forget to do this, the `TMP Importer` window should appear when you create your first plot, in which case simply be sure to click the `Import TMP Essentials` button.

Once this package is installed following the installation instructions below, it's important that your Unity project's Tags/Sorting Layers/Layers match those used by this package. To ensure that this is the case, navigate to `Edit > Project Settings...` in your Unity project and select the `Tags and Layers` tab. Next, click the icon with the two horizontal sliders in the top right corner of the `Tags and Layers` window, just to the left of the icon with the three vertical dots. From here, simply select the `2DPlotsLayerSetup` preset and you should be good to go.

To immediately use 2D plots in your own project, navigate to `Packages/IVLab 2DPlotting/Runtime/Prefabs/Grab and Go` and drag the `Complete Plotting Setup` prefab into an empty scene. 

If you are developing on this package, you will have access to the example scene in `Packages/IVLab 2DPlotting/Runtime/Scenes` which should provide additional context for how 2D/3D vis is possible.

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
