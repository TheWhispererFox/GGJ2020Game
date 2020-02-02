using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Transform PlayerTransform => player.transform;
    public GameObject PlayerObject => player;

    [Space]
    [Header("Settings")]
    [SerializeField] private bool useRandomJackalsCount;
    [SerializeField] private float worldPixelation;
    [SerializeField] private int totalJackalsCount;
    [SerializeField] private int maxJackalsOnMap;
    [SerializeField] private float pixelateStep;

    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private Spawner spawner;
    [SerializeField] private Material pixelateMaterial;

    private void Awake()
    {
        Instance = this;
        pixelateMaterial.SetFloat("_PixelAmount", worldPixelation);
        if (useRandomJackalsCount)
        {
            totalJackalsCount = Random.Range(8, 13);
            maxJackalsOnMap = Random.Range(3, 6);
        }
    }

    private void Start()
    {
        totalJackalsCount = Mathf.Max(totalJackalsCount, maxJackalsOnMap);

        spawner.Spawn(maxJackalsOnMap);
        totalJackalsCount -= maxJackalsOnMap;
    }

    public void RestorePixelation()
    {
        if (totalJackalsCount <= 0)
        {
            totalJackalsCount--;
            if (totalJackalsCount <= -maxJackalsOnMap)
            {
                spawner.DeleteAllSpawned();
                StartCoroutine(EndGame(10));
            }
            return;
        }
        
        spawner.Spawn(1);
        totalJackalsCount--;
        worldPixelation += pixelateStep;
        pixelateStep *= 0.9f;
        pixelateMaterial.SetFloat("_PixelAmount", worldPixelation);
    }

    private IEnumerator EndGame(float waitSeconds)
    {
        yield return new WaitForSeconds(waitSeconds);

        float step = worldPixelation * 0.01f;
        for (int i = 0; i < 100; i++)
        {
            worldPixelation -= step;
            pixelateMaterial.SetFloat("_PixelAmount", worldPixelation);
            yield return new WaitForSeconds(0.05f);
        }

        SceneManager.LoadScene(0);
    }
}
