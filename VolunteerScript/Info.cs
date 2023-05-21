namespace VolunteerScript;

public record Info
(
    string IpAddress = "localhost",
    ushort Port = 8080,
    uint QqBot = 0,
    string VerifyKey = "",
    uint GroupObserved = 0,
    string Name = "",
    uint Id = 0,
    uint Qq = 0,
    ulong Tel = 0,
    string Grade = "",
    string Major = "",
    string Class = ""
);
