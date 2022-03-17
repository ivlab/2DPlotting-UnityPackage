using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{
    public class ClusterMaskingToggle : MaskingMode
    {
        /// <summary>
        /// Basic mask/unmask toggling of unselected points in Scatter, Parallel Coords, and Cluster Plots.
        /// </summary>
        public override void ToggleMasking()
        {
        //     // Toggle masking
        //     dataManager.MaskingData = !dataManager.MaskingData;
  
        //     List<Cluster> clusters = ((ClusterTableData)dataManager.TableData).Clusters;
        //     if (dataManager.MaskingData)
        //     {
        //         int unhighlightedCount = 0;
        //         int totalCount = 0;
        //         // Mask all unhighlighted data points in clusters that are enabled
        //         for (int c = 0; c < clusters.Count; c++)
        //         {
        //             if (clusters[c].Enabled)
        //             {
        //                 for (int i = clusters[c].StartIdx; i < clusters[c].EndIdx; i++)
        //                 {
        //                     if (!dataManager.LinkedIndices[i].Highlighted)
        //                     {
        //                         dataManager.LinkedIndices[i].Masked = true;
        //                         unhighlightedCount++;
        //                     }
        //                     totalCount++;
        //                 }
        //             }
        //         }
        //         // Unmask the data points if all of them were unhighlighted
        //         if (unhighlightedCount == totalCount)
        //         {
        //             for (int c = 0; c < clusters.Count; c++)
        //             {
        //                 if (clusters[c].Enabled)
        //                 {
        //                     for (int i = clusters[c].StartIdx; i < clusters[c].EndIdx; i++)
        //                     {
        //                         dataManager.LinkedIndices[i].Masked = false;
        //                     }
        //                 }
        //             }
        //             dataManager.MaskingData = false;
        //             dataManager.NothingMasked = true;
        //         }
        //         else
        //         {
        //             dataManager.NothingMasked = false;
        //         }
        //     }
        //     else
        //     {
        //         // Unmask all currently masked data points in clusters that are enabled
        //         for (int c = 0; c < clusters.Count; c++)
        //         {
        //             if (clusters[c].Enabled)
        //             {
        //                 for (int i = clusters[c].StartIdx; i < clusters[c].EndIdx; i++)
        //                 {
        //                     dataManager.LinkedIndices[i].Masked = false;
        //                 }
        //             }
        //         }
        //         dataManager.NothingMasked = true;
        //     }
        }
    }
}
