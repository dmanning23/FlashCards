using System;

namespace FlashCards.Core
{
	public class QuestionEventArgs : EventArgs
    {
		#region Properties

		public bool AnsweredCorrectly { get; set; }

		/// <summary>
		/// The correct answer
		/// </summary>
		public FlashCard CorrectAnswer { get; set; }

		/// <summary>
		/// The answer that the user selected.
		/// If AnsweredCorrectly is true, this will be the same object as CorrectAnswer
		/// </summary>
		public FlashCard SelectedAnswer { get; set; }

		#endregion //Properties

		#region Methods

		public QuestionEventArgs(bool answeredCorrectly, FlashCard correctAnswer, FlashCard selectedAnswer)
		{
			AnsweredCorrectly = answeredCorrectly;
			CorrectAnswer = correctAnswer;
			SelectedAnswer = selectedAnswer;
		}

		#endregion //Methods
	}
}
