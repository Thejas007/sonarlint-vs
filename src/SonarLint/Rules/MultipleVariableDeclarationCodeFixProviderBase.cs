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
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.CodeAnalysis.CodeActions;
using SonarLint.Common;

namespace SonarLint.Rules.Common
{
    public abstract class MultipleVariableDeclarationCodeFixProviderBase : CodeFixProvider
    {
        public const string Title = "Separate declarations";
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(MultipleVariableDeclarationBase.DiagnosticId);
            }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return DocumentBasedFixAllProvider.Instance;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c =>
                    {
                        var newRoot = CalculateNewRoot(root, node);
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    },
                    Title),
                context.Diagnostics);
        }

        protected abstract SyntaxNode CalculateNewRoot(SyntaxNode root, SyntaxNode node);
    }
}
