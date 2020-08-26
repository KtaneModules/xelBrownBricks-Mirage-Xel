using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;
using System;
using System.Linq;
public class BrownBricks : MonoBehaviour {
    public KMSelectable[] bricks;
    public MeshRenderer[] brickRenderes;
    public Material[] brickMats;
    int[] posLookup = new int[] { 0, 3, 2, 1, 2 };
    int[] brickLookup = new int[] { 2, 3, 4, 0, 1};
    int[][][] answerLookup = new int[][][] {
    new int[][] {
    new int[] {0,4},
    new int[] {2,1,3,2}
    },
    new int[][] {
    new int[] {4,3},
    new int[] {0,2,1,3}
    },
    new int[][] {
    new int[] {2,0},
    new int[] {3,4,3,1}
    },
    new int[][] {
    new int[] {1,2},
    new int[] {3,4,1,0}
    },
        new int[][] {
    new int[] {3,1},
    new int[] {1,4,0,2}
    },
    };
    string[] loggingNames = new string[] {"Hit", "Question", "Wall", "Ground", "Brick"};
    int[] brickPos = new int[5];
    int[][] correctAnswerLookup;
    public KMBombModule module;
    public KMAudio sound;
    int missingBrick;
    int lookBrick;
    int answerBrick;
    int moduleId;
    static int moduleIdCounter;
    bool solved;
    void Awake()
    {
        moduleId = moduleIdCounter++;
     foreach (KMSelectable brick in bricks)
        {
            KMSelectable pressedBrick = brick;
            brick.OnInteract += delegate { pressBrick(pressedBrick); return false; };
        } 
    }
    void Start () {
        generateBricks();
        for (int i = 0; i < 4; i++)
        {
            brickRenderes[i].material = brickMats[brickPos[i]];
        }
        missingBrick = brickPos[4];
        Debug.LogFormat("[Brown Bricks #{0}] The blocks in order are {1}, {2}, {3}, and {4}." , moduleId, loggingNames[brickPos[0]], loggingNames[brickPos[1]], loggingNames[brickPos[2]], loggingNames[brickPos[3]]);
        Debug.LogFormat("[Brown Bricks #{0}] The missing block is {1}.", moduleId, loggingNames[missingBrick]);
        lookBrick = brickLookup[brickPos[posLookup[missingBrick]]];
        correctAnswerLookup = answerLookup[brickPos[lookBrick]];
        handleAnswerLookup(correctAnswerLookup);
        Debug.LogFormat("[Brown Bricks #{0}] The >LOOK> block is {1}.", moduleId, loggingNames[lookBrick]);
        Debug.LogFormat("[Brown Bricks #{0}] The correct block to press is {1}.", moduleId, loggingNames[answerBrick]);
    }
	void generateBricks()
    {
        for (int i = 0; i < 5; i++)
        {
            brickPos[i] = -1;
        }
        for (int i = 0; i < 5; i++)
        {
            int index = UnityEngine.Random.Range(0, 5);
            while (brickPos[index] != -1)
            {
                index = UnityEngine.Random.Range(0, 5);
            }
            brickPos[index] = i;
        }
    }
    void handleAnswerLookup(int[][] lookup)
    {
        if (missingBrick == lookup[0][0])
        {
            answerBrick = lookup[1][0];
        }
        else if (lookBrick == lookup[0][1])
        {
            answerBrick = lookup[1][1];
        }
        else
        {
            answerBrick = lookup[1][2];
        }
       if (missingBrick == answerBrick)
        {
            answerBrick = lookup[1][3];
        }
    }

    void pressBrick(KMSelectable brick)
    {
        if(!solved)
        {
            brick.AddInteractionPunch(.5f);
            if (Array.IndexOf(bricks, brick) == Array.IndexOf(brickPos, answerBrick))
            {
                module.HandlePass();
                solved = true;
                sound.PlaySoundAtTransform("inspector gadget cartoon intro theme", transform);
                Debug.LogFormat("[Brown Bricks #{0}] That was correct. Module solved.", moduleId);
            }
            else
            {
                module.HandleStrike();
                Debug.LogFormat("[Brown Bricks #{0}] That was incorrect. Strike!", moduleId);
                for (int i = 0; i < 5; i++)
                {
                    brickPos[i] = -1;
                }
                    Start();
            }
        }
	}
#pragma warning disable 414
    private string TwitchHelpMessage = "'!{0} press 'tl/tr/bl/br' to press a block. e.g. '!{0} press tr'";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        string[] validCommands = new string[4] { "tl", "tr", "bl", "br" };
        string[] commandArray = command.Split(' ');
        if (commandArray.Length != 2 || commandArray[0] != "press" || !validCommands.Contains(commandArray[1]))
        {
            yield return "sendtochaterror @{0}, invalid command.";
            yield break;
        }
        yield return null;
        bricks[Array.IndexOf(validCommands, commandArray[1])].OnInteract();
    }
    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        bricks[Array.IndexOf(brickPos, answerBrick)].OnInteract();
    }
}
