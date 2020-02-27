using GameTimer;
using InputHelper;
using LifeBarBuddy;
using ListExtensions;
using MenuBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using ResolutionBuddy;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

		private Random _rand = new Random();

		/// <summary>
		/// method we will call when the user has answered
		/// </summary>
		public event EventHandler<QuestionEventArgs> QuestionAnswered;

		/// <summary>
		/// whether or not the player got the question right
		/// </summary>
		public bool AnsweredCorrect { get; private set; }

		/// <summary>
		/// Flag set when the user chooses an answer, either right or wrong
		/// </summary>
		private bool AnswerChosen { get; set; }

		protected string CorrectAnswerText { get; set; }

		private List<string> WrongAnswersText { get; set; }

		/// <summary>
		/// the correct answer
		/// </summary>
		private QuestionMenuEntry CorrectAnswerEntry { get; set; }

		/// <summary>
		/// How long the user has to answer a question before it is automatically marked as "wrong"
		/// </summary>
		public float QuestionTime { get; set; }

		private SoundEffect WrongAnswerSound { get; set; }

		protected Deck Deck { get; set; }

		private bool TimeRanOut { get; set; }

		ITimerMeter CountdownClock { get; set; }

		IMeterRenderer meterRenderer;

		IScreen TimerScreen { get; set; }

		protected bool FlipQuestion { get; set; }

		#endregion //Properties

		#region Initialization

		/// <summary>
		///	hello, standard constructor!
		/// </summary>
		public QuestionScreen(string question, string correctAnswer, List<string> wrongAnswers, ContentManager content = null, bool flipQuestion = false) :
				base(question, content)
		{
			Init(question, correctAnswer, wrongAnswers, flipQuestion);
		}

		/// <summary>
		/// setup a question screen from a deck of flash cards
		/// </summary>
		/// <param name="cards"></param> 
		public QuestionScreen(Deck cards, ContentManager content = null, bool flipQuestion = false) :
			base("", content)
		{
			Deck = cards;

			string question, correctAnswer;
			List<string> wrongAnswers;
			cards.GetQuestion(out question, out correctAnswer, out wrongAnswers, flipQuestion);
			Init(question, correctAnswer, wrongAnswers, flipQuestion);
		}

		private void Init(string question,
			string correctAnswer,
			List<string> wrongAnswers,
			bool flipQuestion)
		{
			QuestionTime = 6f;
			ScreenName = question;
			TimeRanOut = false;

			//this screen should transition on really slow for effect
			Transition.OnTime = 0.5f;

			//set up the question
			CorrectAnswerText = correctAnswer;
			WrongAnswersText = wrongAnswers;
			AnsweredCorrect = false;
			AnswerChosen = false;
			FlipQuestion = flipQuestion;
		}

		public override async Task LoadContent()
		{
			await base.LoadContent();

			if (null != Deck)
			{
				//Add the text asking a question
				var question = new Label($"What is the {(!FlipQuestion ? Deck.TranslationLanguage : Deck.PrimaryLanguage)} word for", Content, FontSize.Small)
				{
					Vertical = VerticalAlignment.Bottom,
					Horizontal = HorizontalAlignment.Center,
					Highlightable = false,
					TransitionObject = new WipeTransitionObject(TransitionWipeType.PopTop),
					Position = new Point(Resolution.ScreenArea.Center.X, MenuTitle.Rect.Top - 16),
				};
				AddItem(question);
			}

			//store a temp list of all the entries
			var entries = new List<QuestionMenuEntry>();

			//create the correct menu entry
			CorrectAnswerEntry = new QuestionMenuEntry(CorrectAnswerText, true, Content)
			{
				TransitionObject = new WipeTransitionObject(TransitionWipeType.PopBottom),
			};
			CorrectAnswerEntry.OnClick += CorrectAnswerSelected;
			entries.Add(CorrectAnswerEntry);

			//Add exactly three wrong answers
			for (int i = 0; i < 3; i++)
			{
				//get a random wrong answer
				int index = _rand.Next(WrongAnswersText.Count);

				//create a menu entry for that answer
				var wrongMenuEntry = new QuestionMenuEntry(WrongAnswersText[index], false, Content)
				{
					TransitionObject = new WipeTransitionObject(TransitionWipeType.PopBottom)
				};
				wrongMenuEntry.OnClick += WrongAnswerSelected;
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

			//load the sound effect to play when time runs out on a question
			WrongAnswerSound = Content.Load<SoundEffect>("WrongAnswer");

			meterRenderer = new MeterRenderer(Content, "MeterShader.fx");

			var timerRect = new Rectangle(0, 0, 128, 128);

			CountdownClock = new TimerMeter(QuestionTime, Content, "TimerBackground.png", "TimerMeter.png", "TimerGradient.png", timerRect)
			{
				NearEndTime = QuestionTime * 0.5f,
			};

			//add the meter screen
			TimerScreen = new MeterScreen(CountdownClock,
				new Point((int)Resolution.TitleSafeArea.Left, (int)Resolution.TitleSafeArea.Top),
				TransitionWipeType.PopLeft,
				Content,
				VerticalAlignment.Top,
				HorizontalAlignment.Left);

			await ScreenManager.AddScreen(TimerScreen);

			//make the player stare at this screen for 2 seconds before they can quit
			_autoQuit.Start(QuestionTime);
			CountdownClock.Reset();
		}

		public override void ExitScreen()
		{
			base.ExitScreen();
			TimerScreen.ExitScreen();
		}

		public override void Dispose()
		{
			base.Dispose();

			QuestionAnswered = null;
		}

		#endregion //Initialization

		#region Handle Input

		/// <summary>
		/// Event handler for when the High Scores menu entry is selected.
		/// </summary>
		private void CorrectAnswerSelected(object sender, ClickEventArgs e)
		{
			AnswerSelected(true);
		}

		/// <summary>
		/// Event handler for when the High Scores menu entry is selected.
		/// </summary>
		private void WrongAnswerSelected(object sender, ClickEventArgs e)
		{
			AnswerSelected(false);
		}

		#endregion //Handle Input

		#region Methods

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			//update the timers
			_autoQuit.Update(gameTime);

			if (null != CountdownClock)
			{
				CountdownClock.Update(gameTime);
			}

			//check if we been here long enough
			if (IsActive)
			{
				if (!_autoQuit.HasTimeRemaining)
				{
					//has the user picked an answer?
					if (!AnswerChosen)
					{
						//the timer ran out but the user hadn't picked an answer.  That counts as "wrong"
						AnswerSelected(false);

						//play the "wrong" sound effect
						WrongAnswerSound.Play();

						TimeRanOut = true;
					}
					else
					{
						//holla at the combat engine
						ExitScreen();
					}
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

				//Set all the colors of the answers to let the user know which was the correct answer
				foreach (var entry in MenuEntries.Items)
				{
					var questionEntry = entry as QuestionMenuEntry;
					if (null != questionEntry)
					{
						questionEntry.QuestionAnswered = true;
					}
				}

				if (null != QuestionAnswered)
				{
					QuestionAnswered(this, new QuestionEventArgs(AnsweredCorrect));
				}

				//start the timer to exit this screen
				_autoQuit.Start(1f);
				TimerScreen.ExitScreen();
			}
		}

		public override void Cancelled(object obj, ClickEventArgs e)
		{
			//Do nothing if the user cancels a question screen
		}

		public override void Draw(GameTime gameTime)
		{
			ScreenManager.SpriteBatchBegin();
			FadeBackground(0.2f);
			ScreenManager.SpriteBatchEnd();

			base.Draw(gameTime);

			//draw the meters
			meterRenderer.Alpha = Transition.Alpha;
			meterRenderer.SpriteBatchBegin(ScreenManager.SpriteBatch, Resolution.TransformationMatrix());
			if (!AnswerChosen)
			{
				CountdownClock.Draw(_autoQuit.RemainingTime, meterRenderer, ScreenManager.SpriteBatch);
			}
			else if (TimeRanOut)
			{
				CountdownClock.Draw(0f, meterRenderer, ScreenManager.SpriteBatch);
			}
			else
			{
				CountdownClock.Draw(QuestionTime, meterRenderer, ScreenManager.SpriteBatch);
			}
			ScreenManager.SpriteBatch.End();
		}

		#endregion //Methods
	}
}