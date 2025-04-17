using System.Collections.Generic;
using hezaerd.fsm;
using NUnit.Framework;

public class StateMachineEditModeTests
{
    private class DummyOwner { }

    private class StateA : IState<DummyOwner>
    {
        public bool Entered, Exited;
        public void OnEnter(DummyOwner owner) => Entered = true;
        public void OnExit(DummyOwner owner) => Exited = true;
        public void OnUpdate(DummyOwner owner) { }
        public void OnFixedUpdate(DummyOwner owner) { }
    }

    private class StateB : IState<DummyOwner>
    {
        public bool Entered;
        public void OnEnter(DummyOwner owner) => Entered = true;
        public void OnExit(DummyOwner owner) { }
        public void OnUpdate(DummyOwner owner) { }
        public void OnFixedUpdate(DummyOwner owner) { }
    }

    [Test]
    public void Registers_And_Creates_States()
    {
        var fsm = new StateMachine<DummyOwner>(new DummyOwner());
        var stateA = fsm.CreateState<StateA>();
        var stateB = fsm.CreateState<StateB>();
        
        Assert.IsNotNull(stateA);
        Assert.IsNotNull(stateB);
        Assert.AreNotSame(stateA, stateB);
    }
    
    [Test]
    public void Transitions_Between_States()
    {
        var fsm = new StateMachine<DummyOwner>(new DummyOwner());
        var stateA = fsm.CreateState<StateA>();
        var stateB = fsm.CreateState<StateB>();

        fsm.AddTransition(stateA, stateB, new FuncPredicate(() => true));
        fsm.SetRoot(stateA);

        fsm.Update();

        Assert.IsTrue(stateA.Entered);
        Assert.IsTrue(stateA.Exited);
        Assert.IsTrue(stateB.Entered);
        Assert.IsTrue(fsm.IsInState<StateB>());
    }

    [Test]
    public void Any_Transition_Works()
    {
        var fsm = new StateMachine<DummyOwner>(new DummyOwner());
        var stateA = fsm.CreateState<StateA>();
        var stateB = fsm.CreateState<StateB>();

        fsm.SetRoot(stateA);
        fsm.AddAnyTransition(stateB, new FuncPredicate(() => true));

        fsm.Update();

        Assert.IsTrue(stateB.Entered);
        Assert.IsTrue(fsm.IsInState<StateB>());
    }

    [Test]
    public void State_History_GoBack_Works()
    {
        var fsm = new StateMachine<DummyOwner>(new DummyOwner());
        var stateA = fsm.CreateState<StateA>();
        var stateB = fsm.CreateState<StateB>();

        bool toB = false;
        fsm.AddTransition(stateA, stateB, new FuncPredicate(() => toB));
        fsm.SetRoot(stateA);

        toB = true;
        fsm.Update();

        Assert.IsTrue(fsm.IsInState<StateB>());
        Assert.IsTrue(fsm.CanGoBack());

        fsm.GoToPreviousState();

        Assert.IsTrue(fsm.IsInState<StateA>());
    }

    [Test]
    public void Serialization_Hooks_Work()
    {
        var fsm = new StateMachine<DummyOwner>(new DummyOwner());
        var stateA = fsm.CreateState<StateA>();
        var stateB = fsm.CreateState<StateB>();

        fsm.AddTransition(stateA, stateB, new FuncPredicate(() => true));
        fsm.SetRoot(stateA);
        fsm.Update(); // Should transition to B

        // Save
        string currentId = fsm.CurrentStateId;
        List<string> historyIds = fsm.StateHistoryIds;

        // Reset and restore
        fsm.SetRoot(stateA);
        fsm.SetCurrentStateById(currentId);
        fsm.SetStateHistoryByIds(historyIds);

        Assert.IsTrue(fsm.IsInState<StateB>());
        Assert.AreEqual(1, fsm.StateHistoryIds.Count);
    }
}
