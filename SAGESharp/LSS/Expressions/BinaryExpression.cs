﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAGESharp.LSS.Expressions
{
    public class BinaryExpression : Expression
    {
        public override SourceSpan Span => Left.Span + Right.Span;
        public Expression Left { get; }
        public Token Operation { get; }
        public Expression Right { get; }

        public BinaryExpression(Expression left, Token operation, Expression right)
        {
            this.Left = left;
            this.Operation = operation;
            this.Right = right;
        }

        public override T AcceptVisitor<T, C>(ExpressionVisitor<T, C> visitor, C context)
        {
            return visitor.VisitBinaryExpression(this, context);
        }

        public override Expression Duplicate()
        {
            return new BinaryExpression(Left.Duplicate(), Operation, Right.Duplicate()); // Operation is immutable
        }

        public override string ToString()
        {
            if (Operation.Type == TokenType.Period
                || Operation.Type == TokenType.PeriodDollarSign
                || Operation.Type == TokenType.ColonColon
                || Operation.Type == TokenType.ColonColonDollarSign)
            {
                return Left.ToString() + Operation.Content + Right.ToString();
            }
            else
            {
                return Left.ToString() + " " + Operation.Content + " " + Right.ToString();
            }
        }
    }
}
