using System.Runtime.InteropServices;
using System.Windows.Forms;

//Formerly VB project-level imports:
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.IO;
using System.Collections.Concurrent;
using EnvDTE;
using EnvDTE80;
using System.Xml.Linq;
using System.Xml;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace NotifyPropertyChangedRgen
{
	internal static class Extensions
	{
		public const RegexOptions DefaultRegexOption = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline;

#region Code element helpers

        private const string InterfaceImplementationPattern = @"^.*?\sAs\s.*?(?<impl>Implements\s.*?)$";

	    private static readonly Type IsEqual_attrType = typeof(Attribute);
		private static readonly Regex GetInterfaceImplementation_regex = new Regex(InterfaceImplementationPattern, DefaultRegexOption);
		private static readonly Regex RemoveEmptyLines_regex = new Regex("^\\s+$[\\r\\n]*", RegexOptions.Multiline);
		private static readonly Dictionary<string, Assembly> GetTypeFromProject_cache = new Dictionary<string, Assembly>();
		private static readonly ConcurrentDictionary<CodeClass, Type> ToPropertyInfo_classCache = new ConcurrentDictionary<CodeClass, Type>();
		private static readonly Type GetGeneratorAttribute_type = typeof(GeneratorAttribute);

		public static IEnumerable<CodeClass2> GetClassesEx(this Kodeo.Reegenerator.Wrappers.ProjectItem item)
		{
			var classes = item.GetClasses().Values.SelectMany(x => x).Cast<CodeClass2>();
			return classes;
		}


		public static IEnumerable<CodeClass2> GetClassesWithAttributes(this Kodeo.Reegenerator.Wrappers.ProjectItem item, System.Type[] attributes)
		{
			//Replace nested class + delimiter into . as the format used in CodeAttribute.FullName
			var fullNames = attributes.Select(x=> x.DottedFullName()).ToArray();
			var res = item.GetClassesEx().Where(cclass => fullNames.All(attrName => cclass.Attributes.Cast<CodeAttribute>().Any(cAttr => cAttr.FullName == attrName)));

			return res;
		}



		/// <summary>
		/// Returns full name delimited by only dots (and no +(plus sign))
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		/// <remarks>Nested class is separated with +, while CodeClass delimit them using dots</remarks>
		public static string DottedFullName(this Type x)
		{

			return x.FullName.Replace("+", ".");
		}

		public static IEnumerable<CodeClass2> GetClassesWithAttribute(this Kodeo.Reegenerator.Wrappers.ProjectItem item, System.Type attribute)
		{
            var fullName = attribute.DottedFullName();
			//   all attributes is in class attribute
            var res = item.GetClassesEx().Where(cclass => cclass.Attributes.Cast<CodeAttribute>().Any(x => x.FullName == fullName));

			return res;
		}

		public static IEnumerable<CodeClass2> GetClassesWithAttribute(this EnvDTE.DTE dte, System.Type attribute)
		{
			var projects = Kodeo.Reegenerator.Wrappers.Solution.GetSolutionProjects(dte.Solution).Values;
			var res = from p in projects
			          from eleList in p.GetCodeElements<CodeClass2>().Values
			          from ele in eleList
                      where ele.Attributes.Cast<CodeAttribute>().Any(x => x.AsCodeElement().IsEqual(attribute))
			          select ele;

			return res;
		}

		/// <summary>
		/// Use this to convert Code element into a more generic CodeElement and get CodeElement based extensions
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cc"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static CodeElement2 AsCodeElement<T>(this T cc)
		{
			return (CodeElement2)cc;
		}
		public static CodeType AsCode<T>(this T cc)
		{

			return (CodeType)cc;
		}

		public static bool HasAttribute(this CodeType ct, Type attrType)
		{
			return ct.GetCustomAttribute(attrType) != null;
		}


#region CodeElement2 Attribute Functions

		public static CodeAttribute2 GetCustomAttribute(this CodeElement2 cc, Type attrType)
		{

            var res = cc.GetCustomAttributes().FirstOrDefault(x => x.AsCodeElement().IsEqual(attrType));
			return res;
		}

		/// <summary>
		/// Get Custom Attributes
		/// </summary>
		/// <param name="ce"></param>
		/// <returns></returns>
		/// <remarks>
		/// Requires Named Argument when declaring the Custom Attribute, otherwise Name will be empty.
		/// Not using reflection because it requires successful build
		/// </remarks>
		public static IEnumerable<CodeAttribute2> GetCustomAttributes(this CodeElement2 ce)
		{
			//!Property
			var prop = ce as CodeProperty2;
			if (prop != null)
			{
				return prop.Attributes.Cast<CodeAttribute2>();
			}

			//!Function
			var func = ce as CodeFunction2;
			if (func != null)
			{
				return func.Attributes.Cast<CodeAttribute2>();
			}

			//!Class
			var cc = ce as CodeClass2;
			if (cc != null)
			{
				return cc.Attributes.Cast<CodeAttribute2>();
			}

			throw new Exception("CodeElement not recognized");
			return Enumerable.Empty<CodeAttribute2>();
		}

		public static bool HasAttribute(this CodeElement2 ct, Type attrType)
		{
			return ct.GetCustomAttribute(attrType) != null;
		}

#endregion
#region GetCustomAttributes of CodeType/CodeClass 


		public static IEnumerable<CustomAttributeData> GetCustomAttributes(this CodeType ct)
		{

			return Type.GetType(ct.FullName).CustomAttributes;
		}
		public static IEnumerable<CustomAttributeData> GetCustomAttributes(this CodeClass cc)
		{
			return Type.GetType(cc.FullName).CustomAttributes;
		}
		public static CustomAttributeData GetCustomAttribute(this CodeType ct, Type attrType)
		{
			return ct.GetCustomAttributes().Where(x => x.AttributeType == attrType).FirstOrDefault();
		}
		public static CustomAttributeData GetCustomAttribute(this CodeClass cc, Type attrType)
		{
			return cc.GetCustomAttributes().Where(x => x.AttributeType == attrType).FirstOrDefault();
		}



		public static IEnumerable<CodeAttributeArgument> GetCodeAttributeArguments(this CodeAttribute2 cattr)
		{
			if (cattr == null)
			{
				return Enumerable.Empty<CodeAttributeArgument>();
			}
			return cattr.Arguments.Cast<CodeAttributeArgument>();
		}
		public static bool IsEqual(this CodeElement2 ele, Type type)
		{
//INSTANT C# NOTE: VB local static variable moved to class level:
//			Static attrType As Type = GetType(Attribute)
			return ele.FullName == type.FullName || ele.Name == type.Name || (type.IsSubclassOf(IsEqual_attrType) && (ele.FullName + "Attribute" == type.FullName || ele.Name + "Attribute" == type.Name));
		}

#endregion
#region CodeClass members(property, function, variable) helper

		public static IEnumerable<CodeProperty2> GetProperties(this CodeClass cls)
		{
			return cls.Children.OfType<CodeProperty2>();
		}
		public static IEnumerable<CodeFunction2> GetFunctions(this CodeClass cls)
		{
			return cls.Children.OfType<CodeFunction2>();
		}

		public static CodeProperty2[] GetAutoProperties(this CodeClass2 cls)
		{

            var props = cls.GetProperties().ToArray();
		    Func<CodeProperty, bool> isAutoProperty = (CodeProperty x) => !x.Setter.GetText().Contains("{");
		 
			return props.Where(x => x.ReadWrite == EnvDTE80.vsCMPropertyKind.vsCMPropertyKindReadWrite &&
               isAutoProperty(x) && x.OverrideKind != EnvDTE80.vsCMOverrideKind.vsCMOverrideKindAbstract).ToArray();
		}

		public static IEnumerable<CodeVariable> GetVariables(this CodeClass cls)
		{
			return cls.Children.OfType<CodeVariable>();
		}
		public static IEnumerable<CodeVariable> GetDependencyProperties(this CodeClass cls)
		{
			try
			{

				var sharedFields = cls.GetVariables().Where(x => x.IsShared && x.Type.CodeType != null);
				return sharedFields.Where(x => x.Type.CodeType.FullName == "System.Windows.DependencyProperty");
			}
			catch (Exception ex)
			{

				MessageBox.Show(ex.ToString());
			}
			return null;
		}



		/// <summary>
		/// Get Bases recursively
		/// </summary>
		/// <returns></returns>
		/// <remarks></remarks>
		public static IEnumerable<CodeClass2> GetAncestorClasses(this CodeClass2 cc)
		{
		
            var bases = cc.Bases.OfType<CodeClass2>().ToArray();

			if (bases.FirstOrDefault() == null)
			{
				return bases;
			}
			var grandBases = bases.SelectMany(x => x.GetAncestorClasses());

			return bases.Concat(grandBases);

		}
#endregion
#endregion

		public static string GetText(this CodeProperty2 prop, vsCMPart part = vsCMPart.vsCMPartWholeWithAttributes)
		{
		   
		    try
		    {
                var p = prop.GetStartPoint(part);
                if (p == null) {
                    return "";
                }
                return p.CreateEditPoint().GetText(prop.GetEndPoint(part));
		    }
		    catch (Exception ex)
		    {
		        if (ex is COMException || ex is NotImplementedException)
		        {
                    return "";
		        }
                throw;
		    }


		}
        public static string GetText(this CodeFunction ele, vsCMPart part = vsCMPart.vsCMPartWholeWithAttributes) {
            var p = ele.GetStartPoint(part);
            if (p == null) {
                return "";
            }
            return p.CreateEditPoint().GetText(ele.GetEndPoint(part));

        }
		public static string GetText(this CodeClass2 cls, vsCMPart part = vsCMPart.vsCMPartWholeWithAttributes)
		{
            try {
			var startPoint = cls.GetStartPoint(part);
			if (startPoint == null) return "";
			
            TextPoint endPoint = null;
		    endPoint = cls.GetEndPoint(part);
		  
		    return endPoint == null ? "" : startPoint.CreateEditPoint().GetText(endPoint);
            }
            catch (NotImplementedException e) {
                //catch random errors when trying to get start / end point
                Console.WriteLine(e.ToString());
                return "";
            }
		}

		public static string GetInterfaceImplementation(this CodeProperty2 prop)
		{

			var g = GetInterfaceImplementation_regex.Match(prop.GetText(vsCMPart.vsCMPartHeader)).Groups["impl"];

			//add space to separate
			if (g.Success)
			{
				return " " + g.Value;
			}
			return null;
		}
		public static TextPoint GetAttributeStartPoint(this CodeProperty2 prop)
		{
			return prop.GetStartPoint(vsCMPart.vsCMPartWholeWithAttributes);
		}


		private static System.Text.RegularExpressions.Regex _DocCommentRegex;
		/// <summary>
		/// Lazy Regex property to match doc comments
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public static System.Text.RegularExpressions.Regex DocCommentRegex
		{
			get
			{
				const string docCommentPattern = @"\s*///";
				if (_DocCommentRegex == null)
				{
					_DocCommentRegex = new System.Text.RegularExpressions.Regex(docCommentPattern);
				}
				return _DocCommentRegex;
			}
		}

        /// <summary>
        /// GetStartPoint can throw NotImplementedException. This will retry the start point without explicit attribute
        /// </summary>
        /// <param name="ce"></param>
        /// <param name="part"></param>
        /// <returns></returns>
	    public static TextPoint GetSafeStartPoint(this CodeElement ce, vsCMPart part = vsCMPart.vsCMPartWholeWithAttributes)
	    {
            try
            {
                return ce.GetStartPoint(part);

            }
            catch (NotImplementedException ex) {
                //Catch random notimplementedException
                return ce.GetStartPoint();
            }
	    }

	    public static string GetDocComment(this CodeElement ce)
	    {
	        var commentStart = ce.GetCommentStartPoint();
	        if (commentStart == null)
	        {
	            return "";
	        }

            var comment= commentStart.GetText(ce.GetStartPoint(vsCMPart.vsCMPartWholeWithAttributes));
            return comment;
	    }
		public static EditPoint GetCommentStartPoint(this CodeElement ce)
		{
            try {
                return ce.GetSafeStartPoint().GetCommentStartPoint();

            }
            catch (NotImplementedException ex) {
                //Catch random notimplementedException
                return null;
            }
		}
		public static EditPoint GetCommentStartPoint(this CodeProperty2 ce)
		{
		  
		    return ce.AsCodeElement().GetCommentStartPoint();
		}

		/// <summary>
		/// Get to the beginning of doc comments for startPoint
		/// </summary>
		/// <param name="startPoint"></param>
		/// <returns></returns>
		/// <remarks>
		/// EnvDte does not have a way to get to the starting point of a code element doc comment. 
		/// If we need to insert some text before a code element that has doc comments we need to go to the beggining of the comments.
		/// </remarks>
		public static EditPoint GetCommentStartPoint(this TextPoint startPoint)
		{

			var sp = startPoint.CreateEditPoint();
			//keep going 1 line up until the line does not start with doc comment prefix
			do
			{
				sp.LineUp(1);
            } while (DocCommentRegex.IsMatch(sp.GetLineText()));
			//Go to the beginning of first line of comment, or element itself
			sp.LineDown(1);
			sp.StartOfLine();
			return sp;
		}

        public static EditPoint GetPositionBeforeClosingBrace(this CodeFunction cf) {
            const string closingBrace = "}";
            var sp = cf.EndPoint.CreateEditPoint();
            
            //keep going 1 char left until we found the } char
            do {
                sp.CharLeft(1);
            } while (sp.GetText(1) != closingBrace);
            //Go left one more char
            sp.CharLeft(1);
            return sp;
        }

		public static string ToStringFormatted(this XElement xml)
		{
			var settings = new XmlWriterSettings();
			settings.OmitXmlDeclaration = true;

			var result = new StringBuilder();
			using (var writer = XmlWriter.Create(result, settings))
			{

				xml.WriteTo(writer);
			}
			return result.ToString();
		}

#region ExprToString. Convert Member Expression to string.

		public static string[] ExprsToString<T>(params Expression<Func<T, object>>[] exprs)
		{

			var strings = (
			    from x in exprs
			    select ((LambdaExpression)x).ExprToString()).ToArray();
			return strings;
		}

		public static string ExprToString<T, T2>(this Expression<Func<T, T2>> expr)
		{
			return ((LambdaExpression)expr).ExprToString();
		}

		public static string ExprToString(this LambdaExpression memberExpr)
		{
			if (memberExpr == null)
			{
				return "";
			}
			System.Linq.Expressions.Expression currExpr = null;
			//when T2 is object, the expression will be wrapped in UnaryExpression of Convert{}
			var convertedToObject = memberExpr.Body as UnaryExpression;
			if (convertedToObject != null)
			{
				//unwrap
				currExpr = convertedToObject.Operand;
			}
			else
			{
				currExpr = memberExpr.Body;
			}
			switch (currExpr.NodeType)
			{
				case ExpressionType.MemberAccess:
					var ex = (MemberExpression)currExpr;
					return ex.Member.Name;
			}

			throw new Exception("Expression ToString() extension only processes MemberExpression");
		}

#endregion

		public static List<Type> FindAllDerivedTypes<T>()
		{
			return FindAllDerivedTypes<T>(Assembly.GetAssembly(typeof(T)));
		}

		public static List<Type> FindAllDerivedTypes<T>(Assembly assembly)
		{
			var derivedType = typeof(T);
			return assembly.GetTypes().Where(x => x != derivedType && derivedType.IsAssignableFrom(x)).ToList();

		}

		public static IEnumerable<CodeClass2> GetSubclasses(this CodeClass2 cc)
		{
			var fullname = cc.FullName;
			var list = new List<CodeClass2>();
			Kodeo.Reegenerator.Wrappers.CodeElement.TraverseSolutionForCodeElements<CodeClass2>(
                cc.DTE.Solution, list.Add, x => x.FullName != fullname && x.IsDerivedFrom[fullname]);
			return list.ToArray();
		}

		public static string RemoveEmptyLines(this string s)
		{
//INSTANT C# NOTE: VB local static variable moved to class level:
//			Static regex As Regex = new Regex("^\\s+$[\\r\\n]*", RegexOptions.Multiline)
			return RemoveEmptyLines_regex.Replace(s, "");
		}


		public static TResult SelectOrDefault<T, TResult>(this T obj, Func<T, TResult> selectFunc, TResult defaultValue = null) where T :class where TResult : class
		{
		    return obj == null ? defaultValue : selectFunc(obj);
		}

	    /// <summary>
		/// Returns a type from an assembly reference by ProjectItem.Project. Cached.
		/// </summary>
		/// <param name="pi"></param>
		/// <param name="typeName"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static Type GetTypeFromProject(this ProjectItem pi, string typeName)
		{
//INSTANT C# NOTE: VB local static variable moved to class level:
//			Static cache As new Dictionary(Of String, Assembly)
			var path = pi.GetAssemblyPath();
			if (!(GetTypeFromProject_cache.ContainsKey(path)))
			{
				GetTypeFromProject_cache.Add(path, Assembly.LoadFrom(path));
			}

			var asm = GetTypeFromProject_cache[path];
			return asm.GetType(typeName);
		}
		public static Type ToType(this CodeClass cc)
		{
			return cc.ProjectItem.GetTypeFromProject(cc.FullName);
		}

		/// <summary>
		/// Convert CodeProperty2 to PropertyInfo. Cached
		/// </summary>
		/// <param name="prop"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static PropertyInfo ToPropertyInfo(this CodeProperty2 prop)
		{

//INSTANT C# NOTE: VB local static variable moved to class level:
//			Static classCache As new ConcurrentDictionary(Of CodeClass, Type)
            var classType = ToPropertyInfo_classCache.GetOrAdd(prop.Parent, x => prop.Parent.ToType());
			return classType.GetProperty(prop.Name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public static GeneratorAttribute GetGeneratorAttribute(this MemberInfo mi)
		{
//INSTANT C# NOTE: VB local static variable moved to class level:
//			Static type As Type = GetType(GeneratorAttribute)
            var genAttr = mi.GetCustomAttributes().FirstOrDefault(x => x.GetType().IsSubclassOf(GetGeneratorAttribute_type));
			return (GeneratorAttribute)genAttr;
		}

		public static Assembly GetAssemblyOfProjectItem(this ProjectItem pi)
		{
			var path = pi.GetAssemblyPath();

			if (!string.IsNullOrEmpty(path))
			{
				return Assembly.LoadFrom(path);
			}
			else
			{
				return null;
			}

		}

		public static string GetAssemblyPath(this ProjectItem pi)
		{

			var assemblyName = pi.ContainingProject.Properties.Cast<Property>().FirstOrDefault(x => x.Name == "AssemblyName").SelectOrDefault(x => x.Value);

            return pi.ContainingProject.GetAssemblyPath();
		}

		/// <summary>
		/// Currently unused. If we require a succesful build, a project that requires succesful generation would never build, catch-22
		/// </summary>
		/// <param name="vsProject"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static string GetAssemblyPath(this EnvDTE.Project vsProject)
		{
			var fullPath = vsProject.Properties.Item("FullPath").Value.ToString();
			var outputPath = vsProject.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
			var outputDir = Path.Combine(fullPath, outputPath);
			var outputFileName = vsProject.Properties.Item("OutputFileName").Value.ToString();
			var assemblyPath = Path.Combine(outputDir, outputFileName);
			return assemblyPath;
		}

        const string TypeWithoutQualifierPattern = @"(?<=\.?)[^\.]+?$";
		private static readonly Regex TypeWithoutQualifierRegex = new Regex(TypeWithoutQualifierPattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
		public static string StripQualifier(this string s)
		{
			var stripped = TypeWithoutQualifierRegex.Match(s).Value;
			return stripped;
		}

		public static T ParseAsEnum<T>(this string qualifiedName, T defaultValue) where T: struct
		{
			if (string.IsNullOrEmpty(qualifiedName))
			{
				return defaultValue;
			}
			var res = defaultValue;
			Enum.TryParse(qualifiedName.StripQualifier(),out res);
			return res;
		}

		public static T GetOrInit<T>(ref T x, Func<T> initFunc) where T : class
		{
		    return x ?? (x = initFunc());
		}

	    /// <summary>
		/// Create type instance from string
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static object ConvertFromString(this Type type, string value)
		{
			return TypeDescriptor.GetConverter(type).ConvertFromString(value);
		}

		/// <summary>
		/// Set property value from string representation
		/// </summary>
		/// <param name="propInfo"></param>
		/// <param name="obj"></param>
		/// <param name="value"></param>
		/// <remarks></remarks>
		public static void SetValueFromString(this PropertyInfo propInfo, object obj, string value)
		{
			object setValue = null;
			if (propInfo.PropertyType == typeof(Version))
			{
				setValue = Version.Parse(value);
			}
			else
			{
				setValue = propInfo.PropertyType.ConvertFromString(value);

			}
			propInfo.SetValue(obj, setValue);
		}

		public static void AddInterfaceIfNotExists(this CodeClass2 cls, string interfaceName)
		{
		    try
		    {
		      
		        if (!(cls.ImplementedInterfaces.OfType<CodeInterface>().Any(x => x.FullName == interfaceName)))
		        {
		            cls.AddImplementedInterface(interfaceName);
		        }
		    }
		    catch (Exception e)
		    {
                MessageBox.Show("The added interface has to exists in the project." + e.ToString());

		    }
		}

	    public static string GetDefaultNamespace(this ProjectItem item)
	    {
	        return item.ContainingProject.Properties.Item("DefaultNamespace").Value.ToString();
	    }

		public static string DotJoin(this string s, params string[] segments)
		{
			var all = new[] {s}.Concat(segments).ToArray();
			return string.Join(".", all);
		}


		/// <summary>
		/// Returns CodeTypeRef.AsFullName, if null, returns CodeTypeRef.AsString
		/// </summary>
		/// <param name="ctr"></param>
		/// <returns></returns>
		/// <remarks>
		/// If there's compile error AsFullName will be null
		/// </remarks>
		public static string SafeFullName(this CodeTypeRef ctr)
		{
			return (ctr.AsFullName ?? ctr.AsString);
		}

	}

}