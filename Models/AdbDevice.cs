namespace VizADB.Models;

public record AdbDevice(string Serial, string State)
{
    public bool IsOnline => State == "device";
}
