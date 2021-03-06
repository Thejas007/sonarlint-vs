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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarLint.Helpers
{
    public static class NavigationHelper
    {
        #region If

        public static IList<IfStatementSyntax> GetPrecedingIfsInConditionChain(this IfStatementSyntax ifStatement)
        {
            var ifList = new List<IfStatementSyntax>();
            var currentIf = ifStatement;

            while (currentIf.Parent is ElseClauseSyntax &&
                currentIf.Parent.Parent is IfStatementSyntax)
            {
                var precedingIf = (IfStatementSyntax) currentIf.Parent.Parent;
                ifList.Add(precedingIf);
                currentIf = precedingIf;
            }

            ifList.Reverse();
            return ifList;
        }

        public static IEnumerable<StatementSyntax> GetPrecedingStatementsInConditionChain(this IfStatementSyntax ifStatement)
        {
            return GetPrecedingIfsInConditionChain(ifStatement).Select(i => i.Statement);
        }

        public static IEnumerable<ExpressionSyntax> GetPrecedingConditionsInConditionChain(this IfStatementSyntax ifStatement)
        {
            return GetPrecedingIfsInConditionChain(ifStatement).Select(i => i.Condition);
        }

        #endregion

        #region Switch

        public static IEnumerable<SwitchSectionSyntax> GetPrecedingSections(this SwitchSectionSyntax caseStatement)
        {
            if (caseStatement == null)
            {
                return new SwitchSectionSyntax[0];
            }

            var switchStatement = (SwitchStatementSyntax)caseStatement.Parent;

            var currentSectionIndex = switchStatement.Sections.IndexOf(caseStatement);

            return switchStatement.Sections.Take(currentSectionIndex);
        }

        #endregion

        #region Statement

        public static StatementSyntax GetPrecedingStatement(this StatementSyntax currentStatement)
        {
            var statements = currentStatement.Parent.ChildNodes().OfType<StatementSyntax>().ToList();

            var index = statements.IndexOf(currentStatement);

            return index == 0 ? null : statements[index - 1];
        }

        #endregion
    }
}
