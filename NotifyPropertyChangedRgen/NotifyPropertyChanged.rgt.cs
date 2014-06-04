using System.ComponentModel;
using System.Windows.Forms;

//Formerly VB project-level imports:
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Kodeo.Reegenerator.Generators;
using NotifyPropertyChangedRgen.TaggedSegment;
using ThisClass = NotifyPropertyChangedRgen.NotifyPropertyChanged;
using System.IO;
using Writer = NotifyPropertyChangedRgen.TaggedSegment.Manager<NotifyPropertyChangedRgen.NotifyPropertyChanged_GenAttribute>.Writer;
using FoundSegment = NotifyPropertyChangedRgen.TaggedSegment.Manager<NotifyPropertyChangedRgen.NotifyPropertyChanged_GenAttribute>.FoundSegment;

/// <summary>
/// To use this renderer, attach to the target file. And add AutoGenerateAttribute to the class
/// </summary>
/// <remarks></remarks>
namespace NotifyPropertyChangedRgen {
    public partial class NotifyPropertyChanged {


        private static readonly string INotifyPropertyChangedName = typeof(INotifyPropertyChanged).FullName;
        private static readonly Manager<NotifyPropertyChanged_GenAttribute> SharedManager;
        private readonly Type AttrType = typeof(NotifyPropertyChanged_GenAttribute);


        
        private string INotifierFullName
        {
            get { return this.ProjectItem.Project.DefaultNamespace.DotJoin(LibraryRenderer.INotifierName); }
        }

        static NotifyPropertyChanged() {
            //var tagName = (new NotifyPropertyChanged_GenAttribute()).TagName;
            SharedManager = new Manager<NotifyPropertyChanged_GenAttribute>();
        }
        public Manager<NotifyPropertyChanged_GenAttribute> Manager {
            get {
                return SharedManager;
            }
        }

        /// <summary>
        /// Create the library file that contains INotifier and Notification extensions
        /// has to be created before EnvDte can add the interface <see cref="NotifyPropertyChangedRgen.Extensions.AddInterfaceIfNotExists"/> to classes
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        public void RenderLibrary()
        {
            //Check for existing class
            var prj = ProjectItem.Project;
            
            const string className = LibraryRenderer.DefaultClassName;
            var classes = prj.GetCodeElements<CodeClass>();
            var classFullname = prj.DefaultNamespace.DotJoin(className);
            List<CodeClass> matchingClasses = null;
            classes.TryGetValue(classFullname, out matchingClasses);

            ProjectItem classItem = null;
            if (matchingClasses == null) {
                //Class not found, generate
                var filePath = Path.Combine(prj.FullPath, className + ".cs");
                //if file exists, warn then quit
                if (File.Exists(filePath)) {
                    MessageBox.Show(string.Format("Trying to add {0} to project, but file {1} already exists", className, filePath));
                    return;
                }
                //Create new empty file
                File.WriteAllText(filePath, "");
                prj.CheckOut();
                //Add it to the project
                classItem = prj.DteObject.ProjectItems.AddFromFile(filePath);
            }
            else {
                //Class found, get corresponding project item
                classItem = matchingClasses.First().ProjectItem;
            }

            //Open file for editing
            var wasOpen = classItem.IsOpen[Constants.vsViewKindCode];
            if (!wasOpen) {
                classItem.Open(Constants.vsViewKindCode);
            }
            var textDoc = classItem.Document.ToTextDocument();

            var genInfo = new Writer() { SearchStart = textDoc.StartPoint, InsertStart = textDoc.StartPoint, SearchEnd = textDoc.EndPoint, SegmentType = Types.Region };
            if (Manager.IsAnyOutdated(genInfo)) {
                //generate text if outdated
                var extRgen = new LibraryRenderer(prj.DefaultNamespace);

                var code = extRgen.RenderToString();
                genInfo.Content = code;
                Manager.InsertOrReplace(genInfo);
                classItem.Save("");
            }

            //restore to previous state
            if (!wasOpen) {
                classItem.Document.Close(vsSaveChanges.vsSaveChangesPrompt);
            }
        }

