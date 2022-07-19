﻿using Devon4Net.Infrastructure.Nexus.Model.Repositories.Base;
using System.Text.Json.Serialization;

namespace Devon4Net.Infrastructure.Nexus.Model.Repositories.ProxyRepositories
{
    public class BowerProxyRepository : NexusRepositoryProxy
    {
        [JsonPropertyName("bower")]
        public Bower Bower { get; set; }
    }
}



