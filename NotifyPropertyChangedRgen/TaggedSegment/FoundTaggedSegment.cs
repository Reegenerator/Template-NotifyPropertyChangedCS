//Formerly VB project-level imports:
using System;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace NotifyPropertyChangedRgen
{
	public partial class TagManager<T> where T: GeneratorAttribute, new()
	{


		/// <summary>
		/// Stores information parsed by TagManager
		/// </summary>
		/// <remarks></remarks>
		public class FoundTaggedSegment : TextRange
		{
			/// <summary>
			/// Attribute generated from the found xml tag
			/// </summary>
			/// <value></value>
			/// <returns></returns>
			/// <remarks></remarks>
			public T FoundTag {get; set;}
			/// <summary>
			/// Actual attribute declared on containing property or class
			/// </summary>
			/// <value></value>
			/// <returns></returns>
			/// <remarks></remarks>
			public T DeclaredAttribute {get; set;}
			public TagManager<T> Manager {get; set;}
			private Regex Regex;

		    public FoundTaggedSegment(TagManager<T> mgr, T sourceAttr, EnvDTE.TextPoint start, EnvDTE.TextPoint endP)
			{
				Manager = mgr;
				StartPoint = start;
				EndPoint = endP;
				DetectSegmentType();
				Parse(sourceAttr);
			}

			public DateTime? GenerateDate {get; set;}
			public SegmentTypes SegmentType {get; set;}




			public bool IsOutdated()
			{
				switch (FoundTag.RegenMode)
				{
					case GeneratorAttribute.RegenModes.Always:
						return true;
					default:
						var diffProperties = !(FoundTag.AreArgumentsEquals(DeclaredAttribute));
						return diffProperties;

				}

			}

			private void DetectSegmentType()
			{
				var firstline = StartPoint.CreateEditPoint().GetLineText();
			    if (firstline.Trim().StartsWith(RegionBeginKeyword))
				{
					this.SegmentType = SegmentTypes.Region;
					Regex = Manager.RegionRegex;

				}
				else
				{
					this.SegmentType = SegmentTypes.Statements;
					Regex = Manager.CommentRegex;
				}
			}

#region Find Segment

			/// <summary>
			/// Extract valid xml inside Region Name and within inline comment
			/// </summary>
			/// <returns></returns>
			/// <remarks>
			/// </remarks>
			public string ExtractXmlContent()
			{
				var text = this.GetText();
				var xmlContent = "";
				switch (this.SegmentType)
				{
					case SegmentTypes.Region:
						xmlContent = Manager.RegionRegex.Replace(text, "${xml}");
						break;
					case SegmentTypes.Statements:
						xmlContent = Manager.CommentRegex.Replace(text, "${tag}${content}${tagend}");
						break;
				}

				return xmlContent;
			}

			public void Parse(T parentAttr)
			{
				DeclaredAttribute = parentAttr;

				if (!IsValid)
				{
					return;
				}
				var xml = ExtractXmlContent();
				try
				{
					var xdoc = XDocument.Parse(xml);
					var xr = xdoc.Root;

					GenerateDate = (xr.Attribute("Date").Value != null) ? Convert.ToDateTime(xr.Attribute("Date").Value) : (DateTime?) null;
					FoundTag = new T();
					FoundTag.CopyPropertyFromTag(xr);
				}
				catch (Exception ex)
				{
					DebugExtensions.DebugHere();
				}



			}





#endregion

		}
	}
}