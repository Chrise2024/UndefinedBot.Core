using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace UndefinedBot.Generator;

[Generator]
public sealed class PluginConstructorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ClassDeclarationSyntax> provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                (s, _) => s is ClassDeclarationSyntax,
                (ctx, _) =>
                {
                    ClassDeclarationSyntax classDeclarationSyntax = (ClassDeclarationSyntax)ctx.Node;
                    foreach (AttributeSyntax attributeSyntax in classDeclarationSyntax.AttributeLists.SelectMany(attributeListSyntax => attributeListSyntax.Attributes))
                    {
                        if (ctx.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                            continue;

                        if (attributeSymbol.ContainingType.ToDisplayString() == "UndefinedBot.Core.PluginAttribute")
                            return (classDeclarationSyntax, true);
                    }
                    return (classDeclarationSyntax, false);
                })
            .Where(t => t.Item2)
            .Select((t, _) => t.Item1);

        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(provider.Collect()),
            (spc, source) =>
        {
            (Compilation? compilation, ImmutableArray<ClassDeclarationSyntax> declarationSyntax) = source;

            foreach (var classDeclaration in declarationSyntax)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                INamedTypeSymbol? classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
                if (classSymbol is null)
                    continue;
                
                string originalConstructorBody = classDeclaration.Members
                    .OfType<ConstructorDeclarationSyntax>()
                    .FirstOrDefault(x => x.ParameterList.Parameters.Count == 0)
                    ?.Body?.ToString() ?? "{}";

                string generatedCode = $$"""
                                         namespace {{classSymbol.ContainingNamespace}};

                                         [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
                                         public partial class {{classSymbol.Name}} : UndefinedBot.Core.Plugin.BasePlugin
                                         {
                                             public {{classSymbol.Name}}(UndefinedBot.Core.Plugin.PluginDependencyCollection dependencyCollection) : base(dependencyCollection) {{originalConstructorBody}}
                                         }
                                         """;
                spc.AddSource($"{classSymbol.Name}.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
            }
        });
    }
}