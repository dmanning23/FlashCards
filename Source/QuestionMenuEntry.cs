using System.Runtime.InteropServices.WindowsRuntime;
using MenuBuddy;
using FontBuddyLib;
using Microsoft.Xna.Framework;

namespace FlashCards
{
	/// <summary>
	/// This is a menu entry that can change color depending on whether the answer is wrong or right
	/// </summary>
	public class QuestionMenuEntry : MenuEntry
	{
		#region Fields

		private bool _questionAnswered;

		#endregion //Fields

		#region Properties

		/// <summary>
		/// Whether or not the question has been answered.
		/// If this is false, will use the default colors & fonts
		/// if true, will use teh provied colors & fonts
		/// </summary>
		public bool QuestionAnswered
		{
			get 
			{
				return _questionAnswered; 
			}
			set
			{
				//set that flag
				_questionAnswered = value;

				//set the color based on whether or not this is the correct answer
				if (CorrectAnswer)
				{
					QuestionAnsweredColor = CorrectColor;
				}
				else
				{
					QuestionAnsweredColor = WrongNotSelectedColor;

					//set the text to a plain vanilla font buddy
					var font = new FontBuddy();
					font.Font = UnselectedFont.Font;
					UnselectedFont = font;
				}
			}
		}

		/// <summary>
		/// Whether or not this is the corerct answer to the question
		/// </summary>
		private bool CorrectAnswer { get; set; }

		/// <summary>
		/// if this is the correct answer, this is the color that will be displayed after a selection is made
		/// </summary>
		private Color CorrectColor { get; set; }

		/// <summary>
		/// If this is the wrong answer but not selected, this is the color that will be used
		/// </summary>
		private Color WrongNotSelectedColor { get; set; }

		/// <summary>
		/// if this is the wrong answer and is selected, this is the color
		/// </summary>
		private Color WrongSelectedColor { get; set; }

		/// <summary>
		/// After 
		/// </summary>
		private Color QuestionAnsweredColor { get; set; }

		//TODO: use pulsate for correct answer and BouncyFontBuddy for wrong

		#endregion //Properties

		#region Methods

		public QuestionMenuEntry(string text, bool messageBoxEntry, bool correctAnswer)
			: base(text, messageBoxEntry)
		{
			_questionAnswered = false;
			CorrectAnswer = correctAnswer;

			//Set the colors
			CorrectColor = new Color(0.0f, 0.7f, 0.0f);
			WrongNotSelectedColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);
			WrongSelectedColor = Color.Red;
		}

		//protected override void DrawBackground(MenuScreen screen, Rectangle rect, byte alpha)
		//{
		//	//dont draw the background of an answer has been seelcted
		//	if (!QuestionAnswered)
		//	{
		//		base.DrawBackground(screen, rect, alpha);
		//	}
		//}

		/// <summary>
		/// This method gets called when this menu entry is selected.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="e"></param>
		public void OnSelected(object obj, PlayerIndexEventArgs e)
		{
			//Set the display color to the correct 
			if (CorrectAnswer)
			{
				QuestionAnsweredColor = CorrectColor;
				
				//Set the font buddy to shaky text
				var text = new PulsateBuddy();
				text.PulsateSize = 2.0f;
				text.Font = UnselectedFont.Font;
				UnselectedFont = text;
			}
			else
			{
				QuestionAnsweredColor = WrongSelectedColor;

				//Set the font buddy to "wrong" text
				var text = new WrongTextBuddy();
				text.Font = UnselectedFont.Font;
				UnselectedFont = text;
			}
		}

		protected override void GetTextColors(MenuScreen screen, bool isSelected, byte alpha, out Color color, out Color backgroundColor)
		{
			base.GetTextColors(screen, isSelected, alpha, out color, out backgroundColor);

			//if the question has been answered, use the answer color
			if (QuestionAnswered)
			{
				color = QuestionAnsweredColor;

				if (CorrectAnswer)
				{
					int i = 0;
				}

				//set the alpha color too
				float fAlpha = (color.A / 255.0f);
				color.A = (byte)(alpha * fAlpha);
			}
		}

		#endregion //Methods
	}
}