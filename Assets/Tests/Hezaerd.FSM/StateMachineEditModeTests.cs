using System.Collections.Generic;
using hezaerd.fsm;
using NUnit.Framework;

public class StateMachineEditModeTests
{
	private class TestState : IState
    {
        public bool Entered, Exited, Updated;
        public void OnEnter() => Entered = true;
        public void OnExit() => Exited = true;
        public void OnUpdate() => Updated = true;
    }

    private class AnotherState : IState
    {
        public bool Entered, Exited, Updated;
        public void OnEnter() => Entered = true;
        public void OnExit() => Exited = true;
        public void OnUpdate() => Updated = true;
    }

    [Test]
    public void StateMachine_RegistersAndSetsRootState()
    {
        var fsm = new StateMachine();
        var state = fsm.CreateState<TestState>();
        fsm.SetRoot(state);

        Assert.IsTrue(fsm.IsInState<TestState>());
    }

    [Test]
    public void StateMachine_TransitionsBetweenStates()
    {
        var fsm = new StateMachine();
        bool canTransition = false;

        var stateA = fsm.CreateState<TestState>();
        var stateB = fsm.CreateState<AnotherState>();

        fsm.AddTransition(stateA, stateB, new FuncPredicate(() => canTransition));
        fsm.SetRoot(stateA);

        Assert.IsTrue(fsm.IsInState<TestState>());

        canTransition = true;
        fsm.Update();

        Assert.IsTrue(fsm.IsInState<AnotherState>());
    }

    [Test]
    public void StateMachine_CallsOnEnterAndOnExit()
    {
        var fsm = new StateMachine();
        bool canTransition = false;

        // Use the same instances from CreateState<T>()
        var stateA = fsm.CreateState<TestState>();
        var stateB = fsm.CreateState<AnotherState>();

        fsm.AddTransition(stateA, stateB, new FuncPredicate(() => canTransition));
        fsm.SetRoot(stateA);

        canTransition = true;
        fsm.Update();

        Assert.IsTrue(stateA.Exited, "OnExit should be called on previous state");
        Assert.IsTrue(stateB.Entered, "OnEnter should be called on new state");
    }

    [Test]
    public void StateMachine_HistoryAndGoBack()
    {
        var fsm = new StateMachine();
        bool canTransition = false;

        var stateA = fsm.CreateState<TestState>();
        var stateB = fsm.CreateState<AnotherState>();

        fsm.AddTransition(stateA, stateB, new FuncPredicate(() => canTransition));
        fsm.SetRoot(stateA);

        canTransition = true;
        fsm.Update();

        Assert.IsTrue(fsm.CanGoBack());
        fsm.GoToPreviousState();

        Assert.IsTrue(fsm.IsInState<TestState>());
    }

    [Test]
    public void StateMachine_ResetToRoot()
    {
        var fsm = new StateMachine();
        bool canTransition = false;

        var stateA = fsm.CreateState<TestState>();
        var stateB = fsm.CreateState<AnotherState>();

        fsm.AddTransition(stateA, stateB, new FuncPredicate(() => canTransition));
        fsm.SetRoot(stateA);

        canTransition = true;
        fsm.Update();

        fsm.ResetToRoot();

        Assert.IsTrue(fsm.IsInState<TestState>());
        Assert.IsFalse(fsm.CanGoBack());
    }

    [Test]
    public void StateMachine_OnStateChanged_EventIsCalled()
    {
        var fsm = new StateMachine();
        bool canTransition = false;
        bool eventCalled = false;

        var stateA = fsm.CreateState<TestState>();
        var stateB = fsm.CreateState<AnotherState>();

        fsm.AddTransition(stateA, stateB, new FuncPredicate(() => canTransition));
        fsm.SetRoot(stateA);

        fsm.OnStateChanged += (from, to) =>
        {
            eventCalled = true;
            Assert.IsInstanceOf<TestState>(from);
            Assert.IsInstanceOf<AnotherState>(to);
        };

        canTransition = true;
        fsm.Update();

        Assert.IsTrue(eventCalled, "OnStateChanged event should be called on transition");
    }
    
    [Test]
    public void StateMachine_LogsExpectedMessages()
    {
        var fsm = new StateMachine();
        var logs = new List<(string, FsmLogType)>();
        fsm.OnLog = (msg, type) => logs.Add((msg, type));

        var stateA = fsm.CreateState<TestState>();
        var stateB = fsm.CreateState<AnotherState>();

        fsm.AddTransition(stateA, stateB, new FuncPredicate(() => true));
        fsm.SetRoot(stateA);

        // Should log state registration, creation, entry, etc.
        Assert.IsTrue(logs.Exists(l => l.Item1.Contains("State registered") && l.Item2 == FsmLogType.STATE_REGISTERED));
        Assert.IsTrue(logs.Exists(l => l.Item1.Contains("State created") && l.Item2 == FsmLogType.STATE_CREATED));
        Assert.IsTrue(logs.Exists(l => l.Item1.Contains("State entered") && l.Item2 == FsmLogType.STATE_ENTER));

        logs.Clear();
        fsm.Update(); // Should trigger exit and enter logs

        Assert.IsTrue(logs.Exists(l => l.Item1.Contains("State exited") && l.Item2 == FsmLogType.STATE_EXIT));
        Assert.IsTrue(logs.Exists(l => l.Item1.Contains("State entered") && l.Item2 == FsmLogType.STATE_ENTER));
    }

    [Test]
    public void StateMachine_SerializationAndRestoration_Works()
    {
        var fsm = new StateMachine();
        bool canTransition = false;

        var stateA = fsm.CreateState<TestState>();
        var stateB = fsm.CreateState<AnotherState>();

        fsm.AddTransition(stateA, stateB, new FuncPredicate(() => canTransition));
        fsm.SetRoot(stateA);

        canTransition = true;
        fsm.Update(); // Move to stateB

        // Save current state and history
        string currentStateId = fsm.CurrentStateId;
        List<string> historyIds = fsm.StateHistoryIds;

        // Create a new FSM and register the same states
        var fsm2 = new StateMachine();
        var stateA2 = fsm2.CreateState<TestState>();
        var stateB2 = fsm2.CreateState<AnotherState>();
        fsm2.SetRoot(stateA2);

        // Restore state and history
        fsm2.SetCurrentStateById(currentStateId);
        fsm2.SetStateHistoryByIds(historyIds);

        // The restored FSM should be in the same state and have the same history
        Assert.IsTrue(fsm2.IsInState<AnotherState>());
        Assert.AreEqual(historyIds.Count, fsm2.StateHistoryIds.Count);
        Assert.AreEqual(currentStateId, fsm2.CurrentStateId);
    }
}
