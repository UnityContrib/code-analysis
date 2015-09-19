using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnityContrib.CodeAnalysis
{
    /// <summary>
    /// Makes the field private and adds [SerializeField] attribute.
    /// </summary>
    [Shared]
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PrivateFieldCodeFixProvider))]
    public class PrivateFieldCodeFixProvider : CodeFixProvider
    {
        /// <summary>
        /// Gets a list of diagnostic IDs that this provider can provider fixes for.
        /// </summary>
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(PrivateFieldDiagnosticAnalyzer.DiagnosticId);
            }
        }

        /// <summary>
        /// Gets an optional <see cref="FixAllProvider"/> that can fix all/multiple occurrences of diagnostics fixed by this code fix provider.
        /// </summary>
        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <summary>
        /// Computes one or more fixes for the specified <see cref="CodeFixContext"/>.
        /// </summary>
        /// <param name="context">
        /// A <see cref="CodeFixContext"/> containing context information about the diagnostics to fix.
        /// The context must only contain diagnostics with an <see cref="Diagnostic.Id"/> included in the <see cref="FixableDiagnosticIds"/> for the current provider.
        /// </param>
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document
                .GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(continueOnCapturedContext: false)
                ;
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var declaration = root
                .FindToken(diagnosticSpan.Start)
                .Parent
                .AncestorsAndSelf()
                .OfType<FieldDeclarationSyntax>()
                .First()
                ;
            var title = "Make private";
            var action = CodeAction.Create(
                title,
                c => MakePrivateAsync(context.Document, declaration, c, diagnostic),
                equivalenceKey: title
                );
            context.RegisterCodeFix(
                action,
                diagnostic
                );
        }

        /// <summary>
        /// Adds the [Tooltip] attribute to the field.
        /// </summary>
        /// <param name="document">
        /// The document containing the field to fix.
        /// </param>
        /// <param name="fieldDeclaration">
        /// The field to fix.
        /// </param>
        /// <param name="cancellationToken">
        /// Details regarding code fix cancellation.
        /// </param>
        /// <returns>
        /// The fixed document.
        /// </returns>
        private async Task<Document> MakePrivateAsync(Document document, FieldDeclarationSyntax fieldDeclaration, CancellationToken cancellationToken, Diagnostic diagnostic)
        {
            var publicModifier = fieldDeclaration.Modifiers.FirstOrDefault(m => m.IsKind(SyntaxKind.PublicKeyword));
            var privateModifier = SyntaxFactory.Token(SyntaxKind.PrivateKeyword);
            var newModifiers = fieldDeclaration.Modifiers.Replace(publicModifier, privateModifier);
            var newFieldDeclaration = fieldDeclaration.WithModifiers(newModifiers);

            var hasSerializeFieldAttribute = bool.Parse(diagnostic.Properties["hasSerializeFieldAttribute"]);
            if (!hasSerializeFieldAttribute)
            {
                var attribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("SerializeField"));
                var attributeList = SyntaxFactory.AttributeList().AddAttributes(attribute);
                var newAttributeLists = fieldDeclaration.AttributeLists.Add(attributeList);
                newFieldDeclaration = newFieldDeclaration.WithAttributeLists(newAttributeLists);
            }

            var root = await document.GetSyntaxRootAsync();
            var newRoot = root.ReplaceNode(fieldDeclaration, newFieldDeclaration);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}