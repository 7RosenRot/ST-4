using Microsoft.VisualStudio.TestTools.UnitTesting;
using BugPro;
using System;

namespace BugTests;

[TestClass]
public class BugStateMachineTests {
  private Bug _bug = null!;

  [TestInitialize]
  public void Setup() { _bug = new Bug(); }

  [TestMethod]
  public void Test01_InitialState_ShouldBeNew() {
    Assert.AreEqual(BugState.New, _bug.State);
  }

  [TestMethod]
  public void Test02_New_StartTriage_TransitionsToTriage() {
    _bug.Fire(BugTrigger.StartTriage);

    Assert.AreEqual(BugState.Triage, _bug.State);
  }

  [TestMethod]
  public void Test03_New_InvalidTrigger_ThrowsInvalidOperationException() {
    Assert.ThrowsException<InvalidOperationException>(
        () => _bug.Fire(BugTrigger.Approve));
  }

  [TestMethod]
  public void Test04_Triage_SendToDev_TransitionsToInDevelopment() {
    _bug.Fire(BugTrigger.StartTriage);
    _bug.Fire(BugTrigger.StartFix);

    Assert.AreEqual(BugState.Fixing, _bug.State);
  }

  [TestMethod]
  public void Test05_Triage_MarkNotADefect_TransitionsToNotADefect() {
    _bug.Fire(BugTrigger.StartTriage);
    _bug.Fire(BugTrigger.MarkNotBug);

    Assert.AreEqual(BugState.NotABug, _bug.State);
  }

  [TestMethod]
  public void Test06_Triage_MarkWontFix_TransitionsToWontFix() {
    _bug.Fire(BugTrigger.StartTriage);
    _bug.Fire(BugTrigger.MarkWontFix);

    Assert.AreEqual(BugState.WontFix, _bug.State);
  }

  [TestMethod]
  public void Test07_Triage_MarkDuplicate_TransitionsToDuplicate() {
    _bug.Fire(BugTrigger.StartTriage);
    _bug.Fire(BugTrigger.MarkDuplicate);

    Assert.AreEqual(BugState.Duplicate, _bug.State);
  }

  [TestMethod]
  public void Test08_InDevelopment_ProvideFix_TransitionsToReadyForTesting() {
    NavigateToDevState();
    _bug.Fire(BugTrigger.MarkFixed);

    Assert.AreEqual(BugState.Verification, _bug.State);
  }

  [TestMethod]
  public void
  Test09_InDevelopment_MarkCannotReproduce_TransitionsToCannotReproduce() {
    NavigateToDevState();
    _bug.Fire(BugTrigger.MarkNotRepro);

    Assert.AreEqual(BugState.NotReproducible, _bug.State);
  }

  [TestMethod]
  public void Test10_InDevelopment_DeferNoTime_ReturnsToTriage() {
    NavigateToDevState();
    _bug.Fire(BugTrigger.NeedDecisionLater);

    Assert.AreEqual(BugState.NeedDecisionLater, _bug.State);
  }

  [TestMethod]
  public void Test11_InDevelopment_DeferSeparateDecision_ReturnsToTriage() {
    NavigateToDevState();
    _bug.Fire(BugTrigger.NeedDecisionLater);

    Assert.AreEqual(BugState.NeedDecisionLater, _bug.State);
  }

  [TestMethod]
  public void Test12_InDevelopment_DeferOtherProduct_ReturnsToTriage() {
    NavigateToDevState();
    _bug.Fire(BugTrigger.OtherProduct);

    Assert.AreEqual(BugState.OtherProduct, _bug.State);
  }

  [TestMethod]
  public void Test13_InDevelopment_RequestMoreInfo_ReturnsToTriage() {
    NavigateToDevState();
    _bug.Fire(BugTrigger.NeedInfo);

    Assert.AreEqual(BugState.NeedInfo, _bug.State);
  }

  [TestMethod]
  public void Test14_ReadyForTesting_VerifyOk_TransitionsToClosed() {
    NavigateToReadyForTestingState();
    _bug.Fire(BugTrigger.Approve);

    Assert.AreEqual(BugState.Closed, _bug.State);
  }

  [TestMethod]
  public void Test15_ReadyForTesting_VerifyFailed_ReturnsToTriage() {
    NavigateToReadyForTestingState();
    _bug.Fire(BugTrigger.Reject);

    Assert.AreEqual(BugState.Returned, _bug.State);
  }

  [TestMethod]
  public void Test16_CannotReproduce_VerifyOk_TransitionsToClosed() {
    NavigateToCannotReproduceState();
    _bug.Fire(BugTrigger.Approve);

    Assert.AreEqual(BugState.Closed, _bug.State);
  }

  [TestMethod]
  public void Test17_CannotReproduce_VerifyFailed_ReturnsToTriage() {
    NavigateToCannotReproduceState();
    _bug.Fire(BugTrigger.Reject);

    Assert.AreEqual(BugState.Returned, _bug.State);
  }

  [TestMethod]
  public void Test18_Closed_Reopen_ReturnsToTriage() {
    NavigateToReadyForTestingState();
    _bug.Fire(BugTrigger.Approve);

    _bug.Fire(BugTrigger.Reopen);

    Assert.AreEqual(BugState.Triage, _bug.State);
  }

  [TestMethod]
  public void Test19_CannotApprove_From_New() {
    Assert.ThrowsException<System.InvalidOperationException>(
        () => _bug.Fire(BugTrigger.Approve));
  }

  [TestMethod]
  public void Test20_CannotFix_From_Closed() {
    NavigateToVerification();
    _bug.Fire(BugTrigger.Approve);

    Assert.ThrowsException<System.InvalidOperationException>(
        () => _bug.Fire(BugTrigger.StartFix));
  }

  [TestMethod]
  public void Test21_CannotReject_In_Triage() {
    _bug.Fire(BugTrigger.StartTriage);

    Assert.ThrowsException<System.InvalidOperationException>(
        () => _bug.Fire(BugTrigger.Reject));
  }

  [TestMethod]
  public void Test22_DoubleStartTriage_Throws() {
    _bug.Fire(BugTrigger.StartTriage);

    Assert.ThrowsException<System.InvalidOperationException>(
        () => _bug.Fire(BugTrigger.StartTriage));
  }

  private void NavigateToDevState() {
    _bug.Fire(BugTrigger.StartTriage);
    _bug.Fire(BugTrigger.StartFix);
  }

  private void NavigateToFixing() {
    _bug.Fire(BugTrigger.StartTriage);
    _bug.Fire(BugTrigger.StartFix);
  }

  private void NavigateToVerification() {
    NavigateToFixing();

    _bug.Fire(BugTrigger.MarkFixed);
  }

  private void NavigateToReadyForTestingState() {
    NavigateToDevState();

    _bug.Fire(BugTrigger.MarkFixed);
  }

  private void NavigateToCannotReproduceState() {
    NavigateToDevState();

    _bug.Fire(BugTrigger.MarkNotRepro);
  }
}