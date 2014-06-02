using System.Windows.Forms;

//Formerly VB project-level imports:
using System;
using EnvDTE;

namespace NotifyPropertyChangedRgen
{
	internal static class ProjectSolutionExtensions
	{

#region Solution and project navigation helpers

		/// <summary>
		/// Get solution name
		/// </summary>
		/// <param name="solution"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static string GetName(this EnvDTE.Solution solution)
		{
			return solution.Properties.Item("Name").Value.ToString();
		}

		/// <summary>
		/// Get solution
		/// </summary>
		/// <param name="project"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static Solution Solution(this EnvDTE.Project project)
		{

			return project.DTE.Solution;
		}


		/// <summary>
		/// Get path to project node. To be used to select the node in Solution Explorer
		/// </summary>
		/// <param name="project"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static string GetNodePath(this EnvDTE.Project project)
		{
			return string.Format("{0}\\{1}", project.Solution().GetName(), project.Name);

		}

		/// <summary>
		///  Get path to project item node. To be used to select the node in Solution Explorer
		/// </summary>
		/// <param name="projectItem"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static string GetNodePath(this EnvDTE.ProjectItem projectItem)
		{
			//Dim sln = TryCast(projectItem, EnvDTE.Solution)
			//If sln IsNot Nothing Then Return sln.GetName

			//Dim prj = TryCast(projectItem, EnvDTE.Project)
			//If prj IsNot Nothing Then Return prj.GetNodePath

            return string.Format("{0}\\{1}", projectItem.ContainingProject.GetNodePath(), projectItem.Name);

		}

		/// <summary>
		/// Selects project item in Solution Explorer
		/// </summary>
		/// <param name="projectItem"></param>
		/// <remarks></remarks>
		public static void SelectSolutionExplorerNode(this EnvDTE.ProjectItem projectItem)
		{
			((EnvDTE80.DTE2)projectItem.DTE).SelectSolutionExplorerNode(projectItem.GetNodePath());
		}

		/// <summary>
		/// Selects project item in Solution Explorer
		/// </summary>
		/// <remarks></remarks>
		public static void SelectSolutionExplorerNode(this EnvDTE80.DTE2 dte2, string nodePath)
		{
			EnvDTE.UIHierarchyItem item = null;
			try
			{
				item = dte2.ToolWindows.SolutionExplorer.GetItem(nodePath);
				item.Select(vsUISelectionType.vsUISelectionTypeSelect);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
#endregion
	}

}