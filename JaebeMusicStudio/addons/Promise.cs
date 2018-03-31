using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JaebeMusicStudio.addons
{
    class Promise<T>
    {
        public delegate void ResolveDel(T returned);
        public delegate void RejectDel(Exception e);
        public delegate void PromiseFunction(ResolveDel resolve, RejectDel reject);
        public Promise(PromiseFunction fun)
        {
            try
            {
                fun(Resolve, Reject);
            }
            catch (Exception e)
            {
                Reject(e);
            }
        }
        List<Action> waitingOnCompletion = new List<Action>();
        public Promise()
        {

        }
        public PromiseAwaiter GetAwaiter()
        {
            return new PromiseAwaiter(this);
        }
        public void Resolve(T returned)
        {
            lock (this)
            {
                this.result = returned;
                this.completed = true;
            }
            foreach(var x in waitingOnCompletion)
            {
                try
                {
                    x();
                }
                catch { }
            }
        }
        public void Reject(Exception e)
        {

        }
        private bool completed = false;
        private T result = default(T);
        internal class PromiseAwaiter : INotifyCompletion
        {
            private Promise<T> parent;

            public PromiseAwaiter(Promise<T> parent)
            {
                this.parent = parent;
            }

            public bool IsCompleted
            {
                get
                {
                    return parent.completed;
                }
            }


            public T GetResult() { return this.parent.result; }


            public void OnCompleted(Action continuation)
            {
                lock (parent)
                {
                    if (parent.completed)
                        continuation();
                    else
                        parent.waitingOnCompletion.Add(continuation);
                }
            }

            public void UnsafeOnCompleted(Action continuation) { OnCompleted(continuation); }
        }
    }

}
