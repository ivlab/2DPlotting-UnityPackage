# 2DPlotting-UnityPackage

A Unity package that provides 2D plotting functionality, along with brushing, linking, and filtering between multiple plots and 3D visualizations.

## Installation in a Unity Project

### Non-development (read-only) Package use (recommended)
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

## Getting Started

### Setup

**Note:** This packages relies on TextMeshPro to display text. If you have not yet imported TextMeshPro, please navigate to `Window > TextMeshPro > Import TMP Essential Resources` in your Unity project and select `Import` before using this package. If you forget to do this, the `TMP Importer` window should appear when you create your first plot, in which case simply be sure to click the `Import TMP Essentials` button.

Once this package is installed following the installation instructions above, it's important that your Unity project's Tags/Sorting Layers/Layers match those used by this package. To ensure that this is the case, navigate to `Edit > Project Settings...` in your Unity project and select the `Tags and Layers` tab. Next, click the icon with the two horizontal sliders in the top right corner of the `Tags and Layers` window, just to the left of the icon with the three vertical dots. From here, simply select the `2DPlotsLayerSetup` preset and you should be good to go.

### Creating Your First Plots

Navigate to `Packages/IVLab 2DPlotting/Runtime/Prefabs/Grab and Go` and drag the `Complete Plotting Setup` prefab into an empty scene (make sure the scene is truly empty).

With the `Complete Plotting Setup` prefab now in your scene, locate the `Data Manager` GameObject (it exists under `Complete Plotting Setup > Data` in the hierarchy, but can also be found using the search bar). The `Data Manager` script attached to this GameObject will be your main tool for interacting with the package. Two of it's fields you may wish to interact with are:

- **Csv Filename** - Change this to the name (excluding ".csv") of any csv file in your project's `Assets/Resources` folder (create this folder if you haven't already). This script will construct a data table out of that csv and use it to power the data plots. (Note: There are already some example csv files in this package's `Runtime/Resources` folder. To use one of them, simply input its name into this field).
- **Linked Data** - Add any additional data that you want to be linked to the main data table here. See [2D/3D Visualization](### 2D/3D Visualization) for more details.

Once you have selected a csv file to read data from and inputted its name into the "Csv Filename" field, play the scene and use the UI interface to create your plots!

### 2D/3D Visualization

This package also readily supports connections between the 2D plots and any related 3D visualization of the data. To get started, locate and open the `LinkedData` script in the `Runtime/Scripts/General` directory. You'll want to create a new script that inherits from `LinkedData` and implements its UpdateDataPoint() method. For an example of one way you might go about doing this, check out the `LinkedDataExample` script in the `Runtime/Scripts/Example` directory.

Once you have created your own implementation of `LinkedData` and attached it to a GameObject in the scene, add that GameObject to the "Linked Data" field of the `Data Manager` and you should be good to go!

(If you have write-access to the package (i.e. you installed for development) you will also have access to the example scene in `Packages/IVLab 2DPlotting/Runtime/Scenes`, which should provide additional context for how 2D/3D visualization is possible.)