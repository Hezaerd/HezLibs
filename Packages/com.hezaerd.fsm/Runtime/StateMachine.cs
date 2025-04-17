using System;
using System.Collections.Generic;

namespace hezaerd.fsm
{
	/// <summary>
	/// A state machine which supports transitions, "any" transitions, root state
	/// resetting, and backward navigation via history.
	/// </summary>
	public class StateMachine
	{
		/// <summary>
		/// Event fired when a state transition occurs.
		/// </summary>
		public event Action<IState, IState> OnStateChanged;

		private StateNode _root;
		private StateNode _current;
		private readonly Dictionary<Type, StateNode> _nodes = new Dictionary<Type, StateNode>();
		private readonly HashSet<ITransition> _anyTransitions = new HashSet<ITransition>();
		private readonly Stack<IState> _stateHistory = new Stack<IState>();

		#region Lifecycle Methods
		
		/// <summary>
		/// Updates the current state and processes any valid transitions.
		/// </summary>
		public void Update()
		{
			ITransition transition = GetTransition();
			if (transition != null)
				ChangeState(transition.TargetState);
			
			_current.State?.OnUpdate();
		}
		
		/// <summary>
		/// Calls the FixedUpdate method on the current state.
		/// </summary>
		public void FixedUpdate()
		{
			_current.State?.OnFixedUpdate();
		}
		#endregion
		
		#region State Management
		
		/// <summary>
		/// Sets the root state of the state machine.
		/// The root becomes the base state for history.
		/// </summary>
		public void SetRoot(IState state)
		{
			_root = GetOrCreateNode(state);
			_stateHistory.Clear();
			_current = _root;
			_current.State?.OnEnter();
		}
		
		/// <summary>
		/// Resets the state machine to the root state.
		/// </summary>
		public void ResetToRoot()
		{
			if (_root == null)
				return;

			_current.State?.OnExit();
			_stateHistory.Clear();
			IState previousState = _current.State;
			_current = _root;
			_current.State?.OnEnter();
			OnStateChanged?.Invoke(previousState, _current.State);
		}
		
		/// <summary>
		/// Changes the current state to the provided state.
		/// When not coming from history the current state is pushed onto the history stack.
		/// </summary>
		/// <param name="state">The target state.</param>
		/// <param name="fromHistory">Indicates whether the state change is a backward (history) transition.</param>
		private void ChangeState(IState state, bool fromHistory = false)
		{
			if (state == _current.State)
				return;

			IState previousState = _current.State;
			IState nextState = _nodes[state.GetType()].State;

			// Only push onto history if not a backward transition
			if (!fromHistory && previousState != null)
				_stateHistory.Push(previousState);

			previousState?.OnExit();
			nextState?.OnEnter();

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
				return;

			IState previousState = _stateHistory.Pop();
			ChangeState(previousState, fromHistory: true);
		}

		/// <summary>
		/// Checks if the current state is of type T.
		/// </summary>
		/// <typeparam name="T">The state type to test against.</typeparam>
		/// <returns>True if current state is of type T; otherwise false.</returns>
		public bool IsInState<T>() where T : IState => _current.State is T;

		/// <summary>
		/// Retrieves the full route of states from the root to the current state,
		/// including the history of forward transitions.
		/// </summary>
		/// <returns>
		/// A list of <see cref="IState"/> objects starting from the root state,
		/// followed by the states in the transition history in order,
		/// ending with the current state.
		/// </returns>
		public List<IState> GetFullStateRoute()
		{
			var route = new List<IState>();
			
			// if (_root != null)
			// 	route.Add(_root.State);
			
			// Reverse the history stack to get the correct order
			var historyArray = _stateHistory.ToArray();
			Array.Reverse(historyArray);
			route.AddRange(historyArray);
			
			// Append the current state if it's not already the last one.
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
		public void RegisterState(IState state)
		{
			Type type = state.GetType();
			if (_nodes.ContainsKey(type))
				return;

			_nodes.Add(type, new StateNode(state));
		}
		
		/// <summary>
		/// Adds a transition between two registered states.
		/// </summary>
		/// <param name="from">The originating state.</param>
		/// <param name="to">The target state.</param>
		/// <param name="condition">The condition that must be met for the transition to occur.</param>
		public void AddTransition(IState from, IState to, IPredicate condition)
		{
			if (!_nodes.ContainsKey(from.GetType()) || !_nodes.ContainsKey(to.GetType()))
				throw new InvalidOperationException("Both states must be registered before defining a transition.");

			_nodes[from.GetType()].AddTransition(_nodes[to.GetType()].State, condition);
		}

		/// <summary>
		/// Adds an 'any' transition to a registered state. This transition checks regardless of the current state.
		/// </summary>
		/// <param name="to">The target state.</param>
		/// <param name="condition">The condition that must be met for the transition to occur.</param>
		public void AddAnyTransition(IState to, IPredicate condition)
		{
			if (!_nodes.ContainsKey(to.GetType()))
				throw new InvalidOperationException("State must be registered before defining a transition.");

			_anyTransitions.Add(new Transition(_nodes[to.GetType()].State, condition));
		}

		/// <summary>
		/// Returns the state node associated with the given state.
		/// If not registered, creates a new node.
		/// </summary>
		/// <param name="state">The state instance.</param>
		/// <returns>The corresponding state node.</returns>
		private StateNode GetOrCreateNode(IState state)
		{
			Type type = state.GetType();
			if (!_nodes.ContainsKey(type))
				_nodes.Add(type, new StateNode(state));

			return _nodes[type];
		}
		
		/// <summary>
		/// Retrieves the first transition whose condition returns true.
		/// First checks transitions registered on the current state then any-transitions.
		/// </summary>
		/// <returns>An ITransition if a valid transition is found; otherwise null.</returns>
		private ITransition GetTransition()
		{
			foreach (ITransition transition in _current.Transitions)
			{
				if (transition.Condition.Evaluate())
					return transition;
			}

			foreach (ITransition transition in _anyTransitions)
			{
				if (transition.Condition.Evaluate())
					return transition;
			}

			return null;
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
			public IState State { get; }

			/// <summary>
			/// The transitions originating from this state.
			/// </summary>
			public HashSet<ITransition> Transitions { get; }

			public StateNode(IState state)
			{
				State = state;
				Transitions = new HashSet<ITransition>();
			}

			/// <summary>
			/// Adds a transition from this state to the target state.
			/// </summary>
			/// <param name="targetState">The target state.</param>
			/// <param name="condition">The condition required for this transition.</param>
			public void AddTransition(IState targetState, IPredicate condition)
			{
				Transitions.Add(new Transition(targetState, condition));
			}
		}

        #endregion
	}
}
