using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Reflection;

namespace UnityContrib.CodeAnalysis
{
    /// <summary>
    /// Provides helper methods for analyzing code.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Returns a value indicating if the <paramref name="current"/> symbol inherits from or is equal to the <paramref name="other"/>.
        /// </summary>
        /// <param name="current">
        /// The symbol to test if it inherits or is equal to <paramref name="other"/>.
        /// </param>
        /// <param name="other">
        /// The symbol to test if <paramref name="current"/> inherits from or is equal to.
        /// </param>
        /// <returns>
        /// true if <paramref name="current"/> inherits from or is equal to <paramref name="other"/>; otherwise false.
        /// </returns>
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

        /// <summary>
        /// Returns the first symbol matching the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">
        /// The <see cref="T:System.Type"/> for the symbol to match.
        /// </param>
        /// <param name="context">
        /// The context to use to find the symbol.
        /// </param>
        /// <returns>
        /// The symbol if found; otherwise null.
        /// </returns>
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
