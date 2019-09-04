using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Visualizer : MonoBehaviour
{
    private const float rampUpSpeed = 13.5f;
    private const float rampDownSpeed = 5f;

    //Main input driver for voice
    [Range(0.0f, 1.0f)]
    public float voiceInput = 0.0f;

    private float perlinScale = 0.0f;
    private float elapsedTime = 0;
    private List<int> randomNumbers;
    private List<int> randomGColor;
    private List<int> randomBColor;
    private List<int> randomAlpha;

    //Mic-in related properties.
    [Header("Audio controls.")]
    [Tooltip("Enable to drive animation from HMD mic input.")]
    public bool listenForViveMic = true;
    [Tooltip("Component in scene that listens to HMD mic input.")]
    public keyboardAudioInput micIn;    
    public GameObject visualBarGroup;
    float upperBound, lowerBound, presentScale, localTime, amp;
    bool init;


    void Start()
    {
        randomNumbers = new List<int>();
        randomGColor = new List<int>();
        randomBColor = new List<int>();
        randomAlpha = new List<int>();

        foreach (Transform child in visualBarGroup.transform)
        {
            randomNumbers.Add(RandomizeNumber());
            randomGColor.Add(RandomizeGColor());
            randomBColor.Add(RandomizeBColor());
            randomAlpha.Add(RandomizeAlpha());
        }
    }

    void Update()
    {
        if(micIn != null && listenForViveMic && Microphone.IsRecording(micIn.microphone))
        {
            amp = micIn.GetSampleAmplitude();

            if(!visualBarGroup.activeSelf)
                return;
            //Debug.Log("amp: " + amp);

            voiceInput = Mathf.Lerp(0f, 1f, localTime);
            if (amp > 0.3f) { if(localTime <= 1f) localTime += Time.deltaTime * rampUpSpeed; }
            else if (amp < 0.3f) { if(localTime >= 0f) localTime -= Time.deltaTime * rampDownSpeed; }
        
            int i = 0;
            Image[] children = visualBarGroup.GetComponentsInChildren<Image>();
            Color32 newColor;
            elapsedTime = Mathf.PingPong(Time.time * 1.3f, 5);

            //Adding to voiceInput so visualizer doesn't disappear entirely when the input is 0
            voiceInput = Mathf.Clamp(voiceInput + 0.22f, 0.22f, 1f);

            foreach (Transform child in visualBarGroup.transform)
            {
                perlinScale = Mathf.PerlinNoise(voiceInput + elapsedTime * randomNumbers[i] * 0.1f, 10.1f);
                child.localScale = new Vector3(voiceInput + perlinScale * 2.25f, voiceInput * perlinScale * 1.5f, 1.0f);
                i++;
            }

            int a = 0;
            int g = 0;
            int b = 0;

            foreach(Image child in children)
            {
                newColor = child.color;
                newColor.g = (byte)(Mathf.Clamp(randomGColor[g] + (elapsedTime * 10), 0f, 255f));
                newColor.b = (byte)(Mathf.Clamp(randomBColor[b] + (elapsedTime * 10), 0f, 255f));
                newColor.a = (byte)(Mathf.Clamp(randomAlpha[a] + (elapsedTime * 10), 0f, 255f));
                child.color = newColor;
                a++;
                g++;
                b++;
            }
        }
    }

    int RandomizeNumber()
    {
        return (int)Random.Range(1.1f, 19.9f);
    }

    int RandomizeGColor()
    {
        return (int)Random.Range(120f, 205f);
    }

    int RandomizeBColor()
    {
        return (int)Random.Range(170f, 205f);
    }

    int RandomizeAlpha()
    {
        return (int)Random.Range(140f, 205f);
    }
}
