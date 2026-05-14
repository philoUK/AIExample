using System.Diagnostics;

namespace EventStore;

internal static class ActivitySources
{
    internal static readonly ActivitySource Commands = new("AIExample.Commands");
}
