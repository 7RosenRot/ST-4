using System;
using Stateless;

namespace BugPro
{
    public enum State { Open, Assigned, Deferred, Resolved, Closed, InTesting, Reopened }
    public enum Trigger { Assign, Defer, Resolve, Close, Reopen, Test, Reject }

    public class Bug
    {
        private State _state = State.Open;
        private readonly StateMachine<State, Trigger> _machine;
        
        public string? Assignee { get; private set; }
        public string Title { get; }
        public State CurrentState => _machine.State;

        private readonly StateMachine<State, Trigger>.TriggerWithParameters<string> _assignTrigger;

        public Bug(string title)
        {
            Title = title;
            _machine = new StateMachine<State, Trigger>(() => _state, s => _state = s);

            _assignTrigger = _machine.SetTriggerParameters<string>(Trigger.Assign);

            _machine.Configure(State.Open)
                .Permit(Trigger.Assign, State.Assigned);

            _machine.Configure(State.Assigned)
                .OnEntryFrom(_assignTrigger, assignee => Assignee = assignee)
                .Permit(Trigger.Resolve, State.InTesting)
                .Permit(Trigger.Defer, State.Deferred)
                .PermitReentry(Trigger.Assign);

            _machine.Configure(State.Deferred)
                .Permit(Trigger.Assign, State.Assigned);

            _machine.Configure(State.InTesting)
                .Permit(Trigger.Resolve, State.Resolved)
                .Permit(Trigger.Reject, State.Assigned);

            _machine.Configure(State.Resolved)
                .Permit(Trigger.Close, State.Closed)
                .Permit(Trigger.Reopen, State.Reopened);

            _machine.Configure(State.Reopened)
                .Permit(Trigger.Assign, State.Assigned);

            _machine.Configure(State.Closed)
                .Permit(Trigger.Reopen, State.Reopened);
        }

        public void Assign(string assignee) => _machine.Fire(_assignTrigger, assignee);
        public void Defer() => _machine.Fire(Trigger.Defer);
        public void Resolve() => _machine.Fire(Trigger.Resolve);
        public void Close() => _machine.Fire(Trigger.Close);
        public void Reopen() => _machine.Fire(Trigger.Reopen);
        public void SendToTest() => _machine.Fire(Trigger.Resolve);
        public void Reject() => _machine.Fire(Trigger.Reject);

        public bool CanFire(Trigger trigger) => _machine.CanFire(trigger);
    }

    class Program
    {
        static void Main(string[] args)
        {
            var bug = new Bug("Исправление ошибок компиляции");
            Console.WriteLine($"Bug: {bug.Title}, State: {bug.CurrentState}");

            bug.Assign("Dev_User");
            Console.WriteLine($"Assigned to: {bug.Assignee}, State: {bug.CurrentState}");

            bug.SendToTest();
            Console.WriteLine($"State: {bug.CurrentState}");
        }
    }
}