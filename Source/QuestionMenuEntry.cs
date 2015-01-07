using MenuBuddy;
using Microsoft.Xna.Framework;

namespace FlashCards
{
	/// <summary>
	/// This is a menu entry that can change color depending on whether the answer is wrong or right
	/// </summary>
	public class QuestionMenuEntry : MenuEntry
	{
		#region Properties

		/// <summary>
		/// Whether or not the question has been answered.
		/// If this is false, will use the default colors & fonts
		/// if true, will use teh provied colors & fonts
		/// </summary>
		public bool QuestionAnswered { get; set; }

		/// <summary>
		/// The color to display this menu entry after the question has been answered
		/// </summary>
		public Color AnsweredColor { get; set; }

		//TODO: use pulsate for correct answer and BouncyFontBuddy for wrong

		#endregion //Properties

		#region Methods

		public QuestionMenuEntry(string text, bool messageBoxEntry, bool correctAnswer)
			: base(text, messageBoxEntry)
		{
			QuestionAnswered = false;

			//set the answer color based on whether or not this is the right answer
			if (correctAnswer)
			{
				AnsweredColor = new Color(0.0f, 0.7f, 0.0f);
			}
			else
			{
				AnsweredColor = Color.Red;
			}
		}

		protected override void GetTextColors(MenuScreen screen, bool isSelected, byte alpha, out Color color, out Color backgroundColor)
		{
			base.GetTextColors(screen, isSelected, alpha, out color, out backgroundColor);

			//if the question has been answered, use the answer color
			if (QuestionAnswered)
			{
				color = AnsweredColor;
				color.A = alpha;
			}
		}

		#endregion //Methods
	}
}