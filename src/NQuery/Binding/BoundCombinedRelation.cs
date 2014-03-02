﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Binding
{
    internal sealed class BoundCombinedRelation : BoundRelation
    {
        private readonly BoundQueryCombinator _combinator;
        private readonly BoundRelation _left;
        private readonly BoundRelation _right;
        private readonly ReadOnlyCollection<ValueSlot> _outputs;

        public BoundCombinedRelation(BoundQueryCombinator combinator, BoundRelation left, BoundRelation right, IList<ValueSlot> outputs)
        {
            _combinator = combinator;
            _left = left;
            _right = right;
            _outputs = new ReadOnlyCollection<ValueSlot>(outputs);
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.CombinedRelation; }
        }

        public BoundQueryCombinator Combinator
        {
            get { return _combinator; }
        }

        public BoundRelation Left
        {
            get { return _left; }
        }

        public BoundRelation Right
        {
            get { return _right; }
        }

        public ReadOnlyCollection<ValueSlot> Outputs
        {
            get { return _outputs; }
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return _outputs;
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return _outputs;
        }

        public BoundCombinedRelation Update(BoundQueryCombinator combinator, BoundRelation left, BoundRelation right, IList<ValueSlot> outputs)
        {
            if (combinator == _combinator && left == _left && right == _right && outputs == _outputs)
                return this;

            return new BoundCombinedRelation(combinator, left, right, outputs);
        }
    }
}