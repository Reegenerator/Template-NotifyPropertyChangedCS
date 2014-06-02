
using System;
using System.ComponentModel;
using NotifyPropertyChangedRgen;

namespace WinformTest
{
    [NotifyPropertyChangedRgen.NotifyPropertyChanged_Gen]
    public class Person : WinformTest.INotifier
    {
        #region INotifier Functions	<Gen Renderer='NotifyPropertyChanged' Date='06/02/2014 13:10:36' Class='INotifierFunctions' Ver='1.1.0.17' Mode='OnVersionChanged' xmlns='http://tempuri.org/NotifyPropertyChanged.xsd' />
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        void WinformTest.INotifier.Notify(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
        
        #region FirstName auto expanded by	<Gen Renderer='NotifyPropertyChanged' Date='06/02/2014 12:41:42' Class='' Ver='1.1.0.16' Mode='Once' xmlns='http://tempuri.org/NotifyPropertyChanged.xsd' />
        private System.String _FirstName;
        /// <summary>
        /// Test
        /// </summary>
        [DefaultValueAttribute("")]
        public System.String FirstName {
            get {
                return _FirstName;
            }
            set {

                //<Gen Renderer='NotifyPropertyChanged' Date='06/02/2014 13:10:36' Ver='1.1.0.17' Mode='OnVersionChanged' xmlns='http://tempuri.org/NotifyPropertyChanged.xsd'>
                this.SetPropertyAndNotify(ref _FirstName, value, "FirstName");
                //</Gen>
}
        }
        #endregion


        private string _LastName;
        public string LastName
        {
            get { return ""; }
            set { _LastName = value;
            //<Gen Renderer='NotifyPropertyChanged' Date='06/02/2014 13:10:36' Ver='1.1.0.17' Mode='OnVersionChanged' xmlns='http://tempuri.org/NotifyPropertyChanged.xsd'>
            this.SetPropertyAndNotify(ref _LastName, value, "LastName");
                //</Gen>
}
        }
    }


}