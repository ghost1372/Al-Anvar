using Microsoft.CodeAnalysis;

namespace AlAnvar.Generators;

internal static partial class Constants
{
    internal const string HelperNamespace = "AlAnvar.Common.";
    internal const string StringsClassName = "Strings";
    internal const char ConstantSeparator = '/';
    internal static readonly DiagnosticDescriptor FSG1003 = new(
                id: nameof(FSG1003),
                title: "Multiple files with the same name detected",
                messageFormat: "Multiple files named '{0}' were detected. Ensure all generated localization string files have unique names.",
                category: "FileGeneration",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true,
                description: "This diagnostic detects cases where multiple localization string files are being generated with the same name," +
                "which can cause conflicts and overwrite issues.");
}
