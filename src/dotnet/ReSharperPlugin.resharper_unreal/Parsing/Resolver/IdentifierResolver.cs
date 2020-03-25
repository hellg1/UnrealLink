using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.CSharp.Highlighting;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Cpp.Caches;
using JetBrains.ReSharper.Psi.Cpp.Daemon;
using JetBrains.ReSharper.Psi.Cpp.Language;
using JetBrains.ReSharper.Psi.Cpp.Symbols;
using JetBrains.ReSharper.Psi.Cpp.Util;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Util;
using JetBrains.Util.Caches;
using ReSharperPlugin.UnrealEditor.Parsing.Node;

namespace ReSharperPlugin.UnrealEditor.Parsing.Resolver
{
    [SolutionComponent]
    public class IdentifierResolver : IIdentifierResolver
    {
        private const int MaxSizeOfCache = 1 << 6;

        /*private readonly DirectMappedCache<string, CppNamespaceSymbol> myNamespaceCache =
            new DirectMappedCache<string, CppNamespaceSymbol>(MaxSizeOfCache);*/

        private readonly DirectMappedCache<string, CppClassSymbol> myClassCache =
            new DirectMappedCache<string, CppClassSymbol>(MaxSizeOfCache);

        private readonly CppGlobalSymbolCache myCppSymbolNameCache;

        private readonly DirectMappedCache<string, CppDeclaratorSymbol> myDeclaratorCache =
            new DirectMappedCache<string, CppDeclaratorSymbol>(MaxSizeOfCache);

        private readonly ISolution mySolution;
        private readonly IPsiServices myPsiServices;
        private readonly ISymbolCache mySymbolCache;


        public IdentifierResolver(ISolution solution, IPsiServices psiServices, CppGlobalSymbolCache cppSymbolNameCache,
            ISymbolCache symbolCache)
        {
            mySolution = solution;
            myPsiServices = psiServices;
            myCppSymbolNameCache = cppSymbolNameCache;
            mySymbolCache = symbolCache;
        }

        public bool TryGetClassSymbol(string name, CppNamespaceSymbol namespaceSymbol,
            out CppClassSymbol classSymbol)
        {
            if (name.First().IsLowerFast())
            {
                classSymbol = null;
                return false;
            }

            classSymbol = GetClassSymbol(name, namespaceSymbol);
            return classSymbol != null;
        }

        public bool TryGetMemberFunctionSymbol(string name, CppClassSymbol classSymbol,
            out CppDeclaratorSymbol memberFunctionSymbol)
        {
            memberFunctionSymbol = GetMemberFunctionSymbol(classSymbol, name);
            return memberFunctionSymbol != null;
        }


        /*
        public bool TryGetNamespaceSymbol(string name, out CppNamespaceSymbol namespaceSymbol)
        {
            namespaceSymbol = GetNamespaceSymbol(name);
            return namespaceSymbol != null;
        }
        */

        public bool TryGetGlobalFunctionSymbol(string name,
            CppNamespaceSymbol namespaceSymbol,
            out CppDeclaratorSymbol globalFunctionSymbol)
        {
            globalFunctionSymbol = GetGlobalFunctionSymbol(namespaceSymbol, name);
            return globalFunctionSymbol != null;
        }

        public bool TryGetBuildModuleDeclaredElement(string moduleName, out IClrDeclaredElement declaredElement)
        {
            using (ReadLockCookie.Create())
            {
                declaredElement = mySymbolCache
                    .GetSymbolScope(LibrarySymbolScope.NONE, true)
                    .GetElementsByShortName(moduleName)
                    .SingleOrNull();
            }

            return declaredElement != null;
        }

