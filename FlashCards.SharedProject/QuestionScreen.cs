using FontBuddyLib;
using GameTimer;
using InputHelper;
using ListExtensions;
using MenuBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using ResolutionBuddy;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlashCards.Core
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
		public CountdownTimer AutoQuit { get; set; } = new CountdownTimer();

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
		public bool AnswerChosen { get; private set; }

		private FlashCard correctQuestion;

		protected FlashCard CorrectQuestion => correctQuestion;

		private Translation correctAnswer;

		protected Translation CorrectAnswer => correctAnswer;

		private List<FlashCard> wrongQuestions;

		private List<Translation> wrongAnswers;

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

		public bool TimeRanOut { get; private set; }

		public List<QuestionMenuEntry> Entries { get; private set; } = new List<QuestionMenuEntry>();

		protected IFontBuddy FontSmall { get; private set; }
		protected IFontBuddy FontMedium { get; private set; }

		OverlayScreen OverlayScreen { get; set; }

		/// <summary>
		/// The sound effect that says "What is the {language} word for..."
		/// </summary>
		SoundEffect QuestionSoundEffect { get; set; }

		public SoundEffect QuestionWordSoundEffect { get; private set; }

		public QuestionLabel QuestionWordLabel { get; private set; }

		public QuestionStateMachine QuestionStateMachine { get; private set; }

		public float SoundVolume { get; set; } = 1f;

		#endregion //Properties

		#region Initialization

		/// <summary>
		/// setup a question screen from a deck of flash cards
		/// </summary>
		/// <param name="cards"></param> 
		public QuestionScreen(Deck cards, ContentManager content = null, IFontBuddy fontSmall = null, IFontBuddy fontMedium = null) :
			base("", content)
		{
			this.FontSmall = fontSmall;
			this.FontMedium = fontMedium;

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

			cards.GetQuestion(out correctQuestion, out correctAnswer, out wrongQuestions, out wrongAnswers);
		}

		public override async Task LoadContent()
		{
			try
			{
				await base.LoadContent();

				if (null == FontSmall)
				{
					FontSmall = new FontBuddyPlus();
					FontSmall.LoadContent(Content, StyleSheet.SmallFontResource, StyleSheet.UseFontPlus, StyleSheet.SmallFontSize);
				}

				if (null == FontMedium)
				{
					FontMedium = new FontBuddyPlus();
					FontMedium.LoadContent(Content, StyleSheet.MediumFontResource, StyleSheet.UseFontPlus, StyleSheet.MediumFontSize);
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
				questionStack.AddItem(new Label($"What is the", FontSmall)
				{
					Vertical = VerticalAlignment.Center,
					Horizontal = HorizontalAlignment.Center,
					Highlightable = false,
					TransitionObject = new WipeTransitionObject(TransitionWipeType.PopTop),
				});
				questionStack.AddItem(new Shim(0, 8));
				questionStack.AddItem(new Label($"{correctAnswer.Language} for:", FontSmall)
				{
					Vertical = VerticalAlignment.Center,
					Horizontal = HorizontalAlignment.Center,
					Highlightable = false,
					TransitionObject = new WipeTransitionObject(TransitionWipeType.PopTop),
				});
				questionStack.AddItem(new Shim(0, 8));

				//Add all the translations
				foreach (var translation in correctQuestion.Translations)
				{
					if (translation.Language != correctAnswer.Language)
					{
						CreateTranslationLabel(FontMedium, questionStack, translation);
					}
				}

				questionStack.Position = new Point(Resolution.ScreenArea.Center.X, (int)((Resolution.TitleSafeArea.Height * 0.2f) - (questionStack.Rect.Height * 0.4f)));
				AddItem(questionStack);

				//create the correct menu entry
				CorrectAnswerEntry = CreateQuestionMenuEntry(correctAnswer.Word, correctQuestion, true, FontMedium);
				CorrectAnswerEntry.OnClick += CorrectAnswerSelected;
				Entries.Add(CorrectAnswerEntry);

				//Add exactly three wrong answers
				for (int i = 0; i < 3; i++)
				{
					//get a random wrong answer
					int index = _rand.Next(wrongAnswers.Count);

					//create a menu entry for that answer
					var wrongMenuEntry = CreateQuestionMenuEntry(wrongAnswers[index].Word, wrongQuestions[index], false, FontMedium);
					wrongMenuEntry.OnClick += WrongAnswerSelected;
					Entries.Add(wrongMenuEntry);

					//remove the wrong answer from the list so it wont be added again
					wrongAnswers.RemoveAt(index);
					wrongQuestions.RemoveAt(index);
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

				AutoQuit.Stop();
			}
			catch (Exception ex)
			{
				await ScreenManager.ErrorScreen(ex);
			}

			//Try to load the sound effects
			try
			{
				QuestionSoundEffect = Content.Load<SoundEffect>($"TTS//{correctAnswer.Language}Question");
			}
			catch (Exception ex)
			{
				//ignore for now
			}

			//load the correct answer sound effect
			try
			{
				QuestionWordSoundEffect = CorrectQuestion.LoadSoundEffect(correctQuestion.OtherLanguage(correctAnswer.Language), Content);
			}
			catch (Exception ex)
			{
				//ignore for now
			}

			//load the sound effect for each question
			for (int i = 0; i < Entries.Count; i++)
			{
				try
				{
					Entries[i].LoadSoundEffect(correctAnswer.Language, Content);
				}
				catch (Exception ex)
				{
					//ignore for now
				}
			}

			QuestionStateMachine = new QuestionStateMachine();
			QuestionStateMachine.StartTimer(Transition.OnTime + 0.1f);
			QuestionStateMachine.StateChangedEvent += QuestionStateMachine_StateChangedEvent;
		}

		private void QuestionStateMachine_StateChangedEvent(object sender, StateMachineBuddy.StateChangeEventArgs e)
		{
			switch (e.NewState)
			{
				case (int)QuestionStateMachine.QuestionState.AskingQuestion:
					{
						//play the sound that asks the question
						PlaySoundEffect(QuestionSoundEffect);
					}
					break;
				case (int)QuestionStateMachine.QuestionState.QuestionWord:
					{
						//Make sure all the colors are rest
						ResetColors();

						//Set the color of the word being spoken to yellow
						SetLabelOverride(QuestionWordLabel);

						//play the sound that asks the question
						PlaySoundEffect(QuestionWordSoundEffect, 0.4f);
					}
					break;
				case (int)QuestionStateMachine.QuestionState.FirstAnswer:
					{
						ResetColors();
						SetLabelOverride(Entries[0].QuestionLabel);
						PlaySoundEffect(Entries[0].SoundEffect, 0.2f);
					}
					break;
				case (int)QuestionStateMachine.QuestionState.SecondAnswer:
					{
						ResetColors();
						SetLabelOverride(Entries[1].QuestionLabel);
						PlaySoundEffect(Entries[1].SoundEffect, 0.2f);
					}
					break;
				case (int)QuestionStateMachine.QuestionState.ThirdAnswer:
					{
						ResetColors();
						SetLabelOverride(Entries[2].QuestionLabel);
						PlaySoundEffect(Entries[2].SoundEffect, 0.2f);
					}
					break;
				case (int)QuestionStateMachine.QuestionState.FourthAnswer:
					{
						ResetColors();
						SetLabelOverride(Entries[3].QuestionLabel);
						PlaySoundEffect(Entries[3].SoundEffect, 0.2f);
					}
					break;
				case (int)QuestionStateMachine.QuestionState.Done:
					{
						ResetColors();

						//If the user hasn't answered the question, add the overlay screen
						if (!AnswerChosen)
						{
							if (null == OverlayScreen)
							{
								OverlayScreen = new OverlayScreen(this, Content);
								ScreenManager.AddScreen(OverlayScreen);
							}
						}
					}
					break;
			}
		}

		public void PlaySoundEffect(SoundEffect soundEffect, float pause = 0.1f)
		{
			if (null != soundEffect)
			{
				QuestionStateMachine.StartTimer((float)soundEffect.Duration.TotalSeconds + pause);
				soundEffect.Play(SoundVolume, 0f, 0f);
			}
		}

		private void CreateTranslationLabel(IFontBuddy font, StackLayout questionStack, Translation translation)
		{
			try
			{
				QuestionWordLabel = new QuestionLabel(false, translation.Word, font, font)
				{
					Vertical = VerticalAlignment.Center,
					Horizontal = HorizontalAlignment.Center,
					Highlightable = false,
					TransitionObject = new WipeTransitionObject(TransitionWipeType.PopTop),
					Scale = 1.2f,
				};
				QuestionWordLabel.ShrinkToFit(Resolution.TitleSafeArea.Width);
				questionStack.AddItem(QuestionWordLabel);
				questionStack.AddItem(new Shim(0, 8));
			}
			catch (Exception ex)
			{
				ScreenManager.ErrorScreen(ex);
				throw new Exception($"Error creating menu entry for {translation.Word}", ex);
			}
		}

		private void ResetColors()
		{
			QuestionWordLabel.OverrideColor = null;
			foreach (var entry in Entries)
			{
				entry.QuestionLabel.OverrideColor = null;
				entry.QuestionLabel.Scale = 1f;
			}

			QuestionWordLabel.Scale = 1.2f;
		}

		public void SetLabelOverride(QuestionLabel label)
		{
			label.OverrideColor = Color.Yellow;
			label.Scale = 1.5f;
		}

		protected virtual QuestionMenuEntry CreateQuestionMenuEntry(string text, FlashCard flashCard, bool correctAnswer, ContentManager content)
		{
			return new QuestionMenuEntry(text, flashCard, correctAnswer, content)
			{
				TransitionObject = new WipeTransitionObject(TransitionWipeType.PopBottom),
			};
		}

		protected virtual QuestionMenuEntry CreateQuestionMenuEntry(string text, FlashCard flashCard, bool correctAnswer, IFontBuddy font)
		{
			try
			{
				return new QuestionMenuEntry(text, flashCard, correctAnswer, font)
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
			if (null != OverlayScreen)
			{
				OverlayScreen.ExitScreen();
			}
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
			if (sender is QuestionMenuEntry questionMenuEntry)
			{
				AnswerSelected(true, questionMenuEntry.FlashCard);
			}
		}

		/// <summary>
		/// Event handler for when the High Scores menu entry is selected.
		/// </summary>
		private void WrongAnswerSelected(object sender, ClickEventArgs e)
		{
			if (sender is QuestionMenuEntry questionMenuEntry)
			{
				AnswerSelected(false, questionMenuEntry.FlashCard);
			}
		}

		#endregion //Handle Input

		#region Methods

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			//update the timers
			AutoQuit.Update(gameTime);

			//check if we been here long enough
			if (IsActive)
			{
				QuestionStateMachine.Update(gameTime);

				if (!AutoQuit.Paused && !AutoQuit.HasTimeRemaining)
				{
					//has the user picked an answer?
					if (!AnswerChosen)
					{
						//the timer ran out but the user hadn't picked an answer.  That counts as "wrong"
						AnswerSelected(false, correctQuestion);

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
		private void AnswerSelected(bool correctAnswer, FlashCard selectedAnswer)
		{
			QuestionStateMachine.Done();

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
					QuestionAnswered(this, new QuestionEventArgs(AnsweredCorrect, correctQuestion, selectedAnswer));
				}

				//start the timer to exit this screen
				AutoQuit.Start(1f);

				if (null != OverlayScreen)
				{
					OverlayScreen.ExitScreen();
				}
			}
		}

		public override void Draw(GameTime gameTime)
		{
			ScreenManager.SpriteBatchBegin();
			FadeBackground(0.2f);
			ScreenManager.SpriteBatchEnd();

			base.Draw(gameTime);
		}

		#endregion //Methods
	}
}