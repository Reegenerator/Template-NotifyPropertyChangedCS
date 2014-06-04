
using System.ComponentModel;
using NotifyPropertyChangedRgen;
namespace WinformTest
{
    //Uncomment the line below and save the file to see the generated code
//[NotifyPropertyChanged_Gen]
    public class Cat 
	{

        public System.Boolean PlaysPiano { get; set; }

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

	}
	}

}