        public string ResolveAttributeId(IdentifierNode node)
        {
            if (node.Qualifier == null)
            {
                if (TryGetBuildModuleDeclaredElement(node.Name, out var declaredElement))
                {
                    node.CsDeclaredElement = declaredElement;
                    return CSharpHighlightingAttributeIds.CLASS;
                }

                if (TryGetClassSymbol(node.Name, null, out var cppClassSymbol))
                {
                    node.CppDeclaredElement = GetCppDeclaredElement(cppClassSymbol);
                    return CppHighlightingAttributeIds.CPP_CLASS_ATTRIBUTE;
                }

                if (TryGetGlobalFunctionSymbol(node.Name, null, out var globalFunctionSymbol))
                {
                    node.CppDeclaredElement = GetCppDeclaredElement(globalFunctionSymbol);
                    return CppHighlightingAttributeIds.CPP_GLOBAL_FUNCTION_ATTRIBUTE;
                }

                return null;
            }

            if (node.Qualifier.CppDeclaredElement?.GetSymbol() is CppClassSymbol classSymbol)
                if (TryGetMemberFunctionSymbol(node.Name, classSymbol, out var memberFunctionSymbol))
                {
                    node.CppDeclaredElement = GetCppDeclaredElement(memberFunctionSymbol);
                    return CppHighlightingAttributeIds.CPP_MEMBER_FUNCTION_ATTRIBUTE;
                }

            return null;
        }

        /*
        [CanBeNull]
        private CppNamespaceSymbol GetNamespaceSymbol(string name) =>
            myNamespaceCache.GetOrCreate(name, s =>
            {
                var symbolsByShortName = myCppSymbolNameCache.SymbolNameCache.GetSymbolsByShortName(s);
                var namespaceSymbol = symbolsByShortName.OfType<CppNamespaceSymbol>().SingleOrNull();
                return namespaceSymbol;
            });
            */

        [CanBeNull]
        internal CppClassSymbol GetClassSymbol(string name, [CanBeNull] CppNamespaceSymbol namespaceSymbol)
        {
            return myClassCache.GetOrCreate(name, s =>
            {
                if (namespaceSymbol == null)
                {
                    var symbolsByShortName = GetSymbolsByShortName(s);
                    var classSymbol = symbolsByShortName
                        .OfType<CppClassSymbol>()
                        .Where(symbol => ReferenceEquals(symbol.Parent.Name.Name, CppGlobalNamespaceId.INSTANCE))
                        .Where(symbol => symbol.Tag == CppClassTag.CLASS_TAG ||
                                         symbol.Tag == CppClassTag.STRUCT_TAG)
                        .SingleOrNull();
                    return classSymbol;
                }

                return null;
            });
        }

        [CanBeNull]
        internal CppDeclaratorSymbol GetMemberFunctionSymbol([NotNull] CppClassSymbol classSymbol, string method)
        {
            return myDeclaratorCache.GetOrCreate(method, s =>
            {
                //todo
                return classSymbol.Children.OfType<CppDeclaratorSymbol>()
                    .Where(symbol => s == symbol.Name.Name.ToString())
                    .SingleOrNull();
            });
        }


        [CanBeNull]
        private CppDeclaratorSymbol GetGlobalFunctionSymbol([CanBeNull] CppNamespaceSymbol namespaceSymbol,
            string @namespace)
        {
            return myDeclaratorCache.GetOrCreate(@namespace, s =>
            {
                if (namespaceSymbol == null)
                {
                    var symbolsByShortName = GetSymbolsByShortName(s);
                    var functionSymbol = symbolsByShortName
                        .OfType<CppDeclaratorSymbol>()
                        .Where(symbol => symbol.IsFunction())
                        .Where(symbol => symbol.Parent.GetShortName().IsNullOrEmpty())
                        .SingleOrNull();
                    return functionSymbol;
                }

                return null;
            });
        }

        private IEnumerable<ICppSymbol> GetSymbolsByShortName(string name)
        {
            using (ReadLockCookie.Create())
            {
                return myCppSymbolNameCache.SymbolNameCache.GetSymbolsByShortName(name);
            }
        }

        [NotNull]
        public CppParserSymbolDeclaredElement GetCppDeclaredElement(ICppParserSymbol classSymbol)
        {
            return new CppParserSymbolDeclaredElement(myPsiServices, classSymbol);
        }
    }
}