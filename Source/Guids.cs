// Guids.cs
// MUST match guids.h
using System;


namespace Alphaleonis.VSProjectSetMgr
{
    static class GuidList
    {
        public const string guidLoadedProjectsProfileManagerPkgString = "a8380ee6-9355-4ab9-bbf1-914f4d00fbee";
        public const string guidLoadedProjectsProfileManagerCmdSetString = "515751ee-ed30-4ba6-9e92-3320d3bf5e60";

        public static readonly Guid guidLoadedProjectsProfileManagerCmdSet = new Guid(guidLoadedProjectsProfileManagerCmdSetString);
    };
}