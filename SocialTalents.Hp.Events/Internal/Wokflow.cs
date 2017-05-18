using SocialTalents.Hp.Events.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.Hp.Events.Internal
{
    internal class Workflow<TArg>
    {
        internal List<WorkflowStepDelegate<TArg>> _steps = new List<WorkflowStepDelegate<TArg>>();

        internal Workflow()
        {

        }

        internal event WorkflowStepDelegate<TArg> OnEvent
        {
            add
            {
                _steps.Add(value);
            }
            remove
            {
                if (_steps.Contains(value))
                    _steps.Remove(value);
            }
        }

        internal void Execute(TArg args)
        {
            foreach (var stepHandler in _steps)
            {
                try
                {
                    stepHandler(args);
                }
                catch (AbortWorkflowException)
                { break; }
            }
        }

        internal Workflow<TArg> AddStep(WorkflowStepDelegate<TArg> handler)
        {
            OnEvent += handler;
            return this;
        }
    }
}
