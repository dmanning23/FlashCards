using FontBuddyLib;
using InputHelper;
using MenuBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace FlashCards.Core
{
	/// <summary>
	/// This is a menu entry that can change color depending on whether the answer is wrong or right
	/// </summary>
	public class QuestionLabel : Label
	{
		#region Fields

		private bool _questionAnswered;

		/// <summary>
		/// whether or not this item was chosen as the answer.
		/// </summary>
		private bool _chosenAnswer = false;

		#endregion //Fields

		#region Properties

		/// <summary>
		/// The font to use to draw this dude
		/// </summary>
		private IFontBuddy FontBuddy
		{
			get; set;
		}

		/// <summary>
		/// The current color of this item
		/// </summary>
		private Color CurrentColor
		{
			get; set;
		}

		/// <summary>
		/// Whether or not this is the corerct answer to the question
		/// </summary>
		public bool IsCorrectAnswer { get; set; }

		/// <summary>
		/// if this is the correct answer, this is the color that will be displayed after a selection is made
		/// </summary>
		private Color CorrectColor { get; set; }

		/// <summary>
		/// if this is the wrong answer and is selected, this is the color
		/// </summary>
		private Color WrongColor { get; set; }

		/// <summary>
		/// if this is the correct answer but not selected, this is the color that will be used
		/// </summary>
		private Color CorrectNotSelectedColor { get; set; }

		/// <summary>
		/// If this is the wrong answer but not selected, this is the color that will be used
		/// </summary>
		private Color WrongNotSelectedColor { get; set; }

		public Color? OverrideColor { get; set; }

		/// <summary>
		/// Whether or not the question has been answered.
		/// If this is false, will use the default colors & fonts
		/// if true, will use teh provied colors & fonts
		/// </summary>
		public bool QuestionAnswered
		{
			private get
			{
				return _questionAnswered;
			}
			set
			{
				//set that flag
				_questionAnswered = value;

				if (!_chosenAnswer && QuestionAnswered)
				{
					//set the color based on whether or not this is the correct answer
					if (IsCorrectAnswer)
					{
						CurrentColor = CorrectNotSelectedColor;
					}
					else
					{
						CurrentColor = WrongNotSelectedColor;

						//set the text to a plain vanilla font buddy
						this.Font = FontBuddy;
					}
				}
			}
		}

		#endregion //Properties

		#region Methods

		public QuestionLabel(bool isCorrectAnswer, string text, IFontBuddy font, IFontBuddy highlightedFont)
			: base(text, font, highlightedFont)
		{
			Init(isCorrectAnswer);
		}

		public QuestionLabel(bool isCorrectAnswer, string text, ContentManager content, FontSize fontSize = FontSize.Medium)
			: base(text, content, fontSize)
		{
			Init(isCorrectAnswer);
		}

		private void Init(bool isCorrectAnswer)
		{
			_questionAnswered = false;
			IsCorrectAnswer = isCorrectAnswer;

			//Set the colors
			CorrectColor = new Color(0.0f, 0.7f, 0.0f);
			WrongColor = Color.Red;
			WrongNotSelectedColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);
			CorrectNotSelectedColor = new Color(0.0f, 0.7f, 0.0f, 0.5f);

			//set the current stuff
			CurrentColor = base.GetColor();
			FontBuddy = base.GetFont();
		}

		/// <summary>
		/// This method gets called when this menu entry is selected.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="e"></param>
		public void OnAnswer(object obj, ClickEventArgs e)
		{
			if (!QuestionAnswered)
			{
				_chosenAnswer = true;

				//Set the display color to the correct 
				if (IsCorrectAnswer)
				{
					//Set the font buddy to shaky text
					Font = new PulsateBuddy()
					{
						PulsateSize = 4.0f,
						PulsateSpeed = 12.0f,
						Font = FontBuddy
					};

					CurrentColor = CorrectColor;
				}
				else
				{
					//Set the font buddy to "wrong" text
					Font = new WrongTextBuddy()
					{
						Font = FontBuddy
					};

					CurrentColor = WrongColor;
				}
			}
		}

		protected override Color GetColor()
		{
			if (OverrideColor.HasValue)
			{
				return OverrideColor.Value;
			}
			else if (QuestionAnswered)
			{
				return CurrentColor;
			}
			else
			{
				return base.GetColor();
			}
		}

		#endregion //Methods
	}
}