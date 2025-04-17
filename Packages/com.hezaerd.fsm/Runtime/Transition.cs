namespace hezaerd.fsm
{
	public class Transition : ITransition
	{
		public IState TargetState { get; }
		public IPredicate Condition { get; }
		
		public Transition(IState targetState, IPredicate condition)
		{
			TargetState = targetState;
			Condition = condition;
		}
	}
}