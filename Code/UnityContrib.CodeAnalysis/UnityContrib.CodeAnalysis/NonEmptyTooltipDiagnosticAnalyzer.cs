using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using cls = UnityContrib.CodeAnalysis.NonEmptyTooltipDiagnosticAnalyzer;

namespace UnityContrib.CodeAnalysis
{
    /// <summary>
    /// Checks the <see cref="T:UnityEngine.TooltipAttribute"/> to see if has an empty string.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NonEmptyTooltipDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID of the analyzer.
        /// </summary>
        public const string DiagnosticId = "UCNonEmptyTooltip";

        /// <summary>
        /// The diagnostics details of the analyzer.
        /// </summary>
        private static DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            DiagnosticId,
            title: "Tooltip attribute must contain a description of the field.",
            messageFormat: "Tooltip attribute has an empty string where there should be a description of the field.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Replace the empty string with a description of the field."
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
        /// Checks the <see cref="T:UnityEngine.TooltipAttribute"/> for empty description.
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

            // must not have a tooltip attribute
            var tooltipAttributeData = attributes.FirstOrDefault(a => a.AttributeClass.InheritsFromOrEquals("TooltipAttribute", "UnityEngine", "UnityEngine"));
            if(tooltipAttributeData == null)
            {
                return;
            }

            var constructorArguments = tooltipAttributeData.ConstructorArguments;
            if(constructorArguments == null)
            {
                return;
            }

            var typedConstant = constructorArguments.FirstOrDefault();
            var value = typedConstant.Value as string;
            if(value == null)
            {
                return;
            }

            if(value != string.Empty)
            {
                return;
            }

            var diagnostic = Diagnostic.Create(
                cls.descriptor,
                fieldSymbol.Locations[0],
                fieldSymbol.Name
                );
            context.ReportDiagnostic(diagnostic);
        }
    }
}
