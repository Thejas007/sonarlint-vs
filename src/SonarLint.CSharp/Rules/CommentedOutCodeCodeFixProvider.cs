/*
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
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Generic;

namespace SonarLint.Rules
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class CommentedOutCodeCodeFixProvider : CodeFixProvider
    {
        internal const string Title = "Remove \"commented\" code";
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(BooleanCheckInverted.DiagnosticId);
            }
        }
        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override sealed async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            //  var syntaxNode = root.FindNode(diagnosticSpan);
            //var declaration = root.FindToken(diagnosticSpan.Start);
            // var firstToken = declaration.GetFirstToken();
            // var leadingTrivia = firstToken.LeadingTrivia;
            // var trimmedLocal = declaration.ReplaceToken(firstToken, firstToken.WithLeadingTrivia(SyntaxTriviaList.Empty));
            var cw = new AdhocWorkspace();
            OptionSet options = cw.Options;
            options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInMethods, false);
            options = options.WithChangedOption(CSharpFormattingOptions.IndentBlock, false);
            context.RegisterCodeFix(
     CodeAction.Create(
         Title,
        async c =>
         {
             var oldNode = root.FindTrivia(diagnosticSpan.Start);
             var newRoot = root.ReplaceTrivia(oldNode, SyntaxTriviaList.Empty);
             newRoot = newRoot.WithAdditionalAnnotations(Formatter.Annotation);
             newRoot = Formatter.Format(newRoot, new AdhocWorkspace(),options);
             //TODO:Need to remove empty line if it followed by a comment(Formatting).
             return await Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
         }),
     context.Diagnostics);
        }


    }
}

