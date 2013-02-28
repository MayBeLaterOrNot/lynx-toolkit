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

    public abstract class DocumentFormatter
    {
        protected Document source;

        public DocumentFormatter()
        {
            this.StyleSheet = new StyleSheet();
        }

        protected StyleSheet StyleSheet { get; set; }

        public string SymbolDirectory { get; set; }

        public abstract bool Format(Document doc, string outputFile);

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

        protected abstract void Write(Header header, object parent);

        protected abstract void Write(TableOfContents toc, object parent);

        protected abstract void Write(Paragraph paragraph, object parent);

        protected abstract void Write(UnorderedList list, object parent);

        protected abstract void Write(OrderedList list, object parent);

        protected abstract void Write(Quote quote, object parent);

        protected abstract void Write(Section section, object parent);

        protected abstract void Write(CodeBlock codeBlock, object parent);

        protected abstract void Write(HorizontalRuler ruler, object parent);

        protected abstract void Write(NonBreakingSpace nbsp, object parent);

        protected abstract void Write(Run run, object parent);

        protected abstract void Write(Span span, object parent);

        protected abstract void Write(Strong strong, object parent);

        protected abstract void Write(Emphasized em, object parent);

        protected abstract void Write(LineBreak linebreak, object parent);

        protected abstract void Write(InlineCode inlineCode, object parent);

        protected abstract void Write(Hyperlink hyperlink, object parent);

        protected abstract void Write(Image image, object parent);

        protected abstract void Write(Equation equation, object parent);

        protected abstract void Write(Symbol symbol, object parent);

        protected abstract void Write(Anchor anchor, object parent);

        protected virtual void Write(Table table, object parent)
        {
            foreach (var row in table.Rows)
            {
                this.Write(row, parent);
            }
        }

        protected virtual void Write(TableRow row, object parent)
        {
            foreach (var cell in row.Cells)
            {
                this.Write(cell, parent);
            }
        }

        protected virtual void Write(TableCell cell, object parent)
        {
            this.WriteBlocks(cell.Blocks, parent);
        }

        protected virtual void Write(ListItem item, object parent)
        {
            this.WriteInlines(item.Content, parent);
        }

        protected virtual void WriteBlock(Block block, object parent)
        {
            if (this.WriteBlock<Header>(block, parent, this.Write))
            {
                return;
            }

            if (this.WriteBlock<TableOfContents>(block, parent, this.Write))
            {
                return;
            }

            if (this.WriteBlock<Quote>(block, parent, this.Write))
            {
                return;
            }

            if (this.WriteBlock<Paragraph>(block, parent, this.Write))
            {
                return;
            }

            if (this.WriteBlock<Section>(block, parent, this.Write))
            {
                return;
            }

            if (this.WriteBlock<HorizontalRuler>(block, parent, this.Write))
            {
                return;
            }

            if (this.WriteBlock<OrderedList>(block, parent, this.Write))
            {
                return;
            }

            if (this.WriteBlock<UnorderedList>(block, parent, this.Write))
            {
                return;
            }

            if (this.WriteBlock<Table>(block, parent, this.Write))
            {
                return;
            }

            this.WriteBlock<CodeBlock>(block, parent, this.Write);
        }

        protected bool WriteBlock<T>(Block block, object parent, Action<T, object> writer) where T : Block
        {
            var i = block as T;
            if (i == null)
            {
                return false;
            }

            writer(i, parent);
            return true;
        }

        protected virtual void WriteBlocks(IList<Block> blocks, object parent)
        {
            foreach (var block in blocks)
            {
                this.WriteBlock(block, parent);
            }
        }

        protected virtual void WriteInline(Inline inline, object parent)
        {
            if (this.WriteInline<Span>(inline, parent, this.Write))
            {
                return;
            }

            if (this.WriteInline<Strong>(inline, parent, this.Write))
            {
                return;
            }

            if (this.WriteInline<Emphasized>(inline, parent, this.Write))
            {
                return;
            }

            if (this.WriteInline<Symbol>(inline, parent, this.Write))
            {
                return;
            }

            if (this.WriteInline<Run>(inline, parent, this.Write))
            {
                return;
            }

            if (this.WriteInline<NonBreakingSpace>(inline, parent, this.Write))
            {
                return;
            }

            if (this.WriteInline<LineBreak>(inline, parent, this.Write))
            {
                return;
            }

            if (this.WriteInline<InlineCode>(inline, parent, this.Write))
            {
                return;
            }

            if (this.WriteInline<Hyperlink>(inline, parent, this.Write))
            {
                return;
            }

            if (this.WriteInline<Anchor>(inline, parent, this.Write))
            {
                return;
            }

            if (this.WriteInline<Equation>(inline, parent, this.Write))
            {
                return;
            }

            this.WriteInline<Image>(inline, parent, this.Write);
        }

        protected bool WriteInline<Ti>(Inline inline, object parent, Action<Ti, object> writer) where Ti : Inline
        {
            var i = inline as Ti;
            if (i == null)
            {
                return false;
            }

            writer(i, parent);
            return true;
        }

        protected virtual void WriteInlines(IList<Inline> inlines, object parent)
        {
            foreach (var inline in inlines)
            {
                this.WriteInline(inline, parent);
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