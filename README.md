Download undreamai-v1.2.6-llamacpp.zip  
From undreamai/LlamaLib -> Releases  
put in Assets/ as undreamai-v1.2.6-llamacpp  
  

How to use own LLMs:  
1. Go to projectname_Data/StreamingAssets
2. Put disred .gguf LLM near .json
3. Edit model-config.json
4. Start the game
  
```json
{
  "stupid": "qwen2.5-0.5b-instruct-q2_k.gguf",
  "genious": "Meta-Llama-3.1-8B-Instruct-Q3_K_L.gguf",
  "classStudent": "qwen2.5-3b-instruct-q2_k.gguf",
  "classTeacher": "Meta-Llama-3.1-8B-Instruct-Q3_K_L.gguf",
  "goals": []
}
```

How to change LLMs prompts:  
1. Go to projectname_Data/StreamingAssets
2. Edit models-prompts.json

```json
{
  "stupid": "Answer only with YES",
  "genius": "Answer only with YES"
}
```
3. Go to classroom Scene and click update pormpts
4. Enjoy
