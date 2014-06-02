//Formerly VB project-level imports:
using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE80;
using Kodeo.Reegenerator.Generators;
using EnvDTE;
using System.Text;

namespace NotifyPropertyChangedRgen
{
	public abstract class CodeRendererEx : CodeRenderer
	{
		private string SavedOutput;
		public readonly string Newline = Environment.NewLine;
		public EnvDTE.DTE DTE
		{
			get
			{
				return base.ProjectItem.DteObject.DTE;
			}
		}

		public EnvDTE80.DTE2 DTE2
		{
			get
			{
				return (EnvDTE80.DTE2)DTE;
			}
		}

		public StringBuilder _OutputBuilder;
		public StringBuilder OutputBuilder
		{
			get
			{
				if (_OutputBuilder == null)
				{
                    _OutputBuilder = this.Output.GetStringBuilder();
				}
				return _OutputBuilder;

			}
		}


		public CodeElement[] GetCodeElementsAtCursor(vsCMElement? kind = null)
		{

			TextSelection sel = (TextSelection)DTE.ActiveDocument.Selection;
			TextPoint pnt = (TextPoint)sel.ActivePoint;

			// Discover every code element containing the insertion point.
			FileCodeModel fcm = DTE.ActiveDocument.ProjectItem.FileCodeModel;
			var res = GetCodeElementsAtPoint(fcm, pnt);
			if (kind.HasValue)
			{
				res = res.Where((x) => x.Kind == vsCMElement.vsCMElementProperty).ToArray();
			}
			return res;
		}
		public IEnumerable<T> GetCodeElementsAtCursor<T>() where T: class
		{
			vsCMElement kind = 0;
			if (typeof(T) == typeof(CodeProperty))
			{
				kind = vsCMElement.vsCMElementProperty;
			}
			else if (typeof(T) == typeof(CodeClass))
			{
				kind = vsCMElement.vsCMElementClass;
			}

			var ce = GetCodeElementsAtCursor(kind);
			return ce.Cast<T>();

		}

		public string RenderToString()
		{
			return System.Text.ASCIIEncoding.ASCII.GetString(this.Render().GeneratedCode);
		}

		public CodeElement[] GetCodeElementsAtPoint(FileCodeModel fcm, TextPoint pnt)
		{
			var res = new List<CodeElement>();
			CodeElement elem = null;
		    var scopes = Enum.GetValues(typeof (vsCMElement)).Cast<vsCMElement>();
            foreach (var scope in scopes)
			{
				try
				{
					elem = fcm.CodeElementFromPoint(pnt, scope);
					if (elem != null)
					{
						res.Add(elem);
					}
				}
				catch (Exception ex)
				{
					//don’t do anything -
					//this is expected when no code elements are in scope
				}
			}
			return res.ToArray();
		}
		public TextSelection GetTextSelection()
		{
			return (TextSelection)DTE.ActiveDocument.Selection;
		}

		public string SaveAndClearOutput()
		{
			this.OutputBuilder.Insert(0, SavedOutput); // combine with savedoutput in front
			SavedOutput = this.Output.ToString();
			this.OutputBuilder.Clear();
			return SavedOutput;


		}
		/// <summary>
		/// Instead of generating to a file. This is a workaround to return the value as string
		/// </summary>
		/// <param name="action"></param>
		/// <param name="removeEmptyLines"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public string GetIsolatedOutput(Action action, bool removeEmptyLines = true)
		{
			this.SaveAndClearOutput();
			action();
			var s = this.RestoreOutput();
			if (removeEmptyLines)
			{
				s = s.RemoveEmptyLines();
			}
			return s;
		}

		/// <summary>
		/// Restore saved output while returning current output
		/// </summary>
		/// <returns></returns>
		/// <remarks></remarks>
		public string RestoreOutput()
		{
			var saved = SavedOutput;
			SavedOutput = "";
			var curr = this.Output.ToString();

			OutputBuilder.Clear();
			OutputBuilder.Append(saved);
			return curr;
		}


		public bool IsIgnored(CodeProperty2 ce)
		{
			DebugWriteLine(ce.Name);
			var value = ce.ToPropertyInfo().GetGeneratorAttribute();
			return (value != null) && value.IsIgnored;

		}

		public void DebugWrite(string text)
		{
			this.OutputPaneTraceListener.Write(text);
		}
		public void DebugWriteLine(string text)
		{
			this.OutputPaneTraceListener.WriteLine(text);
		}

	}

}