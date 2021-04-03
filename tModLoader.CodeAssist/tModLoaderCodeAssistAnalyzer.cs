using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using tModLoader.CodeAssist.Terraria.ID;

namespace tModLoader.CodeAssist
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class tModLoaderCodeAssistAnalyzer : DiagnosticAnalyzer
    {
        public const string ChangeMagicNumberToIDDiagnosticId = "ChangeMagicNumberToID";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString ChangeMagicNumberToIDTitle = new LocalizableResourceString(nameof(Resources.ChangeMagicNumberToIDTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString ChangeMagicNumberToIDMessageFormat = new LocalizableResourceString(nameof(Resources.ChangeMagicNumberToIDMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString ChangeMagicNumberToIDDescription = new LocalizableResourceString(nameof(Resources.ChangeMagicNumberToIDDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Terraria.ID";

        private static DiagnosticDescriptor ChangeMagicNumberToIDRule = new DiagnosticDescriptor(ChangeMagicNumberToIDDiagnosticId, ChangeMagicNumberToIDTitle, ChangeMagicNumberToIDMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: ChangeMagicNumberToIDDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(ChangeMagicNumberToIDRule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information

            IdDictionary soundIDIDDictionary = IdDictionary.Create(typeof(SoundID), typeof(int));

            FieldToIDTypeBindings = new List<FieldToIDTypeBinding>();
            FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.Item", "createTile", "TileID", TileID.Search));
            FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.Item", "type", "ItemID", ItemID.Search));
            FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.Item", "shoot", "ProjectileID", ProjectileID.Search));
            FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.Item", "useStyle", "ItemUseStyleID", IdDictionary.Create(typeof(ItemUseStyleID), typeof(int))));
            FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.Item", "rare", "ItemRarityID", IdDictionary.Create(typeof(ItemRarityID), typeof(int))));
            FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.NPC", "type", "NPCID", NPCID.Search));
            FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.Main", "netMode", "NetmodeID", IdDictionary.Create(typeof(NetmodeID), typeof(int))));
            FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.ModLoader.ModTile", "soundType", "SoundID", soundIDIDDictionary));
            FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.ModLoader.ModTile", "dustType", "DustID", DustID.Search));
            FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.ModLoader.ModWall", "dustType", "DustID", DustID.Search));

            // Could check parameter name, or check parameter list and index. 
            MethodParameterToIDTypeBindings = new List<MethodParameterToIDTypeBinding>();
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Item.CloneDefaults", "Terraria.Item.CloneDefaults(int)", new string[] { "Int32" }, 0, "ItemID", ItemID.Search));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.ModLoader.ModRecipe.AddTile", "Terraria.ModLoader.ModRecipe.AddTile(int)", new string[] { "Int32" }, 0, "TileID", TileID.Search));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.ModLoader.ModRecipe.AddIngredient", "Terraria.ModLoader.ModRecipe.AddIngredient(int, int)", new string[] { "Int32", "Int32" }, 0, "ItemID", ItemID.Search));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.ModLoader.ModRecipe.SetResult", "Terraria.ModLoader.ModRecipe.SetResult(int, int)", new string[] { "Int32", "Int32" }, 0, "ItemID", ItemID.Search));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.NetMessage.SendData", "Terraria.NetMessage.SendData(int, int, int, Terraria.Localization.NetworkText, int, float, float, float, int, int, int)", new string[] { "Int32", "Int32", "Int32", "NetworkText", "Int32", "Single", "Single", "Single", "Int32", "Int32", "Int32" }, 0, "MessageID", IdDictionary.Create(typeof(MessageID), typeof(byte))));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Main.PlaySound", "Terraria.Main.PlaySound(int, Microsoft.Xna.Framework.Vector2, int)", new string[] { "Int32", "Vector2", "Int32" }, 0, "SoundID", soundIDIDDictionary));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Main.PlaySound", "Terraria.Main.PlaySound(int, int, int, int, float, float)", new string[] { "Int32", "Int32", "Int32", "Int32", "Single", "Single" }, 0, "SoundID", soundIDIDDictionary));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Projectile.NewProjectile", "Terraria.Projectile.NewProjectile(Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, int, int, float, int, float, float)", new string[] { "Vector2", "Vector2", "Int32", "Int32", "Single", "Int32", "Single", "Single" }, 2, "ProjectileID", ProjectileID.Search));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Projectile.NewProjectile", "Terraria.Projectile.NewProjectile(float, float, float, float, int, int, float, int, float, float)", new string[] { "Single", "Single", "Single", "Single", "Int32", "Int32", "Single", "Int32", "Single", "Single" }, 4, "ProjectileID", ProjectileID.Search));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Projectile.NewProjectileDirect", "Terraria.Projectile.NewProjectileDirect(Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, int, int, float, int, float, float)", new string[] { "Vector2", "Vector2", "Int32", "Int32", "Single", "Int32", "Single", "Single" }, 2, "ProjectileID", ProjectileID.Search));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Dust.NewDust", "Terraria.Dust.NewDust(Microsoft.Xna.Framework.Vector2, int, int, int, float, float, int, Microsoft.Xna.Framework.Color, float)", new string[] { "Vector2", "Int32", "Int32", "Int32", "Single", "Single", "Int32", "Color", "Single" }, 3, "DustID", DustID.Search));
            MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Dust.NewDustDirect", "Terraria.Dust.NewDustDirect(Microsoft.Xna.Framework.Vector2, int, int, int, float, float, int, Microsoft.Xna.Framework.Color, float)", new string[] { "Vector2", "Int32", "Int32", "Int32", "Single", "Single", "Int32", "Color", "Single" }, 3, "DustID", DustID.Search));
            //MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Dust.NewDustPerfect", "Terraria.Dust.NewDustPerfect(Microsoft.Xna.Framework.Vector2, int, Microsoft.Xna.Framework.Vector2?, int, Microsoft.Xna.Framework.Color, float)", new string[] { "Vector2", "Int32", "Vector2?", "Int32", "Color", "Single" }, 1, "DustID", DustID.Search));

            // Main.rand.Next(x) == 0 => Main.rand.NextBool(x)

            //modTile.drop, modTile.soundType, modTile.dustType
            // using static ModContent
            // Detect bad AddTile AddIngredient
            // Main.player[Main.myPlayer] => Main.LocalPlayer
            // new Vector2(player.position.X + player.width / 2, player.position.Y + player.height / 2) => player.Center

            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeMagicNumberAssignmentExpressions, SyntaxKind.SimpleAssignmentExpression);

            context.RegisterSyntaxNodeAction(AnalyzeMagicNumberEqualsExpressions, SyntaxKind.EqualsExpression,
                                                                                    SyntaxKind.NotEqualsExpression,
                                                                                    SyntaxKind.GreaterThanExpression,
                                                                                    SyntaxKind.GreaterThanOrEqualExpression,
                                                                                    SyntaxKind.LessThanExpression,
                                                                                    SyntaxKind.LessThanOrEqualExpression);

            context.RegisterSyntaxNodeAction(AnalyzeMagicNumberInvocationExpressions, SyntaxKind.InvocationExpression);

           // context.RegisterSyntaxNodeAction(AnalyzeIncorrectParameterInvocationExpressions, SyntaxKind.InvocationExpression);
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

            public FieldToIDTypeBinding(string fullName, string field, string idType, IdDictionary idDictionary)
            {
                this.fullyQualifiedClassName = fullName;
                this.className = fullName.Substring(fullName.LastIndexOf(".") + 1);
                this.field = field;
                this.idType = idType;
                this.idDictionary = idDictionary;
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

            public MethodParameterToIDTypeBinding(string fullyQualifiedMethodName, string fullMethodWithParameters, string[] parameterNames, int parameterIndex, string idType, IdDictionary idDictionary)
            {
                this.fullyQualifiedMethodName = fullyQualifiedMethodName;
                this.methodName = fullyQualifiedMethodName.Substring(fullyQualifiedMethodName.LastIndexOf(".") + 1);
                this.fullMethodWithParameters = fullMethodWithParameters;
                this.parameterNames = parameterNames;
                this.parameterIndex = parameterIndex;
                this.idType = idType;
                this.idDictionary = idDictionary;
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
            var methodParameterToIDTypeBinding = MethodParameterToIDTypeBindings.FirstOrDefault(x => x.fullMethodWithParameters == fullyQualifiedMethodName && x.parameterNames.SequenceEqual(parameterTypeNames));
            if (methodParameterToIDTypeBinding == null)
                return;

            if (argumentCount < methodParameterToIDTypeBinding.parameterIndex)
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
            var FieldToIDTypeBinding = FieldToIDTypeBindings.FirstOrDefault(x => x.fullyQualifiedClassName == containingType && x.field == fieldName);
            if (FieldToIDTypeBinding == null)
                return;

            int rightValue = (int)right.Token.Value;

            if(FieldToIDTypeBinding.idDictionary.ContainsId(rightValue))
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
            var FieldToIDTypeBinding = FieldToIDTypeBindings.FirstOrDefault(x => x.fullyQualifiedClassName == containingType && x.field == fieldName);
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
    }
}
