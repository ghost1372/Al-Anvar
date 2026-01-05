using System.ComponentModel;

namespace ABI.Microsoft.UI.Content
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class ContentExternalBackdropLinkRcwFactoryAttribute : global::WinRT.WinRTImplementationTypeRcwFactoryAttribute
    {
        public override object CreateInstance(global::WinRT.IInspectable inspectable)
        {
            return new global::Microsoft.UI.Content.ContentExternalBackdropLink(inspectable.ObjRef);
        }
    }
}