using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using rnd = UnityEngine.Random;
using KModkit;
public class Dialtones : MonoBehaviour {
    public KMSelectable[] normalButtons;
    public KMSelectable recButton;
    public KMSelectable playButton;
    public TextMesh text;
    string[] wordlist = new string[30] { "ANGEL", "AZURE", "BEACH", "CANDY", "DRAKE", "ENNUI", "EQUAL", "FOLIO", "GHOST", "HELIX", "INERT", "JOKER", "LIMBO", "MANIA", "NIMOY", "NOTED", "OPERA", "PHONE", "QUARK", "RADIO", "SPACE" ,"STACK", "THING", "TOUCH", "UNITE", "VELDT", "WALTZ", "XENON", "YOUNG", "ZONER"};
    string questionWord;
    string answerWord;
    bool recording;
    Coroutine dialtone;
    public KMBombInfo bomb;
    public KMBombModule module;
    public KMAudio sound;
    int moduleId;
    static int moduleIdCounter = 1;
    bool solved;
    void Awake() {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable i in normalButtons)
        {
            KMSelectable button = i;
            button.OnInteract += delegate { PressNormalButton(button); return false; };
        }
        recButton.OnInteract += delegate {PressRecButton(); return false; };
        playButton.OnInteract += delegate {PressPlayButton(); return false; };  
	}
    void Start()
    {
        GeneratePuzzle();
    }
    void GeneratePuzzle()
    {
        int wordIndex = rnd.Range(0, 30);
        questionWord = ConvertToDialtones(wordlist[wordIndex]);
        answerWord = ConvertToDialtones(wordlist[(wordIndex + bomb.GetSerialNumberNumbers().Last()) % 30]);
        Debug.LogFormat("[Dialtones #{0}] The word on the module is {1}, which is {2} when converted to a sequence of dialtones.", moduleId, wordlist[wordIndex], questionWord);
        Debug.LogFormat("[Dialtones #{0}] The response word is {1}, which is {2} when converted to a sequence of dialtones.", moduleId, wordlist[(wordIndex + bomb.GetSerialNumberNumbers().Last()) % 30], answerWord);
    }
        string ConvertToDialtones(string plaintext)
    {
        List<String> phonepad = new string[] { "ABC", "DEF", "GHI", "JKL", "MNO", "PQR", "STU", "VWX", "YZ" }.ToList();
        string ciphertext = "";
        string currentDialtoneLetter;
        int outerIndex;
        int innerIndex;
        foreach (char i in plaintext)
        {
            currentDialtoneLetter = "";
            outerIndex = (phonepad.TakeWhile(x => !x.Contains(i)).Count() + 2 ) % 10;
            innerIndex = phonepad.First(x => x.Contains(i)).TakeWhile(x => x != i).Count() + 1;
            for (int j = 0; j < innerIndex; j++) currentDialtoneLetter += outerIndex.ToString();
            if (ciphertext.Length != 0) if (ciphertext.ToList().Last() == currentDialtoneLetter[0]) ciphertext += 1;
            ciphertext += currentDialtoneLetter;
        }
        return ciphertext;
    }
    void PressNormalButton (KMSelectable button) {
		if (!solved)
        {
            button.AddInteractionPunch();
            StartCoroutine(ButtonAnimation(button.transform));
            sound.PlaySoundAtTransform(button.GetComponentInChildren<TextMesh>().text, transform);
            if (dialtone != null) StopCoroutine(dialtone);
            if (recording) text.text += button.GetComponentInChildren<TextMesh>().text;
        }
	}
    void PressPlayButton()
    {
        if (!solved)
        {
            playButton.AddInteractionPunch();
            StartCoroutine(ButtonAnimation(playButton.transform));
            if (dialtone != null) StopCoroutine(dialtone);
            dialtone = StartCoroutine(PlayDialtone(questionWord));
        }
    }
    void PressRecButton()
    {
        if (!solved)
        {
            recButton.AddInteractionPunch();
            StartCoroutine(ButtonAnimation(recButton.transform));
            sound.PlaySoundAtTransform("Rec", transform);
            if (recording)
            {
                Debug.LogFormat("[Dialtones #{0}] You submitted {1}.", moduleId, text.text);
                if (text.text.Equals(answerWord))
                {
                    module.HandlePass();
                    text.text = "SOLVED";
                    sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                    Debug.LogFormat("[Dialtones #{0}] That was correct. Module solved.", moduleId);
                    solved = true;
                }
                else
                {
                    module.HandleStrike();
                    text.text = "";
                    recording = false;
                    Debug.LogFormat("[Dialtones #{0}] That was incorrect. Strike!", moduleId);
                    GeneratePuzzle();
                    return;
                }
            }
            if (!recording) recording = true;
        }
    }
    IEnumerator ButtonAnimation(Transform transform)
    {
        {
            for (int i = 0; i < 3; i++)
            {
                transform.localPosition += new Vector3(0f, -0.001f, 0f);
                yield return new WaitForSeconds(0.02f);
            }
            for (int i = 0; i < 3; i++)
            {
                transform.localPosition += new Vector3(0f, 0.001f, 0f);
                yield return new WaitForSeconds(0.02f);
            }
        }
    }
    IEnumerator PlayDialtone(string dialtone)
    {
        foreach (char i in dialtone)
        {
            sound.PlaySoundAtTransform(i.ToString(), transform);
            yield return new WaitForSeconds(0.11f);
        }
    }
    string TwitchHelpMessage = "!{0} play: Presses the play button | !{0} <numbers>: Presses the specified numpad buttons | !{0} record <numbers>: Submits the specified number sequence";
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        string[] commandArray = command.Trim().Split();
        int uselessNumber;
        if (commandArray.Length == 1)
        {
            if (command == "play")
            {
                yield return null;
                playButton.OnInteract();
            }
            else if (command.All(x => int.TryParse(x.ToString(), out uselessNumber)))
            {
                foreach (char i in command)
                {
                    yield return null;
                    yield return new WaitForSeconds(0.11f);
                    normalButtons[int.Parse(i.ToString())].OnInteract();
                }
            }
            else yield return "sendtochaterror Invalid command.";
        }
        else if (commandArray.Length == 2)
        {
            if (commandArray[0] == "record" && commandArray[1].All(x => int.TryParse(x.ToString(), out uselessNumber)))
            {
                yield return null;
                recButton.OnInteract();
                foreach (char i in commandArray[1])
                {
                    yield return new WaitForSeconds(0.11f);
                    normalButtons[int.Parse(i.ToString())].OnInteract();
                }
                yield return new WaitForSeconds(0.11f);
                recButton.OnInteract();
            }
        }
        else yield return "sendtochaterror Invalid command.";
    }
}
