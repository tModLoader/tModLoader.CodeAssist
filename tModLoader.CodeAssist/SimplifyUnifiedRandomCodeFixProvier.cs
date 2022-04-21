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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SimplifyUnifiedRandomCodeFixProvier)), Shared]
    public class SimplifyUnifiedRandomCodeFixProvier : CodeFixProvider
    {
        private const string title = "Simplify common Main.rand.Next usage patterns";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(tModLoaderCodeAssistAnalyzer.SimplifyUnifiedRandomDiagnosticId);
        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => SimplifyUnifiedRandomAsync(context, context.Document, diagnostic, root),
                    equivalenceKey: title),
                diagnostic);
        }

        Task<Document> SimplifyUnifiedRandomAsync(CodeFixContext context, Document document, Diagnostic diagnostic, SyntaxNode root)
        {
            var n = root.FindNode(diagnostic.Location.SourceSpan);

            string result = diagnostic.Properties["result"];

            var newRoot = root.ReplaceNode(n, SyntaxFactory.ParseExpression(result));

            //NameSyntax name = SyntaxFactory.IdentifierName(result);
            //name = name.WithLeadingTrivia(literalExpressionSyntax.GetLeadingTrivia()).WithTrailingTrivia(literalExpressionSyntax.GetTrailingTrivia()); // try WithTriviaFrom instead?

            //var newRoot = root.ReplaceNode(literalExpressionSyntax, name);


            //var compilationUnitSyntax = newRoot as CompilationUnitSyntax;

            //var systemThreadingUsingName =
            //    SyntaxFactory.QualifiedName(
            //        SyntaxFactory.IdentifierName("Terraria"),
            //        SyntaxFactory.IdentifierName("ID"));

            //if (compilationUnitSyntax.Usings.All(u => u.Name.GetText().ToString() != "Terraria.ID"))
            //{
            //    if (compilationUnitSyntax.Usings.Any())
            //        newRoot = newRoot.InsertNodesAfter(compilationUnitSyntax.Usings.Last(), new[] { SyntaxFactory.UsingDirective(systemThreadingUsingName) });
            //    else
            //        newRoot = compilationUnitSyntax.AddUsings(new[] { SyntaxFactory.UsingDirective(systemThreadingUsingName) });
            //}
            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }
    }
}
