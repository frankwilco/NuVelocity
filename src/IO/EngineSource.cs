namespace NuVelocity.IO
{
    public enum EngineSource
    {
        None = 0,
        /// <summary>
        /// Engine version: 1998.<br/>
        /// Includes: Swarm.
        /// </summary>
        From1998 = 1,
        /// <summary>
        /// Engine version: 2001<br/>
        /// Includes: Star Trek, Zax, Lionheart, Ricochet Xtreme.
        /// </summary>
        From2001 = 2,
        /// <summary>
        /// Engine version: 2004<br/>
        /// Includes: Ricochet Lost Worlds, Ricochet Lost Worlds Recharged,
        /// Big Kahuna Reef, Big Kahuna Words, Mosaic: Tomb of Mystery,
        /// Wik and the Fable of Souls.
        /// </summary>
        From2004 = 3,       // M: Includes more sequence properties,
                            // replaces lossless/quality properties.
        /// <summary>
        /// Engine version: 2007<br/>
        /// Includes: Ricochet Infinity, Big Kahuna Reef 2.
        /// </summary>
        From2007 = 4,
        /// <summary>
        /// Engine version: 2008<br/>
        /// Includes: Swarm Gold, Build In Time.
        /// </summary>
        From2008 = 5,       // M: Includes mipmap property.
        /// <summary>
        /// Engine version: 2009<br/>
        /// Includes: Ricochet Infinity iOS, Costume Chaos, Big Kahuna Reef 3.
        /// </summary>
        From2009 = 6,       // M: Fixes Crop Color 0 property name typo.
        /// <summary>
        /// Engine version: PS3<br/>
        /// Includes: Ricochet HD.
        /// </summary>
        FromPS3 = 7,
    }
}
