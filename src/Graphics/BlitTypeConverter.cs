namespace NuVelocity.Graphics
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

    public enum BlitTypeRevision
    {
        Type1,
        Type2,
        Type3,
        Type4,
        Type5,
    }

    public static class BlitTypeConverter
    {
        public static BlitType? Int32ToType1(int type)
        {
            return type switch
            {
                0 => BlitType.TransparentMask,
                1 => BlitType.Normal,
                2 => BlitType.BlendBlackBias,
                3 => BlitType.BlendTestLight,
                4 => BlitType.BlitAsShadow,
                5 => BlitType.NormalScale,
                6 => BlitType.BlendAmplifyLight,
                _ => null,
            };
        }

        public static BlitType? Int32ToType2(int type)
        {
            return type switch
            {
                1 => BlitType.TransparentMask,
                0 => BlitType.Normal,
                2 => BlitType.BlendBlackBias,
                3 => BlitType.BlendTestLight,
                4 => BlitType.BlitAsShadow,
                5 => BlitType.NormalScale,
                6 => BlitType.BlendAmplifyLight,
                _ => null,
            };
        }

        public static BlitType? Int32ToType3(int type)
        {
            return type switch
            {
                2 => BlitType.TransparentMask,
                3 => BlitType.Normal,
                0 => BlitType.BlendBlackBias,
                1 => BlitType.BlendTestLight,
                4 => BlitType.BlitAsShadow,
                //=> BlitType.NormalScale
                5 => BlitType.BlendAmplifyLight,
                _ => null,
            };
        }

        public static BlitType? Int32ToType4(int type)
        {
            return type switch
            {
                0 => BlitType.TransparentMask,
                1 => BlitType.Normal,
                3 => BlitType.BlendBlackBias,
                4 => BlitType.BlendTestLight,
                //=> BlitType.BlitAsShadow,
                //=> BlitType.NormalScale,
                2 => BlitType.BlendAmplifyLight,
                _ => null,
            };
        }

        public static BlitType? Int32ToType5(int type)
        {
            return type switch
            {
                1 => BlitType.TransparentMask,
                0 => BlitType.Normal,
                2 => BlitType.BlendBlackBias,
                3 => BlitType.BlendTestLight,
                //=> BlitType.BlitAsShadow,
                //=> BlitType.NormalScale,
                4 => BlitType.BlendAmplifyLight,
                _ => null,
            };
        }

        public static BlitType? Int32ToType(int type,
            BlitTypeRevision revision)
        {
            return revision switch
            {
                BlitTypeRevision.Type1 => Int32ToType1(type),
                BlitTypeRevision.Type2 => Int32ToType2(type),
                BlitTypeRevision.Type3 => Int32ToType3(type),
                BlitTypeRevision.Type4 => Int32ToType4(type),
                BlitTypeRevision.Type5 => Int32ToType5(type),
                _ => null,
            };
        }

        public static int? ToType1(BlitType type)
        {
            return type switch
            {
                BlitType.TransparentMask => 0,
                BlitType.Normal => 1,
                BlitType.BlendBlackBias => 2,
                BlitType.BlendTestLight => 3,
                BlitType.BlitAsShadow => 4,
                BlitType.NormalScale => 5,
                BlitType.BlendAmplifyLight => 6,
                _ => null,
            };
        }

        public static int? ToType2(BlitType type)
        {
            return type switch
            {
                BlitType.TransparentMask => 1,
                BlitType.Normal => 0,
                BlitType.BlendBlackBias => 2,
                BlitType.BlendTestLight => 3,
                BlitType.BlitAsShadow => 4,
                BlitType.NormalScale => 5,
                BlitType.BlendAmplifyLight => 6,
                _ => null,
            };
        }

        public static int? ToType3(BlitType type)
        {
            return type switch
            {
                BlitType.TransparentMask => 2,
                BlitType.Normal => 3,
                BlitType.BlendBlackBias => 0,
                BlitType.BlendTestLight => 1,
                BlitType.BlitAsShadow => 4,
                //BlitType.NormalScale => -1,
                BlitType.BlendAmplifyLight => 5,
                _ => null,
            };
        }

        public static int? ToType4(BlitType type)
        {
            return type switch
            {
                BlitType.TransparentMask => 0,
                BlitType.Normal => 1,
                BlitType.BlendBlackBias => 3,
                BlitType.BlendTestLight => 4,
                //BlitType.BlitAsShadow => -1,
                //BlitType.NormalScale => -1,
                BlitType.BlendAmplifyLight => 2,
                _ => null,
            };
        }

        public static int? ToType5(BlitType type)
        {
            return type switch
            {
                BlitType.TransparentMask => 1,
                BlitType.Normal => 0,
                BlitType.BlendBlackBias => 2,
                BlitType.BlendTestLight => 3,
                //BlitType.BlitAsShadow => -1,
                //BlitType.NormalScale => -1,
                BlitType.BlendAmplifyLight => 4,
                _ => null,
            };
        }

        public static int? ToInt(BlitType type,
            BlitTypeRevision revision)
        {
            return revision switch
            {
                BlitTypeRevision.Type1 => ToType1(type),
                BlitTypeRevision.Type2 => ToType2(type),
                BlitTypeRevision.Type3 => ToType3(type),
                BlitTypeRevision.Type4 => ToType4(type),
                BlitTypeRevision.Type5 => ToType5(type),
                _ => null,
            };
        }
    }
}
