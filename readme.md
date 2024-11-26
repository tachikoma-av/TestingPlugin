##
test data is taken from `$"./Plugins/Source/TestingPlugin/test_samples/{test_name}.json"`

test outputs are saved to `$"./Plugins/Source/TestingPlugin/test_results/{test_name}.json"`

run_all_tests from button or hotkey, results will be saved to ./Plugins/Source/TestingPlugin/test_results/all_tests.json as an array of results

run `test_name` tests from button or hotkey, results will be printed and saved to $"./Plugins/Source/TestingPlugin/test_results/`test_name`.json"


## default test conditions
test_results.Add(testGameWindow()); // ok, 1024x768

test_results.Add(testGameStates()); // ok, not loading, is in game

test_results.Add(testPlayerInfo()); // ok, witch lvl2

test_results.Add(testArea()); // ok, a1_town, english client

test_results.Add(testPlayerFlasks()); // ok, flask count

test_results.Add(testPlayerSkills()); // ok, lvl2 witch, raise zombie gem on panel

// test_results.Add(testWaypoints()); // 

test_results.Add(testEntities()); // ok, a1_town, checking 3.25 nessa, stash, 
waypoint, player

test_results.Add(testItemsOnGroundLabelsVisible()); // ok, leave a1_town, drop scroll of wisdom

test_results.Add(testInventory()); // ok, in inventory: stack of 3 wisdom scrolls on [0,0] [x,y], default witch wand BB on [1,0] [x,y]

test_results.Add(testStash()); // ok, in stash: stack of 3 wisdom scrolls on [0,0], default witch wand BB on [1,0]

test_results.Add(testMapDevice()); // ok, open map device, kirak missions count

test_results.Add(testKirakMissions()); // ok, open kirak missions, kirak missions variety

test_results.Add(testPurchaseHideoutWindow()); // ok, open trade window with kirak to buy maps\etc

## TODO
- prettier, it's ugly
- default hotkey null
- player, skill tree information
- items, mod list
- map_device, items placed in map device count
