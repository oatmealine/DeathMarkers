using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.DeathMarkers
{
	public class DeathMarkers : EverestModule
    {
		public static DeathMarkers Instance;

		public DeathMarkers()
        {
            Instance = this;
        }

		public override Type SettingsType => typeof(DeathMarkersSettings);
		DeathMarkersSettings Settings => (DeathMarkersSettings)Instance._Settings;

        public override Type SaveDataType => null;      
        public override void Load()
        {
			On.Celeste.Level.LoadLevel += Level_LoadLevel;
			On.Celeste.Player.Die += Player_Die;
			On.Celeste.PlayerDeadBody.Render += PlayerDeadBody_Render;
			On.Celeste.PlayerDeadBody.Update += PlayerDeadBody_Update;
        }

        public override void Unload()
        {
			On.Celeste.Player.Die -= Player_Die;
        }      

		private List<Vector2> deaths = new List<Vector2>();
		private List<float> deathspopupdelays = new List<float>();
		private Random GetRandom = new Random();

		private float deathTimer;

		private MTexture deathsprite;

		public override void Initialize()
		{
			base.Initialize();
			deathsprite = GFX.Game["objects/deathmarkers/youdied"];
		}

		private float limit(float n, float min, float max) {
			if(n > max) {
				return max;
			}
			if(n < min) {
				return min;
			}
			return n;
		}

		private PlayerDeadBody Player_Die(On.Celeste.Player.orig_Die orig, Player self, Microsoft.Xna.Framework.Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
		{
			PlayerDeadBody result = orig(self, direction, evenIfInvincible, registerDeathInStats);
			if (result == null) {
				return null;
			}
			
			if(Settings.RecordDeaths) {
				Vector2 newlocation = self.Position;
                newlocation.Y -= 16;
                deaths.Add(newlocation);            
			}

			deathspopupdelays.Clear();

			deaths.ForEach((_) =>
            {            
				deathspopupdelays.Add(GetRandom.NextFloat(0.25f));
            });         

			if(Settings.RecordDeaths) {
				deathspopupdelays[deaths.Count - 1] = 0f;
			}

			deathTimer = 0f;

			return result;
		}

		private void PlayerDeadBody_Render(On.Celeste.PlayerDeadBody.orig_Render orig, PlayerDeadBody self)
		{
			if (Settings.DisplayDeaths) {
                Level level = self.Scene as Level;
                int i = 0;

                deaths.ForEach((location) =>
                {
                    if(level.IsInBounds(location) && deathTimer >= deathspopupdelays[i]) {
                        float alpha = deathTimer - deathspopupdelays[i];
                        
						if(i == deaths.Count - 1) {
							alpha *= 2;
						}                  
                        if (alpha > 1f) {
                            alpha = 1f;
                        }
                  
                        deathsprite.DrawCentered(location, Color.White, alpha);
                    }

                    i++;
                });
            }  

			orig(self);
		}

		private void PlayerDeadBody_Update(On.Celeste.PlayerDeadBody.orig_Update orig, PlayerDeadBody self)
		{
			deathTimer += Engine.DeltaTime;
			orig(self);
		}


		private void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
		{
			if(playerIntro != Player.IntroTypes.Respawn) {
				deaths.Clear();
			}

			orig(self, playerIntro, isFromLoader);
		}

		public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot)
        {
            base.CreateModMenuSection(menu, inGame, snapshot);

            menu.Add(new TextMenu.Button(Dialog.Clean("modoptions_deathmarkers_resetdeaths")).Pressed(() => {
				deaths.Clear();
				Audio.Play("event:/ui/main/savefile_delete");
            }));
        }
    }
}
