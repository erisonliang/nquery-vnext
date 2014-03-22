using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Syntax;

namespace NQuery.Authoring.Completion.Providers
{
    internal sealed class TypeCompletionProvider : ICompletionProvider
    {
        public IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var token = syntaxTree.Root.FindTokenOnLeft(position);
            var castExpression = token.Parent.AncestorsAndSelf()
                                             .OfType<CastExpressionSyntax>()
                                             .FirstOrDefault(c => !c.AsKeyword.IsMissing && c.AsKeyword.Span.End <= position);

            if (castExpression == null)
                return Enumerable.Empty<CompletionItem>();

            return from typeName in SyntaxFacts.GetTypeNames()
                   select GetCompletionItem(typeName);
        }

        private static CompletionItem GetCompletionItem(string typeName)
        {
            return new CompletionItem(typeName, typeName, typeName, NQueryGlyph.Type);
        }
    }
}