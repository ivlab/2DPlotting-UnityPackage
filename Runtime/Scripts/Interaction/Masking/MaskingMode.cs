using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{
    /// <summary>
    /// Abstract class which declares variables and methods ubiquitous to all possible masking modes.
    /// </summary>
    public abstract class MaskingMode : MonoBehaviour
    {
        [Header("Masking")]
        [SerializeField] protected KeyCode toggleKey = KeyCode.Space;
        [Header("Dependencies")]
        /// <summary> Linked indices group this controls masking for </summary>
        [SerializeField] protected LinkedIndices linkedIndices;
        /// <summary> Key used to toggle masking. </summary>
        public KeyCode ToggleKey { get => toggleKey; set => toggleKey = value; }

        /// <summary>
        /// Mask/unmask unselected points for given linked indices group.
        /// </summary>
        public abstract void ToggleMasking();
    }
}
