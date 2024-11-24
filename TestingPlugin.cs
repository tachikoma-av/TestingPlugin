using System;
using System.IO;
using System.Collections.Generic;

using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;

using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;


using GameOffsets.Native;
using Stack = ExileCore.PoEMemory.Components.Stack;
using System.Linq;


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TestingPlugin;


public class TestingPlugin : BaseSettingsPlugin<TestingPluginSettings>
{

    public dynamic readConfigFromJson(string test_config_path){
        // Read the JSON content from the file
        string json = File.ReadAllText(test_config_path);
        // Deserialize JSON into a dynamic object
        dynamic data = JsonConvert.DeserializeObject(json);
        return data;
    }
    
    public class TestResult{
        public string name { get; set; }
        public bool passed { get; set; } = true;
        public List<string> failure_reasons { get; set; } = new List<string>();
        public TestResult(string name){
            this.name = name;
        }
        public void markAsFailed(string reason){
            this.passed = false;
            this.failure_reasons.Add(reason);
        }
        public string failure_reasonsAsString(){
            return string.Join(" ", this.failure_reasons);
        }

        public string reportAsString(){
            string result = $"passed: {this.passed}";
            if (this.failure_reasons.Count != 0){
                result += "\n";
                result += "failure_reasons:\n";
                result += string.Join("\n", this.failure_reasons);
            }
            return result;
        }
    }
    public string getConfigPath(string test_name){
        return $"./Plugins/Source/TestingPlugin/test_samples/{test_name}.json";
    }
    public string getOutputResultsPath(string test_name){
        return $"./Plugins/Source/TestingPlugin/test_results/{test_name}.json";
    }
    public class EntityToTest{
        [JsonProperty("metadata")]
        public string Metadata { get; set; }

        [JsonProperty("grid_pos_x")]
        public int? GridPosX { get; set; }

        [JsonProperty("grid_pos_y")]
        public int? GridPosY { get; set; }

        [JsonProperty("targetable")]
        public bool? Targetable { get; set; }

        [JsonProperty("id")]
        public int? Id { get; set; } // Use `int?` to allow null values.

        [JsonProperty("render_name")]
        public string RenderName { get; set; }

        [JsonProperty("type_str")]
        public string TypeStr { get; set; }
        [JsonProperty("resource_path")]
        public string ResourcePath { get; set; }
    }

    public class ItemToTest{
        [JsonProperty("name")]
        public string Name { get; set; } // Name of the item

        [JsonProperty("render_path")]
        public string RenderPath { get; set; } // Path to the render image

        [JsonProperty("metadata")]
        public string Metadata { get; set; } // Path to the render metadata

        [JsonProperty("stack_size")]
        public int? StackSize { get; set; } // Nullable, as stack size might be absent

        [JsonProperty("grid_position")]
        public List<int> GridPosition { get; set; } // Array of 2 integers [x, y]

        [JsonProperty("grid_position_size")]
        public List<int> GridPositionSize { get; set; } // Array of 2 integers [width, height]

        [JsonProperty("socket_group")]
        public List<string> SocketGroup { get; set; } // Nullable list of socket groups (e.g., ["BB"])
        [JsonProperty("item_mods")]
        public List<string> ItemMods { get; set; } // Nullable list of item mods (e.g., ["whatever", "whatever"])
        
    }

