// PkgCmdID.cs
// MUST match PkgCmdID.h
using System;

namespace Alphaleonis.VSProjectSetMgr
{
   static class PkgCmdIDList
   {
      public const uint cmdidManageLoadedProjects = 0x100;

      public const int cmdidLoadAllProjectsInSolution = 0x0100;
      public const int cmdidUnloadAllProjectsInSolution = 0x0101;
      public const int cmdidMore = 0x0190;
      public const int cmdidShowManager = 0x0191;

      public const int mnuidMRU0 = 0x0200;
      public const int mnuidMRU1 = 0x0201;
      public const int mnuidMRU2 = 0x0202;
      public const int mnuidMRU3 = 0x0203;

      public const int cmdidLoadMRU0 = 0x3000;
      public const int cmdidLoadMRU1 = 0x3001;
      public const int cmdidLoadMRU2 = 0x3002;
      public const int cmdidLoadMRU3 = 0x3003;

      public const int cmdidUnloadMRU0 = 0x3010;
      public const int cmdidUnloadMRU1 = 0x3011;
      public const int cmdidUnloadMRU2 = 0x3012;
      public const int cmdidUnloadMRU3 = 0x3013;

      public const int cmdidLoadExclusiveMRU0 = 0x3020;
      public const int cmdidLoadExclusiveMRU1 = 0x3021;
      public const int cmdidLoadExclusiveMRU2 = 0x3022;
      public const int cmdidLoadExclusiveMRU3 = 0x3023;

      public const int cmdidUnloadExclusiveMRU0 = 0x3030;
      public const int cmdidUnloadExclusiveMRU1 = 0x3031;
      public const int cmdidUnloadExclusiveMRU2 = 0x3032;
      public const int cmdidUnloadExclusiveMRU3 = 0x3033;

      public const int ToolbarID = 0x4000;
      public const int ToolbarGroupID = 0x4001;

      public const int cmdidAddProfile = 0x132;
      public const int cmdidDeleteProfile = 0x133;
      public const int cmdidEditProfile = 0x134;
      public const int cmdidLoadSelectedProfile = 0x135;
      public const int cmdidLoadExSelectedProfile = 0x136;
      public const int cmdidUnloadSelectedProfile = 0x137;
      public const int cmdidUnloadExSelectedProfile = 0x138;
   };
}