using System;

namespace hezaerd.fsm
{
	public interface IStateFactory
	{
		T Create<T>(params object[] args) where T : IState;
	}
	
	public class DefaultStateFactory : IStateFactory
	
	{
		public T Create<T>(params object[] args) where T : IState
		{
			// if no args use parameterless constructor
			if (args == null || args.Length == 0)
				return (T)Activator.CreateInstance(typeof(T));
			// otherwise pass down the args to the constructor
			return (T)Activator.CreateInstance(typeof(T), args);
		}
	}
}