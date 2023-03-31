namespace VolunteerScript;

public enum Mode
{
    MiraiNet,
    Local,
    Test
}

public record Options(Mode Mode, string ConfigPath, bool AutoSubmit, bool ImagesEnabled, bool ObservedGroupOnly);
