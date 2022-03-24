# 2DPlotting-UnityPackage

A Unity package that provides brushing, linking, and filtering functionality between multiple 2D plots and 3D visualizations.

## Installation in a Unity Project

### Non-development package use (recommended)
1. In Unity, open `Window > Package Manager`. 
2. Click the ```+``` button
3. Select ```Add package from git URL```
4. Paste ```git@github.umn.edu:ivlab-cs/2DPlotting-UnityPackage.git``` for the latest package

### Development use in a non git-managed project
1. Navigate your terminal or Git tool into your non version-controlled Unity project's main folder. 
2. Clone this repository into the Assets folder: ```cd Packages; git clone git@github.umn.edu:ivlab-cs/2DPlotting-UnityPackage.git```

### Development use in a git-managed project
1. Navigate your terminal or Git tool into your version-controlled Unity project's main folder. 
2. Add this repository as a submodule: ```cd Packages; git submodule add git@github.umn.edu:ivlab-cs/2DPlotting-UnityPackage.git; git submodule update --init --recursive```
3. See https://git-scm.com/book/en/v2/Git-Tools-Submodules for more details on working with Submodules. 

## Updating the Package

If you already have the package installed and want to update to the most recent version:

### For a non-development package:
1. Navigate to your Unity project's `Packages` folder using a file explorer or terminal.
2. Delete the "packages-lock" file.
3. Return to your Unity project and wait for it to automatically reload/update all of your packages.

A non-development package can also be updated by simply reperforming the [non-development package use](#non-development-package-use-recommended) instructions above.

### For a development package:
1. Navigate to the directory in which you have cloned the package.
2. Perform a `git pull` for the latest release.

## Getting Started

### Setup

This packages relies on TextMeshPro to display text. If you have not yet imported TextMeshPro, please navigate to `Window > TextMeshPro > Import TMP Essential Resources` in your Unity project and select `Import` before using this package. If you forget to do this, the `TMP Importer` window should appear when you create your first plot, in which case simply be sure to click the `Import TMP Essentials` button.

### Creating Your First Plots

Navigate to `Packages/IVLab 2DPlotting/Runtime/Prefabs/Plotting Setups/Simple` and drag the `Complete Plotting Setup (Simple)` prefab into an empty scene (make sure the scene is truly empty). Click play and use the UI to create your plots! Maximizing the game view window is also recommended.

## 2D/3D Visualization

This package readily supports linking between the 2D plots and any related 3D visualization of the data. 

For context on how this can be achieved, it's recommended that you import the sample scene provided with this package. To do so, navigate back to `Window > Package Manager` in your Unity project, select `IVLab 2DPlotting` from the list of packages, and then under the `Samples` dropdown in the package description window click the `Import` button next to the sample titled "2D/3D Vis Example." 

There should now be a folder by the name of `Samples` inside of `Assets`, which should have a subfolder (potentially a few levels down) titled `2D_3D Vis Example`, which should itself contain a `Scenes` folder. Open up the scene titled `ExampleScene` in this folder and you should be good-to-go to experiment with a sample 2D/3D visualization. Feel free to explore this and manipulate GameObjects and scripts as you wish.

To get started doing this for you own specific 2D/3D visualization, locate the `Linked Indices` GameObject in your scene. This object's `LinkedIndices` component has three publicly visible events that can be listened to:
- `OnIndexAttributeChanged(Int32, IndexAttributes)`: Listen to this event to be able to perform an action for each index that had an attribute change.
- `OnAnyIndexAttributeChanged()`: Listen to this event to be able to perform an action when any index attribute changes.
- `OnIndicesReinitialized()`: Listen to this event to be able to perform an action when the linked indices are reinitialized.

Add a method as a listener to any of these event either through the Unity inspector or through script, and take it away from there!

## Table Data

There are many ways to use your own tabular data source to create plots with this package. Assuming you have already dragged a `Complete Plotting Setup` prefab into your scene, the options are as follows:

### Option #1 - Use the built-in CSV reader
The built-in CSV reader provides the simplest and most direct way to load in data that is already in a tabular format. To begin,
simply locate the `Tabular Data Source` GameObject in the inspector. The `TabularDataFromCSVContainer` script attached to this GameObject has a number of inspector visible fields that allow you to load in a CSV of your own:
- **Load From Resources** - Toggle this to indicate whether to load the csv from the "Resources" folder, or from its full path name.
  - **Csv Filename** - Change this to the name (excluding ".csv") of any csv file in your project's `Assets/Resources` folder (create this folder if it does not exist). This script will construct a data table from the csv with the given name, and then use it to power the data plots. (Note: There are already some example csv files in this package's `Runtime/Resources` folder. To use one of them, simply input its name into this field, e.g. "cars").
  - **Csv Full Path Name** - If you have decided not to load a csv from resources, instead change this field to the full path name of a csv file anywhere on your computer.
