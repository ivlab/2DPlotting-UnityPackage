using UnityEngine;

namespace IVLab.Plotting
{
    /// <summary>
    /// This class provides an "index space" wherein each index is allowed to have additional 
    /// attributes/data attached to it, such as whether or not that index (and the data that correlates
    /// to it) is highlighted or masked.
    /// </summary>
    public class LinkedIndices
    {
        private int _size;
        private bool _linkedAttributesChanged;
        /// <summary> Array of attributes linked to the indices. </summary>
        private LinkedAttributes[] _linkedAttributes;
        private int _highlightedCount;
        private int _maskedCount;

        /// <summary>
        /// Constructor to initialize linked attributes array.
        /// </summary>
        /// <param name="size">Number of indices.</param>
        public LinkedIndices(int size)
        {
            _size = size;
            _linkedAttributesChanged = false;
            _linkedAttributes = new LinkedAttributes[_size];
            for (int i = 0; i < _size; i++)
            {
                _linkedAttributes[i] = new LinkedAttributes(this);
            }
            _highlightedCount = 0;
            _maskedCount = 0;
        }

        /// <summary> Total number of indices (and "data points"). </summary>
        public int Size
        {
            get => _size;
        }

        /// <summary> Automatically toggled flag that indicates if any attributes have been changed. </summary>
        public bool LinkedAttributesChanged
        {
            get => _linkedAttributesChanged;
            set => _linkedAttributesChanged = value;
        }

        /// <summary> Number of indices with highlighted flag currently set to true. </summary>
        public int HighlightedCount
        {
            get => _highlightedCount;
        }

        /// <summary> Number of indices with masked flag currently set to true. </summary>
        public int MaskedCount
        {
            get => _maskedCount;
        }

        /// <summary>
        /// Allows attributes to be accessed and set with array accessor, e.g. linkedIndices[i].
        /// </summary>
        public LinkedAttributes this[int index]
        {
            get => _linkedAttributes[index];
            set {
                // Check to see if any linked attributes were changed
                if (_linkedAttributes[index].Highlighted != value.Highlighted)
                {
                    _linkedAttributesChanged = true;
                    _linkedAttributes[index].LinkedAttributeChanged = true;
                    _linkedAttributes[index].Highlighted = value.Highlighted;
                    _highlightedCount += value.Highlighted ? 1 : -1;
                }
                if (_linkedAttributes[index].Masked != value.Masked)
                {
                    _linkedAttributesChanged = true;
                    _linkedAttributes[index].LinkedAttributeChanged = true;
                    _linkedAttributes[index].Masked = value.Masked;
                    _maskedCount += value.Masked ? 1 : -1;
                }
            }
        }

        /// <summary>
        /// Resets the attributes of the linked indices.
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < _size; i++)
            {
                _linkedAttributes[i].Reset();
            }
        }

        /// <summary>
        /// This class acts as a container for the attributes attached to each individual index, as used
        /// by <see cref="LinkedIndices"/>.
        /// </summary>
        public class LinkedAttributes
        {
            /// <summary> Reference to the linked indices array that the linked attribute is a part of. </summary>
            private LinkedIndices _linkedIndices;
            private bool _highlighted = false;
            private bool _masked = false;
            private bool _linkedAttributeChanged;

            /// <summary>
            /// Constructor takes a reference to the LinkedIndices object that holds 
            /// the array of which this LinkedAtrribute is a part of.
            /// </summary>
            /// <remarks>
            /// Constructors do not increment highlighted/masked counters, as that is the responsibility of
            /// <see cref="LinkedIndices"/>
            /// </remarks>
            public LinkedAttributes(LinkedIndices linkedIndices, bool highlighted = false, bool masked = false)
            {
                _linkedIndices = linkedIndices;
                _highlighted = highlighted;
                _masked = masked;
                _linkedAttributeChanged = false;
            }

            /// <summary>
            /// Copy constructor used to construct linked attributes out of another set of linked attributes.
            /// </summary>
            /// <remarks>
            /// Constructors do not increment highlighted/masked counters, as that is the responsibility of
            /// <see cref="LinkedIndices"/>
            /// </remarks>
            public LinkedAttributes(LinkedAttributes linkedAttributes)
            {
                _linkedIndices = linkedAttributes._linkedIndices;
                _highlighted = linkedAttributes._highlighted;
                _masked = linkedAttributes._masked;
                _linkedAttributeChanged = false;
            }

            /// <summary> Flags whether or not this index is highlighted (selected),
            /// automatically toggling <see cref="LinkedAttributeChanged"/> and
            /// <see cref="LinkedAttributesChanged"/> to true if the value
            /// is indeed changed. </summary>
            public bool Highlighted
            {
                get { return _highlighted; }
                set
                {
                    // Toggles "changed" flag if the value was changed
                    if (_highlighted != value)
                    {
                        _linkedIndices._linkedAttributesChanged = true;
                        _linkedAttributeChanged = true;
                        _highlighted = value;
                        _linkedIndices._highlightedCount += _highlighted ? 1 : -1;
                    }
                }
            }


            /// <summary> Flags whether or not this index is masked (filtered),
            /// automatically toggling <see cref="LinkedAttributeChanged"/> and
            /// <see cref="LinkedAttributesChanged"/> to true if the value
            /// is indeed changed. </summary>
            public bool Masked
            {
                get { return _masked; }
                set
                {
                    // Toggles "changed" flag if the value was changed
                    if (_masked != value)
                    {
                        _linkedIndices._linkedAttributesChanged = true;
                        _linkedAttributeChanged = true;
                        _masked = value;
                        _linkedIndices._maskedCount += _masked ? 1 : -1;
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
            /// "If this attribute was previously false, i.e we are now changing its value, its LinkedAttributeChanged flag will automatically be set to true"
            /// "In other words, there's no need for us to directly set `linkedIndices[i].LinkedAttributeChanged = true;`"
            /// 
            /// linkedIndices[i].Highlighted = true;
            /// 
            ///             . 
            ///             . 
            ///             .
            /// 
            /// "Later on, we can choose to work with the linked index attributes of i only if they have changed"
            /// if (linkedIndices[i].LinkedAttributeChanged) {
            ///     if (linkedIndices[i].Highlighted) {
            ///         Debug.Log("Index " + i + " changed to highlighted state.");
            ///     }
            ///     else
            ///     {
            ///         Debug.Log("Index " + i + " changed to un-highlighted state.");
            ///     }
            ///     
            ///     "We do have to be sure to toggle the flag back to false after we use it, however"
            ///     linkedIndices[i].LinkedAttributeChanged = false;
            /// }
            /// </code>
            /// </example>
            public bool LinkedAttributeChanged
            {
                get => _linkedAttributeChanged;
                set => _linkedAttributeChanged = value;
            }

            /// <summary>
            /// Resets linked attribute to unhighlighted/unmasked state.
            /// </summary>
            public void Reset()
            {
                if (_highlighted != false)
                {
                    _linkedIndices._linkedAttributesChanged = true;
                    _linkedAttributeChanged = true;
                    _highlighted = false;
                    _linkedIndices._highlightedCount--;
                }
                if (_masked != false)
                {
                    _linkedIndices._linkedAttributesChanged = true;
                    _linkedAttributeChanged = true;
                    _masked = false;
                    _linkedIndices._maskedCount--;
                }
            }
        }
    }
}
