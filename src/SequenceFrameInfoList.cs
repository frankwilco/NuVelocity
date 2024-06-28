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
        [PropertyExclude(PropertySerializationFlags.HasTextBlitType)]
        public int BlitType
        {
            get
            {
                return (int)BlitTypeEnum;
            }
            set
            {
                BlitTypeEnum = (BlitType1)value;
            }
        }

        [Property("Blit Type")]
        [PropertyInclude(PropertySerializationFlags.HasTextBlitType)]
        public BlitType1 BlitTypeEnum { get; set; }

        [Property("FPS")]
        public float FramesPerSecond { get; set; }
    }
}
