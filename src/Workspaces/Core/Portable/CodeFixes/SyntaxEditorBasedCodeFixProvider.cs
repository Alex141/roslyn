﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editing;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CodeFixes
{
    internal abstract partial class SyntaxEditorBasedCodeFixProvider : CodeFixProvider
    {
        public sealed override FixAllProvider GetFixAllProvider()
            => new SyntaxEditorBasedFixAllProvider(this);

        protected Task<Document> FixAsync(
            Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            return FixAllAsync(document, ImmutableArray.Create(diagnostic), cancellationToken);
        }

        private async Task<Document> FixAllAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var editor = new SyntaxEditor(root, document.Project.Solution.Workspace);

            await FixAllAsync(document, diagnostics, editor, cancellationToken).ConfigureAwait(false);

            var newRoot = editor.GetChangedRoot();
            return document.WithSyntaxRoot(newRoot);
        }

        protected abstract Task FixAllAsync(
            Document document, ImmutableArray<Diagnostic> diagnostics, SyntaxEditor editor, CancellationToken cancellationToken);

        /// <summary>
        /// Whether or not this diagnostic should be included when performing a FixAll.  This is useful
        /// for providers that create multiple diagnostics for the same issue (For example, one main 
        /// diagnostic and multiple 'faded out code' diagnostics).  FixAll can be invoked from any of 
        /// those, but we'll only want perform an edit for only one diagnostic for each of those sets
        /// of diagnostics.
        /// </summary>
        protected virtual bool IncludeDiagnosticDuringFixAll(Diagnostic diagnostic) => true;
    }
}