namespace NuVelocity.IO
{
    public enum BlitType
    {
        [Property("Transparent Mask")]
        TransparentMask   = 0,
        [Property("Normal")]
        Normal            = 1,
        [Property("Blend Black Bias")]
        BlendBlackBias    = 2,
        [Property("Blend Test Light")]
        BlendTestLight    = 3,
        [Property("Blit As Shadow")]
        BlitAsShadow      = 4,
        [Property("Normal Scale")]
        NormalScale       = 5,
        [Property("Blend Amplify Light")]
        BlendAmplifyLight = 6,
    }
}
