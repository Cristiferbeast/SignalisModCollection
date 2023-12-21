using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;

public class NewBehaviourScript : MonoBehaviour
{
    private bool spaceBarPressed = false;
    public TextMeshProUGUI Text;

    void Start()
    {
        List<Character> CharList = new List<Character>
        {
            new Character { Name = "Falke", Gender = "Female" },
            new Character { Name = "Adler" , Gender = "Male" },
            new Character { Name = "Elster", Gender = "Female" },
            new Character { Name = "Ariane", Gender = "Female", Pronoun2 = "her"},
            new Character { Name = "Arar", Gender = "Female"},
            new Character { Name = "MNHR", Gender = "Female"},
            new Character { Name = "Nikolai", Gender = "Male"}
        };
        Text.color = Color.red;
        Text.lineSpacing = 2;
        List < Character > CharacterList = CharacterCreation();
        StartCoroutine(RunSimulation(CharacterList));
    }

    public static List<Character> CharacterCreation()
    {
        string filename = "StoredCharacterDetails.txt";
        string filePath = Path.Combine(Application.dataPath, filename);
        List<Character> CharList = new List<Character>();
        // Read the contents of the file
        if (File.Exists(filePath))
        {
            string[] fileContents = File.ReadAllLines(filePath);
            foreach (string line in fileContents)
            {
                Character character = new Character();
                string[] characterparts = line.Split(",");
                character.Name = characterparts[0];
                character.Gender = characterparts[1];
                CharList.Add(character);
            }
            return CharList;
        }
        else {
            File.WriteAllText(filePath, "Fale, Female");
            return new List<Character>
            {
                new Character { Name = "Falke", Gender = "Female" },
                new Character { Name = "Adler" , Gender = "Male" },
                new Character { Name = "Elster", Gender = "Female" },
                new Character { Name = "Ariane", Gender = "Female", Pronoun2 = "her"},
                new Character { Name = "Arar", Gender = "Female"},
                new Character { Name = "MNHR", Gender = "Female"},
                new Character { Name = "Nikolai", Gender = "Male"}
            };
        }
    }

