using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using cls = UnityContrib.CodeAnalysis.PrivateFieldDiagnosticAnalyzer;

namespace UnityContrib.CodeAnalysis
{
    /// <summary>
    /// Checks the field to see if it is private.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PrivateFieldDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID of the analyzer.
        /// </summary>
        public const string DiagnosticId = "UCPrivateField";

        /// <summary>
        /// The diagnostics details of the analyzer.
        /// </summary>
        private static DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            DiagnosticId,
            title: "Field must be private.",
            messageFormat: "Field is not private. Use properties or methods if you need to expose the value.",
            category: "Design",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Make the field private."
            );

        /// <summary>
        /// Gets the supported diagnostics of the analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(descriptor);
            }
        }

        /// <summary>
        /// Registers the actions used by the analyzer.
        /// </summary>
        /// <param name="context">
        /// The initialization context.
        /// </param>
        /// <remarks>
        /// Invoked by Roslyn.
        /// </remarks>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(cls.AnalyzeSymbol, SymbolKind.Field);
        }

        /// <summary>
        /// Checks the field.
        /// </summary>
        /// <param name="context">
        /// The context of the action.
        /// </param>
        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var fieldSymbol = context.Symbol as IFieldSymbol;
            if (fieldSymbol == null)
            {
                return;
            }

            // ignore static fields
            if (fieldSymbol.IsStatic)
            {
                return;
            }

            // ignore constant fields
            if (fieldSymbol.IsConst)
            {
                return;
            }

            // ignore read-only fields
            if (fieldSymbol.IsReadOnly)
            {
                return;
            }

            // ignore non-private fields
            if (fieldSymbol.DeclaredAccessibility == Accessibility.Private)
            {
                return;
            }

            var namedTypeSymbol = fieldSymbol.ContainingSymbol as INamedTypeSymbol;
            if (namedTypeSymbol == null)
            {
                return;
            }

            // must derrive from MonoBehaviour
            if (!namedTypeSymbol.BaseType.InheritsFromOrEquals("MonoBehaviour", "UnityEngine", "UnityEngine"))
            {
                return;
            }

            var hasSerializeFieldAttribute = false;
            var attributes = fieldSymbol.GetAttributes();
            if (attributes != null && attributes.Length > 0)
            {
                hasSerializeFieldAttribute = attributes.Any(a => a.AttributeClass.InheritsFromOrEquals("SerializeField", "UnityEngine", "UnityEngine"));
            }

            var properties = ImmutableDictionary<string, string>
                .Empty
                .Add(nameof(hasSerializeFieldAttribute), hasSerializeFieldAttribute.ToString())
                ;

            var diagnostic = Diagnostic.Create(
                cls.descriptor,
                fieldSymbol.Locations[0],
                properties,
                fieldSymbol.Name
                );
            context.ReportDiagnostic(diagnostic);
        }
    }
}
