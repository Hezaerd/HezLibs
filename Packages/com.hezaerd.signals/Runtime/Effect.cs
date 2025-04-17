using System;
using System.Collections.Generic;
using System.Threading;

namespace hezaerd.signals
{
	public class Effect : IDisposable
	{
		private readonly Action _effect;
		private readonly List<ISignal> _dependencies = new List<ISignal>();
		private bool _isRunning = true;

		public static AsyncLocal<Effect> CurrentEffect = new AsyncLocal<Effect>();
		
		public Effect(Action effect, bool runImmediately = true)
		{
			_effect = effect ?? throw new ArgumentNullException(nameof(effect));
			if (runImmediately) 
				Run();
		}
		
		public void Run()
		{
			if (!_isRunning) return;
			foreach (ISignal dependency in _dependencies)
				dependency.Unsubscribe(_effect);
			_dependencies.Clear();

			CurrentEffect.Value = this;
			try { _effect(); }
			finally { CurrentEffect.Value = null; }
		}

		public void AddDependency(ISignal signal)
		{
			if (_dependencies.Contains(signal))
				return;
			
			_dependencies.Add(signal);
			signal.Subscribe(_effect);
		}

		public void Dispose()
		{
			_isRunning = false;
			foreach (ISignal dependency in _dependencies)
				dependency.Unsubscribe(_effect);
			_dependencies.Clear();
		}
	}
}