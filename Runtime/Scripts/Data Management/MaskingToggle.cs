using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{
    /// <summary>
    /// Provides framework for implementation of a method to mask/unmask unselected on a button press.
    /// </summary>
    public abstract class MaskingToggle
    {
        /// <summary>
        /// Key used to toggle masking.
        /// </summary>
        public KeyCode toggleKey = KeyCode.Space;

        /// <summary>
        /// Mask/unmask unselected points.
        /// </summary>
        public abstract void ToggleMasking(DataManager dataManager);
    }

    public class DefaultMaskingToggle : MaskingToggle
    {
        /// <summary>
        /// Constructor sets key that controls toggling.
        /// </summary>
        /// <param name="toggleKey"></param>
        public DefaultMaskingToggle(KeyCode toggleKey = KeyCode.Space)
        {
            this.toggleKey = toggleKey;
        }

        /// <summary>
        /// Basic mask/unmask toggling of unselected points in Scatter, Parallel Coords, and Cluster Plots.
        /// </summary>
        public override void ToggleMasking(DataManager dataManager)
        {
            // Toggle masking
            dataManager.MaskingData = !dataManager.MaskingData;

            // Data table isn't clustered
            if (!dataManager.UsingClusterDataTable)
            {
                if (dataManager.MaskingData)
                {
                    int unhighlightedCount = 0;
                    // Mask all unhighlighted data points
                    for (int i = 0; i < dataManager.LinkedIndices.Size; i++)
                    {
                        if (!dataManager.LinkedIndices[i].Highlighted)
                        {
                            dataManager.LinkedIndices[i].Masked = true;
                            unhighlightedCount++;
                        }
                    }
                    // Unmask the data points if all of them were unhighlighted
                    if (unhighlightedCount == dataManager.LinkedIndices.Size)
                    {
                        for (int i = 0; i < dataManager.LinkedIndices.Size; i++)
                        {
                            dataManager.LinkedIndices[i].Masked = false;
                        }
                        dataManager.MaskingData = false;
                        dataManager.NothingMasked = true;
                    }
                    else
                    {
                        dataManager.NothingMasked = false;
                    }
                }
                else
                {
                    // Unmask all currently masked data points
                    for (int i = 0; i < dataManager.LinkedIndices.Size; i++)
                    {
                        if (dataManager.LinkedIndices[i].Masked)
                        {
                            dataManager.LinkedIndices[i].Masked = false;
                        }
                    }
                    dataManager.NothingMasked = true;
                }
            }
            // Data table is clustered
            else
            {
                List<Cluster> clusters = ((ClusterDataTable)dataManager.DataTable).Clusters;
                if (dataManager.MaskingData)
                {
                    int unhighlightedCount = 0;
                    int totalCount = 0;
                    // Mask all unhighlighted data points in clusters that are enabled
                    for (int c = 0; c < clusters.Count; c++)
                    {
                        if (dataManager.DataPlotManager.ClusterToggles[c].isOn)
                        {
                            for (int i = clusters[c].StartIdx; i < clusters[c].EndIdx; i++)
                            {
                                if (!dataManager.LinkedIndices[i].Highlighted)
                                {
                                    dataManager.LinkedIndices[i].Masked = true;
                                    unhighlightedCount++;
                                }
                                totalCount++;
                            }
                        }
                    }
                    // Unmask the data points if all of them were unhighlighted
                    if (unhighlightedCount == totalCount)
                    {
                        for (int c = 0; c < clusters.Count; c++)
                        {
                            if (dataManager.DataPlotManager.ClusterToggles[c].isOn)
                            {
                                for (int i = clusters[c].StartIdx; i < clusters[c].EndIdx; i++)
                                {
                                    dataManager.LinkedIndices[i].Masked = false;
                                }
                            }
                        }
                        dataManager.MaskingData = false;
                        dataManager.NothingMasked = true;
                    }
                    else
                    {
                        dataManager.NothingMasked = false;
                    }
                }
                else
                {
                    // Unmask all currently masked data points in clusters that are enabled
                    for (int c = 0; c < clusters.Count; c++)
                    {
                        if (dataManager.DataPlotManager.ClusterToggles[c].isOn)
                        {
                            for (int i = clusters[c].StartIdx; i < clusters[c].EndIdx; i++)
                            {
                                dataManager.LinkedIndices[i].Masked = false;
                            }
                        }
                    }
                    dataManager.NothingMasked = true;
                }
            }
        }
    }
}
