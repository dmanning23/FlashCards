using System;
using System.Collections.Generic;
using System.Text;

namespace FlashCards
{
    public class QuestionEventArgs : EventArgs
    {
		#region Properties

		public bool AnsweredCorrectly { get; set; }

		#endregion //Properties

		#region Methods

		public QuestionEventArgs(bool answeredCorrectly)
		{
			AnsweredCorrectly = answeredCorrectly;
		}

		#endregion //Methods
	}
}
