namespace NuVelocity
{
    [PropertyRoot("CSequenceOfCoordinates", "Sequence of Coordinates")]
    public class SequenceOfCoordinates
    {
        public List<Coordinate> Coordinates { get; set; }

        [Property("Coordinates")]
        protected Coordinate[] CoordinatesArray
        {
            get { return Coordinates.ToArray(); }
            set { Coordinates = new(value); }
        }
    }
}
