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

    public class DocumentFormatterOptions
    {
        public string SymbolDirectory { get; set; }
    }

    public abstract class DocumentFormatter
    {
        protected Document doc;

        protected DocumentFormatter(Document doc)
        {
            this.doc = doc;
        }

        protected StyleSheet StyleSheet
        {
            get
            {
                return this.doc.StyleSheet;
            }
        }

        public string SymbolDirectory { get; set; }

        public virtual void Format()
        {
            this.WriteBlocks(this.doc.Blocks);
        }

        protected Style GetStyle(Header header)
        {
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

        protected abstract void Write(Header header);

        protected abstract void Write(TableOfContents toc);

        protected abstract void Write(Paragraph paragraph);

        protected abstract void Write(UnorderedList list);

        protected abstract void Write(OrderedList list);

        protected abstract void Write(Quote quote);

        protected abstract void Write(CodeBlock codeBlock);

        protected abstract void Write(HorizontalRuler ruler);

        protected abstract void Write(NonBreakingSpace nbsp, object parent);

        protected abstract void Write(Run run, object parent);

        protected abstract void Write(Strong strong, object parent);

        protected abstract void Write(Emphasized em, object parent);

        protected abstract void Write(LineBreak linebreak, object parent);

        protected abstract void Write(InlineCode inlineCode, object parent);

        protected abstract void Write(Hyperlink hyperlink, object parent);

        protected abstract void Write(Image image, object parent);

        protected abstract void Write(Symbol symbol, object parent);

        protected abstract void Write(Anchor anchor, object parent);

        protected virtual void Write(Table table)
        {
            foreach (var row in table.Rows)
            {
                Write(row);
            }
        }

        protected virtual void Write(TableRow row)
        {
            foreach (var cell in row.Cells)
            {
                Write(cell);
            }
        }

        protected virtual void Write(TableCell cell)
        {
            this.WriteInlines(cell.Content);
        }

        protected virtual void Write(ListItem item)
        {
            this.WriteInlines(item.Content);
        }

        protected virtual void WriteBlock(Block block)
        {
            if (this.WriteBlock<Header>(block, this.Write))
            {
                return;
            }

            if (this.WriteBlock<TableOfContents>(block, this.Write))
            {
                return;
            }

            if (this.WriteBlock<Quote>(block, this.Write))
            {
                return;
            }

            if (this.WriteBlock<Paragraph>(block, this.Write))
            {
                return;
            }

            if (this.WriteBlock<HorizontalRuler>(block, this.Write))
            {
                return;
            }

            if (this.WriteBlock<OrderedList>(block, this.Write))
            {
                return;
            }

            if (this.WriteBlock<UnorderedList>(block, this.Write))
            {
                return;
            }

            if (this.WriteBlock<Table>(block, this.Write))
            {
                return;
            }

            this.WriteBlock<CodeBlock>(block, this.Write);
        }

        protected bool WriteBlock<T>(Block block, Action<T> writer) where T : Block
        {
            var i = block as T;
            if (i == null)
            {
                return false;
            }

            writer(i);
            return true;
        }

        protected virtual void WriteBlocks(IList<Block> blocks)
        {
            foreach (var block in blocks)
            {
                this.WriteBlock(block);
            }
        }

        protected virtual void WriteInline(Inline inline, object parent)
        {
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

        protected virtual void WriteInlines(IList<Inline> inlines, object parent = null)
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