
using System;
using System.ComponentModel;
using NotifyPropertyChangedRgen;

namespace WinformTest {
    [NotifyPropertyChangedRgen.NotifyPropertyChanged_Gen]
    public class Person : WinformTest.INotifier {
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
        [NotifyPropertyChanged_Gen(ExtraNotifications = "Name")]
        public System.String FirstName {
            get {
                return _FirstName;
            }
            set {

                //<Gen Renderer='NotifyPropertyChanged' Date='06/04/2014 12:05:36' ExtraNotifications='Name' Ver='1.1.0.17' Mode='OnVersionChanged' xmlns='http://tempuri.org/NotifyPropertyChanged.xsd'>
                this.SetPropertyAndNotify(ref _FirstName, value, "FirstName");
                this.NotifyChanged("Name");
                //</Gen>
            }
        }
        #endregion

        [NotifyPropertyChanged_Gen(IsIgnored = true)]
        public string LastName {
            get { return _LastName; }
            set { _LastName = value; }
        }

        #region Age auto expanded by	<Gen Renderer='NotifyPropertyChanged' Date='06/04/2014 11:27:31' Class='' Ver='1.1.0.17' Mode='Once' xmlns='http://tempuri.org/NotifyPropertyChanged.xsd' />
        private int _Age;
        [NotifyPropertyChanged_Gen(ExtraNotifications = "AgeString")]
        public int Age {
            get {
                return _Age;
            }
set {
    if (value > 0) {
        //<Gen Renderer='NotifyPropertyChanged' Type='InsertPoint'/>
        //<Gen Renderer='NotifyPropertyChanged' Date='06/04/2014 12:41:24' ExtraNotifications='AgeString' Ver='1.1.0.17' Mode='OnVersionChanged' xmlns='http://tempuri.org/NotifyPropertyChanged.xsd'>
        this.SetPropertyAndNotify(ref _Age, value, "Age");
        this.NotifyChanged("AgeString");
        //</Gen>

    }

}
        }
        #endregion


        public string Name {
            get { return string.Format("{0} {1}", FirstName, LastName); }
        }

        #region Address auto expanded by	<Gen Renderer='NotifyPropertyChanged' Date='06/04/2014 11:26:59' Class='' Ver='1.1.0.17' Mode='Once' xmlns='http://tempuri.org/NotifyPropertyChanged.xsd' />
        private System.String _Address;
        private string _LastName;

        public System.String Address {
            get {
                return _Address;
            }
            set {
                //<Gen Renderer='NotifyPropertyChanged' Date='06/04/2014 11:26:59' Ver='1.1.0.17' Mode='OnVersionChanged' xmlns='http://tempuri.org/NotifyPropertyChanged.xsd'>
                this.SetPropertyAndNotify(ref _Address, value, "Address");
                //</Gen>
            }
        }
        #endregion

        [NotifyPropertyChanged_Gen(ExtraNotifications = "LastName")]
        public void ChangeLastName(string s) {

            _LastName = s;
            //<Gen Renderer='NotifyPropertyChanged' Date='06/04/2014 11:45:19' ExtraNotifications='LastName' Ver='1.1.0.17' Mode='OnVersionChanged' xmlns='http://tempuri.org/NotifyPropertyChanged.xsd'>
            this.NotifyChanged("LastName");
            //</Gen>
        }


        public string AgeString {
            get { return string.Format("{0} years old", Age); }
        }
    }



}