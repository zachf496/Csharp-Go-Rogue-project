﻿namespace GoRogue.MapViews
{
    /// <summary>
    /// Since some algorithms that use MapViews can be expensive to run entirely on large maps (such as GoalMaps), Viewport is a class that
    /// effectively creates and maintains a "viewport" of the map.  Its indexers perform relative to absolute coordinate translations, and return
    /// the proper value of type T from the underlying map..
    /// </summary>
    /// <typeparam name="T">The type being exposed by the MapView.</typeparam>
    public class Viewport<T> : IMapView<T>
    {
        private BoundedRectangle _boundedRect;

        /// <summary>
        /// The area of the base MapView that this Viewport is exposing.  This property does NOT expose a get accessor - to get the value, use GetViewArea() (which returns a reference).
        /// When set, the rectangle is automatically restricted by the edges of the map as necessary.
        /// </summary>
        public Rectangle ViewArea
        {
            set => _boundedRect.Area = value;
        }

        /// <summary>
        /// The MapView that this Viewport is exposing values from.
        /// </summary>
        public IMapView<T> MapView { get; private set; } 

        /// <summary>
        /// The width of the ViewArea.
        /// </summary>
        public int Width
        {
            get => GetViewArea().Width;
        }

        /// <summary>
        /// The height of the ViewArea.
        /// </summary>
        public int Height
        {
            get => GetViewArea().Height;
        }

        /// <summary>
        /// Given a position in relative coordinates, returns the "value" associated with that location in absolute coordinates.
        /// </summary>
        /// <param name="relativePosition">Viewport-relative position of the location to retrieve the value for.</param>
        /// <returns>The "value" associated with the absolute location represented on the underlying MapView.</returns>
        public T this[Coord relativePosition] => MapView[GetViewArea().MinCorner + relativePosition];

        /// <summary>
        /// Given an X and Y value in relative coordinates, returns the "value" associated with that location in absolute coordinates.
        /// </summary>
        /// <param name="x">Viewport-relative X-value of location.</param>
        /// <param name="y">Viewport-relative Y-value of location.</param>
        /// <returns>The "value" associated with the absolute location represented on the underlying MapView.</returns>
        public T this[int relativeX, int relativeY] => MapView[GetViewArea().X + relativeX, GetViewArea().Y + relativeY];

        /// <summary>
        /// Constructor.  Takes the MapView to represent, and the initial ViewArea for that map.
        /// </summary>
        /// <param name="mapView">The map view being represented.</param>
        /// <param name="viewArea">The initial ViewArea for that map.</param>
        public Viewport(IMapView<T> mapView, Rectangle viewArea)
        {
            MapView = mapView;
            _boundedRect = new BoundedRectangle(viewArea, MapView.Bounds());
            ViewArea = viewArea;
        }

        /// <summary>
        /// Constructor.  Takes the MapView to represent.  Initial ViewArea will be the entire MapView.
        /// </summary>
        /// <param name="mapView">The MapView to represent.</param>
        public Viewport(IMapView<T> mapView)
            : this(mapView, mapView.Bounds()) { }

        /// <summary>
        /// Returns a reference to the ViewArea.  To set, see the ViewArea property.
        /// </summary>
        /// <returns>A reference to the ViewArea.</returns>
        public ref Rectangle GetViewArea() => ref _boundedRect.GetArea();
    }
}
