// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentFormatter.cs" company="Lynx Toolkit">
//   The MIT License (MIT)
//   
//   Copyright (c) 2012 Oystein Bjorke
//   
//   Permission is hereby granted, free of charge, to any person obtaining a
//   copy of this software and associated documentation files (the
//   "Software"), to deal in the Software without restriction, including
//   without limitation the rights to use, copy, modify, merge, publish,
//   distribute, sublicense, and/or sell copies of the Software, and to
//   permit persons to whom the Software is furnished to do so, subject to
//   the following conditions:
//   
//   The above copyright notice and this permission notice shall be included
//   in all copies or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//   OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//   MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//   IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//   CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//   TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//   SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LynxToolkit.Documents
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public abstract class DocumentFormatter<T> : IDocumentFormatter
    {
        protected DocumentFormatter()
        {
            this.StyleSheet = new StyleSheet();
        }

        public string SymbolDirectory { get; set; }

        protected Document Source { get; set; }

        protected StyleSheet StyleSheet { get; set; }

        public abstract void Format(Document doc, Stream stream);

        protected Style GetStyle(Header header)
        {
            if (this.StyleSheet == null)
            {
                return null;
            }

            return this.StyleSheet.HeaderStyles[header.Level - 1];
        }

        protected Style GetStyle(Paragraph p)
        {
            return this.StyleSheet.ParagraphStyle;
        }

        protected Style GetStyle(CodeBlock p)
        {
            return this.StyleSheet.CodeStyle;
        }

        protected Style GetStyle(InlineCode p)
        {
            return this.StyleSheet.InlineCodeStyle;
        }

        protected Style GetStyle(Quote p)
        {
            return this.StyleSheet.QuoteStyle;
        }

        protected Style GetStyle(List p)
        {
            return this.StyleSheet.UnorderedListStyle;
        }

        protected Style GetStyle(OrderedList p)
        {
            return this.StyleSheet.OrderedListStyle;
        }

        protected Style GetStyle(Table p)
        {
            return this.StyleSheet.TableStyle;
        }

        protected Style GetStyle(Hyperlink p)
        {
            return this.StyleSheet.HyperlinkStyle;
        }

        protected Style GetStyle(Image p)
        {
            return this.StyleSheet.ImageStyle;
        }

        protected abstract void Write(Header header, T context);

        protected abstract void Write(TableOfContents toc, T context);

        protected abstract void Write(Paragraph paragraph, T context);

        protected abstract void Write(UnorderedList list, T context);

        protected abstract void Write(OrderedList list, T context);

        protected abstract void Write(Quote quote, T context);

        protected abstract void Write(Section section, T context);

        protected abstract void Write(CodeBlock codeBlock, T context);

        protected abstract void Write(HorizontalRuler ruler, T context);

        protected abstract void Write(NonBreakingSpace nbsp, T context);

        protected abstract void Write(Run run, T context);

        protected abstract void Write(Span span, T context);

        protected abstract void Write(Strong strong, T context);

        protected abstract void Write(Emphasized em, T context);

        protected abstract void Write(LineBreak linebreak, T context);

        protected abstract void Write(InlineCode inlineCode, T context);

        protected abstract void Write(Hyperlink hyperlink, T context);

        protected abstract void Write(Image image, T context);

        protected abstract void Write(Equation equation, T context);

        protected abstract void Write(Symbol symbol, T context);

        protected abstract void Write(Anchor anchor, T context);

        protected virtual void Write(Table table, T context)
        {
            foreach (var row in table.Rows)
            {
                this.Write(row, context);
            }
        }

        protected virtual void Write(TableRow row, T context)
        {
            foreach (var cell in row.Cells)
            {
                this.Write(cell, context);
            }
        }

        protected virtual void Write(TableCell cell, T context)
        {
            this.WriteBlocks(cell.Blocks, context);
        }

        protected virtual void Write(ListItem item, T context)
        {
            this.WriteInlines(item.Content, context);
        }

        protected virtual void WriteBlock(Block block, T context)
        {
            if (this.WriteBlock<Header>(block, context, this.Write))
            {
                return;
            }

            if (this.WriteBlock<TableOfContents>(block, context, this.Write))
            {
                return;
            }

            if (this.WriteBlock<Quote>(block, context, this.Write))
            {
                return;
            }

            if (this.WriteBlock<Paragraph>(block, context, this.Write))
            {
                return;
            }

            if (this.WriteBlock<Section>(block, context, this.Write))
            {
                return;
            }

            if (this.WriteBlock<HorizontalRuler>(block, context, this.Write))
            {
                return;
            }

            if (this.WriteBlock<OrderedList>(block, context, this.Write))
            {
                return;
            }

            if (this.WriteBlock<UnorderedList>(block, context, this.Write))
            {
                return;
            }

            if (this.WriteBlock<Table>(block, context, this.Write))
            {
                return;
            }

            this.WriteBlock<CodeBlock>(block, context, this.Write);
        }

        protected bool WriteBlock<TBlock>(Block block, T context, Action<TBlock, T> writer) where TBlock : Block
        {
            var i = block as TBlock;
            if (i == null)
            {
                return false;
            }

            writer(i, context);
            return true;
        }

        protected virtual void WriteBlocks(IList<Block> blocks, T context)
        {
            foreach (var block in blocks)
            {
                this.WriteBlock(block, context);
            }
        }

        protected virtual void WriteInline(Inline inline, T context)
        {
            if (this.WriteInline<Span>(inline, context, this.Write))
            {
                return;
            }

            if (this.WriteInline<Strong>(inline, context, this.Write))
            {
                return;
            }

            if (this.WriteInline<Emphasized>(inline, context, this.Write))
            {
                return;
            }

            if (this.WriteInline<Symbol>(inline, context, this.Write))
            {
                return;
            }

            if (this.WriteInline<Run>(inline, context, this.Write))
            {
                return;
            }

            if (this.WriteInline<NonBreakingSpace>(inline, context, this.Write))
            {
                return;
            }

            if (this.WriteInline<LineBreak>(inline, context, this.Write))
            {
                return;
            }

            if (this.WriteInline<InlineCode>(inline, context, this.Write))
            {
                return;
            }

            if (this.WriteInline<Hyperlink>(inline, context, this.Write))
            {
                return;
            }

            if (this.WriteInline<Anchor>(inline, context, this.Write))
            {
                return;
            }

            if (this.WriteInline<Equation>(inline, context, this.Write))
            {
                return;
            }

            this.WriteInline<Image>(inline, context, this.Write);
        }

        protected bool WriteInline<TInline>(Inline inline, T context, Action<TInline, T> writer) where TInline : Inline
        {
            var i = inline as TInline;
            if (i == null)
            {
                return false;
            }

            writer(i, context);
            return true;
        }

        protected virtual void WriteInlines(IList<Inline> inlines, T context)
        {
            foreach (var inline in inlines)
            {
                this.WriteInline(inline, context);
            }
        }

        protected string ResolveSymbolPath(Symbol symbol)
        {
            var file = SymbolResolver.Decode(symbol.Name);
            var dir = this.SymbolDirectory ?? string.Empty;
            if (dir.Length > 0 && !dir.EndsWith("/"))
            {
                dir += "/";
            }

            var path = dir + file;
            return path;
        }
    }
}