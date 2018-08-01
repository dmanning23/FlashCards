using MenuBuddy;
using Microsoft.Xna.Framework.Content;
using ResolutionBuddy;

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

		public QuestionMenuEntry(string text, bool correctAnswer, ContentManager content)
			: base(text, content)
		{
			CorrectAnswer = correctAnswer;
			Label = CreateLabel(content);
			Label.ShrinkToFit(Resolution.TitleSafeArea.Width);

			_label = Label as QuestionLabel;
			OnClick += _label.OnAnswer;
			Highlightable = false;

			//Setting the resource name here will cause the correct sound effect to be loaded an played when this button is clicked
			ClickedSound = correctAnswer ? "CorrectAnswer" : "WrongAnswer";
		}

		public override void LoadContent(IScreen screen)
		{
			base.LoadContent(screen);

			if (Rect.Width < _label.Rect.Width)
			{
				Size = new Microsoft.Xna.Framework.Vector2(_label.Rect.Width, Size.Y);
			}
		}

		protected Label CreateLabel(ContentManager content)
		{
			return new QuestionLabel(CorrectAnswer, Text, content)
			{
				Vertical = VerticalAlignment.Center,
				Horizontal = HorizontalAlignment.Center
			};
		}

		#endregion //Methods
	}
}