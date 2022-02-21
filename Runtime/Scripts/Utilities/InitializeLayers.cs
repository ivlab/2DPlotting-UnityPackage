using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;

#if UNITY_EDITOR

namespace IVLab.Plotting {

    /// <summary>
    /// Automatically creates the various layers needed for the 2D plotting
    /// package when it is imported.
    /// </summary>
    [InitializeOnLoad]
    public class InitializeLayers
    {
        static InitializeLayers()
        {
            string sortingLayerName = PlottingUtilities.Consts.PlotsSortingLayerName;
            string layerName = PlottingUtilities.Consts.PlotsLayerName;

            CreateSortingLayer(sortingLayerName);
            CreateLayer(layerName);
        }

        /// <summary>
        /// Create a new sorting layer in this Unity project at the first available slot.
        /// </summary>
        public static void CreateSortingLayer(string sortingLayerName)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
            SerializedProperty sortingLayers = tagManager.FindProperty("m_SortingLayers");
            for (int i = 0; i < sortingLayers.arraySize; i++)
                if (sortingLayers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue.Equals(sortingLayerName))
                    return;
            sortingLayers.InsertArrayElementAtIndex(sortingLayers.arraySize);
            SerializedProperty newLayer = sortingLayers.GetArrayElementAtIndex(sortingLayers.arraySize - 1);
            newLayer.FindPropertyRelative("name").stringValue = sortingLayerName;
            newLayer.FindPropertyRelative("uniqueID").intValue = (int)DateTime.Now.Ticks; /* some unique number */
            tagManager.ApplyModifiedProperties();
            Debug.LogFormat("Successfully created new sorting layer (sorting layer {0}): `{1}`", sortingLayers.arraySize - 1, sortingLayerName);
        }

        /// <summary>
        /// Create a new layer in this Unity project at the first available slot.
        /// </summary>
        public static void CreateLayer(string layerName)
        {
            if (LayerMask.NameToLayer(layerName) < 0)
            {
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty it = tagManager.GetIterator();
                bool showChildren = true;
                int actualLayer = -1;
                while (it.NextVisible(showChildren) && actualLayer < 0)
                {
                    if (it.name == "layers")
                    {
                        int numLayers = it.arraySize;
                        for (int i = 0; i < numLayers && actualLayer < 0; i++)
                        {
                            SerializedProperty element = it.GetArrayElementAtIndex(i);
                            // First empty element of layers list
                            if (element.stringValue.Length == 0)
                            {
                                element.stringValue = layerName;
                                actualLayer = i;
                            }
                        }
                    }
                }
                tagManager.ApplyModifiedProperties();
                Debug.LogFormat("Successfully created new layer (layer {0}): `{1}`", actualLayer, layerName);
            }
        }
    }
}

#endif