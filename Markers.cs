using Newtonsoft.Json.Linq;
using Oxide.Core.Plugins;
using System.Collections.Generic;
using System.Linq;
using System;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries.Covalence;
using Oxide.Game.Rust;
using UnityEngine;
using Oxide.Game.Rust.Cui;
using Network;
using Newtonsoft.Json;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;


namespace Oxide.Plugins
{
    [Info("Markers", "Leon", "1.0.0")]
    [Description("Markers")]
    class Markers : CovalencePlugin
    {
        #region Definitions

        #endregion Definitions

        private const string SphereEnt = "assets/prefabs/visualization/sphere.prefab";
        private const string MarkerEnt = "assets/prefabs/tools/map/genericradiusmarker.prefab";
        private Dictionary<string, List<BaseEntity>> Spheres = new Dictionary<string, List<BaseEntity>>();
        private Dictionary<string, MapMarkerGenericRadius> markers = new Dictionary<string, MapMarkerGenericRadius>();
        public int darkness = 3;

        public static class Sizes {
            public static int person = 5;
            public static int area = 20;
            public static int largeArea = 50;
        }

        [HookMethod("clearSphere")]
        void clearSphereCommand(IPlayer player) {
            Puts("Clearing domes");
            DestroyAllSpheres();
        }

        [HookMethod("updateSphere")]
        void updateSphereCommand(IPlayer player) {
            Puts("Updating domes");
            var playerPosition = getPlayerPositionVector(player);          
         //   UpdateAllDomes(playerPosition);
        }

        void OnServerShutdown() {
            DestroyAllSpheres();
        }

        Vector3 getPlayerPositionVector(IPlayer player) {
            var playerPosition = player.Position();
            if (playerPosition == null) return new Vector3();
            return new Vector3(playerPosition.X, playerPosition.Y, playerPosition.Z);
        }

        void updateSphere(string name, Vector3 position) {
            foreach (var sphere in Spheres[name]) {
                sphere.transform.position = position;
            }
        }

        void OnPluginLoaded(Plugin plugin) {
        }

        private void Unload()
        {
           // InvokeHandler.Instance.CancelInvoke(UpdateMarkers);
            DestroyAllSpheres();         
        }


        [Command("newmarker")]
        void createMarkerCommand(IPlayer player) {
            var position = getPlayerPositionVector(player);  

            //createMarker("most-wanted", position, 5f);
        }

        [HookMethod("markAndDome")]
        void markAndDomeCommand(string name, IPlayer player, float radius, string colour) {
            var position = getPlayerPositionVector(player);  
            var _colour = UnityColour(0, 0f, 0f, 1f);

            //Puts($"Marking: {name} at {position.x} {position.x}");

            if (colour == "red") {
                _colour = UnityColour(230, 75f, 75f, 1f);
            }

            if (position == null) {
                Puts("Failed to markAndDome. Most wanted position not available.");
                return;
            }

            createMarker(name, position, radius, _colour, 0.4f, 5f);

            if (Spheres.ContainsKey(name)) {
                updateSphere(name, position);
            } else {
                createShere(name, position, 60);
            }
        }

        UnityEngine.Color UnityColour(float r, float g, float b, float alpha = 1f) {
            return new UnityEngine.Color(r / 255f, g / 255f, b / 255f, 1f);
        }

        void createMarker(string name, Vector3 position, float radius, UnityEngine.Color colour, float alpha, float deleteAfter = 0f) {
            var marker = GameManager.server.CreateEntity(MarkerEnt, position).GetComponent<MapMarkerGenericRadius>();
            marker.alpha = alpha;
            marker.color1 = colour;
            marker.color2 = colour;
            marker.radius = radius;
            marker.enabled = true;
            marker.Spawn();
            marker.SendUpdate();
     
            if (markers.ContainsKey(name)) {
                if (markers[name].IsValid())
                    markers[name].Kill();
                    
                markers.Remove(name);
            }

            if (deleteAfter > 0f) {
                InvokeHandler.Instance.Invoke(() => { 
                    if (marker != null && marker.IsValid())
                        marker.Kill();
                }, deleteAfter);
            }

            markers.Add(name, marker);
        }

        [Command("clearmarkers")/*, Permission("markers.admin")*/]
        void clearMarkers(IPlayer player) {
            DestroyAllSpheres();
        }

        [Command("clearmarkers2")/*, Permission("markers.admin")*/]
        void clearMarkers2(IPlayer player) {
            foreach(var GateToRemove in GameObject.FindObjectsOfType<MapMarkerGenericRadius>())
            {
            GateToRemove.Kill();
            GateToRemove.KillMessage();
            GateToRemove.SendUpdate();
            }
            
        }

        void createShere(string name, Vector3 position, float radius) {
            if (Spheres.ContainsKey(name)) {
                DestroySpheresFor(name);
            }

            for (int i = 0; i < darkness; i++) {
                BaseEntity sphere = GameManager.server.CreateEntity(SphereEnt, position, new Quaternion(), true);
                SphereEntity ent = sphere.GetComponent<SphereEntity>();
                ent.currentRadius = radius * 2;
                ent.lerpSpeed = 0f;

                sphere.Spawn();

                if (!Spheres.ContainsKey(name)) {
                    Spheres[name] = new List<BaseEntity>();
                }

                Spheres[name].Add(sphere);
            } 
        }
/*
        void OnEntitySpawned(BaseEntity entity) {
            if (entity.PrefabName == SphereEnt)
                Spheres.Add(entity);
        }
*/
        private void DestroySpheresFor(string name) {
            foreach (var sphere in Spheres[name])
                if (sphere != null)
                    sphere.KillMessage();
            Spheres[name].Clear();
        }

        private void DestroyAllSpheres() {
            foreach(var spheres in Spheres) {
                foreach (var sphere in spheres.Value)
                    if (sphere != null)
                        sphere.KillMessage();
                spheres.Value.Clear();
            }
        }

        public static string Colour(string hexColor, float alpha) {
            if (hexColor.StartsWith("#"))
                hexColor = hexColor.TrimStart('#');

            int red = int.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier);
            int green = int.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier);
            int blue = int.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier);

            return $"{(double)red / 255} {(double)green / 255} {(double)blue / 255} {alpha}";
        }    
    }
}