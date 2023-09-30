namespace NuVelocity
{
    [PropertyRoot("CSequenceFrameInfoList", "Sequence Frame Info List")]
    public class SequenceFrameInfoList
    {
        [Property("Frame Infos")]
        public FrameInfo[] Values { get; set; }

        [Property("WasRLE")]
        public bool WasRle { get; set; }

        [Property("Flags")]
        public SequenceFlags Flags { get; set; }

        [Property("BlitType")]
        [PropertyExclude(EngineSource.From2008)]
        public int BlitType
        {
            get
            {
                return (int)BlitTypeEnum;
            }
            set
            {
                BlitTypeEnum = (BlitType)value;
            }
        }

        [Property("Blit Type")]
        [PropertyInclude(EngineSource.From2008)]
        public BlitType BlitTypeEnum { get; set; }

        [Property("FPS")]
        public int FramesPerSecond { get; set; }
    }
}
