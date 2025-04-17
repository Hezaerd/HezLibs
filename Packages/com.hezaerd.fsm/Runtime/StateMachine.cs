using System;
using System.Collections.Generic;
using System.Linq;

namespace hezaerd.fsm
{
	/// <summary>
	/// A generic state machine which supports transitions, "any" transitions, root state
	/// resetting, and backward navigation via history.
	/// </summary>
	public class StateMachine<TOwner>
	{
		/// <summary>
		/// Event fired when a state transition occurs.
		/// </summary>
		public event Action<IState<TOwner>, IState<TOwner>> OnStateChanged;

		public FsmLogType FsmLogType = FsmLogType.ALL;
		
		public Action<string, FsmLogType> OnLog;
		
		private StateNode _root;
		private StateNode _current;
		private readonly Dictionary<Type, StateNode> _nodes = new();
		private readonly HashSet<ITransition<TOwner>> _anyTransitions = new();
		private readonly Stack<IState<TOwner>> _stateHistory = new();

		private readonly TOwner _owner;

		private readonly Func<Type, IState<TOwner>> _stateFactory;
		
		public StateMachine(TOwner owner, Func<Type, IState<TOwner>> customStateFactory = null)
		{
			_owner = owner;
			_stateFactory = customStateFactory;
		}
		
		#region Lifecycle Methods
		
		/// <summary>
		/// Updates the current state and processes any valid transitions.
		/// </summary>
		public void Update()
		{
			ITransition<TOwner> transition = GetTransition();
			if (transition != null)
				ChangeState(transition.TargetState);
			
			_current.State?.OnUpdate(_owner);
		}
		
		/// <summary>
		/// Calls the FixedUpdate method on the current state.
		/// </summary>
		public void FixedUpdate()
		{
			_current.State?.OnFixedUpdate(_owner);
		}
		#endregion
		
		#region State Factory
		/// <summary>
		/// Creates and register a new state of type T.
		/// </summary>
		/// <typeparam name="T">The type of state to create.</typeparam>
		/// <returns>The created state instance.</returns>
		public T CreateState<T>() where T : IState<TOwner>, new()
		{
			Type type = typeof(T);
			if (_nodes.TryGetValue(type, out StateNode node))
				return (T)node.State;

			T state;
			if (_stateFactory != null)
				state = (T)_stateFactory(type);
			else
				state = new T();

			Log($"State created: {type.Name}", FsmLogType.STATE_CREATED);
			RegisterState(state);
			return state;
		}
		
		/// <summary>
		/// Creates and registers a new state using a custom factory function.
		/// </summary>
		/// <param name="factory">The factory function to create the state.</param>
		/// <typeparam name="T">The type of state to create.</typeparam>
		/// <returns>The created state instance.</returns>
		public T CreateState<T>(Func<T> factory) where T : IState<TOwner>
		{
			Type type = typeof(T);
			if (_nodes.TryGetValue(type, out StateNode node))
				return (T)node.State;

			T state = factory();
			Log($"State created: {type.Name}", FsmLogType.STATE_CREATED);
			RegisterState(state);
			return state;
		}
		
		#endregion
		
		#region State Management
		
		/// <summary>
		/// Sets the root state of the state machine.
		/// The root becomes the base state for history.
		/// </summary>
		public void SetRoot(IState<TOwner> state)
		{
			_root = GetOrCreateNode(state);
			_stateHistory.Clear();
			_current = _root;
			_current.State?.OnEnter(_owner);
			Log($"State entered: {state.GetType().Name}", FsmLogType.STATE_ENTER);
		}
		
		/// <summary>
		/// Resets the state machine to the root state.
		/// </summary>
		public void ResetToRoot()
		{
			if (_root == null)
			{
				Log("ResetToRoot called but root is null.", FsmLogType.ERROR);
				return;
			}

			_current.State?.OnExit(_owner);
			Log($"State exited: {_current.State?.GetType().Name}", FsmLogType.STATE_EXIT);
			
			_stateHistory.Clear();
			IState<TOwner> previousState = _current.State;
			_current = _root;
			_current.State?.OnEnter(_owner);
			Log($"State entered: {_current.State?.GetType().Name}", FsmLogType.STATE_ENTER);
			
			OnStateChanged?.Invoke(previousState, _current.State);
		}
		
