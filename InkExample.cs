using UnityEngine;
using Ink.Runtime;
using UnityEngine.UI;
using System;
using System.Collections;

public class InkExample : MonoBehaviour
{
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private GameObject imagePrefab;
    [SerializeField] private GameObject player;

    private Story story;
    public event Action EndHistory;

    private void Start()
    {
        this.enabled = false;
    }

    public void StartDialog(TextAsset inkJSONAsset)
    {
        story = new Story(inkJSONAsset.text);
        player.GetComponent<PlayerInputETriger>().InputE += InputHistory;
        Refresh();
    }//диалог запускается вызовом этого метода

    private void Refresh()
    {
        StopCoroutine("PechatText");
        ClearUI();

        Instance();
    }//Rehresh это основной метод диалогов создающий элементы интерфейса

    private void Instance()
    {
        NewImage();
    }//создание элементов диалогового окна

    private void NewButton()
    {
        if (story.currentChoices.Count > 0)
        {
            foreach (Choice choice in story.currentChoices)
            {
                Button choiceButton = Instantiate(buttonPrefab, transform);//проверяем можем ли мы получить у прифаба компонент буттон и получаем его
                RectTransform buttonRect = choiceButton.GetComponent<RectTransform>();
                buttonRect.anchorMin = new Vector2(0.2f, 0.1f + 0.15f * choice.index);
                buttonRect.anchorMax = new Vector2(0.8f, 0.25f + 0.15f * choice.index);

                Text choiceText = choiceButton.GetComponentInChildren<Text>();
                choiceText.text = choice.text;//помещаем текст варианта выбора в текст кнопки для выбора

                choiceButton.onClick.AddListener(delegate
                {
                    OnClickChoiceButton(choice);
                });//делегат хранит ссылку например на метод

            }//currentChoices заполняется вариантами выбора и каждый вариант содержит индекс и текст
        }
        else
            inputE = true;
    }
    bool inputE = false;
    private void InputHistory()
    {
        if (inputE)
        {
            Refresh();
        }
    }
    private void NewImage()
    {
        GameObject image = Instantiate(imagePrefab, transform);
        RectTransform imageRect = image.GetComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0.1f, 0.5f);
        imageRect.anchorMax = new Vector2(0.9f, 0.8f);//создаем изображение и задаем положение относительно родителя


        Text text = image.transform.GetComponentInChildren<Text>();
        text.fontSize = 60;
        RectTransform imageRects = image.GetComponent<RectTransform>();
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);

        GetNextStoryBlock(text, imageRects);//создаем текст диалога
    }

    private void ClearUI()
    {
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; --i)
        {
            GameObject.Destroy(this.transform.GetChild(i).gameObject);
        }
    }//перед обновлением UI он должен быть очищен

    public void OnClickChoiceButton(Choice choice)
    {
        story.ChooseChoiceIndex(choice.index);//выбор варианта совершается передачей сюда индекса и тогда story заполняется следущей историей
        Refresh();
    }

    private void GetNextStoryBlock(Text newTextObject, RectTransform imageRect)
    {
        if (story.canContinue)
        {
            string text;
            text = story.Continue();
            StartCoroutine(PechatText(text, newTextObject, imageRect));
        }
        else
        {
            player.GetComponent<PlayerInputETriger>().InputE -= InputHistory;
            ClearUI();
            EndHistory?.Invoke();
            this.enabled = false;
        }
    }//метод при помощи canContinue проверяет есть ли следующая история
    //а Continue загружает следующую историю
    private IEnumerator PechatText(string text, Text obText, RectTransform rect)
    {
        inputE = false;
        foreach(var a in text.ToCharArray())
        {
            obText.text += a;
            float textHeight = LayoutUtility.GetPreferredHeight(obText.rectTransform);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, textHeight);

            yield return new WaitForSeconds(0.05f);
        }
        NewButton();
    }
}
