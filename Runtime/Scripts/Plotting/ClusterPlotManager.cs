using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IVLab.Plotting
{
    public class ClusterPlotManager : MonoBehaviour
    {

        [SerializeField] private DataManager dataManager;
        [SerializeField] private DataPlotManager dataPlotManager;
        [SerializeField] private GameObject togglePrefab;
        
        /// <summary> Array of cluster toggles used to hide/show clusters. </summary>
        private Toggle[] clusterToggles = new Toggle[0];
        /// <summary> Parent gameobject of cluster toggles. </summary>
        private Transform clusterToggleParent;
        /// <summary> Saves a copy of each clusters linked attributes before they are toggled off
        /// so that they can easily return to it when toggled on. </summary>
        private LinkedIndices.LinkedAttributes[][] savedClusterLinkedAttributes;
        /// <summary> Gets the cluster toggles created by this plot manager. </summary>
        public Toggle[] ClusterToggles { get => clusterToggles; }

        public void Show() { clusterToggleParent.gameObject.SetActive(true); }

        public void Hide() { clusterToggleParent.gameObject.SetActive(false); }

        public void CreateToggles()
        {
            // Delete any previous cluster toggles
            foreach (Toggle toggle in clusterToggles)
            {
                Destroy(toggle.gameObject);
            }

            // Define a uniform spacing between toggles
            float clusterToggleSpacing = 85;

            // Destroy old / create new cluster toggle parent
            if (clusterToggleParent?.gameObject != null) 
                Destroy(clusterToggleParent.gameObject);
            clusterToggleParent = new GameObject("Cluster Toggles").AddComponent<RectTransform>();
            clusterToggleParent.SetParent(dataPlotManager.PlotsParent.transform);
            clusterToggleParent.localScale = Vector3.one;
            clusterToggleParent.localPosition = Vector3.zero;
            // Stretch it to the size of its parent (plot parent)
            ((RectTransform)clusterToggleParent).anchorMin = new Vector2(0, 0);
            ((RectTransform)clusterToggleParent).anchorMax = new Vector2(1, 1);
            ((RectTransform)clusterToggleParent).pivot = new Vector2(0.5f, 0.5f);
            ((RectTransform)clusterToggleParent).offsetMax = Vector2.zero;
            ((RectTransform)clusterToggleParent).offsetMin = Vector2.zero;
            
            // Initialize relevant cluster arrays
            List<Cluster> clusters = ((ClusterDataTable)dataManager.DataTable).Clusters;
            clusterToggles = new Toggle[clusters.Count];
            savedClusterLinkedAttributes = new LinkedIndices.LinkedAttributes[clusters.Count][];

            // Create the cluster background image
            RectTransform backgroundRect = new GameObject("Toggles Background").AddComponent<RectTransform>();
            backgroundRect.SetParent(clusterToggleParent);
            backgroundRect.transform.localScale = Vector3.one;
            backgroundRect.transform.localPosition = Vector3.zero;
            backgroundRect.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
            backgroundRect.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
            backgroundRect.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            backgroundRect.sizeDelta = new Vector2(clusters.Count * (clusterToggleSpacing) + 25, 25);
            backgroundRect.gameObject.AddComponent<Image>().color = new Color32(228, 239, 243, 255);
            backgroundRect.gameObject.AddComponent<Outline>();

            // Create cluster toggles
            for (int i = 0; i < clusterToggles.Length; i++)
            {
                // Instantiate the toggle
                GameObject toggleObject = Instantiate(togglePrefab, Vector3.zero, Quaternion.identity) as GameObject;
                // Position the toggle
                toggleObject.transform.SetParent(clusterToggleParent);
                toggleObject.transform.localScale = Vector3.one;
                toggleObject.transform.localPosition = Vector3.zero;
                toggleObject.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
                toggleObject.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
                toggleObject.GetComponent<RectTransform>().anchoredPosition = Vector2.right * ((i - (clusters.Count - 1) / 2.0f) * clusterToggleSpacing - 20);
                PlottingUtilities.ApplyPlotsLayersRecursive(toggleObject);
                // Set the toggle's text and color
                Toggle toggle = toggleObject.GetComponent<Toggle>();
                toggle.GetComponentInChildren<TextMeshProUGUI>().text = dataManager.DataTable.ColumnNames[0] + " " + clusters[i].Id;
                toggle.GetComponentInChildren<TextMeshProUGUI>().color = clusters[i].Color;
                toggle.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>().color = clusters[i].Color;
                clusterToggles[i] = toggle;
                // Add a callback for when the toggle is... toggled
                int clusterIdx = i;
                toggle.onValueChanged.AddListener(delegate { ToggleCluster(clusterIdx); });

                // Initialize saved linked attributes for this cluster
                savedClusterLinkedAttributes[i] = new LinkedIndices.LinkedAttributes[clusters[i].EndIdx - clusters[i].StartIdx];
            }
        }

        /// <summary>
        /// Toggles specified cluster's visibility.
        /// </summary>
        /// <param name="clusterIdx"> Index into <see cref="clusterToggles"/> array. </param>
        private void ToggleCluster(int clusterIdx)
        {
            // Determine the start/end indices of the cluster
            Cluster cluster = ((ClusterDataTable)dataManager.DataTable).Clusters[clusterIdx];
            int clusterStartIdx = cluster.StartIdx;
            int clusterEndidx = cluster.EndIdx;
            // Toggled on
            if (clusterToggles[clusterIdx].isOn)
            {
                // If nothing else is masked, it makes most sense to simply fully unmask this cluster
                if (dataManager.NothingMasked)
                {
                    for (int i = clusterStartIdx; i < clusterEndidx; i++)
                    {
                        dataManager.LinkedIndices[i].Masked = false;
                    }
                }
                // Otherwise, let's return all of the points in the cluster to their saved configuration
                else
                {
                    for (int i = clusterStartIdx; i < clusterEndidx; i++)
                    {
                        dataManager.LinkedIndices[i] =
                            new LinkedIndices.LinkedAttributes(savedClusterLinkedAttributes[clusterIdx][i - clusterStartIdx]);
                    }
                    dataManager.LinkedIndices.LinkedAttributesChanged = true;
                }
                cluster.Enabled = true;
            }
            // Toggled off
            else
            {
                // Save the cluster's linked state and mask all of its points
                for (int i = clusterStartIdx; i < clusterEndidx; i++)
                {
                    dataManager.LinkedIndices[i].Highlighted = false;
                    savedClusterLinkedAttributes[clusterIdx][i - clusterStartIdx] = 
                        new LinkedIndices.LinkedAttributes(dataManager.LinkedIndices[i]);
                    dataManager.LinkedIndices[i].Masked = true;
                }
                cluster.Enabled = false;
                // Toggling the cluster off unhighlighted some data points
                // so we should check to see if anything is still selected
                dataPlotManager.CheckSelection();
            }
        }
    }
}
