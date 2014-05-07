namespace LynxToolkit.Documents.Tests
{
    using System;
    using System.IO;

    using NUnit.Framework;

    [TestFixture]
    public class WikiParserTests
    {
        [Test]
        public void ParseFile()
        {
            var o2 = new WikiParser(File.OpenRead);
            var model = o2.ParseFile(@"Input/Example.wiki");
        }

        [Test]
        public void Header1()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("= Header 1");
            Assert.AreEqual(1, model.Blocks.Count);
            var h1 = model.Blocks[0] as Header;
            var r1 = h1.Content[0] as Run;
            Assert.AreEqual("Header 1", r1.Text);
            Assert.AreEqual(1, h1.Level);
        }

        [Test]
        public void Header2()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("\r\n== Header 2");
            Assert.AreEqual(1, model.Blocks.Count);
            var h1 = model.Blocks[0] as Header;
            var r1 = h1.Content[0] as Run;
            Assert.AreEqual("Header 2", r1.Text);
            Assert.AreEqual(2, h1.Level);
        }

        [Test]
        public void Header3()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("=== Header 3\r\n");
            Assert.AreEqual(1, model.Blocks.Count);
            var h1 = model.Blocks[0] as Header;
            var r1 = h1.Content[0] as Run;
            Assert.AreEqual("Header 3", r1.Text);
            Assert.AreEqual(3, h1.Level);
        }

        [Test]
        public void Paragraph1()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("Paragraph 1");
            Assert.AreEqual(1, model.Blocks.Count);
            var p1 = model.Blocks[0] as Paragraph;
            Assert.AreEqual(1, p1.Content.Count);
            var r1 = p1.Content[0] as Run;
            Assert.AreEqual("Paragraph 1", r1.Text);
        }

        [Test]
        public void Paragraph2()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("\r\nParagraph 1");
            Assert.AreEqual(1, model.Blocks.Count);
            var p1 = model.Blocks[0] as Paragraph;
            Assert.AreEqual(1, p1.Content.Count);
            var r1 = p1.Content[0] as Run;
            Assert.AreEqual("Paragraph 1", r1.Text);
        }

        [Test]
        public void Paragraph3()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("Paragraph 1\r\n");
            Assert.AreEqual(1, model.Blocks.Count);
            var p1 = model.Blocks[0] as Paragraph;
            Assert.AreEqual(1, p1.Content.Count);
            var r1 = p1.Content[0] as Run;
            Assert.AreEqual("Paragraph 1", r1.Text);
        }

        [Test]
        public void Paragraph4()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("Line 1 \r\nLine 2");
            Assert.AreEqual(1, model.Blocks.Count);
            var p1 = model.Blocks[0] as Paragraph;
            Assert.AreEqual(2, p1.Content.Count);
            var r1 = p1.Content[0] as Run;
            Assert.AreEqual("Line 1 ", r1.Text);
            var r2 = p1.Content[1] as Run;
            Assert.AreEqual("Line 2", r2.Text);
        }

        [Test]
        public void Paragraph5()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("Paragraph 1\r\nLine 2\r\n\r\nParagraph 2");

            Assert.AreEqual(2, model.Blocks.Count);
            var p1 = model.Blocks[0] as Paragraph;
            Assert.AreEqual(2, p1.Content.Count);
            var r1 = p1.Content[0] as Run;
            var p12 = p1.Content[1] as Run;
            Assert.AreEqual("Paragraph 1", r1.Text);
            Assert.AreEqual("Line 2", p12.Text);

            var p2 = model.Blocks[1] as Paragraph;
            Assert.AreEqual(1, p2.Content.Count);
            var r2 = p2.Content[0] as Run;
            Assert.AreEqual("Paragraph 2", r2.Text);
        }

        [Test, Ignore]
        public void ParagraphFollowedByCode()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("Line 1 \r\n```\r\ncode\r\n```");
            Assert.AreEqual(2, model.Blocks.Count);
            var p1 = (Paragraph)model.Blocks[0];
            Assert.AreEqual(2, p1.Content.Count);
            var r1 = (Run)p1.Content[0];
            Assert.AreEqual("Line 1 ", r1.Text);

            var b2 = (CodeBlock)model.Blocks[1];
            Assert.AreEqual("code", b2.Text);
        }

        [Test]
        public void Code1()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("```xml\r\n<a></a>\r\n```");

            Assert.AreEqual(1, model.Blocks.Count);
            var p1 = model.Blocks[0] as CodeBlock;
            Assert.AreEqual("<a></a>", p1.Text);
            Assert.AreEqual(Language.Xml, p1.Language);
        }

        [Test]
        public void Code_Included()
        {
            var o2 = new WikiParser(File.OpenRead);
            var model = o2.Parse("```xml\r\n@include Input/Example.xml\r\n```");

            Assert.AreEqual(1, model.Blocks.Count);
            var p1 = model.Blocks[0] as CodeBlock;
            Assert.AreEqual("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<xml a=\"b\">\r\n  <c>d</c>\r\n</xml>", p1.Text);
            Assert.AreEqual(Language.Xml, p1.Language);
        }

        [Test]
        public void InlineCode1()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("Inline `co\\de` !");

            Assert.AreEqual(1, model.Blocks.Count);
            var p1 = model.Blocks[0] as Paragraph;
            var r1 = p1.Content[0] as Run;
            var r2 = p1.Content[1] as InlineCode;
            var r3 = p1.Content[2] as Run;
            Assert.AreEqual("Inline ", r1.Text);
            Assert.AreEqual("co\\de", r2.Code);
            Assert.AreEqual(" !", r3.Text);
        }

        [Test]
        public void Strong1()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("This is **strong** text");

            Assert.AreEqual(1, model.Blocks.Count);
            var p1 = model.Blocks[0] as Paragraph;
            Assert.AreEqual(3, p1.Content.Count);
            var r1 = p1.Content[0] as Run;
            var s1 = p1.Content[1] as Strong;
            var r2 = s1.Content[0] as Run;
            var r3 = p1.Content[2] as Run;
            Assert.AreEqual("This is ", r1.Text);
            Assert.AreEqual("strong", r2.Text);
            Assert.AreEqual(" text", r3.Text);
        }

        [Test]
        public void Strong2()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("This is **str~*ong** text");

            Assert.AreEqual(1, model.Blocks.Count);
            var p1 = model.Blocks[0] as Paragraph;
            Assert.AreEqual(3, p1.Content.Count);
            var r1 = p1.Content[0] as Run;
            var s1 = p1.Content[1] as Strong;
            var r2 = s1.Content[0] as Run;
            var r3 = p1.Content[2] as Run;
            Assert.AreEqual("This is ", r1.Text);
            Assert.AreEqual("str*ong", r2.Text);
            Assert.AreEqual(" text", r3.Text);
        }

        [Test]
        public void Emphasized1()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("This is //emphasized// text");

            Assert.AreEqual(1, model.Blocks.Count);
            var p1 = model.Blocks[0] as Paragraph;
            Assert.AreEqual(3, p1.Content.Count);
            var r1 = p1.Content[0] as Run;
            var s1 = p1.Content[1] as Emphasized;
            var r2 = s1.Content[0] as Run;
            var r3 = p1.Content[2] as Run;
            Assert.AreEqual("This is ", r1.Text);
            Assert.AreEqual("emphasized", r2.Text);
            Assert.AreEqual(" text", r3.Text);
        }

        [Test]
        public void Hyperlink1()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("[http://www.google.com] text");

            Assert.AreEqual(1, model.Blocks.Count);
            var p1 = model.Blocks[0] as Paragraph;
            AssertLink("http://www.google.com", "www.google.com", p1.Content[0]);
            AssertInline(" text", p1.Content[1]);
        }

        [Test]
        public void Hyperlink2()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("[link1|text] following text");

            Assert.AreEqual(1, model.Blocks.Count);
            var p1 = model.Blocks[0] as Paragraph;
            AssertLink("link1", "text", p1.Content[0]);
            AssertInline(" following text", p1.Content[1]);
        }

        [Test]
        public void Hyperlink3()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("[link1|text **bold~***] text");

            Assert.AreEqual(1, model.Blocks.Count);
            var p1 = model.Blocks[0] as Paragraph;
            Assert.AreEqual(2, p1.Content.Count);
            var r1 = p1.Content[0] as Hyperlink;
            var c1 = r1.Content[0] as Run;
            var c2 = r1.Content[1] as Strong;
            var c3 = c2.Content[0] as Run;
            var r2 = p1.Content[1] as Run;
            Assert.AreEqual("link1", r1.Url);
            Assert.AreEqual("text ", c1.Text);
            Assert.AreEqual("bold*", c3.Text);
            Assert.AreEqual(" text", r2.Text);
        }

        [Test]
        public void Image1()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("{image1.png} text");

            Assert.AreEqual(1, model.Blocks.Count);
            var p1 = model.Blocks[0] as Paragraph;
            Assert.AreEqual(2, p1.Content.Count);
            AssertImage("image1.png", null, p1.Content[0]);
            AssertInline(" text", p1.Content[1]);
        }

        [Test]
        public void Image2()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("{image1.png|alt1} text");

            Assert.AreEqual(1, model.Blocks.Count);
            var p1 = (Paragraph)model.Blocks[0];
            Assert.AreEqual(2, p1.Content.Count);
            AssertImage("image1.png", "alt1", p1.Content[0]);
            AssertInline(" text", p1.Content[1]);
        }

        [Test]
        public void Image3()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("[link1|{image1.png|alt1}] text");

            Assert.AreEqual(1, model.Blocks.Count);
            var p1 = model.Blocks[0] as Paragraph;
            Assert.AreEqual(2, p1.Content.Count);
            var r1 = p1.Content[0] as Hyperlink;
            var c1 = r1.Content[0] as Image;
            var r2 = p1.Content[1] as Run;
            Assert.AreEqual("link1", r1.Url);
            AssertImage("image1.png", "alt1", c1);
            Assert.AreEqual(" text", r2.Text);
        }

        [Test]
        public void Image4()
        {
            var o2 = new WikiParser { CurrentDirectory = "tmp" };
            var model = o2.Parse("{..\\image1.png|image 1} text");

            Assert.AreEqual(1, model.Blocks.Count);
            var p1 = (Paragraph)model.Blocks[0];
            Assert.AreEqual(2, p1.Content.Count);
            AssertImage("image1.png", "image 1", p1.Content[0]);
            AssertInline(" text", p1.Content[1]);
        }

        [Test]
        public void LineBreak1()
        {
            var o2 = new WikiParser();
            var model = o2.Parse(@"Line1\\Line2");

            Assert.AreEqual(1, model.Blocks.Count);
            var p1 = model.Blocks[0] as Paragraph;
            Assert.AreEqual(3, p1.Content.Count);
            AssertInline("Line1", p1.Content[0]);
            Assert.IsTrue(p1.Content[1] is LineBreak);
            AssertInline("Line2", p1.Content[2]);
        }


        [Test]
        public void HorizontalRuler()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("Line1\r\n\r\n---\r\n\r\nLine2");

            Assert.AreEqual(3, model.Blocks.Count);
            Assert.IsTrue(model.Blocks[1] is HorizontalRuler);
        }

        [Test]
        public void LineBreak()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("Line1  \r\nLine2");
            var p = (Paragraph)model.Blocks[0];
            Assert.AreEqual(3, p.Content.Count);
            Assert.IsTrue(p.Content[1] is LineBreak);
        }

        [Test]
        public void OrderedList()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("# Item 1\r\n# Item 2");

            Assert.AreEqual(1, model.Blocks.Count);
            var list = model.Blocks[0] as OrderedList;
            var i1 = list.Items[0].Content[0] as Run;
            var i2 = list.Items[1].Content[0] as Run;
            Assert.AreEqual("Item 1", i1.Text);
            Assert.AreEqual("Item 2", i2.Text);
        }

        [Test]
        public void UnorderedList()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("- Item 1\r\n- Item 2");

            Assert.AreEqual(1, model.Blocks.Count);
            var list = model.Blocks[0] as UnorderedList;
            var i1 = list.Items[0].Content[0] as Run;
            var i2 = list.Items[1].Content[0] as Run;
            Assert.AreEqual("Item 1", i1.Text);
            Assert.AreEqual("Item 2", i2.Text);
        }

        [Test]
        public void Table1()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("|Cell 1|");

            Assert.AreEqual(1, model.Blocks.Count);
            var table = model.Blocks[0] as Table;
            Assert.AreEqual(1, table.Rows.Count);
            Assert.AreEqual(1, table.Rows[0].Cells.Count);
            AssertCell("Cell 1", table.Rows[0].Cells[0]);
        }

        private void AssertCell(string cell, TableCell tableCell)
        {
            Assert.AreEqual(1, tableCell.Blocks.Count);
            var p = (Paragraph)tableCell.Blocks[0];
            Assert.AreEqual(1, p.Content.Count);
            var r = (Run)p.Content[0];
            Assert.AreEqual(cell, r.Text);
        }

        [Test]
        public void Table2()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("|Cell1|Cell2|\r\n|Cell3|Cell4|");

            Assert.AreEqual(1, model.Blocks.Count);
            var table = model.Blocks[0] as Table;
            Assert.AreEqual(2, table.Rows.Count);
            Assert.AreEqual(2, table.Rows[0].Cells.Count);
            Assert.AreEqual(2, table.Rows[1].Cells.Count);
            AssertCell("Cell1", table.Rows[0].Cells[0]);
            AssertCell("Cell2", table.Rows[0].Cells[1]);
            AssertCell("Cell3", table.Rows[1].Cells[0]);
            AssertCell("Cell4", table.Rows[1].Cells[1]);
        }

        public static void AssertBlock(string expected, Block b)
        {
            var p = (Paragraph)b;
            Assert.AreEqual(expected, p.Content[0].ToString());
        }

        public static void AssertImage(string expectedSource, string expectedAlt, Inline image)
        {
            var img = (Image)image;
            Assert.IsTrue(img.Source.Contains(expectedSource), "Image Source: Expected {0} in {1}", expectedSource, img.Source);
            Assert.AreEqual(expectedAlt, img.AlternateText, "Image alternate text");
        }

        public static void AssertLink(string expectedSource, string expectedText, Inline link)
        {
            Assert.AreEqual(expectedSource, ((Hyperlink)link).Url);
            if (expectedText == null)
            {
                Assert.AreEqual(0, ((Hyperlink)link).Content.Count);
            }
            else
            {
                AssertInline(expectedText, ((Hyperlink)link).Content[0]);
            }
        }

        public static void AssertInline(string expected, Inline b)
        {
            Assert.AreEqual(expected, b.ToString());
        }

        [Test]
        public void Table3()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("|Cell1\r\n|Cell2|\r\n|Cell3|Cell4|\r\ntext");

            Assert.AreEqual(2, model.Blocks.Count);
            AssertBlock("text", model.Blocks[1]);
            var table = (Table)model.Blocks[0];
            Assert.AreEqual(2, table.Rows.Count);
            Assert.AreEqual(2, table.Rows[0].Cells.Count);
            Assert.AreEqual(2, table.Rows[1].Cells.Count);
            AssertCell("Cell1", table.Rows[0].Cells[0]);
            AssertCell("Cell2", table.Rows[0].Cells[1]);
            AssertCell("Cell3", table.Rows[1].Cells[0]);
            AssertCell("Cell4", table.Rows[1].Cells[1]);
        }

        [Test]
        public void Table4()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("||Cell1||Cell2|\r\n|Cell3|Cell4|\r\ntext");

            Assert.AreEqual(2, model.Blocks.Count);
            AssertBlock("text", model.Blocks[1]);
            var table = (Table)model.Blocks[0];
            Assert.AreEqual(2, table.Rows.Count);
            Assert.AreEqual(2, table.Rows[0].Cells.Count);
            Assert.AreEqual(2, table.Rows[1].Cells.Count);
            Assert.IsTrue(table.Rows[0].Cells[0] is TableHeaderCell);
            Assert.IsTrue(table.Rows[0].Cells[1] is TableHeaderCell);
            AssertCell("Cell1", table.Rows[0].Cells[0]);
            AssertCell("Cell2", table.Rows[0].Cells[1]);
            AssertCell("Cell3", table.Rows[1].Cells[0]);
            AssertCell("Cell4", table.Rows[1].Cells[1]);
        }

        [Test]
        public void Table_ColumnSpan()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("|Cell1|^|\r\n|Cell3|Cell4|");

            Assert.AreEqual(1, model.Blocks.Count);
            var table = model.Blocks[0] as Table;
            Assert.AreEqual(2, table.Rows.Count);
            Assert.AreEqual(1, table.Rows[0].Cells.Count);
            Assert.AreEqual(2, table.Rows[1].Cells.Count);
            AssertCell("Cell1", table.Rows[0].Cells[0]);
            Assert.AreEqual(2, table.Rows[0].Cells[0].ColumnSpan);
            AssertCell("Cell3", table.Rows[1].Cells[0]);
            AssertCell("Cell4", table.Rows[1].Cells[1]);
        }

        [Test]
        public void Table_RowSpan()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("|Cell1|Cell2|\r\n|¨|Cell3|");

            Assert.AreEqual(1, model.Blocks.Count);
            var table = model.Blocks[0] as Table;
            Assert.AreEqual(2, table.Rows.Count);
            Assert.AreEqual(2, table.Rows[0].Cells.Count);
            Assert.AreEqual(1, table.Rows[1].Cells.Count);
            Assert.AreEqual(2, table.Rows[0].Cells[0].RowSpan);
            AssertCell("Cell1", table.Rows[0].Cells[0]);
            AssertCell("Cell2", table.Rows[0].Cells[1]);
            AssertCell("Cell3", table.Rows[1].Cells[0]);
        }

        [Test]
        public void Span1()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("Press {{key:F1}} for help.");

            Assert.AreEqual(1, model.Blocks.Count);
            var p = model.Blocks[0] as Paragraph;
            Assert.AreEqual(3, p.Content.Count);
            var r1 = p.Content[0] as Run;
            var s2 = p.Content[1] as Span;
            var r2 = s2.Content[0] as Run;
            var r3 = p.Content[2] as Run;
            Assert.AreEqual("Press ", r1.Text);
            Assert.AreEqual("key", s2.Class);
            Assert.AreEqual("F1", r2.Text);
            Assert.AreEqual(" for help.", r3.Text);
        }

        [Test]
        public void Div1()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("[[figure:{image1.png|caption}]]");

            Assert.AreEqual(1, model.Blocks.Count);
            var s = (Section)model.Blocks[0];
            Assert.AreEqual(1, s.Blocks.Count);
            var p = (Paragraph)s.Blocks[0];
            var i1 = (Image)p.Content[0];
            Assert.AreEqual("figure", s.Class);
            Assert.IsTrue(i1.Source.Contains("image1.png"));
            Assert.AreEqual("caption", i1.AlternateText);
        }

        [Test]
        public void Equation1()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("$$x^2$$");

            Assert.AreEqual(1, model.Blocks.Count);
            var p = (Paragraph)model.Blocks[0];
            var e1 = (Equation)p.Content[0];
            Assert.AreEqual("x^2", e1.Content);
        }

        [Test]
        public void Defines1()
        {
            var o2 = new WikiParser();
            o2.Defines.Add("DEBUG");
            var model = o2.Parse("@if DEBUG\r\ndebug\r\n@endif");

            Assert.AreEqual(1, model.Blocks.Count);
            AssertBlock("debug", model.Blocks[0]);
        }

        [Test]
        public void Defines2()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("@if DEBUG\r\ndebug\r\n@endif");

            Assert.AreEqual(0, model.Blocks.Count);
        }

        [Test]
        public void Variable1()
        {
            var o2 = new WikiParser();
            o2.Variables.Add("title", "Hello");
            var model = o2.Parse("'$title'");
            Assert.AreEqual(1, model.Blocks.Count);
            AssertBlock("'Hello'", model.Blocks[0]);
        }

        [Test]
        public void Variable2()
        {
            var o2 = new WikiParser();
            o2.Variables.Add("title", "Hello");
            var model = o2.Parse("@title $title");
            Assert.AreEqual("Hello", model.Title);
        }

        [Test]
        public void Variable3()
        {
            var o2 = new WikiParser();
            o2.Variables.Add("title", "Hello");
            var model = o2.Parse("[hello|$title]");
            var p = (Paragraph)model.Blocks[0];
            Assert.AreEqual(1, model.Blocks.Count);
            AssertLink("hello", "Hello", p.Content[0]);
        }

        [Test]
        public void Quote1()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("> line1\r\n> line2\r\nparagraph");
            var q = (Quote)model.Blocks[0];
            var p = (Paragraph)model.Blocks[1];
            Assert.AreEqual(2, model.Blocks.Count);
            AssertInline("line1", q.Content[0]);
            AssertInline("line2", q.Content[1]);
            AssertInline("paragraph", p.Content[0]);
        }

        [Test]
        public void Quote2()
        {
            var o2 = new WikiParser();
            var model = o2.Parse("text\r\n\r\n> line1\r\n> line2\r\nparagraph");
            Assert.AreEqual(3, model.Blocks.Count);
            var p1 = (Paragraph)model.Blocks[0];
            var q = (Quote)model.Blocks[1];
            var p2 = (Paragraph)model.Blocks[2];
            AssertInline("text", p1.Content[0]);
            AssertInline("line1", q.Content[0]);
            AssertInline("line2", q.Content[1]);
            AssertInline("paragraph", p2.Content[0]);
        }

        [Test]
        public void Symbol1()
        {
            var o2 = new WikiParser();
            foreach (var s in SymbolResolver.GetSymbolNames())
            {
                //    if (!s.StartsWith("(")) continue;
                var model = o2.Parse(s);
                Assert.AreEqual(1, model.Blocks.Count);
                var p1 = (Paragraph)model.Blocks[0];
                Assert.AreEqual(1, p1.Content.Count);
                var s1 = (Symbol)p1.Content[0];
                Assert.AreEqual(s, s1.Name);
            }
        }

        [Test]
        public void Include()
        {
            var o2 = new WikiParser(File.OpenRead);
            o2.IncludeDefaultExtension = ".wiki";
            var model = o2.Parse("@include Input/Example");
        }
    }
}