    public TestResult testGameWindow(bool print_debug = false){
        string test_name = "testGameWindow";
        if (print_debug) DebugWindow.LogMsg($"[{test_name}] start");
        TestResult test_result = new TestResult(test_name);
        try {
            // Populate variables
            dynamic data = readConfigFromJson(getConfigPath(test_name));
            int expected_size_x = data.expected_size_x;
            int expected_size_y = data.expected_size_y;

            var window = GameController.Window.GetWindowRectangleTimeCache;
            var window_size_x = (int)window.TopRight.X - (int)window.BottomLeft.X;

            // do tests
            if (window_size_x != expected_size_x){
                test_result.markAsFailed($"window_size_x is {window_size_x}, expected to be {expected_size_x}");
            }
            var window_size_y = (int)window.BottomLeft.Y - (int)window.TopRight.Y;
            if (window_size_y != expected_size_y){
                test_result.markAsFailed($"window_size_y is {window_size_y}, expected to be {expected_size_y}");
            }
        } catch (Exception ex){
            test_result.markAsFailed(ex.Message);
            DebugWindow.LogMsg($"[{test_name}] throws exception -> {ex}");
        }
        if (print_debug) {
            DebugWindow.LogMsg($"[{test_name}] end, {test_result.reportAsString()}");
            string json = JsonConvert.SerializeObject(test_result, Formatting.Indented);
            File.WriteAllText(getOutputResultsPath(test_name), json);
        }
        return test_result;
    }
    public TestResult testGameStates(bool print_debug = false){
        var test_name = "testGameStates";
        if (print_debug)  DebugWindow.LogMsg($"[{test_name}] start");
        TestResult test_result = new TestResult(test_name);
        try {
            // Populate variables
            dynamic data = readConfigFromJson($"./Plugins/Source/TestingPlugin/test_samples/{test_name}.json");
            bool expected_loading_status = data.expected_loading_status;
            int expected_game_state = data.expected_game_state;

            // do tests
            if (GameController.Game.IsLoading != expected_loading_status){
                test_result.markAsFailed($"[{test_name}] GameController.Game.IsLoading {GameController.Game.IsLoading}, expected to be {expected_loading_status}");
            }
            // game state
            var game_state = 0; 
            if (GameController.Game.IsInGameState == true){
                game_state = 20;
            } else if (GameController.Game.IsLoginState == true){
                game_state = 1;
            } else if (GameController.Game.IsSelectCharacterState){
                game_state = 10;
            }

            if (game_state != expected_game_state){
                throw new Exception($"game_state {game_state}, expected to be {expected_game_state}");
            }
        } catch (Exception ex){
            test_result.markAsFailed(ex.Message);
            DebugWindow.LogMsg($"[{test_name}] throws exception -> {ex}");
        }
        if (print_debug) {
            DebugWindow.LogMsg($"[{test_name}] end, {test_result.reportAsString()}");
            string json = JsonConvert.SerializeObject(test_result, Formatting.Indented);
            File.WriteAllText(getOutputResultsPath(test_name), json);
        }
        return test_result;
    }
    public TestResult testPlayerInfo(bool print_debug = false){
        string test_name = "testPlayerInfo";
        if (print_debug)  DebugWindow.LogMsg($"[{test_name}] start");
        TestResult test_result = new TestResult(test_name);

        try {
            // Populate variables
            dynamic data = readConfigFromJson(getConfigPath(test_name));
            string expected_player_entity_render_name = data.expected_player_entity_render_name;
            string expected_player_entity_path = data.expected_player_entity_path;
            int expected_player_level = data.expected_player_level;
            int expected_player_strength = data.expected_player_strength;
            int expected_player_intelligence = data.expected_player_intelligence;
            int expected_player_dexterity = data.expected_player_dexterity;
            string expected_player_name = data.expected_player_name;

            // do tests
            var player_entity = GameController.Game.IngameState.Data.LocalPlayer;
            if (expected_player_entity_render_name != player_entity.RenderName){
                test_result.passed = false;
                test_result.failure_reasons.Add($"player_entity.RenderName {player_entity.RenderName}, expected to be {expected_player_entity_render_name}");
            }
            if (expected_player_entity_path != player_entity.Path){
                test_result.passed = false;
                test_result.failure_reasons.Add($"player_entity.Path {player_entity.Path}, expected to be {expected_player_entity_path}");
            }            
            Player player_component  = GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Player>();
            
            if (player_component == null){
                throw new Exception($"player_comp == null");
            }
            
            if (player_component.Level != expected_player_level){
                test_result.passed = false;
                test_result.failure_reasons.Add($"player_component.Level {player_component.Level}, expected to be {expected_player_level}");
            }
            if (player_component.Strength != expected_player_strength){
                test_result.passed = false;
                test_result.failure_reasons.Add($"player_component.Strength {player_component.Strength}, expected to be {expected_player_strength}");
            }
            if (player_component.Strength != expected_player_intelligence){
                test_result.passed = false;
                test_result.failure_reasons.Add($"player_component.Strength {player_component.Strength}, expected to be {expected_player_intelligence}");
            }
            if (player_component.Dexterity != expected_player_dexterity){
                test_result.passed = false;
                test_result.failure_reasons.Add($"player_component.Dexterity {player_component.Dexterity}, expected to be {expected_player_dexterity}");
            }
            if (player_component.PlayerName != expected_player_name){
                test_result.markAsFailed($"player_component.PlayerName {player_component.PlayerName}, expected to be {expected_player_name}");
            }

        } catch (Exception ex){
            test_result.markAsFailed(ex.Message);
            DebugWindow.LogMsg($"[{test_name}] throws exception -> {ex}");
        }
        if (print_debug) {
            DebugWindow.LogMsg($"[{test_name}] end, {test_result.reportAsString()}");
            string json = JsonConvert.SerializeObject(test_result, Formatting.Indented);
            File.WriteAllText(getOutputResultsPath(test_name), json);
        }
        return test_result;
    }
    public TestResult testArea(bool print_debug = false){
        string test_name = "testArea";
        if (print_debug)  DebugWindow.LogMsg($"[{test_name}] start");
        TestResult test_result = new TestResult(test_name);

        try {
            // Populate variables
            dynamic data = readConfigFromJson(getConfigPath(test_name));
            string expected_area_name = data.expected_area_name;
            string expected_area_raw_name = data.expected_area_raw_name;

            // do tests
            var area_object = GameController.Area.CurrentArea.Area;
            if (area_object.Name != expected_area_name){
                test_result.markAsFailed($"area_object.Name {area_object.Name}, expected {expected_area_name}");
            }
            if (area_object.RawName != expected_area_raw_name){
                test_result.markAsFailed($"area_object.RawName {area_object.RawName}, expected {expected_area_raw_name}");
            }

        } catch (Exception ex){
            test_result.markAsFailed(ex.Message);
            DebugWindow.LogMsg($"[{test_name}] throws exception -> {ex}");
        }
        if (print_debug) {
            DebugWindow.LogMsg($"[{test_name}] end, {test_result.reportAsString()}");
            string json = JsonConvert.SerializeObject(test_result, Formatting.Indented);
            File.WriteAllText(getOutputResultsPath(test_name), json);
        }
        return test_result;
    }
    public TestResult testPlayerSkills(bool print_debug = false){
        string test_name = "testPlayerSkills";
        if (print_debug)  DebugWindow.LogMsg($"[{test_name}] start");
        TestResult test_result = new TestResult(test_name);
        try {
            // Populate variables
            dynamic data = readConfigFromJson(getConfigPath(test_name));
            string expected_skill_name = data.expected_skill_name;
            bool expected_skill_use_status = data.expected_skill_use_status;
            int expected_skill_uses_per_100_seconds = data.expected_skill_uses_per_100_seconds;
            string skill_description_to_test_key = data.skill_description_to_test_key;
            int skill_description_to_test_value = data.skill_description_to_test_value;

            // do tests
            var skill_bar_element = GameController.IngameState.IngameUi.SkillBar.Skills.FirstOrDefault(skill => skill.Skill.InternalName == expected_skill_name);
            if (skill_bar_element == null){
                throw new Exception($"cannot find skill with internal name {expected_skill_name}");
            };
            var skill_element = skill_bar_element.Skill;

            if (skill_element.CanBeUsed != expected_skill_use_status){
                test_result.markAsFailed($"skill_element.CanBeUsed != expected_skill_use_status");
            };

            if (skill_element.HundredTimesAttacksPerSecond != expected_skill_uses_per_100_seconds){
                test_result.markAsFailed($"skill_element.HundredTimesAttacksPerSecond != expected_skill_uses_per_100_seconds");
            };
            var skill_stat = skill_element.Stats.FirstOrDefault(stat => stat.Key.ToString() == skill_description_to_test_key);
            // if (skill_stat != null){
            if (skill_stat.Equals(default(KeyValuePair<GameStat, int>))){
                throw new Exception($"cannot find skill_stat with key {skill_description_to_test_key}");
            }

            if (skill_stat.Value != skill_description_to_test_value){
                test_result.markAsFailed($"[{test_name}] skill_stat.Value != skill_description_to_test_value");
            }
        } catch (Exception ex){
            test_result.markAsFailed(ex.Message);
            DebugWindow.LogMsg($"[{test_name}] throws exception -> {ex}");
        }
        if (print_debug) {
            DebugWindow.LogMsg($"[{test_name}] end, {test_result.reportAsString()}");
            string json = JsonConvert.SerializeObject(test_result, Formatting.Indented);
            File.WriteAllText(getOutputResultsPath(test_name), json);
        }
        return test_result;
    }
    public TestResult testPlayerFlasks(bool print_debug = false){
        var test_name = "testPlayerFlasks";
        if (print_debug)  DebugWindow.LogMsg($"[{test_name}] start");
        TestResult test_result = new TestResult(test_name);
        try {
            // Populate variables
            dynamic data = readConfigFromJson(getConfigPath(test_name));
            int expected_flasks_count = data.expected_flasks_count;
            int flasks_count = 0;

            // do tests
            foreach (var inventory in GameController.IngameState.Data.ServerData.PlayerInventories){
                if (inventory.Inventory.InventType == InventoryTypeE.Flask){
                    var flask_items = inventory.Inventory.InventorySlotItems;
                    foreach (var flask_item in flask_items){
                        flasks_count += 1;
                    }
                    break;
                } else {
                    continue;
                }
            }

            if (expected_flasks_count != flasks_count){
                throw new Exception($"flasks_count {flasks_count}, expected {expected_flasks_count}");
            }

        } catch (Exception ex){
            test_result.markAsFailed(ex.Message);
            DebugWindow.LogMsg($"[{test_name}] throws exception -> {ex}");
        }
        if (print_debug) {
            DebugWindow.LogMsg($"[{test_name}] end, {test_result.reportAsString()}");
            string json = JsonConvert.SerializeObject(test_result, Formatting.Indented);
            File.WriteAllText(getOutputResultsPath(test_name), json);
        }
        return test_result;
    }
    public TestResult testSingleItem(Entity item_to_test, ItemToTest expected_item_to_test){
        string test_name = "testSingleItem";
        TestResult test_result = new TestResult(test_name);
        try{
            if (expected_item_to_test.Name != null){
                var testing_item_base_component = item_to_test.GetComponent<Base>();
                if (testing_item_base_component == null){
                    test_result.markAsFailed($"testing_item_base_component == null");
                } else if (testing_item_base_component.Name != expected_item_to_test.Name){
                    test_result.markAsFailed($"name {testing_item_base_component.Name}, expected {expected_item_to_test.Name}");
                }
            }
            
            if (expected_item_to_test.RenderPath != null){
                var testing_item_renderitem_component = item_to_test.GetComponent<RenderItem>();
                if (testing_item_renderitem_component == null){
                    test_result.markAsFailed("testing_item_renderitem_component == null");
                } else if (testing_item_renderitem_component.ResourcePath != expected_item_to_test.RenderPath){
                    test_result.markAsFailed($"ResourcePath {testing_item_renderitem_component.ResourcePath}, expected {expected_item_to_test.RenderPath}");
                }
            }

            if (expected_item_to_test.StackSize != null){
                var testing_item_stack_component = item_to_test.GetComponent<Stack>();
                if (testing_item_stack_component == null){
                    test_result.markAsFailed("testing_item_stack_component == null");
                } else if (testing_item_stack_component.Size != expected_item_to_test.StackSize){
                    test_result.markAsFailed($"StackSize {testing_item_stack_component.Size}, expected {expected_item_to_test.StackSize}");
                }
            }

            if (expected_item_to_test.SocketGroup != null){
                var sockets_component = item_to_test.GetComponent<Sockets>();
                int expected_socket_group_count = expected_item_to_test.SocketGroup.Count();
                int socket_group_count = sockets_component.SocketGroup.Count();
                if (socket_group_count == expected_socket_group_count){
                    for (int i = 0; i < socket_group_count; i++){
                        var testing_item_socket_node = sockets_component.SocketGroup[i];
                        var expected_item_socket_node = expected_item_to_test.SocketGroup[i];
                        if (testing_item_socket_node != expected_item_socket_node){
                            test_result.markAsFailed($"testing_item_socket_node {testing_item_socket_node}, expected {expected_item_socket_node}");
                            break;
                        }
                    }
                } else {
                    test_result.markAsFailed($"socket_group_count {socket_group_count}, expected {expected_socket_group_count}");
                }
            }


        } catch (Exception ex){
            test_result.markAsFailed(ex.Message);
        }
        return test_result;
    }


