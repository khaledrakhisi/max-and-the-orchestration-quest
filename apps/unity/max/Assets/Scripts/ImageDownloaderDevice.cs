using System.Collections;
using TMPro;
using UnityEngine;

public class ImageDownloaderDevice : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI screenText;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private ObjectSpawner spawner;
    [SerializeField] private DirectorWorker orbitBall;

    public string[] AvailableImages;
    public string selectedImage;
    private bool isBusy = false;
    private int selectedIndex = 0;
    private float timeElapsed = 0.0f;
    private bool isTriggered = false;

    void Start()
    {
        //         influxdb                                          latest      771dee7da05d   6 weeks ago    380MB
        // akshaychikhalkar/carla-driving-simulator-client   1.3.29      4df6b9308ee2   5 months ago   1.89GB
        // akshaychikhalkar/carla-driving-simulator-client   latest      4df6b9308ee2   5 months ago   1.89GB
        // traefik                                           latest      91f33a9ff29b   5 months ago   226MB
        // certbot/certbot                                   latest      95d0b35bc337   6 months ago   114MB
        // postgres                                          17-alpine   f40315d0e8a6   6 months ago   279MB
        // alpine                                            latest      cea2ff433c61   7 months ago   8.31MB
        // traefik/whoami                                    latest      6fee7566e427   9 months ago   7.16MB
        // grafana/grafana                                   10.2.3      8387f19108f9   2 years ago    399MB
        AvailableImages = new string[] {
                "influxdb",
                "akshaychikhalkar/carla-driving-simulator-client",
                "akshaychikhalkar/carla-driving-simulator-client",
                "traefik",
                "certbot/certbot",
                "postgres",
                "alpine",
                "traefik/whoami",
                "grafana/grafana",
        };
        // Example usage
        if (AvailableImages.Length > 0)
            selectedImage = AvailableImages[0];
        else
            selectedImage = "NO IMAGE AVAILABLE";

        UpdateDisplay(selectedImage);
    }

    void Update()
    {
        if (isTriggered)
            timeElapsed += Time.deltaTime;

        if (timeElapsed > 3f)
        {
            timeElapsed = 0f;
            isTriggered = false;

            if (spawner)
            {
                spawner.DoSpawn();
                isBusy = true;
            }
            if (orbitBall)
            {
                orbitBall.DoRunAnimation("0");
            }
            UpdateDisplay("Recovering ...");
        }

    }

    public void UpdateDisplay(string newText)
    {
        screenText.text = newText;
    }

    // Optional: Retro "boot-up" typing effect
    public void TypeMessage(string message)
    {
        StopAllCoroutines();
        StartCoroutine(TypeScreeRoutine(message));
    }

    private IEnumerator TypeScreeRoutine(string message)
    {
        screenText.text = "";
        foreach (char c in message.ToCharArray())
        {
            screenText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void DoSelectNextImage()
    {
        selectedIndex++;
        if (selectedIndex >= AvailableImages.Length) selectedIndex = 0;

        selectedImage = AvailableImages[selectedIndex];
        UpdateDisplay(selectedImage);
    }

    public void DoDownloadImage()
    {
        if (isBusy)
        {
            UpdateDisplay("* System busy! *");
            return;
        }

        UpdateDisplay("Downloading ...");
        isTriggered = true;
        if (orbitBall)
        {
            orbitBall.DoRunAnimation("1");
        }
    }

    public void DoReset()
    {
        isBusy = false;
        UpdateDisplay(selectedImage);
    }
}
