using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{
    /// <summary>
    /// Basic control over the 2D/3D split camera view.
    /// </summary>
    public class SplitViewController : MonoBehaviour
    {
        /// <summary> Camera that should occupy the right-hand portion of the screen. </summary>
        [SerializeField] private Camera rightCamera;
        /// <summary> Camera that should occupy the left-hand portion of the screen. </summary>
        [SerializeField] private Camera leftCamera;
        /// <summary> Horizontal position of the partition between the left and right camera. </summary>
        [SerializeField] [Range(0, 1)] private float partition = 0.5f;

        // Update is called once per frame
        void OnValidate()
        {
            rightCamera.rect = new Rect(partition, 0, 1, 1);
            leftCamera.rect = new Rect(0, 0, partition, 1);
        }
    }
}
