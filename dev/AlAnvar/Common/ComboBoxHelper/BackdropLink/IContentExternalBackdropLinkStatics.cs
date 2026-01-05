using Microsoft.Foundation;
using Microsoft.UI.Composition;
using System.Runtime.InteropServices;
using WinRT;

namespace Microsoft.UI.Content
{
    [WindowsRuntimeType("Microsoft.UI")]
    [Guid("46CAC6FB-BB51-510A-958D-E0EB4160F678")]
    [WindowsRuntimeHelperType(typeof(global::ABI.Microsoft.UI.Content.IContentExternalBackdropLinkStatics))]
    [global::Windows.Foundation.Metadata.ContractVersion(typeof(WindowsAppSDKContract), 65543u)]
    internal interface IContentExternalBackdropLinkStatics
    {
        ContentExternalBackdropLink Create(Compositor compositor);
    }
}