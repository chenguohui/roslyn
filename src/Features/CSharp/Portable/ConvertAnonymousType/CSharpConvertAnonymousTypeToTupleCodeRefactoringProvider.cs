﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.ConvertAnonymousType;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace Microsoft.CodeAnalysis.CSharp.ConvertAnonymousType;

using static CSharpSyntaxTokens;
using static SyntaxFactory;

[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = PredefinedCodeRefactoringProviderNames.ConvertAnonymousTypeToTuple), Shared]
[method: ImportingConstructor]
[method: SuppressMessage("RoslynDiagnosticsReliability", "RS0033:Importing constructor should be [Obsolete]", Justification = "Used in test code: https://github.com/dotnet/roslyn/issues/42814")]
internal sealed class CSharpConvertAnonymousTypeToTupleCodeRefactoringProvider()
    : AbstractConvertAnonymousTypeToTupleCodeRefactoringProvider<
        ExpressionSyntax,
        TupleExpressionSyntax,
        AnonymousObjectCreationExpressionSyntax>
{
    protected override int GetInitializerCount(AnonymousObjectCreationExpressionSyntax anonymousType)
        => anonymousType.Initializers.Count;

    protected override TupleExpressionSyntax ConvertToTuple(AnonymousObjectCreationExpressionSyntax anonCreation)
        => TupleExpression(
            OpenParenToken.WithTriviaFrom(anonCreation.OpenBraceToken),
            ConvertInitializers(anonCreation.Initializers),
            CloseParenToken.WithTriviaFrom(anonCreation.CloseBraceToken))
                .WithPrependedLeadingTrivia(anonCreation.GetLeadingTrivia());

    private static SeparatedSyntaxList<ArgumentSyntax> ConvertInitializers(SeparatedSyntaxList<AnonymousObjectMemberDeclaratorSyntax> initializers)
        => SeparatedList(initializers.Select(ConvertInitializer), GetSeparators(initializers));

    private static IEnumerable<SyntaxToken> GetSeparators(SeparatedSyntaxList<AnonymousObjectMemberDeclaratorSyntax> initializers)
        => initializers.Count == 0 ? [] : initializers.GetSeparators().Take(initializers.Count - 1);

    private static ArgumentSyntax ConvertInitializer(AnonymousObjectMemberDeclaratorSyntax declarator)
        => Argument(ConvertName(declarator.NameEquals), refKindKeyword: default, declarator.Expression).WithTriviaFrom(declarator);

    private static NameColonSyntax? ConvertName(NameEqualsSyntax? nameEquals)
        => nameEquals == null
            ? null
            : NameColon(
                nameEquals.Name,
                ColonToken.WithTriviaFrom(nameEquals.EqualsToken));
}
