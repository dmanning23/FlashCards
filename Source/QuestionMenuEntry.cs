using FontBuddyLib;
using InputHelper;
using MenuBuddy;
using Microsoft.Xna.Framework;

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
		}

		public override void LoadContent(IScreen screen)
		{
			base.LoadContent(screen);

			_label = Label as QuestionLabel;
			OnClick += _label.OnAnswer;
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