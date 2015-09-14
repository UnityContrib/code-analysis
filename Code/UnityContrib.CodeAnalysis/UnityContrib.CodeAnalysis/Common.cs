using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Reflection;

namespace UnityContrib.CodeAnalysis
{
    public static class Common
    {
        public static bool InheritsFromOrEquals(this INamedTypeSymbol current, INamedTypeSymbol other)
        {
            if(current == other)
            {
                return true;
            }
            if(current.BaseType == null)
            {
                return false;
            }
            return current.BaseType.InheritsFromOrEquals(other);
        }

        public static INamedTypeSymbol GetNamedSymbol(this Type type, SymbolAnalysisContext context)
        {
            foreach (var reference in context.Compilation.References)
            {
                if(reference == null)
                {
                    continue;
                }

                var assembly = context.Compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
                if (assembly == null)
                {
                    continue;
                }

                if (!assembly.Name.Equals(type.GetTypeInfo().Assembly.GetName().Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                var namedTypeSymbol = assembly.GetTypeByMetadataName(type.FullName);
                if(namedTypeSymbol == null)
                {
                    continue;
                }

                return namedTypeSymbol;
            }

            return null;
        }
    }
}
