namespace NuVelocity.Rebound.Infinity;

[PropertyRoot("CMusicPickerChooseMusic", typeof(MusicPickerChooseMusic))]
public class MusicPickerChooseMusic : MusicPicker
{
    // reference to path of music/ogg file.
    [Property("Music To Play")]
    public string MusicToPlay { get; set; }
}