        /// <summary>
        /// Render within target file, instead of into a separate file
        /// </summary>
        /// <remarks></remarks>
        private void RenderWithinTarget() {

            var undoCtx = DTE.UndoContext;
            undoCtx.Open(AttrType.Name, false);
            try {
                //render shared library. It has to be created before the interface can be added to the classes. Otherwise EnvDte would throw exception
                RenderLibrary();

                var validClasses = GetValidClasses();

                var sw = new Stopwatch();
                var hasError = false;
                //!for each class 
                foreach (var cc in validClasses) {
                    sw.Start();

                    var classWriter = new Writer() { Class = cc };

                    //!generate
                    GenerateInClass(classWriter);

                    //!if also doing derivedClasses
                    if (classWriter.GenAttribute.ApplyToDerivedClasses) {

                        //!for each subclass
                        foreach (var derivedC in cc.GetSubclasses()) {
                            var childInfo = new Writer() { TriggeringBaseClass = cc, Class = derivedC };
                            //generate
                            GenerateInClass(childInfo);
                            //combine status
                            if (childInfo.HasError) {
                                classWriter.HasError = true;
                                classWriter.Status.AppendLine(childInfo.Status.ToString());
                            }
                        }
                    }

                    //if there's error
                    if (classWriter.HasError) {
                        hasError = true;
                        MessageBox.Show(classWriter.Status.ToString());
                    }
                    //finish up
                    sw.Stop();
                    DebugWriteLine(string.Format("Finished {0} in {1}", cc.Name, sw.Elapsed));
                    sw.Reset();
                }
            

                //if there's error
                if (hasError) {
                    //undo everything
                    undoCtx.SetAborted();
                }
                else {
                    undoCtx.Close();
                    //automatically save, since we are changing the target file
                    var doc = ProjectItem.DteObject.Document;
                    //if anything is changed, save
                    if (doc != null && !doc.Saved) {
                        doc.Save();
                    }
                }

            }
            catch (Exception ex)
            {
                DebugExtensions.DebugHere();
                if (undoCtx.IsOpen) {
                    undoCtx.SetAborted();
                }
            }
        }

        private CodeClass2[] GetValidClasses() {
            //get only classes marked with the attribute

            var validClasses = (
                from cc in ProjectItem.GetClassesWithAttribute(AttrType)
                select cc).ToArray();
            return validClasses;
        }

        /// <summary>
        /// Expand Auto properties into a normal properties, so we can insert Notify statement in the setter
        /// </summary>
        /// <param name="tsWriter"></param>
        /// <remarks></remarks>
        public void ExpandAutoProperties(Writer tsWriter) {
            var autoProps = tsWriter.Class.GetAutoProperties().Where((x) => !((new NotifyPropertyChanged_GenAttribute(x)).IsIgnored));
            foreach (var p in autoProps) {
                ExpandAutoProperty(p, tsWriter);
            }
        }
        /// <summary>
        /// Expand auto property into a normal property
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="parentWriter"></param>
        /// <remarks></remarks>
        public void ExpandAutoProperty(CodeProperty2 prop, Writer parentWriter)
		{

			//Save existing doc comment
            var comment = prop.AsCodeElement().GetDocComment();
			//Get its attribute
			var propAttrs = prop.GetText(vsCMPart.vsCMPartAttributesWithDelimiter);
			//Interface implementation 
			var interfaceImpl = prop.GetInterfaceImplementation();


			var tsWriter = new Writer(parentWriter)
			{
			    SegmentType = Types.Region,
			    TagComment = string.Format("{0} auto expanded by", prop.Name),
			    GenAttribute = {RegenMode = GeneratorAttribute.RegenModes.Once}
			};
			//only do this once, since once it is expanded it will no longer be detected as auto property

            var completeProp = GetIsolatedOutput(() => OutProperty(tsWriter.CreateTaggedRegionName(), prop.Name, prop.Type.SafeFullName(), comment, propAttrs, interfaceImpl));

			//Replace all code starting from comment to endPoint of the property
            var ep = prop.GetCommentStartPoint().CreateEditPoint();
            const int options = (int)( vsEPReplaceTextOptions.vsEPReplaceTextAutoformat |
                                       vsEPReplaceTextOptions.vsEPReplaceTextNormalizeNewlines);
            ep.ReplaceText(prop.EndPoint, completeProp, options);

		}

