using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using rnd = UnityEngine.Random;
using KModkit;
public class TemplateScript : MonoBehaviour {
    public KMSelectable[] normalButtons;
    public KMSelectable recButton;
    public KMSelectable playButton;
    public TextMesh text;
    string[] wordlist = new string[] { "ANGEL", "AZURE", "BEACH", "CANDY", "DRAKE", "ENNUI", "EQUAL", "FOLIO", "GHOST", "HELIX", "INERT", "JOKER", "LIMBO", "MANIA", "NIMOY", "NOVEL", "OPERA", "PHONE", "QUARK", "RADIO", "SPACE" ,"STACK", "TOUCH", "THING", "UNITE", "VELDT", "WALTZ", "XENON", "YOUNG", "ZONER"};
    public string questionWord = "266433555";
    public bool recording;
    Coroutine dialtone;
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
        playButton.OnInteract += delegate {PressPlayButton(); return false; };
	}
	
	// Update is called once per frame
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
    IEnumerator PlayDialtone(string elephant)
    {
        foreach (char i in elephant)
        {
            sound.PlaySoundAtTransform(i.ToString(), transform);
            yield return new WaitForSeconds(0.104f);
        }
    }

}
