using Microsoft.Foundation;
using Microsoft.UI.Composition;
using Microsoft.UI.Dispatching;
using System;
using System.Runtime.InteropServices;
using WinRT;

namespace Microsoft.UI.Content
{
    [WindowsRuntimeType("Microsoft.UI")]
    [Guid("1054BF83-B35B-5FDE-8DD7-AC3BB3E6CE27")]
    [WindowsRuntimeHelperType(typeof(global::ABI.Microsoft.UI.Content.IContentExternalBackdropLink))]
    [global::Windows.Foundation.Metadata.ContractVersion(typeof(WindowsAppSDKContract), 65543u)]
    internal interface IContentExternalBackdropLink
    {
        DispatcherQueue DispatcherQueue { get; }

        CompositionBorderMode ExternalBackdropBorderMode { get; set; }

        Visual PlacementVisual { get; }
    }
}