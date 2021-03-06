using FontBuddyLib;
using MenuBuddy;
using Microsoft.Xna.Framework.Content;
using ResolutionBuddy;
using System;
using System.Threading.Tasks;

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

		public QuestionMenuEntry(string text, bool correctAnswer, IFontBuddy font)
			: base(text, font)
		{
			Init(correctAnswer);
		}

		public QuestionMenuEntry(string text, bool correctAnswer, ContentManager content)
			: base(text, content)
		{
			Init(correctAnswer);
		}

		private void Init(bool correctAnswer)
		{
			CorrectAnswer = correctAnswer;
			Label.ShrinkToFit(Resolution.TitleSafeArea.Width);

			_label = Label as QuestionLabel;
			_label.IsCorrectAnswer = correctAnswer;
			OnClick += _label.OnAnswer;
			Highlightable = false;

			//Setting the resource name here will cause the correct sound effect to be loaded an played when this button is clicked
			ClickedSound = correctAnswer ? "CorrectAnswer" : "WrongAnswer";

			if (Rect.Width < _label.Rect.Width)
			{
				Size = new Microsoft.Xna.Framework.Vector2(_label.Rect.Width, Size.Y);
			}
		}

		public override Label CreateLabel(ContentManager content)
		{
			return new QuestionLabel(CorrectAnswer, Text, content, FontSize.Medium)
			{
				Vertical = VerticalAlignment.Center,
				Horizontal = HorizontalAlignment.Center
			};
		}

		public override Label CreateLabel(IFontBuddy font, IFontBuddy highlightedFont = null)
		{
			return new QuestionLabel(CorrectAnswer, Text, font, highlightedFont)
			{
				Vertical = VerticalAlignment.Center,
				Horizontal = HorizontalAlignment.Center
			};
		}

		public override Label CreateLabel(Label inst)
		{
			throw new NotImplementedException();
		}

		#endregion //Methods
	}
}