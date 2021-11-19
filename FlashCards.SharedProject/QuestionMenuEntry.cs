using FontBuddyLib;
using MenuBuddy;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using ResolutionBuddy;
using System;

namespace FlashCards.Core
{
	/// <summary>
	/// This is a menu entry that can change color depending on whether the answer is wrong or right
	/// </summary>
	public class QuestionMenuEntry : MenuEntry
	{
		#region Properties

		public QuestionLabel QuestionLabel => Label as QuestionLabel;

		/// <summary>
		/// Whether or not this is the corerct answer to the question
		/// </summary>
		public bool QuestionAnswered
		{
			set
			{
				QuestionLabel.QuestionAnswered = value;

				if (value)
				{
					//Don't play any more sounds once an item has been selected
					IsQuiet = true;
				}
			}
		}

		private bool CorrectAnswer { get; set; }

		public FlashCard FlashCard { get; private set; }

		public SoundEffect SoundEffect { get; private set; }

		#endregion //Properties

		#region Methods

		public QuestionMenuEntry(string text, FlashCard flashCard, bool correctAnswer, IFontBuddy font)
			: base(text, font)
		{
			Init(flashCard, correctAnswer);
		}

		public QuestionMenuEntry(string text, FlashCard flashCard, bool correctAnswer, ContentManager content)
			: base(text, content)
		{
			Init(flashCard, correctAnswer);
		}

		private void Init(FlashCard flashCard, bool correctAnswer)
		{
			FlashCard = flashCard;
			CorrectAnswer = correctAnswer;
			Label.ShrinkToFit(Resolution.TitleSafeArea.Width);

			QuestionLabel.IsCorrectAnswer = correctAnswer;
			OnClick += QuestionLabel.OnAnswer;
			Highlightable = false;

			//Setting the resource name here will cause the correct sound effect to be loaded an played when this button is clicked
			ClickedSound = correctAnswer ? "CorrectAnswer" : "WrongAnswer";

			if (Rect.Width < QuestionLabel.Rect.Width)
			{
				Size = new Microsoft.Xna.Framework.Vector2(QuestionLabel.Rect.Width, Size.Y);
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

		public void LoadSoundEffect(string language, ContentManager content)
		{
			SoundEffect = FlashCard.LoadSoundEffect(language, content);
		}

		#endregion //Methods
	}
}