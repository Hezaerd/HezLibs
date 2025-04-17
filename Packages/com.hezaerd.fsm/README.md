# FSM Package

Generic Finite State Machine with backward navigation.

## Installation

To install this package via Unity Package Manager:
1. Open **Window > Package Manager**.
2. Click **+** > **Add package from git URL...**.
3. Enter `https://github.com/Hezaerd/HezLibs.git?path=Packages/com.hezaerd.fsm`.

## Usage

```csharp
using hezaerd.fsm;

public abstract class FooState : IState
{
    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void OnUpdate() { }
    public virtual void OnFixedUpdate() { }
}

// Define your states
public class StateA : FooState
{
    public void OnEnter() { Debug.Log("Enter State A"); }
    public void OnExit() { Debug.Log("Exit State A"); }
}

public class StateB : FooState
{
    public void OnEnter() { Debug.Log("Enter State B"); }
    public void OnExit() { Debug.Log("Exit State B"); }
}

public class FooBehaviour : MonoBehaviour
{
    private StateMachine fsm;

    void Start()
    {
        fsm = new StateMachine();
        
        var stateA = new StateA();
        var stateB = new StateB();
        
        fsm.RegisterState(stateA);
        fsm.RegisterState(stateB);
        
        fsm.AddTransition(stateA, stateB, new FuncPredicate(() => someCondition));
        
        fsm.SetRoot(stateA);
    }

    void Update()
    {
        fsm.Update();
        if (fsm.CanGoBack() && Input.GetKeyDown(KeyCode.Backspace))
            fsm.GoToPreviousState();
    }
    
    void FixedUpdate()
    {
        fsm.FixedUpdate();
    }
}
```

## API Reference

- **StateMachine**: Manages states and transitions.
- **IState**: Interface for states (`OnEnter`, `OnExit`, `OnUpdate`, `OnFixedUpdate`).
- **IPredicate**: Interface for transition conditions.
- **FuncPredicate**: Predicate based on `Func<bool>`.
- **Transition**: Represents a state transition.

## Author

Hezaerd ([https://hezaerd.com](https://hezaerd.com))

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
