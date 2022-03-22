using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{
    public class DefaultMaskingMode : MaskingMode
    {
        /// <summary>
        /// Basic mask/unmask toggling of unselected points in Scatter and Parallel Coords plots.
        /// </summary>
        public override void ToggleMasking()
        {
            // Unmask all points if none are highlighted
            if (linkedIndices.HighlightedCount == 0)
            {
                for (int i = 0; i < linkedIndices.Size; i++)
                {
                    linkedIndices[i].Masked = false;
                }
            }
            // Otherwise mask all unselected points
            else
            {
                for (int i = 0; i < linkedIndices.Size; i++)
                {
                    if (!linkedIndices[i].Highlighted)
                    {
                        linkedIndices[i].Masked = true;
                    }
                }
            }
        }
    }
}
