using System;
using System.Collections.Generic;

namespace LynxToolkit.Documents
{
    public abstract class DocumentFormatter
    {
        protected Document doc;
        protected StyleSheet StyleSheet { get { return doc.StyleSheet; } }

        public DocumentFormatter(Document doc)
        {
            this.doc = doc;
        }

        protected Style GetStyle(Header header)
        {
            switch (header.Level)
            {
                case 1:
                    return this.StyleSheet.Header1Style;
                case 2:
                    return this.StyleSheet.Header2Style;
                case 3:
                    return this.StyleSheet.Header3Style;
                case 4:
                    return this.StyleSheet.Header4Style;
                case 5:
                    return this.StyleSheet.Header5Style;
            }
            return null;
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

        public virtual void FormatCore()
        {
            WriteBlocks(doc.Blocks);
        }

        protected virtual void WriteBlocks(IList<Block> blocks)
        {
            foreach (var block in blocks)
            {
                WriteBlock(block);
            }
        }

        protected virtual void WriteBlock(Block block)
        {
            if (WriteBlock<Header>(block, Write)) return;
            if (WriteBlock<Quote>(block, Write)) return;
            if (WriteBlock<Paragraph>(block, Write)) return;
            if (WriteBlock<HorizontalRuler>(block, Write)) return;
            if (WriteBlock<OrderedList>(block, Write)) return;
            if (WriteBlock<List>(block, Write)) return;
            if (WriteBlock<Table>(block, Write)) return;
            WriteBlock<CodeBlock>(block, Write);
        }

        protected virtual void WriteInlines(IList<Inline> inlines)
        {
            foreach (var inline in inlines) WriteInline(inline);
        }

        protected virtual void WriteInline(Inline inline)
        {
            if (WriteInline<Strong>(inline, Write)) return;
            if (WriteInline<Emphasized>(inline, Write)) return;
            if (WriteInline<Symbol>(inline, Write)) return;
            if (WriteInline<Run>(inline, Write)) return;
            if (WriteInline<LineBreak>(inline, Write)) return;
            if (WriteInline<InlineCode>(inline, Write)) return;
            if (WriteInline<Hyperlink>(inline, Write)) return;
            if (WriteInline<Anchor>(inline, Write)) return;
            WriteInline<Image>(inline, Write);
        }

        protected bool WriteInline<T>(Inline inline, Action<T> writer) where T : Inline
        {
            var i = inline as T;
            if (i == null) return false;
            writer(i);
            return true;
        }

        protected bool WriteBlock<T>(Block block, Action<T> writer) where T : Block
        {
            var i = block as T;
            if (i == null) return false;
            writer(i);
            return true;
        }

        protected abstract void Write(Header header);

        protected abstract void Write(Paragraph paragraph);

        protected abstract void Write(List list);

        protected abstract void Write(OrderedList list);

        protected abstract void Write(Quote quote);

        protected abstract void Write(CodeBlock codeBlock);

        protected abstract void Write(HorizontalRuler ruler);

        protected abstract void Write(Run run);

        protected abstract void Write(Strong strong);

        protected abstract void Write(Emphasized em);

        protected abstract void Write(LineBreak linebreak);

        protected abstract void Write(InlineCode inlineCode);

        protected abstract void Write(Hyperlink hyperlink);

        protected abstract void Write(Image image);

        protected abstract void Write(Symbol symbol);

        protected abstract void Write(Anchor anchor);

        protected virtual void Write(Table table)
        {
            foreach (var row in table.Rows) Write(row);
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
            WriteInlines(cell.Content);
        }

        protected virtual void Write(ListItem item)
        {
            WriteInlines(item.Content);
        }
    }
}