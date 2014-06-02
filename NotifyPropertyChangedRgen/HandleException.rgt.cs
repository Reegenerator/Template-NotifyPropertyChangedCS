//Formerly VB project-level imports:
using System;
using System.Collections.Generic;
using EnvDTE;


namespace NotifyPropertyChangedRgen
{
		public partial class HandleException
		{
		private List<CodeInterface> _interfaces;

		public override void PreRender()
		{
			base.PreRender();
			// System.Diagnostics.Debugger.Break()
			this._interfaces = this.GetInterfaces();
		}

		private List<CodeInterface> GetInterfaces()
		{
			List<CodeInterface> interfaces = new List<CodeInterface>();
			if (base.ProjectItem.DteObject.FileCodeModel == null)
			{
				return interfaces;
			}
			AddInterfaces(interfaces, base.ProjectItem.DteObject.FileCodeModel.CodeElements);
			return interfaces;
		}

		private void AddInterfaces(List<CodeInterface> interfaces, EnvDTE.CodeElements codeElements)
		{
			foreach (CodeElement codeElement in codeElements)
			{
				if (codeElement.Kind == EnvDTE.vsCMElement.vsCMElementInterface)
				{
					interfaces.Add((CodeInterface)codeElement);
				}
				if (codeElement.Kind == EnvDTE.vsCMElement.vsCMElementNamespace)
				{
					AddInterfaces(interfaces, ((EnvDTE.CodeNamespace)codeElement).Members);
				}
			}
		}

		public string GetMethodCall(CodeFunction codeFunction)
		{
			string result = codeFunction.get_Prototype(Convert.ToInt32(vsCMPrototype.vsCMPrototypeParamNames));
			result = result.Replace("ByVal ", string.Empty);
			result = result.Replace("ByRef ", string.Empty);
			result = result.Replace(" )", ")");
			return result;
		}
	}

}