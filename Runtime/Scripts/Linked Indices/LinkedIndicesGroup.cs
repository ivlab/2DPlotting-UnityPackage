using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{
    /// <summary>
    /// Defines a linked indices object and the group of listeners that 
    /// should be notified when changes are made to it.
    /// </summary>
    public class LinkedIndicesGroup : MonoBehaviour
    {
        /// <summary> List of all linked indices listeners part of this group. </summary>
        [SerializeField] private List<LinkedIndicesListener> linkedIndicesListeners;
        private LinkedIndices linkedIndices;

        /// <summary>
        /// Gets the linked indices associated with the current data table the manager is using.
        /// Can also set the linked indices, though this will cause all current plots to be removed
        /// to avoid linkage issues.
        /// </summary>
        public LinkedIndices LinkedIndices
        {
            get => linkedIndices;
            set
            {
                linkedIndices = value;
                // Notify all listeners that a new set of linked indices is being used
                foreach (LinkedIndicesListener listener in linkedIndicesListeners)
                {
                    listener.NewLinkedIndicesSet(linkedIndices);
                }
            }
        }

        /// <summary>
        /// Gets and sets the list of linked indices listeners.
        /// </summary>
        // public List<LinkedIndicesListener> LinkedIndicesListeners
        // {
        //     get => linkedIndicesListeners;
        //     set => linkedIndicesListeners = value;
        // }

        void LateUpdate()
        {
            // Only notify listeners if a linked index attribute has been changed
            if (linkedIndices.LinkedAttributesChanged && linkedIndicesListeners != null)
            {
                NotifyListenersOfChanges();
                
                // Reset the linked attributes changed flag
                linkedIndices.LinkedAttributesChanged = false;
            }
        }

        /// <summary>
        /// Notifies listeners attached to this linked index group of changes made to linked indices.
        /// </summary>
        /// <remarks>
        /// We call this method in Unity's LateUpdate() so as to ensure that all
        /// changes to linked indices for the current frame have been accounted for (since those
        /// should occur in Update())
        /// </remarks>
        private void NotifyListenersOfChanges()
        {
            for (int i = 0; i < linkedIndices.Size; i++)
            {
                // Only send notifications for indices that have been changed
                if (linkedIndices[i].LinkedAttributeChanged)
                {
                    foreach (LinkedIndicesListener listener in linkedIndicesListeners)
                    {
                        listener.LinkedIndexChanged(i, linkedIndices[i]);
                    }

                    linkedIndices[i].LinkedAttributeChanged = false;
                }
            }

            // Notify that indices have been changed this frame
            foreach (LinkedIndicesListener listener in linkedIndicesListeners)
            {
                listener.LinkedIndicesChanged();
            }
        }

        /// <summary>
        /// Adds a linked indices listener to this group.
        /// </summary>
        /// <param name="listener">Linked indices listener to be added.</param>
        public void AddListener(LinkedIndicesListener listener)
        {
            linkedIndicesListeners.Add(listener);
        }

        /// <summary>
        /// Removes a linked indices listener from this group.
        /// </summary>
        /// <param name="listener">Linked indices listener to be removed.</param>
        public void RemoveListener(LinkedIndicesListener listener)
        {
            if (linkedIndicesListeners.Contains(listener))
                linkedIndicesListeners.Remove(listener);
        }

        /// <summary>
        /// Returns the number of listeners in this group.
        /// </summary>
        /// <returns></returns>
        public int ListenerCount()
        {
            return linkedIndicesListeners.Count;
        }
    }
}