- **Csv Has Row Names** - Check this box if the csv you are loading has row names in its first column. If the csv's first column consists of data, uncheck this box.
- **Csv Data Is Clustered** - Check this box if the data in the csv is clustered.
  - **Cluster Color Gradient** - Alter this gradient to alter the colors used for each cluster.

### Option #2 - Use a tabular data container
Building your own "Tabular Data Container" gives you more control over how your tabular data is constructed, while simultaneously allowing you to interact with it in the inspector. To take this approach, start by creating a custom class that inherits from [`TabularDataContainer`](https://pages.github.umn.edu/ivlab-cs/2DPlotting-UnityPackage/api/IVLab.Plotting.TabularDataContainer.html) and overrides its `Init()` method. Build your `TableData` object within this method however you please, being sure to set the `initialized` flag to true somewhere along the way.
```
public class CustomTabularDataContainer : TabularDataContainer
{
    protected override void Init()
    {
        // Initialize an array of the data in column-major order
        float[] data = . . .
        // Save the column names
        string[] columnNames = . . .

        // Initialize a new table data and assign it to this class' tableData field
        tableData = new TableData(data, columnNames);

        // Set initialized flag to true
        initialized = true;
    }
}
```
Now that you have your own tabular data container built, add it as a component to the GameObject in your scene that you want to act as your tabular data source for this data. Finally, drag that GameObject into the `Tabular Data Container` field of the `Data Plot Group` in the inspector.
### Option #3 - Use scripting
If you're not in the mood for Unity shenanigans, an alternative is to create and apply your tabular data entirely through script by following a process like so:
1. Create a new MonoBehaviour script that has a reference to the `Data Plot Group`'s `DataPlotGroup` script (e.g. at the top of this script define something like `[SerializeField] private DataPlotGroup dataPlotGroup;` and attach the `Data Plot Group` object to it via the inspector).
3. At some point in this MonoBehaviour (likely the `Start()` or `Awake()` method) initialize your data in whatever way suits your needs (e.g. using your own csv reader, by means of an sql data table, taking data straight from other GameObjects in your Unity scene, etc.).
4. Format this data in such a way that one of the [`TableData` constructor methods](https://pages.github.umn.edu/ivlab-cs/2DPlotting-UnityPackage/api/IVLab.Plotting.TableData.html) will be able to parse it (as suggested in the documentation, refer to the [image](https://pages.github.umn.edu/ivlab-cs/2DPlotting-UnityPackage/api/IVLab.Plotting.TableData.html) at the top of the page for clarification).
5. Create the `TableData` and assign it to the `DataPlotGroup`. This may look something like:
        
        // Initialize a matrix of the data in row-major order
        float[][] data = . . .
        // Save the row and column names
        string[] rowNames = . . .
        string[] columnNames = . . .

        // Initialize a new table data using the "row-major-order matrix data" constructor
        TableData tableData = new TableData(data, rowNames, columnNames);

        // Set this TableData as the table used by the DataPlotGroup
        dataPlotGroup.TableData = tableData;
6. After `dataPlotGroup.TableData = tableData;` has been called, the `DataPlotGroup` will automatically update any plots it manages to use this new data table, which means you should be all set from here!

## Plot Styling

To make a custom stylesheet asset (aka skin), navigate to `Assets > Create > Plotting` and select the type of stylesheet you would like to create. Edit the created stylesheet asset, and apply it by dragging it into the the inspector for the `DataPlotGroup` or any of the specific `Plot Setups` it has reference to.
