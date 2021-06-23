/* TemplateExample.cs
 *
 * Copyright (C) 2021, University of Minnesota
 * Authors: Bridger Herman <herma582@umn.edu>
 *
 */

using UnityEngine;

// Always namespace your Unity code!
namespace IVLab.Template
{
    /// <summary>
    /// Simple example of what can be done in C#. This text is an XML comment, and will show up nicely formatted in the documentation associated with this class.
    /// </summary>
    public class TemplateExample : MonoBehaviour
    {
        private int _counter = 0;

        // Standard MonoBehaviour Update() method - will not get auto-documented because it's protected, not Public.
        void Update()
        {
            _counter = DoWork(_counter);
            if (_counter % 100 == 0)
            {
                Debug.Log(_counter);
            }
        }

        /// <summary>
        /// Do some work.
        /// </summary>
        /// <param name="test">An integer to be incremented</param>
        /// <returns>Returns the `test` parameter + 1</returns>
        /// <example><code>
        /// int x = DoWork(10);
        /// Debug.Log(x); // prints 11
        /// </code></example>
        public int DoWork(int test)
        {
            return test + 1;
        }
    }
}