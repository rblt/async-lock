using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Machine.Specifications;
using rblt.Threading;

namespace rblt.Tests.Threading
{
    [Subject("AsyncLock")]
    public class When_accessing_resource_with_same_tokens : AsyncLockTestBase
    {        
        Establish context = () =>
        {
            SetupMocks();

            Token = new object();

            AllTasks = Task.WhenAll(SampleTaskForToken(Token), SampleTaskForToken(Token));
        };


        Because of = () => AllTasks.Await();


        It should_be_accessed_once_at_a_time = () => Var.MaxValue.ShouldBeLessThan(2);
        It should_hold_no_remaining_locks = () => AsyncLock.GlobalCount.ShouldEqual(0);


        static object Token;
        static Task AllTasks;       
    }
}
