﻿/*
 * SonarQube C# Code Analysis
 * Copyright (C) 2015 SonarSource
 * dev@sonar.codehaus.org
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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarQube.CSharp.CodeAnalysis.Helpers;
using SonarQube.CSharp.CodeAnalysis.Common;
using SonarQube.CSharp.CodeAnalysis.Common.Sqale;

namespace SonarQube.CSharp.CodeAnalysis.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [SqaleConstantRemediation("2min")]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.Readability)]
    [Rule(DiagnosticId, RuleSeverity, Title, IsActivatedByDefault)]
    [Tags("clumsy")]
    public class EmptinessChecking : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1155";
        internal const string Title = "\"Any()\" should be used to test for emptiness";
        internal const string Description =
            "Using \".Count() > 0\" to test for emptiness works, but using \".Any()\" makes the " +
            "intent clearer, and the code more readable.";
        internal const string MessageFormat = "Use \".Any()\" to test whether this \"IEnumerable<{0}>\" is empty or not.";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Major;
        internal const bool IsActivatedByDefault = true;

        internal static readonly DiagnosticDescriptor Rule = 
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, 
                RuleSeverity.ToDiagnosticSeverity(), IsActivatedByDefault,
                helpLinkUri: DiagnosticId.GetHelpLink(),
                description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        private static readonly LiteralExpressionSyntax LiteralOneSyntax =
            SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1));
        private static readonly LiteralExpressionSyntax LiteralZeroSyntax =
            SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0));

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var binary = (BinaryExpressionSyntax) c.Node;
                    CheckCountZero(binary.Right, binary.Left, c);
                    CheckCountOne(binary.Left, binary.Right, c);
                },
                SyntaxKind.GreaterThanExpression);
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var binary = (BinaryExpressionSyntax)c.Node;
                    CheckCountZero(binary.Left, binary.Right, c);
                    CheckCountOne(binary.Right, binary.Left, c);
                },
                SyntaxKind.LessThanExpression);
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var binary = (BinaryExpressionSyntax)c.Node;
                    CheckCountOne(binary.Right, binary.Left, c);
                    CheckCountZero(binary.Left, binary.Right, c);
                },
                SyntaxKind.GreaterThanOrEqualExpression);
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var binary = (BinaryExpressionSyntax)c.Node;
                    CheckCountOne(binary.Left, binary.Right, c);
                    CheckCountZero(binary.Right, binary.Left, c);
                },
                SyntaxKind.LessThanOrEqualExpression);
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var binary = (BinaryExpressionSyntax)c.Node;
                    CheckCountZero(binary.Left, binary.Right, c);
                    CheckCountZero(binary.Right, binary.Left, c);
                },
                SyntaxKind.EqualsExpression);
        }

        private static void CheckCountZero(ExpressionSyntax zero, ExpressionSyntax count, SyntaxNodeAnalysisContext c)
        {
            Location reportLocation;
            string typeArgument;

            if (EquivalenceChecker.AreEquivalent(zero, LiteralZeroSyntax) &&
                TryGetCountCall(count, c.SemanticModel, out reportLocation, out typeArgument))
            {
                c.ReportDiagnostic(Diagnostic.Create(Rule, reportLocation, typeArgument));
            }
        }
        private static void CheckCountOne(ExpressionSyntax one, ExpressionSyntax count, SyntaxNodeAnalysisContext c)
        {
            Location reportLocation;
            string typeArgument;

            if (EquivalenceChecker.AreEquivalent(one, LiteralOneSyntax) &&
                TryGetCountCall(count, c.SemanticModel, out reportLocation, out typeArgument))
            {
                c.ReportDiagnostic(Diagnostic.Create(Rule, reportLocation, typeArgument));
            }
        }

        private static bool TryGetCountCall(ExpressionSyntax expression, SemanticModel semanticModel, out Location countLocation, out string typeArgument)
        {
            countLocation = null;
            typeArgument = null;
            var invocation = expression as InvocationExpressionSyntax;
            if (invocation == null)
            {
                return false;
            }

            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null)
            {
                return false;
            }

            var methodSymbol = semanticModel.GetSymbolInfo(memberAccess).Symbol as IMethodSymbol;
            if (methodSymbol == null || 
                methodSymbol.Name != "Count" ||
                !methodSymbol.IsExtensionMethod ||
                methodSymbol.ReceiverType == null)
            {
                return false;
            }
            
            var enumerableType = semanticModel.Compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);
            var receiverType = methodSymbol.ReceiverType as INamedTypeSymbol;

            if (methodSymbol.MethodKind == MethodKind.Ordinary)
            {
                receiverType = methodSymbol.Parameters.First().Type as INamedTypeSymbol;
            }

            if (receiverType == null ||
                receiverType.ConstructedFrom != enumerableType)
            {
                return false;
            }

            if (methodSymbol.IsGenericMethod)
            {
                typeArgument = methodSymbol.TypeArguments.First().ToDisplayString();
            }

            countLocation = memberAccess.Name.GetLocation();
            return true;
        }
    }
}