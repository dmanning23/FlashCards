using GameTimer;
using Microsoft.Xna.Framework;
using StateMachineBuddy;

namespace FlashCards.Core
{
	public class QuestionStateMachine : StateMachine
	{
		public enum QuestionState
		{
			InitialPause,
			AskingQuestion,
			QuestionWord,
			FirstAnswer,
			SecondAnswer,
			ThirdAnswer,
			FourthAnswer,
			Done,
			ListenAgain
		}

		public enum QuestionMessage
		{
			Next,
			Done,
			Listen
		}

		public CountdownTimer NextTimer { get; private set; }

		public QuestionStateMachine() : base()
		{
			//Setup the state machine
			Set(typeof(QuestionState), typeof(QuestionMessage));

			//Setup the message transitions
			SetEntry((int)QuestionState.InitialPause, (int)QuestionMessage.Next, (int)QuestionState.AskingQuestion);
			SetEntry((int)QuestionState.AskingQuestion, (int)QuestionMessage.Next, (int)QuestionState.QuestionWord);
			SetEntry((int)QuestionState.QuestionWord, (int)QuestionMessage.Next, (int)QuestionState.FirstAnswer);
			SetEntry((int)QuestionState.FirstAnswer, (int)QuestionMessage.Next, (int)QuestionState.SecondAnswer);
			SetEntry((int)QuestionState.SecondAnswer, (int)QuestionMessage.Next, (int)QuestionState.ThirdAnswer);
			SetEntry((int)QuestionState.ThirdAnswer, (int)QuestionMessage.Next, (int)QuestionState.FourthAnswer);
			SetEntry((int)QuestionState.FourthAnswer, (int)QuestionMessage.Next, (int)QuestionState.Done);
			SetEntry((int)QuestionState.ListenAgain, (int)QuestionMessage.Next, (int)QuestionState.Done);

			SetEntry((int)QuestionState.Done, (int)QuestionMessage.Listen, (int)QuestionState.ListenAgain);

			SetEntry((int)QuestionState.InitialPause, (int)QuestionMessage.Done, (int)QuestionState.Done);
			SetEntry((int)QuestionState.AskingQuestion, (int)QuestionMessage.Done, (int)QuestionState.Done);
			SetEntry((int)QuestionState.QuestionWord, (int)QuestionMessage.Done, (int)QuestionState.Done);
			SetEntry((int)QuestionState.FirstAnswer, (int)QuestionMessage.Done, (int)QuestionState.Done);
			SetEntry((int)QuestionState.SecondAnswer, (int)QuestionMessage.Done, (int)QuestionState.Done);
			SetEntry((int)QuestionState.ThirdAnswer, (int)QuestionMessage.Done, (int)QuestionState.Done);
			SetEntry((int)QuestionState.FourthAnswer, (int)QuestionMessage.Done, (int)QuestionState.Done);
			SetEntry((int)QuestionState.ListenAgain, (int)QuestionMessage.Done, (int)QuestionState.Done);

			NextTimer = new CountdownTimer();
		}

		public void Update(GameTime gameTime)
		{
			NextTimer.Update(gameTime);

			if (!NextTimer.HasTimeRemaining)
			{
				SendStateMessage((int)QuestionMessage.Next);
			}
		}

		public void StartTimer(float time)
		{
			NextTimer.Start(time);
		}

		public void Done()
		{
			SendStateMessage((int)QuestionMessage.Done);
		}
	}
}