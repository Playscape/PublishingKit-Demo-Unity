#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using System.Threading;
using Playscape.Internal;

namespace Playscape.Editor
{
    /// <summary>
    /// A class providing a way to invoke callbacks on the UI thread
    /// </summary>
    public class ThreadingEditorWindow : EditorWindow
	{
        /// <summary>
        /// Represents a generic callback
        /// </summary>
        private class Callback {
            
            /// <summary>
            /// The delegate to call
            /// </summary>
            public Delegate toCall;

            /// <summary>
            /// The delegate parameters
            /// </summary>
            public object[] args;
        }

        /// <summary>
        /// The maximum number of calls to invoke in each update call
        /// </summary>
        private const int MAX_CALLS_PER_ITERATIONS = 20;

        /// <summary>
        /// A thread-safe queue of call-backs to invoke
        /// </summary>
        private Queue<Callback> mExecutionQueue = new Queue<Callback>();

        /// <summary>
        /// Reference to the UI thread
        /// </summary>
        private Thread mUIThread; 

        /// <summary>
        /// Constructs a new ThreadingEditorWindow instance
        /// </summary>
        public ThreadingEditorWindow()
        {
            mUIThread = Thread.CurrentThread;
        }

        /// <summary>
        /// Returns true if the current thread is the UI thread
        /// </summary>
        public bool isUIThread
        {
            get { return Thread.CurrentThread == mUIThread; }
        }

        /// <summary>
        /// Runs a function on the UI thread asynchronously 
        /// </summary>
        /// <param name="target">The delegate to run</param>
        /// <param name="arguments">the parameters of the delegate</param>
        public void RunOnUIThread(Delegate target, params object[] arguments)
        {
            lock (mExecutionQueue)
            {
                mExecutionQueue.Enqueue(new Callback { args = arguments, toCall = target });
                //L.E("message enequed " + mExecutionQueue.Count.ToString());
            }
        }

        /// <summary>
        /// Called by Unity 100 times a second. Make sure callbacks are handled
        /// </summary>
        void Update()
        {
            int i = MAX_CALLS_PER_ITERATIONS;
            while (mExecutionQueue.Count > 0 && i > 0)
            {
                Callback foo = null;
                lock (mExecutionQueue)
                {
                    if (mExecutionQueue.Count > 0)
                    {
                        foo = mExecutionQueue.Dequeue();
                    }
                }

                if (foo != null)
                {
                    foo.toCall.DynamicInvoke(foo.args);
                }

                --i;
            }

        }

	}
}
#endif