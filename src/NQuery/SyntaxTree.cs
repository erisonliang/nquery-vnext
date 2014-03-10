using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using NQuery.Syntax;
using NQuery.Text;

namespace NQuery
{
    public sealed class SyntaxTree
    {
        private readonly CompilationUnitSyntax _root;
        private readonly TextBuffer _textBuffer;

        private Dictionary<object, object> _parentFromChild;

        private SyntaxTree(TextBuffer textBuffer, Func<Parser, CompilationUnitSyntax> parseMethod)
        {
            var parser = new Parser(textBuffer, this);
            _root = parseMethod(parser);
            _textBuffer = textBuffer;
        }

        public static SyntaxTree ParseQuery(string source)
        {
            var textBuffer = TextBuffer.From(source);
            return ParseQuery(textBuffer);
        }

        public static SyntaxTree ParseQuery(TextBuffer textBuffer)
        {
            return new SyntaxTree(textBuffer, p => p.ParseRootQuery());
        }

        public static SyntaxTree ParseExpression(string source)
        {
            var textBuffer = TextBuffer.From(source);
            return ParseExpression(textBuffer);
        }

        public static SyntaxTree ParseExpression(TextBuffer textBuffer)
        {
            return new SyntaxTree(textBuffer, p => p.ParseRootExpression());
        }

        public IEnumerable<Diagnostic> GetDiagnostics()
        {
            return from token in Root.DescendantTokens(descendIntoTrivia: true)
                   let leadingDiagnostics = token.LeadingTrivia.SelectMany(t => t.Diagnostics)
                   let tokenDiagnostics = token.Diagnostics
                   let trailingDianostics = token.TrailingTrivia.SelectMany(t => t.Diagnostics)
                   from d in leadingDiagnostics.Concat(tokenDiagnostics).Concat(trailingDianostics)
                   select d;
        }

        private T GetParent<T>(object child)
             where T: class
        {
            if (_parentFromChild == null)
                Interlocked.CompareExchange(ref _parentFromChild, GetParents(_root), null);

            object parent;
            _parentFromChild.TryGetValue(child, out parent);
            return parent as T;
        }

        internal SyntaxNode GetParentNode(SyntaxNode node)
        {
            return GetParent<SyntaxNode>(node);
        }

        internal SyntaxNode GetParentNode(SyntaxToken token)
        {
            return GetParent<SyntaxNode>(token);
        }

        internal SyntaxToken GetParentToken(SyntaxTrivia trivia)
        {
            return GetParent<SyntaxToken>(trivia);
        }

        internal SyntaxTrivia GetParentTrivia(StructuredTriviaSyntax structuredTrivia)
        {
            return GetParent<SyntaxTrivia>(structuredTrivia);
        }

        private static Dictionary<object,object> GetParents(SyntaxNode compilationUnit)
        {
            var result = new Dictionary<object, object>();
            GetParents(result, compilationUnit);
            return result;
        }

        private static void GetParents(IDictionary<object, object> parents, SyntaxNode parent)
        {
            foreach (var nodeOrToken in parent.ChildNodesAndTokens())
            {
                if (nodeOrToken.IsNode)
                {
                    var node = nodeOrToken.AsNode();
                    parents.Add(node, parent);
                    GetParents(parents, node);
                }
                else
                {
                    var token = nodeOrToken.AsToken();
                    parents.Add(token, parent);

                    foreach (var trivia in token.LeadingTrivia.Concat(token.TrailingTrivia))
                    {
                        parents.Add(trivia, token);
                        var structure = trivia.Structure;
                        if (structure != null)
                        {
                            parents.Add(structure, trivia);
                            GetParents(parents, structure);
                        }
                    }
                }
            }
        }

        public static SyntaxTree Empty = ParseQuery(string.Empty);

        public CompilationUnitSyntax Root
        {
            get { return _root; }
        }

        public TextBuffer TextBuffer
        {
            get { return _textBuffer; }
        }
    }
}