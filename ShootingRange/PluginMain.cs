﻿using System;

using Exiled.API.Features;

using ShootingRange.API;


using Player = Exiled.Events.Handlers.Player;
using Server = Exiled.Events.Handlers.Server;
using Scp049 = Exiled.Events.Handlers.Scp049;

namespace ShootingRange
{
    public class PluginMain : Plugin<Config>
    {
        public override string Author => "rayzer";
        public override string Name => "Spectator Shooting Range";
        public override Version Version => new(3, 0, 1);
       
        public static PluginMain Singleton { get; private set; }
        public EventHandlers EventHandler { get; private set; }
        public SpectatorRange ActiveRange { get; set; }

        public override void OnEnabled()
        {
            Singleton = this;   
            EventHandler = new EventHandlers(this);
            RegisterEvents();
            Config.DeathBroadcast.Show = !Config.ForceSpectators;

            base.OnEnabled();
        }
        public override void OnDisabled()
        {
            UnregisterEvents();
            EventHandler = null;
            Singleton = null;

            base.OnDisabled();
        }
        public void RegisterEvents()
        {

            Exiled.Events.Handlers.Player.Verified += EventHandler.OnVerified;
            Exiled.Events.Handlers.Player.Died += EventHandler.OnDied;
            Player.Shooting += EventHandler.OnShooting;
          
            Server.RoundStarted += EventHandler.OnRoundStarted;
            Scp049.FinishingRecall += EventHandler.OnFinishingRecall;
        }
        public void UnregisterEvents()
        {
            Server.RoundStarted -= EventHandler.OnRoundStarted;
            Player.Verified -= EventHandler.OnVerified;
            Player.Died -= EventHandler.OnDied;
            Player.Shooting -= EventHandler.OnShooting;
          
            Scp049.FinishingRecall -= EventHandler.OnFinishingRecall;
        }
    }
}
