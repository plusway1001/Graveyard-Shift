using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the player's lives as hearts instead of text.
/// Put this on an empty UI object (inside a Canvas) that will hold the hearts.
/// It reads PlayerHealth directly, so PlayerHealth needs no changes.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class HeartsUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Found automatically from the object tagged Player if left empty.")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Heart Sprites")]
    [SerializeField] private Sprite fullHeart;
    [Tooltip("Optional. If empty, the full heart is shown darkened instead.")]
    [SerializeField] private Sprite emptyHeart;
    [SerializeField] private Color emptyTint = new Color(0.2f, 0.2f, 0.2f, 0.7f);

    [Header("Layout")]
    [SerializeField] private Vector2 heartSize = new Vector2(48f, 48f);
    [SerializeField, Min(0f)] private float spacing = 6f;

    private readonly List<Image> hearts = new List<Image>();
    private int lastLives = -1;
    private int lastMax = -1;

    private void Awake()
    {
        // Lay the hearts out in a row.
        HorizontalLayoutGroup layout = GetComponent<HorizontalLayoutGroup>();

        if (layout == null)
            layout = gameObject.AddComponent<HorizontalLayoutGroup>();

        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    private void Update()
    {
        if (playerHealth == null && !FindPlayerHealth())
            return;

        int lives = playerHealth.CurrentLives;
        int max = playerHealth.MaxLives;

        // Only redraw when something actually changed.
        if (lives == lastLives && max == lastMax)
            return;

        lastLives = lives;
        lastMax = max;

        Redraw(lives, max);
    }

    private bool FindPlayerHealth()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            playerHealth = player.GetComponentInParent<PlayerHealth>();

        return playerHealth != null;
    }

    private void Redraw(int lives, int max)
    {
        // Create more hearts if the max grew.
        while (hearts.Count < max)
            hearts.Add(CreateHeart(hearts.Count));

        for (int i = 0; i < hearts.Count; i++)
        {
            Image heart = hearts[i];

            // Hearts beyond the max are hidden.
            heart.gameObject.SetActive(i < max);

            bool full = i < lives;

            if (full)
            {
                heart.sprite = fullHeart;
                heart.color = Color.white;
            }
            else if (emptyHeart != null)
            {
                heart.sprite = emptyHeart;
                heart.color = Color.white;
            }
            else
            {
                heart.sprite = fullHeart;
                heart.color = emptyTint;
            }
        }
    }

    private Image CreateHeart(int index)
    {
        GameObject go = new GameObject("Heart_" + (index + 1), typeof(RectTransform), typeof(Image));
        go.transform.SetParent(transform, false);

        RectTransform rect = (RectTransform)go.transform;
        rect.sizeDelta = heartSize;

        Image image = go.GetComponent<Image>();
        image.preserveAspect = true;
        image.raycastTarget = false;

        return image;
    }
}