        /// <summary>
        /// Generate code that will notify other propertyName different from the member with the attribute.
        /// </summary>
        /// <param name="genAttr"></param>
        /// <param name="parentWriter"></param>
        /// <remarks>
        /// Example Add NotifyPropertyChanged_GenAttribute with ExtraNotifications="OtherProperty1,OtherProperty2" to SomeProperty.
        /// This method will generate code for Notify("OtherProperty1") and Notify("OtherProperty2") within that member
        /// This is useful for Property that affects other Property, or a method that affects another property.
        /// This has the advantage of generation/compile time verification of the properties
        /// </remarks>
        private string GenInMember_ExtraNotifications(NotifyPropertyChanged_GenAttribute genAttr, Writer parentWriter)
        {

            //Render extra notifications (notifications for other related properties)
            if (string.IsNullOrEmpty(genAttr.ExtraNotifications)) {
                return null;
            }
            //also split by space to trim it
            var extras = genAttr.ExtraNotifications.Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);

            //Verify that all properties listed in ExtraNotification actually exists
            var invalids = genAttr.ValidateExtraNotifications(parentWriter.Class, extras);
            if (invalids.Any()) {
                parentWriter.HasError = true;
                parentWriter.Status.AppendFormat("Properties:{0} to be notified are not found in the class", string.Join(", ", invalids));
                return "";
            }

            return string.Format("this.NotifyChanged({0});", string.Join(",", extras.Select((x) => x.Quote())));


        }

        /// <summary>
        /// Generates code in 
        /// </summary>
        /// <param name="genAttr"></param>
        /// <param name="parentWriter"></param>
        /// <remarks></remarks>
        private void GenInMember(NotifyPropertyChanged_GenAttribute genAttr, Writer parentWriter)
		{
			var prop = genAttr.ParentProperty;
			//!Parent can be either CodeFunction(only for ExtraNotifications) or CodeProperty
			string code = null;
            switch (genAttr.GenerationType)
            {
                case NotifyPropertyChanged_GenAttribute.GenerationTypes.NotifyOnly:
                    //Only notification
                    code = string.Format("this.NotifyChanged({0})", prop.Name);
                    break;
                default:
                    code = (genAttr.ParentProperty != null) ? string.Format("this.SetPropertyAndNotify(ref _{0}, value, \"{0}\");", prop.Name) : "";
                    break;
            }

			//Extra notifications
			var extraNotifyCode = GenInMember_ExtraNotifications(genAttr, parentWriter);
			code = code.Conjoin(Environment.NewLine, extraNotifyCode);

			//Code Element, could be property setter or a method
			var codeElement = (CodeFunction2)((prop != null) ? prop.Setter : genAttr.ParentFunction);
			var memberWriter = new Writer(parentWriter) {GenAttribute = genAttr, SearchStart = codeElement.StartPoint, SearchEnd = codeElement.EndPoint, Content = code, SegmentType = Types.Statements};

			//Find insertion point
			EditPoint insertPoint = null;
			var insertTag = Manager.FindInsertionPoint(memberWriter);
			if (insertTag == null)
			{
				//!No insertion point tag specified, by default insert as last line of setter
				insertPoint = codeElement.GetPositionBeforeClosingBrace();
                //always insert new line in case the everything in one line
				
			}
			else
			{
				//!InsertPoint Tag found, insert right after it
				insertPoint = insertTag.EndPoint.CreateEditPoint();
				insertPoint.LineDown(1);
				insertPoint.StartOfLine();
			}

			memberWriter.InsertStart = insertPoint;
			Manager.InsertOrReplace(memberWriter);

		}
        private void GenInMembers(Writer tsWriter) {
            //!Generate in properties
            var props = tsWriter.Class.GetProperties().ToArray();
            var propAttrs = (
                from p in props
                select new NotifyPropertyChanged_GenAttribute(p)).ToArray();

            var functions = from f in tsWriter.Class.GetFunctions()
                            where f.AsCodeElement().HasAttribute(AttrType)
                            select f;
            var funcAttrs = (
                from f in functions
                select new NotifyPropertyChanged_GenAttribute(f)).ToArray();



            var dpFields = tsWriter.Class.GetDependencyProperties();


            Func<NotifyPropertyChanged_GenAttribute, bool> notDpField = (NotifyPropertyChanged_GenAttribute x) => !(dpFields.Any((dp) => dp.Name == x.ParentProperty.Name + "Property"));
            Func<NotifyPropertyChanged_GenAttribute, bool> notIgnored = (NotifyPropertyChanged_GenAttribute x) => !(x.IsIgnored);


            var propsWithSetters = propAttrs.Where((x) => x.ParentProperty.Setter != null);

            //?filter out property for DependencyProperties 
            var validMembers = funcAttrs.Concat(propsWithSetters.Where(notDpField).Where(notIgnored));

            foreach (NotifyPropertyChanged_GenAttribute pa in validMembers) {

                GenInMember(pa, tsWriter);
            }

        }

