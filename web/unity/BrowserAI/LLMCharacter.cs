using System;
using System.Threading.Tasks;
using UnityEngine;

namespace LLMUnity
{
    // The original scene's script GUID and persona are retained. Native llama.cpp
    // is replaced only in the isolated browser project.
    public class LLMCharacter : MonoBehaviour
    {
        [TextArea] public string prompt;

        public async Task<string> Chat(string query, Action<string> callback = null,
            Action completionCallback = null, bool addToHistory = true)
        {
            string result = await WebGameBridge.Instance.Analyze(prompt, query);
            callback?.Invoke(result);
            completionCallback?.Invoke();
            return result;
        }
    }
}
