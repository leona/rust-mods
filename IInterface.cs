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
    [Info("IInterface", "Leon", "1.0.0")]
    [Description("Interface wrapper")]
    class IInterface : CovalencePlugin
    {
        [PluginReference]
        private Plugin PlayerDatabase, MostWanted;

        public CuiElementContainer container;

        [Command("interface.exec")]
        void execConsoleCommand(IPlayer player, string command, string[] args) {
            if (args.Count() < 2) {
                player.Reply("Not enough arguments");
                return;
            }
            
            var basePlayer = player.Object as BasePlayer;

            var isChat = args[0] == "chat";
            SendCommand(basePlayer.Connection, args.Skip(1).ToArray(), isChat); 
        }

        [HookMethod("Colour")]
        public string Colour(string hex, float alpha) {
            return Theme.Color(hex, alpha);
        }

        [HookMethod("display")]
        public void display(string player, JArray data) {
            Puts($"Called Interface.Display for {player}");

            var menu = new MenuContainer();
            menu.addBackgroundButton();

            foreach(JObject item in data) {
                var type = item["type"].ToString();

                switch (type) {
                    case "button":
                        menu.addButton(item);
                        break;
                    case "label":
                        menu.addLabel(item);
                        break;
                    case "buttonCollection":
                        var collection = (JArray) item["collection"];
                        var parent = (JObject) item["parent"];

                        menu.addButtonCollection(collection, parent);
                        break;
                    case "panel":
                        menu.addPanel(item);
                        break;
                    case "table":
                        menu.addTable(item);
                        break;
                    default:
                        Puts("Component type does not exist");
                        break;
                }
            }

            var basePlayer = RustCore.FindPlayerByIdString(player);
            menu.display(basePlayer);
        }
        

        public static void SendCommand(Connection conn, string[] args, bool isChat) {
            if (!Net.sv.IsConnected())
                return;

            var command = string.Empty;
            var argsLength = args.Length;
            for (var i = 0; i < argsLength; i++)
                command += $"{args[i]} ";
            
            if (isChat)
                command = $"chat.say {command.QuoteSafe()}";
            
            Net.sv.write.Start();
            Net.sv.write.PacketID(Message.Type.ConsoleCommand);
            Net.sv.write.String(command);
            Net.sv.write.Send(new SendInfo(conn));
        }



    public static class Theme {

        public static Dictionary<string, string> colours = new Dictionary<string, string>() {
            { "background", Color("38393b", 0.98f) },
            { "navButtonBackground", Color("4d545c", 0.8f) },
            { "navButtonBackgroundSelected", Color("38393b", 0.6f) },
            { "navButtonColour", Color("ffffff", 0.8f) },
            { "colour", Color("ffffff", 0.8f) },
            { "buttonColour", Color("d2540b", 0.9f) },
            { "buttonBackground", Color("8f3114", 0.9f) }
        };

        public static string Color(string hexColor, float alpha) {
            if (hexColor.StartsWith("#"))
                hexColor = hexColor.TrimStart('#');

            int red = int.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier);
            int green = int.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier);
            int blue = int.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier);

            return $"{(double)red / 255} {(double)green / 255} {(double)blue / 255} {alpha}";
        }

        public static string GetColour(string colour) {
            if (Theme.colours.ContainsKey(colour)) {
                return Theme.colours[colour];
            } else {
                return colour;
            }
        }
    }

    public class Transform {
        public float width = 0f;
        public float height = 0f;
        public float top = 0f;
        public float bottom = 0f;
        public float left = 0f;
        public float right = 0f;

        public static Transform from(JObject data) {
            return new Transform() {
                width = (float) data["width"],
                height = (float) data["height"],
                top =  data["top"] != null ? (float) data["top"] : 0f,
                bottom = data["bottom"] != null ? (float) data["bottom"] : 0f,
                left = data["left"] != null ? (float) data["left"] : 0f,
                right = data["right"] != null ? (float) data["right"] : 0f
            };
        }

        public string anchorMin {
            get {
                var _left = 0f;
                var _top = 0f;

                if (left == 0f) {
                    _left = 1f - right - width;
                } else {
                    _left = left;
                }

                if (top == 0f) {
                    _top = bottom;
                } else {
                    _top = 1f - top - height;
                }

                return $"{_left} {_top}";
            }
        }
        public string anchorMax {
            get {
                var _left = 0f;
                var _top = 0f;

                if (left == 0f) {
                    _left = 1f - right;
                } else {
                    _left = left + width;
                }

                if (top == 0f) {
                    _top = bottom + height;
                } else {
                    _top = 1f - top;
                }

                return $"{_left} {_top}";
            }
        }
    }

    public class Panel {
        public string colour = Theme.colours["background"];
        public Transform transform;

        public static Panel from(JObject data) {
            return new Panel {
                colour = data["colour"] != null ? Theme.GetColour(data["colour"].ToString()) : Theme.colours["background"],
                transform = Transform.from(data["transform"] as JObject)
            };
        }
    }

    public class Table {
        public JArray data;
        public Transform transform;
        public int increment;
        public int pageSize;
        public string cmd;
        public string subCmd;

        public static Table from(JObject data) {
            return new Table {
                data = (JArray) data["data"],
                transform = Transform.from(data["transform"] as JObject),
                increment = (int) data["increment"],
                pageSize = (int) data["pageSize"],
                cmd = (string) data["cmd"],
                subCmd = data["subCmd"] == null ? "" : (string) data["subCmd"]
            };
        }
    }

    public class Button {
        public string text;
        public string colour;
        public string background;
        public int size = 14;
        public string command = "interface.exec chat /tpa";
        public Transform transform = new Transform();
        public float padding = 0.001f;

        public static Button from(JObject data) {
            return new Button {
                text = data["text"] != null ? data["text"].ToString() : "No text",
                colour = Theme.colours["colour"],
                background = data["background"] != null ? Theme.GetColour(data["background"].ToString()) : Theme.colours["background"],
                size =  data["size"] != null ? data["size"].ToObject<int>() : 14,
                command = data["command"] != null ? data["command"].ToString() : "interface.exec chat /tpa",
                transform = data["transform"] != null ? Transform.from(data["transform"] as JObject) : null,
                padding = data["padding"] != null ? (float) data["padding"] : 0.001f,
            };
        }
    }

    public class Label {
        public string text;
        public int size = 14;
        public string colour = Theme.colours["colour"];
        public Transform transform;
        public TextAnchor align = TextAnchor.MiddleCenter;

        public static Label from(JObject data) {
            var anchor = TextAnchor.MiddleCenter;

            if ((string) data["align"] == "left") {
                anchor = TextAnchor.MiddleLeft;
            }
            
            return new Label {
                text = data["text"] == null ? "No text" : data["text"].ToString(),
                colour = data["colour"] != null ? Theme.GetColour(data["colour"].ToString()) : Theme.colours["colour"],
                align = anchor,
                size =  data["size"] != null ? data["size"].ToObject<int>() : 14,
                transform = Transform.from(data["transform"] as JObject)
            };
        }
    }

    public class MenuContainer {
        public CuiElementContainer container;

        public MenuContainer() {
            container = new CuiElementContainer();
        }

        public void display(BasePlayer player) {
            CuiHelper.DestroyUi(player, "GameMenuCUI");
            CuiHelper.AddUi(player, container);
        }


        public void addButton(JObject data) {
            var button = Button.from(data);

            container.Add(new CuiButton {
                Text =
                {
                    Text = $"<color={button.colour}>{button.text}</color>",
                    FontSize = button.size,
                    Align = TextAnchor.MiddleCenter,
                    FadeIn = 0
                },
                Button =
                {
                    Color = button.background,
                    Command = $"interface.exec chat {button.command}",
                    FadeIn = 0
                },
                RectTransform = {
                    AnchorMin = button.transform.anchorMin,
                    AnchorMax = button.transform.anchorMax
                }
            }, "GameMenuCUI", "GameMenuCUIButton");
        }

        public void addButtonCollection(JArray collection, JObject _parent/*, horizontal/vertical*/) {
            var parent = Button.from(_parent);
            var itemWidth = (parent.transform.width / collection.Count()) - parent.padding;
            
            for (int index = 0; index < collection.Count; index++) {
                var button = Button.from((JObject) collection[index]);
                
                if (parent.colour == null) button.colour = parent.colour;
                if (parent.background != null) button.background = parent.background;
                if (button.size == null) button.size = parent.size;

                var left = parent.transform.left + (itemWidth * index);
                left += parent.padding * index;

                button.transform = new Transform {
                    height = parent.transform.height,
                    top = parent.transform.top,
                    width = itemWidth,
                    left = left
                };

                container.Add(new CuiButton {
                    Text =
                    {
                        Text = $"<color={button.colour}>{button.text}</color>",
                        FontSize = button.size,
                        Align = TextAnchor.MiddleCenter,
                        FadeIn = 0
                    },
                    Button =
                    {
                        Color = button.background,
                        Command = $"interface.exec chat {button.command}",
                        FadeIn = 0
                    },
                    RectTransform = {
                        AnchorMin = button.transform.anchorMin,
                        AnchorMax = button.transform.anchorMax
                    }
                }, "GameMenuCUI", "GameMenuCUIButton");
            
            }
        }

        public void addBackgroundButton() {
            container.Add(new CuiButton {
                Button =
                {
                    Close = "GameMenuCUI",
                    Color = "0.0 0.0 0.0 0.0",
                    FadeIn = 0
                },
                Text =
                {
                    Text = string.Empty
                },
                RectTransform =
                {
                    AnchorMin = "0.0 0.0",
                    AnchorMax = "1.0 1.0"
                }
            }, name:"GameMenuCUI");
        }

        public void addPanel(JObject _panel) {
            var panel = Panel.from(_panel);

            container.Add(new CuiPanel {
                Image =
                {
                    Color = panel.colour,
                    FadeIn = 0
                },
                CursorEnabled = true,
                RectTransform = {
                    AnchorMin = panel.transform.anchorMin,
                    AnchorMax = panel.transform.anchorMax
                }
            }, "GameMenuCUI", "GameMenuCUIBackground");
        }

        public void addTable(JObject _table) {
            var table = Table.from(_table);
            var first = table.data.FirstOrDefault();

            if (first == null) {
                Console.WriteLine("Table data does not exist. Skipping.");
                return;
            }
            
            var columns = (first as JObject).Properties().Select(p => p.Name).ToList();
            var left = table.transform.left;
            var columnWidth = table.transform.width / columns.Count();
            var rowHeight = 0.03f;
            TextAnchor align;

            if ((string) _table["align"] == "left") {
                align = TextAnchor.MiddleLeft;
            } else {
                align = TextAnchor.MiddleCenter;
            }

            foreach(var _column in columns) {
                string column = _column;
                if (column.Contains("<button>")) {
                    column = column.Replace("<button>", "");
                }
                var label = new Label {
                    text = column, 
                    colour = Theme.Color("ffffff", 0.8f), 
                    size = 14,
                    align = align,
                    transform = new Transform {
                        width = columnWidth,
                        height = rowHeight,
                        top = table.transform.top + 0.01f,
                        left = left
                    }
                };

                container.Add(new CuiLabel {
                    Text =
                    {
                        Text = label.text,
                        Align = label.align,
                        Color = label.colour,
                        FadeIn = 0,
                        FontSize = label.size
                    },
                    RectTransform = {
                        AnchorMin = label.transform.anchorMin,
                        AnchorMax = label.transform.anchorMax
                    }
                }, "GameMenuCUI", "GameMenuCUIBackgroundText");

                left += columnWidth;
            }
            
            if (table.data.Count() > table.pageSize) {
                Console.WriteLine($"Opening new table /{table.cmd} {table.increment - table.pageSize}");
                var previous = table.increment - table.pageSize;
                var next = table.increment + table.pageSize;

                addButtonCollection(
                    new JArray() { 
                        new JObject() {
                            { "text",  "Previous Page" },
                            { "command",  $"/{table.cmd} {previous < 0 ? 0 : previous}" },
                        },
                        new JObject() {
                            { "text",  "Next Page" },
                            { "command",  $"/{table.cmd} {next}" }
                        },
                    },
                    new JObject() {
                        { "colour", "colour" },
                        { "background", "background" },
                        {
                            "transform", 
                            new JObject() { 
                                { "width", 0.2f },
                                { "height", 0.05f },
                                { "top", 0.8f },
                                { "left", 0.4 }
                            }
                        },
                    }
                );
            }
            
            var index = 0;

            foreach(var row in table.data.Skip(table.increment).Take(table.pageSize)) {
                left = table.transform.left;
                index++;
                var columnIter = 0;

                foreach(var column in columns) {
                    var top = table.transform.top + (rowHeight * index) + 0.02f;

                    if (column.Contains("<button>")) {
                        addButton(new JObject() {
                            { "colour", "buttonColour" },
                            { "background", "buttonBackground" },
                            { "command", $"/{table.subCmd} {row[column]}" },
                            { "text", ">>" },
                            {
                                "transform", 
                                new JObject() { 
                                    { "width", columnWidth / 2 },
                                    { "height", rowHeight - 0.005f },
                                    { "top", top },
                                    { "left", left + columnWidth / 4 }
                                }
                            },
                        });

                        continue;
                    }

                    var label = new Label {
                        text = (string) row[column], 
                        colour = Theme.Color("ffffff", 0.8f), 
                        size = 14,
                        align = align,
                        transform = new Transform {
                            width = columnWidth,
                            height = rowHeight,
                            top = top,
                            left = left
                        }
                    };

                    container.Add(new CuiLabel {
                        Text =
                        {
                            Text = label.text == null ? "No text" : label.text,
                            Align = label.align,
                            Color = label.colour,
                            FadeIn = 0,
                            FontSize = label.size
                        },
                        RectTransform = {
                            AnchorMin = label.transform.anchorMin,
                            AnchorMax = label.transform.anchorMax
                        }
                    }, "GameMenuCUI", "GameMenuCUIBackgroundText");

                    
                    left += columnWidth;
                    columnIter++;
                }
            }
        }

        public void addLabel(JObject _label) {
            var label = Label.from(_label);

            container.Add(new CuiLabel {
                Text =
                {
                    Text = label.text,
                    Align = label.align,
                    Color = label.colour,
                    FadeIn = 0,
                    FontSize = label.size
                },
                RectTransform = {
                    AnchorMin = label.transform.anchorMin,
                    AnchorMax = label.transform.anchorMax
                }
            }, "GameMenuCUI", "GameMenuCUIBackgroundText");
        }

        public void addInput() {
            var testInput = new CuiElement {
                Name = "Test",
            // = panel,
                Components =
                {
                    new CuiInputFieldComponent {
                        Align = TextAnchor.MiddleLeft,
                        CharsLimit = 300,
                        Command = "/tpa" + "test",
                        FontSize = 15,
                        IsPassword = false,
                        Text = "test",
                    },
                    new CuiRectTransformComponent {AnchorMin = "0.2 0.2", AnchorMax = "0.8 0.8" }
                }
            };
        }
        }
    }
}
