using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class PropertyAccessExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _target;
        private readonly SyntaxToken _dot;
        private readonly SyntaxToken _name;

        public PropertyAccessExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax target, SyntaxToken dot, SyntaxToken name)
            : base(syntaxTree)
        {
            _target = target;
            _dot = dot;
            _name = name;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.PropertyAccessExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _target;
            yield return _dot;
            yield return _name;
        }

        public ExpressionSyntax Target
        {
            get { return _target; }
        }

        public SyntaxToken Dot
        {
            get { return _dot; }
        }

        public SyntaxToken Name
        {
            get { return _name; }
        }
    }
}