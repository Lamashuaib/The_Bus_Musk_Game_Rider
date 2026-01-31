using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class TheBusMuskGameManager : MonoBehaviour
{
    [Header("UI & Transition Settings")]
    public Image fadeImage;          // شاشة التلاشي (البداية والنهاية)
    public GameObject maskOverlay;   // شاشة القناع السوداء (تظهر وتختفي)
    public GameObject startUI;       // كائن يحتوي على نصوص البداية
    public TextMeshProUGUI statusText; 

    [Header("Monsters (Ghouls) Setup")]
    // اسحبي الغيلان من الـ Hierarchy بالترتيب
    public List<GameObject> ghouls; 
    public float[] approachDurations = { 4f, 6f, 8f, 3f }; // مدة بقاء كل غول
    public KeyCode[] correctKeys = { KeyCode.D, KeyCode.A, KeyCode.S, KeyCode.D }; // الأزرار: أصفر، أحمر، أزرق، أصفر

    [Header("Audio Settings")]
    public AudioSource sfxSource;
    public AudioClip screamSound;

    private int currentIndex = 0;
    private bool gameStarted = false;
    private bool canPressSpace = false;
    private bool isDefeated = false;

    void Start()
    {
        // إعداد الحالة الأولية بناءً على صورك
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = Color.black;
        maskOverlay.SetActive(false);
        statusText.gameObject.SetActive(false);
        
        // إخفاء جميع الغيلان في البداية
        foreach (GameObject ghoul in ghouls) ghoul.SetActive(false);

        StartCoroutine(InitialFadeSequence());
    }

    IEnumerator InitialFadeSequence()
    {
        float timer = 0;
        while (timer < 3f)
        {
            timer += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, 1 - (timer / 3f));
            yield return null;
        }
        fadeImage.gameObject.SetActive(false); 
        canPressSpace = true; // اللاعب الآن يرى "Press Space to Start"
    }

    void Update()
    {
        // 1. بدء اللعبة بـ Space
        if (canPressSpace && !gameStarted && Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }

        // 2. زر W لنزع القناع وإبطال السواد
        if (gameStarted && Input.GetKeyDown(KeyCode.W))
        {
            maskOverlay.SetActive(false);
        }
    }

    void StartGame()
    {
        gameStarted = true;
        startUI.SetActive(false); // النصوص تختفي
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        while (currentIndex < ghouls.Count)
        {
            GameObject currentGhoul = ghouls[currentIndex];
            currentGhoul.SetActive(true);
            currentGhoul.transform.localScale = Vector3.zero;

            float duration = approachDurations[currentIndex];
            KeyCode targetKey = correctKeys[currentIndex];
            float timer = 0;
            isDefeated = false;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                // تكبير الغول تدريجياً
                currentGhoul.transform.localScale = Vector3.one * (timer / duration) * 3.5f;

                // إذا ضغط اللاعب الزر الصحيح (A, S, D)
                if (Input.GetKeyDown(targetKey))
                {
                    maskOverlay.SetActive(true); // تفعيل السواد (لبس القناع)
                    isDefeated = true;
                    break;
                }
                yield return null;
            }

            if (isDefeated)
            {
                currentGhoul.SetActive(false);
                currentIndex++;
                yield return new WaitForSeconds(1.5f); // وقت مستقطع قبل الغول التالي
            }
            else
            {
                // لم يلبس القناع في الوقت المناسب
                TriggerJumpScare(currentGhoul);
                yield break;
            }
        }

        // النجاة النهائية
        statusText.gameObject.SetActive(true);
        statusText.text = "YOU SURVIVED";
        statusText.color = Color.green;
    }

    void TriggerJumpScare(GameObject ghoul)
    {
        ghoul.transform.localScale = Vector3.one * 25f; // تكبير انفجاري
        sfxSource.PlayOneShot(screamSound);
        
        //fadeImage.gameObject.SetActive(true);
       // fadeImage.color = Color.black;
        statusText.gameObject.SetActive(true);
        statusText.text = "YOU DID NOT SURVIVE";
        statusText.color = Color.red;

        Invoke("Restart", 3f);
    }

    void Restart() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
}