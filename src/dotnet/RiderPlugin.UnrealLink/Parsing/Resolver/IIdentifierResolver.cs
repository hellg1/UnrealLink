using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Cpp.Symbols;
using ReSharperPlugin.UnrealEditor.Parsing.Node;

namespace ReSharperPlugin.UnrealEditor.Parsing.Resolver
{
    public interface IIdentifierResolver
    {
        // bool TryGetNamespaceSymbol(string name, out CppNamespaceSymbol namespaceSymbol);

        bool TryGetClassSymbol(string name, [CanBeNull] CppNamespaceSymbol namespaceSymbol,
            [CanBeNull] out CppClassSymbol classSymbol);

        bool TryGetMemberFunctionSymbol(string name, CppClassSymbol classSymbol,
            [CanBeNull] out CppDeclaratorSymbol memberFunctionSymbol);

        bool TryGetGlobalFunctionSymbol(string name,
            CppNamespaceSymbol namespaceSymbol,
            [CanBeNull] out CppDeclaratorSymbol globalFunctionSymbol);

        bool TryGetBuildModuleDeclaredElement(string moduleName,
            [CanBeNull] out IClrDeclaredElement declaredElement);

        [CanBeNull]
        string ResolveAttributeId(IdentifierNode identifierNode);
    }
}