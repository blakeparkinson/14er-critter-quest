using UnityEngine;

public static class PeakDatabase
{
    public static readonly PeakTemplate[] Peaks = {
        new PeakTemplate {
            name = "Mt. Elbert",
            elevation = 14433,
            range = "Sawatch Range",
            flavor = "The tallest in Colorado and it won't shut up about it. Gerald lives here.",
            difficulty = 1
        },
        new PeakTemplate {
            name = "Mt. Massive",
            elevation = 14421,
            range = "Sawatch Range",
            flavor = "Second tallest and has a massive chip on its shoulder about it. Literally massive.",
            difficulty = 1
        },
        new PeakTemplate {
            name = "Mt. Harvard",
            elevation = 14420,
            range = "Collegiate Peaks",
            flavor = "Named after the university. Professor Whiskers insists he has a corner office here.",
            difficulty = 2
        },
        new PeakTemplate {
            name = "Longs Peak",
            elevation = 14255,
            range = "Front Range",
            flavor = "The Keyhole Route is not for the faint of heart. Or the faint of goat.",
            difficulty = 3
        },
        new PeakTemplate {
            name = "Pikes Peak",
            elevation = 14115,
            range = "Front Range",
            flavor = "You could drive to the top. But where's the critter content in that?",
            difficulty = 1
        },
        new PeakTemplate {
            name = "Mt. Princeton",
            elevation = 14197,
            range = "Collegiate Peaks",
            flavor = "Big Tony's territory. Bring tribute or bring regret.",
            difficulty = 2
        },
        new PeakTemplate {
            name = "Maroon Bells",
            elevation = 14156,
            range = "Elk Mountains",
            flavor = "The most photographed peaks in Colorado. The critters are tired of paparazzi.",
            difficulty = 3
        },
        new PeakTemplate {
            name = "Capitol Peak",
            elevation = 14130,
            range = "Elk Mountains",
            flavor = "The Knife Edge will test your resolve and your insurance coverage.",
            difficulty = 4
        }
    };
}

[System.Serializable]
public class PeakTemplate
{
    public string name;
    public int elevation;
    public string range;
    public string flavor;
    public int difficulty;
}
