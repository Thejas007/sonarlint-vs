﻿/*
 * SonarLint for Visual Studio
 * Copyright (C) 2015 SonarSource
 * sonarqube@googlegroups.com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace SonarLint.Helpers
{
    public static class ExpressionNumericConverter
    {
        private static bool TryConvertWith<T>(object o, Func<object, T> converter, out T value)
            where T : struct
        {
            try
            {
                value = converter(o);
                return true;
            }
            catch (Exception exception)
            {
                if (exception is FormatException ||
                    exception is OverflowException ||
                    exception is InvalidCastException)
                {
                    value = default(T);
                    return false;
                }

                throw;
            }
        }

        private static bool TryGetConstantValue<T>(ExpressionSyntax expression,
            Func<object, T> converter, Func<int, T, T> multiplierCalculator, out T value)
            where T : struct
        {
            ExpressionSyntax internalExpression;
            var multiplier = GetMultiplier(expression, out internalExpression);
            if (multiplier == null)
            {
                value = default(T);
                return false;
            }

            var literalExpression = internalExpression as LiteralExpressionSyntax;
            if (literalExpression != null &&
                TryConvertWith(literalExpression.Token.Value, converter, out value))
            {
                value = multiplierCalculator(multiplier.Value, value);
                return true;
            }

            value = default(T);
            return false;
        }

        private static int? GetMultiplier(ExpressionSyntax expression, out ExpressionSyntax internalExpression)
        {
            var multiplier = 1;
            internalExpression = expression;
            var unary = internalExpression as PrefixUnaryExpressionSyntax;
            while (unary != null)
            {
                var op = unary.OperatorToken;

                if (!SupportedOperatorTokens.Contains(op.Kind()))
                {
                    return null;
                }

                if (op.IsKind(SyntaxKind.MinusToken))
                {
                    multiplier *= -1;
                }
                internalExpression = unary.Operand;
                unary = internalExpression as PrefixUnaryExpressionSyntax;
            }

            return multiplier;
        }

        private static readonly SyntaxKind[] SupportedOperatorTokens =
        {
            SyntaxKind.MinusToken,
            SyntaxKind.PlusToken
        };

        public static bool TryGetConstantIntValue(ExpressionSyntax expression, out int value)
        {
            return TryGetConstantValue(
                expression,
                Convert.ToInt32,
                (multiplier, v) => multiplier * v,
                out value);
        }
        public static bool TryGetConstantDoubleValue(ExpressionSyntax expression, out double value)
        {
            return TryGetConstantValue(
                expression,
                Convert.ToDouble,
                (multiplier, v) => multiplier * v,
                out value);
        }
    }
}
