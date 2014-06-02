
using System.ComponentModel;
using NotifyPropertyChangedRgen;
namespace WinformTest
{
[NotifyPropertyChanged_Gen]

    public class Cat : WinformTest.INotifier
	{
        #region INotifier Functions	<Gen Renderer='NotifyPropertyChanged' Date='06/02/2014 13:09:22' Class='INotifierFunctions' Ver='1.1.0.17' Mode='OnVersionChanged' xmlns='http://tempuri.org/NotifyPropertyChanged.xsd' />
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        void WinformTest.INotifier.Notify(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
        #region PlaysPiano auto expanded by	<Gen Renderer='NotifyPropertyChanged' Date='06/02/2014 13:09:22' Class='' Ver='1.1.0.17' Mode='Once' xmlns='http://tempuri.org/NotifyPropertyChanged.xsd' />
        private System.Boolean _PlaysPiano;
        public System.Boolean PlaysPiano {
            get {
                return _PlaysPiano;
            }
            set {
                //<Gen Renderer='NotifyPropertyChanged' Date='06/02/2014 13:09:22' Ver='1.1.0.17' Mode='OnVersionChanged' xmlns='http://tempuri.org/NotifyPropertyChanged.xsd'>
                this.SetPropertyAndNotify(ref _PlaysPiano, value, "PlaysPiano");
                //</Gen>
 }
        }
        #endregion


		private int _MaxLife = 9;
		public int MaxLife
		{
			get
			{
				return _MaxLife;
			}
		}

		[NotifyPropertyChanged_Gen(ExtraNotifications="MaxLife")]
		public void Kill()
		{
			_MaxLife -= 1;
            //<Gen Renderer='NotifyPropertyChanged' Date='06/02/2014 13:09:22' ExtraNotifications='MaxLife' Ver='1.1.0.17' Mode='OnVersionChanged' xmlns='http://tempuri.org/NotifyPropertyChanged.xsd'>
            this.NotifyChanged("MaxLife");
            //</Gen>
	}
	}

}