    IEnumerator RunSimulation(List<Character> CharList)
    {
        System.Random random = new System.Random();
        for (int turn = 1; CharList.Count(c => c.IsAlive) > 1; turn ++ )
        {
            Text.text +=  $"<br> Turn {turn} <br>";
            CharList = CharList.OrderBy(c => random.Next()).ToList();
            foreach (Character character in CharList.Where(c => c.IsAlive))
            {
                int ran = random.Next(0, 101);
                var otherCharacters = CharList.Where(c => c.IsAlive && c != character).ToList();
                Character? duo = CharList.FirstOrDefault(c => c.IsAlive && c != character);
                Character? trio = CharList.FirstOrDefault(c => c.IsAlive && c != character && c != duo);
                Responder.Respond(Text, ran, character, duo, trio);
                Text.text += "<br>";
            }

            // Wait for spacebar input before the next turn
            yield return new WaitUntil(() => spaceBarPressed);
            spaceBarPressed = false; 
            Text.text = "";
        }

        var winner = CharList.SingleOrDefault(c => c.IsAlive);
        Text.text +=  winner != null ? $"The winner is {winner.Name}!" : "No winner.";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            spaceBarPressed = true;
        }
        // Additional update logic can go here if needed
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

public class Character
{
    public string Name { get; set; }
    public string Gender { get; set; } 
    public bool IsAlive { get; set; } = true;
    public string Pronoun { get; set; } = "them"; // Accusative
    public string Pronoun2 { get; set; } = "their"; //Ownative
    public string Pronoun3 { get; set; } = "they"; //Secondary
    public int HP { get; set; } = 100;
}
class Responder
{
    public static void kill(Character c)
    {
        c.IsAlive = false;
    }
    public static void Respond(TextMeshProUGUI text, int value, Character Char1, Character? Char2 = null, Character? Char3 = null)
    {
        //Duo Events
        if(value >= 60 && Char2 != null && value < 90)
        {
            switch (value)
            {
                case 65:
                    text.text +=  ($"{Char1.Name} mind was overwritten by a mysterious force and becomes {Char2.Name}");
                    Char1.Name = Char2.Name;
                    break;
                case 70:
                    kill(Char2);
                    text.text +=  ($"{Char1.Name} states you shouldn't have came back and pushes {Char2.Name} down an elevator to {Char2.Pronoun2} Death");
                    break;
                case 80:
                    kill(Char2);
                    text.text +=  ($"{Char1.Name} shoots a flare grenade at {Char2.Name}");
                    break;
                case 85:
                    text.text +=  ($"The Floor breaks under {Char1.Name}'s feet, but they are rescued by {Char2.Name}");
                    break;
                default:
                    text.text +=  ($"{Char1.Name} helps {Char2.Name} with a strange pump puzzle");
                    break;
            }
        }
        //Trio Events
        else if (value >= 90 && Char2 != null && Char3 != null)
        {
            if (value == 100)
            {
                kill(Char2);
                kill(Char3);
                text.text +=  ($"{Char1.Name}, {Char2.Name} and {Char3.Name} have found a solution to the problems possed by this facility, and have fused to become a greater being. The Chimera is Born");
                Char1.Name = "The Chimera";
            }
            else if (value >= 95)
            {
                kill(Char3);
                text.text +=  ($"{Char1.Name} and {Char2.Name} gang up on {Char3.Name} brutally killing {Char3.Pronoun}");
            }
            else
            {
                text.text +=  ($"{Char1.Name}, {Char2.Name} and {Char3.Name} all rest together");
            }
        }
        //Singular Events
        else
        {
            if(value >= 60)
            {
                value -= 60;
            }
            switch (value)
            {
                case 0:
                    text.text +=  ($"{Char1.Name} dreams of the beach");
                    break;
                case 1:
                    text.text +=  ($"{Char1.Name} observed a group of Arar's building a boat");
                    break;
                case 2:
                    text.text +=  ($"{Char1.Name} goes around a pile of Flesh");
                    break;
                case 3:
                    text.text +=  ($"{Char1.Name} sneaks by a Eule chopping meat loudly");
                    break;
                case 4:
                    if (Char1.HP > 50)
                    {
                        Char1.HP -= 50;
                        text.text +=  ($"{Char1.Name} was almost dragged into the Vents by an Arar, but managed to fight them off");
                    }
                    else
                    {
                        kill(Char1);
                        text.text +=  ($"{Char1.Name} was dragged into the Vents by an Arar, Prime Fertilizer for their Plants");
                    }
                    break;
                case 5:
                    text.text +=  ($"{Char1.Name} cannot resist the Infection any further");
                    kill(Char1);
                    break;
                case 6:
                    text.text +=  ($"{Char1.Name} lacks a Flashlight and succumbs to the Nanowire");
                    break;
                case 7:
                    if (Char1.HP > 50)
                    {
                        Char1.HP -= 50;
                        text.text +=  ($"{Char1.Name} hurt themselves on Nanowire");
                    }
                    else
                    {
                        kill(Char1);
                        text.text +=  ($"{Char1.Name} succumbed to injuries from Nanowire");
                    }
                    break;
                case 8:
                    text.text +=  ($"{Char1.Name} cannot find a repair spray and succumbs to bleeding from a cut");
                    kill(Char1);
                    break;
                case 9:
                    text.text +=  ($"{Char1.Name} steps on a human-shaped silhoutte of Ash");
                    break;
                case 10:
                    text.text +=  ($"{Char1.Name} jumps down a hole");
                    break;
                case 12:
                    text.text +=  ($"{Char1.Name} cannot remember the promise");
                    break;
                case 14:
                    text.text +=  ($"{Char1.Name} finds a strange cog laying on the floor and picks a strange nickname for it");
                    break;
                case 15:
                    text.text +=  ($"{Char1.Name} sees a Replika reanimate");
                    break;
                case 16:
                    text.text +=  ($"{Char1.Name} didnt chose to be here");
                    break;
                case 17:
                    text.text +=  ($"{Char1.Name} hides from a patrolling squad");
                    break;
                case 18:
                    text.text +=  ($"{Char1.Name} feels like they are forgetting something");
                    break;
                case 19:
                    text.text +=  ($"{Char1.Name} melts a STAR with a Flare");
                    break; 
                case 20:
                    text.text +=  ($"{Char1.Name} feels like they are remembering their name...");
                    break;
                case 21:
                    text.text +=  ($"{Char1.Name} tries to sleep but dreams of unknown places wake them up");
                    break;
                case 22:
                    text.text +=  ($"{Char1.Name} feels like someone is watching {Char1.Pronoun}");
                    break;
                case 23:
                    text.text +=  ($"{Char1.Name} despairs as it is impossible to move on");
                    break;
                case 24:
                    text.text +=  ($"{Char1.Name} trips and falls down an Elevator Shaft");
                    kill(Char1);
                    break;
                case 25:
                    text.text +=  ($"{Char1.Name} melts into Soup");
                    kill(Char1);
                    break;
                case 26:
                    text.text +=  ($"Curosity overwhelms {Char1.Name} {Char1.Pronoun3} stick {Char1.Pronoun2} arm in a meat processor");
                    Char1.HP -= 60;
                    break;
                case 27:
                    text.text +=  ($"{Char1.Name} learns why AEON bans you from eating Replikas, and Dies");
                    kill(Char1);
                    break;
                case 28:
                    text.text +=  ($"{Char1.Name} notices a few white hairs");
                    break;
                case 29:
                    text.text +=  ($"{Char1.Name} picked up the King in Yellow");
                    break;
                case 30:
                    text.text +=  ($"{Char1.Name} stabs at a Eule's Empty Eye Socket");
                    break;
                case 31:
                    text.text +=  ($"{Char1.Name} finds some ducttape");
                    break;
                case 32:
                    text.text +=  ($"{Char1.Name} scavenges around");
                    break;
                case 33:
                    kill(Char1);
                    text.text +=  ($"{Char1.Name} put a grenade in the wrong flare gun, it couldnt handle the load");
                    break;
                case 34:
                    text.text +=  ($"{Char1.Name} reads classified documents on Replikas");
                    break;
                case 35:
                    text.text +=  ($"{Char1.Name} dreamed of Electric Sheep");
                    break;
                case 36:
                    text.text +=  ($"{Char1.Name} lost their self control and loudly sang and danced");
                    break;
                case 37:
                    text.text +=  ($"{Char1.Name} tries to relax listening to Swan Lake on the Radio");
                    break;
                case 38:
                    text.text +=  ($"{Char1.Name} observes a strange object in the x-ray machine");
                    break;
                case 40:
                    text.text +=  ($"{Char1.Name} finds a repair spray on the ground and uses it");
                    Char1.HP += 25;
                    break;
                case 41:
                    text.text +=  ($"{Char1.Name} got lost in the dark because they lost their flashlight");
                    break;
                case 42:
                    kill(Char1);
                    text.text +=  ($"{Char1.Name} gets caught in the blast of {Char1.Pronoun2} own Flak Grenade");
                    break;
                case 43:
                    text.text +=  ($"{Char1.Name}'s map module breaks");
                    break;
                case 44:
                    kill(Char1);
                    text.text +=  ($"The Floor breaks under {Char1.Name}'s feet, killing them");
                    break;
                case 45:
                    text.text +=  ($"{Char1.Name} remembers their promise becoming LSTR-512");
                    Char1.Name = "LSTR-512";
                    break;
                case 46:
                    text.text +=  ($"{Char1.Name} reads a archived medical base of familar looking faces");
                    break;
                case 47:
                    text.text +=  ($"{Char1.Name} finds the gate, {Char1.Name} peers into the gate");
                    break;
                case 52:
                    text.text +=  ($"{Char1.Name} swears they saw a White Haired Girl appear and suddenly disappear");
                    break;
                case 53:
                    text.text +=  ($"{Char1.Name} hears footsteps nearby and hides");
                    break;
                case 54:
                    text.text +=  ($"{Char1.Name} has to detour around the area due to a locked door");
                    break;
                case 55:
                    text.text +=  ($"{Char1.Name} waits impatiently for an elevator that never moves");
                    break;
                case 56:
                    if(Char1.HP > 50)
                    {
                        Char1.HP -= 50;
                        text.text +=  ($"{Char1.Name} has a close encounter with a corrupted Kolibri");
                    }
                    else
                    {
                        kill(Char1);
                        text.text +=  ($"{Char1.Name} is consumed by a Kolibri's Song");
                    }
                    break;
                case 57:
                    text.text +=  ($"{Char1.Name} talks to a Friendly Eule, they both hope to get out of this place");
                    Char1.Name += " And Eule";
                    Char1.HP += 100;
                    break;
                case 59:
                    text.text +=  ($"{Char1.Name} stumbles into Adlers \"Collection\" of Falke Posters, {Char1.Pronoun3} live to tell noone ");
                    kill(Char1);
                    break;
                default:
                    text.text +=  ($"{Char1.Name} is too afraid, and just huddles down");
                    break;
            }
        }
    }
    
}