    public TestResult testInventory( bool print_debug = false){
        var test_name = "testInventory";
        if (print_debug)  DebugWindow.LogMsg($"[{test_name}] start");
        TestResult test_result = new TestResult(test_name);
        try {
            // Populate variables
            dynamic data = readConfigFromJson(getConfigPath(test_name));
            List<ItemToTest> expected_items_to_test = ((JArray)data["items_to_test"]).ToObject<List<ItemToTest>>();

            // do tests
            var inventory_items = GameController.IngameState.Data.ServerData.PlayerInventories[0].Inventory.InventorySlotItems;
            foreach (var expected_item_to_test in expected_items_to_test){
                var items_to_test = inventory_items.Where(item => item.Item.Metadata == expected_item_to_test.Metadata).ToList();
                if (items_to_test.Count() != 1){
                    test_result.markAsFailed($"items_to_test.Count() {items_to_test.Count()}, expected {1} metadata {expected_item_to_test.Metadata}");
                    continue;
                }
                
                var inventory_item = items_to_test[0];
                Entity item_to_test = inventory_item.Item; 
                TestResult item_test_result = testSingleItem(item_to_test, expected_item_to_test);
                if (item_test_result.passed == false){
                    foreach (var failure_reason in item_test_result.failure_reasons){
                        test_result.markAsFailed($"{expected_item_to_test.Metadata}: {failure_reason}");
                    }
                }

                // check grid pos
                if (expected_item_to_test.GridPosition != null){
                    if (inventory_item.PosX != expected_item_to_test.GridPosition[0] || inventory_item.PosY != expected_item_to_test.GridPosition[1]){
                        test_result.markAsFailed($"{expected_item_to_test.Metadata}: inventory_item.PosX != expected_item_to_test.GridPosition[0] || inventory_item.PosY != expected_item_to_test.GridPosition[1]");
                    }
                }
                
                if (expected_item_to_test.GridPositionSize != null){
                    if (inventory_item.SizeX != expected_item_to_test.GridPositionSize[0] || inventory_item.SizeY != expected_item_to_test.GridPositionSize[1]){
                        test_result.markAsFailed($"{expected_item_to_test.Metadata}: inventory_item.SizeX != expected_item_to_test.GridPositionSize[0] || inventory_item.SizeY != expected_item_to_test.GridPositionSize[1]");
                    }
                }
                
            }
            // if (detailed == true){
            // open inventory
            // check if there is no anything on [1,1]
            // move item to from [0,0] to [1,1]
            // check if position changed
            // move item to from [1,1] to [0,0]
            // check if hovered item changed
            // close inventory
            // }
        } catch (Exception ex){
            test_result.markAsFailed(ex.Message);
            DebugWindow.LogMsg($"[{test_name}] throws exception -> {ex}");
        }
        if (print_debug) {
            DebugWindow.LogMsg($"[{test_name}] end, {test_result.reportAsString()}");
            string json = JsonConvert.SerializeObject(test_result, Formatting.Indented);
            File.WriteAllText(getOutputResultsPath(test_name), json);
        }
        return test_result;
    }
    public TestResult testStash(bool print_debug = false){
        var test_name = "testStash";
        if (print_debug)  DebugWindow.LogMsg($"[{test_name}] start");
        TestResult test_result = new TestResult(test_name);
        try {
            // Populate variables
            dynamic data = readConfigFromJson(getConfigPath(test_name));
            int expected_stash_tab_index = data.expected_stash_tab_index;
            List<ItemToTest> expected_items_to_test = ((JArray)data["items_to_test"]).ToObject<List<ItemToTest>>();

            // do tests
            if (GameController.IngameState.IngameUi.StashElement.IsVisible == false){
                throw new Exception($"GameController.IngameState.IngameUi.StashElement.IsVisible == false");
            }
            var stash_tab_index = GameController.IngameState.IngameUi.StashElement.IndexVisibleStash;
            if (stash_tab_index != expected_stash_tab_index){
                test_result.markAsFailed($"stash_tab_index {stash_tab_index}, expected {expected_stash_tab_index}");
            }

            var current_tab_stash_items = GameController.IngameState.IngameUi.StashElement.AllInventories[stash_tab_index].VisibleInventoryItems;
            foreach (var expected_item_to_test in expected_items_to_test){
                var items_to_test = current_tab_stash_items.Where(item => item.Item.Metadata == expected_item_to_test.Metadata).ToList();
                if (items_to_test.Count() != 1){
                    test_result.markAsFailed($"items_to_test.Count() {items_to_test.Count()}, expected {1} metadata {expected_item_to_test.Metadata}");
                    continue;
                }
                
                var stash_item = items_to_test[0];
                Entity item_to_test = stash_item.Item; 
                TestResult item_test_result = testSingleItem(item_to_test, expected_item_to_test);
                if (item_test_result.passed == false){
                    foreach (var failure_reason in item_test_result.failure_reasons){
                        test_result.markAsFailed($"item_test_result with metadata {expected_item_to_test.Metadata}: {failure_reason}");
                    }
                }

                // // check grid pos
                // if (expected_item_to_test.GridPosition != null){
                //     if (inventory_item.PosX != expected_item_to_test.GridPosition[0] || inventory_item.PosY != expected_item_to_test.GridPosition[1]){
                //         test_result.markAsFailed($"inventory_item.PosX != expected_item_to_test.GridPosition[0] || inventory_item.PosY != expected_item_to_test.GridPosition[1]");
                //     }
                // }
                
                // if (expected_item_to_test.GridPositionSize != null){
                //     if (inventory_item.SizeX != expected_item_to_test.GridPositionSize[0] || inventory_item.SizeY != expected_item_to_test.GridPositionSize[1]){
                //         test_result.markAsFailed($"inventory_item.SizeX != expected_item_to_test.GridPositionSize[0] || inventory_item.SizeY != expected_item_to_test.GridPositionSize[1]");
                //     }
                // }
                
            }

        } catch (Exception ex){
            test_result.markAsFailed(ex.Message);
            DebugWindow.LogMsg($"[{test_name}] throws exception -> {ex}");
        }
        if (print_debug) {
            DebugWindow.LogMsg($"[{test_name}] end, {test_result.reportAsString()}");
            string json = JsonConvert.SerializeObject(test_result, Formatting.Indented);
            File.WriteAllText(getOutputResultsPath(test_name), json);
        }
        return test_result;
    }
    public TestResult testMapDevice(bool print_debug = false){
        var test_name = "testMapDevice";
        if (print_debug)  DebugWindow.LogMsg($"[{test_name}] start");
        TestResult test_result = new TestResult(test_name);
        try {
            // Populate variables
            dynamic data = readConfigFromJson(getConfigPath(test_name));
            List<int> expected_kirak_maps_variety = ((JArray)data.expected_kirak_maps_variety).ToObject<List<int>>();
            int expected_items_count = data.expected_items_count;

            // do tests
            if (GameController.IngameState.IngameUi.MapDeviceWindow.IsVisible == false){
                throw new Exception($"GameController.IngameState.IngameUi.MapDeviceWindow.IsVisible == false");
            }
            // public int slots_count { get; set; }
            // public List<InventoryObjectCustom_c> items { get; set; } // items in map device
            if (false){
                //TODO check if there is smth in map device and item is ok
                test_result.markAsFailed($"valid_items_in_map_device_count == 0");
            }
            // public List<int> k_m_c { get; set; } // kirak missions count [ white yellow red]
            List<int> kirak_missions_count = new List<int>();
            var kirak_missions_element = GameController.IngameState.IngameUi.MapDeviceWindow.ChooseMastersMods.Children[1];
            for (int i = 1; i < 4; i++){
                var kirak_mission_element = kirak_missions_element.Children[i];
                kirak_missions_count.Add(int.Parse(kirak_mission_element.Children[0].Text));
            };
            
            if (!kirak_missions_count.SequenceEqual(expected_kirak_maps_variety)){
                test_result.markAsFailed($"kirak_missions_count [{string.Join(", ", kirak_missions_count)}], expected [{string.Join(", ", expected_kirak_maps_variety)}]");
            }

            // public List<string> i_p { get; set; } // invintation_progress [ exarch maven eater]
            var map_device_craft_window_element = GameController.IngameState.IngameUi.MapDeviceWindow.Children[2].Children[0].Children[0]; 
            string map_device_craft_string_merged = "";
            int map_device_craft_elements_count = 0;
            foreach (var map_device_craft_element in map_device_craft_window_element.Children[1].Children){ 
                map_device_craft_elements_count += 1;
                map_device_craft_string_merged += map_device_craft_element.Children[0].Text; 
            }
            if (map_device_craft_elements_count == 0 || map_device_craft_string_merged == ""){
                test_result.markAsFailed($"[{test_name}] map_device_craft_elements_count == 0 || map_device_craft_string_merged == empty_string");
            }
            // public Posx1x2y1y2 a_b_p { get; set; } // activate_button_pos
        } catch (Exception ex){
            test_result.markAsFailed(ex.Message);
            DebugWindow.LogMsg($"[{test_name}] throws exception -> {ex}");
        }
        if (print_debug) {
            DebugWindow.LogMsg($"[{test_name}] end, {test_result.reportAsString()}");
            string json = JsonConvert.SerializeObject(test_result, Formatting.Indented);
            File.WriteAllText(getOutputResultsPath(test_name), json);
        }
        return test_result;
    }
    public TestResult testKirakMissions(bool print_debug = false){
        var test_name = "testKirakMissions";
        if (print_debug)  DebugWindow.LogMsg($"[{test_name}] start");
        TestResult test_result = new TestResult(test_name);
        try {
            // Populate variables
            dynamic data = readConfigFromJson(getConfigPath(test_name));
            List<int> expected_kirak_maps_variety = ((JArray)data.expected_kirak_maps_variety).ToObject<List<int>>();
            // do tests
            if (GameController.IngameState.IngameUi.ZanaMissionChoice.IsVisible == false){
                throw new Exception($"GameController.IngameState.IngameUi.ZanaMissionChoice.IsVisible == false");
            }
            var ui_element = GameController.IngameState.IngameUi.ZanaMissionChoice;
            var maps_header = ui_element.Children[0].Children[0];
            List<int> kirak_missions_count = new List<int>();
            foreach( var tab in maps_header.Children){
                var count = int.Parse(tab.Children[0].Text);
                kirak_missions_count.Add(count);
            }
            if (!kirak_missions_count.SequenceEqual(expected_kirak_maps_variety)){
                test_result.markAsFailed($"kirak_missions_count [{string.Join(", ", kirak_missions_count)}], expected [{string.Join(", ", expected_kirak_maps_variety)}]");
            }
        } catch (Exception ex){
            test_result.markAsFailed(ex.Message);
            DebugWindow.LogMsg($"[{test_name}] throws exception -> {ex}");
        }
        if (print_debug) {
            DebugWindow.LogMsg($"[{test_name}] end, {test_result.reportAsString()}");
            string json = JsonConvert.SerializeObject(test_result, Formatting.Indented);
            File.WriteAllText(getOutputResultsPath(test_name), json);
        }
        return test_result;
    }
    public TestResult testPurchaseHideoutWindow(bool print_debug = false){
        var test_name = "testPurchaseHideoutWindow";
        if (print_debug)  DebugWindow.LogMsg($"[{test_name}] start");
        TestResult test_result = new TestResult(test_name);
        try {
            dynamic data = readConfigFromJson(getConfigPath(test_name));
            int expected_items_count = data.expected_items_count;
            
            // do tests
            if (GameController.IngameState.IngameUi.PurchaseWindowHideout.IsVisible == false){
                throw new Exception($"GameController.IngameState.IngameUi.PurchaseWindowHideout.IsVisible == false");
            }
            var ui_element = GameController.IngameState.IngameUi.PurchaseWindowHideout;
            var tab_container = ui_element.TabContainer;
            int visible_items_count = 0;
            foreach (var normal_inventory_item in tab_container.VisibleStash.VisibleInventoryItems){
                var item = normal_inventory_item.Item;
                if (item == null){
                    continue;
                }
                visible_items_count += 1;
            }
            if (visible_items_count != expected_items_count){
                test_result.markAsFailed($"visible_items_count {visible_items_count}, expected {expected_items_count}");
            }
        } catch (Exception ex){
            test_result.markAsFailed(ex.Message);
            DebugWindow.LogMsg($"[{test_name}] throws exception -> {ex}");
        }
        if (print_debug) {
            DebugWindow.LogMsg($"[{test_name}] end, {test_result.reportAsString()}");
            string json = JsonConvert.SerializeObject(test_result, Formatting.Indented);
            File.WriteAllText(getOutputResultsPath(test_name), json);
        }
        return test_result;
    }
    public TestResult testSingleEntity(Entity entity_to_test, EntityToTest expected_entity_to_test){
        string test_name = "testSingleEntity";
        TestResult test_result = new TestResult(test_name);
        try{
            if (expected_entity_to_test.Id != null && entity_to_test.Id != expected_entity_to_test.Id){
                test_result.markAsFailed($"entity_to_test.Id {entity_to_test.Id}, expected {expected_entity_to_test.Id}");
            }
            if (expected_entity_to_test.GridPosX != null && (int)entity_to_test.GridPosNum.X != expected_entity_to_test.GridPosX){
                test_result.markAsFailed($"(int)entity_to_test.GridPosNum.X {(int)entity_to_test.GridPosNum.X}, expected {expected_entity_to_test.GridPosX}");
            }
            if (expected_entity_to_test.GridPosY != null && (int)entity_to_test.GridPosNum.Y != expected_entity_to_test.GridPosY){
                test_result.markAsFailed($"(int)entity_to_test.GridPosNum.Y {(int)entity_to_test.GridPosNum.Y}, expected {expected_entity_to_test.GridPosY}");
            }
            if (expected_entity_to_test.RenderName != null && entity_to_test.RenderName != expected_entity_to_test.RenderName){
                test_result.markAsFailed($"entity_to_test.RenderName {entity_to_test.RenderName}, expected {expected_entity_to_test.RenderName}");
            }
            if (expected_entity_to_test.Targetable != null && entity_to_test.IsTargetable != expected_entity_to_test.Targetable){
                test_result.markAsFailed($"entity_to_test.IsTargetable {entity_to_test.IsTargetable}, expected {expected_entity_to_test.Targetable}");
            }
            if (expected_entity_to_test.TypeStr != null && entity_to_test.Type.ToString() != expected_entity_to_test.TypeStr){
                test_result.markAsFailed($"entity_to_test.Type.ToString() {entity_to_test.Type.ToString()}, expected {expected_entity_to_test.TypeStr}");
            }
            if (expected_entity_to_test.ResourcePath != null){
                var world_item_component = entity_to_test.GetComponent<WorldItem>();
                if (world_item_component != null){
                    var item_entity = world_item_component.ItemEntity;
                    // icon?
                    var render_item_component = item_entity.GetComponent<RenderItem>();
                    if (render_item_component != null){
                        if (render_item_component.ResourcePath != expected_entity_to_test.ResourcePath){
                            test_result.markAsFailed($"render_item_component.ResourcePath {render_item_component.ResourcePath}, expected {expected_entity_to_test.ResourcePath}");
                        }
                    } else {
                        test_result.markAsFailed("render_item_component == null");
                    }
                } else {
                    test_result.markAsFailed("world_item_component == null");
                }
            }

        } catch (Exception ex){
            test_result.markAsFailed(ex.Message);
        }
        return test_result;
    }
    public TestResult testEntities(bool print_debug = false){
        var test_name = "testEntities";
        if (print_debug)  DebugWindow.LogMsg($"[{test_name}] start");
        TestResult test_result = new TestResult(test_name);
        try {
            dynamic data = readConfigFromJson(getConfigPath(test_name));
            string expected_area_raw_name = data.expected_area_raw_name;
            List<EntityToTest> expected_entities_to_test = ((JArray)data["entities_to_test"]).ToObject<List<EntityToTest>>();

            // do tests
            if (expected_area_raw_name != null && GameController.Area.CurrentArea.Area.RawName != expected_area_raw_name){
                test_result.markAsFailed($"area_object.RawName {GameController.Area.CurrentArea.Area.RawName}, expected {expected_area_raw_name}");
            }

            foreach (var expected_entity_to_test in expected_entities_to_test){
                var entities_to_test = GameController.EntityListWrapper.Entities.Where(e => e.Metadata == expected_entity_to_test.Metadata).ToList();
                if (entities_to_test.Count() != 1){
                    test_result.markAsFailed($"entities_to_test.Count() {entities_to_test.Count()}, expected {1} metadata {expected_entity_to_test.Metadata}");
                    continue;
                }
                Entity entity_to_test = entities_to_test[0]; 
                TestResult entity_test_result = testSingleEntity(entity_to_test, expected_entity_to_test);
                if (entity_test_result.passed == false){
                    foreach (var failure_reason in entity_test_result.failure_reasons){
                        test_result.markAsFailed($"entity_test_result with metadata {expected_entity_to_test.Metadata}: {failure_reason}");
                    }
                }
            }


        } catch (Exception ex){
            test_result.markAsFailed(ex.Message);
            DebugWindow.LogMsg($"[{test_name}] throws exception -> {ex}");
        }
        if (print_debug) {
            DebugWindow.LogMsg($"[{test_name}] end, {test_result.reportAsString()}");
            string json = JsonConvert.SerializeObject(test_result, Formatting.Indented);
            File.WriteAllText(getOutputResultsPath(test_name), json);
        }
        return test_result;
    }
    public TestResult testItemsOnGroundLabelsVisible(bool print_debug = false){
        var test_name = "testItemsOnGroundLabelsVisible";
        if (print_debug)  DebugWindow.LogMsg($"[{test_name}] start");
        TestResult test_result = new TestResult(test_name);
        try {
            dynamic data = readConfigFromJson(getConfigPath(test_name));
            string expected_area_raw_name = data.expected_area_raw_name;
            List<EntityToTest> expected_entities_to_test = ((JArray)data["entities_to_test"]).ToObject<List<EntityToTest>>();
            
            // do tests
            foreach (var expected_entity_to_test in expected_entities_to_test){
                var entities_to_test = GameController.IngameState.IngameUi.ItemsOnGroundLabelsVisible.Where(e => e.ItemOnGround.Metadata == expected_entity_to_test.Metadata).ToList();
                if (entities_to_test.Count() != 1){
                    test_result.markAsFailed($"entities_to_test.Count() {entities_to_test.Count()}, expected {1} metadata {expected_entity_to_test.Metadata}");
                    continue;
                }
                Entity entity_to_test = entities_to_test[0].ItemOnGround; 
                TestResult entity_test_result = testSingleEntity(entity_to_test, expected_entity_to_test);
                if (entity_test_result.passed == false){
                    foreach (var failure_reason in entity_test_result.failure_reasons){
                        test_result.markAsFailed($"entity_test_result with metadata {expected_entity_to_test.Metadata}: {failure_reason}");
                    }
                }
            }
        } catch (Exception ex){
            test_result.markAsFailed(ex.Message);
            DebugWindow.LogMsg($"[{test_name}] throws exception -> {ex}");
        }
        if (print_debug) {
            DebugWindow.LogMsg($"[{test_name}] end, {test_result.reportAsString()}");
            string json = JsonConvert.SerializeObject(test_result, Formatting.Indented);
            File.WriteAllText(getOutputResultsPath(test_name), json);
        }
        return test_result;
    }


