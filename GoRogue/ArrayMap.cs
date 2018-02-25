﻿using System;

namespace GoRogue
{
    /// <summary>
    /// Default implementation of the ISettableMapView interface, exposing the interface using an
    /// actual 2D array to store data.
    /// </summary>
    /// <typeparam name="T">The type of value being stored.</typeparam>
    public class ArrayMap<T> : ISettableMapView<T>, ICloneable
    {
        private T[,] array;

        /// <summary>
        /// Constructor. Takes width and height of array.
        /// </summary>
        /// <param name="width">Width of array.</param>
        /// <param name="height">Height of array.</param>
        public ArrayMap(int width, int height)
        {
            array = new T[width, height];
        }

        /// <summary>
        /// The height of the array.
        /// </summary>
        public int Height { get => array.GetLength(1); }

        /// <summary>
        /// The width of the array.
        /// </summary>
        public int Width { get => array.GetLength(0); }

        /// <summary>
        /// Given an X and Y value, returns/sets the "value" associated with that location.
        /// </summary>
        /// <param name="x">X-value of location.</param>
        /// <param name="y">Y-value of location.</param>
        /// <returns>The "value" associated with that location.</returns>
        public T this[int x, int y]
        {
            get => array[x, y];
            set => array[x, y] = value;
        }

        /// <summary>
        /// Given a Coord, returns/sets the "value" associated with that location.
        /// </summary>
        /// <param name="pos">Location to get/set the value for.</param>
        /// <returns>THe "value" associated with the provided location.</returns>
        public T this[Coord pos]
        {
            get => array[pos.X, pos.Y];
            set => array[pos.X, pos.Y] = value;
        }

        /// <summary>
        /// Performs deep copy of array map.
        /// </summary>
        /// <returns>The cloned ArrayMap.</returns>
        public object Clone()
        {
            var newObj = new ArrayMap<T>(Width, Height);

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    newObj[x, y] = array[x, y];

            return newObj;
        }

        /// <summary>
        /// Returns a string representation of the 2D array.
        /// </summary>
        /// <returns>A string representation of the 2D array.</returns>
        public override string ToString() => ToString((T elem) => elem.ToString());

        /// <summary>
        /// Returns a string representation of the 2D array, using the elementMap
        /// function given to determine what string represents which value.
        /// </summary>
        /// <remarks>
        /// This could be used, for example, on an ArrayMap of boolean values, to output '#' for
        /// false values, and '.' for true values.
        /// </remarks>
        /// <param name="elementMap">Function determining the string representation of each element.</param>
        /// <returns>A string representation of the 2D array.</returns>
        public string ToString(Func<T, string> elementMap)
        {
            string result = "";

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                    result += elementMap(array[x, y]) + " ";

                result += '\n';
            }

            return result;
        }
    }
}