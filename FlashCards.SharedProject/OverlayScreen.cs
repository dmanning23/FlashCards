using GameTimer;
using LifeBarBuddy;
using MenuBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ResolutionBuddy;
using System.Threading.Tasks;

namespace FlashCards.Core
{
	public class OverlayScreen : WidgetScreen
	{
		#region Properties

		ITimerMeter CountdownClock { get; set; }

		IMeterRenderer meterRenderer;

		IScreen TimerScreen { get; set; }

		QuestionScreen QuestionScreen { get; set; }

		float QuestionTime => QuestionScreen.QuestionTime;

		CountdownTimer AutoQuit => QuestionScreen.AutoQuit;

		#endregion //Properties

		#region Methods

		public OverlayScreen(QuestionScreen questionScreen, ContentManager content = null) : base("Overlay Screen", content)
		{
			QuestionScreen = questionScreen;

			CoveredByOtherScreens = false;
			CoverOtherScreens = false;
		}

		public override async Task LoadContent()
		{
			await base.LoadContent();

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

			//make the player stare at this screen for 2 seconds before they can quit
			AutoQuit.Start(QuestionTime);
			CountdownClock.Reset();

			await ScreenManager.AddScreen(TimerScreen);

			//add the speech button for the question word
			AddListenButton(QuestionScreen.QuestionWordLabel, QuestionScreen.QuestionWordSoundEffect);

			//Add the speech button for each menuentry
			foreach (var entry in QuestionScreen.Entries)
			{
				AddListenButton(entry.QuestionLabel, entry.SoundEffect);
			}
		}

		private void AddListenButton(QuestionLabel label, SoundEffect soundEffect)
		{
			//create the button
			var button = new RelativeLayoutButton()
			{
				TransitionObject = new WipeTransitionObject(TransitionWipeType.PopRight, Transition),
				Size = new Vector2(48f),
				Vertical = VerticalAlignment.Center,
				Horizontal = HorizontalAlignment.Right,
				Position = new Point(Resolution.TitleSafeArea.Right, label.Rect.Center.Y),
				IsQuiet = true
			};

			//create the image for the button
			var image = new Image(Content.Load<Texture2D>("volume-high"))
			{
				TransitionObject = new WipeTransitionObject(TransitionWipeType.PopRight, Transition),
				Size = new Vector2(48f),
				Vertical = VerticalAlignment.Center,
				Horizontal = HorizontalAlignment.Right,
				Highlightable = false,
				FillRect = true
			};

			button.AddItem(image);

			button.OnClick += (obj, e) =>
			{
				//send the listen again message
				QuestionScreen.QuestionStateMachine.SendStateMessage((int)QuestionStateMachine.QuestionMessage.Listen);
				//set the override of the label
				QuestionScreen.SetLabelOverride(label);

				//play the sound
				QuestionScreen.PlaySoundEffect(soundEffect);
			};
			AddItem(button);
		}

		public override void ExitScreen()
		{
			base.ExitScreen();
			TimerScreen.ExitScreen();
		}

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			if (null != CountdownClock)
			{
				CountdownClock.Update(gameTime);
			}

			base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		}

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			//draw the meters
			meterRenderer.Alpha = Transition.Alpha;
			meterRenderer.SpriteBatchBegin(ScreenManager.SpriteBatch, Resolution.TransformationMatrix());
			if (!QuestionScreen.AnswerChosen)
			{
				CountdownClock.Draw(AutoQuit.RemainingTime, meterRenderer, ScreenManager.SpriteBatch);
			}
			else if (QuestionScreen.TimeRanOut)
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
