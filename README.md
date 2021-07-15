# 2DPlotting-UnityPackage

![Plotting Example](../media/PlottingExample.gif?raw=true)

A Unity package that provides 2D plotting functionality, along with brushing, linking, and filtering between multiple plots and 3D visualizations.

## Installation in a Unity Project

### Non-development (read-only) Package use (recommended)
1. In Unity, open `Window > Package Manager`. 
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

## Updating the Package

If you already have the package installed and want to update to the most recent version:

### For a non-development package:
1. In Unity, open `Window > Package Manager`.
2. Click the refresh button at the bottom of the window, and then select `Refresh list`.
3. Select `IVLab 2DPlotting` from the list of packages.
4. Click the `Update to #.#.#` button at the bottom right of the window.
5. If no update button appears, simply follow the [non-development](#non-development-read-only-package-use-recommended) installation instructions above. This will be equivalent to updating the package.

### For a development package:
1. Navigate to the directory in which you have cloned the package.
2. Perform a `git pull` for the latest release.

**Note:** It's apparently possible for your Unity project's Tags/Sorting Layers/Layers to get mixed up after updating the package, so it's not a bad idea to run through the [Setup](#setup) portion of Getting Started after updating if things appear to be amiss.

## Getting Started

### Setup

**Note:** This packages relies on TextMeshPro to display text. If you have not yet imported TextMeshPro, please navigate to `Window > TextMeshPro > Import TMP Essential Resources` in your Unity project and select `Import` before using this package. If you forget to do this, the `TMP Importer` window should appear when you create your first plot, in which case simply be sure to click the `Import TMP Essentials` button.

Once this package is installed following the installation instructions above, it's important that your Unity project's Tags/Sorting Layers/Layers match those used by this package. To ensure that this is the case, navigate to `Edit > Project Settings...` in your Unity project and select the `Tags and Layers` tab. Next, click the icon with the two horizontal sliders in the top right corner of the `Tags and Layers` window, just to the left of the icon with the three vertical dots. From here, simply select the `2DPlotsLayerSetup` preset and you should be good to go.

### Creating Your First Plots

Navigate to `Packages/IVLab 2DPlotting/Runtime/Prefabs/Grab and Go` and drag the `Complete Plotting Setup` prefab into an empty scene (make sure the scene is truly empty).

With the `Complete Plotting Setup` prefab now in your scene, locate the `Data Manager` GameObject (it exists under `Complete Plotting Setup > Data` in the hierarchy, but can also be found using the search bar). The `DataManager` script attached to this GameObject will be your main tool for interacting with the package. Two of it's fields you may wish to interact with are:

- **Csv Filename** - Change this to the name (excluding ".csv") of any csv file in your project's `Assets/Resources` folder (create this folder if you haven't already). This script will construct a data table from the csv with the given name, and then use it to power the data plots. (Note: There are already some example csv files in this package's `Runtime/Resources` folder. To use one of them, simply input its name into this field, e.g. "cars").
- **Linked Data** - Add any additional data that you want to be linked to the main data table here. See [2D/3D Visualization](#2d3d-visualization) for more details.

Once you have selected a csv file to read data from and inputted its name into the "Csv Filename" field, play the scene and use the UI to create your plots!

### 2D/3D Visualization

This package readily supports linking between the 2D plots and any related 3D visualization of the data. 

For context on how this can be achieved, it's recommended that you import the sample scene provided with this package. To do so, navigate back to `Window > Package Manager` in your Unity project, select `IVLab 2DPlotting` from the list of packages, and then under the `Samples` dropdown in the package description window click the `Import` button next to the sample titled "2D/3D Vis Example." 

There should now be a folder by the name of `Samples` inside of `Assets`, which should have a subfolder (potentially a few levels down) titled `2D_3D Vis Example`, which should itself contain a `Scenes` folder. Open up the scene titled `ExampleScene` in this folder and you should be setup to experiment with a sample 2D/3D visualization. Feel free to explore this and manipulate GameObjects and scripts as you wish.

To get started doing this for you own specific 2D/3D visualization, begin by locating and opening the `LinkedData` script in this package's `Runtime/Scripts/General` folder. You'll then want to create a new script that inherits from `LinkedData` and implements its UpdateDataPoint() method (now may be a good time to return to sample you downloaded and open the `LinkedDataExample` script in its `Scripts` folder for an example on how one might do this). 

Once you have created your own implementation of `LinkedData`, attach it to a GameObject in the scene, add that GameObject to the "Linked Data" field of the `Data Manager`, and you should be good to go! For clarification on this step, fell free to return to `ExampleScene` in the sample you may have downloaded to get a sense for how this is all wired together.

## A Note on Data Tables

When creating your first plots after dragging the "Complete Plotting Setup" prefab into your scene, it's possible that you'll want to use a data source external to the csv reader provided in the [DataTable](https://pages.github.umn.edu/ivlab-cs/2DPlotting-UnityPackage/api/IVLab.Plotting.DataTable.html) class. If this is the case, a recommended approach would be as follows:
1. Create a new MonoBehaviour script that has a reference to the `DataManager` (e.g. at the top of this script define something like `[SerializeField] private DataManager dataManager;` and attach the `DataManager` object to it using the inspector).
2. At some point in this MonoBehaviour (likely the `Start()` method) initialize your data in whatever way suits your needs (e.g. using your own csv reader, by means of an sql data table, taking data straight from GameObjects in your Unity scene, etc.).
3. Format this data in such a way that this [DataTable constructor method](https://pages.github.umn.edu/ivlab-cs/2DPlotting-UnityPackage/api/IVLab.Plotting.DataTable.html#IVLab_Plotting_DataTable__ctor_System_Single_____System_String___System_String___) will be able to parse it (as suggested in the documentation, refer to the [image](https://pages.github.umn.edu/ivlab-cs/2DPlotting-UnityPackage/api/IVLab.Plotting.DataTable.html) at the top of the page for clarification).
4. Create the `DataTable` and assign it to the `DataManager`, this might look something like:
        
        // Initialize a matrix to hold the data in row-major order
        float[][] data = . . .
        // Save the row and column names
        string[] rowNames = . . .
        string[] columnNames = . . .

        // Initialize a new data table using the "external-data" constructor
        DataTable dataTable = new DataTable(data, rowNames, columnNames);

        // Set this DataTable as the table used by the DataManager
        dataManager.DataTable = dataTable;
5. After `dataManager.DataTable = dataTable;` has been called, the `DataManager` will automatically update any plots it manages (indirectly through its `DataPlotManager`) to use this new data table, which means you should be all set from here!

## Documentation
[Auto-generated documentation for the UnityPackage is available](https://pages.github.umn.edu/ivlab-cs/2DPlotting-UnityPackage/api/IVLab.Plotting.html). To re-generate the documentation, follow the instructions in the [Documentation](https://github.umn.edu/ivlab-cs/2DPlotting-UnityPackage/tree/master/Documentation) folder.
