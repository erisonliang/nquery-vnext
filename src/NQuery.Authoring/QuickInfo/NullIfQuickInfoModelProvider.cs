using System;
using System.ComponentModel.Composition;

using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.QuickInfo
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class NullIfQuickInfoModelProvider : QuickInfoModelProvider<NullIfExpressionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, NullIfExpressionSyntax node)
        {
            var keywordSpan = node.NullIfKeyword.Span;
            return !keywordSpan.Contains(position)
                       ? null
                       : new QuickInfoModel(semanticModel, keywordSpan, NQueryGlyph.Function, SymbolMarkup.ForNullIfSymbol());
        }
    }
}