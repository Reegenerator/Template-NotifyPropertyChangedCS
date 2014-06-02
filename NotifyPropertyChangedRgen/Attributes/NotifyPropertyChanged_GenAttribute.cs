using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using EnvDTE80;
using ThisClass = NotifyPropertyChangedRgen.NotifyPropertyChanged_GenAttribute;
namespace NotifyPropertyChangedRgen
{
    public class NotifyPropertyChanged_GenAttribute : GeneratorAttribute {
        public NotifyPropertyChanged_GenAttribute() {
            Init();
        }
        public NotifyPropertyChanged_GenAttribute(CodeProperty2 p)
            : base(p) {
            Init();
            }
        public NotifyPropertyChanged_GenAttribute(CodeClass2 cc)
            : base(cc) {
            Init();
            }
        sealed public override void Init() {
            base.Init();
            //To regenerate all OnVersionChanged generated code, increment the version number
            Version = new Version(1, 1, 0, 17);
        }
        internal NotifyPropertyChanged_GenAttribute(CodeFunction2 f)
            : base(f) {
            Init();
            }


        private static XElement PrototypeXElement;
        public override XElement TagPrototype {
            get {
                XNamespace ns = "http://tempuri.org/NotifyPropertyChanged.xsd";
                var xEle = new XElement(ns + "Gen");
                xEle.Add(new XAttribute("Renderer", TagName));
                //REMOVED can't be converted to C# <rgn:Gen Renderer=<%= TagName %>/>
                return PrototypeXElement ?? (PrototypeXElement = xEle);
            }
        }

        public enum GenerationTypes {
            DefaultType,
            SetAndNotify,
            NotifyOnly
        }

        private static readonly Type ThisType = typeof(ThisClass);

        public GenerationTypes GenerationType { get; set; }


        /// <summary>
        /// A simple comma delimited string, since its in attribute we cannot use expression as parameters
        /// But it will be checked against the type during generation, if the property does not exists there will be a warning
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [XmlProperty("ExtraNotifications")]
        public string ExtraNotifications { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>Array of invalid properties</returns>
        /// <remarks></remarks>
        public string[] ValidateExtraNotifications(CodeClass2 cc, string[] extras) {

            var propNames = new HashSet<string>(cc.GetProperties().Select((x) => x.Name));
            var invalids = extras.Where((x) => !(propNames.Contains(x))).ToArray();
            if (invalids.Any()) {
                return invalids;
            }

            ExtraNotifications = string.Join(", ", extras);
            return new string[0]; //return empty array
        }

        public override bool AreArgumentsEquals(GeneratorAttribute other) {
            var otherNPC = (NotifyPropertyChanged_GenAttribute)other;
            return base.AreArgumentsEquals(other) && this.ExtraNotifications == otherNPC.ExtraNotifications;
        }
    }
}