		/// <summary>
		/// Changes the current state to the provided state.
		/// When not coming from history the current state is pushed onto the history stack.
		/// </summary>
		/// <param name="state">The target state.</param>
		/// <param name="fromHistory">Indicates whether the state change is a backward (history) transition.</param>
		private void ChangeState(IState<TOwner> state, bool fromHistory = false)
		{
			if (state == _current.State)
				return;

			IState<TOwner> previousState = _current.State;
			IState<TOwner> nextState = _nodes[state.GetType()].State;

			if (!fromHistory && previousState != null)
			{
				_stateHistory.Push(previousState);
			}

			previousState?.OnExit(_owner);
			Log($"State exited: {previousState?.GetType().Name}", FsmLogType.STATE_EXIT);
			
			nextState?.OnEnter(_owner);
			Log($"State entered: {nextState?.GetType().Name}", FsmLogType.STATE_ENTER);

			OnStateChanged?.Invoke(previousState, nextState);
			_current = _nodes[state.GetType()];
		}
		
		/// <summary>
		/// Determines if the state machine can navigate to a previous state.
		/// </summary>
		/// <returns>True if there's a previous state on history, false otherwise.</returns>
		public bool CanGoBack() => _stateHistory.Count > 0;

		/// <summary>
		/// Moves the state machine back to the previously visited state.
		/// </summary>
		public void GoToPreviousState()
		{
			if (_stateHistory.Count == 0)
			{
				Log("Unable to go back: no previous state in history.", FsmLogType.ERROR);
				return;
			}

			IState<TOwner> previousState = _stateHistory.Pop();
			ChangeState(previousState, fromHistory: true);
		}

		/// <summary>
		/// Checks if the current state is of type T.
		/// </summary>
		/// <typeparam name="T">The state type to test against.</typeparam>
		/// <returns>True if current state is of type T; otherwise false.</returns>
		public bool IsInState<T>() where T : IState<TOwner> => _current.State is T;

		/// <summary>
		/// Retrieves the full route of states from the root to the current state,
		/// including the history of forward transitions.
		/// </summary>
		/// <returns>
		/// A list of <see cref="IState"/> objects starting from the root state,
		/// followed by the states in the transition history in order,
		/// ending with the current state.
		/// </returns>
		public List<IState<TOwner>> GetFullStateRoute()
		{
			var route = new List<IState<TOwner>>();
			var historyArray = _stateHistory.ToArray();
			Array.Reverse(historyArray);
			route.AddRange(historyArray);

			if (route.Count == 0 || route[^1] != _current.State)
				route.Add(_current.State);

			return route;
		}
		
		#endregion

		#region Transition & Node Registration
		
		/// <summary>
		/// Registers a state onto the state machine.
		/// All states should be registered before setting the root state or adding transitions.
		/// </summary>
		/// <param name="state">The state to register.</param>
		private void RegisterState(IState<TOwner> state)
		{
			Type type = state.GetType();
			if (_nodes.ContainsKey(type))
				return;

			_nodes.Add(type, new StateNode(state));
			Log($"State registered: {type.Name}", FsmLogType.STATE_REGISTERED);
		}
		
		/// <summary>
		/// Adds a transition between two registered states.
		/// </summary>
		/// <param name="from">The originating state.</param>
		/// <param name="to">The target state.</param>
		/// <param name="condition">The condition that must be met for the transition to occur.</param>
		public void AddTransition(IState<TOwner> from, IState<TOwner> to, IPredicate condition)
		{
			if (!_nodes.ContainsKey(from.GetType()) || !_nodes.ContainsKey(to.GetType()))
			{
				Log("AddTransition failed: Both states must be registered before defining a transition.", FsmLogType.ERROR);
				throw new InvalidOperationException("Both states must be registered before defining a transition.");
			}

			_nodes[from.GetType()].AddTransition(_nodes[to.GetType()].State, condition);
		}

		/// <summary>
		/// Adds an 'any' transition to a registered state. This transition checks regardless of the current state.
		/// </summary>
		/// <param name="to">The target state.</param>
		/// <param name="condition">The condition that must be met for the transition to occur.</param>
		public void AddAnyTransition(IState<TOwner> to, IPredicate condition)
		{
			if (!_nodes.ContainsKey(to.GetType()))
			{
				Log("AddAnyTransition failed: State must be registered before defining a transition.", FsmLogType.ERROR);
				throw new InvalidOperationException("State must be registered before defining a transition.");
			}

			_anyTransitions.Add(new Transition<TOwner>(_nodes[to.GetType()].State, condition));
		}

