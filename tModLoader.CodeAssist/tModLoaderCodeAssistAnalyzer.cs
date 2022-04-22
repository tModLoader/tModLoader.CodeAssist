using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using tModLoader.CodeAssist.Terraria;

namespace tModLoader.CodeAssist
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class tModLoaderCodeAssistAnalyzer : DiagnosticAnalyzer
    {
        public tModLoaderVersion tModLoaderVersion = tModLoaderVersion.Unknown;

        public const string ChangeMagicNumberToIDDiagnosticId = "ChangeMagicNumberToID";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString ChangeMagicNumberToIDTitle = new LocalizableResourceString(nameof(Resources.ChangeMagicNumberToIDTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString ChangeMagicNumberToIDMessageFormat = new LocalizableResourceString(nameof(Resources.ChangeMagicNumberToIDMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString ChangeMagicNumberToIDDescription = new LocalizableResourceString(nameof(Resources.ChangeMagicNumberToIDDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Terraria.ID";

        private static DiagnosticDescriptor ChangeMagicNumberToIDRule = new DiagnosticDescriptor(ChangeMagicNumberToIDDiagnosticId, ChangeMagicNumberToIDTitle, ChangeMagicNumberToIDMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: ChangeMagicNumberToIDDescription);

        public const string SimplifyUnifiedRandomDiagnosticId = "SimplifyUnifiedRandom";
        private const string SimplifyUnifiedRandomTitle = "Simplify common Main.rand.Next usage patterns";
        private const string SimplifyUnifiedRandomMessageFormat = "The expression \"{0}\" should be changed to \"{1}\" for readability";
        private const string SimplifyUnifiedRandomDescription = "Simplifies common Main.rand.Next usage patterns";
        private const string SimplifyUnifiedRandomCategory = "UnifiedRandom";
        private static DiagnosticDescriptor SimplifyUnifiedRandomRule = new DiagnosticDescriptor(SimplifyUnifiedRandomDiagnosticId, SimplifyUnifiedRandomTitle, SimplifyUnifiedRandomMessageFormat, SimplifyUnifiedRandomCategory, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: SimplifyUnifiedRandomDescription);

        // TODO: detect npc.ai[4+]

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(ChangeMagicNumberToIDRule, SimplifyUnifiedRandomRule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information

            FieldToIDTypeBindings = new List<FieldToIDTypeBinding>();
            SameFieldSeparateSearch("Terraria.Item", "createTile", "TileID", Terraria.ID_1_3.TileID.Search, Terraria.ID_1_4.TileID.Search);
            SameFieldSeparateSearch("Terraria.Item", "type", "ItemID", Terraria.ID_1_3.ItemID.Search, Terraria.ID_1_4.ItemID.Search);
            SameFieldSeparateSearch("Terraria.Item", "shoot", "ProjectileID", Terraria.ID_1_3.ProjectileID.Search, Terraria.ID_1_4.ProjectileID.Search);
            SameFieldSeparateSearch("Terraria.Item", "useStyle", "ItemUseStyleID", IdDictionary.Create(typeof(Terraria.ID_1_3.ItemUseStyleID), typeof(int)), IdDictionary.Create(typeof(Terraria.ID_1_4.ItemUseStyleID), typeof(int)));
            SameFieldSeparateSearch("Terraria.Item", "rare", "ItemRarityID", IdDictionary.Create(typeof(Terraria.ID_1_3.ItemRarityID), typeof(int)), IdDictionary.Create(typeof(Terraria.ID_1_4.ItemRarityID), typeof(int)));
            SameFieldSeparateSearch("Terraria.NPC", "type", "NPCID", Terraria.ID_1_3.NPCID.Search, Terraria.ID_1_4.NPCID.Search);
            FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.Main", "netMode", "NetmodeID", IdDictionary.Create(typeof(Terraria.ID.NetmodeID), typeof(int))));
            SeparateFieldSeparateSearch("Terraria.ModLoader.ModTile", "soundType", "SoundType", "SoundID", Terraria.ID_1_3.SoundID.Search, Terraria.ID_1_4.SoundID.Search);
            SeparateFieldSeparateSearch("Terraria.ModLoader.ModTile", "dustType", "DustType", "DustID", Terraria.ID_1_3.DustID.Search, Terraria.ID_1_4.DustID.Search);
            SeparateFieldSeparateSearch("Terraria.ModLoader.ModWall", "dustType", "DustType", "DustID", Terraria.ID_1_3.DustID.Search, Terraria.ID_1_4.DustID.Search);

            // Helper methods for handling slight 1.4 and 1.3 differences
            void SameFieldSeparateSearch(string fullName, string field, string idType, IdDictionary idDictionary, IdDictionary idDictionary_1_4)
            {
                FieldToIDTypeBindings.Add(new FieldToIDTypeBinding(fullName, field, idType, idDictionary, tModLoaderVersion.tModLoader_1_3));
                FieldToIDTypeBindings.Add(new FieldToIDTypeBinding(fullName, field, idType, idDictionary_1_4, tModLoaderVersion.tModLoader_1_4));
            }
            void SeparateFieldSeparateSearch(string fullName, string field, string field_1_4, string idType, IdDictionary idDictionary, IdDictionary idDictionary_1_4)
            {
                FieldToIDTypeBindings.Add(new FieldToIDTypeBinding(fullName, field, idType, idDictionary, tModLoaderVersion.tModLoader_1_3));
                FieldToIDTypeBindings.Add(new FieldToIDTypeBinding(fullName, field_1_4, idType, idDictionary_1_4, tModLoaderVersion.tModLoader_1_4));
            }

            // Could check parameter name, or check parameter list and index. 
            MethodParameterToIDTypeBindings = new List<MethodParameterToIDTypeBinding>();
            SameMethodSeparateSearch("Terraria.Item.CloneDefaults", "Terraria.Item.CloneDefaults(int)", new string[] { "Int32" }, 0, "ItemID", Terraria.ID_1_3.ItemID.Search, Terraria.ID_1_4.ItemID.Search);
            SameMethodSeparateSearch("Terraria.NetMessage.SendData", "Terraria.NetMessage.SendData(int, int, int, Terraria.Localization.NetworkText, int, float, float, float, int, int, int)", new string[] { "Int32", "Int32", "Int32", "NetworkText", "Int32", "Single", "Single", "Single", "Int32", "Int32", "Int32" }, 0, "MessageID", IdDictionary.Create(typeof(Terraria.ID_1_3.MessageID), typeof(byte)), IdDictionary.Create(typeof(Terraria.ID_1_4.MessageID), typeof(byte)));
            SameMethodSeparateSearch("Terraria.Dust.NewDust", "Terraria.Dust.NewDust(Microsoft.Xna.Framework.Vector2, int, int, int, float, float, int, Microsoft.Xna.Framework.Color, float)", new string[] { "Vector2", "Int32", "Int32", "Int32", "Single", "Single", "Int32", "Color", "Single" }, 3, "DustID", Terraria.ID_1_3.DustID.Search, Terraria.ID_1_4.DustID.Search);
            SameMethodSeparateSearch("Terraria.Dust.NewDustDirect", "Terraria.Dust.NewDustDirect(Microsoft.Xna.Framework.Vector2, int, int, int, float, float, int, Microsoft.Xna.Framework.Color, float)", new string[] { "Vector2", "Int32", "Int32", "Int32", "Single", "Single", "Int32", "Color", "Single" }, 3, "DustID", Terraria.ID_1_3.DustID.Search, Terraria.ID_1_4.DustID.Search);

            // 1.3 only
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.ModLoader.ModRecipe.AddTile", "Terraria.ModLoader.ModRecipe.AddTile(int)", new string[] { "Int32" }, 0, "TileID", Terraria.ID_1_3.TileID.Search, tModLoaderVersion.tModLoader_1_3));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.ModLoader.ModRecipe.AddIngredient", "Terraria.ModLoader.ModRecipe.AddIngredient(int, int)", new string[] { "Int32", "Int32" }, 0, "ItemID", Terraria.ID_1_3.ItemID.Search, tModLoaderVersion.tModLoader_1_3));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.ModLoader.ModRecipe.SetResult", "Terraria.ModLoader.ModRecipe.SetResult(int, int)", new string[] { "Int32", "Int32" }, 0, "ItemID", Terraria.ID_1_3.ItemID.Search, tModLoaderVersion.tModLoader_1_3));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Main.PlaySound", "Terraria.Main.PlaySound(int, Microsoft.Xna.Framework.Vector2, int)", new string[] { "Int32", "Vector2", "Int32" }, 0, "SoundID", Terraria.ID_1_3.SoundID.Search, tModLoaderVersion.tModLoader_1_3));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Main.PlaySound", "Terraria.Main.PlaySound(int, int, int, int, float, float)", new string[] { "Int32", "Int32", "Int32", "Int32", "Single", "Single" }, 0, "SoundID", Terraria.ID_1_3.SoundID.Search, tModLoaderVersion.tModLoader_1_3));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Projectile.NewProjectile", "Terraria.Projectile.NewProjectile(Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, int, int, float, int, float, float)", new string[] { "Vector2", "Vector2", "Int32", "Int32", "Single", "Int32", "Single", "Single" }, 2, "ProjectileID", Terraria.ID_1_3.ProjectileID.Search, tModLoaderVersion.tModLoader_1_3));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Projectile.NewProjectile", "Terraria.Projectile.NewProjectile(float, float, float, float, int, int, float, int, float, float)", new string[] { "Single", "Single", "Single", "Single", "Int32", "Int32", "Single", "Int32", "Single", "Single" }, 4, "ProjectileID", Terraria.ID_1_3.ProjectileID.Search, tModLoaderVersion.tModLoader_1_3));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Projectile.NewProjectileDirect", "Terraria.Projectile.NewProjectileDirect(Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, int, int, float, int, float, float)", new string[] { "Vector2", "Vector2", "Int32", "Int32", "Single", "Int32", "Single", "Single" }, 2, "ProjectileID", Terraria.ID_1_3.ProjectileID.Search, tModLoaderVersion.tModLoader_1_3));
            //MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Dust.NewDustPerfect", "Terraria.Dust.NewDustPerfect(Microsoft.Xna.Framework.Vector2, int, Microsoft.Xna.Framework.Vector2?, int, Microsoft.Xna.Framework.Color, float)", new string[] { "Vector2", "Int32", "Vector2?", "Int32", "Color", "Single" }, 1, "DustID", DustID.Search));

            // 1.4 only
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Recipe.AddTile", "Terraria.Recipe.AddTile(int)", new string[] { "Int32" }, 0, "TileID", Terraria.ID_1_4.TileID.Search, tModLoaderVersion.tModLoader_1_4));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Recipe.AddIngredient", "Terraria.Recipe.AddIngredient(int, int)", new string[] { "Int32", "Int32" }, 0, "ItemID", Terraria.ID_1_4.ItemID.Search, tModLoaderVersion.tModLoader_1_4));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.ModLoader.Mod.CreateRecipe", "Terraria.ModLoader.Mod.CreateRecipe(int, int)", new string[] { "Int32", "Int32" }, 0, "ItemID", Terraria.ID_1_4.ItemID.Search, tModLoaderVersion.tModLoader_1_4));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Audio.SoundEngine.PlaySound", "Terraria.Audio.SoundEngine.PlaySound(int, Microsoft.Xna.Framework.Vector2, int)", new string[] { "Int32", "Vector2", "Int32" }, 0, "SoundID", Terraria.ID_1_4.SoundID.Search, tModLoaderVersion.tModLoader_1_4));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Audio.SoundEngine.PlaySound", "Terraria.Audio.SoundEngine.PlaySound(int, int, int, int, float, float)", new string[] { "Int32", "Int32", "Int32", "Int32", "Single", "Single" }, 0, "SoundID", Terraria.ID_1_4.SoundID.Search, tModLoaderVersion.tModLoader_1_4));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Projectile.NewProjectile", "Terraria.Projectile.NewProjectile(Terraria.DataStructures.IEntitySource, Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, int, int, float, int, float, float)", new string[] { "IEntitySource", "Vector2", "Vector2", "Int32", "Int32", "Single", "Int32", "Single", "Single" }, 3, "ProjectileID", Terraria.ID_1_4.ProjectileID.Search, tModLoaderVersion.tModLoader_1_4));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Projectile.NewProjectile", "Terraria.Projectile.NewProjectile(Terraria.DataStructures.IEntitySource, float, float, float, float, int, int, float, int, float, float)", new string[] { "IEntitySource", "Single", "Single", "Single", "Single", "Int32", "Int32", "Single", "Int32", "Single", "Single" }, 5, "ProjectileID", Terraria.ID_1_4.ProjectileID.Search, tModLoaderVersion.tModLoader_1_4));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Projectile.NewProjectileDirect", "Terraria.Projectile.NewProjectileDirect(Terraria.DataStructures.IEntitySource, Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, int, int, float, int, float, float)", new string[] { "IEntitySource", "Vector2", "Vector2", "Int32", "Int32", "Single", "Int32", "Single", "Single" }, 3, "ProjectileID", Terraria.ID_1_4.ProjectileID.Search, tModLoaderVersion.tModLoader_1_4));

            void SameMethodSeparateSearch(string fullyQualifiedMethodName, string fullMethodWithParameters, string[] parameterNames, int parameterIndex, string idType, IdDictionary idDictionary, IdDictionary idDictionary_1_4)
            {
                MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding(fullyQualifiedMethodName, fullMethodWithParameters, parameterNames, parameterIndex, idType, idDictionary, tModLoaderVersion.tModLoader_1_3));
                MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding(fullyQualifiedMethodName, fullMethodWithParameters, parameterNames, parameterIndex, idType, idDictionary_1_4, tModLoaderVersion.tModLoader_1_4));
            }

            // Main.rand.Next(x) == 0 => Main.rand.NextBool(x)

            // modTile.drop, modTile.dustType
            // using static ModContent
            // Detect bad AddTile AddIngredient
            // Main.player[Main.myPlayer] => Main.LocalPlayer
            // new Vector2(player.position.X + player.width / 2, player.position.Y + player.height / 2) => player.Center

            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeMagicNumberAssignmentExpressions, SyntaxKind.SimpleAssignmentExpression);

            context.RegisterSyntaxNodeAction(AnalyzeRandNextEqualsExpressions, SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression);

            context.RegisterSyntaxNodeAction(AnalyzeMagicNumberEqualsExpressions, SyntaxKind.EqualsExpression,
                                                                                    SyntaxKind.NotEqualsExpression,
                                                                                    SyntaxKind.GreaterThanExpression,
                                                                                    SyntaxKind.GreaterThanOrEqualExpression,
                                                                                    SyntaxKind.LessThanExpression,
                                                                                    SyntaxKind.LessThanOrEqualExpression);

            context.RegisterSyntaxNodeAction(AnalyzeMagicNumberInvocationExpressions, SyntaxKind.InvocationExpression);

            // context.RegisterSyntaxNodeAction(AnalyzeIncorrectParameterInvocationExpressions, SyntaxKind.InvocationExpression);

            context.RegisterCompilationAction(AnalyzeCompilation);

            context.RegisterCompilationStartAction(AnalyzeCompilationStart);

        }

        private List<FieldToIDTypeBinding> FieldToIDTypeBindings;

        class FieldToIDTypeBinding
        {
            // Example: Item - createTile - TileID
            //Type className;
            internal string fullyQualifiedClassName;
            internal string className;
            internal string field;
            //Type idType;
            internal string idType;
            internal IdDictionary idDictionary;
            internal tModLoaderVersion appliesToVersion;

            public FieldToIDTypeBinding(string fullName, string field, string idType, IdDictionary idDictionary, tModLoaderVersion appliesToVersion = tModLoaderVersion.tModLoader_1_3_Or_1_4)
            {
                this.fullyQualifiedClassName = fullName;
                this.className = fullName.Substring(fullName.LastIndexOf(".") + 1);
                this.field = field;
                this.idType = idType;
                this.idDictionary = idDictionary;
                this.appliesToVersion = appliesToVersion;
                //if (idType == "TileID")
                //    idDictionary = TileID.Search;
                //if (idType == "ItemID")
                //    idDictionary = ItemID.Search;
            }

            public override string ToString()
            {
                return $"{fullyQualifiedClassName} {field} {idType}";
            }
        }

        private List<MethodParameterToIDTypeBinding> MethodParameterToIDTypeBindings;
        class MethodParameterToIDTypeBinding
        {
            internal string fullyQualifiedMethodName;
            internal string methodName;
            internal string fullMethodWithParameters;
            internal string[] parameterNames;
            internal int parameterIndex;
            internal string idType;
            internal IdDictionary idDictionary;
            internal tModLoaderVersion appliesToVersion;

            public MethodParameterToIDTypeBinding(string fullyQualifiedMethodName, string fullMethodWithParameters, string[] parameterNames, int parameterIndex, string idType, IdDictionary idDictionary, tModLoaderVersion appliesToVersion = tModLoaderVersion.tModLoader_1_3_Or_1_4)
            {
                this.fullyQualifiedMethodName = fullyQualifiedMethodName;
                this.methodName = fullyQualifiedMethodName.Substring(fullyQualifiedMethodName.LastIndexOf(".") + 1);
                this.fullMethodWithParameters = fullMethodWithParameters;
                this.parameterNames = parameterNames;
                this.parameterIndex = parameterIndex;
                this.idType = idType;
                this.idDictionary = idDictionary;
                this.appliesToVersion = appliesToVersion;
            }

            public override string ToString()
            {
                return $"{fullMethodWithParameters} {parameterIndex} {idType}";
            }
        }

        private void AnalyzeIncorrectParameterInvocationExpressions(SyntaxNodeAnalysisContext context)
        {
            return;

            // if Method name exists in list

            // if 

            // Detect bad AddTile AddIngredient -> Check for presence of ItemID or call to ItemType
        }

        private void AnalyzeMagicNumberInvocationExpressions(SyntaxNodeAnalysisContext context)
        {
            var invocationExpressionSyntax = (InvocationExpressionSyntax)context.Node;

            if (!(invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax MemberAccessExpressionSyntax))
                return;

            string methodName = MemberAccessExpressionSyntax.Name.ToString();
            //if (MemberAccessExpressionSyntax?.Name.ToString() != "AddIngredient")
            //    return;

            var argumentListSyntax = invocationExpressionSyntax.ArgumentList as ArgumentListSyntax;
            if (argumentListSyntax == null)
                return;
            int argumentCount = argumentListSyntax.Arguments.Count;


            //if (argumentCount != 1 && argumentCount != 2) 
            //    return;

            var memberSymbol = context.SemanticModel.GetSymbolInfo(MemberAccessExpressionSyntax).Symbol as IMethodSymbol;
            if (memberSymbol == null)
                return;

            //if (argumentCount != memberSymbol.Parameters.Length) // < is fine, optional?
            //    return;

            string fullyQualifiedMethodName = memberSymbol.ToString();

            //   if (!memberSymbol.ToString().StartsWith("Terraria.ModLoader.ModRecipe.AddIngredient") ?? true) return;

            var parameterTypeNames = memberSymbol.Parameters.Select(p => p.Type.Name);

            // Find exact MethodParameterToIDTypeBinding related to this InvocationExpressionSyntax
            var methodParameterToIDTypeBinding = MethodParameterToIDTypeBindings.FirstOrDefault(x => x.fullMethodWithParameters == fullyQualifiedMethodName && x.parameterNames.SequenceEqual(parameterTypeNames) && x.appliesToVersion.HasFlag(tModLoaderVersion));
            if (methodParameterToIDTypeBinding == null)
                return;

            if (argumentCount < methodParameterToIDTypeBinding.parameterIndex) // IS this the bug? SetDefaults();
                return;

            // Check if parameter at index is literal number: SetDefaults(111, 2)
            if (!(argumentListSyntax.Arguments[methodParameterToIDTypeBinding.parameterIndex].Expression is LiteralExpressionSyntax parameter && parameter.IsKind(SyntaxKind.NumericLiteralExpression)))
                return;
            //if (!(memberSymbol.Parameters[methodParameterToIDTypeBinding.parameterIndex] is LiteralExpressionSyntax right && right.IsKind(SyntaxKind.NumericLiteralExpression)))
            //     return;

            //  methodParameterToIDTypeBinding.parameterIndex


            //if (p.Length != 2)
            //    return;

            //if (p[0].Type.Name != "Int32" || p[1].Type.Name != "Int32")
            //    return;

            Console.WriteLine();

            int rightValue = (int)parameter.Token.Value;

            if (methodParameterToIDTypeBinding.idDictionary.ContainsId(rightValue))
            //if (rightValue > 0 && rightValue < methodParameterToIDTypeBinding.idDictionary.Count /* ItemID.Count*/)
            {
                //string result = "ItemID." + ItemID.Search.GetName(rightValue);
                string result = $"{methodParameterToIDTypeBinding.idType}.{methodParameterToIDTypeBinding.idDictionary.GetName(rightValue)}"; // + ItemID.Search.GetName(rightValue);

                var builder = ImmutableDictionary.CreateBuilder<string, string>();
                builder["result"] = result;
                builder["idType"] = methodParameterToIDTypeBinding.idType;
                var properties = builder.ToImmutable();

                var diagnostic = Diagnostic.Create(ChangeMagicNumberToIDRule, parameter.GetLocation(), properties, rightValue, result);
                context.ReportDiagnostic(diagnostic);
            }

        }

        private void AnalyzeRandNextEqualsExpressions(SyntaxNodeAnalysisContext context)
        {
            var binaryExpressionSyntax = (BinaryExpressionSyntax)context.Node;

            // Check if right side is literal number: a == 123
            if (!(binaryExpressionSyntax.Right is LiteralExpressionSyntax right && right.IsKind(SyntaxKind.NumericLiteralExpression)))
                return;

            ISymbol symbol;
            // Check if left is invoking a method: a.b() == 123
            if (binaryExpressionSyntax.Left is InvocationExpressionSyntax invocationExpressionSyntax)
            {
                symbol = context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol;
            }
            else
                return;

            // Check if left Type exists: item.b = 123
            if (symbol == null || symbol.ContainingType == null)
                return;

            if (!(symbol is IMethodSymbol methodSymbol))
                return;

            string containingType = symbol.ContainingType.ToString();

            string methodName = methodSymbol.Name;
            if (containingType != "Terraria.Utilities.UnifiedRandom" || methodName != "Next")
                return;

            if (methodSymbol.Parameters.Length != 1 || methodSymbol.Parameters[0].Type.Name != "Int32")
                return;

            //var argumentListSyntax = invocationExpressionSyntax.ArgumentList;

            //if (!(argumentListSyntax.Arguments[0].Expression is ExpressionSyntax argument))
            //    return;

            //if (!(argumentListSyntax.Arguments[0].Expression is LiteralExpressionSyntax parameter && parameter.IsKind(SyntaxKind.NumericLiteralExpression)))
            //    return;

            //int parameterValue = (int)parameter.Token.Value;

            //if (parameterValue != 0)
            //    return;

            string original = binaryExpressionSyntax.ToFullString();

            //string result = $"NextBool({argument.GetText()})";

            bool not = binaryExpressionSyntax.Kind() == SyntaxKind.NotEqualsExpression;

            var methodcall = invocationExpressionSyntax.Expression as MemberAccessExpressionSyntax;
            var nextBool = invocationExpressionSyntax.ReplaceNode(methodcall.Name, SyntaxFactory.IdentifierName("NextBool"));
            nextBool = nextBool.WithoutTrailingTrivia();
            var notNextBool = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, nextBool);
            
            //var c = binaryExpressionSyntax.remove(binaryExpressionSyntax.OperatorToken, SyntaxRemoveOptions.KeepExteriorTrivia);
            //c = c.RemoveNode(binaryExpressionSyntax.);

            string result = not ? notNextBool.ToFullString() : nextBool.ToFullString();

            //invocationExpressionSyntax.ReplaceToken(invocationExpressionSyntax.Expression as MemberAccessException mem)

            var builder = ImmutableDictionary.CreateBuilder<string, string>();
            builder["result"] = result;
            var properties = builder.ToImmutable();

            // "The expression {0} should be changed to {1} for readability"
            var diagnostic = Diagnostic.Create(SimplifyUnifiedRandomRule, binaryExpressionSyntax.GetLocation(), properties, original, result);
            context.ReportDiagnostic(diagnostic);

            Console.WriteLine();
        }

        // assignment and equals can share code.

        private void AnalyzeMagicNumberEqualsExpressions(SyntaxNodeAnalysisContext context)
        {
            // Only support EqualsExpression: a == b
            var binaryExpressionSyntax = (BinaryExpressionSyntax)context.Node;

            // Check if right side is literal number: a == 123
            if (!(binaryExpressionSyntax.Right is LiteralExpressionSyntax right && right.IsKind(SyntaxKind.NumericLiteralExpression)))
                return;

            ISymbol symbol;
            // Check if left is just a field: a = 123
            if (binaryExpressionSyntax.Left is IdentifierNameSyntax identifierNameSyntax)
            {
                symbol = context.SemanticModel.GetSymbolInfo(identifierNameSyntax).Symbol;
            }
            // Check if left is accessing a member: a.b = 123
            else if (binaryExpressionSyntax.Left is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
            {
                symbol = context.SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax).Symbol;
            }
            else
                return;

            // Check if left Type exists: item.b = 123
            if (symbol == null || symbol.ContainingType == null)
                return;

            if (!(symbol is IFieldSymbol fieldSymbol))
                return;

            string containingType = symbol.ContainingType.ToString();

            string fieldName = fieldSymbol.Name;
            var FieldToIDTypeBinding = FieldToIDTypeBindings.FirstOrDefault(x => x.fullyQualifiedClassName == containingType && x.field == fieldName && x.appliesToVersion.HasFlag(tModLoaderVersion));
            if (FieldToIDTypeBinding == null)
                return;

            int rightValue = (int)right.Token.Value;

            if (FieldToIDTypeBinding.idDictionary.ContainsId(rightValue))
            //if (rightValue > 0 && rightValue < FieldToIDTypeBinding.idDictionary.Count /* ItemID.Count*/)
            {
                string result = $"{FieldToIDTypeBinding.idType}.{FieldToIDTypeBinding.idDictionary.GetName(rightValue)}";

                var builder = ImmutableDictionary.CreateBuilder<string, string>();
                builder["result"] = result;
                builder["idType"] = FieldToIDTypeBinding.idType;
                var properties = builder.ToImmutable();

                var diagnostic = Diagnostic.Create(ChangeMagicNumberToIDRule, right.GetLocation(), properties, rightValue, result);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private void AnalyzeMagicNumberAssignmentExpressions(SyntaxNodeAnalysisContext context)
        {
            // Only support simple assignment: a = b
            var assignmentExpressionSyntax = (AssignmentExpressionSyntax)context.Node;
            if (!assignmentExpressionSyntax.IsKind(SyntaxKind.SimpleAssignmentExpression))
                return;

            // Check if right side is literal number: a = 123
            if (!(assignmentExpressionSyntax.Right is LiteralExpressionSyntax right && right.IsKind(SyntaxKind.NumericLiteralExpression)))
                return;

            ISymbol symbol;
            // Check if left is just a field: a = 123
            if (assignmentExpressionSyntax.Left is IdentifierNameSyntax identifierNameSyntax)
            {
                symbol = context.SemanticModel.GetSymbolInfo(identifierNameSyntax).Symbol;
            }
            // Check if left is accessing a member: a.b = 123
            else if (assignmentExpressionSyntax.Left is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
            {
                symbol = context.SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax).Symbol;
            }
            else
                return;

            // Check if left Type exists: item.b = 123
            if (symbol == null || symbol.ContainingType == null)
                return;

            if (!(symbol is IFieldSymbol fieldSymbol))
                return;

            string containingType = symbol.ContainingType.ToString();
            //if (!containingType.Equals("Terraria.Item"))
            //     return;

            string fieldName = fieldSymbol.Name;
            var FieldToIDTypeBinding = FieldToIDTypeBindings.FirstOrDefault(x => x.fullyQualifiedClassName == containingType && x.field == fieldName && x.appliesToVersion.HasFlag(tModLoaderVersion));
            if (FieldToIDTypeBinding == null)
                return;

            int rightValue = (int)right.Token.Value;

            if (FieldToIDTypeBinding.idDictionary.ContainsId(rightValue))
            //if (rightValue > 0 && rightValue < FieldToIDTypeBinding.idDictionary.Count /* ItemID.Count*/)
            {
                //string result = "ItemID." + ItemID.Search.GetName(rightValue);
                string result = $"{FieldToIDTypeBinding.idType}.{FieldToIDTypeBinding.idDictionary.GetName(rightValue)}"; // + ItemID.Search.GetName(rightValue);

                var builder = ImmutableDictionary.CreateBuilder<string, string>();
                builder["result"] = result;
                builder["idType"] = FieldToIDTypeBinding.idType;
                var properties = builder.ToImmutable();

                var diagnostic = Diagnostic.Create(ChangeMagicNumberToIDRule, right.GetLocation(), properties, rightValue, result);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
        {
            var compilation = context.Compilation;
            bool tMod13 = compilation.References.Any(x => x.Display.EndsWith("tModLoader.exe"));
            bool tMod14 = compilation.References.Any(x => x.Display.EndsWith("tModLoader.dll"));

            if (tMod13)
                tModLoaderVersion = tModLoaderVersion.tModLoader_1_3;
            if (tMod14)
                tModLoaderVersion = tModLoaderVersion.tModLoader_1_4;

            Console.WriteLine("tModLoaderVersion is " + tModLoaderVersion);

            //var tmodReference = compilation.References.FirstOrDefault(x => x.Display.EndsWith("tModLoader.exe"));

            // TODO: can possibly check .png files if  <AdditionalFiles Include="**\*.png" /> added
            //var b = context.Options.AdditionalFiles.Length;

            //foreach (var item in context.Options.AdditionalFiles)
            //{
            //    var a = item.Path;
            //    //context.Compilation.SourceModule.Locations.
            //}
        }

        private void AnalyzeCompilation(CompilationAnalysisContext context)
        {
            var compilation = context.Compilation;
            //int referenceCount = compilation.References.Count();

            //var a = compilation.References.FirstOrDefault(x => x.Display.Contains("tModLoader.exe"));

            //var b = compilation.References.FirstOrDefault(x => x.Display.Contains("tModLoader.dll"));

            //if (referenceCount > 5)
            //{
            //    //context.ReportDiagnostic(
            //    //    Diagnostic.Create(
            //    //        TooManyReferences,
            //    //        null,
            //    //        compilation.AssemblyName,
            //    //        referenceCount));
            //}
        }
    }
}
