using System.Collections.ObjectModel;

namespace CCC39UI;

class ScenarioNode
{
    public ObservableCollection<ScenarioNode> Children { get; set; } = new();

    public LawnSet Scenario { get; set; }

    public string Name { get; set; }

}
