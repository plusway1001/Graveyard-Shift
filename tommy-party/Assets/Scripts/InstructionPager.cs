using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Flip through instruction pages inside ONE scene.
/// Each page is a GameObject (panel). Only the current page is shown.
/// The number of pages is simply how many panels you add to the list.
/// </summary>
public class InstructionPager : MonoBehaviour
{
    [Header("Pages (in order)")]
    [SerializeField] private GameObject[] pages;

    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button nextButton;

    [Header("Page Counter (optional)")]
    [SerializeField] private TMP_Text pageCounterText;

    [Header("Behaviour")]
    [Tooltip("Ticked: Back/Next are hidden on the first/last page. Unticked: they stay visible but greyed out.")]
    [SerializeField] private bool hideButtonsAtEnds = false;

    [Tooltip("Ticked: Next on the last page goes back to the first page (and Back on the first goes to the last).")]
    [SerializeField] private bool loopPages = false;

    [Tooltip("Ticked: always start on page 1 when this menu is opened.")]
    [SerializeField] private bool resetToFirstPageOnEnable = true;

    [Header("Keyboard (optional)")]
    [SerializeField] private bool useKeyboard = true;
    [SerializeField] private KeyCode nextKey = KeyCode.RightArrow;
    [SerializeField] private KeyCode backKey = KeyCode.LeftArrow;

    private int currentPage;

    public int PageCount => pages != null ? pages.Length : 0;
    public int CurrentPage => currentPage;

    private void OnEnable()
    {
        if (backButton != null) backButton.onClick.AddListener(Previous);
        if (nextButton != null) nextButton.onClick.AddListener(Next);

        if (resetToFirstPageOnEnable)
            currentPage = 0;

        ShowPage(currentPage);
    }

    private void OnDisable()
    {
        if (backButton != null) backButton.onClick.RemoveListener(Previous);
        if (nextButton != null) nextButton.onClick.RemoveListener(Next);
    }

    private void Update()
    {
        if (!useKeyboard)
            return;

        if (Input.GetKeyDown(nextKey)) Next();
        if (Input.GetKeyDown(backKey)) Previous();
    }

    // Hook these to buttons if you prefer wiring them in the Inspector.
    public void Next()
    {
        if (PageCount == 0)
            return;

        if (currentPage < PageCount - 1)
            ShowPage(currentPage + 1);
        else if (loopPages)
            ShowPage(0);
    }

    public void Previous()
    {
        if (PageCount == 0)
            return;

        if (currentPage > 0)
            ShowPage(currentPage - 1);
        else if (loopPages)
            ShowPage(PageCount - 1);
    }

    public void GoToPage(int index)
    {
        if (PageCount == 0)
            return;

        ShowPage(Mathf.Clamp(index, 0, PageCount - 1));
    }

    private void ShowPage(int index)
    {
        if (PageCount == 0)
            return;

        currentPage = Mathf.Clamp(index, 0, PageCount - 1);

        // Only the current page is active.
        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
                pages[i].SetActive(i == currentPage);
        }

        if (pageCounterText != null)
            pageCounterText.text = (currentPage + 1) + " / " + PageCount;

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        bool canGoBack = loopPages || currentPage > 0;
        bool canGoNext = loopPages || currentPage < PageCount - 1;

        SetButton(backButton, canGoBack);
        SetButton(nextButton, canGoNext);
    }

    private void SetButton(Button button, bool usable)
    {
        if (button == null)
            return;

        if (hideButtonsAtEnds)
            button.gameObject.SetActive(usable);
        else
            button.interactable = usable;
    }
}
