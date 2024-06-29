namespace NuVelocity.Graphics
{
    [PropertyRoot("CFrameInfo", "Frame Info")]
    public class FrameInfo
    {
        [Property("Left")]
        public int Left { get; set; }

        [Property("Top")]
        public int Top { get; set; }

        [Property("Right")]
        public int Right { get; set; }

        [Property("Bottom")]
        public int Bottom { get; set; }

        [Property("UpperLeftXOffset")]
        public int UpperLeftXOffset { get; set; }

        [Property("UpperLeftYOffset")]
        public int UpperLeftYOffset { get; set; }
    }
}
