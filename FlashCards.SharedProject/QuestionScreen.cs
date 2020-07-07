using FontBuddyLib;
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
	public class QuestionScreen : WidgetScreen
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

		FlashCard question;

		protected FlashCard Question => question;

		Translation correctAnswer;

		protected Translation CorrectAnswer => correctAnswer;

		List<Translation> wrongAnswers;

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

		List<QuestionMenuEntry> Entries { get; set; } = new List<QuestionMenuEntry>();

		IFontBuddy fontSmall;
		IFontBuddy fontMedium;

		#endregion //Properties

		#region Initialization

		/// <summary>
		/// setup a question screen from a deck of flash cards
		/// </summary>
		/// <param name="cards"></param> 
		public QuestionScreen(Deck cards, ContentManager content = null, IFontBuddy fontSmall = null, IFontBuddy fontMedium = null) :
			base("", content)
		{
			this.fontSmall = fontSmall;
			this.fontMedium = fontMedium;

			Deck = cards;
			QuestionTime = 6f;
			CoveredByOtherScreens = false;
			CoverOtherScreens = true;

			//this screen should transition on really slow for effect
			Transition.OnTime = 0.5f;

			//set up the question
			AnsweredCorrect = false;
			AnswerChosen = false;
			TimeRanOut = false;

			cards.GetQuestion(out question, out correctAnswer, out wrongAnswers);
		}

		public override async Task LoadContent()
		{
			try
			{
				await base.LoadContent();

				if (null == fontSmall)
				{
					fontSmall = new FontBuddyPlus();
					fontSmall.LoadContent(Content, StyleSheet.SmallFontResource, StyleSheet.UseFontPlus, StyleSheet.SmallFontSize);
				}

				if (null == fontMedium)
				{
					fontMedium = new FontBuddyPlus();
					fontMedium.LoadContent(Content, StyleSheet.MediumFontResource, StyleSheet.UseFontPlus, StyleSheet.MediumFontSize);
				}

				//create the stack layout to hold the question and words
				var questionStack = new StackLayout(StackAlignment.Top)
				{
					Vertical = VerticalAlignment.Bottom,
					Horizontal = HorizontalAlignment.Center,
					Highlightable = false,
					TransitionObject = new WipeTransitionObject(TransitionWipeType.PopTop),
				};

				//Add the text asking a question
				questionStack.AddItem(new Label($"What is the", fontSmall)
				{
					Vertical = VerticalAlignment.Center,
					Horizontal = HorizontalAlignment.Center,
					Highlightable = false,
					TransitionObject = new WipeTransitionObject(TransitionWipeType.PopTop),
				});
				questionStack.AddItem(new Shim(0, 8));
				questionStack.AddItem(new Label($"{correctAnswer.Language} for:", fontSmall)
				{
					Vertical = VerticalAlignment.Center,
					Horizontal = HorizontalAlignment.Center,
					Highlightable = false,
					TransitionObject = new WipeTransitionObject(TransitionWipeType.PopTop),
				});
				questionStack.AddItem(new Shim(0, 8));

				//Add all the translations
				foreach (var translation in question.Translations)
				{
					if (translation.Language != correctAnswer.Language)
					{
						CreateTranslationLabel(fontMedium, questionStack, translation);
					}
				}

				questionStack.Position = new Point(Resolution.ScreenArea.Center.X, (int)((Resolution.TitleSafeArea.Height * 0.2f) - (questionStack.Rect.Height * 0.4f)));
				AddItem(questionStack);

				//create the correct menu entry
				CorrectAnswerEntry = CreateQuestionMenuEntry(correctAnswer.Word, true, fontMedium);
				CorrectAnswerEntry.OnClick += CorrectAnswerSelected;
				Entries.Add(CorrectAnswerEntry);

				//Add exactly three wrong answers
				for (int i = 0; i < 3; i++)
				{
					//get a random wrong answer
					int index = _rand.Next(wrongAnswers.Count);

					//create a menu entry for that answer
					var wrongMenuEntry = CreateQuestionMenuEntry(wrongAnswers[index].Word, false, fontMedium);
					wrongMenuEntry.OnClick += WrongAnswerSelected;
					Entries.Add(wrongMenuEntry);

					//remove the wrong answer from the list so it wont be added again
					wrongAnswers.RemoveAt(index);
				}

				//shuffle the answers
				Entries.Shuffle(_rand);

				//create the stack layout to hold the possible answers
				var answersStack = new StackLayout(StackAlignment.Top)
				{
					Name = "AnswerStack",
					Vertical = VerticalAlignment.Center,
					Horizontal = HorizontalAlignment.Center,
					Highlightable = false,
					TransitionObject = new WipeTransitionObject(TransitionWipeType.PopBottom),
					Position = new Point(Resolution.ScreenArea.Center.X, (int)(Resolution.TitleSafeArea.Height * 0.5f))
				};

				//add all the question entries to the menu
				foreach (var entry in Entries)
				{
					answersStack.AddItem(entry);
				}
				AddItem(answersStack);

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
			catch (Exception ex)
			{
				await ScreenManager.ErrorScreen(ex);
			}
		}

		private void CreateTranslationLabel(IFontBuddy font, StackLayout questionStack, Translation translation)
		{
			try
			{
				var translationLabel = new Label(translation.Word, font)
				{
					Vertical = VerticalAlignment.Center,
					Horizontal = HorizontalAlignment.Center,
					Highlightable = false,
					TransitionObject = new WipeTransitionObject(TransitionWipeType.PopTop),
					Scale = 1.2f,
				};
				translationLabel.ShrinkToFit(Resolution.TitleSafeArea.Width);
				questionStack.AddItem(translationLabel);
				questionStack.AddItem(new Shim(0, 8));
			}
			catch (Exception ex)
			{
				ScreenManager.ErrorScreen(ex);
				throw new Exception($"Error creating menu entry for {translation.Word}", ex);
			}
		}

		protected virtual QuestionMenuEntry CreateQuestionMenuEntry(string text, bool correctAnswer, ContentManager content)
		{
			return new QuestionMenuEntry(text, correctAnswer, content)
			{
				TransitionObject = new WipeTransitionObject(TransitionWipeType.PopBottom),
			};
		}

		protected virtual QuestionMenuEntry CreateQuestionMenuEntry(string text, bool correctAnswer, IFontBuddy font)
		{
			try
			{
				return new QuestionMenuEntry(text, correctAnswer, font)
				{
					TransitionObject = new WipeTransitionObject(TransitionWipeType.PopBottom),
				};
			}
			catch (Exception ex)
			{
				ScreenManager.ErrorScreen(ex);
				throw new Exception($"Error creating menu entry for {text}", ex);
			}
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
				foreach (var entry in Entries)
				{
					entry.QuestionAnswered = true;
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