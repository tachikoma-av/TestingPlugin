using ExileCore.Shared.Nodes;
using ExileCore.Shared.Interfaces;
using System.Windows.Forms;

namespace TestingPlugin
{
    public class TestingPluginSettings : ISettings
    {
        public TestingPluginSettings()
        {
            Enable = new ToggleNode(true);
        }
        public ToggleNode Enable { get; set; }
        public ButtonNode run_all_tests_button { get; set; } = new ButtonNode();
        public HotkeyNode run_all_tests_key { get; set; } = new HotkeyNode(Keys.NumPad3);

        // testGameWindow
        public ButtonNode run_testGameWindow_button { get; set; } = new ButtonNode();
        public HotkeyNode run_testGameWindow_key { get; set; } = new HotkeyNode();

        // testGameStates
        public ButtonNode run_testGameStates_button { get; set; } = new ButtonNode();
        public HotkeyNode run_testGameStates_key { get; set; } = new HotkeyNode();

        // testPlayerInfo
        public ButtonNode run_testPlayerInfo_button { get; set; } = new ButtonNode();
        public HotkeyNode run_testPlayerInfo_key { get; set; } = new HotkeyNode();

        // testArea
        public ButtonNode run_testArea_button { get; set; } = new ButtonNode();
        public HotkeyNode run_testArea_key { get; set; } = new HotkeyNode();

        // testPlayerFlasks
        public ButtonNode run_testPlayerFlasks_button { get; set; } = new ButtonNode();
        public HotkeyNode run_testPlayerFlasks_key { get; set; } = new HotkeyNode();

        // testPlayerSkills
        public ButtonNode run_testPlayerSkills_button { get; set; } = new ButtonNode();
        public HotkeyNode run_testPlayerSkills_key { get; set; } = new HotkeyNode();

        // testWaypoints
        public ButtonNode run_testWaypoints_button { get; set; } = new ButtonNode();
        public HotkeyNode run_testWaypoints_key { get; set; } = new HotkeyNode();

        // testEntities
        public ButtonNode run_testEntities_button { get; set; } = new ButtonNode();
        public HotkeyNode run_testEntities_key { get; set; } = new HotkeyNode();

        // testVisibleLabelEntities
        public ButtonNode run_testItemsOnGroundLabelsVisible_button { get; set; } = new ButtonNode();
        public HotkeyNode run_testItemsOnGroundLabelsVisible_key { get; set; } = new HotkeyNode();

        // testInventory
        public ButtonNode run_testInventory_button { get; set; } = new ButtonNode();
        public HotkeyNode run_testInventory_key { get; set; } = new HotkeyNode();

        // testStash
        public ButtonNode run_testStash_button { get; set; } = new ButtonNode();
        public HotkeyNode run_testStash_key { get; set; } = new HotkeyNode();

        // testMapDevice
        public ButtonNode run_testMapDevice_button { get; set; } = new ButtonNode();
        public HotkeyNode run_testMapDevice_key { get; set; } = new HotkeyNode();

        // testKirakMissions
        public ButtonNode run_testKirakMissions_button { get; set; } = new ButtonNode();
        public HotkeyNode run_testKirakMissions_key { get; set; } = new HotkeyNode();

        // testPurchaseHideoutWindow
        public ButtonNode run_testPurchaseHideoutWindow_button { get; set; } = new ButtonNode();
        public HotkeyNode run_testPurchaseHideoutWindow_key { get; set; } = new HotkeyNode();

        // testNpcDialogueUi
        public ButtonNode run_testNpcDialogueUi_button { get; set; } = new ButtonNode();
        public HotkeyNode run_testNpcDialogueUi_key { get; set; } = new HotkeyNode();


    }
}
