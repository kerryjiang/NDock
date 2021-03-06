﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRack.Base;
using NRack.Base.Configuration;
using NRack.Base.Provider;
using NRack.Server.Isolation;

namespace NRack.Server.Recycle
{
    [Export(typeof(IRecycleTrigger))]
    [ProviderMetadata(TriggerName)]
    public class AssemblyUpdatedRecycleTrigger : IRecycleTrigger
    {
        private int m_CheckInterval;
        private int m_RestartRelay;

        internal const string TriggerName = "AssemblyUpdatedTrigger";

        public string Name
        {
            get
            {
                return TriggerName;
            }
        }

        public bool Initialize(NameValueCollection options)
        {
            var checkInterval = 0;

            if (!int.TryParse(options.GetValue("checkInterval", "5"), out checkInterval))
                return false;

            m_CheckInterval = checkInterval;

            var restartDelay = 0;

            if (!int.TryParse(options.GetValue("restartDelay", "1"), out restartDelay))
                return false;

            m_RestartRelay = restartDelay;

            return true;
        }

        private bool IsDeplayOverdue(DateTime lastUpdatedTime)
        {
            if (lastUpdatedTime.AddMinutes(m_RestartRelay) <= DateTime.Now)
                return true;

            return false;
        }

        public bool NeedBeRecycled(IManagedApp app, StatusInfoCollection status)
        {
            var state = (app as IsolationApp).AssemblyUpdateState;

            // not in running state
            if (state == null)
                return false;

            if (state.LastUpdatedTime > state.CurrentAssemblyTime)
            {
                // check to see if there is any latest update
                // if yes, deplay much longer time
                state.TryCheckUpdate();
                return IsDeplayOverdue(state.LastUpdatedTime);
            }

            // next check time has not reached yet
            if (state.LastCheckTime.AddMinutes(m_CheckInterval) > DateTime.Now)
                return false;

            if (!state.TryCheckUpdate())
                return false;

            return IsDeplayOverdue(state.LastUpdatedTime);
        }
    }
}
