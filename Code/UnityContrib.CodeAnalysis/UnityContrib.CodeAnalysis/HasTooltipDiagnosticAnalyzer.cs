using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using cls = UnityContrib.CodeAnalysis.HasTooltipCodeAnalyticsAnalyzer;

namespace UnityContrib.CodeAnalysis
{
    /// <summary>
    /// Checks the code for subclasses of <see cref="T:UnityEngine.MonoBehaviour"/> that has private instance fields marked with
    /// <see cref="T:UnityEngine.SerializeField"/> but isn't marked with a <see cref="T:UnityEngine.TooltipAttribute"/>.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class HasTooltipCodeAnalyticsAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID of the analyzer.
        /// </summary>
        public const string DiagnosticId = "HasToolTip";

        /// <summary>
        /// The diagnostics details of the analyzer.
        /// </summary>
        private static DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            DiagnosticId,
            title: "Private field marked with SerializeField attribute must also have a Tooltip attribute.",
            messageFormat: "Private field marked with SerializeField attribute must also have a Tooltip attribute.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Add [Tooltip(\"description\")] to the field."
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
        /// Checks the field for missing <see cref="T:UnityEngine.TooltipAttribute"/>.
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
            if(fieldSymbol.IsReadOnly)
            {
                return;
            }

            // ignore non-private fields
            if(fieldSymbol.DeclaredAccessibility != Accessibility.Private)
            {
                return;
            }

            var namedTypeSymbol = fieldSymbol.ContainingSymbol as INamedTypeSymbol;
            if(namedTypeSymbol == null)
            {
                return;
            }

            // must derrive from MonoBehaviour
            if(!namedTypeSymbol.BaseType.InheritsFromOrEquals("MonoBehaviour", "UnityEngine", "UnityEngine"))
            {
                return;
            }

            // ignore fields without attributes
            var attributes = fieldSymbol.GetAttributes();
            if(attributes == null || attributes.Length == 0)
            {
                return;
            }

            // must have serializefield attribute
            if (!attributes.Any(a => a.AttributeClass.InheritsFromOrEquals("SerializeField", "UnityEngine", "UnityEngine")))
            {
                return;
            }

            // must not have a tooltip attribute
            if(attributes.Any(a => a.AttributeClass.InheritsFromOrEquals("TooltipAttribute", "UnityEngine", "UnityEngine")))
            {
                return;
            }

            var diagnostic = Diagnostic.Create(cls.descriptor, fieldSymbol.Locations[0], fieldSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
