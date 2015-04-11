using GameTimer;
using ListExtensions;
using MenuBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using HadoukInput;

namespace FlashCards
{
	/// <summary>
	/// Used to communicate with the combat engine if the user gave a correct answer
	/// </summary>
	/// <param name="correctAnswer"></param>
	public delegate void AnsweredCorrectly(bool correctAnswer);

	/// <summary>
	/// This is a menu screen that has a thing to translate, and then four possible options.
	/// The user has to choose one of the options, but only one of the answers is correct. 
	/// </summary>
	public class QuestionScreen : MenuScreen
	{
		#region Properties

		/// <summary>
		/// this timer is used to automatically choose an answer
		/// also reused to display the correct ansnwer and exit the screen
		/// </summary>
		private CountdownTimer _autoQuit = new CountdownTimer();

		/// <summary>
		/// method we will call when the user has answered
		/// </summary>
		private AnsweredCorrectly Answered { get; set; }

		/// <summary>
		/// whether or not the player got the question right
		/// </summary>
		public bool AnsweredCorrect { get; private set; }

		/// <summary>
		/// Flag set when the user chooses an answer, either right or wrong
		/// </summary>
		private bool AnswerChosen { get; set; }

		private string CorrectAnswerText { get; set; }

		private List<string> WrongAnswersText { get; set; }

		/// <summary>
		/// the correct answer
		/// </summary>
		private QuestionMenuEntry CorrectAnswerEntry { get; set; }

		Random _rand = new Random();

		#endregion //Properties

		#region Initialization

		/// <summary>
		///	hello, standard constructor!
		/// </summary>
		public QuestionScreen(
			AnsweredCorrectly answered,
			string question,
			string correctAnswer,
			List<string> wrongAnswers)
			: base(question)
		{
			Init(answered, question, correctAnswer, wrongAnswers);
		}

		/// <summary>
		/// setup a question screen from a deck of flash cards
		/// </summary>
		/// <param name="answered"></param>
		/// <param name="cards"></param>
		public QuestionScreen(AnsweredCorrectly answered, Deck cards)
		{
			string question, correctAnswer;
			List<string> wrongAnswers;
			cards.GetQuestion(out question, out correctAnswer, out wrongAnswers);
			Init(answered, question, correctAnswer, wrongAnswers);
		}

		private void Init(AnsweredCorrectly answered,
			string question,
			string correctAnswer,
			List<string> wrongAnswers)
		{
			Debug.Assert(null != answered);
			Debug.Assert(null != wrongAnswers);

			MenuTitleOffset = new Point(0, -150);
			MenuEntryOffset = new Point(0, -85);

			ScreenName = question;

			//this screen should transition on really slow for effect
			Transition.OnTime = TimeSpan.FromSeconds(0.5f);

			//set up the question
			CorrectAnswerText = correctAnswer;
			WrongAnswersText = wrongAnswers;
			Answered = answered;
			AnsweredCorrect = false;
			AnswerChosen = false;
		}

		public override void LoadContent()
		{
			base.LoadContent();

			//create a style for the question entries
			var style = new StyleSheet(ScreenManager.Styles.MenuEntryStyle);
			style.Transition = TransitionType.PopBottom;

			//store a temp list of all the entries
			var entries = new List<QuestionMenuEntry>();

			//create the correct menu entry
			CorrectAnswerEntry = new QuestionMenuEntry(style, CorrectAnswerText, true);
			CorrectAnswerEntry.Selected += CorrectAnswerSelected;
			CorrectAnswerEntry.Selected += CorrectAnswerEntry.OnSelected;
			entries.Add(CorrectAnswerEntry);

			//Add exactly three wrong answers
			Debug.Assert(3 <= WrongAnswersText.Count);
			for (int i = 0; i < 3; i++)
			{
				//get a random wrong answer
				int index = _rand.Next(WrongAnswersText.Count);

				//create a menu entry for that answer
				var wrongMenuEntry = new QuestionMenuEntry(style, WrongAnswersText[index], false);
				wrongMenuEntry.Selected += WrongAnswerSelected;
				wrongMenuEntry.Selected += wrongMenuEntry.OnSelected;
				entries.Add(wrongMenuEntry);

				//remove the wrong answer from the list so it wont be added again
				WrongAnswersText.RemoveAt(index);
			}

			//shuffle the answers
			entries.Shuffle(_rand);

			//add all the question entries to the menu
			foreach (var entry in entries)
			{
				AddMenuEntry(entry);
			}

			//make the player stare at this screen for 2 seconds before they can quit
			_autoQuit.Start(3.0f);
		}

		#endregion //Initialization

		#region Handle Input

		///// <summary>
		///// Responds to user input, changing the selected entry and accepting
		///// or cancelling the menu.
		///// </summary>
		//public override void HandleInput(InputState input, GameTime rGameTime)
		//{
		//	if (!AnswerChosen)
		//	{
		//		base.HandleInput(input, rGameTime);
		//	}
		//}

		/// <summary>
		/// Event handler for when the High Scores menu entry is selected.
		/// </summary>
		private void CorrectAnswerSelected(object sender, PlayerIndexEventArgs e)
		{
			AnswerSelected(true);
		}

		/// <summary>
		/// Event handler for when the High Scores menu entry is selected.
		/// </summary>
		private void WrongAnswerSelected(object sender, PlayerIndexEventArgs e)
		{
			AnswerSelected(false);
		}

		#endregion //Handle Input

		#region Methods

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			//update the timers
			_autoQuit.Update(gameTime);

			//check if we been here long enough
			if (0.0f >= _autoQuit.RemainingTime())
			{
				//has the user picked an answer?
				if (!AnswerChosen)
				{
					//the timer ran out but the user hadn't picked an answer.  That counts as "wrong"
					AnswerSelected(false);
				}
				else
				{
					//holla at the combat engine
					ExitScreen();
				}
			}

			base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		}

		/// <summary>
		/// The user selected an answer.
		/// Set the flags, menu entry colors, and start timer.
		/// </summary>
		private void AnswerSelected(bool correctAnswer)
		{
			if (!AnswerChosen)
			{
				//set flags
				AnswerChosen = true;
				AnsweredCorrect = correctAnswer;

				//play the appropriate sound effect
				if (AnsweredCorrect)
				{
					//TODO: play "KA-CHING" sound effect
				}
				else
				{
					//TODO: play "BOI-OING" sound effect
				}

				//Set all the colors of the answers to let the user know which was the correct answer
				foreach (var entry in MenuEntries.Items)
				{
					var questionEntry = entry as QuestionMenuEntry;
					if (null != questionEntry)
					{
						questionEntry.QuestionAnswered = true;
					}
				}

				Answered(AnsweredCorrect);

				//start the timer to exit this screen
				_autoQuit.Start(0.75f);
			}
		}

		public override void OnCancel(PlayerIndex? playerIndex)
		{
			//Do nothing if the user cancels a question screen
		}

		public override void Draw(GameTime gameTime)
		{
			ScreenManager.SpriteBatchBegin();

			// Darken down any other screens that were drawn beneath the popup.
			FadeBackground();

			ScreenManager.SpriteBatchEnd();

			base.Draw(gameTime);
		}

		#endregion //Methods
	}
}