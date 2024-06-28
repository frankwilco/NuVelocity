namespace NuVelocity
{
    /*
     * The integer representation of the Blit Type property value has changed
     * across the different versions of the Velocity Engine, making it
     * difficult to automatically determine the correct value without knowing
     * the source game.
     *
     * (BT1) Ricochet Infinity HD, Swarm Gold, Build In Time, Costume Chaos,
     *       and Big Kahuna Reef 3
     * -> Text.
     * (BT1) Ricochet Infinity
     * -> Transparent Mask is 0.
     * (BT2) Ricochet Lost Worlds
     * -> Transparent Mask is 1.
     * (BT3) Big Kahuna Reef 1/2/Words
     * -> Transparent Mask is 2.
     *    Removed: Normal Scale.
     * (BT4) Mosaic Tomb of Mystery
     * -> Transparent Mask is 0.
     *    Removed: Normal Scale, Blit As Shadow.
     * (BT5) Wik and the Fable of Souls
     * -> Transparent Mask is 1.
     *    Removed: Normal Scale, Blit As Shadow.
     * (BT6 TODO) Ricochet Xtreme, Swarm
     * -> Values in use: 2, 5, and 6.
     *    Swarm uses Normal.
     *    RX uses Transparent Mask.
     */

    public enum BlitType1
    {
        [Property("Transparent Mask")]
        TransparentMask = 0,
        [Property("Normal")]
        Normal = 1,
        [Property("Blend Black Bias")]
        BlendBlackBias = 2,
        [Property("Blend Test Light")]
        BlendTestLight = 3,
        [Property("Blit As Shadow")]
        BlitAsShadow = 4,
        [Property("Normal Scale")]
        NormalScale = 5,
        [Property("Blend Amplify Light")]
        BlendAmplifyLight = 6,
    }

    public enum BlitType2
    {
        [Property("Transparent Mask")]
        TransparentMask = 1,
        [Property("Normal")]
        Normal = 0,
        [Property("Blend Black Bias")]
        BlendBlackBias = 2,
        [Property("Blend Test Light")]
        BlendTestLight = 3,
        [Property("Blit As Shadow")]
        BlitAsShadow = 4,
        [Property("Normal Scale")]
        NormalScale = 5,
        [Property("Blend Amplify Light")]
        BlendAmplifyLight = 6,
    }

    public enum BlitType3
    {
        [Property("Transparent Mask")]
        TransparentMask = 2,
        [Property("Normal")]
        Normal = 3,
        [Property("Blend Black Bias")]
        BlendBlackBias = 0,
        [Property("Blend Test Light")]
        BlendTestLight = 1,
        [Property("Blit As Shadow")]
        BlitAsShadow = 4,
        [Property("Blend Amplify Light")]
        BlendAmplifyLight = 5,
    }

    public enum BlitType4
    {
        [Property("Transparent Mask")]
        TransparentMask = 0,
        [Property("Normal")]
        Normal = 1,
        [Property("Blend Black Bias")]
        BlendBlackBias = 3,
        [Property("Blend Test Light")]
        BlendTestLight = 4,
        [Property("Blend Amplify Light")]
        BlendAmplifyLight = 2,
    }

    public enum BlitType5
    {
        [Property("Transparent Mask")]
        TransparentMask = 1,
        [Property("Normal")]
        Normal = 0,
        [Property("Blend Black Bias")]
        BlendBlackBias = 2,
        [Property("Blend Test Light")]
        BlendTestLight = 3,
        [Property("Blend Amplify Light")]
        BlendAmplifyLight = 4,
    }
}
