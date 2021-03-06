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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarLint.Common;
using SonarLint.Common.Sqale;
using SonarLint.Helpers;

namespace SonarLint.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [SqaleConstantRemediation("5min")]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.InstructionReliability)]
    [Rule(DiagnosticId, RuleSeverity, Title, IsActivatedByDefault)]
    [Tags(Tag.Bug)]
    public class TernaryOperatorPointless : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2758";
        internal const string Title = "The ternary operator should not return the same value regardless of the condition";
        internal const string Description =
            "When the second and third operands of a ternary operator are the same, the operator will always " +
            "return the same value regardless of the condition. Either the operator itself is pointless, or a " +
            "mistake was made in coding it.";
        internal const string MessageFormat =
            "This operation returns the same value whether the condition is \"true\" or \"false\".";
        internal const string Category = Constants.SonarLint;
        internal const Severity RuleSeverity = Severity.Critical;
        internal const bool IsActivatedByDefault = true;

        internal static readonly DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
                RuleSeverity.ToDiagnosticSeverity(), IsActivatedByDefault,
                helpLinkUri: DiagnosticId.GetHelpLink(),
                description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var expression = (ConditionalExpressionSyntax) c.Node;

                    if (EquivalenceChecker.AreEquivalent(
                        RemoveParentheses(expression.WhenTrue),
                        RemoveParentheses(expression.WhenFalse)))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, expression.GetLocation()));
                    }
                },
                SyntaxKind.ConditionalExpression);
        }

        internal static ExpressionSyntax RemoveParentheses(ExpressionSyntax expression)
        {
            var currentExpression = expression;
            var parentheses = expression as ParenthesizedExpressionSyntax;
            while(parentheses != null)
            {
                currentExpression = parentheses.Expression;
                parentheses = currentExpression as ParenthesizedExpressionSyntax;
            }
            return currentExpression;
        }
    }
}
