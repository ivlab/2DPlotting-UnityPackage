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
            if (linkedIndicesGroup.LinkedIndices.HighlightedCount == 0)
            {
                for (int i = 0; i < linkedIndicesGroup.LinkedIndices.Size; i++)
                {
                    linkedIndicesGroup.LinkedIndices[i].Masked = false;
                }
            }
            // Otherwise mask all unselected points
            else
            {
                for (int i = 0; i < linkedIndicesGroup.LinkedIndices.Size; i++)
                {
                    if (!linkedIndicesGroup.LinkedIndices[i].Highlighted)
                    {
                        linkedIndicesGroup.LinkedIndices[i].Masked = true;
                    }
                }
            }
        }
    }
}