		/// <summary>
		/// Returns the state node associated with the given state.
		/// If not registered, creates a new node.
		/// </summary>
		/// <param name="state">The state instance.</param>
		/// <returns>The corresponding state node.</returns>
		private StateNode GetOrCreateNode(IState<TOwner> state)
		{
			Type type = state.GetType();
			if (!_nodes.ContainsKey(type))
			{
				_nodes.Add(type, new StateNode(state));
				Log($"State registered: {type.Name}", FsmLogType.STATE_REGISTERED);
			}

			return _nodes[type];
		}
		
		/// <summary>
		/// Retrieves the first transition whose condition returns true.
		/// First checks transitions registered on the current state then any-transitions.
		/// </summary>
		/// <returns>An ITransition if a valid transition is found; otherwise null.</returns>
		private ITransition<TOwner> GetTransition()
		{
			foreach (var transition in _current.Transitions)
			{
				if (transition.Condition.Evaluate())
					return transition;
			}

			foreach (var transition in _anyTransitions)
			{
				if (transition.Condition.Evaluate())
					return transition;
			}

			return null;
		}
		
		#endregion
		
		#region Serialization Hooks
		/// <summary>
		/// Gets the current state's unique identifier (type name).
		/// </summary>
		public string CurrentStateId => _current?.State?.GetType().AssemblyQualifiedName;
		
		/// <summary>
		/// Gets the state history as a list of unique identifiers (type names).
		/// </summary>
		public List<string> StateHistoryIds => _stateHistory.Select(s => s.GetType().AssemblyQualifiedName).ToList();
		
		/// <summary>
		/// Gets all registered states
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IState<TOwner>> GetAllStates()
		{
			return _nodes.Values.Select(n => n.State);
		}
		
		/// <summary>
		/// Restores the current state by its unique identifier.
		/// </summary>
		/// <param name="id">The unique identifier (type name) of the state.</param>
		public void SetCurrentStateById(string id)
		{
			if (string.IsNullOrEmpty(id)) 
				return;
			Type type = Type.GetType(id);
			if (type != null && _nodes.TryGetValue(type, out var node))
				_current = node;
		}

		/// <summary>
		/// Restores the state history by a list of unique identifiers.
		/// </summary>
		/// <param name="ids">The list of unique identifiers (type names) of the states.</param>
		public void SetStateHistoryByIds(IEnumerable<string> ids)
		{
			_stateHistory.Clear();
			foreach (var id in ids)
			{
				Type type = Type.GetType(id);
				if (type != null && _nodes.TryGetValue(type, out var node))
					_stateHistory.Push(node.State);
			}
		}
		#endregion

		#region Private State Node Class
		
		/// <summary>
		/// Represents a node in the state machine, holding the state
		/// and its connected transitions.
		/// </summary>
		private class StateNode
		{
			/// <summary>
			/// The state represented by this node.
			/// </summary>
			public IState<TOwner> State { get; }

			/// <summary>
			/// The transitions originating from this state.
			/// </summary>
			public HashSet<ITransition<TOwner>> Transitions { get; }

			public StateNode(IState<TOwner> state)
			{
				State = state;
				Transitions = new HashSet<ITransition<TOwner>>();
			}

			/// <summary>
			/// Adds a transition from this state to the target state.
			/// </summary>
			/// <param name="targetState">The target state.</param>
			/// <param name="condition">The condition required for this transition.</param>
			public void AddTransition(IState<TOwner> targetState, IPredicate condition)
			{
				Transitions.Add(new Transition<TOwner>(targetState, condition));
			}
		}

        #endregion
		
		#region Logging
		private void Log(string message, FsmLogType type)
		{
			if ((FsmLogType & type) != 0)
				OnLog?.Invoke(message, type);
		}
		#endregion
	}
	
	/// <summary>
	/// Non-generic <see cref="StateMachine{TOwner}"/>
	/// </summary>
	public class StateMachine : StateMachine<object>
	{
		public StateMachine() : base(null) { }
	}
}
