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
        [SerializeField] private Camera rightCamera;
        [SerializeField] private Camera leftCamera;
        [SerializeField] [Range(0, 1)] private float partition = 0.5f;

        // Update is called once per frame
        void OnValidate()
        {
            rightCamera.rect = new Rect(partition, 0, 1, 1);
            leftCamera.rect = new Rect(0, 0, partition, 1);
        }
    }
}
