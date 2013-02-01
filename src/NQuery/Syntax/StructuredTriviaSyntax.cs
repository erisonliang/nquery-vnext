using System;

namespace NQuery.Syntax
{
    public abstract class StructuredTriviaSyntax : SyntaxNode
    {
        protected StructuredTriviaSyntax(SyntaxTree syntaxTree)
            : base(syntaxTree)
        {
        }

        public SyntaxTrivia ParentTrivia
        {
            get { return SyntaxTree == null ? null : SyntaxTree.GetParentTrivia(this); }
        }
    }
}