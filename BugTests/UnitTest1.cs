using Microsoft.VisualStudio.TestTools.UnitTesting;
using BugPro;
using System;

namespace BugTests
{
    [TestClass]
    public class BugStateMachineTests
    {
        private Bug _bug = null!;

        [TestInitialize]
        public void Setup()
        {
            _bug = new Bug("Test Bug");
        }

        private void AssertThrowsInvalidOp(Action action)
        {
            try
            {
                action();
                Assert.Fail("Ожидалось исключение InvalidOperationException, но его не было.");
            }
            catch (InvalidOperationException)
            {
                // Invalid Operation Exception
            }
            catch (Exception ex)
            {
                Assert.Fail($"Ожидалось InvalidOperationException, но получено {ex.GetType().Name}");
            }
        }

        [TestMethod]
        public void InitialState_ShouldBeOpen() 
            => Assert.AreEqual(State.Open, _bug.CurrentState);

        [TestMethod]
        public void Open_AssignTrigger_ChangesToAssigned()
        {
            _bug.Assign("Dev1");
            Assert.AreEqual(State.Assigned, _bug.CurrentState);
        }

        [TestMethod]
        public void Assigned_AssigneeIsCorrectlySet()
        {
            _bug.Assign("Dev1");
            Assert.AreEqual("Dev1", _bug.Assignee);
        }

        [TestMethod]
        public void Assigned_DeferTrigger_ChangesToDeferred()
        {
            _bug.Assign("Dev1");
            _bug.Defer();
            Assert.AreEqual(State.Deferred, _bug.CurrentState);
        }

        [TestMethod]
        public void Deferred_AssignTrigger_ReturnsToAssigned()
        {
            _bug.Assign("Dev1");
            _bug.Defer();
            _bug.Assign("Dev2");
            Assert.AreEqual(State.Assigned, _bug.CurrentState);
        }

        [TestMethod]
        public void Assigned_Resolve_MovesToInTesting()
        {
            _bug.Assign("Dev1");
            _bug.SendToTest();
            Assert.AreEqual(State.InTesting, _bug.CurrentState);
        }

        [TestMethod]
        public void InTesting_Reject_MovesBackToAssigned()
        {
            _bug.Assign("Dev1");
            _bug.SendToTest();
            _bug.Reject();
            Assert.AreEqual(State.Assigned, _bug.CurrentState);
        }

        [TestMethod]
        public void InTesting_Resolve_MovesToResolved()
        {
            _bug.Assign("Dev1");
            _bug.SendToTest();
            _bug.Resolve();
            Assert.AreEqual(State.Resolved, _bug.CurrentState);
        }

        [TestMethod]
        public void Resolved_Close_MovesToClosed()
        {
            _bug.Assign("Dev1");
            _bug.SendToTest();
            _bug.Resolve();
            _bug.Close();
            Assert.AreEqual(State.Closed, _bug.CurrentState);
        }

        [TestMethod]
        public void Closed_Reopen_MovesToReopened()
        {
            _bug.Assign("Dev1");
            _bug.SendToTest();
            _bug.Resolve();
            _bug.Close();
            _bug.Reopen();
            Assert.AreEqual(State.Reopened, _bug.CurrentState);
        }

        [TestMethod]
        public void Reopened_Assign_MovesToAssigned()
        {
            _bug.Assign("Dev1");
            _bug.SendToTest();
            _bug.Resolve();
            _bug.Close();
            _bug.Reopen();
            _bug.Assign("Dev3");
            Assert.AreEqual(State.Assigned, _bug.CurrentState);
        }

        [TestMethod]
        public void CanFire_AssignFromOpen_IsTrue() 
            => Assert.IsTrue(_bug.CanFire(Trigger.Assign));

        [TestMethod]
        public void CanFire_ResolveFromOpen_IsFalse() 
            => Assert.IsFalse(_bug.CanFire(Trigger.Resolve));

        [TestMethod]
        public void Reassigned_ShouldUpdateAssignee()
        {
            _bug.Assign("Dev1");
            _bug.Assign("Dev2");
            Assert.AreEqual("Dev2", _bug.Assignee);
        }

        [TestMethod]
        public void StateFlow_FullCycle_Works()
        {
            _bug.Assign("Dev1");
            _bug.SendToTest();
            _bug.Resolve();
            _bug.Close();
            Assert.AreEqual(State.Closed, _bug.CurrentState);
        }

        [TestMethod]
        public void Open_Close_ShouldThrowException() 
            => AssertThrowsInvalidOp(() => _bug.Close());

        [TestMethod]
        public void Deferred_Resolve_ShouldThrowException()
        {
            _bug.Assign("Dev1");
            _bug.Defer();
            AssertThrowsInvalidOp(() => _bug.Resolve());
        }

        [TestMethod]
        public void Resolved_Assign_ShouldThrowException()
        {
            _bug.Assign("Dev1");
            _bug.SendToTest();
            _bug.Resolve();
            AssertThrowsInvalidOp(() => _bug.Assign("Dev2"));
        }

        [TestMethod]
        public void Closed_Resolve_ShouldThrowException()
        {
            _bug.Assign("Dev1");
            _bug.SendToTest();
            _bug.Resolve();
            _bug.Close();
            AssertThrowsInvalidOp(() => _bug.Resolve());
        }

        [TestMethod]
        public void InTesting_Assign_ShouldThrowException()
        {
            _bug.Assign("Dev1");
            _bug.SendToTest();
            AssertThrowsInvalidOp(() => _bug.Assign("Dev2"));
        }

        [TestMethod]
        public void Open_Reject_ShouldThrowException() 
            => AssertThrowsInvalidOp(() => _bug.Reject());
    }
}