﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDock.Base.Config
{
    public class NDockConfig : IConfigurationSource
    {
        public IsolationMode Isolation { get; set; }

        public IEnumerable<IServerConfig> Servers { get; set; }
    }
}
