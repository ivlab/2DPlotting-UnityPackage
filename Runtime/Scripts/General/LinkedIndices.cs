using UnityEngine;

namespace IVLab.Plotting
{
    /// <summary>
    /// This class provides an "index space" where each index has additional attributes/data attached to it.
    /// </summary>
    public class LinkedIndices
    {
        /// <summary> Total number of indices (and "data points"). </summary>
        private int _size;
        /// <summary> Automatically toggled flag that indicates if any attributes have been changed. </summary>
        private bool _linkedAttributesChanged;
        /// <summary> Array of attributes linked to the indices. </summary>
        private LinkedAttributes[] _linkedAttributes;

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
        }

        /// <summary> Gets <see cref="_size"/>. </summary>
        public int Size
        {
            get => _size;
        }
        /// <summary> Gets and sets <see cref="_linkedAttributesChanged"/>. </summary>
        public bool LinkedAttributesChanged
        {
            get => _linkedAttributesChanged;
            set => _linkedAttributesChanged = value;
        }

        /// <summary>
        /// Allows attributes to be accesed with classic array accesor, e.g. linkedIndices[i].
        /// </summary>
        public LinkedAttributes this[int index]
        {
            get => _linkedAttributes[index];
        }

        /// <summary>
        /// This class acts as a container for attributes attached to an individual index.
        /// </summary>
        public class LinkedAttributes
        {
            /// <summary> Reference to the linked indices array that the linked attribute is a part of. </summary>
            private LinkedIndices _linkedIndices;
            /// <summary> Flags whether or not this index is highlighted (selected). </summary>
            private bool _highlighted;  // Flags whether or not this index is highlighted (selected)
            /// <summary> Flags whether or not this index is masked (filtered). </summary>
            private bool _masked;
            /// <summary> Automatically flags whether or not this attribute has changed. </summary>
            private bool _linkedAttributeChanged;

            /// <summary>
            ///  Constructor takes a reference to the LinkedIndices object that holds 
            ///  the array of which this LinkedAtrribute is a part of.
            /// </summary>
            public LinkedAttributes(LinkedIndices linkedIndices)
            {
                _linkedIndices = linkedIndices;
                _highlighted = false;
                _masked = false;
                _linkedAttributeChanged = false;
            }

            /// <summary> Gets and sets <see cref="_highlighted"/>, 
            /// automatically toggling <see cref="_linkedAttributeChanged"/> and
            /// <see cref="_linkedAttributesChanged"/> to true if the value
            /// is changed. </summary>
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
                    }
                }
            }

            /// <summary> Gets and sets <see cref="_masked"/>, 
            /// automatically toggling <see cref="_linkedAttributeChanged"/> and
            /// <see cref="_linkedAttributesChanged"/> to true if the value
            /// is changed. </summary>
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
                    }
                }
            }

            /// <summary> Gets and sets <see cref="_linkedAttributeChanged"/>. </summary>
            public bool LinkedAttributeChanged
            {
                get => _linkedAttributeChanged;
                set => _linkedAttributeChanged = value;
            }
        }
    }
}
