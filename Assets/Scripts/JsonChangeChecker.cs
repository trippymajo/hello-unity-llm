using System.IO;
using LLMUnity;
using UnityEngine;

/// <summary>
/// Класс для обновления промптов (хотел сделать чтобы он сам проверял обновление файла но тупо по итогу поставил кнопку)
/// </summary>
public class JsonChangeChecker : MonoBehaviour
{
    public LLMCharacter stupidCharacter; // тупой персонаж
    public LLMCharacter geniusCharacter; // препод
    private PromptsJson promptsJson; // класс чтобы хранить наш запарсенный джейсон

    
    private void Start()
    {
        ChangePromptsFromJson();
    }

    /// <summary>
    /// Метод читает джейсон как текст из него делает экземпляр класса промптов и обновляет их у персонажей
    /// </summary>
    public void ChangePromptsFromJson()
    {
        string jsonText = File.ReadAllText(Application.streamingAssetsPath + "/models-prompts.json");
        //TextAsset jsonFile = Resources.Load<TextAsset>(Application.streamingAssetsPath + "/models-prompts.json");
        promptsJson = JsonUtility.FromJson<PromptsJson>(jsonText);
        
        stupidCharacter.SetPrompt(promptsJson.stupid);
        geniusCharacter.SetPrompt(promptsJson.genius);
        
        Debug.Log(jsonText);
    }
}

/// <summary>
/// Класс для хранение промптов
/// </summary>
public class PromptsJson
{
    public string stupid;
    public string genius;
}
