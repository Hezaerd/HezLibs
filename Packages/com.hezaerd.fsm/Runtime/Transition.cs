namespace hezaerd.fsm
{
	public interface ITransition<in TOwner>
	{
		IState<TOwner> TargetState { get; }
		IPredicate Condition { get; }
	}
	
	public class Transition<TOwner> : ITransition<TOwner>
	{
		public IState<TOwner> TargetState { get; }
		public IPredicate Condition { get; }
		
		public Transition(IState<TOwner> targetState, IPredicate condition)
		{
			TargetState = targetState;
			Condition = condition;
		}
	}
}