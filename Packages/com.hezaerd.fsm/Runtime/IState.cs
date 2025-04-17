namespace hezaerd.fsm
{
	public interface IState<in TOwner>
	{
		void OnEnter(TOwner owner);
		void OnExit(TOwner owner);
		void OnUpdate(TOwner owner);
		void OnFixedUpdate(TOwner owner);
	}

	public interface IState : IState<object>
	{
		void IState<object>.OnEnter(object owner) => OnEnter();
		void IState<object>.OnExit(object owner) => OnExit();
		void IState<object>.OnUpdate(object owner) => OnUpdate();
		void IState<object>.OnFixedUpdate(object owner) => OnFixedUpdate();
		
		void OnEnter();
		void OnExit();
		void OnUpdate();
		void OnFixedUpdate();
	}

	public interface IStateSerializable
	{
		object SaveStateData();
		void LoadStateData(object data);
	}
}