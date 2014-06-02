//Formerly VB project-level imports:

namespace NotifyPropertyChangedRgen
{
	public partial class LibraryRenderer
	{
	    public LibraryRenderer(string ns)
	    {
	        Namespace = ns;
	    }
		public const string DefaultClassName = "NotifyPropertyChanged_Gen_Extensions";
		public bool IsNet45 {get; set;}
		private string _ClassName = DefaultClassName;
		public string ClassName
		{
			get
			{
				return _ClassName;
			}
			set
			{
				_ClassName = value;
			}
		}
		public const string INotifierName = "INotifier";
		//Public Property GeneratorTag As String
		public override void PreRender()
		{
			base.PreRender();

		}

		public string RenderToString(string classNm)
		{
			this.ClassName = classNm;
			return this.RenderToString();
		}

        public string Namespace{get;set;}
	}

}