        private static CodeClass2 GetFirstAncestorImplementing(IEnumerable<CodeClass2> ancestorClasses, string interfaceName) {
            return ancestorClasses.FirstOrDefault((x) => x.ImplementedInterfaces.OfType<CodeInterface>().Any((i) => i.FullName == interfaceName));

        }
        private void GenerateNotifyFunctions(Writer tsWriter)
		{
			if (tsWriter.IsTriggeredByBaseClass)
			{
				return;
			}
            var firstMember = tsWriter.Class.Members.Cast<CodeElement>().FirstOrDefault();
			if (firstMember == null) //?if there's no member, there won't be any properties. Skip
			{
				return;
			}

			//!If INotify is already implemented by base class, do not generate (only generate tag)
            var ancestorClasses = tsWriter.Class.GetAncestorClasses().ToArray();
			var ancestorImplementingINPC = GetFirstAncestorImplementing(ancestorClasses, INotifyPropertyChangedName);
			string inotifierFullname = string.Format("{0}.{1}", ProjectItem.Project.DefaultNamespace, LibraryRenderer.INotifierName);
			var ancestorImplementingINotifier = GetFirstAncestorImplementing(ancestorClasses, inotifierFullname);
			string code = "";
			if (ancestorImplementingINotifier != null)
			{
				code = string.Format("'{0} already implemented by {1}", LibraryRenderer.INotifierName, ancestorImplementingINotifier.FullName);
			}
			else if (ancestorImplementingINPC != null)
			{
                code = string.Format("'{0} already implemented by {1}{2}{3}", ancestorImplementingINPC.FullName, INotifyPropertyChangedName, Environment.NewLine, GetIsolatedOutput(() => OutFunctions(tsWriter.Class.FullName, false)));
			}
			else
			{
				code = GetIsolatedOutput(() => OutFunctions(tsWriter.Class.FullName, true));
			}

            var insertPoint = tsWriter.Class.GetStartPoint(vsCMPart.vsCMPartBody).CreateEditPoint();
			insertPoint.StartOfLine();
            //var body = tsWriter.Class.GetText(vsCMPart.vsCMPartBody);
            //var header = tsWriter.Class.GetText(vsCMPart.vsCMPartHeader);
            //var x = tsWriter.Class.GetText(vsCMPart.vsCMPartBodyWithDelimiter);
            //var name = tsWriter.Class.GetText(vsCMPart.vsCMPartName);
			//copy info, instead of using the passed parameter, prevent unintentionally using irrelevant property set 
			// by other code
			var newInfo = new Writer(tsWriter)
			{
			    SearchStart = tsWriter.Class.StartPoint,
			    SearchEnd = tsWriter.Class.EndPoint,
			    InsertStart = insertPoint,
			    Content = code,
			    SegmentType = Types.Region,
			    TagComment = "INotifier Functions",
			    GenAttribute = {SegmentClass = "INotifierFunctions"}
			};

            var isUpdated = Manager.InsertOrReplace(newInfo);
			if (isUpdated)
			{
                var fullname = tsWriter.Class.ProjectItem.GetDefaultNamespace().DotJoin(LibraryRenderer.INotifierName);
                tsWriter.Class.AddInterfaceIfNotExists(fullname);
			}


		}
        public void GenerateInClass(Writer writer) {

            GenerateNotifyFunctions(writer);
            ExpandAutoProperties(writer);
            GenInMembers(writer);
            AppendWarning(writer);
        }

        private void AppendWarning(Writer writer) {
            CodeProperty2[] autoProperties = writer.Class.GetAutoProperties().Where((x) => !((new NotifyPropertyChanged_GenAttribute(x)).IsIgnored)).ToArray();
            //?Warn unprocesssed autoproperties
            if (autoProperties.Any()) {
                writer.HasError = true;
                writer.Status.AppendFormat("{0} Autoproperties skipped:", writer.Class.Name).AppendLine();

                foreach (var ap in autoProperties) {
                    writer.Status.AppendIndent(1, ap.Name).AppendLine();
                }


            }
        }

        public override RenderResults Render() {

            RenderWithinTarget();
            return null; // new RenderResults();
            //"'Because of the way custom tool works a file has to be generated. This file can be safely ignored.");

        }



    }
}