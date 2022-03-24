using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace IVLab.Plotting
{
    [System.Serializable]
    public class AttributeChangedEvent : UnityEvent<int, LinkedIndices.IndexAttributes>
    {
        private int listenerCount = 0;
        public int ListenerCount { get => listenerCount; }

        new public void AddListener(UnityAction<int, LinkedIndices.IndexAttributes> unityAction)
        {
            base.AddListener(unityAction);
            listenerCount++;
        }

        new public void RemoveListener(UnityAction<int, LinkedIndices.IndexAttributes> unityAction)
        {
            base.RemoveListener(unityAction);
            listenerCount--;
        }
    }

    /// <summary>
    /// This class provides an "index space" wherein each index consists of a number attributes,
    /// such as whether or not that index (and the data that is linked with it) is highlighted or masked.
    /// </summary>
    public class LinkedIndices : MonoBehaviour
    {
        [SerializeField] private AttributeChangedEvent onIndexAttributeChanged;
        [SerializeField] private UnityEvent onAnyIndexAttributeChanged;
        [SerializeField] private UnityEvent onIndicesReinitialized;

        private int size;
        private int highlightedCount;
        private int maskedCount;
        private bool anyIndexAttributeChanged;
        /// <summary> Array of index attributes that make up the linked indices. </summary>
        private IndexAttributes[] indexAttributes;

        /// <summary> Event triggered for each index that had an attribute change during the current frame. </summary>
        /// <remarks> Invoked in LateUpdate for each index that had an attribute change that frame. </remarks>
        public AttributeChangedEvent OnIndexAttributeChanged { get => onIndexAttributeChanged; }
        /// <summary> Event triggered at most once per frame when any linked index attribute has changed. </summary>
        /// <remarks>
        /// Invoked in LateUpdate after all <see cref="OnIndexAttributeChanged"/> invocations for that
        /// frame have been made. Can thus also be used as a callback for when to apply styling changes.
        /// </remarks>
        public UnityEvent OnAnyIndexAttributeChanged { get => onAnyIndexAttributeChanged; }
        /// <summary> Event triggered when linked indices <see cref="Init"/> method is called. </summary>
        public UnityEvent OnIndicesReinitialized { get => onIndicesReinitialized; }
        /// <summary> Total number of indices. </summary>
        public int Size { get => size; }
        /// <summary> Number of indices with highlighted attribute currently set to true. </summary>
        public int HighlightedCount { get => highlightedCount; }
        /// <summary> Number of indices with masked attribute currently set to true. </summary>
        public int MaskedCount { get => maskedCount; }
        /// <summary> An automatically toggled flag that indicates if any attribute has been changed. </summary>
        public bool AnyIndexAttributeChanged
        {
            get => anyIndexAttributeChanged;
            set => anyIndexAttributeChanged = value;
        }
        /// <summary>
        /// Allows attributes to be accessed and set with array accessor, e.g. linkedIndices[i].
        /// </summary>
        public IndexAttributes this[int index]
        {
            get => indexAttributes[index];
            set {
                // Check to see if any linked attributes were changed
                if (indexAttributes[index].Highlighted != value.Highlighted)
                {
                    anyIndexAttributeChanged = true;
                    indexAttributes[index].IndexAttributeChanged = true;
                    indexAttributes[index].Highlighted = value.Highlighted;
                    highlightedCount += value.Highlighted ? 1 : -1;
                }
                if (indexAttributes[index].Masked != value.Masked)
                {
                    anyIndexAttributeChanged = true;
                    indexAttributes[index].IndexAttributeChanged = true;
                    indexAttributes[index].Masked = value.Masked;
                    maskedCount += value.Masked ? 1 : -1;
                }
            }
        }

        /// <summary>
        /// Constructor to initialize linked attributes array with set size.
        /// </summary>
        /// <param name="size">Number of indices.</param>
        public void Init(int size)
        {
            this.size = size;
            anyIndexAttributeChanged = false;
            indexAttributes = new IndexAttributes[size];
            for (int i = 0; i < size; i++)
            {
                indexAttributes[i] = new IndexAttributes(this);
            }
            highlightedCount = 0;
            maskedCount = 0;
            onIndicesReinitialized.Invoke();
        }

        /// <summary>
        /// Resets the attributes of the linked indices.
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < size; i++)
            {
                indexAttributes[i].Reset();
            }
        }

        void LateUpdate()
        {
            // Only notify listeners if a linked index attribute has been changed
            if (anyIndexAttributeChanged)
            {
                NotifyListenersOfChanges();

                // Reset the linked attributes changed flag
                anyIndexAttributeChanged = false;
            }
        }

        /// <summary>
        /// Notifies listeners subscribed to this set of linked indices of changes made to linked indices.
        /// </summary>
        /// <remarks>
        /// We call this method in Unity's LateUpdate() so as to ensure that all
        /// changes to linked indices for the current frame have been accounted for (since those
        /// should occur in Update())
        /// </remarks>
        private void NotifyListenersOfChanges()
        {
            for (int i = 0; i < size; i++)
            {
                // Only send notifications for indices that have been changed
                if (indexAttributes[i].IndexAttributeChanged)
                {
                    onIndexAttributeChanged.Invoke(i, indexAttributes[i]);

                    indexAttributes[i].IndexAttributeChanged = false;
                }
            }

            // Notify that indices have been changed this frame
            onAnyIndexAttributeChanged.Invoke();
        }

        /// <summary>
        /// This class acts as a container for the attributes attached to each individual index, as used
        /// by <see cref="LinkedIndices"/>.
        /// </summary>
        public class IndexAttributes
        {
            /// <summary> Reference to the linked indices array that the linked attribute is a part of. </summary>
            private LinkedIndices _linkedIndices;
            private bool _highlighted = false;
            private bool _masked = false;
            private bool _indexAttributeChanged;

            /// <summary>
            /// Constructor takes a reference to the LinkedIndices object that holds 
            /// the array of which this LinkedAtrribute is a part of.
            /// </summary>
            /// <remarks>
            /// Constructors do not increment highlighted/masked counters, as that is the responsibility of
            /// <see cref="LinkedIndices"/>
            /// </remarks>
            public IndexAttributes(LinkedIndices linkedIndices, bool highlighted = false, bool masked = false)
            {
                _linkedIndices = linkedIndices;
                _highlighted = highlighted;
                _masked = masked;
                _indexAttributeChanged = false;
            }

            /// <summary>
            /// Copy constructor used to construct linked attributes out of another set of linked attributes.
            /// </summary>
            /// <remarks>
            /// Constructors do not increment highlighted/masked counters, as that is the responsibility of
            /// <see cref="LinkedIndices"/>
            /// </remarks>
            public IndexAttributes(IndexAttributes indexAttributes)
            {
                _linkedIndices = indexAttributes._linkedIndices;
                _highlighted = indexAttributes._highlighted;
                _masked = indexAttributes._masked;
                _indexAttributeChanged = false;
            }

            /// <summary> Flags whether or not this index is highlighted (selected),
            /// automatically toggling <see cref="IndexAttributeChanged"/> and
            /// <see cref="AnyIndexAttributeChanged"/> to true if the value
            /// is indeed changed. </summary>
            public bool Highlighted
            {
                get { return _highlighted; }
                set
                {
                    // Toggles "changed" flag if the value was changed
                    if (_highlighted != value)
                    {
                        _linkedIndices.anyIndexAttributeChanged = true;
                        _indexAttributeChanged = true;
                        _highlighted = value;
                        _linkedIndices.highlightedCount += _highlighted ? 1 : -1;
                    }
                }
            }


            /// <summary> Flags whether or not this index is masked (filtered),
            /// automatically toggling <see cref="IndexAttributeChanged"/> and
            /// <see cref="AnyIndexAttributeChanged"/> to true if the value
            /// is indeed changed. </summary>
            public bool Masked
            {
                get { return _masked; }
                set
                {
                    // Toggles "changed" flag if the value was changed
                    if (_masked != value)
                    {
                        _linkedIndices.anyIndexAttributeChanged = true;
                        _indexAttributeChanged = true;
                        _masked = value;
                        _linkedIndices.maskedCount += _masked ? 1 : -1;
                    }
                }
            }

            /// <summary> 
            /// A flag that signals whether or not any of the linked attributes have
            /// changed. This flag is automatically set to true when an attribute changes, but
            /// it must manually be set to false. 
            /// </summary>
            /// <example>
            /// For example, a common sequence of use for this attribute would occur along the lines of:
            /// <code>
            /// "At some point, the 'Highlighted' attribute of linked index i is set to true."
            /// "If this attribute was previously false, i.e we are now changing its value, its IndexAttributeChanged flag will automatically be set to true"
            /// "In other words, there's no need for us to directly set `linkedIndices[i].IndexAttributeChanged = true;`"
            /// 
            /// linkedIndices[i].Highlighted = true;
            /// 
            ///             . 
            ///             . 
            ///             .
            /// 
            /// "Later on, we can choose to work with the linked index attributes of i only if they have changed"
            /// if (linkedIndices[i].IndexAttributeChanged) {
            ///     if (linkedIndices[i].Highlighted) {
            ///         Debug.Log("Index " + i + " changed to highlighted state.");
            ///     }
            ///     else
            ///     {
            ///         Debug.Log("Index " + i + " changed to un-highlighted state.");
            ///     }
            ///     
            ///     "We do have to be sure to toggle the flag back to false after we use it, however"
            ///     linkedIndices[i].IndexAttributeChanged = false;
            /// }
            /// </code>
            /// </example>
            public bool IndexAttributeChanged
            {
                get => _indexAttributeChanged;
                set => _indexAttributeChanged = value;
            }

            /// <summary>
            /// Resets linked attribute to unhighlighted/unmasked state.
            /// </summary>
            public void Reset()
            {
                if (_highlighted != false)
                {
                    _linkedIndices.anyIndexAttributeChanged = true;
                    _indexAttributeChanged = true;
                    _highlighted = false;
                    _linkedIndices.highlightedCount--;
                }
                if (_masked != false)
                {
                    _linkedIndices.anyIndexAttributeChanged = true;
                    _indexAttributeChanged = true;
                    _masked = false;
                    _linkedIndices.maskedCount--;
                }
            }
        }
    }
}
