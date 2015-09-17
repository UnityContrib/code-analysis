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
        /// Returns a value indicating if the <paramref name="namedTypeSymbol"/> inherits from or is equal to the type specified by
        /// <paramref name="typeName"/>, <paramref name="namespace"/> and <paramref name="assemblyName"/>.
        /// </summary>
        /// <param name="namedTypeSymbol">
        /// The symbol to test if inherits from or equals to the other.
        /// </param>
        /// <param name="typeName">
        /// Name of the other type.
        /// </param>
        /// <param name="@namespace">
        /// Namespace of the other type.
        /// </param>
        /// <param name="assemblyName">
        /// Assembly name of the other type.
        /// </param>
        /// <returns>
        /// true if inherits from or is equal to; otherwise false.
        /// </returns>
        public static bool InheritsFromOrEquals(this INamedTypeSymbol namedTypeSymbol, string typeName, string @namespace, string assemblyName)
        {
            if (namedTypeSymbol.Name.Equals(typeName, StringComparison.CurrentCulture))
            {
                if (namedTypeSymbol.ContainingNamespace.Name.Equals(@namespace, StringComparison.CurrentCulture))
                {
                    if (namedTypeSymbol.ContainingAssembly.Name.Equals(assemblyName, StringComparison.CurrentCulture))
                    {
                        return true;
                    }
                }
            }
            if (namedTypeSymbol.BaseType == null)
            {
                return false;
            }
            return namedTypeSymbol.BaseType.InheritsFromOrEquals(typeName, @namespace, assemblyName);
        }

        /// <summary>
        /// Returns a value indicating if the attribute inherits from  or is equal to the type specified by
        /// <paramref name="typeName"/>, <paramref name="namespace"/> and <paramref name="assemblyName"/>.
        /// </summary>
        /// <param name="attribute">
        /// The attribute to test if inherits from or equals to the other.
        /// </param>
        /// <param name="typeName">
        /// Name of the other type.
        /// </param>
        /// <param name="@namespace">
        /// Namespace of the other type.
        /// </param>
        /// <param name="assemblyName">
        /// Assembly name of the other type.
        /// </param>
        /// <returns>
        /// true if inherits from or is equal to; otherwise false.
        /// </returns>
        public static bool IsAttribute(this AttributeData attribute, string typeName, string @namespace, string assemblyName)
        {
            return attribute.AttributeClass.InheritsFromOrEquals(typeName, @namespace, assemblyName);
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
