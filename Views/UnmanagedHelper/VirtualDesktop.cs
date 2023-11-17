// Author: Markus Scholtes, 2023
// Version 1.16, 2023-09-17
// Version for Windows 10 1809 to 22H2
// Compile with:
// C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe VirtualDesktop.cs

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

// set attributes
using System.Reflection;

// From https://github.com/MScholtes/VirtualDesktop/blob/master/VirtualDesktop.cs

namespace FruitLanguageSwitcher.Views.UnmanagedHelper
{
    internal static class Guids
    {
        public static readonly Guid CLSID_ImmersiveShell = new("C2F03A33-21F5-47FA-B4BB-156362A2F239");
        public static readonly Guid CLSID_VirtualDesktopPinnedApps = new("B5A399E7-1C87-46B8-88E9-FC5747B171BD");
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    [Guid("372E1D3B-38D3-42E4-A15B-8AB2B178F513")]
    internal interface IApplicationView
    { }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("4CE81583-1E4C-4632-A621-07A53543148F")]
    internal interface IVirtualDesktopPinnedApps
    {
        bool IsAppIdPinned(string appId);
        void PinAppID(string appId);
        void UnpinAppID(string appId);
        bool IsViewPinned(IApplicationView applicationView);
        void PinView(IApplicationView applicationView);
        void UnpinView(IApplicationView applicationView);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
    internal interface IServiceProvider10
    {
        [return: MarshalAs(UnmanagedType.IUnknown)]
        object QueryService(ref Guid service, ref Guid riid);
    }

    internal static class DesktopManager
    {
        static DesktopManager()
        {
            var shell = (IServiceProvider10)Activator.CreateInstance(Type.GetTypeFromCLSID(Guids.CLSID_ImmersiveShell));
            VirtualDesktopPinnedApps = (IVirtualDesktopPinnedApps)shell.QueryService(Guids.CLSID_VirtualDesktopPinnedApps, typeof(IVirtualDesktopPinnedApps).GUID);
        }
        internal static IVirtualDesktopPinnedApps VirtualDesktopPinnedApps;
    }

    public static class VirtualDesktop
    {
        public static void PinApp(string AppID)
        {
            DesktopManager.VirtualDesktopPinnedApps.PinAppID(AppID);
        }
    }
}


