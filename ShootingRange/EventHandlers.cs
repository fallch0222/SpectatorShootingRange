﻿using System.Collections.Generic;

using MEC;

using Exiled.API.Features;
using Exiled.API.Features.Items;

using ShootingRange.API;
using System.Linq;

namespace ShootingRange 
{
    public class EventHandlers
    {
        private readonly PluginMain _plugin;
        public List<Player> FreshlyDead { get; } = new List<Player>();

        public EventHandlers(PluginMain plugin)
        {
            _plugin = plugin;
        }
        
        public void OnRoundStarted()
        {
            SpectatorRange range = _plugin.Config.UseRangeLocation ? new SpectatorRange(_plugin.Config.RangeLocation) : new SpectatorRange();
            range.SpawnTargets();
           

            if (_plugin.Config.UsePrimitives)
                range.SpawnPrimitives();

            _plugin.ActiveRange = range;

            Timing.RunCoroutine(WaitForRespawnCoroutine());
        }
        public void OnVerified(Exiled.Events.EventArgs.Player.VerifiedEventArgs ev)
        {
            Timing.CallDelayed(10f, () =>
            {
                if (ev.Player.IsDead)
                    ev.Player.Broadcast(PluginMain.Singleton.Config.DeathBroadcast);
            });
        }
        public void OnDied(Exiled.Events.EventArgs.Player.DiedEventArgs ev) => Timing.RunCoroutine(OnDiedCoroutine(ev.Player));
        
        public void OnShooting(Exiled.Events.EventArgs.Player.ShootingEventArgs ev)
        {
            if (_plugin.ActiveRange.HasPlayer(ev.Player))
            {
                Firearm gun = (Firearm)ev.Player.CurrentItem;
                gun.Ammo = gun.MaxAmmo;
            }
        }
        
        public IEnumerator<float> WaitForRespawnCoroutine()
        {
            for (;;)
            {
                if (Respawn.TimeUntilNextPhase < 20)
                {
                    foreach (Player plyr in Player.List.Where((plyr) => _plugin.ActiveRange.HasPlayer(plyr)))
                    {
                        _plugin.ActiveRange.RemovePlayer(plyr);
                        plyr.Broadcast(PluginMain.Singleton.Config.RespawnBroadcast, true);
                    }
                }

                yield return Timing.WaitForSeconds(15f);
                
                if (!Round.IsStarted)
                    break;
            }
        }
        private IEnumerator<float> OnDiedCoroutine(Player plyr)
        {
            FreshlyDead.Add(plyr);

            if (_plugin.Config.ForceSpectators)
            {
                yield return Timing.WaitForSeconds(0.5f);
                _plugin.ActiveRange.TryAdmit(plyr);
            }

            yield return Timing.WaitForSeconds(30f);
            FreshlyDead.Remove(plyr);

            if (plyr.IsDead)
                plyr.Broadcast(_plugin.Config.DeathBroadcast);
        }
        public void OnFinishingRecall(Exiled.Events.EventArgs.Scp049.FinishingRecallEventArgs ev)
        {
            ev.IsAllowed |= FreshlyDead.Contains(ev.Target) && _plugin.ActiveRange.HasPlayer(ev.Target);
        }
    }
}
