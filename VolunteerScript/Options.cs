namespace VolunteerScript;

public enum Mode
{
    MiraiNet,
    Local,
    Test
}

public record Options(Mode Mode, string ConfigPath, int PlacesToSubmit, bool AutoSubmit, bool ImagesEnabled, bool ObservedGroupOnly);
