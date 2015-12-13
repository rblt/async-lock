using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Machine.Specifications;
using System.Collections.Concurrent;
using rblt.Threading;

namespace rblt.Tests.Threading
{
    [Subject("Tools.AsyncLock")]
    public class When_accessing_resource_with_different_token : AsyncLockTestBase
    {        
        Establish context = () =>
        {
            SetupMocks();

            Token1 = new object();
            Token2 = new object();

            AllTasks = Task.WhenAll(SampleTaskForToken(Token1), SampleTaskForToken(Token2));
        };


        Because of = () => AllTasks.Await();


        It should_be_accessed_at_least_once_at_a_time = () => Var.MaxValue.ShouldBeGreaterThanOrEqualTo(1);
        It should_be_accessed_maximum_twice_at_a_time = () => Var.MaxValue.ShouldBeLessThanOrEqualTo(2);
        It should_hold_no_remaining_locks = () => AsyncLock.GlobalCount.ShouldEqual(0);


        static object Token1;
        static object Token2;
        static Task AllTasks;       
    }
}
