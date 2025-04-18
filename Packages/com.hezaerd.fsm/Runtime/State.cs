namespace hezaerd.fsm
{
	public interface IState
	{
		void OnEnter();
		void OnExit();
		void OnUpdate();
	}
}