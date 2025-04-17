using System;
using System.Collections.Generic;
using UnityEngine;

namespace hezaerd.signals
{
	public class Signal<T> : ISignal
	{
		private T _value;
		private readonly HashSet<Action> _subscribers = new HashSet<Action>();
		private readonly IEqualityComparer<T> _comparer;

		public Signal(T initialValue, IEqualityComparer<T> comparer = null)
		{
			_value = initialValue;
			_comparer = comparer ?? EqualityComparer<T>.Default;
		}

		public T Value
		{
			get
			{
				Effect currentEffect = Effect.CurrentEffect.Value;
				currentEffect?.AddDependency(this);
				return _value;
			}
			set
			{
				if (_comparer.Equals(_value, value))
					return;
				
				_value = value;
				NotifySubscribers();
			}
		}

		public void Subscribe(Action subscriber) => _subscribers.Add(subscriber);
		public void Unsubscribe(Action subscriber) => _subscribers.Remove(subscriber);

		private void NotifySubscribers()
		{
			foreach (Action subscriber in _subscribers)
			{
				try { subscriber?.Invoke(); }
				catch (Exception e) { Debug.LogError($"Error in subscriber: {e}"); }
			}
		}

		public string GetValueString()
		{
			return Value?.ToString();
		}
	}
}