using System;

namespace hezaerd.signals
{
	public interface ISignal
	{
		void Subscribe(Action subscriber);
		void Unsubscribe(Action subscriber);

		string GetValueString();
	}
}