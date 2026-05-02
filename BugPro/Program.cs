using System;
using Stateless;

namespace BugPro;

public enum BugState {
  New,
  Triage,
  Fixing,
  Verification,
  Closed,

  Returned,
  Reopened,
  NeedInfo,

  OtherProduct,
  NeedDecisionLater,

  NotABug,
  WontFix,
  Duplicate,
  NotReproducible
}

public enum BugTrigger {
  StartTriage,
  StartFix,
  MarkFixed,

  Approve,
  Reject,
  Return,
  Reopen,

  NeedInfo,
  OtherProduct,
  NeedDecisionLater,

  MarkNotBug,
  MarkWontFix,
  MarkDuplicate,
  MarkNotRepro
}

public class Bug {
  private readonly StateMachine<BugState, BugTrigger> _machine;

  public BugState State => _machine.State;

  public Bug(BugState initialState = BugState.New) {
    _machine = new StateMachine<BugState, BugTrigger>(initialState);
    Configure();
  }

  public bool CanFire(BugTrigger trigger) => _machine.CanFire(trigger);
  public void Fire(BugTrigger trigger) => _machine.Fire(trigger);
  public void StartTriage() => _machine.Fire(BugTrigger.StartTriage);
  public void StartFix() => _machine.Fire(BugTrigger.StartFix);
  public void MarkFixed() => _machine.Fire(BugTrigger.MarkFixed);
  public void Approve() => _machine.Fire(BugTrigger.Approve);
  public void Reject() => _machine.Fire(BugTrigger.Reject);
  public void Return() => _machine.Fire(BugTrigger.Return);
  public void Reopen() => _machine.Fire(BugTrigger.Reopen);
  public void NeedInfo() => _machine.Fire(BugTrigger.NeedInfo);
  public void OtherProduct() => _machine.Fire(BugTrigger.OtherProduct);
  public void
  NeedDecisionLater() => _machine.Fire(BugTrigger.NeedDecisionLater);
  public void MarkNotBug() => _machine.Fire(BugTrigger.MarkNotBug);
  public void MarkWontFix() => _machine.Fire(BugTrigger.MarkWontFix);
  public void MarkDuplicate() => _machine.Fire(BugTrigger.MarkDuplicate);
  public void MarkNotRepro() => _machine.Fire(BugTrigger.MarkNotRepro);

  private void Configure() {
    _machine.Configure(BugState.New)
        .Permit(BugTrigger.StartTriage, BugState.Triage);

    _machine.Configure(BugState.Triage)
        .Permit(BugTrigger.StartFix, BugState.Fixing)
        .Permit(BugTrigger.NeedInfo, BugState.NeedInfo)
        .Permit(BugTrigger.OtherProduct, BugState.OtherProduct)
        .Permit(BugTrigger.NeedDecisionLater, BugState.NeedDecisionLater)
        .Permit(BugTrigger.MarkNotBug, BugState.NotABug)
        .Permit(BugTrigger.MarkWontFix, BugState.WontFix)
        .Permit(BugTrigger.MarkDuplicate, BugState.Duplicate)
        .Permit(BugTrigger.MarkNotRepro, BugState.NotReproducible);

    _machine.Configure(BugState.NeedInfo)
        .Permit(BugTrigger.Return, BugState.Returned);
    _machine.Configure(BugState.OtherProduct)
        .Permit(BugTrigger.Return, BugState.Returned);
    _machine.Configure(BugState.NeedDecisionLater)
        .Permit(BugTrigger.Return, BugState.Returned);
    _machine.Configure(BugState.NotABug)
        .Permit(BugTrigger.Return, BugState.Returned);
    _machine.Configure(BugState.WontFix)
        .Permit(BugTrigger.Return, BugState.Returned);
    _machine.Configure(BugState.Duplicate)
        .Permit(BugTrigger.Return, BugState.Returned);
    _machine.Configure(BugState.NotReproducible)
        .Permit(BugTrigger.Approve, BugState.Closed)
        .Permit(BugTrigger.Reject, BugState.Returned);

    _machine.Configure(BugState.Returned)
        .Permit(BugTrigger.StartTriage, BugState.Triage);

    _machine.Configure(BugState.Fixing)
        .Permit(BugTrigger.MarkFixed, BugState.Verification)
        .Permit(BugTrigger.NeedInfo, BugState.NeedInfo)
        .Permit(BugTrigger.OtherProduct, BugState.OtherProduct)
        .Permit(BugTrigger.NeedDecisionLater, BugState.NeedDecisionLater)
        .Permit(BugTrigger.MarkNotRepro, BugState.NotReproducible);

    _machine.Configure(BugState.Verification)
        .Permit(BugTrigger.Approve, BugState.Closed)
        .Permit(BugTrigger.Reject, BugState.Returned);

    _machine.Configure(BugState.Closed)
        .Permit(BugTrigger.Reopen, BugState.Triage);

    _machine.Configure(BugState.Reopened)
        .Permit(BugTrigger.StartTriage, BugState.Triage);
  }
}

public static class Program {
  public static void Main() {
    var bug = new Bug();
    Console.WriteLine($"Initial: {bug.State}");

    bug.StartTriage();
    Console.WriteLine($"Start triage: {bug.State}");

    bug.StartFix();
    Console.WriteLine($"Start fix: {bug.State}");

    bug.MarkFixed();
    Console.WriteLine($"Mark fixed: {bug.State}");

    bug.Approve();
    Console.WriteLine($"Approve: {bug.State}");

    bug.Reopen();
    Console.WriteLine($"Reopen: {bug.State}");

    bug.StartTriage();
    Console.WriteLine($"Reopened -> Start triage: {bug.State}");
  }
}