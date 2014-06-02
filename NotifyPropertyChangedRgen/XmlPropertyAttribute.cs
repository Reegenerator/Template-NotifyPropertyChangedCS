//Formerly VB project-level imports:
using System;

namespace NotifyPropertyChangedRgen
{
	public class XmlPropertyAttribute : Attribute
	{
		public string Name {get; set;}
		public XmlPropertyAttribute(string attrName)
		{
			Name = attrName;
		}
	}

}