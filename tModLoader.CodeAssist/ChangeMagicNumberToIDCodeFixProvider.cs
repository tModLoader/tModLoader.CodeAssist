using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace tModLoader.CodeAssist
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ChangeMagicNumberToIDCodeFixProvider)), Shared]
    public class ChangeMagicNumberToIDCodeFixProvider : CodeFixProvider
    {
        private const string title = "Change magic number into appropriate ID value";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(tModLoaderCodeAssistAnalyzer.ChangeMagicNumberToIDDiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

            string customTitle = title;
            if (diagnostic.Properties.ContainsKey("idType"))
                customTitle = $"Change magic number into appropriate {diagnostic.Properties["idType"]} value";

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: customTitle,
                    createChangedDocument: c => ChangeMagicNumberToItemIDAsync(context, context.Document, diagnostic, root),
                    equivalenceKey: title),
                diagnostic);
        }


        Task<Document> ChangeMagicNumberToItemIDAsync(CodeFixContext context, Document document, Diagnostic diagnostic, SyntaxNode root)
        {
            var n = root.FindNode(diagnostic.Location.SourceSpan);

            if (n is ArgumentSyntax argument && argument.Expression is LiteralExpressionSyntax)
                n = argument.Expression;

            // We expect this to point directly at the NumericLiteralExpression
            if (!(n is LiteralExpressionSyntax literalExpressionSyntax && literalExpressionSyntax.IsKind(SyntaxKind.NumericLiteralExpression)))
                return Task.FromResult(document);

            string result = diagnostic.Properties["result"];

            NameSyntax name = SyntaxFactory.IdentifierName(result);
            name = name.WithLeadingTrivia(literalExpressionSyntax.GetLeadingTrivia()).WithTrailingTrivia(literalExpressionSyntax.GetTrailingTrivia()); // try WithTriviaFrom instead?

            var newRoot = root.ReplaceNode(literalExpressionSyntax, name);


            var compilationUnitSyntax = newRoot as CompilationUnitSyntax;

            var systemThreadingUsingName =
                SyntaxFactory.QualifiedName(
                    SyntaxFactory.IdentifierName("Terraria"),
                    SyntaxFactory.IdentifierName("ID"));

            if (compilationUnitSyntax.Usings.All(u => u.Name.GetText().ToString() != "Terraria.ID"))
            {
                if(compilationUnitSyntax.Usings.Any())
                    newRoot = newRoot.InsertNodesAfter(compilationUnitSyntax.Usings.Last(), new[] { SyntaxFactory.UsingDirective(systemThreadingUsingName) });
                else
                    newRoot = compilationUnitSyntax.AddUsings(new[] { SyntaxFactory.UsingDirective(systemThreadingUsingName) });
            }
            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }
    }
}
