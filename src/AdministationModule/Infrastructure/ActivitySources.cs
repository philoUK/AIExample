using System.Diagnostics;

namespace AdministrationModule.Infrastructure;

internal static class ActivitySources
{
    internal static readonly ActivitySource Endpoints = new(
        "AIExample.AdministrationModule.Endpoints"
    );
}
