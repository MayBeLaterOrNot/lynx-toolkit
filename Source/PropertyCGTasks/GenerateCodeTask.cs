// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenerateCodeTask.cs" company="Lynx Toolkit">
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
// <summary>
//   Provides a task that generates c# code.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace PropertyCGTasks
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    using PropertyCG;

    // Issues
    // - log messages not visible?
    // - updating .cs files is too late (not compiled before next build)
    // - Should this be a custom tool: http://www.drewnoakes.com/snippets/WritingACustomCodeGeneratorToolForVisualStudio/

    /// <summary>
    /// Provides a task that generates c# code.
    /// </summary>
    /// <remarks><code>
    ///  <UsingTask TaskName="PropertyCGTasks.GenerateCodeTask" AssemblyFile="$(SolutionDir)..\..\Tools\Lynx\PropertyCGTasks.dll"/>
    ///  <Target Name="BeforeBuild">
    ///  <PropertyCGTasks.GenerateCodeTask Type="oml" Scc="p4"/>
    ///  </Target>
    ///  </code>
    /// </remarks>
    public class GenerateCodeTask : Task
    {
        public string OpenForEditExecutable { get; set; }
        public string OpenForEditArguments { get; set; }
        public string SearchPattern { get; set; }
        public string Folder { get; set; }

        public string Type
        {
            set
            {
                SearchPattern = "*." + value;
            }
        }

        public string Scc
        {
            set
            {
                switch (value.ToLower())
                {
                    case "p4":
                        OpenForEditExecutable = "p4.exe";
                        OpenForEditArguments = "edit {0}";
                        break;
                }
            }
        }

        public GenerateCodeTask()
        {
            OpenForEditExecutable = null;
            OpenForEditArguments = null;
            SearchPattern = "*.pml";
            Folder = ".";
        }

        public override bool Execute()
        {
            Console.SetOut(new LogWriter(this));
            var stopwatch = Stopwatch.StartNew();
            this.LogMessage(LynxToolkit.Application.Header);
            try
            {
                this.LogMessage("  " + Environment.CurrentDirectory);
                this.SearchDirectory(this.Folder);
            }
            catch (Exception exception)
            {
                this.Log.LogError("{0}: {1}", this, exception);
                return false;
            }
            finally
            {
                stopwatch.Stop();
                Console.WriteLine("Finished ({0}ms)", stopwatch.ElapsedMilliseconds);
            }
            return true;
        }

        private void SearchDirectory(string path)
        {
            foreach (var file in Directory.GetFiles(path, SearchPattern))
            {
                Process(file);
            }
            foreach (var dir in Directory.GetDirectories(path))
                SearchDirectory(dir);
        }

        private void Process(string file)
        {
            var cg = new PropertyCodeGenerator(file);
            cg.GenerateModel();
            var modified = cg.SaveIfModified();
            this.LogMessage("  " + (modified ? "UPDATED: " : "") + file);
        }

        private void LogMessage(string value)
        {
            try
            {
                var args = new BuildMessageEventArgs(value, string.Empty, this.ToString(), MessageImportance.Normal);
                this.BuildEngine.LogMessageEvent(args);
            }
            catch (NullReferenceException)
            {
                // Don't throw as task and BuildEngine will be null in unit test.
            }
        }
    }

    public class LogWriter : TextWriter
    {
        private readonly ITask task;

        public LogWriter(ITask task)
        {
            this.task = task;
        }

        public override void WriteLine(string value)
        {
            try
            {
                var args = new BuildMessageEventArgs(value, string.Empty, task.ToString(), MessageImportance.Normal);
                task.BuildEngine.LogMessageEvent(args);
            }
            catch (NullReferenceException)
            {
                // Don't throw as task and BuildEngine will be null in unit test.
            }
        }

        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }
    }
}