    public void runTests(){
        List<TestResult> test_results = new List<TestResult>();
        DebugWindow.LogMsg("[runAllTests] start");
        test_results.Add(testGameWindow()); // ok, 1024x768
        test_results.Add(testGameStates()); // ok, not loading, is in game
        test_results.Add(testPlayerInfo()); // ok, witch lvl2
        test_results.Add(testArea()); // ok, a1_town, english client
        test_results.Add(testPlayerFlasks()); // ok, flask count
        test_results.Add(testPlayerSkills()); // ok, lvl2 witch, raise zombie gem on panel
        // test_results.Add(testWaypoints()); // 
        test_results.Add(testEntities()); // ok, a1_town, checking 3.25 nessa, stash, waypoint, player
        test_results.Add(testItemsOnGroundLabelsVisible()); // ok, leave a1_town, drop scroll of wisdom
        test_results.Add(testInventory()); // ok, in inventory: stack of 3 wisdom scrolls on [0,0] [x,y], default witch wand BB on [1,0] [x,y]
        test_results.Add(testStash()); // ok, in stash: stack of 3 wisdom scrolls on [0,0], default witch wand BB on [1,0]
        test_results.Add(testMapDevice()); // ok, open map device, kirak missions count
        test_results.Add(testKirakMissions()); // ok, open kirak missions, kirak missions variety
        test_results.Add(testPurchaseHideoutWindow()); // ok, open trade window with kirak to buy maps\etc
        // test_results.Add(testNpcDialogueUi()); //
        foreach (var test_result in test_results){
            DebugWindow.LogMsg($"{test_result.name}: {test_result.reportAsString()}");
        }
        // gotoCharSelectionWindow()
        // testCharSelectionWindow();
        // gotoMainMenuWindow();
        // testMainMenuWindow();

        string json = JsonConvert.SerializeObject(test_results, Formatting.Indented);
        File.WriteAllText(getOutputResultsPath("all_tests"), json);

        DebugWindow.LogMsg("[runAllTests] end");
    }
    public override bool Initialise(){
        // mini, gonna check what is possible to check without ANY ingame interactions
        Settings.run_all_tests_button.OnPressed = () => runTests();
        Settings.run_testGameWindow_button.OnPressed = () => testGameWindow(true);
        Settings.run_testGameStates_button.OnPressed = () => testGameStates(true);
        Settings.run_testPlayerInfo_button.OnPressed = () => testPlayerInfo(true);
        Settings.run_testArea_button.OnPressed = () => testArea(true);
        Settings.run_testPlayerFlasks_button.OnPressed = () => testPlayerFlasks(true);
        Settings.run_testPlayerSkills_button.OnPressed = () => testPlayerSkills(true);
        // Settings.run_testWaypoints_button.OnPressed = () => testWaypoints(true);
        Settings.run_testEntities_button.OnPressed = () => testEntities(true);
        Settings.run_testItemsOnGroundLabelsVisible_button.OnPressed = () => testItemsOnGroundLabelsVisible(true);
        Settings.run_testInventory_button.OnPressed = () => testInventory(true);
        Settings.run_testStash_button.OnPressed = () => testStash(true);
        Settings.run_testMapDevice_button.OnPressed = () => testMapDevice(true);
        Settings.run_testKirakMissions_button.OnPressed = () => testKirakMissions(true);
        Settings.run_testPurchaseHideoutWindow_button.OnPressed = () => testPurchaseHideoutWindow(true);
        // Settings.run_testNpcDialogueUi_button.OnPressed = () => testNpcDialogueUi(true);
        return true;
    }

