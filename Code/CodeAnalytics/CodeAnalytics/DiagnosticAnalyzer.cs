using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;
using cls = CodeAnalytics.CodeAnalyticsAnalyzer;

namespace CodeAnalytics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CodeAnalyticsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "HasToolTip";

        private static DiagnosticDescriptor rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "Private field marked with SerializeField attribute must also have a Tooltip attribute.",
            messageFormat: "Private field marked with SerializeField attribute must also have a Tooltip attribute.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Add [Tooltip(\"description\")] to the field."
            );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(cls.AnalyzeSymbol, SymbolKind.Field);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var fieldSymbol = context.Symbol as IFieldSymbol;
            if (fieldSymbol == null)
            {
                return;
            }

            if (fieldSymbol.IsStatic)
            {
                return;
            }

            if (fieldSymbol.IsConst)
            {
                return;
            }

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
            var monobehaviourType = typeof(MonoBehaviour);
            var monoBehaviourNamedTypeSymbol = monobehaviourType.GetNamedSymbol(context);
            if(!namedTypeSymbol.BaseType.InheritsFromOrEquals(monoBehaviourNamedTypeSymbol))
            {
                return;
            }

            var attributes = fieldSymbol.GetAttributes();
            if(attributes == null || attributes.Length == 0)
            {
                return;
            }

            // must have serializefield attribute
            var serializeFieldType = typeof(SerializeField);
            var serializeFieldNamedTypeSymbol = serializeFieldType.GetNamedSymbol(context);
            if (!attributes.Any(a => a.AttributeClass == serializeFieldNamedTypeSymbol))
            {
                return;
            }

            // must not have a tooltip attribute
            var tooltipAttributeType = typeof(TooltipAttribute);
            var tooltipAttributeTypeSymbol = tooltipAttributeType.GetNamedSymbol(context);
            if(attributes.Any(a => a.AttributeClass == tooltipAttributeTypeSymbol))
            {
                return;
            }

            var diagnostic = Diagnostic.Create(cls.rule, fieldSymbol.Locations[0], fieldSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
