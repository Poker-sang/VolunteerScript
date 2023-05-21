namespace VolunteerScript;

public record Options
(
    Mode Mode,
    string ConfigPath,
    int PlacesToSubmit,
    bool AutoSubmit,
    bool ImagesEnabled,
    bool ObservedGroupOnly,
    string QqFilesReceive,
    string TestFile
);
