using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIAutoSetup : MonoBehaviour
{
    [Header("Référence joueur")]
    public FPSController playerController;

    [Header("Options d'inventaire")]
    public int inventorySlotCount = 20;
    public int inventoryColumns = 5;
    public float slotSize = 70f;
    public float slotSpacing = 12f;

    [Header("Couleurs")]
    public Color panelColor = new Color(0.1f, 0.1f, 0.12f, 0.92f);
    public Color buttonColor = new Color(0.25f, 0.25f, 0.3f, 1f);
    public Color buttonHoverColor = new Color(0.4f, 0.4f, 0.45f, 1f);
    public Color titleColor = new Color(0.9f, 0.9f, 1f, 1f);
    public Color accentColor = new Color(0.4f, 0.7f, 1f, 1f); // Bleu Poppy Playtime

    Font defaultFont;

    void Start()
    {
        if (FindFirstObjectByType<UIManager>() != null)
        {
            Debug.Log("[UIAutoSetup] Un UIManager existe déjà dans la scène. Auto-configuration annulée.");
            Destroy(this);
            return;
        }

        // Charger la police (compatibilité versions Unity)
        defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null)
            defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        SetupAll();
        Debug.Log("[UIAutoSetup] Interface configurée automatiquement !");
    }

    void SetupAll()
    {
        Canvas canvas = EnsureCanvas();
        EnsureEventSystem();

        // GameObject UIManager
        GameObject uiManagerGo = new GameObject("UIManager");
        uiManagerGo.transform.SetParent(canvas.transform, false);
        UIManager uiManager = uiManagerGo.AddComponent<UIManager>();
        OptionsMenu optionsMenu = uiManagerGo.AddComponent<OptionsMenu>();
        InventoryManager inventoryManager = uiManagerGo.AddComponent<InventoryManager>();

        // Panneaux avec fond sombre
        GameObject mainMenuPanel = CreatePanel(canvas.transform, "MainMenuPanel", new Vector2(0, 0), new Vector2(1, 1));
        GameObject pausePanel = CreatePanel(canvas.transform, "PausePanel", new Vector2(0, 0), new Vector2(1, 1));
        GameObject optionsPanel = CreatePanel(canvas.transform, "OptionsPanel", new Vector2(0, 0), new Vector2(1, 1));
        GameObject inventoryPanel = CreatePanel(canvas.transform, "InventoryPanel", new Vector2(0, 0), new Vector2(1, 1));

        mainMenuPanel.SetActive(true);
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        inventoryPanel.SetActive(false);

        // Relier à l'UIManager
        uiManager.mainMenuPanel = mainMenuPanel;
        uiManager.pausePanel = pausePanel;
        uiManager.optionsPanel = optionsPanel;
        uiManager.inventoryPanel = inventoryPanel;
        uiManager.optionsMenu = optionsMenu;
        uiManager.inventoryManager = inventoryManager;
        optionsMenu.playerController = playerController;

        // ============ MAIN MENU ============
        CreateText(mainMenuPanel.transform, "POPPY PLAYTIME", new Vector2(0.5f, 0.55f), Vector2.zero, titleColor, 48, TextAnchor.MiddleCenter);
        CreateText(mainMenuPanel.transform, "FANGAME", new Vector2(0.5f, 0.45f), Vector2.zero, accentColor, 32, TextAnchor.MiddleCenter);

        float mainBtnY = 0.0f;
        CreateStyledButton(mainMenuPanel.transform, "BtnNewGame", "Nouvelle partie", new Vector2(0.5f, mainBtnY), new Vector2(300, 55),
            () => { uiManager.ResumeGame(); });

        CreateStyledButton(mainMenuPanel.transform, "BtnOptions", "Options", new Vector2(0.5f, mainBtnY - 0.1f), new Vector2(300, 55),
            () => { uiManager.OpenOptions(); });

        CreateStyledButton(mainMenuPanel.transform, "BtnQuit", "Quitter", new Vector2(0.5f, mainBtnY - 0.2f), new Vector2(300, 55),
            () => { uiManager.QuitGame(); });

        // ============ PAUSE MENU ============
        CreateText(pausePanel.transform, "PAUSE", new Vector2(0.5f, 0.55f), Vector2.zero, titleColor, 48, TextAnchor.MiddleCenter);

        float pauseBtnY = 0.0f;
        CreateStyledButton(pausePanel.transform, "BtnResume", "Reprendre", new Vector2(0.5f, pauseBtnY), new Vector2(300, 55),
            () => { uiManager.ResumeGame(); });

        CreateStyledButton(pausePanel.transform, "BtnPauseOptions", "Options", new Vector2(0.5f, pauseBtnY - 0.1f), new Vector2(300, 55),
            () => { uiManager.OpenOptions(); });

        CreateStyledButton(pausePanel.transform, "BtnPauseInventory", "Inventaire", new Vector2(0.5f, pauseBtnY - 0.2f), new Vector2(300, 55),
            () => { uiManager.OpenInventory(); });

        CreateStyledButton(pausePanel.transform, "BtnPauseQuit", "Quitter", new Vector2(0.5f, pauseBtnY - 0.3f), new Vector2(300, 55),
            () => { uiManager.QuitGame(); });

        // ============ OPTIONS ============
        CreateText(optionsPanel.transform, "OPTIONS", new Vector2(0.5f, 0.8f), Vector2.zero, titleColor, 36, TextAnchor.MiddleCenter);

        float optY = 0.55f;
        CreateText(optionsPanel.transform, "Sensibilité souris", new Vector2(0.5f, optY), new Vector2(-220, 25), Color.white, 18, TextAnchor.MiddleLeft);
        Slider sensSlider = CreateSlider(optionsPanel.transform, new Vector2(0.5f, optY), new Vector2(250, 20), 0f, 30f, playerController != null ? playerController.mouseSensitivity : 15f);
        optionsMenu.sensitivitySlider = sensSlider;

        Text sensValueText = CreateText(optionsPanel.transform, sensSlider.value.ToString("F0"), new Vector2(0.78f, optY), new Vector2(40, 25), accentColor, 18, TextAnchor.MiddleLeft);
        optionsMenu.sensitivityValueText = sensValueText;
        sensSlider.onValueChanged.AddListener((val) => sensValueText.text = val.ToString("F0"));

        optY -= 0.12f;
        CreateText(optionsPanel.transform, "Plein écran", new Vector2(0.5f, optY), new Vector2(-220, 25), Color.white, 18, TextAnchor.MiddleLeft);
        Toggle fullscreenToggle = CreateToggle(optionsPanel.transform, new Vector2(0.75f, optY), new Vector2(30, 30), Screen.fullScreen);
        optionsMenu.fullscreenToggle = fullscreenToggle;

        optY -= 0.12f;
        CreateText(optionsPanel.transform, "Qualité graphique", new Vector2(0.5f, optY), new Vector2(-220, 25), Color.white, 18, TextAnchor.MiddleLeft);
        Dropdown qualityDropdown = CreateDropdown(optionsPanel.transform, new Vector2(0.75f, optY), new Vector2(150, 30), QualitySettings.names, QualitySettings.GetQualityLevel());
        optionsMenu.qualityDropdown = qualityDropdown;

        optY -= 0.12f;
        CreateText(optionsPanel.transform, "Volume musique", new Vector2(0.5f, optY), new Vector2(-220, 25), Color.white, 18, TextAnchor.MiddleLeft);
        Slider musicSlider = CreateSlider(optionsPanel.transform, new Vector2(0.5f, optY), new Vector2(250, 20), 0f, 1f, 0.7f);
        optionsMenu.musicVolumeSlider = musicSlider;

        optY -= 0.12f;
        CreateText(optionsPanel.transform, "Volume SFX", new Vector2(0.5f, optY), new Vector2(-220, 25), Color.white, 18, TextAnchor.MiddleLeft);
        Slider sfxSlider = CreateSlider(optionsPanel.transform, new Vector2(0.5f, optY), new Vector2(250, 20), 0f, 1f, 0.8f);
        optionsMenu.sfxVolumeSlider = sfxSlider;

        optY -= 0.16f;
        CreateStyledButton(optionsPanel.transform, "BtnApply", "Appliquer", new Vector2(0.4f, optY), new Vector2(140, 45),
            () => optionsMenu.ApplyAndSave(), accentColor);

        CreateStyledButton(optionsPanel.transform, "BtnOptionsBack", "Retour", new Vector2(0.6f, optY), new Vector2(140, 45),
            () => uiManager.CloseOptions());

        // ============ INVENTORY ============
        CreateText(inventoryPanel.transform, "INVENTAIRE", new Vector2(0.5f, 0.85f), Vector2.zero, titleColor, 34, TextAnchor.MiddleCenter);

        // Texte quand inventaire vide
        GameObject emptyTextGo = new GameObject("EmptyText");
        emptyTextGo.transform.SetParent(inventoryPanel.transform, false);
        RectTransform emptyRect = emptyTextGo.AddComponent<RectTransform>();
        emptyRect.anchorMin = new Vector2(0.5f, 0.5f);
        emptyRect.anchorMax = new Vector2(0.5f, 0.5f);
        emptyRect.pivot = new Vector2(0.5f, 0.5f);
        emptyRect.sizeDelta = new Vector2(300, 40);
        Text emptyText = emptyTextGo.AddComponent<Text>();
        emptyText.text = "Inventaire vide";
        emptyText.font = defaultFont;
        emptyText.fontSize = 18;
        emptyText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        emptyText.alignment = TextAnchor.MiddleCenter;
        emptyText.raycastTarget = false;
        inventoryManager.GetType().GetField("emptyText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(inventoryManager, emptyText);
        emptyTextGo.SetActive(true);

        // Grid de slots
        GameObject slotsContainer = new GameObject("SlotsContainer");
        slotsContainer.transform.SetParent(inventoryPanel.transform, false);
        RectTransform containerRect = slotsContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        int rows = Mathf.CeilToInt((float)inventorySlotCount / inventoryColumns);
        containerRect.sizeDelta = new Vector2(inventoryColumns * (slotSize + slotSpacing) + slotSpacing, rows * (slotSize + slotSpacing) + slotSpacing);

        GridLayoutGroup grid = slotsContainer.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(slotSize, slotSize);
        grid.spacing = new Vector2(slotSpacing, slotSpacing);
        grid.childAlignment = TextAnchor.MiddleCenter;

        InventorySlot[] slots = new InventorySlot[inventorySlotCount];
        for (int i = 0; i < inventorySlotCount; i++)
        {
            GameObject slotGo = new GameObject($"Slot_{i}");
            slotGo.transform.SetParent(slotsContainer.transform, false);
            RectTransform slotRect = slotGo.AddComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(slotSize, slotSize);

            Image slotBg = slotGo.AddComponent<Image>();
            slotBg.color = new Color(0.25f, 0.25f, 0.3f, 0.9f);
            slotBg.raycastTarget = true;

            // Bordure accent
            GameObject borderGo = new GameObject("Border");
            borderGo.transform.SetParent(slotGo.transform, false);
            RectTransform borderRect = borderGo.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;
            Image borderImg = borderGo.AddComponent<Image>();
            borderImg.color = accentColor;
            borderImg.enabled = false;

            GameObject iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(slotGo.transform, false);
            RectTransform iconRect = iconGo.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.15f, 0.15f);
            iconRect.anchorMax = new Vector2(0.85f, 0.85f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            Image iconImg = iconGo.AddComponent<Image>();
            iconImg.enabled = false;

            GameObject qtyGo = new GameObject("Quantity");
            qtyGo.transform.SetParent(slotGo.transform, false);
            RectTransform qtyRect = qtyGo.AddComponent<RectTransform>();
            qtyRect.anchorMin = new Vector2(0.5f, 0.1f);
            qtyRect.anchorMax = new Vector2(1f, 0.35f);
            qtyRect.offsetMin = Vector2.zero;
            qtyRect.offsetMax = Vector2.zero;
            Text qtyText = qtyGo.AddComponent<Text>();
            qtyText.text = "";
            qtyText.font = defaultFont;
            qtyText.fontSize = 18;
            qtyText.color = Color.white;
            qtyText.alignment = TextAnchor.MiddleRight;
            qtyText.raycastTarget = false;

            InventorySlot slot = slotGo.AddComponent<InventorySlot>();
            slot.iconImage = iconImg;
            slot.quantityText = qtyText;
            slot.borderImage = borderImg;

            slots[i] = slot;
        }

        inventoryManager.slotCount = inventorySlotCount;
        inventoryManager.slots = slots;

        CreateStyledButton(inventoryPanel.transform, "BtnInventoryClose", "Fermer l'inventaire", new Vector2(0.5f, 0.1f), new Vector2(250, 50),
            () => uiManager.CloseInventory());

        Debug.Log("[UIAutoSetup] Configuration terminée avec succès !");
    }

    #region Helpers UI

    Canvas EnsureCanvas()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null) return canvas;

        GameObject go = new GameObject("Canvas");
        canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }

    GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = go.AddComponent<Image>();
        img.color = panelColor;
        img.raycastTarget = true;

        return go;
    }

    Text CreateText(Transform parent, string text, Vector2 anchor, Vector2 offset, Color color, int fontSize, TextAnchor align)
    {
        GameObject go = new GameObject("Text_" + text);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = offset;
        rect.sizeDelta = new Vector2(500, 50);

        Text txt = go.AddComponent<Text>();
        txt.text = text;
        txt.font = defaultFont;
        txt.fontSize = fontSize;
        txt.color = color;
        txt.alignment = align;
        txt.raycastTarget = false;

        return txt;
    }

    Button CreateStyledButton(Transform parent, string name, string text, Vector2 anchor, Vector2 size, UnityEngine.Events.UnityAction onClick, Color? customColor = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;

        Color btnColor = customColor ?? buttonColor;
        Image img = go.AddComponent<Image>();
        img.color = btnColor;
        img.raycastTarget = true;

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.ColorTint;
        btn.colors = new ColorBlock
        {
            normalColor = btnColor,
            highlightedColor = buttonHoverColor,
            pressedColor = new Color(0.5f, 0.5f, 0.55f, 1f),
            disabledColor = new Color(0.2f, 0.2f, 0.2f, 1f),
            colorMultiplier = 1f,
            fadeDuration = 0.1f
        };
        if (onClick != null) btn.onClick.AddListener(onClick);

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        RectTransform textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text btnText = textGo.AddComponent<Text>();
        btnText.text = text;
        btnText.font = defaultFont;
        btnText.fontSize = 18;
        btnText.color = Color.white;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.raycastTarget = false;

        return btn;
    }

    Slider CreateSlider(Transform parent, Vector2 anchor, Vector2 size, float minValue, float maxValue, float value)
    {
        GameObject go = new GameObject("Slider");
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;

        Slider slider = go.AddComponent<Slider>();
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = value;
        slider.wholeNumbers = false;
        slider.navigation = new Navigation { mode = Navigation.Mode.None };

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(go.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.18f, 1f);
        bgImg.raycastTarget = true;

        GameObject fill = new GameObject("Fill Area");
        fill.transform.SetParent(go.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0.25f);
        fillRect.anchorMax = new Vector2(1, 0.75f);
        fillRect.offsetMin = new Vector2(4, 4);
        fillRect.offsetMax = new Vector2(-4, -4);

        GameObject fillImgGo = new GameObject("Fill");
        fillImgGo.transform.SetParent(fill.transform, false);
        RectTransform fillImgRect = fillImgGo.AddComponent<RectTransform>();
        fillImgRect.anchorMin = Vector2.zero;
        fillImgRect.anchorMax = new Vector2(1, 1);
        fillImgRect.offsetMin = Vector2.zero;
        fillImgRect.offsetMax = Vector2.zero;
        Image fillImg = fillImgGo.AddComponent<Image>();
        fillImg.color = accentColor;

        slider.fillRect = fillImgRect;
        slider.targetGraphic = bgImg;

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(fill.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 20);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;
        slider.handleRect = handleRect;

        return slider;
    }

    Toggle CreateToggle(Transform parent, Vector2 anchor, Vector2 size, bool isOn)
    {
        GameObject go = new GameObject("Toggle");
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;

        Toggle toggle = go.AddComponent<Toggle>();
        toggle.isOn = isOn;
        toggle.navigation = new Navigation { mode = Navigation.Mode.None };

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(go.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.18f, 1f);

        GameObject check = new GameObject("Checkmark");
        check.transform.SetParent(bg.transform, false);
        RectTransform checkRect = check.AddComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.15f, 0.15f);
        checkRect.anchorMax = new Vector2(0.85f, 0.85f);
        Image checkImg = check.AddComponent<Image>();
        checkImg.color = accentColor;
        checkImg.enabled = isOn;

        toggle.targetGraphic = bgImg;
        toggle.graphic = checkImg;

        return toggle;
    }

    Dropdown CreateDropdown(Transform parent, Vector2 anchor, Vector2 size, string[] options, int value)
    {
        GameObject go = new GameObject("Dropdown");
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;

        Dropdown dropdown = go.AddComponent<Dropdown>();
        dropdown.options = new List<Dropdown.OptionData>();
        foreach (var opt in options)
            dropdown.options.Add(new Dropdown.OptionData(opt));
        dropdown.value = value;
        dropdown.navigation = new Navigation { mode = Navigation.Mode.None };

        GameObject template = new GameObject("Template");
        template.transform.SetParent(go.transform, false);
        RectTransform templateRect = template.AddComponent<RectTransform>();
        templateRect.anchorMin = Vector2.zero;
        templateRect.anchorMax = Vector2.one;
        templateRect.offsetMin = Vector2.zero;
        templateRect.offsetMax = Vector2.zero;

        Image templateImg = template.AddComponent<Image>();
        templateImg.color = new Color(0.15f, 0.15f, 0.18f, 1f);

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(template.transform, false);
        RectTransform vpRect = viewport.AddComponent<RectTransform>();
        vpRect.anchorMin = new Vector2(0, 0);
        vpRect.anchorMax = new Vector2(1, 1);
        vpRect.offsetMin = Vector2.zero;
        vpRect.offsetMax = Vector2.zero;

        Mask vpMask = viewport.AddComponent<Mask>();
        vpMask.showMaskGraphic = false;

        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 30);
        contentRect.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject itemBg = new GameObject("Item Background");
        itemBg.transform.SetParent(content.transform, false);
        RectTransform itemRect = itemBg.AddComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 1);
        itemRect.anchorMax = new Vector2(1, 1);
        itemRect.sizeDelta = new Vector2(0, 35);
        Image itemImg = itemBg.AddComponent<Image>();
        itemImg.color = new Color(0.25f, 0.25f, 0.3f, 1f);

        GameObject itemText = new GameObject("Item Label");
        itemText.transform.SetParent(itemBg.transform, false);
        RectTransform itemTextRect = itemText.AddComponent<RectTransform>();
        itemTextRect.anchorMin = Vector2.zero;
        itemTextRect.anchorMax = Vector2.one;
        itemTextRect.offsetMin = new Vector2(10, 0);
        itemTextRect.offsetMax = new Vector2(-10, 0);
        Text itemTextComp = itemText.AddComponent<Text>();
        itemTextComp.font = defaultFont;
        itemTextComp.fontSize = 16;
        itemTextComp.color = Color.white;
        itemTextComp.alignment = TextAnchor.MiddleLeft;

        dropdown.template = template.GetComponent<RectTransform>();
        dropdown.captionText = itemTextComp;
        dropdown.captionImage = itemImg;
        dropdown.itemText = itemTextComp;

        return dropdown;
    }

    #endregion
}