        public override void Render(){
            // testGameWindow
            if (Settings.run_testGameWindow_key.PressedOnce()) testGameWindow(true);
            if (Settings.run_testGameStates_key.PressedOnce()) testGameStates(true);
            if (Settings.run_testPlayerInfo_key.PressedOnce()) testPlayerInfo(true);
            if (Settings.run_testArea_key.PressedOnce()) testArea(true);
            if (Settings.run_testPlayerFlasks_key.PressedOnce()) testPlayerFlasks(true);
            if (Settings.run_testPlayerSkills_key.PressedOnce()) testPlayerSkills(true);
            // if (Settings.run_testWaypoints_key.PressedOnce()) testWaypoints(true);
            if (Settings.run_testEntities_key.PressedOnce()) testEntities(true);
            if (Settings.run_testItemsOnGroundLabelsVisible_key.PressedOnce()) testItemsOnGroundLabelsVisible(true);
            if (Settings.run_testInventory_key.PressedOnce()) testInventory(true);
            if (Settings.run_testStash_key.PressedOnce()) testStash(true);
            if (Settings.run_testMapDevice_key.PressedOnce()) testMapDevice(true);
            if (Settings.run_testKirakMissions_key.PressedOnce()) testKirakMissions(true);
            if (Settings.run_testPurchaseHideoutWindow_key.PressedOnce()) testPurchaseHideoutWindow(true);
            // if (Settings.run_testNpcDialogueUi_key.PressedOnce()) testNpcDialogueUi(true);
        }

}
