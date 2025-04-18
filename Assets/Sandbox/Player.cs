using UnityEngine;
using hezaerd.fsm;

namespace sandbox
{
    public abstract class PlayerState : IState
    {
        protected readonly Player Player;
        
        protected PlayerState(Player player) { Player = player; }
        
        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void OnUpdate() { }
    }

    public sealed class IdleState : PlayerState
    {
        public IdleState(Player player) : base(player) { }
        
        public override void OnEnter() { Debug.Log("IdleState enter"); }
        public override void OnExit() { Debug.Log("IdleState exit"); }
        public override void OnUpdate() { Debug.Log("Idle"); }
    }
    
    public sealed class WalkState : PlayerState
    {
        public WalkState(Player player) : base(player) { }
        
        public override void OnEnter() { Debug.Log("WalkState enter"); }
        public override void OnExit() { Debug.Log("WalkState exit"); }
        public override void OnUpdate() { Debug.Log("Walking"); }
    }
    
    public class Player : MonoBehaviour
    {
        private StateMachine _fsm;

        private void Awake()
        {
            _fsm = new StateMachine();

            var idleState = _fsm.CreateState<IdleState>(this);
            var walkState = _fsm.CreateState<WalkState>(this);

            _fsm.AddTransition(idleState, walkState, new FuncPredicate(() => Input.GetKeyDown(KeyCode.W)));
            _fsm.AddTransition(walkState, idleState, new FuncPredicate(() => Input.GetKeyUp(KeyCode.W)));
            
            _fsm.SetRoot(idleState);
        }

        // Update is called once per frame
        private void Update()
        {
            _fsm.Update();
        }
    }
}
