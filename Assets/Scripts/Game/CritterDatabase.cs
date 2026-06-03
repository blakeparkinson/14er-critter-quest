using UnityEngine;

public static class CritterDatabase
{
    public static readonly CritterTemplate[] Templates = {
        new CritterTemplate {
            name = "Gerald",
            sillyTitle = "The Judgmental Marmot",
            entry = "Gerald has lived on Mt. Elbert for 47 years and has opinions about your hiking boots. He whistles not as a warning, but as commentary.",
            rarity = CritterRarity.Common,
            personality = CritterPersonality.Bold,
            sillyActions = new[] { "Judges your outfit", "Whistles sarcastically", "Steals a granola bar", "Poses dramatically on a rock" }
        },
        new CritterTemplate {
            name = "Beatrice",
            sillyTitle = "The Anxious Pika",
            entry = "Beatrice is collecting hay for winter. It's July. She's not taking any chances. Her haypile is three times her size and she's still not satisfied.",
            rarity = CritterRarity.Common,
            personality = CritterPersonality.Shy,
            sillyActions = new[] { "Adds to enormous hay pile", "Squeaks at nothing", "Rearranges hay nervously", "Hides behind a pebble" }
        },
        new CritterTemplate {
            name = "Reginald",
            sillyTitle = "The Distinguished Mountain Goat",
            entry = "Reginald stands on impossibly narrow ledges and looks at you like you're the weird one. He has a beard. It is magnificent.",
            rarity = CritterRarity.Uncommon,
            personality = CritterPersonality.Bold,
            sillyActions = new[] { "Balances on one hoof", "Strokes own beard", "Stares into your soul", "Climbs vertical cliff casually" }
        },
        new CritterTemplate {
            name = "Chef Boyar-Dee",
            sillyTitle = "The Cooking Ptarmigan",
            entry = "This ptarmigan was spotted arranging berries in what can only be described as a 'tasting menu.' It changes plumage seasonally. It calls this 'rebranding.'",
            rarity = CritterRarity.Uncommon,
            personality = CritterPersonality.Curious,
            sillyActions = new[] { "Arranges berries artistically", "Tastes the wind", "Presents a leaf as appetizer", "Critiques your trail mix" }
        },
        new CritterTemplate {
            name = "Big Tony",
            sillyTitle = "The Entrepreneurial Bighorn Sheep",
            entry = "Big Tony runs a protection racket on the northwest face of Mt. Princeton. Pay the toll (one energy bar) or face the horns. His headbutts are tax-deductible.",
            rarity = CritterRarity.Rare,
            personality = CritterPersonality.Bold,
            sillyActions = new[] { "Blocks the trail menacingly", "Counts imaginary money", "Flexes horns", "Offers 'trail insurance'" }
        },
        new CritterTemplate {
            name = "Karen",
            sillyTitle = "The Complaining Elk",
            entry = "Karen would like to speak to the manager of this mountain. The trails are too steep. The altitude is unacceptable. She's leaving a one-star review.",
            rarity = CritterRarity.Rare,
            personality = CritterPersonality.Chaotic,
            sillyActions = new[] { "Writes complaint on tree bark", "Demands to see manager", "Blocks trail while monologuing", "Rates the mountain 1 star" }
        },
        new CritterTemplate {
            name = "Professor Whiskers",
            sillyTitle = "The Academic Yellow-Bellied Marmot",
            entry = "Professor Whiskers has tenure. Nobody knows at which university. He lectures passing hikers on alpine geology whether they want it or not.",
            rarity = CritterRarity.Uncommon,
            personality = CritterPersonality.Curious,
            sillyActions = new[] { "Lectures about rocks", "Adjusts invisible glasses", "Grades your climbing technique", "Publishes paper on lichen" }
        },
        new CritterTemplate {
            name = "Steve",
            sillyTitle = "The Lost Llama",
            entry = "Steve is not from here. Steve is not supposed to be here. Steve does not care. He wandered up from a farm in Leadville and has found his truth at 14,000 feet.",
            rarity = CritterRarity.Legendary,
            personality = CritterPersonality.Chaotic,
            sillyActions = new[] { "Wears a tiny sweater", "Spits at the void", "Vibes existentially", "Hums to self", "Photobombs other critters" }
        },
        new CritterTemplate {
            name = "Duchess Von Floof",
            sillyTitle = "The Regal White Ptarmigan",
            entry = "Duchess Von Floof only appears during fresh snowfall. She considers herself above all other birds. Literally — she only perches on the highest available rock.",
            rarity = CritterRarity.Legendary,
            personality = CritterPersonality.Shy,
            sillyActions = new[] { "Poses regally in snow", "Dismisses lesser birds", "Inspects snowflakes individually", "Refuses to acknowledge you" }
        },
        new CritterTemplate {
            name = "Gremlin",
            sillyTitle = "The Unhinged Chickaree",
            entry = "This squirrel has been at altitude too long. It screams at hikers, throws pine cones with alarming accuracy, and has been banned from three campsites.",
            rarity = CritterRarity.Common,
            personality = CritterPersonality.Chaotic,
            sillyActions = new[] { "Throws pinecone at you", "Screams into the void", "Steals your hat", "Vibrates with rage", "Does a backflip for no reason" }
        },
        new CritterTemplate {
            name = "Brenda & Carl",
            sillyTitle = "The Bickering Clark's Nutcrackers",
            entry = "They've been together for 12 years. They fight constantly. They cannot be separated. Brenda thinks Carl buries seeds in stupid places. Carl thinks Brenda is a backseat flyer.",
            rarity = CritterRarity.Rare,
            personality = CritterPersonality.Chaotic,
            sillyActions = new[] { "Argue about directions", "Passive-aggressively share a seed", "Give each other the silent treatment", "Bicker about nest decor" }
        },
        new CritterTemplate {
            name = "The Council",
            sillyTitle = "Three Pikas in a Trenchcoat",
            entry = "No one has confirmed this is actually three pikas. But the trenchcoat is suspicious and they keep squeaking in harmony. They claim to be a single very tall pika named 'Richard.'",
            rarity = CritterRarity.Legendary,
            personality = CritterPersonality.Curious,
            sillyActions = new[] { "Wobbles convincingly", "Introduces self as 'one normal pika'", "Top pika peeks out", "Attempts to buy coffee" }
        }
    };
}

[System.Serializable]
public class CritterTemplate
{
    public string name;
    public string sillyTitle;
    public string entry;
    public CritterRarity rarity;
    public CritterPersonality personality;
    public string[] sillyActions;
}
