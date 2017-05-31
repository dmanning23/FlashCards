using MenuBuddy;

namespace FlashCards
{
	/// <summary>
	/// This is a menu entry that can change color depending on whether the answer is wrong or right
	/// </summary>
	public class QuestionMenuEntry : MenuEntry
	{
		#region Fields

		private QuestionLabel _label;

		#endregion //Fields

		#region Properties

		/// <summary>
		/// Whether or not this is the corerct answer to the question
		/// </summary>
		public bool QuestionAnswered
		{
			set
			{
				_label.QuestionAnswered = value;

				if (value)
				{
					//Don't play any more sounds once an item has been selected
					IsQuiet = true;
				}
			}
		}

		private bool CorrectAnswer { get; set; }

		#endregion //Properties

		#region Methods

		public QuestionMenuEntry(string text, bool correctAnswer)
			: base(text)
		{
			CorrectAnswer = correctAnswer;
			Label = CreateLabel();

			_label = Label as QuestionLabel;
			OnClick += _label.OnAnswer;
			Highlightable = false;

			//Setting the resource name here will cause the correct sound effect to be loaded an played when this button is clicked
			ClickedSound = correctAnswer ? "CorrectAnswer" : "WrongAnswer";
		}

		protected override Label CreateLabel()
		{
			return new QuestionLabel(CorrectAnswer, Text)
			{
				Vertical = VerticalAlignment.Top,
				Horizontal = HorizontalAlignment.Center
			};
		}

		#endregion //Methods
	}
}