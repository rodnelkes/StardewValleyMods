using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace FarmHouseDataModifier
{
    internal sealed class ModEntry : Mod
    {
        private ModConfig _config;
        
        public override void Entry(IModHelper helper)
        {
            this._config = this.Helper.ReadConfig<ModConfig>();
            if (!this._config.EnableMod)
                return;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;
            
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this._config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this._config)
            );
            
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                getValue: () => this._config.EnableMod,
                setValue: value => this._config.EnableMod = value,
                name: () => "Enable Mod",
                tooltip: () => "Enables or disables the mod."
            );
            
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                getValue: () => this._config.GlobalSettings,
                setValue: value => this._config.GlobalSettings = value,
                name: () => "Global Settings",
                tooltip: () => "Will use the Global FarmHouseData for every new farm regardless of farm type."
            );
            
            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "FarmHouseData"
            );
            
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => "FarmHouseFlooring: Sets the initial farmhouse floor to the given ID when creating a new save.\nFormat: @flooring id>\n\nFarmHouseFurniture: Changes the starter furniture in the farmhouse when creating a new save. If you add multiple furniture to the same tile, the first one will be placed on the ground and the last one will be placed on the first one. A value of \"-1 0 0 0\" will not spawn any furniture at all.\nFormat: @furniture ID> @tile X> @tile Y> @rotations>\nSeparate multiple pieces of furniture by using a space. Until Generic Mod Config Menu updates to properly support editing long strings, modify this section in config.json.\n\nFarmHouseStarterSeedsPosition: Sets the tile position in the farmhouse where the seed package is placed when creating a new save.\nFormat: @tile X> @tile Y>\n\nFarmHouseWallpaper: Sets the initial farmhouse wallpaper to the given ID when creating a new save.\nFormat: @wallpaper id>\n"
            );

            for (var i = 0; i < this._config.FarmHouseData.GetLength(0); i++)
            {
                var farm = i;
                configMenu.AddSectionTitle(
                    mod: this.ModManifest,
                    text: () =>
                    {
                        return farm switch
                        {
                            0 => "Global",
                            1 => "Standard Farm",
                            2 => "Riverland Farm",
                            3 => "Forest Farm",
                            4 => "Hill-top Farm",
                            5 => "Wilderness Farm",
                            6 => "Four Corners Farm",
                            7 => "Beach Farm",
                            _ => ""
                        };
                    }
                );
                for (var j = 0; j < this._config.FarmHouseData.GetLength(1); j++)
                {
                    var property = j;
                    configMenu.AddTextOption(
                        mod: this.ModManifest,
                        getValue: () => this._config.FarmHouseData[farm, property],
                        setValue: value => this._config.FarmHouseData[farm, property] = value,
                        name: () => ((FarmMapProperty) property).ToString()
                    );
                }
            }
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            var whichFarm = Game1.whichFarm;
            if (!e.Name.IsEquivalentTo("Maps/" + Farm.getMapNameFromTypeInt(whichFarm)))
                return;
            e.Edit(asset =>
            {
                for (var i = 0; i < Enum.GetNames(typeof(FarmMapProperty)).Length; i++)
                {
                    asset.AsMap().Data.Properties[((FarmMapProperty)i).ToString()] = this._config.FarmHouseData[this._config.GlobalSettings ? 0 : whichFarm + 1, i];
                }
            });
        }
    }
}