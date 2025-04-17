namespace hezaerd.fsm
{
	public interface ITransition
	{
		IState TargetState { get; }
		IPredicate Condition { get; }
	}
}